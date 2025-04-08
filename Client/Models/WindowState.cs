using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Client.Models;

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

    /// <summary>
    /// 窗口宽度
    /// </summary>
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

    /// <summary>
    /// 窗口高度
    /// </summary>
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

    /// <summary>
    /// 窗口X坐标
    /// </summary>
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

    /// <summary>
    /// 窗口Y坐标
    /// </summary>
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

    /// <summary>
    /// 是否最大化
    /// </summary>
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
    public WindowState Clone()
    {
        return new WindowState
        {
            Width = Width,
            Height = Height,
            X = X,
            Y = Y,
            IsMaximized = IsMaximized
        };
    }
} 