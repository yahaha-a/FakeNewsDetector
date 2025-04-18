using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using Client.ViewModels;
using Client.Views;
using Client.DependencyInjection;
using Client.Helpers;
using Client.Helpers.Events;
using System.IO;
using Client.Services;
using Avalonia.Styling;
using Avalonia.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Client;

// 添加一个通知常量类，用于保存全局设置
public static class NotificationConstants
{
    /// <summary>
    /// 默认通知持续时间(毫秒)
    /// </summary>
    public static int DefaultDuration { get; set; } = 5000;
}

public partial class App : Application
{
    private readonly ILogger _logger;
    private Panel? _notificationHost;
    private ThemeService? _themeService;
    private bool _isThemeServiceInitialized = false;
    private readonly object _themeLock = new object();

    public App()
    {
        _logger = Log.ForContext<App>();
    }

    public ThemeVariant ThemeVariant
    {
        get => _themeService?.ThemeVariant ?? ThemeVariant.Light;
        set
        {
            if (_themeService != null)
            {
                lock (_themeLock)
                {
                    var oldTheme = _themeService.ThemeVariant;
                    _themeService.ThemeVariant = value;
                    // 通知主题变更
                    OnThemeChanged(oldTheme, value);
                }
            }
        }
    }

    private void OnThemeChanged(ThemeVariant oldTheme, ThemeVariant newTheme)
    {
        // 在UI线程上更新主题
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    // 更新所有窗口的主题
                    foreach (var window in desktop.Windows)
                    {
                        window.RequestedThemeVariant = newTheme;
                    }

                    // 发布主题变更事件
                    EventAggregator.Instance.Publish(new ThemeChangedEvent(oldTheme, newTheme));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "主题更新失败");
                // 尝试回滚主题
                try
                {
                    if (_themeService != null)
                    {
                        _themeService.ThemeVariant = oldTheme;
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.Error(rollbackEx, "主题回滚失败");
                }
            }
        });
    }

    public override void Initialize()
    {
        try
        {
            AvaloniaXamlLoader.Load(this);
            
            // 延迟初始化主题服务
            InitializeThemeServiceAsync();
            
            DataContext = this;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "应用程序初始化失败");
            throw;
        }
    }

    private async void InitializeThemeServiceAsync()
    {
        try
        {
            // 等待应用程序完全初始化
            await Task.Delay(100); // 给应用程序一些时间完成初始化
            
            if (Application.Current == null)
            {
                throw new InvalidOperationException("应用程序未初始化");
            }

            // 初始化主题服务
            _themeService = ThemeService.Instance;
            
            // 订阅系统主题变更
            Application.Current.ActualThemeVariantChanged += OnSystemThemeChanged;
            
            _isThemeServiceInitialized = true;
            
            _logger.Information("主题服务初始化完成");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "主题服务初始化失败");
            // 使用默认主题
            _themeService = ThemeService.Instance;
            _themeService.ThemeVariant = ThemeVariant.Light;
            _isThemeServiceInitialized = true;
        }
    }

    private void OnSystemThemeChanged(object? sender, EventArgs e)
    {
        if (_themeService != null && _themeService.ThemeVariant == ThemeVariant.Default)
        {
            // 如果当前主题是跟随系统，则更新主题
            var systemTheme = Application.Current?.ActualThemeVariant ?? ThemeVariant.Light;
            ThemeVariant = systemTheme;
        }
    }

    /// <summary>
    /// 在应用程序框架初始化完成时执行
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            // 移除表达式验证器，以支持 .NET 原生绑定
            var dataValidationPluginsToRemove = BindingPlugins.DataValidators
                .OfType<DataAnnotationsValidationPlugin>()
                .ToList();
            
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _logger.Information("App: 框架初始化完成");
                
                // 避免Avalonia和CommunityToolkit的重复验证
                DisableAvaloniaDataAnnotationValidation();
                _logger.Information("App: 禁用了重复数据验证");
                
                // 初始化事件聚合器订阅
                InitializeEventSubscriptions();
                _logger.Information("App: 事件订阅已初始化");
                
                // 从依赖注入容器获取主窗口ViewModel
                var mainWindowViewModel = DependencyContainer.GetService<MainWindowViewModel>();
                if (mainWindowViewModel == null)
                {
                    _logger.Error("App: 无法创建主窗口视图模型");
                    throw new InvalidOperationException("无法创建主窗口视图模型");
                }
                
                // 创建主窗口并设置数据上下文
                var mainWindow = new MainWindow();
                mainWindow.DataContext = mainWindowViewModel;
                _logger.Information("App: 已创建主窗口并设置数据上下文");
                
                // 设置主窗口
                desktop.MainWindow = mainWindow;
                // 确保窗口显示
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mainWindow.Show();
                _logger.Information("App: 已设置主窗口并显示");
                
                // 查找通知区域控件
                _notificationHost = mainWindow.FindControl<Panel>("NotificationArea");
                if (_notificationHost == null)
                {
                    _logger.Error("App: 无法找到通知区域控件");
                }
                else
                {
                    _logger.Information("App: 已找到通知区域控件");
                }
                
                // 等待主题服务初始化完成
                if (!_isThemeServiceInitialized)
                {
                    _logger.Warning("App: 主题服务未初始化完成，等待初始化");
                    Task.Run(async () =>
                    {
                        while (!_isThemeServiceInitialized)
                        {
                            await Task.Delay(100);
                        }
                        // 应用初始主题
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (_themeService != null)
                            {
                                mainWindow.RequestedThemeVariant = _themeService.ThemeVariant;
                            }
                        });
                    });
                }
                else
                {
                    // 直接应用主题
                    mainWindow.RequestedThemeVariant = _themeService?.ThemeVariant ?? ThemeVariant.Light;
                }
            }
        }
        catch (Exception ex)
        {
            HandleStartupException(ex);
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// 初始化事件订阅
    /// </summary>
    private void InitializeEventSubscriptions()
    {
        // 订阅错误事件
        EventAggregator.Instance.Subscribe<ErrorOccurredEvent>(HandleErrorEvent);
        
        // 订阅设置变更事件
        EventAggregator.Instance.Subscribe<SettingsChangedEvent>(HandleSettingsChangedEvent);
        
        // 订阅主题变更事件
        EventAggregator.Instance.Subscribe<ThemeChangedEvent>(HandleThemeChangedEvent);
        
        // 订阅分析完成事件
        EventAggregator.Instance.Subscribe<NewsAnalysisCompletedEvent>(HandleAnalysisCompletedEvent);
    }

    private void HandleThemeChangedEvent(ThemeChangedEvent themeEvent)
    {
        _logger.Information($"主题已从 {themeEvent.OldTheme} 切换到 {themeEvent.NewTheme}");
        
        // 可以在这里添加额外的主题变更处理逻辑
        // 例如：更新状态栏、通知用户等
    }

    /// <summary>
    /// 处理启动异常
    /// </summary>
    private void HandleStartupException(Exception ex)
    {
        // 记录异常
        LogError($"App启动异常: {ex.Message}", ex);
        
        // 通知用户
        try
        {
            EventAggregator.Instance.Publish(new ErrorOccurredEvent(
                "应用程序启动失败",
                "初始化过程中发生错误",
                ErrorSeverity.Error,
                ex
            ));
        }
        catch (Exception publishEx)
        {
            LogError($"无法发布错误事件: {publishEx.Message}");
        }
    }

    /// <summary>
    /// 处理错误事件
    /// </summary>
    private void HandleErrorEvent(ErrorOccurredEvent errorEvent)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            // 确保在UI线程上执行
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (_notificationHost != null)
                {
                    // 显示错误通知
                    Views.Controls.ErrorNotification.FromEvent(_notificationHost, errorEvent);
                    LogInfo($"App: 显示了错误通知: {errorEvent.Message}");
                }
                else
                {
                    // 备选错误显示机制
                    LogError("App: 无法显示错误通知，通知区域控件为空");
                    if (desktopLifetime.MainWindow != null)
                    {
                        // 如果无法使用通知区域，则直接在窗口中显示错误
                        try
                        {
                            LogInfo($"App: 尝试使用备选方式显示错误: {errorEvent.Message}");
                            
                            // 创建一个简单的弹窗来显示错误
                            var messageBox = new Avalonia.Controls.Window()
                            {
                                Title = "错误",
                                Width = 400,
                                Height = 200,
                                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                Content = new Avalonia.Controls.StackPanel
                                {
                                    Margin = new Avalonia.Thickness(20),
                                    Spacing = 10,
                                    Children = 
                                    {
                                        new Avalonia.Controls.TextBlock 
                                        { 
                                            Text = errorEvent.Message, 
                                            TextWrapping = Avalonia.Media.TextWrapping.Wrap 
                                        },
                                        new Avalonia.Controls.TextBlock 
                                        { 
                                            Text = errorEvent.Source, 
                                            Foreground = Avalonia.Media.Brushes.Gray,
                                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                                        },
                                        new Avalonia.Controls.Button 
                                        { 
                                            Content = "确定", 
                                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
                                        }
                                    }
                                }
                            };
                            
                            // 设置按钮点击事件关闭窗口
                            if (messageBox.Content is Avalonia.Controls.StackPanel panel && 
                                panel.Children[2] is Avalonia.Controls.Button button)
                            {
                                button.Click += (s, e) => messageBox.Close();
                            }
                            
                            // 显示错误窗口
                            messageBox.ShowDialog(desktopLifetime.MainWindow);
                        }
                        catch (Exception ex)
                        {
                            LogError($"App: 备选错误显示也失败了: {ex.Message}", ex);
                        }
                    }
                }
            });
        }
    }

    /// <summary>
    /// 处理设置变更事件
    /// </summary>
    private void HandleSettingsChangedEvent(SettingsChangedEvent settingsEvent)
    {
        try
        {
            LogInfo($"App: 收到设置变更事件: {settingsEvent.SettingKey}, 值: {settingsEvent.NewValue}");
            
            // 使用字典或switch表达式处理所有可能的设置键
            switch (settingsEvent.SettingKey)
            {
                case "Theme":
                    ApplyThemeChange(settingsEvent.NewValue?.ToString());
                    break;
                    
                case "Language":
                    ApplyLanguageChange(settingsEvent.NewValue?.ToString());
                    break;
                    
                case "FontSize":
                    ApplyFontSizeChange(settingsEvent.NewValue);
                    break;
                    
                case "UseHardwareAcceleration":
                    ApplyHardwareAccelerationChange(settingsEvent.NewValue);
                    break;
                    
                case "UIScale":
                    ApplyUIScaleChange(settingsEvent.NewValue);
                    break;
                    
                case "NotificationDuration":
                    ApplyNotificationDurationChange(settingsEvent.NewValue);
                    break;
                    
                case "AnalysisAccuracy":
                    // 分析精度设置可能不需要立即应用
                    LogInfo($"App: 分析精度设置已更改: {settingsEvent.NewValue}");
                    break;
                    
                default:
                    LogInfo($"App: 收到未处理的设置变更: {settingsEvent.SettingKey}");
                    break;
            }
        }
        catch (Exception ex)
        {
            LogError($"App: 处理设置变更事件失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 应用主题变更
    /// </summary>
    private void ApplyThemeChange(string? themeName)
    {
        if (string.IsNullOrEmpty(themeName))
            return;
            
        LogInfo($"App: 正在应用主题变更: {themeName}");
        
        try
        {
            // 在UI线程上安全地应用主题变更
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (themeName.Equals("Light", StringComparison.OrdinalIgnoreCase))
                {
                    RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
                }
                else if (themeName.Equals("Dark", StringComparison.OrdinalIgnoreCase))
                {
                    RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
                }
                else if (themeName.Equals("System", StringComparison.OrdinalIgnoreCase))
                {
                    RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Default;
                }
                
                LogInfo($"App: 主题已更改为: {RequestedThemeVariant?.Key ?? "Default"}");
            });
        }
        catch (Exception ex)
        {
            LogError($"App: 应用主题变更失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 应用语言变更
    /// </summary>
    private void ApplyLanguageChange(string? languageCode)
    {
        if (string.IsNullOrEmpty(languageCode))
            return;
            
        LogInfo($"App: 正在应用语言变更: {languageCode}");
        
        try
        {
            // 这里可以实现语言切换逻辑
            // 由于没有完整的国际化架构，这里仅作记录
            LogInfo($"App: 语言已更改为: {languageCode}");
        }
        catch (Exception ex)
        {
            LogError($"App: 应用语言变更失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 应用字体大小变更
    /// </summary>
    private void ApplyFontSizeChange(object? fontSize)
    {
        LogInfo($"App: 正在应用字体大小变更: {fontSize}");
        
        try
        {
            // 这里可以实现字体大小变更逻辑
            LogInfo($"App: 字体大小已更改为: {fontSize}");
        }
        catch (Exception ex)
        {
            LogError($"App: 应用字体大小变更失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 应用硬件加速设置变更
    /// </summary>
    private void ApplyHardwareAccelerationChange(object? useHardwareAcceleration)
    {
        LogInfo($"App: 正在应用硬件加速设置变更: {useHardwareAcceleration}");
        
        try
        {
            // 硬件加速设置可能需要重启应用生效
            LogInfo($"App: 硬件加速设置已更改为: {useHardwareAcceleration}，可能需要重启应用生效");
        }
        catch (Exception ex)
        {
            LogError($"App: 应用硬件加速设置变更失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 应用UI缩放设置变更
    /// </summary>
    private void ApplyUIScaleChange(object? scale)
    {
        LogInfo($"App: 正在应用UI缩放设置变更: {scale}");
        
        try
        {
            // 这里可以实现UI缩放逻辑
            LogInfo($"App: UI缩放已更改为: {scale}");
        }
        catch (Exception ex)
        {
            LogError($"App: 应用UI缩放变更失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 应用通知持续时间变更
    /// </summary>
    private void ApplyNotificationDurationChange(object? duration)
    {
        LogInfo($"App: 正在应用通知持续时间变更: {duration}");
        
        try
        {
            // 实现通知持续时间变更逻辑
            if (duration is int durationMs && durationMs > 0)
            {
                // 设置全局的通知持续时间
                NotificationConstants.DefaultDuration = durationMs;
                LogInfo($"App: 通知持续时间已更改为: {durationMs}ms");
            }
        }
        catch (Exception ex)
        {
            LogError($"App: 应用通知持续时间变更失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 处理分析完成事件
    /// </summary>
    private void HandleAnalysisCompletedEvent(NewsAnalysisCompletedEvent analysisEvent)
    {
        // 获取真实性得分，用1减去得到假新闻可能性
        double fakeNewsProbability = 1.0 - analysisEvent.Result.TruthScore;
        string newsUrl = analysisEvent.Result.NewsItem?.Url ?? "未知URL";
        
        // 如果需要，可以向用户显示分析完成通知
        if (_notificationHost != null && fakeNewsProbability > 0.7)
        {
            // 如果是高概率的假新闻，则显示警告通知
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (_notificationHost != null)
                {
                    // 创建通知并添加到面板
                    Views.Controls.ErrorNotification.AddToPanel(
                        _notificationHost,
                        "检测到可能的假新闻",
                        $"分析结果表明这可能是假新闻 (概率: {fakeNewsProbability:P0})",
                        Constants.NotificationSeverity.Warning  // 使用Constants.NotificationSeverity枚举
                    );
                    LogInfo($"App: 显示了假新闻警告通知，概率: {fakeNewsProbability:P0}");
                }
            });
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // 获取要删除的插件数组
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // 删除每个找到的条目
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
    
    // 记录信息到文件
    private void LogInfo(string message)
    {
        var action = DetermineAction(message);
        var details = ExtractDetails(message);
        
        SerilogLoggerService.Instance.LogComponentInfo(
            LogContext.Components.App, 
            action,
            details);
    }
    
    // 记录错误到文件
    private void LogError(string message, Exception? ex = null)
    {
        var action = DetermineAction(message);
        var details = ExtractDetails(message);
        
        if (ex != null)
        {
            SerilogLoggerService.Instance.LogComponentError(
                ex,
                LogContext.Components.App, 
                action,
                details);
        }
        else
        {
            SerilogLoggerService.Instance.LogComponentError(
                LogContext.Components.App, 
                action,
                details);
        }
    }

    // 从消息中确定操作类型
    private string DetermineAction(string message)
    {
        if (message.Contains("初始化")) return LogContext.Actions.Initialize;
        if (message.Contains("启动")) return LogContext.Actions.Start;
        if (message.Contains("配置")) return LogContext.Actions.Configure;
        if (message.Contains("设置变更") || message.Contains("设置已")) return LogContext.Actions.Configure;
        if (message.Contains("加载")) return LogContext.Actions.Load;
        if (message.Contains("保存")) return LogContext.Actions.Save;
        if (message.Contains("清理")) return LogContext.Actions.Cleanup;
        if (message.Contains("关闭")) return LogContext.Actions.Shutdown;
        if (message.Contains("主题")) return LogContext.Actions.ChangeTheme;
        if (message.Contains("语言")) return LogContext.Actions.ChangeLanguage;
        if (message.Contains("导航")) return LogContext.Actions.Navigate;
        if (message.Contains("处理")) return LogContext.Actions.Process;
        if (message.Contains("显示")) return LogContext.Actions.Display;
        if (message.Contains("创建")) return LogContext.Actions.Create;
        if (message.Contains("更新")) return LogContext.Actions.Update;
        if (message.Contains("删除")) return LogContext.Actions.Delete;
        if (message.Contains("执行")) return LogContext.Actions.Execute;
        if (message.Contains("验证")) return LogContext.Actions.Validate;
        return LogContext.Actions.Process; // 默认操作
    }

    // 从消息中提取详细信息
    private string ExtractDetails(string message)
    {
        // 移除旧格式的前缀
        if (message.StartsWith("App: "))
        {
            message = message.Substring(5);
        }
        
        return message;
    }

    /// <summary>
    /// 应用程序退出时执行清理
    /// </summary>
    private void OnApplicationExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        LogInfo("App: 执行应用程序关闭清理");
        
        try
        {
            // 在UI线程上安全地执行清理
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    // 取消事件订阅
                    EventAggregator.Instance.Unsubscribe<ErrorOccurredEvent>(HandleErrorEvent);
                    EventAggregator.Instance.Unsubscribe<SettingsChangedEvent>(HandleSettingsChangedEvent);
                    EventAggregator.Instance.Unsubscribe<NewsAnalysisCompletedEvent>(HandleAnalysisCompletedEvent);
                    LogInfo("App: 已清理事件订阅");
                }
                catch (Exception ex)
                {
                    LogError("App: 取消事件订阅时发生错误", ex);
                }
            }, Avalonia.Threading.DispatcherPriority.Send);
            
            // 释放依赖注入容器 (这个操作可以在任何线程上执行)
            if (DependencyContainer.IsInitialized)
            {
                DependencyContainer.Dispose();
                LogInfo("App: 已释放依赖注入容器");
            }
        }
        catch (Exception ex)
        {
            LogError("App: 关闭清理过程中发生错误", ex);
        }
    }
}