using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Client.Models;

/// <summary>
/// UI设置
/// </summary>
public class UiSettings : INotifyPropertyChanged
{
    private double _fontSize = 14;
    private bool _showStatusBar = true;
    private bool _showToolbar = true;
    private bool _autoSave = true;
    private bool _confirmOnExit = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 字体大小
    /// </summary>
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

    /// <summary>
    /// 是否显示状态栏
    /// </summary>
    [JsonPropertyName("showStatusBar")]
    public bool ShowStatusBar
    {
        get => _showStatusBar;
        set
        {
            if (_showStatusBar != value)
            {
                _showStatusBar = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 是否显示工具栏
    /// </summary>
    [JsonPropertyName("showToolbar")]
    public bool ShowToolbar
    {
        get => _showToolbar;
        set
        {
            if (_showToolbar != value)
            {
                _showToolbar = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 是否自动保存
    /// </summary>
    [JsonPropertyName("autoSave")]
    public bool AutoSave
    {
        get => _autoSave;
        set
        {
            if (_autoSave != value)
            {
                _autoSave = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 退出时是否确认
    /// </summary>
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

    /// <summary>
    /// 属性变更通知
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 创建副本
    /// </summary>
    public UiSettings Clone()
    {
        return new UiSettings
        {
            FontSize = FontSize,
            ShowStatusBar = ShowStatusBar,
            ShowToolbar = ShowToolbar,
            AutoSave = AutoSave,
            ConfirmOnExit = ConfirmOnExit
        };
    }
} 