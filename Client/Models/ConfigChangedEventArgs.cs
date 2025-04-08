using System;

namespace Client.Models;

/// <summary>
/// 配置变更事件参数
/// </summary>
public class ConfigChangedEventArgs : EventArgs
{
    /// <summary>
    /// 变更的属性名称
    /// </summary>
    public string PropertyName { get; }
    
    /// <summary>
    /// 旧值
    /// </summary>
    public object? OldValue { get; }
    
    /// <summary>
    /// 新值
    /// </summary>
    public object? NewValue { get; }
    
    /// <summary>
    /// 初始化配置变更事件参数
    /// </summary>
    public ConfigChangedEventArgs(string propertyName, object? oldValue, object? newValue)
    {
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }
} 