namespace COG.NewListener;

/// <summary>
/// 事件挂钩的类型
/// </summary>
public enum EventHandlerType
{
    /// <summary>
    /// 事件发生之前执行
    /// 可以设置取消
    /// </summary>
    Prefix,
    
    /// <summary>
    /// 事件发生之后执行
    /// 不可设置取消
    /// </summary>
    Postfix
}