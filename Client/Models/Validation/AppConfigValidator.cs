using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Client.Models;

namespace Client.Models.Validation;

/// <summary>
/// 应用配置验证器
/// </summary>
public class AppConfigValidator : AbstractValidator<AppConfig>
{
    /// <summary>
    /// 有效的主题类型
    /// </summary>
    private static readonly List<string> ValidThemes = new() { "Light", "Dark", "System" };
    
    /// <summary>
    /// 有效的语言设置
    /// </summary>
    private static readonly List<string> ValidLanguages = new() { "zh-CN", "en-US" };
    
    /// <summary>
    /// 最小窗口宽度
    /// </summary>
    private const int MinWindowWidth = 800;
    
    /// <summary>
    /// 最小窗口高度
    /// </summary>
    private const int MinWindowHeight = 600;
    
    /// <summary>
    /// 最大窗口宽度
    /// </summary>
    private const int MaxWindowWidth = 4000;
    
    /// <summary>
    /// 最大窗口高度
    /// </summary>
    private const int MaxWindowHeight = 3000;
    
    /// <summary>
    /// 最小API超时
    /// </summary>
    private const int MinApiTimeout = 5;
    
    /// <summary>
    /// 最大API超时
    /// </summary>
    private const int MaxApiTimeout = 120;
    
    /// <summary>
    /// 最小重试次数
    /// </summary>
    private const int MinRetryCount = 0;
    
    /// <summary>
    /// 最大重试次数
    /// </summary>
    private const int MaxRetryCount = 10;
    
    /// <summary>
    /// 最小字体大小
    /// </summary>
    private const double MinFontSize = 8;
    
    /// <summary>
    /// 最大字体大小
    /// </summary>
    private const double MaxFontSize = 24;
    
    /// <summary>
    /// 初始化配置验证器
    /// </summary>
    public AppConfigValidator()
    {
        // 主题验证
        RuleFor(x => x.Theme)
            .NotEmpty().WithMessage("主题设置不能为空")
            .Must(theme => theme != null && ValidThemes.Contains(theme))
            .WithMessage($"主题必须是以下之一: {string.Join(", ", ValidThemes)}");
        
        // 语言验证
        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("语言设置不能为空")
            .Must(lang => lang != null && ValidLanguages.Contains(lang))
            .WithMessage($"语言必须是以下之一: {string.Join(", ", ValidLanguages)}");
        
        // 窗口状态验证
        RuleFor(x => x.WindowState).NotNull().WithMessage("窗口状态不能为空");
        
        When(x => x.WindowState != null, () =>
        {
            RuleFor(x => x.WindowState!.Width)
                .InclusiveBetween(MinWindowWidth, MaxWindowWidth)
                .WithMessage($"窗口宽度必须在 {MinWindowWidth} 到 {MaxWindowWidth} 之间");
                
            RuleFor(x => x.WindowState!.Height)
                .InclusiveBetween(MinWindowHeight, MaxWindowHeight)
                .WithMessage($"窗口高度必须在 {MinWindowHeight} 到 {MaxWindowHeight} 之间");
        });
        
        // 最近文件验证
        RuleFor(x => x.RecentFiles)
            .NotNull().WithMessage("最近文件列表不能为空");
            
        RuleForEach(x => x.RecentFiles!)
            .NotEmpty().WithMessage("最近文件路径不能为空");
            
        // API设置验证
        RuleFor(x => x.ApiSettings).NotNull().WithMessage("API设置不能为空");
        
        When(x => x.ApiSettings != null, () =>
        {
            RuleFor(x => x.ApiSettings!.BaseUrl)
                .NotEmpty().WithMessage("API基础URL不能为空")
                .Must(url => url != null && (url.StartsWith("http://") || url.StartsWith("https://")))
                .WithMessage("API基础URL必须以http://或https://开头");
                
            RuleFor(x => x.ApiSettings!.Timeout)
                .InclusiveBetween(MinApiTimeout, MaxApiTimeout)
                .WithMessage($"API超时必须在 {MinApiTimeout} 到 {MaxApiTimeout} 秒之间");
                
            RuleFor(x => x.ApiSettings!.RetryCount)
                .InclusiveBetween(MinRetryCount, MaxRetryCount)
                .WithMessage($"重试次数必须在 {MinRetryCount} 到 {MaxRetryCount} 之间");
        });
        
        // UI设置验证
        RuleFor(x => x.UiSettings).NotNull().WithMessage("UI设置不能为空");
        
        When(x => x.UiSettings != null, () =>
        {
            RuleFor(x => x.UiSettings!.FontSize)
                .InclusiveBetween(MinFontSize, MaxFontSize)
                .WithMessage($"字体大小必须在 {MinFontSize} 到 {MaxFontSize} 之间");
        });
    }
} 