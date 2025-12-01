using System;

namespace COG.Command;

public abstract class CommandBase
{
    /// <summary>
    ///     构造一个Command
    /// </summary>
    /// <param name="name">命令名称</param>
    protected CommandBase(string name)
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
    ///     命令其他形式
    /// </summary>
    public string[] Aliases { get; set; } = [];

    /// <summary>
    ///     玩家运行命令执行逻辑
    /// </summary>
    /// <param name="player">运行命令玩家</param>
    /// <param name="args">命令字符串集</param>
    /// <returns>是否取消执行</returns>
    public abstract bool OnExecute(PlayerControl player, string[] args);
}