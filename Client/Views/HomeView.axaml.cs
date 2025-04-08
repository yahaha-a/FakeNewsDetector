using Avalonia.Controls;
using Client.Services;
using Client.ViewModels;
using System;
using System.Diagnostics;

namespace Client.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        try
        {
            SerilogLoggerService.Instance.Debug("HomeView: 正在初始化HomeView...");
            InitializeComponent();
            
            this.DataContextChanged += HomeView_DataContextChanged;
            this.Loaded += HomeView_Loaded;
            
            SerilogLoggerService.Instance.Debug("HomeView: 初始化完成");
        }
        catch (Exception ex)
        {
            SerilogLoggerService.Instance.Error(ex, "HomeView: 初始化错误");
        }
    }
    
    private void HomeView_DataContextChanged(object? sender, EventArgs e)
    {
        try
        {
            SerilogLoggerService.Instance.Debug("HomeView: DataContext已更改");
            if (DataContext is HomeViewModel viewModel)
            {
                SerilogLoggerService.Instance.Debug("HomeView: 新的DataContext是HomeViewModel: TotalDetections={TotalDetections}", viewModel.TotalDetections);
            }
            else if (DataContext == null)
            {
                SerilogLoggerService.Instance.Warning("HomeView: 警告: DataContext为null");
            }
            else
            {
                SerilogLoggerService.Instance.Warning("HomeView: 警告: DataContext类型不是HomeViewModel，而是 {ActualType}", DataContext.GetType().Name);
            }
        }
        catch (Exception ex)
        {
            SerilogLoggerService.Instance.Error(ex, "HomeView: DataContextChanged事件处理错误");
        }
    }
    
    private void HomeView_Loaded(object? sender, EventArgs e)
    {
        try
        {
            SerilogLoggerService.Instance.Debug("HomeView: 已加载");
            if (DataContext is HomeViewModel viewModel)
            {
                SerilogLoggerService.Instance.Debug("HomeView: 加载完成，DataContext是HomeViewModel: TotalDetections={TotalDetections}", viewModel.TotalDetections);
            }
            else
            {
                SerilogLoggerService.Instance.Warning("HomeView: 警告: 加载完成，但DataContext不是HomeViewModel");
            }
        }
        catch (Exception ex)
        {
            SerilogLoggerService.Instance.Error(ex, "HomeView: Loaded事件处理错误");
        }
    }
} 