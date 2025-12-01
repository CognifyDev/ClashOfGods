using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COG.Rpc;
using COG.Utils;
using UnityEngine;

namespace COG.Command.Impl;

public class RpcCommand : CommandBase
{
    private byte _current;
    private int _initialSize;
    private RpcWriter? _writer;

    public RpcCommand() : base("rpc")
    {
    }

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
                    if (_writer != null) GameUtils.SendSystemMessage("当前仍存在一个RpcWriter实例！");
                    var value = args[1];
                    if (int.TryParse(value, out var result))
                    {
                        _current = (byte)result;
                        _writer = player.StartRpcImmediately(_current);
                    }
                    else
                    {
                        if (Enum.TryParse<RpcCalls>(value, true, out var vanillaRpc))
                            _writer = player.StartRpcImmediately(vanillaRpc);
                        else if (Enum.TryParse<KnownRpc>(value, false, out var modRpc))
                            _writer = player.StartRpcImmediately(modRpc);
                        else
                            throw new NotSupportedException("The RPC you request to send is not supported.");
                    }

                    _initialSize = _writer.GetWriters().First().Length;

                    GameUtils.SendSystemMessage("一个RpcWriter实例已启动！\n输入 /rpc help 获得更多信息。");
                }
                    break;
                default:
                {
                    StringBuilder sb = new();
                    sb.AppendLine("/rpc add %dataType% %context%")
                        .AppendLine("可用类型：byte, sbyte, int, ushort, uint, ulong, bool, float, string, player, vector")
                        .AppendLine("""
                                    例：
                                    /rpc add bool true
                                    /rpc add vector2 3 -1.2
                                    /rpc add player 3（玩家Id）
                                    /rpc add player 玩家名字
                                    /rpc add vector 1 -3
                                    """)
                        .Append("""
                                /rpc start %callId%
                                /rpc send
                                /rpc close
                                """);
                    GameUtils.SendSystemMessage(sb.ToString());
                }
                    break;
                case "add":
                {
                    var typeName = args[1];
                    if (string.IsNullOrEmpty(typeName) || _writer == null)
                        throw new NullReferenceException(
                            "You haven't started a RpcWriter instance yet or the name of the type you entered is null. (Normally, the second situation won't occur.)");

                    List<(string, string)> nameToClass =
                    [
                        ("sbyte", "SByte"),
                        ("int", "Int32"),
                        ("ushort", "UInt16"),
                        ("uint", "UInt32"),
                        ("ulong", "UInt64"),
                        ("float", "Single")
                    ];

                    if (typeName is not ("player" or "string" or "vector"))
                    {
                        typeName = typeName[0].ToString().ToUpper() + typeName[1..];
                        if (nameToClass.Any(tuple => tuple.Item1 == typeName))
                            typeName = nameToClass.First(tuple => tuple.Item1 == typeName).Item2;
                        var assembly = typeof(int).Assembly;
                        var typeLocation = "System." + typeName;
                        var type = assembly.GetType(typeLocation);
                        if (type == null) throw new NotSupportedException($"The data type {typeName} is null.");

                        object[] array = [args[2], new()];
                        var method = type.GetMethod("TryParse",
                        [
                            typeof(string),
                                assembly.GetType(typeLocation + "&")! /* Out argument actually is a pointer */
                        ]);
                        if (method == null)
                            throw new NotSupportedException(
                                "The parsing method is null. (Normally, you aren't able to see this message.)");

                        var success = (bool)method.Invoke(null, array)!;
                        if (success)
                        {
                            dynamic result = array[1];
                            _writer.Write(result);
                        }
                        else
                        {
                            throw new NotSupportedException("Error parsing data.");
                        }
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
                                    pc = PlayerControl.AllPlayerControls.ToArray()
                                        .FirstOrDefault(p => p.Data.PlayerName == nameOrId);

                                if (!pc) throw new NullReferenceException("Error getting player to write.");
                                _writer.WriteNetObject(pc!);
                                break;
                            }
                            case "string":
                            {
                                List<string> strList = new(args);
                                if (strList.Count > 1) strList.RemoveAt(0);
                                StringBuilder sb = new();
                                var i = 0;
                                foreach (var str in strList)
                                {
                                    if (i != 0) sb.Append(' ');
                                    sb.Append(str);
                                    i++;
                                }

                                _writer.Write(sb.ToString());
                                break;
                            }
                            case "vector":
                            {
                                var (xStr, yStr) = (args[2], args[3]);
                                var (x, y) = (-1, -1);

                                if (int.TryParse(xStr, out x) && int.TryParse(yStr, out y))
                                    _writer.WriteVector2(new Vector2(x, y));
                                else
                                    throw new InvalidCastException("The position of the vector is invalid.");
                                break;
                            }
                        }
                    }

                    GameUtils.SendSystemMessage("写入成功！");
                    break;
                }
                case "send":
                {
                    if (_writer == null) throw new NullReferenceException("Writer is null.");
                    _writer.Finish();

                    var deltaSize = _writer.GetWriters().First().Length - _initialSize;
                    var reader = MessageReader.GetSized(deltaSize);

                    Array.Copy(_writer.GetWriters().First().Buffer, _initialSize, reader.Buffer, 0, deltaSize);

                    _writer = null;

                    PlayerControl.LocalPlayer.HandleRpc(_current, reader);

                    GameUtils.SendSystemMessage("Rpc已发送！");
                    break;
                }
                case "close":
                {
                    _writer = null;
                    break;
                }
            }
        }
        catch (System.Exception e)
        {
            GameUtils.SendGameMessage("出现异常，请检查是否已开启实例或数据格式正确！\n要了解更多详细信息，请阅读日志。");
            Main.Logger.LogError(e);
        }

        return false;
    }
}