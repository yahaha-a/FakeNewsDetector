using Avalonia.Controls;
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
            Debug.WriteLine("正在初始化HomeView...");
            InitializeComponent();
            
            this.DataContextChanged += HomeView_DataContextChanged;
            this.Loaded += HomeView_Loaded;
            
            Debug.WriteLine("HomeView初始化完成");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HomeView初始化错误: {ex.Message}");
            Debug.WriteLine(ex.StackTrace);
        }
    }
    
    private void HomeView_DataContextChanged(object? sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("HomeView DataContext已更改");
            if (DataContext is HomeViewModel viewModel)
            {
                Debug.WriteLine($"新的DataContext是HomeViewModel: TotalDetections={viewModel.TotalDetections}");
            }
            else if (DataContext == null)
            {
                Debug.WriteLine("警告: HomeView的DataContext为null");
            }
            else
            {
                Debug.WriteLine($"警告: HomeView的DataContext类型不是HomeViewModel，而是 {DataContext.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HomeView DataContextChanged事件处理错误: {ex.Message}");
        }
    }
    
    private void HomeView_Loaded(object? sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("HomeView已加载");
            if (DataContext is HomeViewModel viewModel)
            {
                Debug.WriteLine($"HomeView加载完成，DataContext是HomeViewModel: TotalDetections={viewModel.TotalDetections}");
            }
            else
            {
                Debug.WriteLine($"警告: HomeView加载完成，但DataContext不是HomeViewModel");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HomeView Loaded事件处理错误: {ex.Message}");
        }
    }
} 