using System;

namespace Client.Models
{
    /// <summary>
    /// 分析结果包装器，为视图提供更友好的属性
    /// </summary>
    public class AnalysisResultWrapper
    {
        private readonly AnalysisResult? _result;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="result">原始分析结果</param>
        public AnalysisResultWrapper(AnalysisResult? result)
        {
            _result = result;
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title => _result?.NewsItem.Title ?? string.Empty;

        /// <summary>
        /// 内容
        /// </summary>
        public string Content => _result?.NewsItem.Content ?? string.Empty;

        /// <summary>
        /// 是假新闻的可能性（1-TruthScore，即真实性的反面）
        /// </summary>
        public double IsLikely => _result != null ? 1.0 - _result.TruthScore : 0;

        /// <summary>
        /// 可信度（百分比形式）
        /// </summary>
        public double Confidence => _result != null ? _result.TruthScore * 100 : 0;

        /// <summary>
        /// 分析原因/说明
        /// </summary>
        public string Reason => _result?.Summary ?? string.Empty;

        /// <summary>
        /// 详细描述
        /// </summary>
        public string Description => _result?.DetailedAnalysis ?? string.Empty;

        /// <summary>
        /// 分析时间
        /// </summary>
        public DateTime AnalysisTime => _result?.AnalysisTime ?? DateTime.MinValue;

        /// <summary>
        /// 创建日期（用于显示）
        /// </summary>
        public DateTime DateCreated => _result?.AnalysisTime ?? DateTime.MinValue;

        /// <summary>
        /// 分析结果类型（用于分类）
        /// </summary>
        public ResultType ResultType => _result?.ResultType ?? ResultType.Unknown;

        /// <summary>
        /// 是否为真实新闻
        /// </summary>
        public bool IsReal => _result?.ResultType == ResultType.Real;

        /// <summary>
        /// 是否为可疑新闻
        /// </summary>
        public bool IsSuspicious => _result?.ResultType == ResultType.Suspicious;

        /// <summary>
        /// 是否为虚假新闻
        /// </summary>
        public bool IsFake => _result?.ResultType == ResultType.Fake;
    }
} 