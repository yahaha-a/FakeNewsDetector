using System;
using System.Collections.Generic;

namespace Client.Models;

/// <summary>
/// 配置默认值管理类
/// </summary>
public static class DefaultConfigs
{
    /// <summary>
    /// 获取默认主题
    /// </summary>
    public const string DefaultTheme = "Light";
    
    /// <summary>
    /// 获取默认语言
    /// </summary>
    public const string DefaultLanguage = "zh-CN";
    
    /// <summary>
    /// 获取默认API基础URL
    /// </summary>
    public const string DefaultApiBaseUrl = "http://localhost:5000";
    
    /// <summary>
    /// 获取默认API请求超时(秒)
    /// </summary>
    public const int DefaultApiTimeout = 30;
    
    /// <summary>
    /// 获取默认API重试次数
    /// </summary>
    public const int DefaultApiRetryCount = 3;
    
    /// <summary>
    /// 获取默认窗口宽度
    /// </summary>
    public const double DefaultWindowWidth = 1200;
    
    /// <summary>
    /// 获取默认窗口高度
    /// </summary>
    public const double DefaultWindowHeight = 800;
    
    /// <summary>
    /// 获取默认字体大小
    /// </summary>
    public const double DefaultFontSize = 14;
    
    /// <summary>
    /// 获取默认完整应用配置
    /// </summary>
    /// <returns>默认应用配置</returns>
    public static AppConfig GetDefaultConfig()
    {
        return new AppConfig
        {
            Theme = DefaultTheme,
            Language = DefaultLanguage,
            WindowState = GetDefaultWindowState(),
            ApiSettings = GetDefaultApiSettings(),
            UiSettings = GetDefaultUiSettings(),
            RecentFiles = new List<string>()
        };
    }
    
    /// <summary>
    /// 获取默认窗口状态
    /// </summary>
    /// <returns>默认窗口状态</returns>
    public static WindowState GetDefaultWindowState()
    {
        return new WindowState
        {
            Width = 1280,
            Height = 720,
            X = 100,
            Y = 100,
            IsMaximized = false
        };
    }
    
    /// <summary>
    /// 获取默认API设置
    /// </summary>
    /// <returns>默认API设置</returns>
    public static ApiSettings GetDefaultApiSettings()
    {
        return new ApiSettings
        {
            BaseUrl = "https://api.fakenewsdetector.com",
            Timeout = 30,
            RetryCount = 3,
            ApiKey = string.Empty
        };
    }
    
    /// <summary>
    /// 获取默认UI设置
    /// </summary>
    /// <returns>默认UI设置</returns>
    public static UiSettings GetDefaultUiSettings()
    {
        return new UiSettings
        {
            FontSize = 14,
            ShowStatusBar = true,
            ShowToolbar = true,
            AutoSave = true,
            ConfirmOnExit = true
        };
    }
    
    /// <summary>
    /// 重置特定部分的配置
    /// </summary>
    /// <param name="config">待修改的配置对象</param>
    /// <param name="section">要重置的配置部分</param>
    /// <returns>修改后的配置对象</returns>
    public static AppConfig ResetConfigSection(AppConfig config, ConfigSection section)
    {
        if (config == null)
        {
            return GetDefaultConfig();
        }
        
        switch (section)
        {
            case ConfigSection.WindowState:
                config.WindowState = GetDefaultWindowState();
                break;
            case ConfigSection.ApiSettings:
                config.ApiSettings = GetDefaultApiSettings();
                break;
            case ConfigSection.UiSettings:
                config.UiSettings = GetDefaultUiSettings();
                break;
            case ConfigSection.Theme:
                config.Theme = DefaultTheme;
                break;
            case ConfigSection.Language:
                config.Language = DefaultLanguage;
                break;
            case ConfigSection.RecentFiles:
                config.RecentFiles = new List<string>();
                break;
            case ConfigSection.All:
                return GetDefaultConfig();
        }
        
        return config;
    }
} 