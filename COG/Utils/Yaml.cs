using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace COG.Utils;

public class Yaml
{
    private Yaml(string text)
    {
        Text = text;

        var input = new StringReader(Text);
        var yamlStream = new YamlStream();
        yamlStream.Load(input);

        YamlStream = yamlStream;
    }

    /// <summary>
    ///     Yaml内容
    /// </summary>
    public string Text { get; private set; }

    /// <summary>
    ///     Yaml Stream
    /// </summary>
    public YamlStream YamlStream { get; }

    /// <summary>
    ///     获取Int值
    /// </summary>
    /// <param name="location">路径，例如: GetInt("abab.sb"),"."表示下级</param>
    /// <returns>Int值</returns>
    public int? GetInt(string location)
    {
        var str = GetString(location);
        if (str == null) return null;
        if (int.TryParse(str, out var result)) return result;

        return null;
    }

    public List<string>? GetStringList(string location)
    {
        var locations = location.Contains('.') ? location.Split(".") : new[] { location };
        var rootNode = (YamlMappingNode)YamlStream.Documents[0].RootNode;

        if (locations.Length < 1) return new List<string>();

        YamlNode? valueNode = rootNode;
        foreach (var loc in locations)
            try
            {
                if (valueNode is YamlMappingNode mappingNode)
                {
                    var keyNode = new YamlScalarNode(loc);
                    if (mappingNode.Children.TryGetValue(keyNode, out valueNode))
                        // 继续向下查找
                        continue;
                }

                // 如果找不到对应的键或节点不是一个映射节点，则返回空列表
                return null;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }

        // 如果值节点是一个列表节点，则将列表中的值添加到结果列表中
        if (valueNode is YamlSequenceNode sequenceNode)
        {
            var result = new List<string>();
            foreach (var item in sequenceNode.Children)
                if (item is YamlScalarNode { Value: not null } scalarNode)
                    result.Add(scalarNode.Value);
            return result;
        }

        // 如果值节点不是列表节点，则返回空列表
        return new List<string>();
    }

    public byte? GetByte(string location)
    {
        var str = GetString(location);
        if (str == null) return null;
        if (byte.TryParse(str, out var result)) return result;

        return null;
    }

    public string? GetString(string location)
    {
        var locations = location.Contains('.') ? location.Split(".") : new[] { location };
        var rootNode = (YamlMappingNode)YamlStream.Documents[0].RootNode;

        if (locations.Length < 1) return null;

        YamlNode? valueNode = rootNode;
        foreach (var loc in locations)
            try
            {
                if (valueNode is YamlMappingNode mappingNode)
                {
                    var keyNode = new YamlScalarNode(loc);
                    if (mappingNode.Children.TryGetValue(keyNode, out valueNode))
                        // 继续向下查找
                        continue;
                }

                // 如果找不到对应的键或节点不是一个映射节点，则返回 null
                return null;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }

        return $"{valueNode}";
    }

    public void Set(string location, dynamic obj)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var yamlObject = deserializer.Deserialize<dynamic>(Text);
        SetValue(yamlObject, location, obj);

        Text = serializer.Serialize(yamlObject);
    }

    private static void SetValue(dynamic yamlObject, string location, object value)
    {
        var parts = location.Split('.');

        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (!yamlObject.ContainsKey(parts[i]))
                yamlObject[parts[i]] = new Dictionary<string, object>();

            yamlObject = yamlObject[parts[i]];
        }

        yamlObject[parts[^1]] = value;
    }

    public static Yaml LoadFromString(string text)
    {
        return new Yaml(text);
    }

    public static Yaml LoadFromFile(string path)
    {
        return LoadFromString(File.ReadAllText(path));
    }

    public void WriteTo(string path, bool replace = true, Encoding? encoding = null)
    {
        if (replace && File.Exists(path)) File.Delete(path);

        if (!File.Exists(path)) File.WriteAllText(path, Text, encoding ?? Encoding.UTF8);
    }

    public static Yaml NewEmptyYaml()
    {
        return LoadFromString("");
    }
}