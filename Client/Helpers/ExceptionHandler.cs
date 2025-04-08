using Avalonia.Threading;
using Client.DependencyInjection;
using Client.Helpers.Events;
using Client.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Client.Helpers
{
    /// <summary>
    /// 全局异常处理器
    /// </summary>
    public static class ExceptionHandler
    {
        private static ILoggerService? _logger;
        
        /// <summary>
        /// 日志服务
        /// </summary>
        public static ILoggerService Logger
        {
            get
            {
                if (_logger == null)
                {
                    try
                    {
                        _logger = DependencyContainer.GetService<ILoggerService>();
                    }
                    catch
                    {
                        // 如果依赖注入容器未初始化，使用控制台临时记录
                        // 这种情况通常只会在应用初始化早期阶段发生
                        return new Services.SerilogLoggerService();
                    }
                }
                return _logger;
            }
        }
        
        /// <summary>
        /// 初始化全局异常处理
        /// </summary>
        public static void Initialize()
        {
            // 处理未捕获的异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            // 处理未捕获的任务异常
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            
            // 处理分发器未处理异常
            Dispatcher.UIThread.UnhandledException += UIThread_UnhandledException;
            
            // 记录初始化完成日志
            Logger.Information("全局异常处理已初始化");
        }

        /// <summary>
        /// 处理并报告异常
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="source">异常来源</param>
        /// <param name="severity">严重程度</param>
        public static void HandleException(Exception ex, string source, ErrorSeverity severity = ErrorSeverity.Error)
        {
            if (ex == null)
                return;
                
            // 记录异常详情
            LogException(ex, source);
            
            // 发布错误事件，通知UI显示错误
            var errorEvent = new ErrorOccurredEvent(ex.Message, source, severity, ex);
            EventAggregator.Instance.Publish(errorEvent);
        }

        /// <summary>
        /// 记录异常详情
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="source">来源</param>
        private static void LogException(Exception exception, string source)
        {
            if (exception.InnerException != null)
            {
                // 如果有内部异常，记录更详细的信息
                Logger.LogComponentError(
                    exception,
                    source,
                    LogContext.Actions.Process,
                    "异常处理",
                    new { InnerException = exception.InnerException.Message });
            }
            else
            {
                // 记录基本异常信息
                Logger.LogComponentError(
                    exception,
                    source,
                    LogContext.Actions.Process,
                    "异常处理");
            }
        }

        /// <summary>
        /// 处理 UI 线程未捕获的异常
        /// </summary>
        private static void UIThread_UnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception, "UI线程异常", ErrorSeverity.Critical);
                
                // 标记为已处理，避免应用崩溃
                e.Handled = true;
            }
            catch (Exception ex)
            {
                // 防止处理过程中的异常导致递归
                Logger.Fatal(ex, "处理UI线程异常时出错");
            }
        }

        /// <summary>
        /// 处理未观察到的任务异常
        /// </summary>
        private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception, "未观察的任务异常", ErrorSeverity.Error);
                
                // 标记为已观察，避免崩溃
                e.SetObserved();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "处理任务异常时出错");
            }
        }

        /// <summary>
        /// 处理应用域未捕获的异常
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception exception)
                {
                    HandleException(exception, "应用域未处理异常", ErrorSeverity.Critical);
                }
                else
                {
                    Logger.Fatal("未知异常对象: {ExceptionObject}", e.ExceptionObject);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "处理应用域异常时出错");
            }
        }
        
        /// <summary>
        /// 包装执行异步操作，自动处理异常
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <param name="source">操作来源</param>
        /// <returns>异步任务</returns>
        public static async Task WrapAsync(Func<Task> action, string source)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                HandleException(ex, source);
            }
        }
        
        /// <summary>
        /// 包装执行异步操作，自动处理异常并返回结果
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="action">要执行的操作</param>
        /// <param name="source">操作来源</param>
        /// <param name="defaultValue">发生异常时的默认返回值</param>
        /// <returns>操作结果或默认值</returns>
        public static async Task<T?> WrapAsync<T>(Func<Task<T>> action, string source, T? defaultValue = default)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                HandleException(ex, source);
                return defaultValue;
            }
        }
    }
} 