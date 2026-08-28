using System;
using System.IO;
using System.Text;
using COG.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace COG.Infrastructure;

/// <summary>
/// 持久化配置管理器，提供配置的加载、保存、删除功能
/// </summary>
public class ConfigManager
{
    private static ConfigManager? _instance;
    public static ConfigManager Instance => _instance ??= new ConfigManager();

    private readonly string _basePath;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// 配置变更事件，参数为配置文件名
    /// </summary>
    public event Action<string>? OnConfigChanged;

    private ConfigManager()
    {
        _basePath = Path.Combine(ConfigBase.BasePath, "configs");
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);

        _serializer = new SerializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// 加载配置文件，如果不存在则创建默认配置并保存
    /// </summary>
    public T Load<T>(string name) where T : class, IConfig, new()
    {
        var path = GetConfigPath(name);

        if (!File.Exists(path))
        {
            var config = new T();
            Save(name, config);
            return config;
        }

        try
        {
            var yaml = File.ReadAllText(path, Encoding.UTF8);
            var config = _deserializer.Deserialize<T>(yaml);

            // 版本迁移
            if (config != null)
            {
                var currentVersion = config.Version;
                var savedVersion = GetSavedVersion(path);
                if (savedVersion != null && savedVersion != currentVersion)
                {
                    config.Migrate(savedVersion);
                    // 迁移后立即保存
                    Save(name, config);
                }
            }

            return config ?? new T();
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"Failed to load config '{name}': {ex.Message}");
            var fallback = new T();
            Save(name, fallback);
            return fallback;
        }
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    public void Save<T>(string name, T config) where T : class, IConfig
    {
        var path = GetConfigPath(name);

        try
        {
            var yaml = _serializer.Serialize(config);
            File.WriteAllText(path, yaml, Encoding.UTF8);
            OnConfigChanged?.Invoke(name);
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"Failed to save config '{name}': {ex.Message}");
        }
    }

    /// <summary>
    /// 删除配置文件
    /// </summary>
    public bool Delete(string name)
    {
        var path = GetConfigPath(name);
        if (!File.Exists(path)) return false;

        try
        {
            File.Delete(path);
            return true;
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"Failed to delete config '{name}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 检查配置是否存在
    /// </summary>
    public bool Exists(string name)
    {
        return File.Exists(GetConfigPath(name));
    }

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    private string GetConfigPath(string name)
    {
        return Path.Combine(_basePath, $"{name}.yml");
    }

    /// <summary>
    /// 从YAML文件中读取版本号（不完整反序列化，避免反序列化失败导致版本迁移失败）
    /// </summary>
    private string? GetSavedVersion(string path)
    {
        try
        {
            var yaml = File.ReadAllText(path, Encoding.UTF8);
            var lines = yaml.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.TrimStart();
                if (trimmed.StartsWith("Version:") || trimmed.StartsWith("version:"))
                {
                    var value = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim();
                    return value?.Trim('"', '\'');
                }
            }
        }
        catch
        {
            // ignored - 版本读取失败时跳过迁移
        }

        return null;
    }
}
