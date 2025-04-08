using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

using Client.DependencyInjection;
using Client.Helpers;
using Client.Helpers.Events;
using Client.Services.Interfaces;
using Client.ViewModels;
using Client.Views;

namespace Client.Services
{
    /// <summary>
    /// 支持参数传递的页面接口
    /// </summary>
    public interface IParameterPage
    {
        /// <summary>
        /// 初始化页面参数
        /// </summary>
        /// <param name="parameter">传递的参数</param>
        void InitializeParameters(object parameter);
    }

    /// <summary>
    /// 导航完成事件
    /// </summary>
    public class NavigationCompletedEvent
    {
        /// <summary>
        /// 导航目标页面
        /// </summary>
        public PageType TargetPage { get; }

        /// <summary>
        /// 创建导航完成事件
        /// </summary>
        /// <param name="targetPage">导航目标页面</param>
        public NavigationCompletedEvent(PageType targetPage) => TargetPage = targetPage;
    }

    /// <summary>
    /// 导航服务实现
    /// </summary>
    public partial class NavigationService : ObservableObject, INavigationService
    {
        // 页面类型到视图模型类型的映射
        private readonly Dictionary<PageType, Type> _pageViewModels;
        private readonly ILoggerService _logger;
        private ContentControl? _contentControl;
        private readonly SemaphoreSlim _navigationLock = new(1, 1);
        
        /// <summary>
        /// 当前视图模型
        /// </summary>
        [ObservableProperty]
        private ViewModelBase _currentViewModel;

        /// <summary>
        /// 当前页面
        /// </summary>
        [ObservableProperty]
        private PageType _currentPage = PageType.Home;

        /// <summary>
        /// 当前页面标题
        /// </summary>
        [ObservableProperty]
        private string _currentPageTitle = "首页";

        /// <summary>
        /// 构造函数
        /// </summary>
        public NavigationService(ILoggerService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentViewModel = new ViewModelBase();
            
            // 初始化页面映射
            _pageViewModels = new Dictionary<PageType, Type>
            {
                { PageType.Home, typeof(HomeViewModel) },
                { PageType.Detection, typeof(DetectionViewModel) },
                { PageType.Settings, typeof(SettingsViewModel) },
                { PageType.History, typeof(HistoryViewModel) },
                { PageType.Statistics, typeof(StatisticsViewModel) },
                { PageType.Test, typeof(TestViewModel) }
            };
        }

        /// <inheritdoc/>
        public void SetNavigationTarget(ContentControl contentControl)
        {
            _contentControl = contentControl;
            _logger.Debug("设置导航目标: {ContentControl}", contentControl?.GetType().Name ?? "null");
        }

        /// <inheritdoc/>
        public async Task NavigateToAsync(PageType pageType) => 
            await NavigateToAsync(pageType, null);
        
        /// <summary>
        /// 带参数的异步导航
        /// </summary>
        public async Task NavigateToAsync(PageType pageType, object? parameter)
        {
            if (_contentControl == null)
            {
                _logger.Error("导航失败：未设置导航目标控件");
                return;
            }
            
            _logger.Debug("开始导航到 {PageType}, 当前页面={CurrentPage}", pageType, CurrentPage);
            
            try
            {
                // 获取导航锁，确保一次只有一个导航操作
                await _navigationLock.WaitAsync();
                await InternalNavigateAsync(pageType, parameter);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "获取导航锁时发生异常: {Message}", ex.Message);
                ReportNavigationError(pageType, ex);
            }
            finally
            {
                // 释放导航锁
                _navigationLock.Release();
            }
        }
        
        /// <summary>
        /// 内部导航方法，包含实际导航逻辑
        /// </summary>
        private async Task InternalNavigateAsync(PageType pageType, object? parameter)
        {
            try
            {
                // 获取页面实例
                var control = await GetPageInstanceAsync(pageType);
                if (control == null)
                {
                    _logger.Error("导航失败：无法创建页面 {PageType}", pageType);
                    return;
                }
                
                // 检查页面是否支持参数
                if (parameter != null && control is IParameterPage parameterPage)
                {
                    _logger.Debug("正在初始化页面参数 {PageType}", pageType);
                    parameterPage.InitializeParameters(parameter);
                }
                
                // 清理旧页面资源
                if (_contentControl?.Content is UserControl oldPage && oldPage is IDisposable disposable)
                {
                    _logger.Debug("正在清理旧页面: {OldPage}", oldPage.GetType().Name);
                    disposable.Dispose();
                }
                
                // 设置新页面 - 使用InvokeAsync确保在UI线程上执行并等待完成
                try
                {
                    await Dispatcher.UIThread.InvokeAsync(() => 
                    {
                        if (_contentControl != null)
                        {
                            _contentControl.Content = control;
                        }
                        
                        // 更新当前页面状态
                        CurrentPage = pageType;
                        CurrentPageTitle = GetPageTitle(pageType);
                    });
                }
                catch (Exception uiEx)
                {
                    _logger.Error(uiEx, "在UI线程上设置页面时发生异常: {Message}", uiEx.Message);
                    throw;
                }
                
                // 发布导航成功事件
                EventAggregator.Instance.Publish(new NavigationCompletedEvent(pageType));
                
                _logger.Debug("已成功导航到页面: {PageType}", pageType);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "导航过程中发生异常: {Message}", ex.Message);
                ReportNavigationError(pageType, ex);
            }
        }

        /// <summary>
        /// 根据ViewModel类型创建对应的页面
        /// </summary>
        private static UserControl GetPageForViewModel(ViewModelBase viewModel)
        {
            UserControl page = viewModel switch
            {
                HomeViewModel _ => new HomeView(),
                DetectionViewModel _ => new DetectionView(),
                SettingsViewModel _ => new SettingsView(),
                HistoryViewModel _ => new HistoryView(),
                StatisticsViewModel _ => new StatisticsView(),
                TestViewModel _ => new TestView(),
                _ => new HomeView()
            };
            
            page.DataContext = viewModel;
            return page;
        }
        
        /// <inheritdoc/>
        public void Navigate(PageType pageType) => Navigate(pageType, null);
        
        /// <inheritdoc/>
        public void Navigate(PageType pageType, object? parameter)
        {
            try
            {
                _logger.Debug("触发导航到页面: {PageType}", pageType);
                
                // 使用Task.Run在非UI线程上启动导航过程，避免阻塞UI线程
                _ = Task.Run(async () => 
                {
                    try
                    {
                        // 通过UI线程调度器执行导航，使用InvokeAsync确保等待完成
                        await Dispatcher.UIThread.InvokeAsync(async () => 
                        {
                            try 
                            {
                                await NavigateToAsync(pageType, parameter);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "导航过程中发生异常: {Message}", ex.Message);
                                ReportNavigationError(pageType, ex);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        // 如果InvokeAsync调用本身失败，则在UI线程上报告错误
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            _logger.Error(ex, "UI线程调度失败: {Message}", ex.Message);
                            ReportNavigationError(pageType, ex, "导航线程调度失败");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "启动导航任务失败: {Message}", ex.Message);
                ReportNavigationError(pageType, ex, "无法启动导航任务");
            }
        }

        /// <inheritdoc/>
        public string GetPageTitle(PageType pageType) => pageType switch
        {
            PageType.Home => "首页",
            PageType.Detection => "检测",
            PageType.Settings => "设置",
            PageType.History => "历史记录",
            PageType.Statistics => "统计报告",
            PageType.Test => "功能测试",
            _ => "未知页面"
        };

        /// <summary>
        /// 获取页面实例
        /// </summary>
        private async Task<UserControl?> GetPageInstanceAsync(PageType pageType)
        {
            // 获取对应页面的ViewModel类型
            if (!_pageViewModels.TryGetValue(pageType, out var viewModelType))
            {
                _logger.Warning("未找到页面类型：{PageType}，使用默认页面", pageType);
                viewModelType = typeof(HomeViewModel);
                pageType = PageType.Home;
            }
            
            // 获取视图模型实例
            ViewModelBase? viewModel = GetViewModelForPageType(viewModelType.Name);
            
            if (viewModel == null)
            {
                _logger.Error("无法从依赖注入容器获取视图模型: {ViewModelType}", viewModelType.Name);
                return null;
            }
            
            try
            {
                // 更新当前视图模型 - 使用InvokeAsync确保在UI线程上执行并等待完成
                await Dispatcher.UIThread.InvokeAsync(() => CurrentViewModel = viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "更新当前视图模型时发生异常: {Message}", ex.Message);
                throw;
            }
            
            // 根据ViewModel创建页面
            return GetPageForViewModel(viewModel);
        }
        
        /// <summary>
        /// 根据视图模型类型名称获取视图模型实例
        /// </summary>
        private static ViewModelBase? GetViewModelForPageType(string viewModelTypeName) => viewModelTypeName switch
        {
            nameof(HomeViewModel) => DependencyContainer.GetService<HomeViewModel>(),
            nameof(DetectionViewModel) => DependencyContainer.GetService<DetectionViewModel>(),
            nameof(SettingsViewModel) => DependencyContainer.GetService<SettingsViewModel>(),
            nameof(HistoryViewModel) => DependencyContainer.GetService<HistoryViewModel>(),
            nameof(StatisticsViewModel) => DependencyContainer.GetService<StatisticsViewModel>(),
            nameof(TestViewModel) => DependencyContainer.GetService<TestViewModel>(),
            _ => new ViewModelBase()
        };
        
        /// <summary>
        /// 报告导航错误
        /// </summary>
        private void ReportNavigationError(PageType pageType, Exception ex, string? customMessage = null)
        {
            var message = customMessage ?? $"无法导航到{GetPageTitle(pageType)}";
            
            // 发布错误事件
            EventAggregator.Instance.Publish(new ErrorOccurredEvent(
                "导航错误",
                message,
                ErrorSeverity.Error,
                ex
            ));
        }
    }
} 