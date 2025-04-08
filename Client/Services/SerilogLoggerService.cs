using Client.Services.Interfaces;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.IO;

namespace Client.Services
{
    /// <summary>
    /// 基于Serilog的日志服务实现
    /// </summary>
    public class SerilogLoggerService : ILoggerService
    {
        private readonly LoggingLevelSwitch _levelSwitch;
        private readonly ILogger _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logFilePath">日志文件路径，默认为"logs/app.log"</param>
        /// <param name="minimumLevel">最小日志级别，默认为"Information"</param>
        public SerilogLoggerService(string logFilePath = "logs/app.log", string minimumLevel = "Information")
        {
            // 初始化日志级别
            _levelSwitch = new LoggingLevelSwitch(ParseLogLevel(minimumLevel));

            // 确保日志目录存在
            var logDirectory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // 配置Serilog
            _logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(_levelSwitch)
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 7)
                .Enrich.FromLogContext()
                .CreateLogger();

            // 记录日志系统初始化完成
            Information("日志系统已初始化，最小日志级别: {Level}", minimumLevel);
        }

        /// <inheritdoc/>
        public void Debug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }

        /// <inheritdoc/>
        public void Information(string message, params object[] args)
        {
            _logger.Information(message, args);
        }

        /// <inheritdoc/>
        public void Warning(string message, params object[] args)
        {
            _logger.Warning(message, args);
        }

        /// <inheritdoc/>
        public void Error(string message, params object[] args)
        {
            _logger.Error(message, args);
        }

        /// <inheritdoc/>
        public void Error(Exception exception, string message, params object[] args)
        {
            _logger.Error(exception, message, args);
        }

        /// <inheritdoc/>
        public void Fatal(string message, params object[] args)
        {
            _logger.Fatal(message, args);
        }

        /// <inheritdoc/>
        public void Fatal(Exception exception, string message, params object[] args)
        {
            _logger.Fatal(exception, message, args);
        }

        /// <inheritdoc/>
        public void SetMinimumLevel(string level)
        {
            _levelSwitch.MinimumLevel = ParseLogLevel(level);
            Information("日志级别已更改为: {Level}", level);
        }

        /// <summary>
        /// 解析日志级别字符串为LogEventLevel
        /// </summary>
        /// <param name="level">日志级别字符串</param>
        /// <returns>LogEventLevel枚举值</returns>
        private static LogEventLevel ParseLogLevel(string level)
        {
            return level?.ToLower() switch
            {
                "debug" => LogEventLevel.Debug,
                "verbose" => LogEventLevel.Verbose,
                "information" => LogEventLevel.Information,
                "info" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "warn" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }
    }
} 