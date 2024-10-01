using System;
using System.Collections.Generic;
using System.Linq;
using COG.Constant;
using COG.Role;
using COG.Role.Impl.SubRole;
using COG.Utils;
using COG.Utils.Coding;
using TMPro;
using UnityEngine;

namespace COG.UI.CustomGameObject.Meeting;

[ShitCode]
public class GuessButton : TemplatedCustomGameObject
{
    public static List<GuessButton> Buttons { get; } = new();
    
    private const string GuessButtonName = "GuessButton";
    private static readonly Vector3 LocalPosition = new(-0.95f, 0.03f, -1.3f);

    private readonly PlayerControl? _target;

    /// <summary>
    ///     赌怪按钮
    /// </summary>
    public GuessButton(MeetingHud hud, GameObject template, PlayerVoteArea playerVoteArea, Guesser guesser) : base(
        template, playerVoteArea.transform)
    {
        Guesser = guesser;
        _target = PlayerUtils.GetPlayerById(playerVoteArea.TargetPlayerId);
        MeetingHud = hud;
        GameObject.name = GuessButtonName;
        GameObject.transform.localPosition = LocalPosition;
        var renderer = GameObject.GetComponent<SpriteRenderer>();
        renderer.sprite = ResourceUtils.LoadSprite(ResourcesConstant.GuessButton, 150F);
        
        Buttons.Add(this);
    }

    public void Destroy()
    {
        GameObject.Destroy();
    }
    
    private Guesser Guesser { get; }

    public MeetingHud MeetingHud { get; }

    public void SetupListeners()
    {
        var button = GameObject.GetComponent<PassiveButton>();
        button.StopAllCoroutines();
        button.OnClick.RemoveAllListeners();
        button.OnClick.AddListener((Action)(() =>
        {
            if (_target == null || !PlayerControl.LocalPlayer.IsAlive() || !_target.IsAlive() || MeetingHud.state == MeetingHud.VoteStates.Discussion || MeetingHud.state == MeetingHud.VoteStates.Voted) return;
            MeetingHud.playerStates.ToList().ForEach(x => x.gameObject.SetActive(false));
            
            var enabledRolesOnly = Guesser.EnabledRolesOnly.GetBool();
            var canGuessSubRoles = Guesser.CanGuessSubRoles.GetBool();
            var roles = enabledRolesOnly
                ? GetCustomRolesFromPlayers()
                : CustomRoleManager.GetManager().GetRoles().Where(role =>
                        canGuessSubRoles || (!canGuessSubRoles && !role.IsSubRole))
                    .ToArray();

            var guesserUI = new PhoneUI(MeetingHud);
            var buttonTemplate = MeetingHud.playerStates[0].transform.FindChild("votePlayerBase");
            var maskTemplate = MeetingHud.playerStates[0].transform.FindChild("MaskArea");
            var smallButtonTemplate = MeetingHud.playerStates[0].Buttons.transform.Find("CancelButton");
            var textTemplate = MeetingHud.playerStates[0].NameText;

            var exitButtonParent = new GameObject().transform;
            guesserUI.AddChild(exitButtonParent);
            var exitButton = Object.Instantiate(buttonTemplate.transform, exitButtonParent);
            Object.Instantiate(maskTemplate, exitButtonParent);
            exitButton.gameObject.GetComponent<SpriteRenderer>().sprite =
                smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
            exitButtonParent.transform.localPosition = new Vector3(2.725f, 2.1f, -5);
            exitButtonParent.transform.localScale = new Vector3(0.217f, 0.9f, 1);
            exitButton.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
            exitButton.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() =>
            {
                Exit(guesserUI);
            }));

            for (var i = 0; i < roles.Length; i++)
            {
                var role = roles[i];
                var buttonParent = new GameObject().transform;
                guesserUI.AddChild(buttonParent);
                var buttonTransform = Object.Instantiate(buttonTemplate, buttonParent);
                Object.Instantiate(maskTemplate, buttonParent);
                var label = Object.Instantiate(textTemplate, buttonTransform);
                int row = i / 5, col = i % 5;
                buttonParent.localPosition = new Vector3(-3.47f + 1.75f * col, 1.5f - 0.45f * row, -5);
                buttonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
                label.text = role.GetColorName();
                label.alignment = TextAlignmentOptions.Center;
                label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
                label.transform.localScale *= 1.7f;

                var passiveButton = buttonTransform.GetComponent<PassiveButton>();
                passiveButton.OnClick.RemoveAllListeners();
                passiveButton.OnClick.AddListener((Action) (() =>
                {
                    if (_target == null)
                    {
                        return;
                    }

                    var target = _target!;
                    var playerData = target.GetPlayerData()!;

                    if (role.Id == playerData.Role.Id 
                        || playerData.SubRoles.Select(data => data.Id).Contains(role.Id))
                    {
                        Shoot();
                        
                        Exit(guesserUI);
                    }
                    else
                    {
                        PlayerControl.LocalPlayer.RpcKillPlayerCompletely(PlayerControl.LocalPlayer);
                        Buttons.ForEach(guessButton => guessButton.Destroy());
                        Exit(guesserUI);
                    }
                    
                    if (!Guesser.GuessContinuously.GetBool() || Guesser.GuessedTime >= Guesser.MaxGuessTime.GetInt())
                    {
                        Buttons.ForEach(guessButton => guessButton.Destroy());
                        Buttons.Clear();
                    }
                }));
            }
        }));
    }

    private void Exit(PhoneUI guesserUI)
    {
        MeetingHud.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
        guesserUI.Destroy();
    }

    private void Shoot()
    {
        if (_target == null)
        {
            return;
        }

        var target = _target!;
        PlayerControl.LocalPlayer.RpcKillPlayerCompletely(target);
        Guesser.GuessedTime ++;
    }

    private CustomRole[] GetCustomRolesFromPlayers()
    {
        var canGuessSubRoles = Guesser.CanGuessSubRoles.GetBool();
        var players = PlayerUtils.GetAllPlayers();

        var customRoles = players.Select(player => player.GetMainRole()).ToList();
        if (canGuessSubRoles)
            players.Select(player => player.GetSubRoles()).ForEach(roles =>
            {
                if (!roles.Any()) return;
                foreach (var customRole in roles)
                {
                    if (!customRoles.Select(role => role.Id).Contains(customRole.Id))
                    {
                        customRoles.Add(customRole);
                    }
                }
            });

        return customRoles.ToArray();
    }
}