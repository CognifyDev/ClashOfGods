using System;
using COG;
using COG.Constant;
using COG.Listener;
using COG.Listener.Event;
using COG.Listener.Event.Impl.Game;
using COG.Plugin;
using COG.Plugin.CSharp;
using COG.Role;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.Utils;
using UnityEngine;

public sealed class MedicRole : CustomRole
{
    private static MedicRole _instance;
    private CustomButton _shieldButton;
    private bool _shieldUsed;

    public MedicRole() : base(new Color(0.2f, 0.8f, 0.4f), CampType.Crewmate, true)
    {
        _instance = this;
        CanKill = false;
        Name = PluginContext.Current.GetString("medic.name");
        ShortDescription = PluginContext.Current.GetString("medic.short-desc");
    }

    public static MedicRole Instance => _instance;

    public override IListener GetListener()
    {
        var listener = base.GetListener();
        ListenerManager.GetManager().RegisterHandlers(new Handler[]
        {
            new(listener, GetType().GetMethod(nameof(OnGameStart), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public),
                EventHandlerType.Postfix)
        });
        return listener;
    }

    public static void OnGameStart(GameStartEvent _)
    {
        if (Instance == null || !Instance.IsLocalPlayerRole()) return;

        var sprite = PluginContext.Current.GetSprite("shield.png")
                     ?? ResourceUtils.LoadSpriteFromResources(ResourceConstant.GeneralKillButton);

        var buttonBuilder = CustomButton.Builder(
            "medic-shield",
            sprite ?? ResourceUtils.LoadSpriteFromResources(ResourceConstant.GeneralKillButton),
            PluginContext.Current.GetString("medic.shield-button")
        );

        Instance._shieldButton = buttonBuilder
            .Cooldown(() => Instance._shieldUsed ? float.MaxValue : 15f)
            .OnClick(() =>
            {
                if (Instance._shieldUsed) return;
                Instance._shieldUsed = true;

                var target = PlayerControl.LocalPlayer;
                GameUtils.SendGameMessage(
                    PluginContext.Current.GetString("medic.shield-applied")
                );

                Main.Logger.LogInfo($"[Medic] Shield applied by {target.Data.PlayerName}");
            })
            .Position(new Vector3(-2f, 0f))
            .Build();
    }

}

public static class MedicPluginInit
{
    [PluginModuleInitializer]
    public static void Initialize()
    {
        Main.Logger.LogInfo("[MedicPlugin] Registering Medic role...");

        var medic = new MedicRole();
        CustomRoleManager.GetManager().RegisterRole(medic);

        Main.Logger.LogInfo("[MedicPlugin] Medic role registered successfully!");
    }
}
