using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Client.Models;

/// <summary>
/// API设置
/// </summary>
public class ApiSettings : INotifyPropertyChanged
{
    private string _baseUrl = "http://localhost:5000";
    private int _timeout = 30;
    private int _retryCount = 3;
    private string _apiKey = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// API基础URL
    /// </summary>
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

    /// <summary>
    /// API超时时间（秒）
    /// </summary>
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

    /// <summary>
    /// 重试次数
    /// </summary>
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

    /// <summary>
    /// API密钥
    /// </summary>
    [JsonPropertyName("apiKey")]
    public string ApiKey
    {
        get => _apiKey;
        set
        {
            if (_apiKey != value)
            {
                _apiKey = value;
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
    public ApiSettings Clone()
    {
        return new ApiSettings
        {
            BaseUrl = BaseUrl,
            Timeout = Timeout,
            RetryCount = RetryCount,
            ApiKey = ApiKey
        };
    }
} 