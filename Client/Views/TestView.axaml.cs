using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Client.Services;
using Client.ViewModels;
using System;

namespace Client.Views
{
    public partial class TestView : UserControl, IParameterPage
    {
        public TestView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        /// <summary>
        /// 实现 IParameterPage 接口，处理接收到的导航参数
        /// </summary>
        /// <param name="parameter">导航参数</param>
        public void InitializeParameters(object parameter)
        {
            if (DataContext is TestViewModel viewModel)
            {
                viewModel.InitializeWithParameter(parameter);
            }
        }
    }
} 