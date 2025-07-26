using COG.Command.Impl;
using COG.Utils;
using InnerNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COG.Rpc;

public class RpcWriter
{
    private readonly MessageWriter[] _writers;
    private bool _finished = false;

    internal RpcWriter(MessageWriter[] writers)
    {
        _writers = writers;
    }

    public RpcWriter Write(bool value)
    {
        foreach (var messageWriter in _writers) messageWriter.Write(value);
        return this;
    }

    public RpcWriter Write(byte[] bytes)
    {
        foreach (var messageWriter in _writers) messageWriter.Write(bytes);
        return this;
    }

    public RpcWriter Write(string value)
    {
        foreach (var messageWriter in _writers) messageWriter.Write(value);
        return this;
    }

    public RpcWriter Write(byte value)
    {
        foreach (var messageWriter in _writers) messageWriter.Write(value);
        return this;
    }

    public RpcWriter Write(sbyte value)
    {
        foreach (var messageWriter in _writers) messageWriter.Write(value);
        return this;
    }

    public RpcWriter Write(float value)
    {
        foreach (var messageWriter in _writers) messageWriter.Write(value);
        return this;
    }

    public RpcWriter Write(int value)
    {
        foreach (var messageWriter in _writers) messageWriter.Write(value);
        return this;
    }

    public RpcWriter WritePacked(int value)
    {
        foreach (var messageWriter in _writers) messageWriter.WritePacked(value);
        return this;
    }

    public RpcWriter WritePacked(uint value)
    {
        foreach (var messageWriter in _writers) messageWriter.WritePacked(value);
        return this;
    }

    public RpcWriter WriteBytesAndSize(byte[] bytes)
    {
        foreach (var messageWriter in _writers) messageWriter.WriteBytesAndSize(bytes);
        return this;
    }

    public RpcWriter WriteNetObject(InnerNetObject obj)
    {
        foreach (var messageWriter in _writers) messageWriter.WriteNetObject(obj);
        return this;
    }

    public RpcWriter WriteVector2(Vector2 vec)
    {
        foreach (var messageWriter in _writers) NetHelpers.WriteVector2(vec, messageWriter);
        return this;
    }

    public void Finish()
    {
        if (_finished) return;
        foreach (var messageWriter in _writers) AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        _finished = true;
    }

    public MessageWriter[] GetWriters() => _writers;

    public static RpcWriter Start(PlayerControl playerControl, RpcCalls callId,
        PlayerControl[]? targets = null)
    {
        return Start(playerControl, (byte)callId, targets);
    }

    public static RpcWriter Start(PlayerControl playerControl, KnownRpc callId,
        PlayerControl[]? targets = null)
    {
        return Start(playerControl, (byte)callId, targets);
    }

    public static RpcWriter Start(KnownRpc callId,
        PlayerControl[]? targets = null)
    {
        return Start(PlayerControl.LocalPlayer, (byte)callId, targets);
    }

    public static RpcWriter Start(RpcCalls callId,
        PlayerControl[]? targets = null)
    {
        return Start(PlayerControl.LocalPlayer, (byte)callId, targets);
    }

    public static RpcWriter Start(byte callId,
        PlayerControl[]? targets = null)
    {
        return Start(PlayerControl.LocalPlayer, callId, targets);
    }

    public static RpcWriter Start(PlayerControl playerControl, byte callId,
        PlayerControl[]? targets = null)
    {
        List<string> parts = new();

        var writers = new List<MessageWriter>();

        if (DebugCommand.EnableRpcTest) // directly uses -1 for targetClientId and PROVED TO BE STILL UNABLE TO SEND RPC
        {
            targets ??= PlayerUtils.GetAllPlayers().Where(p => p.PlayerId != playerControl.PlayerId).ToArray();
            foreach (var control in targets)
            {
                writers.Add(AmongUsClient.Instance.StartRpcImmediately(playerControl.NetId, callId, SendOption.Reliable,
                    control.GetClientID()));
                parts.Add($"{control.name}({control.PlayerId})");
            }
        }
        else // legacy rpc sending, for unable to send rpc by directly using -1 for argument targetClientId
        {
            if (targets == null)
            {
                writers.Add(AmongUsClient.Instance.StartRpcImmediately(playerControl.NetId, callId, SendOption.Reliable));
                parts.Add("everyone");
            }
            else
            {
                foreach (var control in targets)
                {
                    writers.Add(AmongUsClient.Instance.StartRpcImmediately(playerControl.NetId, callId, SendOption.Reliable,
                        control.GetClientID()));
                    parts.Add($"{control.name}({control.PlayerId})");
                }
            }
        }

        Main.Logger.LogInfo($"Rpc {callId} sent to {string.Join(", ", parts)}");

        return new RpcWriter(writers.ToArray());
    }

    public static void StartAndSend(PlayerControl player, KnownRpc rpc, PlayerControl[]? targets = null)
        => Start(player, rpc, targets).Finish();

    public static void StartAndSend(PlayerControl player, RpcCalls rpc, PlayerControl[]? targets = null)
        => Start(player, rpc, targets).Finish();

    public static void StartAndSend(PlayerControl player, byte rpc, PlayerControl[]? targets = null)
        => Start(player, rpc, targets).Finish();

    public static void StartAndSend(KnownRpc rpc, PlayerControl[]? targets = null)
        => Start(rpc, targets).Finish();

    public static void StartAndSend(RpcCalls rpc, PlayerControl[]? targets = null)
        => Start(rpc, targets).Finish();

    public static void StartAndSend(byte rpc, PlayerControl[]? targets = null)
        => Start(rpc, targets).Finish();
}