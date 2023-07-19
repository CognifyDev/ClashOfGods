using System.Collections.Generic;
using UnityEngine;

namespace COG.Listener.Impl
{
    class ModOptionListener : IListener
    {
        public static List<Transform> Vanilla = new();
        public bool Inited = false;
        public void OnSettingInit(OptionsMenuBehaviour menu)
        {
            Vector3? position = menu.CensorChatButton.transform.localPosition;
            Transform transform = menu.CensorChatButton.transform;
            ToggleButtonBehaviour button = menu.EnableFriendInvitesButton;

            var modOptions = Object.Instantiate(menu.CensorChatButton, menu.CensorChatButton.transform.parent);
            
            //设置原版按钮的大小/位置
            menu.CensorChatButton.Text.transform.localScale = new Vector3(1 / 0.66f, 1, 1);
            transform.localPosition = position.Value + Vector3.left * 0.45f;
            transform.localScale = new Vector3(0.66f, 1, 1);

            button.transform.localScale = new Vector3(0.66f, 1, 1);
            button.transform.localPosition += Vector3.right * 0.5f;
            button.Text.transform.localScale = new Vector3(1.2f, 1, 1);

            //设置模组选项按钮
            modOptions.gameObject.SetActive(true);
            modOptions.Text.text = "COG Options";
            modOptions.transform.localPosition = position.Value + Vector3.right * 4f / 3f;
            modOptions.transform.localScale = new Vector3(0.66f, 1, 1);
            modOptions.Text.transform.localScale = new Vector3(1 / 0.66f, 1, 1);
            modOptions.Background.color = Palette.EnabledColor;

            Holders.ModOptionHolder.Init();
            LoadButtons(menu);
            
            var modOptionsButton = modOptions.GetComponent<PassiveButton>();
            modOptionsButton.OnClick = new();
            modOptionsButton.OnClick.AddListener((System.Action)(() =>
            {
                HideVanillaButtons(menu);
                foreach (var btn in ModOption.buttons) btn.ToggleButton.gameObject.SetActive(true);
            }));
        }



        void HideVanillaButtons(OptionsMenuBehaviour menu)
        {
            Vanilla.Clear();
            for (int i = 0; i < menu.transform.childCount; i++)
            {
                var child = menu.transform.GetChild(i);
                if (child.name == "Background" || 
                    child.name == "CloseButton" || 
                    child.name == "Tint" || 
                    child.name == "TabButtons" || 
                    child.name == "GeneralButton" || 
                    child.name == "GraphicsButton" ||
                    !child.gameObject.active) continue;
                Vanilla.Add(child);
                child.gameObject.SetActive(false);
            }
        }

        void LoadButtons(OptionsMenuBehaviour menu)
        {
            int a = 0;
            foreach(var btn in ModOption.buttons)
            {
                CreateButton(menu, a, btn);
                a++;
            }
            Inited = true;
        }
        
        /// <summary>
        /// 创建一个按钮
        /// </summary>
        /// <param name="menu">OptionMenuBehaviour 的实例</param>
        /// <param name="idx">（从0开始）加入按钮的序号</param>
        /// <param name="option">对应的 ModOption</param>
        void CreateButton(OptionsMenuBehaviour menu, int idx, ModOption option)
        {
            if (option.Inited) return;
            var template = menu.CensorChatButton;
            var button = Object.Instantiate(template, menu.transform);
            Vector3 pos = new(idx % 2 == 0 ? -1.17f : 1.17f, 1.7f - idx / 2 * 0.8f, -0.5f);

            button.transform.localPosition = pos;
            button.onState = option.DefaultValue;
            button.Background.color = button.onState ? Palette.AcceptedGreen : Palette.EnabledColor;
            button.Text.text = option.Text;
            button.name = option.Text.Replace(" ", "");
            button.gameObject.SetActive(true);

            var passive = button.GetComponent<PassiveButton>();
            passive.OnClick = new();
            passive.OnMouseOut = new();
            passive.OnMouseOver = new();

            passive.OnMouseOut.AddListener((System.Action)(() => button.Background.color = button.onState ? Palette.AcceptedGreen : Palette.EnabledColor));
            passive.OnMouseOver.AddListener((System.Action)(() => { if (!button.onState) button.Background.color = Palette.AcceptedGreen; }));
            passive.OnClick.AddListener((System.Action)(() =>
            {
                button.onState = option.OnClick();
                button.Background.color = button.onState ? Palette.AcceptedGreen : Palette.EnabledColor;
            }));

            option.ToggleButton = button;

            button.gameObject.SetActive(false);
            option.Inited = true;
        }
    }




    class ModOption
    {
        public string Text;
        public System.Func<bool> OnClick;
        public bool DefaultValue;
        public bool Inited = false;
        public ToggleButtonBehaviour ToggleButton;

        public static List<ModOption> buttons = new();

        public ModOption(string text, System.Func<bool> onClick, bool defaultValue)
        {
            this.Text = text;
            this.OnClick = onClick;
            this.DefaultValue = defaultValue;
            buttons.Add(this);
            Main.Logger.LogInfo("Mod Option " + text + " loaded");
        }
    }
}
