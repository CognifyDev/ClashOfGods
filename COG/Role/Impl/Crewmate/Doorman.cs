using COG.Constant;
using COG.UI.CustomButton;
using COG.Utils;
using COG.Utils.Coding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace COG.Role.Impl.Crewmate;

[HarmonyPatch]
[NotTested]
[NotUsed]
[WorkInProgress]
public class Doorman : CustomRole
{
    public const MapOptions.Modes CustomMode = (MapOptions.Modes)int.MaxValue;

    public CustomButton BlockButton { get; }

    private bool _usedThisRound = false;

    private static SystemTypes? _lastRoom = null;
    private static TextMeshPro? _roomText = null;
    private static Doorman _instance = null!;
    private static bool _isShowing = false;

    public Doorman() : base(Color.blue, CampType.Crewmate)
    {
        _instance = this;

        BlockButton = CustomButton.Of("doorman-block",
            () =>
            {
                HudManager.Instance.ToggleMapVisible(new()
                {
                    AllowMovementWhileMapOpen = true,
                    Mode = CustomMode
                });
            },
            () =>
            {
                _usedThisRound = false;

                if (_lastRoom.HasValue && _roomText)
                {
                    _roomText!.text = TranslationController.Instance.GetString(_lastRoom.Value);
                    _roomText = null;
                    _lastRoom = null;
                }
            },
            () => !_usedThisRound,
            () => true,
            ResourceUtils.LoadSprite(ResourceConstant.BlockButton)!,
            2,
            KeyCode.B,
            "BLOCK",
            () => 0f,
            -1);

        AddButton(BlockButton);
    }

    public override void ClearRoleGameData()
    {
        MapBehaviour.Instance.Destroy(); // destroy modified map, HudManager will auto instantiate when u open map next time
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Show))]
    [HarmonyPrefix]
    static bool ShowMapPatch(MapBehaviour __instance, MapOptions opts)
    {
        if (opts.Mode != CustomMode || !_instance.IsLocalPlayerRole()) return true;

        _isShowing = true;

        __instance.ShowSabotageMap();
        __instance.infectedOverlay.allButtons.Where(b => !b.name.Contains("Doors")).Do(b => b.gameObject.SetActive(false)); // Hide critical sabotage buttons

        var texts = __instance.transform.FindChild("RoomNames (1)").GetComponentsInChildren<TextMeshPro>(true);
        var buttons = __instance.infectedOverlay.allButtons.Where(b => b.name.Contains("Doors"));
        foreach (var button in buttons)
        {
            button.OnClick.AddListener((UnityAction)new Action(() =>
            {
                var room = button.transform.parent.GetComponent<MapRoom>();
                _lastRoom = room.room;
                _roomText = texts.FirstOrDefault(tmp => tmp.GetComponent<TextTranslatorTMP>().TargetText == TranslationController.Instance.GetSystemName(_lastRoom.Value))!;
                var shipRoom = ShipStatus.Instance.FastRooms[_lastRoom.Value];

                var list = PlayerUtils.GetAllAlivePlayers().Where(player => shipRoom.roomArea.OverlapPoint(player.GetTruePosition()));

                var finalString = string.Join('\n', list);
                if (finalString == "") finalString = "NO_PLAYER";

                __instance.Close();
                HudManager.Instance.ShowPopUp(TranslationController.Instance.GetString(_lastRoom.Value) + ": \n\n" + finalString);
            }));
        }

        return false;
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Close))]
    [HarmonyPostfix]
    static void MapClosePatch(MapBehaviour __instance)
    {
        if (_isShowing)
            __instance.gameObject.Destroy();
    }
}