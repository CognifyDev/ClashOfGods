using Il2CppSystem.IO;
using Il2CppSystem.Text.Json;

namespace COG.Utils;

public class Language
{
    private static bool _inited = false;
    private static readonly string Path = "./" + ConfigManager.GetDataDirectoryName() + "/lang.json";

    /// <summary>
    /// 初始化语言
    /// </summary>
    public static void Init()
    {
        if (_inited) return;

        if (!File.Exists(Path))
        {
            File.WriteAllText(Path, ConfigManager.GetResourcesString("COG.Resources.InDLL.lang.json"));
        }


    }

    /// <summary>
    /// 获取语言文件中的语言内容
    /// </summary>
    /// <param name="path">语言索引</param>
    /// <param name="language">语言</param>
    /// <returns></returns>
    public static string GetLang(string path, SupportedLanguage language)
    {
        string jsonContent = File.ReadAllText(Path);
        JsonDocument document = JsonDocument.Parse(jsonContent);

        if (document.RootElement.TryGetProperty(language.ToString(), out JsonElement langElement))
        {
            if (langElement.TryGetProperty(path, out JsonElement typeElement))
            {
                if (typeElement.ValueKind == JsonValueKind.String)
                {
                    return typeElement.GetString();
                }
            }
        }

        return null;
    }

}

/// <summary>
/// 支持的语言
/// </summary>
public enum SupportedLanguage
{
    SimplifiedChinese, English
}