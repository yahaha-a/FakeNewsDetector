using Avalonia.Controls;
using Client.ViewModels;
using System.Threading.Tasks;

namespace Client.Services.Interfaces
{
    /// <summary>
    /// 页面类型枚举
    /// </summary>
    public enum PageType
    {
        Home,
        Detection,
        Settings,
        History,
        Statistics,
        Test
    }
    
    /// <summary>
    /// 导航服务接口
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// 当前页面
        /// </summary>
        PageType CurrentPage { get; }
        
        /// <summary>
        /// 当前视图模型
        /// </summary>
        ViewModelBase? CurrentViewModel { get; }
        
        /// <summary>
        /// 当前页面标题
        /// </summary>
        string CurrentPageTitle { get; }
        
        /// <summary>
        /// 导航到指定页面
        /// </summary>
        /// <param name="pageType">页面类型</param>
        void Navigate(PageType pageType);
        
        /// <summary>
        /// 导航到指定页面并传递参数
        /// </summary>
        /// <param name="pageType">页面类型</param>
        /// <param name="parameter">导航参数</param>
        void Navigate(PageType pageType, object? parameter);
        
        /// <summary>
        /// 异步导航到指定页面
        /// </summary>
        /// <param name="pageType">页面类型</param>
        /// <returns>异步任务</returns>
        Task NavigateToAsync(PageType pageType);
        
        /// <summary>
        /// 异步导航到指定页面并传递参数
        /// </summary>
        /// <param name="pageType">页面类型</param>
        /// <param name="parameter">导航参数</param>
        /// <returns>异步任务</returns>
        Task NavigateToAsync(PageType pageType, object? parameter);
        
        /// <summary>
        /// 设置导航目标控件
        /// </summary>
        /// <param name="contentControl">内容控件</param>
        void SetNavigationTarget(ContentControl contentControl);
        
        /// <summary>
        /// 获取页面标题
        /// </summary>
        /// <param name="pageType">页面类型</param>
        /// <returns>页面标题</returns>
        string GetPageTitle(PageType pageType);
    }
} 