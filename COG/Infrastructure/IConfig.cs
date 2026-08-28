namespace COG.Infrastructure;

/// <summary>
/// 配置接口，所有需要版本管理的配置应实现此接口
/// </summary>
public interface IConfig
{
    /// <summary>
    /// 配置名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 配置版本号
    /// </summary>
    string Version { get; }

    /// <summary>
    /// 版本迁移逻辑，处理旧版本配置到新版本的转换
    /// </summary>
    /// <param name="fromVersion">源版本号</param>
    void Migrate(string fromVersion);
}
