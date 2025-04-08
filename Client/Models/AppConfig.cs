using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Client.Models;

/// <summary>
/// 应用配置
/// </summary>
public class AppConfig
{
    /// <summary>
    /// 主题
    /// </summary>
    [JsonPropertyName("theme")]
    public string? Theme { get; set; }
    
    /// <summary>
    /// 语言
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }
    
    /// <summary>
    /// 窗口状态
    /// </summary>
    [JsonPropertyName("windowState")]
    public WindowState? WindowState { get; set; }
    
    /// <summary>
    /// 最近文件列表
    /// </summary>
    [JsonPropertyName("recentFiles")]
    public List<string>? RecentFiles { get; set; }
    
    /// <summary>
    /// API设置
    /// </summary>
    [JsonPropertyName("apiSettings")]
    public ApiSettings? ApiSettings { get; set; }
    
    /// <summary>
    /// UI设置
    /// </summary>
    [JsonPropertyName("uiSettings")]
    public UiSettings? UiSettings { get; set; }
    
    /// <summary>
    /// 创建一个新的AppConfig实例
    /// </summary>
    public AppConfig()
    {
        RecentFiles = new List<string>();
    }
    
    /// <summary>
    /// 创建一个AppConfig副本
    /// </summary>
    public AppConfig Clone()
    {
        return new AppConfig
        {
            Theme = Theme,
            Language = Language,
            WindowState = WindowState?.Clone(),
            RecentFiles = RecentFiles != null ? new List<string>(RecentFiles) : new List<string>(),
            ApiSettings = ApiSettings?.Clone(),
            UiSettings = UiSettings?.Clone()
        };
    }
} 