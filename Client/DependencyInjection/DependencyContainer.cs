using Client.Helpers;
using Client.Services;
using Client.Services.Interfaces;
using Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Client.DependencyInjection
{
    /// <summary>
    /// 依赖注入容器
    /// </summary>
    public static class DependencyContainer
    {
        private static IServiceProvider? _serviceProvider;
        private static readonly object _lockObject = new object();
        private static bool _isInitialized = false;
        
        /// <summary>
        /// 获取或创建服务提供器
        /// </summary>
        public static IServiceProvider ServiceProvider => 
            _serviceProvider ?? throw new InvalidOperationException("依赖注入容器未初始化，请先调用Initialize方法");
        
        /// <summary>
        /// 判断容器是否已初始化
        /// </summary>
        public static bool IsInitialized => _isInitialized && _serviceProvider != null;
        
        /// <summary>
        /// 初始化依赖注入容器
        /// </summary>
        public static void Initialize()
        {
            // 防止重复初始化
            lock (_lockObject)
            {
                if (_isInitialized)
                {
                    LogInfo("依赖注入容器已经初始化，跳过重复初始化");
                    return;
                }
                
                try
                {
                    LogInfo("开始初始化依赖注入容器");
                    var services = new ServiceCollection();
                    
                    // 注册日志服务 - 单例模式（优先注册，便于其他服务使用）
                    services.AddSingleton<ILoggerService>(sp => SerilogLoggerService.Instance);
                    
                    // 注册设置服务 - 单例模式
                    services.AddSingleton<ISettingsService, SettingsService>();
                    
                    // 注册新闻服务 - 单例模式
                    services.AddSingleton<INewsService, MockNewsService>();
                    
                    // 注册分析服务 - 单例模式，使用工厂获取依赖
                    services.AddSingleton<IAnalysisService>(sp => {
                        var newsService = sp.GetRequiredService<INewsService>();
                        return new MockAnalysisService(newsService);
                    });
                    
                    // 注册导航服务 - 单例模式
                    services.AddSingleton<INavigationService, NavigationService>();
                    
                    // 注册视图模型
                    RegisterViewModels(services);
                    
                    // 构建服务提供器
                    _serviceProvider = services.BuildServiceProvider();
                    _isInitialized = true;
                    LogInfo("依赖注入容器初始化完成");
                }
                catch (Exception ex)
                {
                    _isInitialized = false;
                    _serviceProvider = null;
                    LogError("依赖注入容器初始化失败", ex);
                    throw;
                }
            }
        }
        
        /// <summary>
        /// 注册视图模型
        /// </summary>
        private static void RegisterViewModels(IServiceCollection services)
        {
            // 注册主窗口ViewModel - 单例模式
            services.AddSingleton<MainWindowViewModel>();
            
            // 注册页面视图模型 - 瞬态模式，每次导航创建新实例
            services.AddTransient<HomeViewModel>();
            services.AddTransient<DetectionViewModel>();
            
            // 注册设置页面视图模型
            services.AddTransient<SettingsViewModel>();
            
            // 注册历史记录页面视图模型
            services.AddTransient<HistoryViewModel>();
            
            // 注册统计分析页面视图模型
            services.AddTransient<StatisticsViewModel>();
            
            // 注册测试页面ViewModel，手动注入依赖
            services.AddTransient<TestViewModel>(sp => {
                var analysisService = sp.GetRequiredService<IAnalysisService>();
                var newsService = sp.GetRequiredService<INewsService>();
                return new TestViewModel(analysisService, newsService);
            });
        }
        
        /// <summary>
        /// 从容器获取服务实例，如果容器未初始化则先初始化
        /// </summary>
        public static T GetService<T>() where T : notnull
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            
            return ServiceProvider.GetRequiredService<T>();
        }
        
        /// <summary>
        /// 释放服务提供器及其管理的所有资源
        /// </summary>
        public static void Dispose()
        {
            lock (_lockObject)
            {
                if (_serviceProvider is IDisposable disposable)
                {
                    try
                    {
                        LogInfo("开始释放依赖注入容器资源");
                        disposable.Dispose();
                        LogInfo("依赖注入容器资源释放完成");
                    }
                    catch (Exception ex)
                    {
                        LogError("释放依赖注入容器资源时发生错误", ex);
                    }
                    finally
                    {
                        _serviceProvider = null;
                        _isInitialized = false;
                    }
                }
            }
        }
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        private static void LogInfo(string message)
        {
            string action;
            
            if (message.Contains("初始化")) 
                action = LogContext.Actions.Initialize;
            else if (message.Contains("释放"))
                action = LogContext.Actions.Cleanup;
            else
                action = LogContext.Actions.Process;
        
            SerilogLoggerService.Instance.LogComponentInfo(
                LogContext.Components.DependencyContainer, 
                action,
                message);
        }
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        private static void LogError(string message, Exception ex)
        {
            string action;
            
            if (message.Contains("初始化")) 
                action = LogContext.Actions.Initialize;
            else if (message.Contains("释放"))
                action = LogContext.Actions.Cleanup;
            else
                action = LogContext.Actions.Process;
        
            SerilogLoggerService.Instance.LogComponentError(
                ex,
                LogContext.Components.DependencyContainer, 
                action,
                message);
        }
    }
} 