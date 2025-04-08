using Client.Models;
using System;

namespace Client.Helpers.Events
{
    /// <summary>
    /// 新闻分析完成事件
    /// </summary>
    public class NewsAnalysisCompletedEvent
    {
        /// <summary>
        /// 分析结果
        /// </summary>
        public AnalysisResult Result { get; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// 耗时（毫秒）
        /// </summary>
        public long ElapsedMilliseconds { get; }

        public NewsAnalysisCompletedEvent(AnalysisResult result, bool isSuccessful, long elapsedMilliseconds)
        {
            Result = result;
            IsSuccessful = isSuccessful;
            ElapsedMilliseconds = elapsedMilliseconds;
        }
    }

    /// <summary>
    /// 新闻加载完成事件
    /// </summary>
    public class NewsLoadedEvent
    {
        /// <summary>
        /// 新闻项
        /// </summary>
        public NewsItem NewsItem { get; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// 来源类型
        /// </summary>
        public NewsSourceType SourceType { get; }

        public NewsLoadedEvent(NewsItem newsItem, bool isSuccessful, NewsSourceType sourceType)
        {
            NewsItem = newsItem;
            IsSuccessful = isSuccessful;
            SourceType = sourceType;
        }
    }

    /// <summary>
    /// 应用设置变更事件
    /// </summary>
    public class SettingsChangedEvent
    {
        /// <summary>
        /// 设置键
        /// </summary>
        public string SettingKey { get; }

        /// <summary>
        /// 旧值
        /// </summary>
        public object? OldValue { get; }

        /// <summary>
        /// 新值
        /// </summary>
        public object? NewValue { get; }

        public SettingsChangedEvent(string settingKey, object? oldValue, object? newValue)
        {
            SettingKey = settingKey;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// 错误发生事件
    /// </summary>
    public class ErrorOccurredEvent
    {
        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 异常对象
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// 错误来源
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// 严重程度
        /// </summary>
        public ErrorSeverity Severity { get; }

        public ErrorOccurredEvent(string message, string source, ErrorSeverity severity = ErrorSeverity.Error, Exception? exception = null)
        {
            Message = message;
            Source = source;
            Severity = severity;
            Exception = exception;
        }
    }

    /// <summary>
    /// 新闻来源类型
    /// </summary>
    public enum NewsSourceType
    {
        /// <summary>
        /// URL加载
        /// </summary>
        Url,
        
        /// <summary>
        /// 本地文本
        /// </summary>
        Text,
        
        /// <summary>
        /// 历史记录
        /// </summary>
        History
    }

    /// <summary>
    /// 错误严重程度
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// 信息
        /// </summary>
        Info,
        
        /// <summary>
        /// 警告
        /// </summary>
        Warning,
        
        /// <summary>
        /// 错误
        /// </summary>
        Error,
        
        /// <summary>
        /// 严重错误
        /// </summary>
        Critical
    }
} 