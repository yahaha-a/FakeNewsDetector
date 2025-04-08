using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Client.Models;
using Client.Models.Validation;
using Client.Services.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Serilog;

namespace Client.Services;

/// <summary>
/// 配置服务实现
/// </summary>
public class ConfigService : IConfigService, IDisposable
{
    private readonly IConfigStorageService _storageService;
    private readonly IValidator<AppConfig> _validator;
    private AppConfig _currentConfig;
    private AppConfig _systemConfig;
    private AppConfig _userConfig;
    private AppConfig _sessionConfig;
    private readonly FileSystemWatcher _watcher;
    private bool _disposed;

    /// <summary>
    /// 配置变更事件
    /// </summary>
    public event EventHandler<ConfigChangedEventArgs>? ConfigChanged;

    /// <summary>
    /// 初始化配置服务
    /// </summary>
    public ConfigService(IConfigStorageService storageService, IValidator<AppConfig> validator)
    {
        _storageService = storageService;
        _validator = validator;
        
        // 初始化默认配置
        _systemConfig = DefaultConfigs.GetDefaultConfig();
        _userConfig = DefaultConfigs.GetDefaultConfig();
        _sessionConfig = DefaultConfigs.GetDefaultConfig();
        _currentConfig = DefaultConfigs.GetDefaultConfig();

        // 初始化文件监视器
        _watcher = new FileSystemWatcher
        {
            Path = Path.GetDirectoryName(_storageService.GetConfigPath(ConfigLevel.User))!,
            Filter = "*.json",
            NotifyFilter = NotifyFilters.LastWrite
        };
        _watcher.Changed += OnConfigFileChanged;
        _watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// 获取当前配置
    /// </summary>
    public AppConfig GetConfig() => _currentConfig;

    /// <summary>
    /// 获取指定级别的配置
    /// </summary>
    public AppConfig GetConfig(ConfigLevel level) => level switch
    {
        ConfigLevel.System => _systemConfig,
        ConfigLevel.User => _userConfig,
        ConfigLevel.Session => _sessionConfig,
        _ => throw new ArgumentException("不支持的配置层级", nameof(level))
    };

    /// <summary>
    /// 更新配置
    /// </summary>
    public async Task UpdateConfigAsync(AppConfig config)
    {
        var oldConfig = _currentConfig;
        _currentConfig = config;
        await SaveConfigAsync();
        RaiseConfigChanged(oldConfig, config);
    }

    /// <summary>
    /// 更新指定级别的配置
    /// </summary>
    public async Task UpdateConfigAsync(AppConfig config, ConfigLevel level)
    {
        switch (level)
        {
            case ConfigLevel.System:
                _systemConfig = config;
                break;
            case ConfigLevel.User:
                _userConfig = config;
                break;
            case ConfigLevel.Session:
                _sessionConfig = config;
                break;
            default:
                throw new ArgumentException("不支持的配置层级", nameof(level));
        }

        await SaveConfigAsync(level);
        RaiseConfigChanged(GetConfig(level), config);
    }

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    public async Task SaveConfigAsync()
    {
        await _storageService.SaveConfigAsync(_currentConfig, ConfigLevel.User);
    }

    /// <summary>
    /// 保存指定级别的配置到文件
    /// </summary>
    public async Task SaveConfigAsync(ConfigLevel level)
    {
        await _storageService.SaveConfigAsync(GetConfig(level), level);
    }

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    public async Task LoadConfigAsync()
    {
        _systemConfig = await _storageService.LoadConfigAsync(ConfigLevel.System);
        _userConfig = await _storageService.LoadConfigAsync(ConfigLevel.User);
        _sessionConfig = DefaultConfigs.GetDefaultConfig();
        
        // 合并配置：会话级 > 用户级 > 系统级
        _currentConfig = MergeConfigs(_sessionConfig, _userConfig, _systemConfig);
    }

    /// <summary>
    /// 从指定级别的文件加载配置
    /// </summary>
    public async Task LoadConfigAsync(ConfigLevel level)
    {
        switch (level)
        {
            case ConfigLevel.System:
                _systemConfig = await _storageService.LoadConfigAsync(level);
                break;
            case ConfigLevel.User:
                _userConfig = await _storageService.LoadConfigAsync(level);
                break;
            case ConfigLevel.Session:
                _sessionConfig = DefaultConfigs.GetDefaultConfig();
                break;
            default:
                throw new ArgumentException("不支持的配置层级", nameof(level));
        }

        // 重新合并配置
        _currentConfig = MergeConfigs(_sessionConfig, _userConfig, _systemConfig);
    }

    /// <summary>
    /// 重置配置为默认值
    /// </summary>
    public async Task ResetConfigAsync()
    {
        _currentConfig = DefaultConfigs.GetDefaultConfig();
        await SaveConfigAsync();
        RaiseConfigChanged(null, _currentConfig);
    }

    /// <summary>
    /// 重置指定级别的配置为默认值
    /// </summary>
    public async Task ResetConfigAsync(ConfigLevel level)
    {
        var defaultConfig = DefaultConfigs.GetDefaultConfig();
        await UpdateConfigAsync(defaultConfig, level);
    }
    
    /// <summary>
    /// 重置配置的指定部分为默认值
    /// </summary>
    public async Task ResetConfigSectionAsync(ConfigSection section)
    {
        var defaultConfig = DefaultConfigs.GetDefaultConfig();
        var currentConfig = _currentConfig;

        switch (section)
        {
            case ConfigSection.Theme:
                currentConfig.Theme = defaultConfig.Theme;
                break;
            case ConfigSection.Language:
                currentConfig.Language = defaultConfig.Language;
                break;
            case ConfigSection.WindowState:
                currentConfig.WindowState = defaultConfig.WindowState;
                break;
            case ConfigSection.RecentFiles:
                currentConfig.RecentFiles = defaultConfig.RecentFiles;
                break;
            case ConfigSection.ApiSettings:
                currentConfig.ApiSettings = defaultConfig.ApiSettings;
                break;
            case ConfigSection.UiSettings:
                currentConfig.UiSettings = defaultConfig.UiSettings;
                break;
        }

        await UpdateConfigAsync(currentConfig);
    }

    /// <summary>
    /// 重置指定级别的配置的指定部分为默认值
    /// </summary>
    public async Task ResetConfigSectionAsync(ConfigSection section, ConfigLevel level)
    {
        var defaultConfig = DefaultConfigs.GetDefaultConfig();
        var config = GetConfig(level);

        switch (section)
        {
            case ConfigSection.Theme:
                config.Theme = defaultConfig.Theme;
                break;
            case ConfigSection.Language:
                config.Language = defaultConfig.Language;
                break;
            case ConfigSection.WindowState:
                config.WindowState = defaultConfig.WindowState;
                break;
            case ConfigSection.RecentFiles:
                config.RecentFiles = defaultConfig.RecentFiles;
                break;
            case ConfigSection.ApiSettings:
                config.ApiSettings = defaultConfig.ApiSettings;
                break;
            case ConfigSection.UiSettings:
                config.UiSettings = defaultConfig.UiSettings;
                break;
        }

        await UpdateConfigAsync(config, level);
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
        var validationResult = await ValidateConfigAsync(config);
        if (validationResult.IsValid)
            return config;

        var fixedConfig = DefaultConfigs.GetDefaultConfig();
        // 保留有效的配置值
        if (validationResult.Errors.All(e => !e.PropertyName.StartsWith("Theme")))
            fixedConfig.Theme = config.Theme;
        if (validationResult.Errors.All(e => !e.PropertyName.StartsWith("Language")))
            fixedConfig.Language = config.Language;
        // ... 其他属性的修复逻辑

        return fixedConfig;
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType == WatcherChangeTypes.Changed)
        {
            _ = LoadConfigAsync();
        }
    }

    private void RaiseConfigChanged(AppConfig? oldConfig, AppConfig newConfig)
    {
        if (oldConfig == null)
        {
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs("All", null, newConfig));
            return;
        }

        if (oldConfig.Theme != newConfig.Theme)
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs("Theme", oldConfig.Theme, newConfig.Theme));
        if (oldConfig.Language != newConfig.Language)
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs("Language", oldConfig.Language, newConfig.Language));
        // ... 其他属性的变更通知
    }

    private AppConfig MergeConfigs(AppConfig sessionConfig, AppConfig userConfig, AppConfig systemConfig)
    {
        return new AppConfig
        {
            Theme = sessionConfig.Theme ?? userConfig.Theme ?? systemConfig.Theme,
            Language = sessionConfig.Language ?? userConfig.Language ?? systemConfig.Language,
            WindowState = sessionConfig.WindowState ?? userConfig.WindowState ?? systemConfig.WindowState,
            RecentFiles = sessionConfig.RecentFiles ?? userConfig.RecentFiles ?? systemConfig.RecentFiles ?? new List<string>(),
            ApiSettings = sessionConfig.ApiSettings ?? userConfig.ApiSettings ?? systemConfig.ApiSettings,
            UiSettings = sessionConfig.UiSettings ?? userConfig.UiSettings ?? systemConfig.UiSettings
        };
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _watcher.Dispose();
            _disposed = true;
        }
    }
} 