# ClashOfGods 架构审查报告

生成日期：2026-08-29

---

## 🔴 严重问题

### 1. 双事件系统并存（Dead Code）
- `COG.Infrastructure.EventBus` — 泛型订阅/发布，线程安全，但**从未被任何 Patch 使用**
- `COG.Listener.ListenerManager` — 反射 + `[EventHandler]` 属性，实际在用的系统
- `COG.Infrastructure.EventHandlerAttribute` 与 `COG.Listener.EventHandlerAttribute` 同名但语义不同，容易混淆

### 2. 10+ 手写单例，ServiceContainer 形同虚设
- `CustomRoleManager.GetManager()`, `ListenerManager.GetManager()`, `CommandManager.GetManager()`, `ClientOptionManager.GetManager()`, `CustomButtonManager.GetManager()`, `CustomWinnerManager.GetManager()` 等全部绕过 `ServiceContainer`
- `ServiceContainer` 只注册了 4 个服务（Logger, EventBus, ConfigManager, Main），其余代码直接调用静态单例

### 3. `CustomRole` 上帝类（744 行）
- 身份/能力/按钮/选项/RPC/事件/生命周期全在一个类
- 组件拆分（`RoleMetadata`, `RoleCapabilities`, `RoleButtons`, `RoleOptions`）只是薄委托，`CustomRole` 仍然暴露一切

### 4. 三套 RPC 系统共存
- `IRpcHandler` / `RpcHandler` (旧静态 HashSet，无清理)
- `KnownRpc` + `RpcListener` 字典分发（主力）
- `RoleRpc` / `RoleRpcManager`（per-role，静态字典只增不减）

### 5. 双配置系统
- `ConfigBase`（旧 YAML）— 自注册到静态 List，`AutoReplace` 全局可变
- `ConfigManager`（新 YAML）— 支持版本迁移，但与旧系统并存

---

## 🟡 中等问题

### 6. 反射滥用，热路径性能差
- `ListenerManager.AsHandlers()` 反射发现 `[EventHandler]` 方法
- `ExecuteHandlers()` 每次调用都 `Method.Invoke`
- `RaiseEvent()` 每次检查 `[LocalOnly]` 属性 via 反射
- `EventBus.RegisterHandlers()` 也用反射

### 7. `GameStates.InRealGame` setter 有副作用
- 设置 `true` 时：`ClearRoleGameData()` + 创建 `EventRecorder`
- 设置 `false` 时：清空 `PlayerData`、按钮、GuesserButton
- 隐藏的级联操作容易引发时序 bug

### 8. 无卸载安全
- `Main.Unload()` 清理集合但 `RoleRpcManager.DispatchTable`, `IRpcHandler.Handlers`, 加载的程序集从不释放
- 热重载会内存泄漏

### 9. 全局可变状态泛滥
- `GameUtils.PlayerData`, `CustomOption.Options`, `ListenerManager._handlers`, `IRpcHandler.Handlers`, `RoleRpcManager.DispatchTable` 全是 public/static 可变集合

---

## 🟢 低优先级 / 设计问题

### 10. 角色实现不一致
- 只有 `Reaper` 用新的 `Float()` 便捷方法，其余 20+ 角色仍用冗长的 `CreateOption(() => ..., new FloatOptionValueRule(...))`
- Jester/Reporter 仍实现 `IListener`（旧模式），Inspector 用 `OnRoleAbilityUsed +=`（第三种模式）
- 6 个角色用硬编码语言路径，其余用 `GetContextFromLanguage()` 自动前缀
- `RoleRegistry` 已创建但未集成，`CustomRoleManager` 仍是实际注册中心

### 11. God Object 散布
- `PlayerUtils.cs` (822 行) — 混合数据模型、RPC、UI、玩家查询
- `CustomButton.cs` (611 行) — 自标 `[ShitCode]`
- `GamePatch.cs` (372 行) — 15+ 个 patch 类塞在一个文件
- `CustomOption.cs` (465 行) — 静态全局列表 + RPC + 文件 I/O + UI 刷新

### 12. 插件系统泄漏
- `CSharpPluginLoader` 用 `AssemblyLoadContext.Default.LoadFromStream` 加载 DLL，Unload 时不释放 AssemblyLoadContext
- `RuntimeHelpers.RunClassConstructor` 触发所有类型的静态构造函数——副作用重

### 13. 线程安全不一致
- `EventBus` 有 `lock`，`ListenerManager` / `ServiceContainer` / `CustomRoleManager` 无锁
- Unity 游戏通常单线程，但不一致容易留下隐患

---

## 架构依赖流向

```
Patches (Harmony hooks)
    ↓ via
ListenerManager (反射 + 委托分发)
    ↓ invokes
IListener implementations (GameListener, RpcListener, etc.)
    ↓ read/write
CustomRoleManager → CustomRole instances
    ↓ use
ServiceContainer (几乎不用), ConfigManager, EventBus (死代码)
```

`Infrastructure` 层设计良好但被架空，`Listener` 层是实际核心。

---

## 角色实现详情

### 基类

| 基类 | 文件 | CampType | 自动设置 |
|---|---|---|---|
| `CustomRole` | `COG/Role/CustomRole.cs` | Any | 根类，提供 `On<T>`, `CreateOption`, `NewRpc`, `CreateRoleRpc`, `AddButton` |
| `CrewmateRole` | `COG/Role/Camp/CrewmateRole.cs` | `Crewmate` | `base(color, CampType.Crewmate)` |
| `ImpostorRole` | `COG/Role/Camp/ImpostorRole.cs` | `Impostor` | `base(color, CampType.Impostor)` + `CanKill/CanVent/CanSabotage = true` |
| `NeutralRole` | `COG/Role/Camp/NeutralRole.cs` | `Neutral` | `base(color, CampType.Neutral)` |

### Impostor 角色

| 角色 | 选项 | 按钮 | 事件 | 备注 |
|---|---|---|---|---|
| Cleaner | `CleanBodyCd` (Float) | `CleanBodyButton` | — | `CustomButton.Builder` |
| Nightmare | `_storeCooldown` (Float) | `_storeButton` | — | `CreateRoleRpc` 多类型 RPC |
| Reaper | `TimeToReduce` (Float via `Float()`) | — | `On<PlayerMurderEvent>` | 唯一用 `Float()` 便捷方法 |
| Spy | `_observeCooldown` (Float) | `_observeButton` | — | `CreateRoleRpc<bool>` |
| Stabber | `_maxUseTime` (Float) | `_dispatchButton` | — | `DefaultKillButtonSetting.AddCustomCondition` |
| Troublemaker | `_disturbDuration`, `_disturbCooldown` (Float) | `_disturbButton` | — | `NewRpc` + `.Receive()` |

### Crewmate 角色

| 角色 | 选项 | 按钮 | 事件 | 备注 |
|---|---|---|---|---|
| Bait | `KillerSelfReportDelay`, `WarnKiller` | — | `On<PlayerMurderEvent>` | `WarnKiller` 选项创建但未读取 |
| Chief | — | `_giveKillButton`, `_giveShieldButton` | — | 直接 `new KillButtonSetting` |
| Doorman | — | `BlockButton` | — | 静态单例反模式 |
| Enchanter | `ImmobilizationDuration`, `CooldownIncreament` | `ContractButton` | `On<PlayerMurderEvent>` | 拼写错误 Increament→Increment |
| Inspector | `AbilityCooldownOption` | `ExamineButton` | — | 用旧 `OnRoleAbilityUsed +=` |
| Seer | `Cooldown`, `InitialAvailableUsableTimes` | `CheckButton` | 多事件 | 直接修改 `Data.PlayerName` |
| Sheriff | `SheriffKillCd` | — | — | 修改 `DefaultKillButtonSetting` |
| SoulHunter | `ReviveAfter`, `SoulHunterKillCd` | — | 多事件 | 用 `RpcMark` 标签 |
| Technician | — | `RepairButton` | `On<VentCheckEvent>` | `CanVent = true` |
| Vigilante | `_minCrewmateNumber` | — | `On<PlayerFixedUpdateEvent>` | — |
| Witch | `_antidoteCooldown` | `_antidoteButton` | 多事件 | 静态可变状态，`OnMeetingStarts` 未实现 |

### Neutral 角色

| 角色 | 选项 | 按钮 | 事件 | 备注 |
|---|---|---|---|---|
| DeathBringer | `_killCooldown`, `_neededPlayerNumber` | `_stareButton` | `On<PlayerReportDeadBodyEvent>` | — |
| Jester | `_allowStartMeeting`, `_allowReportDeadBody` | — | — | 实现 `IListener`（旧模式） |
| Reporter | `_neededReportTimes` | — | — | 实现 `IListener`（旧模式） |

### SubRole

| 角色 | 选项 | 备注 |
|---|---|---|
| Guesser | `MaxGuessTime`, `GuessContinuously`, `EnabledRolesOnly` | 实现 `IMeetingButton` |
| SpeedBooster | `IncreasingSpeed` | 最简单角色 |

---

## 代码异味清单

1. 只有 1 个角色用新的 `Float()` 便捷方法，其余用冗长 `CreateOption`
2. 6 个角色用硬编码语言路径，重命名角色会静默破坏
3. `Enchanter` 选项名拼写错误 `CooldownIncreament`
4. Witch/Doorman/Reporter 有静态可变状态
5. Doorman 静态单例反模式
6. 部分事件处理器缺少 `[LocalOnly]`
7. Witch `OnMeetingStarts` 未实现（`// TODO`）
8. 三套 RPC 模式共存
9. Seer 直接修改 `Data.PlayerName`（脆弱）
10. Nightmare/Spy 标注 `[NotTested("rpc")]`
11. Chief 直接 `new KillButtonSetting`
12. Bait 的 `WarnKiller` 选项创建但未使用
13. `Guesser.GuessedTime` setter 是 internal
14. `Seer.AvailableUsageTimes` 无封装
