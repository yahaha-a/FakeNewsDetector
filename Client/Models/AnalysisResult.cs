using System;
using System.Collections.Generic;

namespace Client.Models
{
    /// <summary>
    /// 新闻分析特征
    /// </summary>
    public class FeatureAnalysis
    {
        /// <summary>
        /// 特征名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 特征值
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 特征描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 特征对结果的影响类型
        /// </summary>
        public FeatureImpact Impact { get; set; }

        /// <summary>
        /// 特征重要性（影响程度，0-1范围）
        /// </summary>
        public double Importance { get; set; }
    }

    /// <summary>
    /// 新闻分析结果
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>
        /// 分析ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 关联的新闻项
        /// </summary>
        public NewsItem NewsItem { get; set; } = null!;

        /// <summary>
        /// 分析时间
        /// </summary>
        public DateTime AnalysisTime { get; set; }

        /// <summary>
        /// 结果类型（真实/可疑/虚假）
        /// </summary>
        public ResultType ResultType { get; set; }

        /// <summary>
        /// 真实性得分（0-1之间，越高越真实）
        /// </summary>
        public double TruthScore { get; set; }

        /// <summary>
        /// 可信度（0-1之间，模型对结果的确信程度）
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// 分析摘要
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// 详细分析结果
        /// </summary>
        public string DetailedAnalysis { get; set; } = string.Empty;

        /// <summary>
        /// 特征分析列表
        /// </summary>
        public List<FeatureAnalysis> Features { get; set; } = new List<FeatureAnalysis>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public AnalysisResult()
        {
            Id = Guid.NewGuid().ToString();
            AnalysisTime = DateTime.Now;
            ResultType = ResultType.Unknown;
        }
    }
} 