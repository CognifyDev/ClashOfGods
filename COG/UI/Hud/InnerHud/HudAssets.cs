using COG.Utils;
using UnityEngine;

namespace COG.UI.Hud.InnerHud
{
    public static class HudAssets
    {
        public static Sprite BackgroundInner {  get; set; }
        public static Sprite BackgroundOuter { get; set; }
        public static Sprite CancelButton {  get; set; }
        public static Sprite MarkButton { get; set; }
        public static Sprite Highlight { get; set; }

        public static void LoadUiSprites()
        {
            var loader = DividedSpriteLoader.FromResource("", 100f, 2, 1, false);

            Sprite firstElement = loader.GetSprite(0);
            Sprite secondElement = loader.GetSprite(1);
        }
    }
}
