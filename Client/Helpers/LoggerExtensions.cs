using Client.Services.Interfaces;
using System;

namespace Client.Helpers
{
    /// <summary>
    /// 日志服务扩展方法
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// 记录组件信息日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="details">详情</param>
        public static void LogComponentInfo(this ILoggerService logger, string component, string action, string? details = null)
        {
            logger.Information("[{Component}] {Action}{Details}", 
                component, 
                action, 
                !string.IsNullOrEmpty(details) ? $": {details}" : "");
        }
        
        /// <summary>
        /// 记录组件调试日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="details">详情</param>
        public static void LogComponentDebug(this ILoggerService logger, string component, string action, string? details = null)
        {
            logger.Debug("[{Component}] {Action}{Details}", 
                component, 
                action, 
                !string.IsNullOrEmpty(details) ? $": {details}" : "");
        }
        
        /// <summary>
        /// 记录组件警告日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="details">详情</param>
        public static void LogComponentWarning(this ILoggerService logger, string component, string action, string? details = null)
        {
            logger.Warning("[{Component}] {Action}{Details}", 
                component, 
                action, 
                !string.IsNullOrEmpty(details) ? $": {details}" : "");
        }
        
        /// <summary>
        /// 记录组件错误日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="details">详情</param>
        public static void LogComponentError(this ILoggerService logger, string component, string action, string? details = null)
        {
            logger.Error("[{Component}] {Action}{Details}", 
                component, 
                action, 
                !string.IsNullOrEmpty(details) ? $": {details}" : "");
        }
        
        /// <summary>
        /// 记录带异常的组件错误日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="ex">异常</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="details">详情</param>
        public static void LogComponentError(this ILoggerService logger, Exception ex, string component, string action, string? details = null)
        {
            logger.Error(ex, "[{Component}] {Action}{Details}", 
                component, 
                action, 
                !string.IsNullOrEmpty(details) ? $": {details}" : "");
        }
        
        /// <summary>
        /// 记录组件状态改变日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="state">状态</param>
        /// <param name="details">详情</param>
        public static void LogComponentState(this ILoggerService logger, string component, string state, string? details = null)
        {
            logger.Information("[{Component}] {State}{Details}", 
                component, 
                state, 
                !string.IsNullOrEmpty(details) ? $": {details}" : "");
        }
        
        /// <summary>
        /// 记录带参数的组件信息日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="paramName">参数名</param>
        /// <param name="paramValue">参数值</param>
        public static void LogComponentInfoWithParam(this ILoggerService logger, string component, string action, string paramName, object paramValue)
        {
            logger.Information("[{Component}] {Action}: {ParamName}={ParamValue}", 
                component, 
                action, 
                paramName, 
                paramValue);
        }
        
        /// <summary>
        /// 记录带多个参数的组件信息日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="parameters">参数对象</param>
        public static void LogComponentInfoWithParams(this ILoggerService logger, string component, string action, object parameters)
        {
            logger.Information("[{Component}] {Action}: {@Parameters}", 
                component, 
                action, 
                parameters);
        }

        /// <summary>
        /// 记录带参数的组件调试日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="message">消息</param>
        /// <param name="paramValue">参数值</param>
        public static void LogComponentDebug(this ILoggerService logger, string component, string action, string message, object? paramValue)
        {
            logger.Debug("[{Component}] {Action}: {Message} {ParamValue}", 
                component, 
                action, 
                message,
                paramValue ?? "[null]");
        }

        /// <summary>
        /// 记录带参数的组件警告日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="message">消息</param>
        /// <param name="paramValue">参数值</param>
        public static void LogComponentWarning(this ILoggerService logger, string component, string action, string message, object? paramValue)
        {
            logger.Warning("[{Component}] {Action}: {Message} {ParamValue}", 
                component, 
                action, 
                message,
                paramValue ?? "[null]");
        }

        /// <summary>
        /// 记录带参数的组件信息日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="message">消息</param>
        /// <param name="paramValue">参数值</param>
        public static void LogComponentInfo(this ILoggerService logger, string component, string action, string message, object? paramValue)
        {
            logger.Information("[{Component}] {Action}: {Message} {ParamValue}", 
                component, 
                action, 
                message,
                paramValue ?? "[null]");
        }
    }
} 