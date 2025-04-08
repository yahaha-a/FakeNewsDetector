using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Client.Models;

public class AppConfig
{
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "Light";

    [JsonPropertyName("language")]
    public string Language { get; set; } = "zh-CN";

    [JsonPropertyName("windowState")]
    public WindowState WindowState { get; set; } = new();

    [JsonPropertyName("recentFiles")]
    public List<string> RecentFiles { get; set; } = new();

    [JsonPropertyName("apiSettings")]
    public ApiSettings ApiSettings { get; set; } = new();

    [JsonPropertyName("uiSettings")]
    public UiSettings UiSettings { get; set; } = new();
}

public class WindowState
{
    [JsonPropertyName("width")]
    public double Width { get; set; } = 1200;

    [JsonPropertyName("height")]
    public double Height { get; set; } = 800;

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("isMaximized")]
    public bool IsMaximized { get; set; }
}

public class ApiSettings
{
    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; } = "http://localhost:5000";

    [JsonPropertyName("timeout")]
    public int Timeout { get; set; } = 30;

    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; } = 3;
}

public class UiSettings
{
    [JsonPropertyName("fontSize")]
    public double FontSize { get; set; } = 14;

    [JsonPropertyName("animationEnabled")]
    public bool AnimationEnabled { get; set; } = true;

    [JsonPropertyName("showTooltips")]
    public bool ShowTooltips { get; set; } = true;

    [JsonPropertyName("confirmOnExit")]
    public bool ConfirmOnExit { get; set; } = true;
} 