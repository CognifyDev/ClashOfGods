using COG.Rpc;
using COG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COG.Command.Impl;

public class RpcCommand : Command
{
    public RpcCommand() : base("rpc")
    {
        HostOnly = true;
    }

    private RpcUtils.RpcWriter? Writer = null;

    public override void OnExecute(PlayerControl player, string[] args)
    {
        // /rpc <CallId> <string>
        try
        {
            var sign = args.FirstOrDefault();
            var chat = HudManager.Instance.Chat;
            switch (sign)
            {
                case "start":
                    {
                        if (int.TryParse(args[1], out var result))
                        {
                            if (Enum.IsDefined((RpcCalls)result))
                                Writer = RpcUtils.StartRpcImmediately(player, (RpcCalls)result);
                            else if (Enum.IsDefined((KnownRpc)result))
                                Writer = RpcUtils.StartRpcImmediately(player, (KnownRpc)result);
                            else
                            {
                                throw null!; // 直接跳到catch块中
                            }
                        }
                        chat.AddChat(player, "一个RpcWriter实例已启动！\n输入 /rpc help 获得更多信息。", false);
                    }
                    break;
                case "help":
                    {
                        StringBuilder sb = new();
                        sb.Append("/rpc add %dataType% %context%\n")
                            .Append("可用类型：byte, sbyte, int, ushort, uint, ulong, bool, float, string, player, vector\n")
                            .Append("例：\n /rpc add bool true\n /rpc add vector2 3 -1.2\n /rpc add player 3（玩家Id）\n /rpc add player 玩家名字\n /rpc add vector 1 -3")
                            .Append("/rpc start %callId%\n/rpc send");
                        chat.AddChat(player, sb.ToString(), false);
                    }
                    break;
                case "add":
                    {
                        var typeName = args[1];
                        if (string.IsNullOrEmpty(typeName) || Writer == null) throw null!;

                        List<(string, string)> nameToClass = new()
                        {
                            ("sbyte", "SByte"),
                            ("int", "Int32"),
                            ("ushort", "UInt16"),
                            ("uint", "UInt32"),
                            ("ulong", "UInt64"),
                            ("float", "Single"),
                        };
                        if (typeName != "player" || typeName != "string" || typeName != "vector")
                        {
                            typeName = typeName[0].ToString().ToUpper() + typeName[1..];
                            var assembly = typeof(int).Assembly;
                            var typeLocation = "System." + typeName;
                            var type = assembly.GetType(typeLocation);
                            if (type == null) throw null!;

                            object[] array = new object[] { args[2], new object() };
                            var method = type.GetMethod("TryParse", new Type[] { typeof(string), assembly.GetType(typeLocation + "&")! });
                            if (method == null) throw null!;

                            bool success = (bool)method.Invoke(null, array)!;
                            if (success)
                            {
                                dynamic result = array[1];
                                Writer.Write(result);
                            }
                            else 
                                throw null!;
                        }
                        else
                        {
                            switch (typeName)
                            {
                                case "player":
                                    {
                                        var nameOrId = args[2];
                                        PlayerControl? pc = null;

                                        if (byte.TryParse(nameOrId, out var result))
                                            pc = PlayerUtils.GetPlayerById(result);
                                        else
                                            pc = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.Data.PlayerName == nameOrId);

                                        if (!pc) throw null!;
                                        Writer.WriteNetObject(pc!);
                                    }
                                    break;
                                case "string":
                                    {
                                        List<string> strList = new(args);
                                        strList.RemoveAt(0);
                                        StringBuilder sb = new();
                                        int i = 0;
                                        foreach (var str in strList)
                                        {
                                            if (i != 0) sb.Append(' ');
                                            sb.Append(str);
                                            i++;
                                        }
                                        Writer.Write(sb.ToString());
                                    }
                                    break;
                                case "vector":
                                    {
                                        var (xStr, yStr) = (args[2], args[3]);
                                        var (x, y) = (-1, -1);
                                        if (int.TryParse(xStr, out x) && int.TryParse(yStr, out y))
                                        {
                                            var vector = new Vector2(x, y);
                                            Writer.WriteVector2(vector);
                                        }
                                    }
                                    break;
                            }
                        }

                        chat.AddChat(player, "写入成功！", false);
                    }
                    break;
                case "send":
                    {
                        if (Writer == null) throw null!;
                        Writer.Finish();
                        chat.AddChat(player, "Rpc已发送！", false);
                    }
                    break;
            }
        }
        catch
        {
            GameUtils.SendGameMessage("发送失败，检查数据格式");
        }
    }
}