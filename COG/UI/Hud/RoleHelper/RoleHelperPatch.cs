using System.Collections.Generic;
using System.Linq;
using COG.Listener;
using COG.Listener.Attribute;
using COG.Listener.Event.Impl.Game;
using COG.Role;
using COG.Utils;
using Reactor.Networking.Rpc;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Input = UnityEngine.Windows.Input;

namespace COG.UI.Hud.RoleHelper;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public class RoleHelperPatch
{
    public static string Shower = "";
    public static CustomRole _role;
    public static List<CustomRole> _subrole;

    public static bool _isOpen;

    [EventHandler(EventHandlerType.Postfix)]
    [OnlyLocalPlayerWithThisRoleInvokable]
    public void OnGameStart(GameStartEvent _)
    {
        _role = new CustomRole();
        _subrole = new List<CustomRole>();
        _role = PlayerControl.LocalPlayer.GetMainRole();
        _subrole = PlayerControl.LocalPlayer.GetSubRoles().ToList();
        MainRoleSet();
        SubRoleSet();
    }
    public void MainRoleSet()
    {
        var info = new RoleHelpersBase(_role.Name, _role.ShortDescription, _role.GetLongDescription(),
            SecondTextType.None);
        Shower +=
            $"<color={_role.Color.ToColorHexString()}><size=200%{info.Title}</size>\n<b><size=80%>{info.SubTtile}</size></b></color>\n{_role.GetLongDescription()}";
    }

    public void SubRoleSet()
    {
        foreach (var subrole in _subrole)
        {
            var info = new RoleHelpersBase(subrole.Name, subrole.ShortDescription, subrole.GetLongDescription(),
                SecondTextType.None);
            Shower +=
                $"<color={subrole.Color.ToColorHexString()}><size=200%{info.Title}</size>\n<b><size=80%>{info.SubTtile}</size></b></color>\n{_role.GetLongDescription()}";
        }
    }
    public static void Postfix(HudManager hud)
    {
        if (GameStates.InRealGame)
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F1))
            {
                GameUtils.Popup.Show(Shower);
            }
        }
    }
}