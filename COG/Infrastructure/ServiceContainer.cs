using System;
using System.Collections.Generic;

namespace COG.Infrastructure;

/// <summary>
/// 轻量级依赖注入容器
/// </summary>
public sealed class ServiceContainer
{
    private readonly Dictionary<Type, object> _services = new();
    private readonly Dictionary<Type, Func<object>> _factories = new();

    /// <summary>
    /// 注册单例服务
    /// </summary>
    public void RegisterSingleton<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    /// <summary>
    /// 注册工厂（每次获取时创建新实例）
    /// </summary>
    public void RegisterTransient<T>(Func<T> factory) where T : class
    {
        _factories[typeof(T)] = () => factory();
    }

    /// <summary>
    /// 注册单例工厂（首次获取时创建，之后复用）
    /// </summary>
    public void RegisterLazySingleton<T>(Func<T> factory) where T : class
    {
        _factories[typeof(T)] = () =>
        {
            if (!_services.ContainsKey(typeof(T)))
                _services[typeof(T)] = factory();
            return _services[typeof(T)];
        };
    }

    /// <summary>
    /// 获取服务实例
    /// </summary>
    public T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
            return (T)service;

        if (_factories.TryGetValue(typeof(T), out var factory))
            return (T)factory();

        throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");
    }

    /// <summary>
    /// 尝试获取服务实例
    /// </summary>
    public bool TryGet<T>(out T? service) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var obj))
        {
            service = (T)obj;
            return true;
        }

        if (_factories.TryGetValue(typeof(T), out var factory))
        {
            service = (T)factory();
            return true;
        }

        service = null;
        return false;
    }

    /// <summary>
    /// 检查服务是否已注册
    /// </summary>
    public bool IsRegistered<T>() where T : class
    {
        return _services.ContainsKey(typeof(T)) || _factories.ContainsKey(typeof(T));
    }

    /// <summary>
    /// 清除所有注册
    /// </summary>
    public void Clear()
    {
        _services.Clear();
        _factories.Clear();
    }
}
