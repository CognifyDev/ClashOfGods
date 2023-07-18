using COG.Utils;

namespace COG.Config.Impl;

public class LanguageConfig : Config
{
    public static LanguageConfig Instance { get; }
    public string MessageForNextPage { get; private set; }
    public string MakePublicMessage { get; private set; }
   
    public LanguageConfig() : base(
        "Language", 
        DataDirectoryName + "/language.yml",
        new ResourceFile("COG.Resources.InDLL.Config.language.yml")
        )
    {
        MessageForNextPage = YamlReader.GetString("lobby.message-for-next-page")!;
        MakePublicMessage = YamlReader.GetString("lobby.make-public-message")!;
    }

    static LanguageConfig()
    {

        Instance = new LanguageConfig();
    }
}