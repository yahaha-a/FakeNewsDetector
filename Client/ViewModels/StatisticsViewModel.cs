using System;
using System.Collections.ObjectModel;
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
    /// 统计分析页面视图模型
    /// </summary>
    public partial class StatisticsViewModel : ViewModelBase
    {
        private readonly IAnalysisService _analysisService;
        private readonly ILoggerService _logger;

        /// <summary>
        /// 检测总次数
        /// </summary>
        [ObservableProperty]
        private int _totalDetectionCount;

        /// <summary>
        /// 真实新闻数
        /// </summary>
        [ObservableProperty]
        private int _realNewsCount;

        /// <summary>
        /// 可疑新闻数
        /// </summary>
        [ObservableProperty]
        private int _suspiciousNewsCount;

        /// <summary>
        /// 虚假新闻数
        /// </summary>
        [ObservableProperty]
        private int _fakeNewsCount;

        /// <summary>
        /// 真实新闻百分比
        /// </summary>
        [ObservableProperty]
        private double _realNewsPercentage;

        /// <summary>
        /// 可疑新闻百分比
        /// </summary>
        [ObservableProperty]
        private double _suspiciousNewsPercentage;

        /// <summary>
        /// 虚假新闻百分比
        /// </summary>
        [ObservableProperty]
        private double _fakeNewsPercentage;

        /// <summary>
        /// 是否正在加载
        /// </summary>
        [ObservableProperty]
        private bool _isLoading;

        /// <summary>
        /// 最近的分析结果集合
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<AnalysisResultWrapper> _recentResults = new();

        /// <summary>
        /// 构造函数
        /// </summary>
        public StatisticsViewModel(IAnalysisService analysisService, ILoggerService logger)
        {
            _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.Information("初始化统计分析视图模型");
            
            // 初始化后立即加载数据
            Task.Run(LoadStatisticsAsync);
        }

        /// <summary>
        /// 加载统计数据命令
        /// </summary>
        [RelayCommand]
        private async Task LoadStatisticsAsync()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;
                _logger.Debug("正在加载统计数据");

                // 获取历史分析记录
                var historyResults = await _analysisService.GetHistoryAnalysisAsync(100);
                
                // 计算统计数据
                TotalDetectionCount = historyResults.Count;
                RealNewsCount = 0;
                SuspiciousNewsCount = 0;
                FakeNewsCount = 0;
                
                // 清空最近结果
                RecentResults.Clear();
                
                // 处理每条记录
                foreach (var result in historyResults)
                {
                    switch (result.ResultType)
                    {
                        case ResultType.Real:
                            RealNewsCount++;
                            break;
                        case ResultType.Suspicious:
                            SuspiciousNewsCount++;
                            break;
                        case ResultType.Fake:
                            FakeNewsCount++;
                            break;
                    }
                    
                    // 添加最近的结果（最多10条）
                    if (RecentResults.Count < 10)
                    {
                        RecentResults.Add(new AnalysisResultWrapper(result));
                    }
                }
                
                // 计算百分比
                if (TotalDetectionCount > 0)
                {
                    RealNewsPercentage = (double)RealNewsCount / TotalDetectionCount * 100;
                    SuspiciousNewsPercentage = (double)SuspiciousNewsCount / TotalDetectionCount * 100;
                    FakeNewsPercentage = (double)FakeNewsCount / TotalDetectionCount * 100;
                }
                else
                {
                    RealNewsPercentage = 0;
                    SuspiciousNewsPercentage = 0;
                    FakeNewsPercentage = 0;
                }
                
                _logger.Information("统计数据加载完成，总检测次数: {Count}", TotalDetectionCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "加载统计数据失败");
                ReportError(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 报告错误
        /// </summary>
        private void ReportError(Exception ex)
        {
            EventAggregator.Instance.Publish(new ErrorOccurredEvent(
                "统计分析错误",
                "加载统计数据时发生错误",
                ErrorSeverity.Warning,
                ex
            ));
        }
    }
} 