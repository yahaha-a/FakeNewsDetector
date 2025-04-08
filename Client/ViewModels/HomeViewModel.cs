using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Client.Services.Interfaces;
using Client.ViewModels.Base;
using Client.DependencyInjection;
using Client.Models;

namespace Client.ViewModels;

/// <summary>
/// 首页视图模型
/// </summary>
public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly ILoggerService _logger;
    
    /// <summary>
    /// 总检测数量
    /// </summary>
    [ObservableProperty]
    private int _totalDetections = 10234;
    
    /// <summary>
    /// 真实新闻数量
    /// </summary>
    [ObservableProperty]
    private int _realNewsCount = 7021;
    
    /// <summary>
    /// 可疑新闻数量
    /// </summary>
    [ObservableProperty]
    private int _suspiciousNewsCount = 2105;
    
    /// <summary>
    /// 虚假新闻数量
    /// </summary>
    [ObservableProperty]
    private int _fakeNewsCount = 1108;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="navigationService">导航服务</param>
    /// <param name="logger">日志服务</param>
    public HomeViewModel(INavigationService navigationService, ILoggerService logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // 确保属性有初始值
        TotalDetections = 10234;
        RealNewsCount = 7021;
        SuspiciousNewsCount = 2105;
        FakeNewsCount = 1108;
    }
    
    /// <summary>
    /// 开始检测命令
    /// </summary>
    [RelayCommand]
    private void StartDetection()
    {
        try
        {
            _logger.Information("导航到检测页面");
            // 导航到检测页面
            _navigationService.Navigate(PageType.Detection);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "导航错误: {Message}", ex.Message);
            // 可以选择添加更多错误处理逻辑
        }
    }
    
    /// <summary>
    /// 参数传递测试命令 - 传递文本
    /// </summary>
    [RelayCommand]
    private void NavigateWithTextParam()
    {
        try
        {
            _logger.Information("导航到测试页面并传递文本参数");
            string parameter = "这是来自首页的测试参数，当前时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _navigationService.Navigate(PageType.Test, parameter);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "导航错误: {Message}", ex.Message);
        }
    }
    
    /// <summary>
    /// 参数传递测试命令 - 传递复杂对象
    /// </summary>
    [RelayCommand]
    private void NavigateWithObjectParam()
    {
        try
        {
            _logger.Information("导航到测试页面并传递对象参数");
            
            // 创建一个模拟的新闻项
            var newsItem = new NewsItem
            {
                Id = Guid.NewGuid().ToString(),
                Title = "测试新闻标题",
                Source = "参数传递测试",
                Content = "这是一个通过导航参数传递的测试新闻内容。\n用于验证复杂对象的参数传递功能。",
                Url = "https://example.com/test",
                PublishDate = DateTime.Now,
                Author = "参数传递测试"
            };
            
            _navigationService.Navigate(PageType.Test, newsItem);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "导航错误: {Message}", ex.Message);
        }
    }
} 