using Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace Client.Services;

public interface IConfigService
{
    /// <summary>
    /// 获取当前配置
    /// </summary>
    AppConfig GetConfig();

    /// <summary>
    /// 更新配置
    /// </summary>
    Task UpdateConfigAsync(AppConfig config);

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    Task SaveConfigAsync();

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    Task LoadConfigAsync();

    /// <summary>
    /// 重置配置为默认值
    /// </summary>
    Task ResetConfigAsync();
    
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