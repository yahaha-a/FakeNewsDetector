using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.ViewModels;

/// <summary>
/// 简单的视图模型基类
/// </summary>
public class ViewModelBase : ObservableObject
{
    /// <summary>
    /// 标题属性
    /// </summary>
    private string _title = string.Empty;
    
    /// <summary>
    /// 标题
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
    
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public ViewModelBase()
    {
        // 设置默认标题
        Title = GetType().Name;
        if (Title.EndsWith("ViewModel"))
        {
            Title = Title.Substring(0, Title.Length - "ViewModel".Length);
        }
    }
}
