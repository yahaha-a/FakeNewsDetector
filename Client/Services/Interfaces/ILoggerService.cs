using System;

namespace Client.Services.Interfaces
{
    /// <summary>
    /// 日志服务接口
    /// </summary>
    public interface ILoggerService
    {
        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="args">格式化参数</param>
        void Debug(string message, params object[] args);

        /// <summary>
        /// 记录普通信息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="args">格式化参数</param>
        void Information(string message, params object[] args);

        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="args">格式化参数</param>
        void Warning(string message, params object[] args);

        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="args">格式化参数</param>
        void Error(string message, params object[] args);

        /// <summary>
        /// 记录错误信息（包含异常）
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">消息内容</param>
        /// <param name="args">格式化参数</param>
        void Error(Exception exception, string message, params object[] args);

        /// <summary>
        /// 记录关键错误信息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="args">格式化参数</param>
        void Fatal(string message, params object[] args);

        /// <summary>
        /// 记录关键错误信息（包含异常）
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">消息内容</param>
        /// <param name="args">格式化参数</param>
        void Fatal(Exception exception, string message, params object[] args);
        
        /// <summary>
        /// 动态设置最小日志级别
        /// </summary>
        /// <param name="level">日志级别 (Debug, Information, Warning, Error, Fatal)</param>
        void SetMinimumLevel(string level);
    }
} 