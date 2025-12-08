using System;
using System.Linq;
using COG.Config.Impl;
using COG.Constant;
using COG.UI.Hud.CustomButton;
using COG.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace COG.Role.Impl.Crewmate;

public class Doorman : CustomRole
{
    public const MapOptions.Modes CustomMode = (MapOptions.Modes)int.MaxValue;

    private static SystemTypes? _lastRoom;
    private static Doorman _instance = null!;
    private static bool _isShowing;

    private bool _usedThisRound;

    public Doorman() : base(Color.blue, CampType.Crewmate)
    {
        _instance = this;

        BlockButton = CustomButton.Of("doorman-block",
            () =>
            {
                HudManager.Instance.ToggleMapVisible(new MapOptions
                {
                    AllowMovementWhileMapOpen = true,
                    Mode = CustomMode
                });
            },
            () =>
            {
                _usedThisRound = false;
                _lastRoom = null;
            },
            () => !_usedThisRound,
            () => true,
            ResourceUtils.LoadSprite(ResourceConstant.BlockButton),
            2,
            LanguageConfig.Instance.BlockAction,
            () => 0f,
            -1);

        AddButton(BlockButton);
    }

    public CustomButton BlockButton { get; }

    public override void ClearRoleGameData()
    {
        _isShowing = false;
        _lastRoom = null;
        MapBehaviour.Instance
            .TryDestroy(); // destroy modified map, HudManager will auto instantiate when u open map next time
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Show))]
    [HarmonyPrefix]
    private static bool ShowMapPatch(MapBehaviour __instance, MapOptions opts)
    {
        if (opts.Mode != CustomMode || !_instance.IsLocalPlayerRole()) return true;

        _isShowing = true;

        __instance.ShowSabotageMap();
        __instance.infectedOverlay.allButtons.Where(b => !b.name.Contains("Doors"))
            .Do(b => b.gameObject.SetActive(false)); // Hide critical sabotage buttons

        var texts = __instance.transform.FindChild("RoomNames (1)").GetComponentsInChildren<TextMeshPro>(true);
        var buttons = __instance.infectedOverlay.allButtons.Where(b => b.name.Contains("Doors"));
        foreach (var button in buttons)
            button.OnClick.AddListener((UnityAction)new Action(() =>
            {
                var room = button.transform.parent.GetComponent<MapRoom>();
                _lastRoom = room.room;
                var shipRoom = ShipStatus.Instance.FastRooms[_lastRoom.Value];

                var nameList = PlayerUtils.GetAllAlivePlayers()
                    .Where(player => shipRoom.roomArea.OverlapPoint(player.GetTruePosition()))
                    .Select(player => player.Data.PlayerName.Color(player.Data.Color));

                var finalString = string.Join('\n', nameList);
                if (finalString == "") finalString = _instance.GetContextFromLanguage("no-player");

                __instance.Close();
                HudManager.Instance.ShowPopUp(TranslationController.Instance.GetString(_lastRoom.Value) + ": \n\n" +
                                              finalString);
                _instance._usedThisRound = true;
            }));

        return false;
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Close))]
    [HarmonyPostfix]
    private static void MapClosePatch(MapBehaviour __instance)
    {
        if (_isShowing)
            __instance.gameObject.TryDestroy();
    }
}