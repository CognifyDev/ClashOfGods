using System;
using System.Collections.Generic;
using System.Linq;
using COG.Constant;
using COG.Role;
using COG.Role.Impl.SubRole;
using COG.Utils;
using UnityEngine;

namespace COG.UI.CustomGameObject.Meeting;

public class GuessButton : TemplatedCustomGameObject
{
    private const string GuessButtonName = "GuessButton";
    private static readonly Vector3 LocalPosition = new(-0.95f, 0.03f, -1.3f);
    
    private Guesser Guesser { get; }
    
    public MeetingHud MeetingHud { get; }
    
    /// <summary>
    /// 赌怪按钮
    /// </summary>
    public GuessButton(MeetingHud hud, GameObject template, PlayerVoteArea playerVoteArea, Guesser guesser) : base(template, playerVoteArea.transform)
    {
        Guesser = guesser;
        MeetingHud = hud;
        GameObject.name = GuessButtonName;
        GameObject.transform.localPosition = LocalPosition;
        var renderer = GameObject.GetComponent<SpriteRenderer>();
        renderer.sprite = ResourceUtils.LoadSprite(ResourcesConstant.GuessButton, 150F);
        
    }

    public void SetupListeners()
    {
        var button = GameObject.GetComponent<PassiveButton>();
        button.StopAllCoroutines();
        button.OnClick.RemoveAllListeners();
        button.OnClick.AddListener((System.Action)(() =>
        {
            var buttonTemplate = MeetingHud.playerStates[0].transform.FindChild("votePlayerBase");
            var maskTemplate = MeetingHud.playerStates[0].transform.FindChild("MaskArea");
            var smallButtonTemplate = MeetingHud.playerStates[0].Buttons.transform.Find("CancelButton");
            var textTemplate = MeetingHud.playerStates[0].NameText;
            
            MeetingHud.playerStates.ForEach(x => x.gameObject.SetActive(false));
            var ui = new PanelUI(MeetingHud, new Vector3(0, 0, -5f));
            
            ui.AddButton(buttonTemplate.transform, smallButtonTemplate.GetComponent<SpriteRenderer>().sprite,
                new Vector3(2.725f, 2.1f, -5), new Vector3(0.217f, 0.9f, 1), () =>
                {
                    MeetingHud.playerStates.ForEach(x => x.gameObject.SetActive(true));
                    Object.Destroy(ui.Container.gameObject);
                }, maskTemplate);

            var enabledRolesOnly = Guesser.EnabledRolesOnly.GetBool();
            var canGuessSubRoles = Guesser.CanGuessSubRoles.GetBool();
            
            var roles = enabledRolesOnly
                ? GetCustomRolesFromPlayers()
                : CustomRoleManager.GetManager().GetRoles().Where(role =>
                        (canGuessSubRoles && !role.IsSubRole) || !canGuessSubRoles)
                    .ToArray();
            
            

            ui.Container.transform.localScale *= 0.75f;
        }));
    }

    private CustomRole[] GetCustomRolesFromPlayers()
    {
        var canGuessSubRoles = Guesser.CanGuessSubRoles.GetBool();
        var players = PlayerUtils.GetAllPlayers();
        
        var customRoles = players.Select(player => player.GetMainRole()).ToList();
        if (canGuessSubRoles)
        {
            players.Select(player => player.GetSubRoles()).ForEach(roles =>
            {
                if (roles.Any())
                {
                    customRoles.AddRange(roles);
                }
            });
        }

        return customRoles.ToArray();
    }
}