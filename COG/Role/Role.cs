using COG.Listener;
using UnityEngine;

namespace COG.Role;

/// <summary>
/// 用来表示一个职业
/// </summary>
public abstract class Role
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public int ID { get; }
    /// <summary>
    /// 角色颜色
    /// </summary>
    public Color Color { get; protected set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; protected set; }
    
    /// <summary>
    /// 角色介绍
    /// </summary>
    public string Description { get; protected set; }

    /// <summary>
    /// 角色阵营
    /// </summary>
    public Camp Camp { get; protected set; }

    protected Role(int id)
    {
        ID = id;
        Color = Color.white;
        Camp = Camp.Crewmate;
    }

    public abstract IListener GetListener(PlayerControl player);
}