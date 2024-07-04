using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Rpc;
using COG.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COG.Command.Impl;

public class RpcCommand : Command
{
    public RpcCommand() : base("rpc")
    {
    }

    private RpcUtils.RpcWriter? Writer = null;

    public override bool OnExecute(PlayerControl player, string[] args)
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
                        if (Writer != null) chat.StartCoroutine(CoSendChatMessage("当前仍存在一个RpcWriter实例！").WrapToIl2Cpp());
                        var value = args[1];
                        if (int.TryParse(value, out var result))
                        {
                            if (Enum.IsDefined((RpcCalls)result))
                                Writer = RpcUtils.StartRpcImmediately(player, (RpcCalls)result);
                            else if (Enum.IsDefined((KnownRpc)result))
                                Writer = RpcUtils.StartRpcImmediately(player, (KnownRpc)result);
                            else
                                Writer = RpcUtils.StartRpcImmediately(player, (byte)result);
                        }
                        else
                        {
                            if (Enum.TryParse<RpcCalls>(value, true, out var vanillaRpc))
                                Writer = RpcUtils.StartRpcImmediately(player, vanillaRpc);
                            else if (Enum.TryParse<KnownRpc>(value, false, out var modRpc))
                                Writer = RpcUtils.StartRpcImmediately(player, modRpc);
                            else
                                throw new NotSupportedException("The RPC you request to send is not supported.");
                        }
                        chat.StartCoroutine(CoSendChatMessage("一个RpcWriter实例已启动！\n输入 /rpc help 获得更多信息。").WrapToIl2Cpp());
                    }
                    break;
                default:
                case "help":
                    {
                        StringBuilder sb = new();
                        sb.Append("/rpc add %dataType% %context%\n")
                            .Append("可用类型：byte, sbyte, int, ushort, uint, ulong, bool, float, string, player, vector\n")
                            .Append("例：\n /rpc add bool true\n /rpc add vector2 3 -1.2\n /rpc add player 3（玩家Id）\n /rpc add player 玩家名字\n /rpc add vector 1 -3\n")
                            .Append("/rpc start %callId%\n/rpc send");
                        chat.StartCoroutine(CoSendChatMessage(sb.ToString()).WrapToIl2Cpp());
                    }
                    break;
                case "add":
                    {
                        var typeName = args[1];
                        if (string.IsNullOrEmpty(typeName) || Writer == null) throw new NullReferenceException("You haven't started a RpcWriter instance yet or the name of the type you entered is null. (Normally, the second situation won't be happened.)");

                        List<(string, string)> nameToClass = new()
                        {
                            ("sbyte", "SByte"),
                            ("int", "Int32"),
                            ("ushort", "UInt16"),
                            ("uint", "UInt32"),
                            ("ulong", "UInt64"),
                            ("float", "Single"),
                        };
                        if (typeName is not ("player" or "string" or "vector"))
                        {
                            typeName = typeName[0].ToString().ToUpper() + typeName[1..];
                            var assembly = typeof(int).Assembly;
                            var typeLocation = "System." + typeName;
                            var type = assembly.GetType(typeLocation);
                            if (type == null) throw new NotSupportedException($"The data type {typeName} is null.");

                            object[] array = new object[] { args[2], new object() };
                            var method = type.GetMethod("TryParse", new Type[] { typeof(string), assembly.GetType(typeLocation + "&")! /* Out argument actually is a pointer. */ });
                            if (method == null) throw new NotSupportedException("The parsing method is null. (Normally, you aren't able to see this message.)");

                            bool success = (bool)method.Invoke(null, array)!;
                            if (success)
                            {
                                dynamic result = array[1];
                                Writer.Write(result);
                            }
                            else
                                throw new NotSupportedException("Error parsing data.");
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

                                        if (!pc) throw new NullReferenceException("Error getting player to write.");
                                        Writer.WriteNetObject(pc!);
                                    }
                                    break;
                                case "string":
                                    {
                                        List<string> strList = new(args);
                                        if (strList.Count > 1) strList.RemoveAt(0);
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
                                            Writer.WriteVector2(new Vector2(x, y));
                                        else
                                            throw new InvalidCastException("The position of the vector is invalid.");
                                    }
                                    break;
                            }
                        }
                        chat.StartCoroutine(CoSendChatMessage("写入成功！").WrapToIl2Cpp());
                    }
                    break;
                case "send":
                    {
                        if (Writer == null) throw new NullReferenceException("Writer is null.");
                        Writer.Finish();
                        Writer = null;
                        chat.StartCoroutine(CoSendChatMessage("Rpc已发送！").WrapToIl2Cpp());
                    }
                    break;
            }
        }
        catch (System.Exception e)
        {
            GameUtils.SendGameMessage("出现异常，请检查是否已开启实例或数据格式正确！\n要了解更多详细信息，请阅读日志。");
            Main.Logger.LogError(e);
        }

        return false;
    }

    public IEnumerator CoSendChatMessage(string text)
    {
        yield return new WaitForSeconds(0.2f);
        HudManager.Instance?.Chat?.AddChat(PlayerControl.LocalPlayer, text, false);
    }
}