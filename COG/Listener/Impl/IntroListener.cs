using AmongUs.GameOptions;
using COG.Listener.Event.Impl.ICutscene;
using COG.Role;
using COG.Utils;
using System.Text;
using UnityEngine;

namespace COG.Listener.Impl;

public class IntroListener : IListener
{
    [EventHandler(EventHandlerType.Postfix)]
    public void OnSetUpRoleText(IntroCutsceneShowRoleEvent @event)
    {
        var intro = @event.IntroCutscene!;
        Main.Logger.LogInfo("Set up role text for the player...");

        var myRole = GameUtils.GetLocalPlayerRole();

        SetUpRoles();

        void SetUpRoles()
        {
            if (GameOptionsManager.Instance.currentGameMode != GameModes.Normal) return;
            intro.RoleText.text = myRole.Name;
            intro.RoleText.color = myRole.Color;

            var sb = new StringBuilder(myRole.GetColorName());
            foreach (var sub in PlayerControl.LocalPlayer.GetSubRoles())
                sb.Append(" + ").Append(sub.GetColorName());

            intro.YouAreText.text = sb.ToString();
            intro.YouAreText.color = myRole.Color;
            intro.RoleBlurbText.color = myRole.Color;
            intro.RoleBlurbText.text = myRole.ShortDescription;

            intro.YouAreText.gameObject.SetActive(true);
            intro.RoleText.gameObject.SetActive(true);
            intro.RoleBlurbText.gameObject.SetActive(true);

            SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.Data.Role.IntroSound, false);

            if (intro.ourCrewmate == null)
            {
                intro.ourCrewmate = intro.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, false);
                intro.ourCrewmate.gameObject.SetActive(false);
            }

            intro.ourCrewmate.gameObject.SetActive(true);
            var transform = intro.ourCrewmate.transform;
            transform.localPosition = new Vector3(0f, -1.05f, -18f);
            transform.localScale = new Vector3(1f, 1f, 1f);
            intro.ourCrewmate.ToggleName(false);
        }
    }

    [EventHandler(EventHandlerType.Prefix)]
    public void OnSetUpTeamText(IntroCutsceneBeginCrewmateEvent @event)
    {
        var role = GameUtils.GetLocalPlayerRole();
        var player = PlayerControl.LocalPlayer;

        var camp = role.CampType;
        if (camp is not (CampType.Neutral or CampType.Unknown)) return;
        var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
        soloTeam.Add(player);
        @event.SetTeamToDisplay(soloTeam);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void AfterSetUpTeamText(IntroCutsceneBeginCrewmateEvent @event)
    {
        var intro = @event.IntroCutscene;
        var role = GameUtils.GetLocalPlayerRole();

        var camp = role.CampType;

        intro.BackgroundBar.material.color = camp.GetColor();
        intro.TeamTitle.text = camp.GetName();
        intro.TeamTitle.color = camp.GetColor();

        intro.ImpostorText.text = camp.GetDescription();
    }
}