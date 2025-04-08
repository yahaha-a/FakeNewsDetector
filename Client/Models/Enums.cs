using System;

namespace Client.Models
{
    /// <summary>
    /// 新闻检测结果类型
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown,
        
        /// <summary>
        /// 真实新闻
        /// </summary>
        Real,
        
        /// <summary>
        /// 可疑新闻
        /// </summary>
        Suspicious,
        
        /// <summary>
        /// 虚假新闻
        /// </summary>
        Fake
    }

    /// <summary>
    /// 特征影响类型
    /// </summary>
    public enum FeatureImpact
    {
        /// <summary>
        /// 负面影响(降低真实性)
        /// </summary>
        Negative,
        
        /// <summary>
        /// 中性影响
        /// </summary>
        Neutral,
        
        /// <summary>
        /// 正面影响(增加真实性)
        /// </summary>
        Positive
    }
} 