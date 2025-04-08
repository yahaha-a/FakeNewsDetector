using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Client.ViewModels;
using Client.Views;

namespace Client;

/// <summary>
/// 视图定位器（根据ViewModel查找相应的View）
/// </summary>
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// 构建视图
    /// </summary>
    public Control Build(object? param)
    {
        try
        {
            if (param is null)
            {
                return new TextBlock { Text = "No data" };
            }
            
            // 针对常用ViewModel类型进行直接匹配（性能优化）
            if (param is HomeViewModel) return new HomeView();
            if (param is MainWindowViewModel) return new MainWindow();
            if (param is DetectionViewModel) return new DetectionView();
            if (param is TestViewModel) return new TestView();
            
            // 获取视图名称
            var viewModelName = param.GetType().FullName!;
            var viewName = viewModelName.Replace("ViewModel", "View");
            
            // 按优先级尝试不同方式查找视图类型
            Type? type = null;
            
            // 1. 在Client.Views命名空间中查找
            var clientViewsName = $"Client.Views.{param.GetType().Name.Replace("ViewModel", "View")}";
            type = Type.GetType(clientViewsName) ?? param.GetType().Assembly.GetType(clientViewsName);
            
            // 2. 使用完全限定名查找
            if (type == null)
            {
                type = Type.GetType(viewName) ?? param.GetType().Assembly.GetType(viewName);
            }
            
            // 3. 使用简单名称查找（不带命名空间）
            if (type == null)
            {
                var simpleViewName = param.GetType().Name.Replace("ViewModel", "View");
                var viewTypes = param.GetType().Assembly.GetTypes()
                    .Where(t => t.Name == simpleViewName && typeof(Control).IsAssignableFrom(t))
                    .ToArray();
                
                if (viewTypes.Length > 0)
                {
                    type = viewTypes[0];
                }
            }
            
            // 如果找到视图类型，则创建实例
            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            
            // 未找到视图，返回错误提示
            return new TextBlock { Text = $"找不到视图: {viewName}" };
        }
        catch (Exception ex)
        {
            // 发生异常时返回错误信息
            return new TextBlock { Text = $"视图加载错误: {ex.Message}" };
        }
    }

    /// <summary>
    /// 检查对象是否为ViewModel
    /// </summary>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
