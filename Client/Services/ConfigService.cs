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

public class ConfigService : IConfigService
{
    private readonly string _configPath;
    private AppConfig _currentConfig;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IValidator<AppConfig> _validator;

    public ConfigService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FakeNewsDetector"
        );
        Directory.CreateDirectory(appDataPath);
        _configPath = Path.Combine(appDataPath, "config.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        _currentConfig = new AppConfig();
        _validator = new AppConfigValidator();
    }

    public AppConfig GetConfig()
    {
        return _currentConfig;
    }

    public async Task UpdateConfigAsync(AppConfig config)
    {
        // 验证配置
        var validationResult = await ValidateConfigAsync(config);
        if (!validationResult.IsValid)
        {
            var exception = new ConfigValidationException(validationResult.Errors);
            Log.Error(exception, "配置验证失败");
            
            // 尝试修复配置
            config = await FixConfigAsync(config);
            Log.Information("已尝试修复配置");
        }

        _currentConfig = config;
        await SaveConfigAsync();
    }

    public async Task SaveConfigAsync()
    {
        try
        {
            // 保存前再次验证
            var validationResult = await ValidateCurrentConfigAsync();
            if (!validationResult.IsValid)
            {
                Log.Warning("保存时配置验证失败，尝试修复配置");
                _currentConfig = await FixConfigAsync(_currentConfig);
            }
            
            var json = JsonSerializer.Serialize(_currentConfig, _jsonOptions);
            await File.WriteAllTextAsync(_configPath, json);
            Log.Information("配置已保存");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "保存配置失败");
            throw;
        }
    }

    public async Task LoadConfigAsync()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = await File.ReadAllTextAsync(_configPath);
                _currentConfig = JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions) ?? new AppConfig();
                
                // 验证加载的配置
                var validationResult = await ValidateCurrentConfigAsync();
                if (!validationResult.IsValid)
                {
                    Log.Warning("加载的配置验证失败，尝试修复配置");
                    _currentConfig = await FixConfigAsync(_currentConfig);
                }
                
                Log.Information("配置已加载");
            }
            else
            {
                _currentConfig = new AppConfig();
                await SaveConfigAsync();
                Log.Information("创建新配置文件");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "加载配置失败");
            _currentConfig = new AppConfig();
        }
    }

    public async Task ResetConfigAsync()
    {
        _currentConfig = new AppConfig();
        await SaveConfigAsync();
        Log.Information("配置已重置为默认值");
    }
    
    public Task<ValidationResult> ValidateConfigAsync(AppConfig config)
    {
        return Task.FromResult(_validator.Validate(config));
    }
    
    public Task<ValidationResult> ValidateCurrentConfigAsync()
    {
        return ValidateConfigAsync(_currentConfig);
    }
    
    public Task<AppConfig> FixConfigAsync(AppConfig config)
    {
        try
        {
            // 创建一个有效的默认配置
            var defaultConfig = new AppConfig();
            
            // 有效的主题列表
            var validThemes = new[] { "Light", "Dark", "System" };
            
            // 有效的语言列表
            var validLanguages = new[] { "zh-CN", "en-US" };
            
            // 修复主题
            if (string.IsNullOrEmpty(config.Theme) || 
                !validThemes.Contains(config.Theme))
            {
                config.Theme = defaultConfig.Theme;
                Log.Information("已修复无效的主题设置");
            }
            
            // 修复语言
            if (string.IsNullOrEmpty(config.Language) || 
                !validLanguages.Contains(config.Language))
            {
                config.Language = defaultConfig.Language;
                Log.Information("已修复无效的语言设置");
            }
            
            // 修复窗口状态
            if (config.WindowState == null)
            {
                config.WindowState = defaultConfig.WindowState;
                Log.Information("已修复窗口状态设置");
            }
            else
            {
                // 修复窗口尺寸
                if (config.WindowState.Width < 800 || config.WindowState.Width > 4000)
                {
                    config.WindowState.Width = defaultConfig.WindowState.Width;
                    Log.Information("已修复窗口宽度设置");
                }
                
                if (config.WindowState.Height < 600 || config.WindowState.Height > 3000)
                {
                    config.WindowState.Height = defaultConfig.WindowState.Height;
                    Log.Information("已修复窗口高度设置");
                }
            }
            
            // 修复最近文件列表
            if (config.RecentFiles == null)
            {
                config.RecentFiles = defaultConfig.RecentFiles;
                Log.Information("已修复最近文件列表设置");
            }
            else
            {
                // 移除空路径
                config.RecentFiles.RemoveAll(string.IsNullOrWhiteSpace);
                Log.Information("已移除空的最近文件路径");
            }
            
            // 修复API设置
            if (config.ApiSettings == null)
            {
                config.ApiSettings = defaultConfig.ApiSettings;
                Log.Information("已修复API设置");
            }
            else
            {
                // 修复BaseUrl
                if (string.IsNullOrEmpty(config.ApiSettings.BaseUrl) || 
                    (!config.ApiSettings.BaseUrl.StartsWith("http://") && 
                     !config.ApiSettings.BaseUrl.StartsWith("https://")))
                {
                    config.ApiSettings.BaseUrl = defaultConfig.ApiSettings.BaseUrl;
                    Log.Information("已修复API基础URL设置");
                }
                
                // 修复超时设置
                if (config.ApiSettings.Timeout < 5 || config.ApiSettings.Timeout > 120)
                {
                    config.ApiSettings.Timeout = defaultConfig.ApiSettings.Timeout;
                    Log.Information("已修复API超时设置");
                }
                
                // 修复重试设置
                if (config.ApiSettings.RetryCount < 0 || config.ApiSettings.RetryCount > 10)
                {
                    config.ApiSettings.RetryCount = defaultConfig.ApiSettings.RetryCount;
                    Log.Information("已修复API重试次数设置");
                }
            }
            
            // 修复UI设置
            if (config.UiSettings == null)
            {
                config.UiSettings = defaultConfig.UiSettings;
                Log.Information("已修复UI设置");
            }
            else
            {
                // 修复字体大小
                if (config.UiSettings.FontSize < 8 || config.UiSettings.FontSize > 24)
                {
                    config.UiSettings.FontSize = defaultConfig.UiSettings.FontSize;
                    Log.Information("已修复字体大小设置");
                }
            }
            
            // 验证修复后的配置
            var validationResult = _validator.Validate(config);
            if (!validationResult.IsValid)
            {
                Log.Warning("修复后的配置仍然无效，将使用默认配置");
                return Task.FromResult(defaultConfig);
            }
            
            return Task.FromResult(config);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "修复配置失败，将使用默认配置");
            return Task.FromResult(new AppConfig());
        }
    }
} 