using Client.Models;
using Client.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Services
{
    /// <summary>
    /// 模拟新闻服务实现（用于开发和测试）
    /// </summary>
    public class MockNewsService : INewsService
    {
        private readonly List<NewsItem> _mockNewsDatabase;
        private static readonly Random _random = new Random();

        public MockNewsService()
        {
            // 初始化模拟数据
            _mockNewsDatabase = new List<NewsItem>
            {
                new NewsItem
                {
                    Title = "人工智能在医疗领域取得重大突破",
                    Content = "近日，研究人员利用深度学习算法成功开发出一种能够早期检测癌症的AI系统，准确率高达95%。该系统已在多家医院进行临床试验。专家表示，这一突破将大大提高癌症的早期诊断率，为患者提供更好的治疗机会。",
                    Source = "科技日报",
                    Author = "张明",
                    PublishDate = DateTime.Now.AddDays(-2),
                    Url = "https://example.com/tech/ai-medical-breakthrough",
                    Keywords = new List<string> { "人工智能", "医疗", "癌症检测" }
                },
                new NewsItem
                {
                    Title = "全球气候变化加剧，联合国呼吁各国采取紧急行动",
                    Content = "联合国最新气候报告显示，全球气温持续上升，极端天气事件频发。报告指出，如果不立即采取行动，到2050年，全球平均气温将上升2度以上，导致不可逆转的环境灾难。联合国秘书长呼吁各国加大减排力度，实现碳中和目标。",
                    Source = "环球时报",
                    Author = "李华",
                    PublishDate = DateTime.Now.AddDays(-1),
                    Url = "https://example.com/news/climate-change-un-report",
                    Keywords = new List<string> { "气候变化", "联合国", "环境保护" }
                },
                new NewsItem
                {
                    Title = "外星人已秘密访问地球数十年",
                    Content = "据匿名政府内部人士透露，外星生物已经秘密访问地球超过50年，政府一直在掩盖这一事实。据称，多个国家的政府与外星文明已建立联系，并获得了先进技术。科学家对此说法表示怀疑，称缺乏任何可靠证据。",
                    Source = "神秘世界网",
                    Author = "未知",
                    PublishDate = DateTime.Now.AddDays(-5),
                    Url = "https://example.com/conspiracy/aliens-visiting-earth",
                    Keywords = new List<string> { "外星人", "阴谋论", "政府" }
                },
                new NewsItem
                {
                    Title = "研究发现喝咖啡可以延长寿命",
                    Content = "一项涉及10万人的长期研究表明，每天适量饮用咖啡可以降低多种疾病风险，平均延长寿命3-5年。研究者指出，咖啡中的抗氧化物质是主要功效来源。但专家提醒，过量饮用咖啡可能导致睡眠问题和焦虑。",
                    Source = "健康生活报",
                    Author = "王健",
                    PublishDate = DateTime.Now.AddDays(-3),
                    Url = "https://example.com/health/coffee-longevity-study",
                    Keywords = new List<string> { "咖啡", "健康", "长寿" }
                },
                new NewsItem
                {
                    Title = "神奇药丸可在一周内治愈所有疾病",
                    Content = "一家新兴制药公司宣称研发出一种革命性药物，能够在一周内治愈从感冒到癌症的所有疾病。该公司表示这种药物利用纳米技术直接攻击病原体。多位医学专家对此表示强烈质疑，警告公众不要轻信此类夸大宣传。",
                    Source = "生命奇迹网",
                    Author = "未知",
                    PublishDate = DateTime.Now.AddDays(-7),
                    Url = "https://example.com/health/miracle-pill-cures-all",
                    Keywords = new List<string> { "药物", "虚假宣传", "健康" }
                }
            };
        }

        public async Task<List<NewsItem>> GetTrendingNewsAsync(int count = 10)
        {
            // 模拟网络延迟
            await Task.Delay(300);
            
            // 返回模拟的热门新闻
            return _mockNewsDatabase
                .OrderByDescending(n => n.PublishDate)
                .Take(Math.Min(count, _mockNewsDatabase.Count))
                .ToList();
        }

        public async Task<NewsItem?> GetNewsByUrlAsync(string url)
        {
            // 模拟网络延迟
            await Task.Delay(300);
            
            // 查找匹配URL的新闻
            return _mockNewsDatabase.FirstOrDefault(n => n.Url.Equals(url, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<NewsItem>> SearchNewsAsync(string query, int count = 10)
        {
            // 模拟网络延迟
            await Task.Delay(500);
            
            if (string.IsNullOrWhiteSpace(query))
                return new List<NewsItem>();
            
            // 简单模拟搜索功能
            return _mockNewsDatabase
                .Where(n => 
                    n.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                    n.Content.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    n.Keywords.Any(k => k.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .Take(Math.Min(count, _mockNewsDatabase.Count))
                .ToList();
        }

        public async Task<bool> SaveNewsAsync(NewsItem newsItem)
        {
            // 模拟网络延迟
            await Task.Delay(200);
            
            if (newsItem == null)
                return false;
            
            // 如果已存在则更新，否则添加
            var existingItem = _mockNewsDatabase.FirstOrDefault(n => n.Id == newsItem.Id);
            if (existingItem != null)
            {
                _mockNewsDatabase.Remove(existingItem);
            }
            
            _mockNewsDatabase.Add(newsItem);
            return true;
        }
    }
} 