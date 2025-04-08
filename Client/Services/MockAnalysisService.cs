using Client.Models;
using Client.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Services
{
    /// <summary>
    /// 模拟新闻分析服务实现（用于开发和测试）
    /// </summary>
    public class MockAnalysisService : IAnalysisService
    {
        private readonly INewsService _newsService;
        private readonly List<AnalysisResult> _mockAnalysisHistory;
        private static readonly Random _random = new Random();

        public MockAnalysisService(INewsService newsService)
        {
            _newsService = newsService;
            _mockAnalysisHistory = new List<AnalysisResult>();
        }

        public async Task<AnalysisResult> AnalyzeNewsAsync(NewsItem newsItem)
        {
            try
            {
                // 模拟网络延迟和处理时间
                await Task.Delay(1000);
    
                if (newsItem == null)
                    throw new ArgumentNullException(nameof(newsItem));
    
                // 模拟分析结果
                var result = GenerateAnalysisResult(newsItem);
                
                // 添加一致性检查，确保结果状态正确
                EnsureConsistentResultState(result);
                
                // 添加到历史记录
                _mockAnalysisHistory.Add(result);
                
                return result;
            }
            catch (Exception ex)
            {
                // 创建一个错误结果而不是抛出异常
                var errorResult = new AnalysisResult
                {
                    NewsItem = newsItem ?? new NewsItem { Title = "未知标题", Content = "无内容" },
                    AnalysisTime = DateTime.Now,
                    ResultType = ResultType.Unknown,
                    TruthScore = 0.5, // 中性得分
                    Confidence = 0,   // 无信心
                    Summary = $"分析过程中发生错误: {ex.Message}",
                    DetailedAnalysis = "由于技术原因，无法完成分析。请稍后重试。"
                };
                
                return errorResult;
            }
        }

        public async Task<AnalysisResult> AnalyzeUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL不能为空", nameof(url));

            // 获取新闻内容
            var newsItem = await _newsService.GetNewsByUrlAsync(url);
            
            if (newsItem == null)
                throw new Exception("无法获取该URL的新闻内容");

            // 分析新闻
            return await AnalyzeNewsAsync(newsItem);
        }

        public async Task<AnalysisResult> AnalyzeTextAsync(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("内容不能为空", nameof(content));

            // 创建临时新闻项
            var newsItem = new NewsItem
            {
                Title = title ?? "未知标题",
                Content = content,
                PublishDate = DateTime.Now,
                Source = "用户提供",
                Author = "未知",
                Url = $"local://{Guid.NewGuid()}"
            };

            // 分析新闻
            return await AnalyzeNewsAsync(newsItem);
        }

        public async Task<List<AnalysisResult>> GetHistoryAnalysisAsync(int count = 10)
        {
            // 模拟网络延迟
            await Task.Delay(300);
            
            // 返回历史分析
            return _mockAnalysisHistory
                .OrderByDescending(a => a.AnalysisTime)
                .Take(Math.Min(count, _mockAnalysisHistory.Count))
                .ToList();
        }

        public async Task<bool> SaveAnalysisResultAsync(AnalysisResult result)
        {
            // 模拟网络延迟
            await Task.Delay(200);
            
            if (result == null)
                return false;
            
            // 如果已存在则更新，否则添加
            var existingResult = _mockAnalysisHistory.FirstOrDefault(r => r.Id == result.Id);
            if (existingResult != null)
            {
                _mockAnalysisHistory.Remove(existingResult);
            }
            
            _mockAnalysisHistory.Add(result);
            return true;
        }

        #region 辅助方法

        /// <summary>
        /// 确保分析结果状态一致
        /// </summary>
        private void EnsureConsistentResultState(AnalysisResult result)
        {
            // 确保TruthScore在0-1之间
            result.TruthScore = Math.Clamp(result.TruthScore, 0, 1);
            
            // 确保Confidence在0-1之间
            result.Confidence = Math.Clamp(result.Confidence, 0, 1);
            
            // 根据TruthScore重新设置ResultType，确保状态一致
            if (result.TruthScore < 0.4)
            {
                result.ResultType = ResultType.Fake;
                
                // 如果摘要为空，添加默认摘要
                if (string.IsNullOrEmpty(result.Summary))
                {
                    result.Summary = "该新闻很可能是虚假的。内容包含明显虚构信息，缺乏可靠来源，请勿轻信。";
                }
            }
            else if (result.TruthScore < 0.7)
            {
                result.ResultType = ResultType.Suspicious;
                
                // 如果摘要为空，添加默认摘要
                if (string.IsNullOrEmpty(result.Summary))
                {
                    result.Summary = "该新闻可能包含误导性信息。部分内容缺乏事实依据，或存在夸大事实的情况。建议从其他渠道验证信息。";
                }
            }
            else
            {
                result.ResultType = ResultType.Real;
                
                // 如果摘要为空，添加默认摘要
                if (string.IsNullOrEmpty(result.Summary))
                {
                    result.Summary = "该新闻很可能是真实的。内容符合事实，来源可靠，没有发现虚假信息的常见特征。";
                }
            }
            
            // 确保不返回空的特征列表
            if (result.Features == null || !result.Features.Any())
            {
                result.Features = new List<FeatureAnalysis>
                {
                    new FeatureAnalysis
                    {
                        Name = "基础分析",
                        Value = result.TruthScore,
                        Description = "基于内容的综合评估",
                        Impact = result.TruthScore >= 0.5 ? FeatureImpact.Positive : FeatureImpact.Negative,
                        Importance = 1.0
                    }
                };
            }
        }

        private AnalysisResult GenerateAnalysisResult(NewsItem newsItem)
        {
            // 基于新闻内容生成模拟分析结果
            // 这里使用一些简单规则来模拟分析

            var result = new AnalysisResult
            {
                NewsItem = newsItem,
                AnalysisTime = DateTime.Now
            };

            try
            {
                // 判断标题是否包含可疑词汇
                bool hasSuspiciousTitle = ContainsSuspiciousKeywords(newsItem.Title);
                
                // 判断内容是否包含可疑词汇
                bool hasSuspiciousContent = ContainsSuspiciousKeywords(newsItem.Content);
                
                // 来源可信度评估
                bool isReliableSource = IsReliableSource(newsItem.Source);
                
                // 计算真实性得分 (0-1)
                double truthScore = CalculateTruthScore(hasSuspiciousTitle, hasSuspiciousContent, isReliableSource);
                result.TruthScore = truthScore;
                
                // 设置结果类型 - 这些设置会在EnsureConsistentResultState中被再次验证
                if (truthScore >= 0.7)
                {
                    result.ResultType = ResultType.Real;
                    result.Summary = "该新闻很可能是真实的。内容符合事实，来源可靠，没有发现虚假信息的常见特征。";
                }
                else if (truthScore >= 0.4)
                {
                    result.ResultType = ResultType.Suspicious;
                    result.Summary = "该新闻可能包含误导性信息。部分内容缺乏事实依据，或存在夸大事实的情况。建议从其他渠道验证信息。";
                }
                else
                {
                    result.ResultType = ResultType.Fake;
                    result.Summary = "该新闻很可能是虚假的。内容包含明显虚构信息，缺乏可靠来源，请勿轻信。";
                }
                
                // 模拟可信度 (0.6-0.95)
                result.Confidence = 0.6 + (_random.NextDouble() * 0.35);
                
                // 添加特征分析
                result.Features = GenerateFeatures(newsItem, hasSuspiciousTitle, hasSuspiciousContent, isReliableSource);
            }
            catch (Exception)
            {
                // 发生异常时设置默认值
                result.TruthScore = 0.5;
                result.ResultType = ResultType.Unknown;
                result.Confidence = 0.3;
                result.Summary = "分析过程中出现意外情况，结果可能不准确。";
            }
            
            return result;
        }

        private bool ContainsSuspiciousKeywords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
                
            string[] suspiciousKeywords = { 
                "震惊", "惊爆", "突发", "神奇", "秘密", "不可思议", "百分百", "无敌", "万能", 
                "秘诀", "揭秘", "一夜暴富", "100%", "立即", "官方认证", "钱包爆炸",
                "医生不会告诉你", "药物公司不想让你知道", "政府掩盖", "阴谋"
            };
            
            return suspiciousKeywords.Any(keyword => 
                text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsReliableSource(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return false;
                
            string[] reliableSources = { 
                "人民日报", "新华社", "中央电视台", "科技日报", "经济日报",
                "BBC", "CNN", "路透社", "美联社", "纽约时报", "华尔街日报",
                "科学杂志", "自然杂志", "柳叶刀", "环球时报", "中国日报" 
            };
            
            return reliableSources.Any(s => 
                source.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        private double CalculateTruthScore(bool hasSuspiciousTitle, bool hasSuspiciousContent, bool isReliableSource)
        {
            double score = 0.5; // 初始中间值
            
            // 标题可疑性影响
            if (hasSuspiciousTitle)
                score -= 0.2;
            else
                score += 0.1;
                
            // 内容可疑性影响
            if (hasSuspiciousContent)
                score -= 0.3;
            else
                score += 0.2;
                
            // 来源可靠性影响
            if (isReliableSource)
                score += 0.3;
            else
                score -= 0.1;
                
            // 添加一些随机性
            score += (_random.NextDouble() * 0.2) - 0.1;
            
            // 限制范围在0-1之间
            return Math.Clamp(score, 0, 1);
        }
        
        private List<FeatureAnalysis> GenerateFeatures(NewsItem newsItem, bool hasSuspiciousTitle, 
            bool hasSuspiciousContent, bool isReliableSource)
        {
            var features = new List<FeatureAnalysis>();
            
            // 1. 标题分析
            features.Add(new FeatureAnalysis
            {
                Name = "标题分析",
                Value = hasSuspiciousTitle ? 0.3 : 0.8,
                Description = hasSuspiciousTitle 
                    ? "标题中包含夸张或情绪化词汇，可能意在吸引点击而非准确报道事实" 
                    : "标题客观，没有明显的夸张或情绪化倾向",
                Impact = hasSuspiciousTitle ? FeatureImpact.Negative : FeatureImpact.Positive,
                Importance = 0.7
            });
            
            // 2. 内容分析
            features.Add(new FeatureAnalysis
            {
                Name = "内容分析",
                Value = hasSuspiciousContent ? 0.2 : 0.9,
                Description = hasSuspiciousContent 
                    ? "内容中存在夸张或误导性表述，缺乏具体数据支持" 
                    : "内容表述客观，论据充分",
                Impact = hasSuspiciousContent ? FeatureImpact.Negative : FeatureImpact.Positive,
                Importance = 0.9
            });
            
            // 3. 来源分析
            features.Add(new FeatureAnalysis
            {
                Name = "来源可信度",
                Value = isReliableSource ? 0.9 : 0.4,
                Description = isReliableSource 
                    ? "来源为知名可靠媒体，有良好信誉记录" 
                    : "来源不明或缺乏公信力，需谨慎对待",
                Impact = isReliableSource ? FeatureImpact.Positive : FeatureImpact.Negative,
                Importance = 0.8
            });
            
            // 4. 情感分析
            double sentimentScore = 0.5 + (_random.NextDouble() * 0.5) - 0.25;
            bool isNeutral = sentimentScore > 0.4 && sentimentScore < 0.6;
            features.Add(new FeatureAnalysis
            {
                Name = "情感倾向",
                Value = sentimentScore,
                Description = isNeutral 
                    ? "内容情感倾向中立，客观报道事实" 
                    : "内容情感倾向明显，可能影响客观性",
                Impact = isNeutral ? FeatureImpact.Positive : FeatureImpact.Neutral,
                Importance = 0.6
            });
            
            // 5. 出版时间分析
            bool isRecent = (DateTime.Now - newsItem.PublishDate).TotalDays < 7;
            features.Add(new FeatureAnalysis
            {
                Name = "时效性",
                Value = isRecent ? 0.9 : 0.6,
                Description = isRecent 
                    ? "新闻发布时间较近，内容时效性强" 
                    : "新闻发布时间较早，内容可能已过时",
                Impact = FeatureImpact.Neutral,
                Importance = 0.4
            });
            
            return features;
        }

        #endregion
    }
} 