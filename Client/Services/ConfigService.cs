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
public class ConfigService : IConfigService, IDisposable
{
    private readonly string _configFilePath;
    private readonly IValidator<AppConfig> _validator;
    private AppConfig _currentConfig;
    private bool _isInitialized;
    private FileSystemWatcher? _configWatcher;
    private bool _isDisposed;

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
        
        // 使用默认配置
        _currentConfig = DefaultConfigs.GetDefaultAppConfig();
        _isInitialized = false;
        _isDisposed = false;

        // 初始化文件监视器
        InitializeFileWatcher();
    }

    /// <summary>
    /// 初始化文件监视器
    /// </summary>
    private void InitializeFileWatcher()
    {
        var configDirectory = Path.GetDirectoryName(_configFilePath);
        if (string.IsNullOrEmpty(configDirectory))
        {
            throw new InvalidOperationException("配置文件路径无效");
        }

        _configWatcher = new FileSystemWatcher(configDirectory)
        {
            Filter = Path.GetFileName(_configFilePath),
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = false
        };

        _configWatcher.Changed += async (sender, e) =>
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                try
                {
                    // 等待一小段时间，确保文件写入完成
                    await Task.Delay(100);
                    await LoadConfigAsync();
                }
                catch (Exception ex)
                {
                    // 记录错误但不抛出异常
                    Console.WriteLine($"配置文件监视错误: {ex.Message}");
                }
            }
        };
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
        // 暂时禁用文件监视
        if (_configWatcher != null)
        {
            _configWatcher.EnableRaisingEvents = false;
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(_currentConfig, options);
            await File.WriteAllTextAsync(_configFilePath, json);
        }
        finally
        {
            // 重新启用文件监视
            if (_configWatcher != null)
            {
                _configWatcher.EnableRaisingEvents = true;
            }
        }
    }

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    public async Task LoadConfigAsync()
    {
        if (!File.Exists(_configFilePath))
        {
            // 使用默认配置
            _currentConfig = DefaultConfigs.GetDefaultAppConfig();
            await SaveConfigAsync();
        }
        else
        {
            try
            {
                var json = await File.ReadAllTextAsync(_configFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var oldConfig = _currentConfig;
                var loadedConfig = JsonSerializer.Deserialize<AppConfig>(json, options);
                _currentConfig = loadedConfig ?? DefaultConfigs.GetDefaultAppConfig();

                // 验证加载的配置
                var validationResult = await ValidateConfigAsync(_currentConfig);
                if (!validationResult.IsValid)
                {
                    // 尝试修复配置
                    _currentConfig = await FixConfigAsync(_currentConfig);
                    await SaveConfigAsync();
                }

                // 触发配置变更事件
                OnConfigChanged(oldConfig, _currentConfig);
            }
            catch (Exception ex)
            {
                // 出错时使用默认配置
                Log.Error(ex, "加载配置文件失败，将使用默认配置");
                var oldConfig = _currentConfig;
                _currentConfig = DefaultConfigs.GetDefaultAppConfig();
                
                // 触发配置变更事件
                OnConfigChanged(oldConfig, _currentConfig);
                
                // 保存有效的默认配置
                await SaveConfigAsync();
            }
        }

        _isInitialized = true;
        
        // 启用文件监视
        if (_configWatcher != null)
        {
            _configWatcher.EnableRaisingEvents = true;
        }
    }

    /// <summary>
    /// 重置配置为默认值
    /// </summary>
    public async Task ResetConfigAsync()
    {
        var oldConfig = _currentConfig;
        _currentConfig = DefaultConfigs.GetDefaultAppConfig();

        // 触发配置变更事件
        OnConfigChanged(oldConfig, _currentConfig);

        await SaveConfigAsync();
    }
    
    /// <summary>
    /// 重置配置的指定部分为默认值
    /// </summary>
    public async Task ResetConfigSectionAsync(ConfigSection section)
    {
        var oldConfig = _currentConfig;
        _currentConfig = DefaultConfigs.ResetConfigSection(_currentConfig, section);
        
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
        if (config == null)
        {
            return DefaultConfigs.GetDefaultAppConfig();
        }
        
        var result = await ValidateConfigAsync(config);
        if (result.IsValid)
        {
            return config;
        }

        // 按属性逐个修复配置
        // 主题
        if (result.Errors.Any(e => e.PropertyName == nameof(AppConfig.Theme)))
        {
            config.Theme = DefaultConfigs.DefaultTheme;
        }
        
        // 语言
        if (result.Errors.Any(e => e.PropertyName == nameof(AppConfig.Language)))
        {
            config.Language = DefaultConfigs.DefaultLanguage;
        }
        
        // 窗口状态
        if (result.Errors.Any(e => e.PropertyName.StartsWith(nameof(AppConfig.WindowState))))
        {
            config.WindowState = DefaultConfigs.GetDefaultWindowState();
        }
        
        // API设置
        if (result.Errors.Any(e => e.PropertyName.StartsWith(nameof(AppConfig.ApiSettings))))
        {
            config.ApiSettings = DefaultConfigs.GetDefaultApiSettings();
        }
        
        // UI设置
        if (result.Errors.Any(e => e.PropertyName.StartsWith(nameof(AppConfig.UiSettings))))
        {
            config.UiSettings = DefaultConfigs.GetDefaultUiSettings();
        }
        
        // 最近文件列表
        if (result.Errors.Any(e => e.PropertyName == nameof(AppConfig.RecentFiles)))
        {
            config.RecentFiles.Clear();
        }
        
        // 再次验证修复后的配置
        result = await ValidateConfigAsync(config);
        if (!result.IsValid)
        {
            // 如果修复后仍然无效，则返回完全默认的配置
            Log.Warning("配置修复失败，使用完全默认配置");
            return DefaultConfigs.GetDefaultAppConfig();
        }
        
        return config;
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
        
        if (oldConfig.Theme != newConfig.Theme)
        {
            OnPropertyChanged(nameof(AppConfig.Theme), oldConfig.Theme, newConfig.Theme);
        }
        
        if (oldConfig.Language != newConfig.Language)
        {
            OnPropertyChanged(nameof(AppConfig.Language), oldConfig.Language, newConfig.Language);
        }
    }

    /// <summary>
    /// 触发属性变更事件
    /// </summary>
    private void OnPropertyChanged(string propertyName, object? oldValue, object? newValue)
    {
        ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(propertyName, oldValue, newValue));
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _configWatcher?.Dispose();
            }
            _isDisposed = true;
        }
    }
} 