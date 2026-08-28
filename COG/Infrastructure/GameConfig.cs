using System.Collections.Generic;

namespace COG.Infrastructure;

/// <summary>
/// 游戏配置示例，展示如何使用 ConfigManager 和 IConfig 接口
/// </summary>
public class GameConfig : IConfig
{
    public string Name => "GameSettings";
    public string Version => "1.0.0";

    // 角色配置
    public int MaxCrewmates { get; set; } = 15;
    public int MaxImpostors { get; set; } = 3;
    public int MaxNeutrals { get; set; } = 3;

    // 游戏参数
    public float KillCooldown { get; set; } = 30f;
    public int DiscussionTime { get; set; } = 120;
    public int VotingTime { get; set; } = 120;

    // 启用的角色列表
    public List<string> EnabledRoles { get; set; } = new();

    public void Migrate(string fromVersion)
    {
        // 在此处理旧版本配置的迁移逻辑
        // 例如: 版本 0.9.x 到 1.0.0 的迁移
    }
}
