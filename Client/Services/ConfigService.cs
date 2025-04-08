using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Client.Models;
using Client.Models.Validation;
using FluentValidation;
using FluentValidation.Results;
using Serilog;

namespace Client.Services;

/// <summary>
/// 配置服务实现
/// </summary>
public class ConfigService : IConfigService
{
    private readonly string _configFilePath;
    private readonly IValidator<AppConfig> _validator;
    private AppConfig _currentConfig;
    private bool _isInitialized;

    /// <summary>
    /// 配置变更事件
    /// </summary>
    public event EventHandler<ConfigChangedEventArgs>? ConfigChanged;

    /// <summary>
    /// 初始化配置服务
    /// </summary>
    public ConfigService(string configFilePath, IValidator<AppConfig> validator)
    {
        _configFilePath = configFilePath;
        _validator = validator;
        _currentConfig = new AppConfig();
        _isInitialized = false;
    }

    /// <summary>
    /// 获取当前配置
    /// </summary>
    public AppConfig GetConfig()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("配置服务未初始化，请先调用LoadConfigAsync方法");
        }
        return _currentConfig;
    }

    /// <summary>
    /// 更新配置
    /// </summary>
    public async Task UpdateConfigAsync(AppConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var oldConfig = _currentConfig;
        _currentConfig = config;

        // 触发配置变更事件
        OnConfigChanged(oldConfig, config);

        await SaveConfigAsync();
    }

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    public async Task SaveConfigAsync()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(_currentConfig, options);
        await File.WriteAllTextAsync(_configFilePath, json);
    }

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    public async Task LoadConfigAsync()
    {
        if (!File.Exists(_configFilePath))
        {
            _currentConfig = new AppConfig();
            await SaveConfigAsync();
        }
        else
        {
            var json = await File.ReadAllTextAsync(_configFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var oldConfig = _currentConfig;
            _currentConfig = JsonSerializer.Deserialize<AppConfig>(json, options) ?? new AppConfig();

            // 触发配置变更事件
            OnConfigChanged(oldConfig, _currentConfig);
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 重置配置为默认值
    /// </summary>
    public async Task ResetConfigAsync()
    {
        var oldConfig = _currentConfig;
        _currentConfig = new AppConfig();

        // 触发配置变更事件
        OnConfigChanged(oldConfig, _currentConfig);

        await SaveConfigAsync();
    }

    /// <summary>
    /// 验证配置
    /// </summary>
    public async Task<ValidationResult> ValidateConfigAsync(AppConfig config)
    {
        return await _validator.ValidateAsync(config);
    }

    /// <summary>
    /// 验证当前配置
    /// </summary>
    public async Task<ValidationResult> ValidateCurrentConfigAsync()
    {
        return await ValidateConfigAsync(_currentConfig);
    }

    /// <summary>
    /// 修复配置
    /// </summary>
    public async Task<AppConfig> FixConfigAsync(AppConfig config)
    {
        var result = await ValidateConfigAsync(config);
        if (result.IsValid)
        {
            return config;
        }

        // 这里可以根据实际需求实现配置修复逻辑
        // 目前只是简单地返回一个新的默认配置
        return new AppConfig();
    }

    /// <summary>
    /// 触发配置变更事件
    /// </summary>
    private void OnConfigChanged(AppConfig oldConfig, AppConfig newConfig)
    {
        if (oldConfig == null || newConfig == null)
        {
            return;
        }

        // 比较并触发各个属性的变更事件
        if (oldConfig.WindowState != newConfig.WindowState)
        {
            OnPropertyChanged(nameof(AppConfig.WindowState), oldConfig.WindowState, newConfig.WindowState);
        }

        if (oldConfig.ApiSettings != newConfig.ApiSettings)
        {
            OnPropertyChanged(nameof(AppConfig.ApiSettings), oldConfig.ApiSettings, newConfig.ApiSettings);
        }

        if (oldConfig.UiSettings != newConfig.UiSettings)
        {
            OnPropertyChanged(nameof(AppConfig.UiSettings), oldConfig.UiSettings, newConfig.UiSettings);
        }

        if (oldConfig.RecentFiles != newConfig.RecentFiles)
        {
            OnPropertyChanged(nameof(AppConfig.RecentFiles), oldConfig.RecentFiles, newConfig.RecentFiles);
        }
    }

    /// <summary>
    /// 触发属性变更事件
    /// </summary>
    private void OnPropertyChanged(string propertyName, object? oldValue, object? newValue)
    {
        ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(propertyName, oldValue, newValue));
    }
} 