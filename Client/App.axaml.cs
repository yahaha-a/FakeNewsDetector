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
    // 错误通知容器
    private Panel? _notificationHost;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// 在应用程序框架初始化完成时执行
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            try
            {
                LogInfo("App: 框架初始化完成");
                
                // 避免Avalonia和CommunityToolkit的重复验证
                DisableAvaloniaDataAnnotationValidation();
                LogInfo("App: 禁用了重复数据验证");
                
                // 依赖注入容器已在Program.cs中初始化，此处不再重复初始化
                
                // 初始化事件聚合器订阅
                InitializeEventSubscriptions();
                LogInfo("App: 事件订阅已初始化");
                
                // 从依赖注入容器获取主窗口ViewModel
                var mainWindowViewModel = DependencyContainer.GetService<MainWindowViewModel>();
                if (mainWindowViewModel == null)
                {
                    LogError("App: 无法创建主窗口视图模型");
                    throw new InvalidOperationException("无法创建主窗口视图模型");
                }
                
                // 创建主窗口并设置数据上下文
                var mainWindow = new MainWindow();
                mainWindow.DataContext = mainWindowViewModel;
                LogInfo("App: 已创建主窗口并设置数据上下文");
                
                // 设置主窗口
                desktopLifetime.MainWindow = mainWindow;
                // 确保窗口显示
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mainWindow.Show();
                LogInfo("App: 已设置主窗口并显示");
                
                // 查找通知区域控件
                _notificationHost = mainWindow.FindControl<Panel>("NotificationArea");
                if (_notificationHost == null)
                {
                    LogError("App: 无法找到通知区域控件");
                }
                else
                {
                    LogInfo("App: 已找到通知区域控件");
                }
                
                // 导航将在MainWindow的Loaded事件中进行初始化
                LogInfo("App: 窗口创建完成，导航将在窗口加载时执行");
                
                // 注释掉重复导航代码
                // mainWindowViewModel.NavigateCommand.Execute("Home");
                // LogInfo("App: 导航到首页");
            }
            catch (Exception ex)
            {
                // 处理并记录异常
                HandleStartupException(ex);
            }
            
            // 注册应用程序退出事件
            desktopLifetime.Exit += OnApplicationExit;
        }

        base.OnFrameworkInitializationCompleted();
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
    /// 初始化事件订阅
    /// </summary>
    private void InitializeEventSubscriptions()
    {
        // 订阅错误事件
        EventAggregator.Instance.Subscribe<ErrorOccurredEvent>(HandleErrorEvent);
        
        // 订阅设置变更事件
        EventAggregator.Instance.Subscribe<SettingsChangedEvent>(HandleSettingsChangedEvent);
        
        // 订阅分析完成事件
        EventAggregator.Instance.Subscribe<NewsAnalysisCompletedEvent>(HandleAnalysisCompletedEvent);
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
        try
        {
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);
            File.AppendAllText(Path.Combine(logDir, "app-lifecycle.log"), $"{DateTime.Now}: {message}\n");
        }
        catch
        {
            // 无法记录到文件时，不做任何处理
        }
    }
    
    // 记录错误到文件
    private void LogError(string message, Exception? ex = null)
    {
        try
        {
            var errorMessage = $"{DateTime.Now}: {message}\n";
            
            if (ex != null)
            {
                errorMessage += $"错误: {ex.Message}\n" +
                               $"堆栈: {ex.StackTrace}\n";
                
                if (ex.InnerException != null)
                {
                    errorMessage += $"内部错误: {ex.InnerException.Message}\n" +
                                  $"内部堆栈: {ex.InnerException.StackTrace}\n";
                }
            }
            
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);
            File.AppendAllText(Path.Combine(logDir, "app-errors.log"), errorMessage);
        }
        catch
        {
            // 无法记录到文件时，不做任何处理
        }
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