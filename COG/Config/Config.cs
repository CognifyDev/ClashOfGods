using System.Collections.Generic;
using System.IO;
using System.Text;
using COG.Utils;

namespace COG.Config;

public class Config
{
    public const string DataDirectoryName = $"{Main.DisplayName}_DATA";

    public Config(string name, string path, string text)
    {
        Name = name;
        Path = path;
        Text = text;
        Configs.Add(this);

        LoadConfigs();
    }

    protected Config(string name, string path, ResourceFile resourceFile)
    {
        Name = name;
        Path = path;
        Text = resourceFile.GetResourcesText();
        Configs.Add(this);

        LoadConfigs();
    }

    public Config(string name, string path)
    {
        Name = name;
        Path = path;
        Text = "";
        Configs.Add(this);

        LoadConfigs();
    }

    public static List<Config> Configs { get; } = new();
    public string Name { get; }
    public string Path { get; }
    public string Text { get; }
    public Yaml? YamlReader { get; private set; }

    public void LoadConfigs(bool replace = false)
    {
        if (!Directory.Exists(DataDirectoryName)) Directory.CreateDirectory(DataDirectoryName);
        if (!File.Exists(Path) || replace)
        {
            if (replace && File.Exists(Path)) File.Delete(Path);
            File.WriteAllText(Path, Text, Encoding.UTF8);
        }

        var text = File.ReadAllText(Path, Encoding.UTF8);
        YamlReader = Yaml.LoadFromString(text);
    }
}