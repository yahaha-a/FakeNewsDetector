using Client.Services.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace Client.Services
{
    /// <summary>
    /// 应用设置服务实现
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private Dictionary<string, object> _settings;
        private const string DEFAULT_API_URL = "http://localhost:5000";
        private readonly SemaphoreSlim _initializationLock = new SemaphoreSlim(1, 1);
        private bool _isInitialized = false;

        public string ApiBaseUrl { get; set; } = string.Empty;
        public bool IsDarkMode { get; set; }
        public bool AutoSaveHistory { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public SettingsService()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FakeNewsDetector");
            
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
            _settings = new Dictionary<string, object>();
            
            // 设置默认值
            ApiBaseUrl = DEFAULT_API_URL;
            IsDarkMode = false;
            AutoSaveHistory = true;
            
            // 不在构造函数中同步加载设置，初始化过程将移至异步方法
        }

        /// <summary>
        /// 确保服务已初始化
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized)
                return;

            await _initializationLock.WaitAsync();
            try
            {
                if (!_isInitialized)
                {
                    await LoadSettingsAsync();
                    // 初始化属性
                    ApiBaseUrl = await GetSettingAsync<string>("ApiBaseUrl", DEFAULT_API_URL);
                    IsDarkMode = await GetSettingAsync<bool>("IsDarkMode", false);
                    AutoSaveHistory = await GetSettingAsync<bool>("AutoSaveHistory", true);
                    _isInitialized = true;
                }
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        private async Task LoadSettingsAsync()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(_settingsFilePath);
                    var loadedSettings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                    
                    if (loadedSettings != null)
                    {
                        _settings = new Dictionary<string, object>();
                        foreach (var kvp in loadedSettings)
                        {
                            switch (kvp.Value.ValueKind)
                            {
                                case JsonValueKind.String:
                                    string? strValue = kvp.Value.GetString();
                                    _settings[kvp.Key] = strValue ?? string.Empty;
                                    break;
                                case JsonValueKind.Number:
                                    if (kvp.Value.TryGetInt32(out int intValue))
                                        _settings[kvp.Key] = intValue;
                                    else
                                        _settings[kvp.Key] = kvp.Value.GetDouble();
                                    break;
                                case JsonValueKind.True:
                                case JsonValueKind.False:
                                    _settings[kvp.Key] = kvp.Value.GetBoolean();
                                    break;
                                default:
                                    _settings[kvp.Key] = kvp.Value.GetRawText();
                                    break;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    _settings = new Dictionary<string, object>();
                }
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(_settings);
                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception)
            {
                // 记录异常
            }
        }

        public async Task<T> GetSettingAsync<T>(string key, T? defaultValue = default)
        {
            // 确保设置已加载
            await EnsureInitializedAsync();
            
            if (_settings.TryGetValue(key, out object? value))
            {
                try
                {
                    if (value is JsonElement jsonElement)
                    {
                        string json = jsonElement.GetRawText();
                        var result = JsonSerializer.Deserialize<T>(json);
                        if (result != null)
                            return result;
                        return defaultValue!;
                    }
                    
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }
                    
                    // 尝试转换
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue!;
                }
            }
            
            return defaultValue!;
        }

        public async Task<bool> SaveSettingAsync<T>(string key, T value)
        {
            try
            {
                // 确保设置已加载
                await EnsureInitializedAsync();
                
                _settings[key] = value!;
                
                // 更新属性
                if (key == "ApiBaseUrl" && value is string apiUrl)
                    ApiBaseUrl = apiUrl;
                else if (key == "IsDarkMode" && value is bool darkMode)
                    IsDarkMode = darkMode;
                else if (key == "AutoSaveHistory" && value is bool autoSave)
                    AutoSaveHistory = autoSave;
                
                await SaveSettingsAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveSettingAsync(string key)
        {
            // 确保设置已加载
            await EnsureInitializedAsync();
            
            if (_settings.ContainsKey(key))
            {
                _settings.Remove(key);
                await SaveSettingsAsync();
                return true;
            }
            
            return false;
        }

        public async Task<bool> ClearAllSettingsAsync()
        {
            try
            {
                // 确保设置已加载
                await EnsureInitializedAsync();
                
                _settings.Clear();
                
                // 重置属性到默认值
                ApiBaseUrl = DEFAULT_API_URL;
                IsDarkMode = false;
                AutoSaveHistory = true;
                
                await SaveSettingsAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 