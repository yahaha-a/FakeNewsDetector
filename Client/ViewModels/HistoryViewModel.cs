using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Client.Services.Interfaces;
using Client.Models;
using Client.Helpers.Events;
using Client.Helpers;

namespace Client.ViewModels
{
    /// <summary>
    /// 历史记录页面视图模型
    /// </summary>
    public partial class HistoryViewModel : ViewModelBase
    {
        private readonly IAnalysisService _analysisService;
        private readonly ILoggerService _logger;

        /// <summary>
        /// 历史分析结果集合
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<AnalysisResultWrapper> _historyItems = new();

        /// <summary>
        /// 选中的历史记录
        /// </summary>
        [ObservableProperty]
        private AnalysisResultWrapper? _selectedHistoryItem;

        /// <summary>
        /// 是否正在加载
        /// </summary>
        [ObservableProperty]
        private bool _isLoading;

        /// <summary>
        /// 是否没有历史记录
        /// </summary>
        [ObservableProperty]
        private bool _hasNoHistory = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        public HistoryViewModel(IAnalysisService analysisService, ILoggerService logger)
        {
            _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.Information("初始化历史记录视图模型");
            
            // 初始化后立即加载数据
            Task.Run(LoadHistoryAsync);
        }

        /// <summary>
        /// 删除当前选中的历史记录
        /// </summary>
        public void DeleteSelectedItem()
        {
            if (SelectedHistoryItem != null)
            {
                DeleteHistoryItem(SelectedHistoryItem);
            }
        }

        /// <summary>
        /// 加载历史记录命令
        /// </summary>
        [RelayCommand]
        private async Task LoadHistoryAsync()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;
                _logger.Debug("正在加载历史记录");
                
                // 清空现有记录
                HistoryItems.Clear();
                
                // 获取历史分析记录
                var historyResults = await _analysisService.GetHistoryAnalysisAsync(50);
                
                // 更新UI
                if (historyResults.Any())
                {
                    foreach (var result in historyResults)
                    {
                        HistoryItems.Add(new AnalysisResultWrapper(result));
                    }
                    HasNoHistory = false;
                }
                else
                {
                    HasNoHistory = true;
                }
                
                _logger.Information("历史记录加载完成，共 {Count} 条记录", HistoryItems.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "加载历史记录失败");
                ReportError(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 清空历史记录命令
        /// </summary>
        [RelayCommand]
        private void ClearHistory()
        {
            try
            {
                _logger.Debug("正在清空历史记录");
                
                // 清空UI
                HistoryItems.Clear();
                SelectedHistoryItem = null;
                HasNoHistory = true;
                
                // TODO: 实现实际的清空历史记录功能，目前只清空内存中的数据
                
                _logger.Information("历史记录已清空");
                
                // 通知用户
                EventAggregator.Instance.Publish(new ErrorOccurredEvent(
                    "历史记录已清空", 
                    "历史记录页面", 
                    ErrorSeverity.Info
                ));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "清空历史记录失败");
                ReportError(ex);
            }
        }

        /// <summary>
        /// 删除历史记录命令
        /// </summary>
        [RelayCommand]
        private void DeleteHistoryItem(AnalysisResultWrapper? item)
        {
            if (item == null)
                return;

            try
            {
                _logger.Debug("正在删除历史记录：{Title}", item.Title);
                
                // 从UI中移除
                HistoryItems.Remove(item);
                
                if (SelectedHistoryItem == item)
                    SelectedHistoryItem = null;
                
                // 检查是否为空
                HasNoHistory = HistoryItems.Count == 0;
                
                // TODO: 实现实际的删除历史记录功能，目前只删除内存中的数据
                
                _logger.Information("历史记录已删除：{Title}", item.Title);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "删除历史记录失败");
                ReportError(ex);
            }
        }

        /// <summary>
        /// 查看详情命令
        /// </summary>
        [RelayCommand]
        private void ViewDetails(AnalysisResultWrapper? item)
        {
            if (item == null)
                return;

            try
            {
                SelectedHistoryItem = item;
                _logger.Debug("查看历史记录详情：{Title}", item.Title);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "查看历史记录详情失败");
                ReportError(ex);
            }
        }

        /// <summary>
        /// 报告错误
        /// </summary>
        private void ReportError(Exception ex)
        {
            EventAggregator.Instance.Publish(new ErrorOccurredEvent(
                "历史记录错误",
                "操作历史记录时发生错误",
                ErrorSeverity.Warning,
                ex
            ));
        }
    }
} 