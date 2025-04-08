using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Client.Models;
using Client.Services.Interfaces;

namespace Client.Services;

/// <summary>
/// 配置存储服务实现
/// </summary>
public class ConfigStorageService : IConfigStorageService
{
    private readonly string _systemConfigPath;
    private readonly string _userConfigPath;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// 初始化配置存储服务
    /// </summary>
    public ConfigStorageService()
    {
        // 系统级配置路径
        var appPath = AppDomain.CurrentDomain.BaseDirectory;
        _systemConfigPath = Path.Combine(appPath, "config", "system.config.json");
        
        // 用户级配置路径
        var userPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FakeNewsDetector"
        );
        _userConfigPath = Path.Combine(userPath, "user.config.json");
        
        // 确保目录存在
        Directory.CreateDirectory(Path.GetDirectoryName(_systemConfigPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(_userConfigPath)!);
        
        // JSON序列化选项
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    public string GetConfigPath(ConfigLevel level)
    {
        return level switch
        {
            ConfigLevel.System => _systemConfigPath,
            ConfigLevel.User => _userConfigPath,
            _ => throw new ArgumentException($"不支持的配置层级: {level}")
        };
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    public async Task SaveConfigAsync(AppConfig config, ConfigLevel level)
    {
        if (level == ConfigLevel.Session)
        {
            throw new ArgumentException("会话级配置不支持持久化");
        }

        var configPath = GetConfigPath(level);
        var json = JsonSerializer.Serialize(config, _jsonOptions);
        await File.WriteAllTextAsync(configPath, json);
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    public async Task<AppConfig> LoadConfigAsync(ConfigLevel level)
    {
        if (level == ConfigLevel.Session)
        {
            throw new ArgumentException("会话级配置不支持加载");
        }

        var configPath = GetConfigPath(level);
        if (!File.Exists(configPath))
        {
            return DefaultConfigs.GetDefaultConfig();
        }

        var json = await File.ReadAllTextAsync(configPath);
        return JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions) 
               ?? DefaultConfigs.GetDefaultConfig();
    }

    /// <summary>
    /// 检查配置是否存在
    /// </summary>
    public Task<bool> ConfigExistsAsync(ConfigLevel level)
    {
        if (level == ConfigLevel.Session)
        {
            return Task.FromResult(false);
        }

        var configPath = GetConfigPath(level);
        return Task.FromResult(File.Exists(configPath));
    }
} 