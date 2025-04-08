using Avalonia.Controls;
using Client.Helpers;
using Client.Services;
using Client.ViewModels;
using System;

namespace Client.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        try
        {
            SerilogLoggerService.Instance.LogComponentDebug(
                LogContext.Components.HomeView,
                LogContext.Actions.Initialize,
                "正在初始化HomeView");
                
            InitializeComponent();
            
            this.DataContextChanged += HomeView_DataContextChanged;
            this.Loaded += HomeView_Loaded;
            
            SerilogLoggerService.Instance.LogComponentDebug(
                LogContext.Components.HomeView,
                LogContext.Actions.Initialize,
                "初始化完成");
        }
        catch (Exception ex)
        {
            SerilogLoggerService.Instance.LogComponentError(
                ex,
                LogContext.Components.HomeView,
                LogContext.Actions.Initialize,
                "初始化错误");
        }
    }
    
    private void HomeView_DataContextChanged(object? sender, EventArgs e)
    {
        try
        {
            SerilogLoggerService.Instance.LogComponentDebug(
                LogContext.Components.HomeView,
                LogContext.Actions.Process,
                "DataContext已更改");
                
            if (DataContext is HomeViewModel viewModel)
            {
                SerilogLoggerService.Instance.LogComponentDebug(
                    LogContext.Components.HomeView,
                    LogContext.Actions.Process,
                    $"新的DataContext是HomeViewModel: TotalDetections={viewModel.TotalDetections}");
            }
            else if (DataContext == null)
            {
                SerilogLoggerService.Instance.LogComponentWarning(
                    LogContext.Components.HomeView,
                    LogContext.Actions.Process,
                    "DataContext为null");
            }
            else
            {
                SerilogLoggerService.Instance.LogComponentWarning(
                    LogContext.Components.HomeView,
                    LogContext.Actions.Process,
                    $"DataContext类型不是HomeViewModel，而是 {DataContext.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            SerilogLoggerService.Instance.LogComponentError(
                ex,
                LogContext.Components.HomeView,
                LogContext.Actions.Process,
                "DataContextChanged事件处理错误");
        }
    }
    
    private void HomeView_Loaded(object? sender, EventArgs e)
    {
        try
        {
            SerilogLoggerService.Instance.LogComponentDebug(
                LogContext.Components.HomeView,
                LogContext.Actions.Load,
                "视图已加载");
                
            if (DataContext is HomeViewModel viewModel)
            {
                SerilogLoggerService.Instance.LogComponentDebug(
                    LogContext.Components.HomeView,
                    LogContext.Actions.Load,
                    $"加载完成，DataContext是HomeViewModel: TotalDetections={viewModel.TotalDetections}");
            }
            else
            {
                SerilogLoggerService.Instance.LogComponentWarning(
                    LogContext.Components.HomeView,
                    LogContext.Actions.Load,
                    "加载完成，但DataContext不是HomeViewModel");
            }
        }
        catch (Exception ex)
        {
            SerilogLoggerService.Instance.LogComponentError(
                ex,
                LogContext.Components.HomeView,
                LogContext.Actions.Load,
                "Loaded事件处理错误");
        }
    }
} 