using Client.Helpers;
using Client.Helpers.Events;
using Client.Models;
using Client.Services.Interfaces;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client.ViewModels
{
    /// <summary>
    /// 测试页面视图模型
    /// </summary>
    public class TestViewModel : ViewModelBase
    {
        private readonly IAnalysisService _analysisService;
        private readonly INewsService _newsService;
        private string _url = string.Empty;
        private string _content = string.Empty;
        private bool _isAnalyzing;
        private AnalysisResult? _result;
        
        // 错误信息字段
        private string _urlError = string.Empty;
        private string _contentError = string.Empty;

        private string _parameterInfo = string.Empty;
        
        /// <summary>
        /// 接收到的参数信息
        /// </summary>
        public string ParameterInfo
        {
            get => _parameterInfo;
            private set => SetProperty(ref _parameterInfo, value);
        }

        /// <summary>
        /// URL
        /// </summary>
        public string Url
        {
            get => _url;
            set
            {
                if (SetProperty(ref _url, value))
                {
                    ValidateUrl();
                    AnalyzeUrlCommand.NotifyCanExecuteChanged();
                }
            }
        }
        
        /// <summary>
        /// URL错误信息
        /// </summary>
        public string UrlError
        {
            get => _urlError;
            private set => SetProperty(ref _urlError, value);
        }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content
        {
            get => _content;
            set
            {
                if (SetProperty(ref _content, value))
                {
                    ValidateContent();
                    AnalyzeContentCommand.NotifyCanExecuteChanged();
                }
            }
        }
        
        /// <summary>
        /// 内容错误信息
        /// </summary>
        public string ContentError
        {
            get => _contentError;
            private set => SetProperty(ref _contentError, value);
        }

        /// <summary>
        /// 是否正在分析
        /// </summary>
        public bool IsAnalyzing
        {
            get => _isAnalyzing;
            private set
            {
                if (SetProperty(ref _isAnalyzing, value))
                {
                    AnalyzeUrlCommand.NotifyCanExecuteChanged();
                    AnalyzeContentCommand.NotifyCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 分析结果
        /// </summary>
        public AnalysisResult? Result
        {
            get => _result;
            set
            {
                SetProperty(ref _result, value);
                OnPropertyChanged(nameof(HasAnalysisResult));
                OnPropertyChanged(nameof(AnalysisResult));
            }
        }

        /// <summary>
        /// 分析结果的包装器（用于视图绑定）
        /// </summary>
        public AnalysisResultWrapper AnalysisResult => new AnalysisResultWrapper(_result);

        /// <summary>
        /// 是否有分析结果
        /// </summary>
        public bool HasAnalysisResult => _result != null;
        
        /// <summary>
        /// 分析URL命令
        /// </summary>
        public IRelayCommand AnalyzeUrlCommand { get; }
        
        /// <summary>
        /// 分析内容命令
        /// </summary>
        public IRelayCommand AnalyzeContentCommand { get; }
        
        /// <summary>
        /// 触发异常命令
        /// </summary>
        public IRelayCommand ThrowExceptionCommand { get; }
        
        /// <summary>
        /// 触发业务异常命令
        /// </summary>
        public IRelayCommand ThrowBusinessExceptionCommand { get; }
        
        /// <summary>
        /// 发布事件命令
        /// </summary>
        public IRelayCommand PublishEventCommand { get; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="analysisService">分析服务</param>
        /// <param name="newsService">新闻服务</param>
        public TestViewModel(IAnalysisService analysisService, INewsService newsService)
        {
            _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
            _newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            
            // 初始化命令
            AnalyzeUrlCommand = new RelayCommand(
                async () => await AnalyzeUrlAsync(), 
                CanAnalyzeUrl);
                
            AnalyzeContentCommand = new RelayCommand(
                async () => await AnalyzeContentAsync(), 
                CanAnalyzeContent);
                
            ThrowExceptionCommand = new RelayCommand(TriggerError);
            
            ThrowBusinessExceptionCommand = new RelayCommand(TriggerBusinessError);
            
            PublishEventCommand = new RelayCommand(PublishTestEvent);
        }
        
        /// <summary>
        /// 验证URL
        /// </summary>
        private void ValidateUrl()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                UrlError = "URL不能为空";
                return;
            }
            
            if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
            {
                UrlError = "请输入有效的URL";
                return;
            }
            
            UrlError = string.Empty;
        }
        
        /// <summary>
        /// 验证内容
        /// </summary>
        private void ValidateContent()
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                ContentError = "内容不能为空";
                return;
            }
            
            if (Content.Length < 10)
            {
                ContentError = "内容太短，请至少输入10个字符";
                return;
            }
            
            ContentError = string.Empty;
        }

        /// <summary>
        /// 是否可以分析URL
        /// </summary>
        /// <returns>是否可用</returns>
        private bool CanAnalyzeUrl()
        {
            if (IsAnalyzing)
                return false;
                
            return !string.IsNullOrWhiteSpace(Url) && Uri.TryCreate(Url, UriKind.Absolute, out _);
        }

        /// <summary>
        /// 是否可以分析内容
        /// </summary>
        /// <returns>是否可用</returns>
        private bool CanAnalyzeContent()
        {
            if (IsAnalyzing)
                return false;
                
            return !string.IsNullOrWhiteSpace(Content) && Content.Length >= 10;
        }

        /// <summary>
        /// 分析URL
        /// </summary>
        /// <returns>任务</returns>
        private async Task AnalyzeUrlAsync()
        {
            ValidateUrl();
            if (!string.IsNullOrEmpty(UrlError))
                return;
                
            try
            {
                IsAnalyzing = true;
                
                // 记录开始时间
                var startTime = DateTime.Now;
                
                // 执行分析
                var result = await _analysisService.AnalyzeUrlAsync(Url);
                Result = result;
                
                // 计算耗时
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                
                // 发布分析完成事件
                var analysisEvent = new NewsAnalysisCompletedEvent(result, true, (long)elapsed);
                EventAggregator.Instance.Publish(analysisEvent);
            }
            catch (Exception ex)
            {
                // 使用全局异常处理器
                ExceptionHandler.HandleException(ex, "URL分析");
            }
            finally
            {
                IsAnalyzing = false;
            }
        }

        /// <summary>
        /// 分析内容
        /// </summary>
        /// <returns>任务</returns>
        private async Task AnalyzeContentAsync()
        {
            ValidateContent();
            if (!string.IsNullOrEmpty(ContentError))
                return;
                
            try
            {
                IsAnalyzing = true;
                
                // 记录开始时间
                var startTime = DateTime.Now;
                
                // 执行分析
                var result = await _analysisService.AnalyzeTextAsync("用户输入内容", Content);
                Result = result;
                
                // 计算耗时
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                
                // 发布分析完成事件
                var analysisEvent = new NewsAnalysisCompletedEvent(result, true, (long)elapsed);
                EventAggregator.Instance.Publish(analysisEvent);
            }
            catch (Exception ex)
            {
                // 使用全局异常处理器
                ExceptionHandler.HandleException(ex, "内容分析");
            }
            finally
            {
                IsAnalyzing = false;
            }
        }

        /// <summary>
        /// 触发错误
        /// </summary>
        private void TriggerError()
        {
            try
            {
                // 故意触发异常
                throw new InvalidOperationException("这是一个测试错误，用于验证全局异常处理机制");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "测试错误", ErrorSeverity.Warning);
            }
        }
        
        /// <summary>
        /// 触发业务错误
        /// </summary>
        private void TriggerBusinessError()
        {
            try
            {
                // 故意触发业务异常
                throw new Exception("这是一个业务逻辑错误，通常由应用程序的业务规则验证产生");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "业务错误", ErrorSeverity.Error);
            }
        }
        
        /// <summary>
        /// 发布测试事件
        /// </summary>
        private void PublishTestEvent()
        {
            // 创建设置变更事件并发布
            var settingEvent = new SettingsChangedEvent("TestSetting", "旧值", "新值");
            EventAggregator.Instance.Publish(settingEvent);
            
            // 显示成功信息
            var successEvent = new ErrorOccurredEvent("测试事件已发布", "事件测试", ErrorSeverity.Info);
            EventAggregator.Instance.Publish(successEvent);
        }

        /// <summary>
        /// 使用导航参数初始化视图模型
        /// </summary>
        /// <param name="parameter">导航参数</param>
        public void InitializeWithParameter(object parameter)
        {
            try
            {
                if (parameter == null)
                {
                    ParameterInfo = "未接收到参数";
                    return;
                }
                
                // 可以根据不同类型的参数进行不同处理
                if (parameter is string textParam)
                {
                    ParameterInfo = $"接收到文本参数: {textParam}";
                    Content = textParam; // 自动填充内容框
                }
                else if (parameter is NewsItem newsItem)
                {
                    ParameterInfo = $"接收到新闻项: {newsItem.Title}";
                    Content = newsItem.Content; // 自动填充内容框
                }
                else if (parameter is AnalysisResult analysisResult)
                {
                    ParameterInfo = $"接收到分析结果: 真实度 {analysisResult.TruthScore:P2}";
                    Result = analysisResult; // 直接显示结果
                }
                else
                {
                    ParameterInfo = $"接收到未知类型参数: {parameter.GetType().Name}";
                }
            }
            catch (Exception ex)
            {
                ParameterInfo = $"处理参数时出错: {ex.Message}";
                EventAggregator.Instance.Publish(new ErrorOccurredEvent(
                    "参数处理错误",
                    $"处理导航参数时出错: {ex.Message}",
                    ErrorSeverity.Error,
                    ex
                ));
            }
        }
    }
} 