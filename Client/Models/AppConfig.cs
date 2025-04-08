using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Client.Models;

/// <summary>
/// 应用配置
/// </summary>
public class AppConfig : INotifyPropertyChanged
{
    private string _theme = "Light";
    private string _language = "zh-CN";
    private WindowState _windowState = new();
    private ObservableCollection<string> _recentFiles = new();
    private ApiSettings _apiSettings = new();
    private UiSettings _uiSettings = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? RecentFilesChanged;

    [JsonPropertyName("theme")]
    public string Theme
    {
        get => _theme;
        set
        {
            if (_theme != value)
            {
                _theme = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("language")]
    public string Language
    {
        get => _language;
        set
        {
            if (_language != value)
            {
                _language = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("windowState")]
    public WindowState WindowState
    {
        get => _windowState;
        set
        {
            if (_windowState != value)
            {
                _windowState = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("recentFiles")]
    public ObservableCollection<string> RecentFiles
    {
        get => _recentFiles;
        set
        {
            if (_recentFiles != value)
            {
                if (_recentFiles != null)
                {
                    _recentFiles.CollectionChanged -= OnRecentFilesChanged;
                }
                
                _recentFiles = value;
                
                if (_recentFiles != null)
                {
                    _recentFiles.CollectionChanged += OnRecentFilesChanged;
                }
                
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("apiSettings")]
    public ApiSettings ApiSettings
    {
        get => _apiSettings;
        set
        {
            if (_apiSettings != value)
            {
                _apiSettings = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("uiSettings")]
    public UiSettings UiSettings
    {
        get => _uiSettings;
        set
        {
            if (_uiSettings != value)
            {
                _uiSettings = value;
                OnPropertyChanged();
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnRecentFilesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RecentFilesChanged?.Invoke(this, e);
    }
}

/// <summary>
/// 窗口状态
/// </summary>
public class WindowState : INotifyPropertyChanged
{
    private double _width = 1200;
    private double _height = 800;
    private double _x;
    private double _y;
    private bool _isMaximized;

    public event PropertyChangedEventHandler? PropertyChanged;

    [JsonPropertyName("width")]
    public double Width
    {
        get => _width;
        set
        {
            if (Math.Abs(_width - value) > 0.01)
            {
                _width = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("height")]
    public double Height
    {
        get => _height;
        set
        {
            if (Math.Abs(_height - value) > 0.01)
            {
                _height = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("x")]
    public double X
    {
        get => _x;
        set
        {
            if (Math.Abs(_x - value) > 0.01)
            {
                _x = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("y")]
    public double Y
    {
        get => _y;
        set
        {
            if (Math.Abs(_y - value) > 0.01)
            {
                _y = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("isMaximized")]
    public bool IsMaximized
    {
        get => _isMaximized;
        set
        {
            if (_isMaximized != value)
            {
                _isMaximized = value;
                OnPropertyChanged();
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// API设置
/// </summary>
public class ApiSettings : INotifyPropertyChanged
{
    private string _baseUrl = "http://localhost:5000";
    private int _timeout = 30;
    private int _retryCount = 3;

    public event PropertyChangedEventHandler? PropertyChanged;

    [JsonPropertyName("baseUrl")]
    public string BaseUrl
    {
        get => _baseUrl;
        set
        {
            if (_baseUrl != value)
            {
                _baseUrl = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("timeout")]
    public int Timeout
    {
        get => _timeout;
        set
        {
            if (_timeout != value)
            {
                _timeout = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("retryCount")]
    public int RetryCount
    {
        get => _retryCount;
        set
        {
            if (_retryCount != value)
            {
                _retryCount = value;
                OnPropertyChanged();
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// UI设置
/// </summary>
public class UiSettings : INotifyPropertyChanged
{
    private double _fontSize = 14;
    private bool _animationEnabled = true;
    private bool _showTooltips = true;
    private bool _confirmOnExit = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    [JsonPropertyName("fontSize")]
    public double FontSize
    {
        get => _fontSize;
        set
        {
            if (Math.Abs(_fontSize - value) > 0.01)
            {
                _fontSize = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("animationEnabled")]
    public bool AnimationEnabled
    {
        get => _animationEnabled;
        set
        {
            if (_animationEnabled != value)
            {
                _animationEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("showTooltips")]
    public bool ShowTooltips
    {
        get => _showTooltips;
        set
        {
            if (_showTooltips != value)
            {
                _showTooltips = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("confirmOnExit")]
    public bool ConfirmOnExit
    {
        get => _confirmOnExit;
        set
        {
            if (_confirmOnExit != value)
            {
                _confirmOnExit = value;
                OnPropertyChanged();
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 