using Client.Services.Interfaces;
using System;

namespace Client.Helpers
{
    /// <summary>
    /// 日志服务扩展方法
    /// </summary>
    public static class LoggerExtensions
    {
        // 基础日志方法 ===================================

        /// <summary>
        /// 记录组件信息日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="details">详情</param>
        public static void LogComponentInfo(this ILoggerService logger, string component, string action, string? details = null)
        {
            logger.Information("[{Component}] {Action}: {Details}", 
                component, 
                action, 
                details ?? "");
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
            logger.Debug("[{Component}] {Action}: {Details}", 
                component, 
                action, 
                details ?? "");
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
            logger.Warning("[{Component}] {Action}: {Details}", 
                component, 
                action, 
                details ?? "");
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
            logger.Error("[{Component}] {Action}: {Details}", 
                component, 
                action, 
                details ?? "");
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
            logger.Error(ex, "[{Component}] {Action}: {Details}", 
                component, 
                action, 
                details ?? "");
        }
        
        // 带有结构化数据的日志方法 ===================================
        
        /// <summary>
        /// 记录带结构化数据的组件信息日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="details">详情</param>
        /// <param name="data">结构化数据</param>
        public static void LogComponentInfo(this ILoggerService logger, string component, string action, string details, object? data)
        {
            logger.Information("[{Component}] {Action}: {Details} {@Data}", 
                component, 
                action, 
                details,
                data ?? new { Value = "[null]" });
        }
        
        /// <summary>
        /// 记录带结构化数据的组件调试日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="details">详情</param>
        /// <param name="data">结构化数据</param>
        public static void LogComponentDebug(this ILoggerService logger, string component, string action, string details, object? data)
        {
            logger.Debug("[{Component}] {Action}: {Details} {@Data}", 
                component, 
                action, 
                details,
                data ?? new { Value = "[null]" });
        }
        
        /// <summary>
        /// 记录带结构化数据的组件警告日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="details">详情</param>
        /// <param name="data">结构化数据</param>
        public static void LogComponentWarning(this ILoggerService logger, string component, string action, string details, object? data)
        {
            logger.Warning("[{Component}] {Action}: {Details} {@Data}", 
                component, 
                action, 
                details,
                data ?? new { Value = "[null]" });
        }
        
        /// <summary>
        /// 记录带结构化数据的组件错误日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="details">详情</param>
        /// <param name="data">结构化数据</param>
        public static void LogComponentError(this ILoggerService logger, string component, string action, string details, object? data)
        {
            logger.Error("[{Component}] {Action}: {Details} {@Data}", 
                component, 
                action, 
                details,
                data ?? new { Value = "[null]" });
        }
        
        /// <summary>
        /// 记录带异常和结构化数据的组件错误日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="ex">异常</param>
        /// <param name="component">组件名称</param>
        /// <param name="action">操作</param>
        /// <param name="details">详情</param>
        /// <param name="data">结构化数据</param>
        public static void LogComponentError(this ILoggerService logger, Exception ex, string component, string action, string details, object? data)
        {
            logger.Error(ex, "[{Component}] {Action}: {Details} {@Data}", 
                component, 
                action, 
                details,
                data ?? new { Value = "[null]" });
        }
        
        // 状态日志方法 ===================================
        
        /// <summary>
        /// 记录组件状态改变日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="state">状态</param>
        /// <param name="details">详情</param>
        public static void LogComponentState(this ILoggerService logger, string component, string state, string? details = null)
        {
            logger.Information("[{Component}] State: {State} {Details}", 
                component, 
                state, 
                details ?? "");
        }
        
        /// <summary>
        /// 记录带结构化数据的组件状态改变日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="component">组件名称</param>
        /// <param name="state">状态</param>
        /// <param name="details">详情</param>
        /// <param name="data">结构化数据</param>
        public static void LogComponentState(this ILoggerService logger, string component, string state, string details, object? data)
        {
            logger.Information("[{Component}] State: {State} {Details} {@Data}", 
                component, 
                state, 
                details,
                data ?? new { Value = "[null]" });
        }
        
        // 已废弃的方法 (保留向后兼容性) ===================================
        
        /// <summary>
        /// 记录带参数的组件信息日志 (已废弃，请使用 LogComponentInfo 的重载版本)
        /// </summary>
        [Obsolete("此方法已废弃，请使用 LogComponentInfo 的重载版本", false)]
        public static void LogComponentInfoWithParam(this ILoggerService logger, string component, string action, string paramName, object? paramValue)
        {
            logger.Information("[{Component}] {Action}: {ParamName}={ParamValue}", 
                component, 
                action, 
                paramName, 
                paramValue ?? "[null]");
        }
        
        /// <summary>
        /// 记录带多个参数的组件信息日志 (已废弃，请使用 LogComponentInfo 的重载版本)
        /// </summary>
        [Obsolete("此方法已废弃，请使用 LogComponentInfo 的重载版本", false)]
        public static void LogComponentInfoWithParams(this ILoggerService logger, string component, string action, object? parameters)
        {
            logger.Information("[{Component}] {Action}: {@Parameters}", 
                component, 
                action, 
                parameters ?? new { Value = "[null]" });
        }
    }
} 