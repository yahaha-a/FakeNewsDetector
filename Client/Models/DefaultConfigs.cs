using System;
using System.Collections.ObjectModel;

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
    public static AppConfig GetDefaultAppConfig()
    {
        return new AppConfig
        {
            Theme = DefaultTheme,
            Language = DefaultLanguage,
            WindowState = GetDefaultWindowState(),
            RecentFiles = new ObservableCollection<string>(),
            ApiSettings = GetDefaultApiSettings(),
            UiSettings = GetDefaultUiSettings()
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
            Width = DefaultWindowWidth,
            Height = DefaultWindowHeight,
            X = 0,
            Y = 0,
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
            BaseUrl = DefaultApiBaseUrl,
            Timeout = DefaultApiTimeout,
            RetryCount = DefaultApiRetryCount
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
            FontSize = DefaultFontSize,
            AnimationEnabled = true,
            ShowTooltips = true,
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
            return GetDefaultAppConfig();
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
                config.RecentFiles = new ObservableCollection<string>();
                break;
            case ConfigSection.All:
                return GetDefaultAppConfig();
        }
        
        return config;
    }
}

/// <summary>
/// 配置部分枚举
/// </summary>
public enum ConfigSection
{
    /// <summary>
    /// 所有配置
    /// </summary>
    All,
    
    /// <summary>
    /// 窗口状态
    /// </summary>
    WindowState,
    
    /// <summary>
    /// API设置
    /// </summary>
    ApiSettings,
    
    /// <summary>
    /// UI设置
    /// </summary>
    UiSettings,
    
    /// <summary>
    /// 主题
    /// </summary>
    Theme,
    
    /// <summary>
    /// 语言
    /// </summary>
    Language,
    
    /// <summary>
    /// 最近文件
    /// </summary>
    RecentFiles
} 