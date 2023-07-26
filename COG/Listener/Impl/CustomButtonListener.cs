using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COG.UI.CustomButtons;
using UnityEngine;

namespace COG.Listener.Impl
{
    class CustomButtonListener : IListener
    {
        public void OnHudStart(HudManager hud)
        {
            CustomButtonManager.GetManager().GetButtons().Clear();
            CustomButton btn = new(() => { Main.Logger.LogInfo("Click"); }, () => { Main.Logger.LogInfo("MeetingEnd"); }, () => { Main.Logger.LogInfo("Effect"); }, () => { return true; }, () => { return true; }, hud.KillButton.graphic.sprite, CustomButton.ButtonPositions.lowerRowRight, KeyCode.Space, "Button", true, 15f, 3f, hud, 10);
            CustomButtonManager.GetManager().RegisterCustomButtons(new CustomButton[]
            {
                btn
            });
            CustomButton.Init(hud);
        }

        public void OnHudUpdate()
        {
            foreach (var button in CustomButtonManager.GetManager().GetButtons())
            {
                if (button == null) continue;
                button.Update();
            }
        }
    }
}
