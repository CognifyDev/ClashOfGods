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

    public Doorman() : base(Color.blue, CampType.Crewmate)
    {
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
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Show))]
    [HarmonyPrefix]
    static bool ShowMapPatch(MapBehaviour __instance, MapOptions opts)
    {
        if (opts.Mode != CustomMode) return true;

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

                var collidersResult = new Collider2D[45];

                var num = shipRoom.roomArea.OverlapCollider(new()
                {
                    useLayerMask = true,
                    useTriggers = true,
                    layerMask = Constants.LivingPlayersOnlyMask
                }, collidersResult);

                var list = new List<string>();

                for (int i = 0; i < num; i++)
                {
                    var collider = collidersResult[i];

                    if (!collider.isTrigger)
                    {
                        var player = collider.GetComponent<PlayerControl>();
                        if (!player) continue;

                        list.Add(player.Data.PlayerName);
                    }
                }

                var finalString = string.Join('\n', list);
                _roomText.DestroyComponent<TextTranslatorTMP>();
                _roomText.text = TranslationController.Instance.GetString(_lastRoom.Value) + "\n\n" + finalString;
            }));
        }

        return false;
    }
}