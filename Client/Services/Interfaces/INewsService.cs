using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Models;

namespace Client.Services.Interfaces
{
    /// <summary>
    /// 新闻服务接口
    /// </summary>
    public interface INewsService
    {
        /// <summary>
        /// 获取热门新闻列表
        /// </summary>
        /// <param name="count">获取数量</param>
        /// <returns>新闻列表</returns>
        Task<List<NewsItem>> GetTrendingNewsAsync(int count = 10);

        /// <summary>
        /// 根据URL获取新闻
        /// </summary>
        /// <param name="url">新闻URL</param>
        /// <returns>新闻项</returns>
        Task<NewsItem?> GetNewsByUrlAsync(string url);

        /// <summary>
        /// 搜索新闻
        /// </summary>
        /// <param name="query">搜索关键词</param>
        /// <param name="count">获取数量</param>
        /// <returns>新闻列表</returns>
        Task<List<NewsItem>> SearchNewsAsync(string query, int count = 10);

        /// <summary>
        /// 保存新闻
        /// </summary>
        /// <param name="newsItem">新闻项</param>
        /// <returns>是否成功</returns>
        Task<bool> SaveNewsAsync(NewsItem newsItem);
    }
} 