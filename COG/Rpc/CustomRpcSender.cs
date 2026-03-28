using System;
using COG.Role;
using Hazel;
using InnerNet;

namespace COG.Rpc;

public sealed class CustomRpcSender
{
    public enum State
    {
        BeforeInit = 0,
        Ready,
        InRootMessage,
        InRpc,
        Finished
    }

    public readonly MessageWriter Stream;
    public readonly string Name;
    public readonly SendOption SendOption;
    
    public bool IsUnsafe;

    public delegate void OnSendDelegateType();
    public OnSendDelegateType OnSendDelegate;

    private State _currentState = State.BeforeInit;

    private int _currentRpcTarget = -2;

    private CustomRpcSender() { }

    public CustomRpcSender(string name, SendOption sendOption = SendOption.Reliable, bool isUnsafe = false)
    {
        Stream     = MessageWriter.Get(sendOption);
        Name       = name;
        SendOption = sendOption;
        IsUnsafe   = isUnsafe;

        OnSendDelegate = () => Main.Logger.LogInfo($"[CustomRpcSender] \"{Name}\" sent.");
        _currentState  = State.Ready;

        Main.Logger.LogInfo($"[CustomRpcSender] \"{Name}\" is ready.");
    }

    public static CustomRpcSender Create(
        string name       = "Unnamed Sender",
        SendOption option = SendOption.Reliable,
        bool isUnsafe     = false)
        => new(name, option, isUnsafe);

    public State CurrentState
    {
        get => _currentState;
        set
        {
            if (IsUnsafe)
                _currentState = value;
            else
                Main.Logger.LogWarning("[CustomRpcSender] CurrentState 只能在 IsUnsafe=true 时直接赋值。");
        }
    }

    public CustomRpcSender StartMessage(int targetClientId = -1)
    {
        AssertState(State.Ready, nameof(StartMessage));

        if (targetClientId < 0)
        {
            Stream.StartMessage(5);
            Stream.Write(AmongUsClient.Instance.GameId);
        }
        else
        {
            Stream.StartMessage(6);
            Stream.Write(AmongUsClient.Instance.GameId);
            Stream.WritePacked(targetClientId);
        }

        _currentRpcTarget = targetClientId;
        _currentState     = State.InRootMessage;
        return this;
    }

    public CustomRpcSender EndMessage()
    {
        AssertState(State.InRootMessage, nameof(EndMessage));
        Stream.EndMessage();
        _currentRpcTarget = -2;
        _currentState     = State.Ready;
        return this;
    }

    public CustomRpcSender StartRpc(uint targetNetId, KnownRpc rpc)
        => StartRpc(targetNetId, (byte)rpc);

    public CustomRpcSender StartRpc(uint targetNetId, byte callId)
    {
        AssertState(State.InRootMessage, nameof(StartRpc));

        Stream.StartMessage(2);
        Stream.WritePacked(targetNetId);
        Stream.Write(callId);

        _currentState = State.InRpc;
        return this;
    }

    public CustomRpcSender EndRpc()
    {
        AssertState(State.InRpc, nameof(EndRpc));
        Stream.EndMessage();
        _currentState = State.InRootMessage;
        return this;
    }

    public CustomRpcSender AutoStartRpc(
        uint targetNetId,
        byte callId,
        int targetClientId = -1)
    {
        if (targetClientId == -2) targetClientId = -1;

        if (_currentState is not State.Ready and not State.InRootMessage)
        {
            var err = $"[CustomRpcSender] AutoStartRpc: State must be Ready or InRootMessage (in: \"{Name}\")";
            if (IsUnsafe) Main.Logger.LogWarning(err);
            else throw new InvalidOperationException(err);
        }

        if (_currentRpcTarget != targetClientId)
        {
            if (_currentState == State.InRootMessage) EndMessage();
            StartMessage(targetClientId);
        }

        return StartRpc(targetNetId, callId);
    }

    public CustomRpcSender AutoStartRpc(uint targetNetId, KnownRpc rpc, int targetClientId = -1)
        => AutoStartRpc(targetNetId, (byte)rpc, targetClientId);

    public void SendMessage()
    {
        if (_currentState == State.InRootMessage) EndMessage();

        AssertState(State.Ready, nameof(SendMessage));

        AmongUsClient.Instance.SendOrDisconnect(Stream);
        OnSendDelegate?.Invoke();
        _currentState = State.Finished;

        Main.Logger.LogInfo($"[CustomRpcSender] \"{Name}\" finished.");
        Stream.Recycle();
    }

    public CustomRpcSender Write(float   val) => WriteInternal(w => w.Write(val));
    public CustomRpcSender Write(string  val) => WriteInternal(w => w.Write(val));
    public CustomRpcSender Write(int     val) => WriteInternal(w => w.Write(val));
    public CustomRpcSender Write(uint    val) => WriteInternal(w => w.Write(val));
    public CustomRpcSender Write(ushort  val) => WriteInternal(w => w.Write(val));
    public CustomRpcSender Write(byte    val) => WriteInternal(w => w.Write(val));
    public CustomRpcSender Write(sbyte   val) => WriteInternal(w => w.Write(val));
    public CustomRpcSender Write(bool    val) => WriteInternal(w => w.Write(val));
    public CustomRpcSender WritePacked(int  val) => WriteInternal(w => w.WritePacked(val));
    public CustomRpcSender WritePacked(uint val) => WriteInternal(w => w.WritePacked(val));
    public CustomRpcSender WriteNetObject(InnerNetObject obj) => WriteInternal(w => w.WriteNetObject(obj));

    private CustomRpcSender WriteInternal(Action<MessageWriter> action)
    {
        AssertState(State.InRpc, "Write");
        action(Stream);
        return this;
    }

    private void AssertState(State expected, string operation)
    {
        if (_currentState == expected) return;

        var msg = $"[CustomRpcSender] {operation}: current State={_currentState}，need {expected} (in: \"{Name}\")";
        if (IsUnsafe)
            Main.Logger.LogWarning(msg);
        else
            throw new InvalidOperationException(msg);
    }
}

