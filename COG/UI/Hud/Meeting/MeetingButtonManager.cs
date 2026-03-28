using System;
using System.Collections.Generic;
using System.Linq;
using COG.Utils;
using UnityEngine;

namespace COG.UI.Hud.Meeting;

[HarmonyPatch(typeof(MeetingHud))]
public static class MeetingButtonManager
{
    private const string ButtonNamePrefix = "COG_MeetingButton_";

    private static readonly Vector3 BaseLocalPosition = new(-0.95f, 0.03f, -1.31f);

    private const float ButtonSpacingX = 0.45f;
    private static bool _buttonsCreated;
    private static int _tickCounter;

    [HarmonyPatch(nameof(MeetingHud.Start)), HarmonyPostfix]
    public static void OnMeetingStart(MeetingHud __instance)
    {
        _buttonsCreated = false;
        _tickCounter = 0;
        TryCreateButtons(__instance);
    }

    [HarmonyPatch(nameof(MeetingHud.Update)), HarmonyPostfix]
    public static void OnMeetingUpdate(MeetingHud __instance)
    {
        if (__instance == null) return;

        _tickCounter = (_tickCounter + 1) % 20;
        if (_tickCounter != 0) goto UpdateOnly;

        if (__instance.state == MeetingHud.VoteStates.Results)
        {
            ClearAllButtons(__instance);
            return;
        }

        var handlers = GetLocalPlayerMeetingButtonHandlers();

        if (_buttonsCreated && handlers.Count == 0)
        {
            ClearAllButtons(__instance);
        }

        if (handlers.Count > 0)
        {
            TryCreateButtons(__instance);
        }

        foreach (var pva in __instance.playerStates)
        {
            if (pva == null) continue;
            var data = PlayerUtils.GetPlayerById(pva.TargetPlayerId)?.Data;
            if (data != null) continue;

            RemoveButtonsFromArea(pva.transform);
        }

        UpdateOnly:
        foreach (var handler in GetLocalPlayerMeetingButtonHandlers())
            handler.OnMeetingButtonUpdate(__instance);
    }

    public static void RefreshButtons(MeetingHud meetingHud)
    {
        ClearAllButtons(meetingHud);
        TryCreateButtons(meetingHud);
    }

    private static void TryCreateButtons(MeetingHud meetingHud)
    {
        var handlers = GetLocalPlayerMeetingButtonHandlers();
        if (handlers.Count == 0) return;

        foreach (var pva in meetingHud.playerStates)
        {
            if (pva == null || !pva.gameObject.activeSelf) continue;

            var existingCount = CountExistingCogButtons(pva.transform);

            for (var idx = 0; idx < handlers.Count; idx++)
            {
                var handler = handlers[idx];
                var buttonName = ButtonNamePrefix + idx;

                if (pva.transform.FindChild(buttonName) != null) continue;

                if (!handler.ShouldShowMeetingButton()) continue;
                if (!handler.ShouldShowMeetingButtonFor(pva)) continue;

                var position = BaseLocalPosition + new Vector3((existingCount + idx) * ButtonSpacingX, 0f, 0f);

                CreateSingleButton(pva, buttonName, position, handler);
            }
        }

        _buttonsCreated = true;
    }

    private static void CreateSingleButton(
        PlayerVoteArea pva,
        string buttonName,
        Vector3 localPosition,
        IMeetingButton handler)
    {
        var template = pva.Buttons.transform.Find("CancelButton")?.gameObject;
        if (template == null)
        {
            Main.Logger.LogWarning("[MeetingButtonManager] CancelButton template not found.");
            return;
        }

        var btn = UnityEngine.Object.Instantiate(template, pva.transform);
        btn.name = buttonName;
        btn.transform.localPosition = localPosition;

        var renderer = btn.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sprite = handler.MeetingButtonSprite;

        var passive = btn.GetComponent<PassiveButton>();
        if (passive != null)
        {
            passive.StopAllCoroutines();
            passive.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            var capturedPva = pva;
            var capturedHandler = handler;
            passive.OnClick.AddListener(new Action(() => capturedHandler.OnMeetingButtonClick(capturedPva)));
        }
    }

    private static void ClearAllButtons(MeetingHud meetingHud)
    {
        foreach (var pva in meetingHud.playerStates)
        {
            if (pva == null) continue;
            RemoveButtonsFromArea(pva.transform);
        }

        _buttonsCreated = false;
    }

    private static void RemoveButtonsFromArea(Transform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (child.name.StartsWith(ButtonNamePrefix))
                UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    private static int CountExistingCogButtons(Transform parent)
    {
        var count = 0;
        foreach (Transform child in parent)
            if (child.name.StartsWith(ButtonNamePrefix))
                count++;
        return count;
    }

    private static List<IMeetingButton> GetLocalPlayerMeetingButtonHandlers()
    {
        var result = new List<IMeetingButton>();

        var mainRole = PlayerControl.LocalPlayer?.GetMainRole();
        if (mainRole is IMeetingButton mainHandler && mainHandler.ShouldShowMeetingButton())
            result.Add(mainHandler);

        if (PlayerControl.LocalPlayer != null)
        {
            foreach (var sub in PlayerControl.LocalPlayer.GetSubRoles())
            {
                if (sub is IMeetingButton subHandler && subHandler.ShouldShowMeetingButton())
                    result.Add(subHandler);
            }
        }

        return result;
    }
}
