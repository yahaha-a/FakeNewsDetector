using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Models;

namespace Client.Services.Interfaces
{
    /// <summary>
    /// 新闻分析服务接口
    /// </summary>
    public interface IAnalysisService
    {
        /// <summary>
        /// 分析新闻真实性
        /// </summary>
        /// <param name="newsItem">新闻项</param>
        /// <returns>分析结果</returns>
        Task<AnalysisResult> AnalyzeNewsAsync(NewsItem newsItem);

        /// <summary>
        /// 分析新闻URL的真实性
        /// </summary>
        /// <param name="url">新闻URL</param>
        /// <returns>分析结果</returns>
        Task<AnalysisResult> AnalyzeUrlAsync(string url);

        /// <summary>
        /// 分析新闻文本的真实性
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <returns>分析结果</returns>
        Task<AnalysisResult> AnalyzeTextAsync(string title, string content);

        /// <summary>
        /// 获取历史分析结果
        /// </summary>
        /// <param name="count">获取数量</param>
        /// <returns>分析结果列表</returns>
        Task<List<AnalysisResult>> GetHistoryAnalysisAsync(int count = 10);

        /// <summary>
        /// 保存分析结果
        /// </summary>
        /// <param name="result">分析结果</param>
        /// <returns>是否成功</returns>
        Task<bool> SaveAnalysisResultAsync(AnalysisResult result);
    }
} 