using COG.Utils;

namespace COG.Config.Impl;

public class LanguageConfig : Config
{
    public static string MessageForNextPage { get; private set; } = null!;

    public LanguageConfig() : base(
        "Language", 
        DataDirectoryName + "/language.yml",
        new ResourceFile("COG.Resources.InDLL.Config.language.yml")
        )
    {
        MessageForNextPage = YamlReader.GetString("message-for-next-page")!;
    }

    static LanguageConfig()
    {
        new LanguageConfig();
    }
}