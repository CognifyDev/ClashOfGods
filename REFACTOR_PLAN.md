# ClashOfGods 架构重构计划

## 当前状态

- **分支**: `refactor/architecture-v2`
- **已推送**: `5b706b2` + `05a86bd`（共 2 个重构提交）
- **未提交**: 6 个文件（Reaper 迁移 WIP + 基础设施微调）
- **编译状态**: 0 error，1 warning（CS0114，需加 `override`）

---

## 已完成工作（已提交）

### Commit 1: `05a86bd` — major architecture overhaul
基础设施层：
- `COG/Infrastructure/EventBase.cs` — 事件基类
- `COG/Infrastructure/EventBus.cs` — 事件总线
- `COG/Infrastructure/EventHandlerAttribute.cs` — 处理器属性
- `COG/Infrastructure/GameEvent.cs` — 游戏事件
- `COG/Infrastructure/GameEventType.cs` — 事件类型枚举
- `COG/Infrastructure/IEvent.cs` — 事件接口
- `COG/Infrastructure/ServiceContainer.cs` — 服务容器
- `COG/Infrastructure/ConfigManager.cs` — 配置管理
- `COG/Infrastructure/GameConfig.cs` — 游戏配置
- `COG/Infrastructure/IConfig.cs` — 配置接口

角色组件：
- `COG/Role/Components/RoleButtons.cs`
- `COG/Role/Components/RoleCapabilities.cs`
- `COG/Role/Components/RoleMetadata.cs`
- `COG/Role/Components/RoleOptions.cs`

CustomRole 增强：
- 事件委托系统 `On<T>()` / `Off<T>()` / `RaiseEvent<T>()`
- 生命周期钩子 `AfterSharingRoles()` / `ClearRoleGameData()` / `OnRoleGameDataGettingSynchronized()`
- 能力属性从 `protected init` 改为 `protected set`
- 便捷工厂方法 `Bool()` / `Float()` / `Int()` / `Button()`

### Commit 2: `5b706b2` — role system infrastructure
阵营基类：
- `COG/Role/Camp/CrewmateRole.cs` — 船员阵营基类
- `COG/Role/Camp/ImpostorRole.cs` — 内鬼阵营基类
- `COG/Role/Camp/NeutralRole.cs` — 中立阵营基类

选项包装：
- `COG/Role/Options/BoolOption.cs`
- `COG/Role/Options/FloatOption.cs`
- `COG/Role/Options/IntOption.cs`
- `COG/Role/Options/RoleButtonBuilder.cs`

注册系统：
- `COG/Role/RoleRegistry.cs`

---

## 未提交修改（当前 WIP）

| 文件 | 修改内容 |
|------|---------|
| `COG/Listener/ListenerManager.cs` | 添加 `DispatchToRoleDelegates` 事件分发到角色 |
| `COG/Role/Camp/ImpostorRole.cs` | 添加 `SetImpostorCapabilities()` 方法 |
| `COG/Role/CustomRole.cs` | 属性 setter 微调 |
| `COG/Role/Impl/Impostor/Reaper.cs` | **新架构迁移（基本完成）** |
| `COG/Role/Options/FloatOption.cs` | 移除多余分号 |
| `COG/Role/Options/IntOption.cs` | 移除多余分号 |

---

## 重要发现：命名空间遮蔽 Bug

### 问题
在 `COG.Role.Impl.Impostor` 命名空间中，未限定名 `ImpostorRole` 不会解析到 `COG.Role.Camp.ImpostorRole`，而是解析到全局命名空间的某个类型（可能是 Among Us 游戏库中的同名类型），导致：
- CS0103: `Float`/`On`/`DefaultKillButtonSetting` 未找到（继承链断裂）
- CS0029: `Reaper` 无法转换为 `COG.Role.CustomRole`

### 解决方案
所有新架构角色类中，基类必须**全限定命名空间**：

```csharp
// ❌ 错误
public class Reaper : ImpostorRole

// ✅ 正确
public class Reaper : COG.Role.Camp.ImpostorRole
```

对于 Crewmate 角色也一样：
```csharp
public class Seer : COG.Role.Camp.CrewmateRole
public class Jester : COG.Role.Camp.NeutralRole
```

### Reaper 待修复
- 添加 `override` 关键字到 `ClearRoleGameData()`（当前有 CS0114 警告）

---

## 待完成工作

### 阶段 1：完成 Reaper 迁移（立即）
1. 在 Reaper.cs 中添加 `override` 关键字
2. 提交 WIP 变更
3. 推送分支

### 阶段 2：迁移 Impostor 角色
每个角色改为继承 `COG.Role.Camp.ImpostorRole`，使用新架构：

| 文件 | 基类 | 难度 |
|------|------|------|
| `Impostor/Impostor.cs` | 保留原样（游戏内置） | - |
| `Impostor/Cleaner.cs` | `ImpostorRole` | 低 |
| `Impostor/Stabber.cs` | `ImpostorRole` | 低-中 |
| `Impostor/Troublemaker.cs` | `ImpostorRole` | 中 |
| `Impostor/Spy.cs` | `ImpostorRole` | 中 |
| `Impostor/Nightmare.cs` | `ImpostorRole` | 高 |

### 阶段 3：迁移 Crewmate 角色
继承 `COG.Role.Camp.CrewmateRole`：

`Bait`, `Chief`, `Declarer`, `Doorman`, `Enchanter`, `Inspector`, `Seer`, `Sheriff`, `SoulHunter`, `Technician`, `Vigilante`, `Witch`

### 阶段 4：迁移 Neutral 角色
继承 `COG.Role.Camp.NeutralRole`：

`DeathBringer`, `Jester`, `Reporter`

### 阶段 5：迁移 SubRole
`Guesser`, `SpeedBooster`（可能需要新的 SubRole 基类）

### 阶段 6：迁移 Unknown
`Unknown.cs`（待确认角色类型）

---

## 迁移模式参考（Reaper）

```csharp
using COG.Role.Camp;  // 仍然需要 using，用于非基类引用
using UnityEngine;

namespace COG.Role.Impl.Impostor;

// ⚠️ 基类必须全限定！
public class Reaper : COG.Role.Camp.ImpostorRole
{
    private float _cooldown;
    private FloatOption TimeToReduce { get; }

    public Reaper()
    {
        TimeToReduce = Float("time-to-reduce", 0.5f, 5f, 1.5f, 0.5f);
        _cooldown = GameUtils.GetGameOptions()?.KillCooldown ?? 30f;
        DefaultKillButtonSetting.CustomCooldown = () => _cooldown;
        On<Listener.Event.Impl.Player.PlayerMurderEvent>(OnPlayerMurder);
    }

    private void OnPlayerMurder(Listener.Event.Impl.Player.PlayerMurderEvent @event)
    {
        _cooldown = Mathf.Clamp(_cooldown - TimeToReduce.Value, 1f, float.MaxValue);
    }

    public override void ClearRoleGameData()
    {
        _cooldown = GameUtils.GetGameOptions()?.KillCooldown ?? 30f;
    }
}
```

关键点：
1. 基类全限定：`COG.Role.Camp.ImpostorRole`
2. 构造器中使用 `Float()` 等便捷方法（来自 CustomRole）
3. 使用 `On<T>()` 注册事件
4. 使用 `DefaultKillButtonSetting`（来自 CustomRole._buttons）
5. 重写生命周期方法添加 `override`

---

## 构建命令

```bash
export PATH="$HOME/.dotnet:$PATH"
rm -rf COG/obj COG/bin
dotnet restore COG/COG.csproj -p:Platform=Windows
dotnet build COG/COG.csproj --no-restore -p:Platform=Windows
```

## Git 身份

```bash
git -c user.name="coralundersea" -c user.email="mkhjmy@qq.com" commit -m "..."
git push origin refactor/architecture-v2
```

## 环境要求

- .NET SDK 9.0（安装于 `~/.dotnet`）
- 分支：`refactor/architecture-v2`
- Among Us 路径：`~/.local/share/Steam/steamapps/common/Among Us/`
