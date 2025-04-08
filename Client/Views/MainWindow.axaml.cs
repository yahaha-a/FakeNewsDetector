using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Client.DependencyInjection;
using Client.Helpers;
using Client.Services;
using Client.Services.Interfaces;
using Client.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Views;

/// <summary>
/// 主窗口
/// </summary>
public partial class MainWindow : Window
{
    // 导航初始化状态标记
    private bool _navigationInitialized = false;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        
        // 注册事件
        this.Loaded += MainWindow_Loaded;
        this.DataContextChanged += MainWindow_DataContextChanged;
    }
    
    /// <summary>
    /// 当数据上下文改变时
    /// </summary>
    private void MainWindow_DataContextChanged(object? sender, EventArgs e)
    {
        // 确保导航命令能够正常工作
        CheckAndInitializeViewModel();
        
        // 尝试再次设置导航目标
        SetupNavigationTarget();
    }
    
    /// <summary>
    /// 当窗口加载时
    /// </summary>
    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            LogDebugInfo(LogContext.Actions.Load, "窗口加载事件触发");
            
            // 设置导航目标
            SetupNavigationTarget();
            CheckAndInitializeViewModel();
            
            // 如果尚未初始化导航，则执行导航初始化
            if (!_navigationInitialized)
            {
                _navigationInitialized = true;
                LogDebugInfo(LogContext.Actions.Initialize, "正在初始化导航 - 这是唯一的导航初始化入口");
                
                // 立即执行导航，不使用延迟
                if (DataContext is MainWindowViewModel viewModel)
                {
                    // 使用UI线程调度器执行导航并正确等待
                    // 注意：Loaded 事件处理程序不能是异步的，所以我们不能直接使用 await
                    // 但我们可以使用 Task.Run 来避免界面冻结
                    Task.Run(async () =>
                    {
                        try
                        {
                            // 使用 InvokeAsync 代替 Post，并等待其完成
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                try
                                {
                                    // 调用视图模型的导航方法
                                    await viewModel.InitializeNavigation();
                                    LogDebugInfo(LogContext.Actions.Initialize, "导航初始化成功完成");
                                }
                                catch (Exception ex)
                                {
                                    // 记录错误但不重置导航状态，避免多次重试
                                    LogError(LogContext.Actions.Initialize, "导航初始化出错", ex);
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            // 处理在 Task.Run 或 InvokeAsync 中可能发生的异常
                            Dispatcher.UIThread.Post(() => 
                            {
                                LogError(LogContext.Actions.Initialize, "导航初始化异步操作失败", ex);
                                _navigationInitialized = false; // 重置标志以允许重试
                            });
                        }
                    });
                }
                else
                {
                    LogDebugInfo(LogContext.Actions.Initialize, "无法导航：视图模型未初始化");
                    _navigationInitialized = false;
                }
            }
            else
            {
                LogDebugInfo(LogContext.Actions.Initialize, "导航已经初始化，跳过重复初始化");
            }
        }
        catch (Exception ex)
        {
            LogError(LogContext.Actions.Load, "窗口加载事件处理失败", ex);
            _navigationInitialized = false;
        }
    }
    
    /// <summary>
    /// 设置导航目标
    /// </summary>
    private void SetupNavigationTarget()
    {
        try
        {
            // 获取导航服务
            var navigationService = DependencyContainer.GetService<INavigationService>();
            if (navigationService == null)
            {
                LogDebugInfo(LogContext.Actions.Configure, "错误：找不到导航服务，请检查依赖注入容器配置");
                return;
            }
            
            // 查找名为 "PageContent" 的内容控件
            var contentControl = this.FindControl<ContentControl>("PageContent");
            if (contentControl == null)
            {
                LogDebugInfo(LogContext.Actions.Configure, "错误：找不到PageContent控件，请检查XAML布局");
                return;
            }
            
            LogDebugInfo(LogContext.Actions.Configure, "找到PageContent控件，准备设置导航目标");
            
            // 设置导航目标
            navigationService.SetNavigationTarget(contentControl);
            LogDebugInfo(LogContext.Actions.Configure, "导航目标设置成功");
        }
        catch (Exception ex)
        {
            LogError(LogContext.Actions.Configure, "设置导航目标失败", ex);
        }
    }
    
    /// <summary>
    /// 检查并初始化视图模型
    /// </summary>
    private void CheckAndInitializeViewModel()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            // 确保视图模型随窗口数据上下文变化而更新
            LogDebugInfo(LogContext.Actions.Initialize, "视图模型已连接到窗口");
        }
        else
        {
            LogDebugInfo(LogContext.Actions.Initialize, "警告：窗口数据上下文不是MainWindowViewModel类型");
        }
    }
    
    /// <summary>
    /// 记录调试信息
    /// </summary>
    private void LogDebugInfo(string action, string details)
    {
        SerilogLoggerService.Instance.LogComponentDebug(
            LogContext.Components.MainWindow, 
            action,
            details);
    }
    
    /// <summary>
    /// 记录错误信息
    /// </summary>
    private void LogError(string action, string details, Exception ex)
    {
        SerilogLoggerService.Instance.LogComponentError(
            ex,
            LogContext.Components.MainWindow, 
            action,
            details);
    }
}