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
using Client.Views;

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
            // 记录应用启动信息
            LogInfo("应用程序启动...");
            
            // 初始化全局异常处理
            InitializeGlobalExceptionHandling();
            LogInfo("全局异常处理已初始化");
            
            // 配置依赖注入容器
            DependencyContainer.Initialize();
            LogInfo("依赖注入容器已初始化");
            
            // 启动Avalonia应用程序
            try 
            {
                // 使用最简单的方式启动，避免平台特定问题
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
                
                // 记录启动成功
                LogInfo("Avalonia应用程序启动成功");
            }
            catch (Exception ex)
            {
                LogError($"启动UI框架时发生错误: {ex.Message}", ex);
                
                // 尝试以后备方式启动
                BuildFallbackAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
            }
            
            // 清理资源
            _exitEvent.WaitOne();
            LogInfo("应用程序正常退出");
        }
        catch (Exception ex)
        {
            // 记录致命错误
            LogError($"应用程序启动失败: {ex.Message}", ex);
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
                LogError("未处理的应用程序域异常", ex);
            }
            else
            {
                LogError($"未处理的非Exception类型异常: {e.ExceptionObject}", null);
            }
        };
        
        // 设置线程异常处理程序
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            LogError("未观察到的任务异常", e.Exception);
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
            LogInfo("构建Avalonia应用");
            return AppBuilder.Configure<App>()
                .LogToTrace()
                .WithInterFont()
                .UseSkia()
                .UseWin32()
                .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 });
        }
        catch (Exception ex)
        {
            LogError($"构建Avalonia应用时发生错误: {ex.Message}", ex);
            throw;
        }
    }
    
    /// <summary>
    /// 构建后备Avalonia应用（当主方法失败时使用）
    /// </summary>
    public static AppBuilder BuildFallbackAvaloniaApp()
    {
        LogInfo("尝试使用后备方式构建Avalonia应用");
        return AppBuilder.Configure<App>()
            .LogToTrace()
            .WithInterFont()
            .UseSkia()
            .UseWin32()
            .With(new SkiaOptions { MaxGpuResourceSizeBytes = 0 });
    }
    
    // 记录信息到文件
    private static void LogInfo(string message)
    {
        try
        {
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);
            File.AppendAllText(Path.Combine(logDir, "startup.log"), $"{DateTime.Now}: {message}\n");
        }
        catch
        {
            // 无法记录到文件时，不做任何处理
        }
    }
    
    // 记录错误到文件
    private static void LogError(string message, Exception? ex)
    {
        try
        {
            var errorMessage = $"{DateTime.Now}: {message}\n" +
                               $"错误: {ex?.Message ?? "未知错误"}\n" +
                               $"堆栈: {ex?.StackTrace ?? "无堆栈信息"}\n";
            
            if (ex?.InnerException != null)
            {
                errorMessage += $"内部错误: {ex.InnerException.Message}\n" +
                                $"内部堆栈: {ex.InnerException.StackTrace}\n";
            }
            
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);
            File.AppendAllText(Path.Combine(logDir, "error.log"), errorMessage);
        }
        catch
        {
            // 无法记录到文件时，不做任何处理
        }
    }
}
