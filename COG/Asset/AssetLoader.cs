using System.Reflection;
using UnityEngine;
using COG.Utils;

namespace COG.Asset
{
    public static class AssetLoader
    {
        private static readonly Assembly dll = Assembly.GetExecutingAssembly();
        private static bool flag = false;
        static internal AssetBundle AssetBundle { get; private set; } = null!;

        public static void LoadAssets()
        {
            if (flag) return;
            flag = true;
            LoadAudioAssets();
            LoadSpriteAssets();
            LoadShaderAssets();
            var resourceStream = dll.GetManifestResourceStream("");
            AssetBundle = AssetBundle.LoadFromMemory(resourceStream!.ReadFully());
        }

        private static void LoadAudioAssets()
        {
            var resourceAudioAssetBundleStream = dll.GetManifestResourceStream("");
            var assetBundleBundle = AssetBundle.LoadFromMemory(resourceAudioAssetBundleStream.ReadFully());
        }

        private static void LoadSpriteAssets()
        {
            var resourceTestAssetBundleStream = dll.GetManifestResourceStream("");
            var assetBundleBundle = AssetBundle.LoadFromMemory(resourceTestAssetBundleStream.ReadFully());
        }

        private static void LoadShaderAssets()
        {
            var resourceTestAssetBundleStream = dll.GetManifestResourceStream("");
            var assetBundleBundle = AssetBundle.LoadFromMemory(resourceTestAssetBundleStream.ReadFully());
        }
    }
}
