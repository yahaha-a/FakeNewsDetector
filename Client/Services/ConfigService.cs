using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Client.Models;
using Serilog;

namespace Client.Services;

public class ConfigService : IConfigService
{
    private readonly string _configPath;
    private AppConfig _currentConfig;
    private readonly JsonSerializerOptions _jsonOptions;

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
    }

    public AppConfig GetConfig()
    {
        return _currentConfig;
    }

    public async Task UpdateConfigAsync(AppConfig config)
    {
        _currentConfig = config;
        await SaveConfigAsync();
    }

    public async Task SaveConfigAsync()
    {
        try
        {
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
} 