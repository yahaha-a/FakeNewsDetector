using System;
using System.IO;
using System.Threading;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Skia;
using Avalonia.Win32;

using Client.DependencyInjection;
using Client.Helpers;
using Client.Services;
using Client.Services.Interfaces;
using Client.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Client;

/// <summary>
/// 程序入口类
/// </summary>
class Program
{
    // 程序退出事件
    private static ManualResetEvent _exitEvent = new ManualResetEvent(false);
    
    /// <summary>
    /// 应用程序入口点
    /// </summary>
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // 初始化日志服务
            var logger = SerilogLoggerService.CreateInstance();
            
            // 记录应用启动信息
            logger.LogComponentInfo(LogContext.Components.Program, LogContext.Actions.Start, "应用程序启动");
            
            // 初始化全局异常处理
            InitializeGlobalExceptionHandling();
            logger.LogComponentInfo(LogContext.Components.Program, LogContext.Actions.Initialize, "全局异常处理已初始化");
            
            // 配置依赖注入容器，传入预初始化的日志服务
            DependencyContainer.Initialize(logger);
            
            // 从依赖注入容器获取日志服务
            var diLogger = DependencyContainer.GetService<ILoggerService>();
            diLogger.LogComponentInfo(LogContext.Components.Program, LogContext.Actions.Initialize, "依赖注入容器已初始化");
            
            // 启动Avalonia应用程序
            try 
            {
                // 使用最简单的方式启动，避免平台特定问题
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
                
                // 记录启动成功
                diLogger.LogComponentInfo(LogContext.Components.Program, LogContext.Actions.Start, "Avalonia应用程序启动成功");
            }
            catch (Exception ex)
            {
                diLogger.LogComponentError(ex, LogContext.Components.Program, LogContext.Actions.Start, "启动UI框架失败");
                
                // 尝试以后备方式启动
                BuildFallbackAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
            }
            
            // 清理资源
            _exitEvent.WaitOne();
            diLogger.LogComponentInfo(LogContext.Components.Program, LogContext.Actions.Shutdown, "应用程序正常退出");
        }
        catch (Exception ex)
        {
            // 记录致命错误
            // 这里无法使用依赖注入，因为可能在初始化过程中失败
            SerilogLoggerService.CreateInstance().LogComponentError(ex, LogContext.Components.Program, LogContext.Actions.Start, "应用程序启动失败");
        }
    }
    
    /// <summary>
    /// 初始化全局异常处理
    /// </summary>
    private static void InitializeGlobalExceptionHandling()
    {
        // 设置未处理异常处理程序
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogError(LogContext.Actions.Process, "未处理的应用程序域异常", ex);
            }
            else
            {
                LogError(LogContext.Actions.Process, "未处理的非Exception类型异常", null);
            }
        };
        
        // 设置线程异常处理程序
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            LogError(LogContext.Actions.Process, "未观察到的任务异常", e.Exception);
            e.SetObserved(); // 标记为已观察，防止应用程序崩溃
        };
    }
    
    /// <summary>
    /// 构建Avalonia应用
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
    {
        try
        {
            LogInfo(LogContext.Actions.Configure, "构建Avalonia应用");
            return AppBuilder.Configure<App>()
                .LogToTrace()
                .WithInterFont()
                .UseSkia()
                .UseWin32()
                .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 });
        }
        catch (Exception ex)
        {
            LogError(LogContext.Actions.Configure, "构建Avalonia应用失败", ex);
            throw;
        }
    }
    
    /// <summary>
    /// 构建后备Avalonia应用（当主方法失败时使用）
    /// </summary>
    public static AppBuilder BuildFallbackAvaloniaApp()
    {
        LogInfo(LogContext.Actions.Configure, "使用后备方式构建Avalonia应用");
        return AppBuilder.Configure<App>()
            .LogToTrace()
            .WithInterFont()
            .UseSkia()
            .UseWin32()
            .With(new SkiaOptions { MaxGpuResourceSizeBytes = 0 });
    }
    
    // 记录信息到文件
    private static void LogInfo(string action, string? details = null)
    {
        if (DependencyContainer.IsInitialized)
        {
            var logger = DependencyContainer.GetService<ILoggerService>();
            logger.LogComponentInfo(
                LogContext.Components.Program, 
                action,
                details);
        }
        else
        {
            SerilogLoggerService.CreateInstance().LogComponentInfo(
                LogContext.Components.Program, 
                action,
                details);
        }
    }
    
    // 记录错误到文件
    private static void LogError(string action, string details, Exception? ex)
    {
        if (DependencyContainer.IsInitialized)
        {
            var logger = DependencyContainer.GetService<ILoggerService>();
            if (ex != null)
            {
                logger.LogComponentError(
                    ex,
                    LogContext.Components.Program, 
                    action,
                    details);
            }
            else
            {
                logger.LogComponentError(
                    LogContext.Components.Program, 
                    action,
                    details);
            }
        }
        else
        {
            var logger = SerilogLoggerService.CreateInstance();
            if (ex != null)
            {
                logger.LogComponentError(
                    ex,
                    LogContext.Components.Program, 
                    action,
                    details);
            }
            else
            {
                logger.LogComponentError(
                    LogContext.Components.Program, 
                    action,
                    details);
            }
        }
    }
}
