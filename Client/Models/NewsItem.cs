using System;
using System.Collections.Generic;

namespace Client.Models
{
    /// <summary>
    /// 新闻项模型
    /// </summary>
    public class NewsItem
    {
        /// <summary>
        /// 新闻ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 新闻标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 新闻内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 新闻来源
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// 发布日期
        /// </summary>
        public DateTime PublishDate { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// 新闻URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 关键词列表
        /// </summary>
        public List<string> Keywords { get; set; } = new List<string>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public NewsItem()
        {
            Id = Guid.NewGuid().ToString();
            PublishDate = DateTime.Now;
        }
    }
} 