# ClashOfGods 架构重构计划

## 当前状态

- **分支**: `refactor/architecture-v2`
- **最新提交**: `efcf152` — fix: address architecture review issues
- **编译状态**: 0 error，0 code warning

---

## 已完成工作

### Commit 1: `05a86bd` — major architecture overhaul
### Commit 2: `5b706b2` — role system infrastructure
### Commit 3: `c4522c6` — migrate all roles to new architecture
### Commit 4: `efcf152` — fix architecture review issues

---

## 事件系统设计决策

### On\<T\>() vs IListener 选择规则

| 场景 | 使用方式 | 原因 |
|------|---------|------|
| 普通 Postfix 事件 | `On<T>(handler)` | 新架构首选，简洁 |
| 需要本地玩家限制的 Postfix | `On<T>(handler)` + `[LocalOnly]` | 属性驱动，优雅 |
| 需要返回 bool 取消事件的 Prefix | `IListener` + `[EventHandler(Prefix)]` | On\<T\>() 不支持返回值 |
| 需要 `[OnlyInRealGame]` 限制 | `IListener` + 属性 | On\<T\>() 不检查方法属性 |

### 关键约束

1. **`On<T>()` 无法取消事件**: 注册的委托是 `Action<T>` (void return)，不能像 `[EventHandler(Prefix)]` 返回 `bool` 来阻止事件传播。
   - **Jester** 和 **Reporter** 使用 prefix 返回 `bool` 拦截报告事件 → 保留 `IListener`。

2. **`[LocalOnly]` 属性**: 在 `CustomRole.RaiseEvent<T>()` 中检查，只对带 `[LocalOnly]` 的委托方法生效。
   - 替代旧系统的 `[OnlyLocalPlayerWithThisRoleInvokable]`。
   - 不带此属性的处理器对所有拥有该角色的玩家触发。

3. **两套系统共存**: `On<T>()` 和 `IListener` 可以同时使用。`ListenerManager` 先执行 `IListener` 处理器，再执行 `On<T>()` 委托。
   - 新角色优先使用 `On<T>()`。
   - 仅在需要 prefix 取消时使用 `IListener`。

---

## 待完成工作

### 阶段 A：RpcHandler 迁移（deferred）

旧模式 `new RpcHandler<T>(KnownRpc.XXX, ...)` + `RegisterRpcHandler()` 仍然有效。
新模式 `CreateRoleRpc<T>(localId, onPerform)` 提供更好的角色封装。

| 角色 | 旧 RPC | 复杂度 |
|------|--------|--------|
| Technician | `RpcHandler(KnownRpc.ClearSabotages, ...)` | 低 |
| Chief | `RpcHandler<PlayerControl>(KnownRpc.GiveOneKill, ...)` | 低 |
| Enchanter | `RpcHandler<PlayerControl>(KnownRpc.EnchanterPunishesKiller, ...)` | 中 |
| Witch | `RpcHandler<byte>(KnownRpc.WitchUsesAntidote, ...)` | 中 |
| Spy | `RpcHandler<bool>(KnownRpc.SpyRevealClosestTarget, ...)` | 中 |
| Troublemaker | `RpcHandler(KnownRpc.TroubleMakerDisturb, ...)` | 中 |
| Nightmare | `RpcHandler<PlayerControl>(...)` + `RpcHandler<PlayerControl, PlayerControl, float>(...)` | 高 |

### 阶段 B：完整 IListener 淘汰

当所有角色都迁移到 `On<T>()` + `[LocalOnly]` 后，可以考虑：
- 将 `IListener.EmptyListener` 替换为无操作默认
- 逐步移除 `GetListener()` 虚方法

---

## 迁移模式参考（Reaper）

```csharp
using COG.Role.Camp;
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
