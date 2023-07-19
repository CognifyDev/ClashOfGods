using System.Collections.Generic;
using COG.Config.Impl;
using COG.Exception;
using COG.Role;
using COG.States;
using COG.Utils;

namespace COG.Listener.Impl;

public class GameListener : IListener
{
    private static readonly List<IListener> RegisteredListeners = new();

    public void OnCoBegin()
    {
        GameStates.InGame = true;
    }

    public void OnSelectRoles()
    {
        RegisteredListeners.Clear();
        GameUtils.Data.Clear();
        var players = PlayerUtils.GetAllPlayers().Disarrange();
        var maxImpostors = GameUtils.GetGameOptions().NumImpostors;

        var getter = Role.RoleManager.GetManager().NewGetter();
        foreach (var player in players)
        {
            if (!getter.HasNext()) break;

            if (maxImpostors > 0)
            {
                maxImpostors --;

                Role.Role? impostorRole;
                try
                {
                    impostorRole = ((Role.RoleManager.RoleGetter)getter).GetNextTypeCampRole(CampType.Impostor);
                }
                catch (GetterCanNotGetException)
                {
                    impostorRole = Role.RoleManager.GetManager().GetTypeCampRoles(CampType.Impostor)[0];
                }
                
                // 如果没有内鬼职业会出BUG
                RoleManager.Instance.SetRole(player, impostorRole!.BaseRoleType);
                GameUtils.Data.Add(player, impostorRole);
                RegisteredListeners.Add(impostorRole.GetListener(player));
                continue;
            }
            setRoles:
            var role = getter.GetNext() ?? Role.RoleManager.GetManager().GetTypeCampRoles(CampType.Crewmate)[0];
            if (role!.CampType == CampType.Impostor) goto setRoles; 
            RoleManager.Instance.SetRole(player, role.BaseRoleType);
            GameUtils.Data.Add(player, role);
            RegisteredListeners.Add(role.GetListener(player));
        }
        
        ListenerManager.GetManager().RegisterListeners(RegisteredListeners.ToArray());
    }

    public void OnGameEnd(AmongUsClient client, EndGameResult endGameResult)
    {
        GameStates.InGame = false;
        foreach (var registeredListener in RegisteredListeners)
        {
            ListenerManager.GetManager().UnregisterListener(registeredListener);
        }
    }

    public void OnGameStart(GameStartManager manager)
    {
        // 改变按钮颜色
        manager.MakePublicButton.color = Palette.DisabledClear;
        manager.privatePublicText.color = Palette.DisabledClear;
    }

    public bool OnMakePublic(GameStartManager manager)
    {
        GameUtils.SendGameMessage(LanguageConfig.Instance.MakePublicMessage);
        // 禁止设置为公开
        return false;
    }

    public void OnSetUpRoleText(IntroCutscene intro)
    {
        PlayerControl? player = null;
        Role.Role? role = null;

        foreach (var keyValuePair in GameUtils.Data)
        {
            var target = keyValuePair.Key;
            if (intro.PlayerPrefab.name.Equals(target.name))
            {
                player = target;
                role = keyValuePair.Value;
            }
        }
        
        if (role == null || player == null) return;
        
        // 游戏开始的时候显示角色信息
        intro.YouAreText.color = role.Color;
        intro.RoleText.text = role.Name;
        intro.RoleText.color = role.Color;
        intro.RoleBlurbText.color = role.Color;
        intro.RoleBlurbText.text = role.Description;
    }
    
    public void OnSetUpTeamText(IntroCutscene intro)
    {
        PlayerControl? player = null;
        Role.Role? role = null;

        foreach (var keyValuePair in GameUtils.Data)
        {
            var target = keyValuePair.Key;
            if (intro.PlayerPrefab.name.Equals(target.name))
            {
                player = target;
                role = keyValuePair.Value;
            }
        }
        
        if (role == null || player == null) return;
        
        // 游戏开始的时候显示陣營信息
        intro.TeamTitle.color = role.Color;
        if (role.CampType is CampType.Crewmate or CampType.Impostor) return;
        /*
         *  其他陣營文本預留
         */
    }

    public void OnGameEndSetEverythingUp(EndGameManager manager)
    {
        Role.Role? role = null;
        foreach (var keyValuePair in GameUtils.Data)
        {
            if (keyValuePair.Key.name.Equals(manager.PlayerPrefab.name))
            {
                role = keyValuePair.Value;
                break;
            }
        }
        if (role == null) return;
        manager.WinText.text = role.CampType.GetCampString() + " 胜利！";
        manager.WinText.color = role.Color;
    }
}