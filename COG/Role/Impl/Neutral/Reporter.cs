using System.Collections.Generic;
using System.Linq;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Game.CustomWinner.Data;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;
using Random = System.Random;

namespace COG.Role.Impl.Neutral;

[HarmonyPatch]
public class Reporter : CustomRole, IListener, IWinnable
{
    private readonly Dictionary<PlayerControl, uint> _reportersWhoReported = new();
    
    public override void ClearRoleGameData()
    {
        _reportersWhoReported.Clear();
        _isReporterReported = false;
    }

    private readonly CustomOption _neededReportTimes;
    private static bool _isReporterReported = false;
    private static Reporter _instance;
    
    public Reporter() : base(Color.gray, CampType.Neutral)
    {
        _neededReportTimes = CreateOption(() => LanguageConfig.Instance.ReporterNeededReportTimes,
            new FloatOptionValueRule(1F, 1F, 14F, 3F));

        _instance = this;
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnPlayerReport(PlayerReportDeadBodyEvent @event)
    {
        var player = @event.Player;
        var target = @event.Target;
        if (!player.IsRole(this)) return true;

        if (target == null) return true;

        _isReporterReported = true;
        
        if (_reportersWhoReported.TryGetValue(player, out var times))
            _reportersWhoReported[player] = ++times;
        else
            _reportersWhoReported.Add(player, 1);
        
        return false;
    }

    public override IListener GetListener()
    {
        return this;
    }

    public void CheckWin(WinnableData data)
    {
        if (GameStates.IsMeeting) return;
        
        foreach (var (target, times) in _reportersWhoReported)
        {
            if (times < _neededReportTimes.GetFloat()) return;
            
            data.WinnableCampType = CampType; 
            data.WinText = LanguageConfig.Instance.NeutralsWinText.CustomFormat(target);
            data.WinColor = Color;
            data.WinnablePlayers.Add(target.Data);
            data.Winnable = true;
        }
    }

    public uint GetWeight()
    {
        return IWinnable.GetOrder(6);
    }

    [HarmonyPatch(typeof(MeetingIntroAnimation._CoRun_d__17), nameof(MeetingIntroAnimation._CoRun_d__17.MoveNext))]
    [HarmonyPostfix]
    static void MeetingIntroPatch(MeetingIntroAnimation._CoRun_d__17 __instance)
    {
        if (_isReporterReported)
        {
            var pva = __instance.__4__this.transform.FindChild("PlayerVoteArea(Clone)").GetComponent<PlayerVoteArea>();
            pva.Background.sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;

            var icon = pva.PlayerIcon;
            icon.UpdateFromPlayerOutfit(new(), PlayerMaterial.MaskType.ComplexUI, false, false);
            icon.cosmetics.colorBlindText.text = string.Empty;
            icon.cosmetics.nameText.text = _instance.Name;

            var anonymousColor = ColorUtils.AsColor("#8995a4");
            PlayerMaterial.SetColors(anonymousColor, icon.cosmetics.currentBodySprite.BodySprite);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    [HarmonyPostfix]
    static void MeetingUpdatePatch(MeetingHud __instance)
    {
        if (_isReporterReported)
            __instance.playerStates.Do(pva => pva.Megaphone.gameObject.SetActive(false));
    }
}