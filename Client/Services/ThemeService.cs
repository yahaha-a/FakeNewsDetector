using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Client.Services
{
    /// <summary>
    /// 主题服务
    /// </summary>
    public partial class ThemeService : ObservableObject
    {
        private static readonly Lazy<ThemeService> _instance = new(() => new ThemeService());
        public static ThemeService Instance => _instance.Value;

        [ObservableProperty]
        private ThemeVariant _themeVariant = ThemeVariant.Light;

        private ThemeService()
        {
            // 初始化时尝试从系统获取主题设置
            var systemTheme = Application.Current?.ActualThemeVariant ?? ThemeVariant.Light;
            _themeVariant = systemTheme;
        }

        /// <summary>
        /// 切换主题
        /// </summary>
        public void ToggleTheme()
        {
            ThemeVariant = ThemeVariant == ThemeVariant.Light ? ThemeVariant.Dark : ThemeVariant.Light;
            
            // 通知主题变更
            if (Application.Current is App app)
            {
                app.ThemeVariant = ThemeVariant;
                
                // 更新所有窗口的主题
                if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    foreach (var window in desktop.Windows)
                    {
                        window.RequestedThemeVariant = ThemeVariant;
                    }
                }
            }
        }

        /// <summary>
        /// 设置主题
        /// </summary>
        /// <param name="theme">要设置的主题</param>
        public void SetTheme(ThemeVariant theme)
        {
            ThemeVariant = theme;
            
            // 通知主题变更
            if (Application.Current is App app)
            {
                app.ThemeVariant = ThemeVariant;
                
                // 更新所有窗口的主题
                if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    foreach (var window in desktop.Windows)
                    {
                        window.RequestedThemeVariant = ThemeVariant;
                    }
                }
            }
        }
    }
} 