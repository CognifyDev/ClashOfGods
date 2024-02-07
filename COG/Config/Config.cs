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
        LoadConfig();
    }

    protected Config(string name, string path, ResourceFile resourceFile)
    {
        Name = name;
        Path = path;
        Text = resourceFile.GetResourcesText();
        LoadConfig();
    }

    public Config(string name, string path)
    {
        Name = name;
        Path = path;
        Text = File.ReadAllText(Path, Encoding.UTF8);
        YamlReader = Yaml.LoadFromString(Text);
    }

    public static List<Config> Configs { get; } = new();
    public string Name { get; }
    public string Path { get; }
    public string Text { get; }
    public Yaml? YamlReader { get; private set; }

    public void LoadConfig(bool replace = false)
    {
        if (!Directory.Exists(DataDirectoryName)) Directory.CreateDirectory(DataDirectoryName);
        if (!File.Exists(Path) || replace)
        {
            if (replace && File.Exists(Path)) File.Delete(Path);
            File.WriteAllText(Path, Text, Encoding.Unicode);
        }

        var text = File.ReadAllText(Path, Encoding.Unicode);
        YamlReader = Yaml.LoadFromString(text);
    }
}