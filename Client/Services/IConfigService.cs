using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.Results;
using Client.Models;

namespace Client.Services;

/// <summary>
/// 配置服务接口
/// </summary>
public interface IConfigService : IDisposable
{
    /// <summary>
    /// 配置变更事件
    /// </summary>
    event EventHandler<ConfigChangedEventArgs>? ConfigChanged;

    /// <summary>
    /// 获取当前配置
    /// </summary>
    AppConfig GetConfig();

    /// <summary>
    /// 获取指定层级的配置
    /// </summary>
    /// <param name="level">配置层级</param>
    AppConfig GetConfig(ConfigLevel level);

    /// <summary>
    /// 更新配置
    /// </summary>
    Task UpdateConfigAsync(AppConfig config);

    /// <summary>
    /// 更新指定层级的配置
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="level">配置层级</param>
    Task UpdateConfigAsync(AppConfig config, ConfigLevel level);

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    Task SaveConfigAsync();

    /// <summary>
    /// 保存指定层级的配置
    /// </summary>
    /// <param name="level">配置层级</param>
    Task SaveConfigAsync(ConfigLevel level);

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    Task LoadConfigAsync();

    /// <summary>
    /// 加载指定层级的配置
    /// </summary>
    /// <param name="level">配置层级</param>
    Task LoadConfigAsync(ConfigLevel level);

    /// <summary>
    /// 重置配置为默认值
    /// </summary>
    Task ResetConfigAsync();

    /// <summary>
    /// 重置指定层级的配置为默认值
    /// </summary>
    /// <param name="level">配置层级</param>
    Task ResetConfigAsync(ConfigLevel level);

    /// <summary>
    /// 重置配置的指定部分为默认值
    /// </summary>
    /// <param name="section">要重置的配置部分</param>
    Task ResetConfigSectionAsync(ConfigSection section);

    /// <summary>
    /// 重置指定层级配置的指定部分为默认值
    /// </summary>
    /// <param name="section">要重置的配置部分</param>
    /// <param name="level">配置层级</param>
    Task ResetConfigSectionAsync(ConfigSection section, ConfigLevel level);

    /// <summary>
    /// 验证配置
    /// </summary>
    /// <param name="config">要验证的配置</param>
    /// <returns>验证结果</returns>
    Task<ValidationResult> ValidateConfigAsync(AppConfig config);
    
    /// <summary>
    /// 验证当前配置
    /// </summary>
    /// <returns>验证结果</returns>
    Task<ValidationResult> ValidateCurrentConfigAsync();
    
    /// <summary>
    /// 修复配置
    /// </summary>
    /// <param name="config">要修复的配置</param>
    /// <returns>修复后的配置</returns>
    Task<AppConfig> FixConfigAsync(AppConfig config);
}

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