using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Client.Services.Interfaces;
using Client.Helpers.Events;
using Client.Helpers;
using Client.Constants;
using Avalonia.Threading;
using Client.Services;
using Avalonia.Styling;

namespace Client.ViewModels
{
    /// <summary>
    /// 设置页面视图模型
    /// </summary>
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ILoggerService _logger;
        private readonly ThemeService _themeService;
        private bool _isBusy;

        /// <summary>
        /// 是否繁忙
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// 是否启用深色主题
        /// </summary>
        [ObservableProperty]
        private bool _isDarkThemeEnabled;

        /// <summary>
        /// 是否自动保存历史记录
        /// </summary>
        [ObservableProperty]
        private bool _autoSaveHistory = true;

        /// <summary>
        /// 最大历史记录数量
        /// </summary>
        [ObservableProperty]
        private int _maxHistoryCount = 200;

        /// <summary>
        /// API基础URL
        /// </summary>
        [ObservableProperty]
        private string _apiBaseUrl = "http://localhost:5000";

        /// <summary>
        /// 当前主题名称
        /// </summary>
        [ObservableProperty]
        private string _currentTheme = "浅色";

        /// <summary>
        /// 构造函数
        /// </summary>
        public SettingsViewModel(ISettingsService settingsService, ILoggerService logger, ThemeService themeService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _themeService = themeService;

            _logger.Information("初始化设置视图模型");
            
            // 不在构造函数中加载设置，而是在页面Loaded事件中加载
            // 直接使用正确的初始化方式，在Loaded时加载
            Dispatcher.UIThread.Post(async () => 
            {
                await LoadSettingsAsync();
            });

            // 初始化主题状态
            IsDarkThemeEnabled = _themeService.ThemeVariant == ThemeVariant.Dark;
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        private async Task LoadSettingsAsync()
        {
            try
            {
                IsBusy = true;
                _logger.Debug("正在加载设置");
                
                // 主题设置
                IsDarkThemeEnabled = await _settingsService.GetSettingAsync<bool>("IsDarkTheme", false);
                CurrentTheme = IsDarkThemeEnabled ? "深色" : "浅色";
                
                // 历史记录设置
                AutoSaveHistory = await _settingsService.GetSettingAsync<bool>("AutoSaveHistory", true);
                MaxHistoryCount = await _settingsService.GetSettingAsync<int>("MaxHistoryCount", 200);
                
                // API设置
                ApiBaseUrl = await _settingsService.GetSettingAsync<string>("ApiBaseUrl", "http://localhost:5000");
                
                _logger.Information("设置加载完成");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "加载设置失败");
                ReportError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// 切换主题命令
        /// </summary>
        [RelayCommand]
        private async Task ToggleThemeAsync()
        {
            try
            {
                var oldTheme = CurrentTheme;
                IsDarkThemeEnabled = !IsDarkThemeEnabled;
                CurrentTheme = IsDarkThemeEnabled ? "深色" : "浅色";
                
                // 切换主题
                _themeService.ToggleTheme();
                
                // 保存主题设置
                await _settingsService.SaveSettingAsync("IsDarkTheme", IsDarkThemeEnabled);
                
                // 发布主题变更事件
                EventAggregator.Instance.Publish(new SettingsChangedEvent("Theme", oldTheme, CurrentTheme));
                
                _logger.Information("主题已切换为 {Theme}", CurrentTheme);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "切换主题失败");
                ReportError(ex);
            }
        }

        /// <summary>
        /// 保存设置命令
        /// </summary>
        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            try
            {
                IsBusy = true;
                _logger.Debug("正在保存设置");
                
                // 保存所有设置
                await _settingsService.SaveSettingAsync("IsDarkTheme", IsDarkThemeEnabled);
                await _settingsService.SaveSettingAsync("AutoSaveHistory", AutoSaveHistory);
                await _settingsService.SaveSettingAsync("MaxHistoryCount", MaxHistoryCount);
                await _settingsService.SaveSettingAsync("ApiBaseUrl", ApiBaseUrl);
                
                // 发布设置变更事件
                EventAggregator.Instance.Publish(new SettingsChangedEvent("Settings", null, "All"));
                
                _logger.Information("设置保存完成");
                
                // 通知用户
                EventAggregator.Instance.Publish(new ErrorOccurredEvent(
                    "设置已保存", 
                    "设置页面", 
                    ErrorSeverity.Info
                ));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "保存设置失败");
                ReportError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// 重置设置命令
        /// </summary>
        [RelayCommand]
        private async Task ResetSettingsAsync()
        {
            try
            {
                IsBusy = true;
                _logger.Debug("正在重置设置");
                
                // 重置为默认值
                IsDarkThemeEnabled = false;
                CurrentTheme = "浅色";
                AutoSaveHistory = true;
                MaxHistoryCount = 200;
                ApiBaseUrl = "http://localhost:5000";
                
                // 保存默认设置
                await SaveSettingsAsync();
                
                _logger.Information("设置已重置为默认值");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "重置设置失败");
                ReportError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// 报告错误
        /// </summary>
        private void ReportError(Exception ex)
        {
            EventAggregator.Instance.Publish(new ErrorOccurredEvent(
                "设置错误",
                "操作设置时发生错误",
                ErrorSeverity.Warning,
                ex
            ));
        }
    }
} 