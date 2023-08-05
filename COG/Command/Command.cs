using System;

namespace COG.Command;

public abstract class Command
{
    /// <summary>
    ///     构造一个Command
    /// </summary>
    /// <param name="name">命令名称</param>
    public Command(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     是否只允许房主使用
    /// </summary>
    public bool HostOnly { get; set; }

    /// <summary>
    ///     指令内容
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     是否取消玩家发送命令到公屏
    /// </summary>
    public bool Cancellable { get; set; }

    /// <summary>
    ///     命令其他形式
    /// </summary>
    public string[] Aliases { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     玩家运行命令执行逻辑
    /// </summary>
    /// <param name="player">运行命令玩家</param>
    /// <param name="args">命令字符串集</param>
    public abstract void OnExecute(PlayerControl player, string[] args);
}