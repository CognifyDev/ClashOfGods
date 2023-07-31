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
        public void OnHudStart(HudManager hud) => CustomButton.Init(hud);

        public void OnHudUpdate(HudManager manager)
        {
            foreach (var button in CustomButtonManager.GetManager().GetButtons())
            {
                button.Update();
            }
        }
    }
}
