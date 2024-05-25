using COG.Config.Impl;
using COG.Constant;
using COG.Game.CustomWinner;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.Role.Impl.Impostor;
using COG.Rpc;
using COG.UI.Arrow;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COG.Role.Impl.Neutral;

public class Vulture : Role, IListener, IWinnable
{
    public CustomOption EatingCooldown { get; }
    public CustomOption WinningEatenCount { get; }
    public CustomOption HasArrowToBodies { get; }
    public CustomButton EatButton { get; }
    public static int EatenCount { get; private set; }
    private static DeadBody? ClosestBody { get; set; }

    public Vulture() : base("Vulture", new(139, 69, 19), CampType.Neutral)
    {
        Description = "";

        var page = ToCustomOption(this);
        EatingCooldown = CustomOption.Create(page, "cd", 30f, 10f, 60f, 5f, MainRoleOption);
        WinningEatenCount = CustomOption.Create(page, "count", 4f, 2f, 6f, 1f, MainRoleOption);
        HasArrowToBodies = CustomOption.Create(page, "arrow", true, MainRoleOption);

        EatButton = CustomButton.Create(
            () =>
            {
                ClosestBody = PlayerUtils.GetClosestBody();
                RpcEatBody(ClosestBody!);
            },
            () => EatButton?.ResetCooldown(),
            couldUse: () => ClosestBody,
            () => true,
            null!,
            2,
            KeyCode.E,
            "eat",
            () => EatingCooldown?.GetFloat() ?? 30f,
            0
        );
        AddButton(EatButton);

        EatenCount = 0;
    }

    public void RpcEatBody(DeadBody body)
    {
        RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.EatBody)
            .Write(body.ParentId).WritePacked(EatenCount)
            .Finish();
        EatBody(body);
    }

    public void EatBody(DeadBody body)
    {
        body.gameObject.SetActive(false);
        EatenCount++;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnReceiveRpc(PlayerHandleRpcEvent @event)
    {
        if ((KnownRpc)@event.CallId != KnownRpc.EatBody) return;

        var reader = @event.MessageReader;

        byte pid = reader.ReadByte();
        var body = Object.FindObjectsOfType<DeadBody>().ToList().FirstOrDefault(b => b.ParentId == pid);
        if (!body) return;

        EatenCount = reader.ReadPackedInt32();
        EatBody(body!);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerDead(PlayerMurderEvent @event)
    {
        if (!(HasArrowToBodies?.GetBool() ?? true)) return;

        var victim = @event.Target;
        var body = Object.FindObjectsOfType<DeadBody>().ToList().FirstOrDefault(b => b.ParentId == victim.PlayerId);
        _ = new Arrow(body!.transform.position);
    }

    public override bool OnRoleSelection(List<Role> roles)
    {
        roles.RemoveAll(r => r == CustomRoleManager.GetManager().GetTypeRoleInstance<Cleaner>()); // 场上不能同时存在秃鹫和清洁工
        return false;
    }

    public override void ClearRoleGameData()
    {
        EatenCount = 0;
        ClosestBody = null;
    }

    public ulong GetWeight() => IWinnable.GetOrder(6);

    public bool CanWin()
    {
        if (EatenCount >= (WinningEatenCount?.GetFloat() ?? 4))
        {
            CustomWinnerManager.RegisterWinningPlayers(Players);
            CustomWinnerManager.SetWinColor(Color);
            CustomWinnerManager.SetWinText("Vulture wins");
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
            return true;
        }
        return false;
    }

    public override IListener GetListener() => this;
}