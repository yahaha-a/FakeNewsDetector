using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Client.Services.Interfaces;
using Client.Helpers.Events;
using Client.Helpers;

namespace Client.ViewModels;

/// <summary>
/// 主窗口视图模型
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly ILoggerService _logger;
    
    /// <summary>
    /// 当前页面标题
    /// </summary>
    [ObservableProperty]
    private string _currentPageTitle = "首页";
    
    /// <summary>
    /// 通知集合
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<object> _notifications = new();
    
    /// <summary>
    /// 当前视图模型
    /// </summary>
    public ViewModelBase? CurrentViewModel => _navigationService.CurrentViewModel;
    
    /// <summary>
    /// 当前页面
    /// </summary>
    public PageType CurrentPage => _navigationService.CurrentPage;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="navigationService">导航服务</param>
    /// <param name="logger">日志服务</param>
    public MainWindowViewModel(INavigationService navigationService, ILoggerService logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _logger.Information("初始化主窗口视图模型");
        
        // 监听导航服务的属性变更
        if (_navigationService is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += (sender, args) =>
            {
                // 当导航服务的属性变更时，通知UI更新
                if (args.PropertyName == nameof(INavigationService.CurrentViewModel))
                {
                    OnPropertyChanged(nameof(CurrentViewModel));
                }
                else if (args.PropertyName == nameof(INavigationService.CurrentPage))
                {
                    OnPropertyChanged(nameof(CurrentPage));
                }
                else if (args.PropertyName == nameof(INavigationService.CurrentPageTitle))
                {
                    CurrentPageTitle = _navigationService.CurrentPageTitle;
                }
            };
        }
        
        // 初始化导航在 MainWindow 的 Loaded 事件中完成，因为需要页面内容控件
    }
    
    /// <summary>
    /// 初始化导航
    /// </summary>
    public async Task InitializeNavigation()
    {
        try
        {
            // 默认导航到首页
            _logger.Information("开始执行导航初始化 - 导航到首页");
            await _navigationService.NavigateToAsync(PageType.Home);
            _logger.Information("导航到首页成功完成");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "初始化导航失败: {Message}", ex.Message);
            ReportException(ex);
        }
    }
    
    /// <summary>
    /// 导航命令
    /// </summary>
    [RelayCommand]
    private async Task NavigateAsync(string pageTypeString)
    {
        if (Enum.TryParse<PageType>(pageTypeString, true, out var pageType))
        {
            _logger.Information("导航到页面: {PageType}", pageType);
            await _navigationService.NavigateToAsync(pageType);
        }
        else
        {
            _logger.Warning("无效的页面类型: {PageType}", pageTypeString);
        }
    }
    
    /// <summary>
    /// 报告异常
    /// </summary>
    private void ReportException(Exception ex)
    {
        // 报告错误
        EventAggregator.Instance.Publish(new ErrorOccurredEvent(
            "视图导航错误",
            "导航到页面时发生错误",
            ErrorSeverity.Error,
            ex
        ));
    }
}
