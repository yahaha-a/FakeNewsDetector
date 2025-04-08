using Avalonia.Styling;
using System;

namespace Client.Helpers.Events
{
    /// <summary>
    /// 主题变更事件
    /// </summary>
    public class ThemeChangedEvent
    {
        /// <summary>
        /// 旧主题
        /// </summary>
        public ThemeVariant OldTheme { get; }
        
        /// <summary>
        /// 新主题
        /// </summary>
        public ThemeVariant NewTheme { get; }
        
        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; }
        
        /// <summary>
        /// 创建主题变更事件
        /// </summary>
        /// <param name="oldTheme">旧主题</param>
        /// <param name="newTheme">新主题</param>
        public ThemeChangedEvent(ThemeVariant oldTheme, ThemeVariant newTheme)
        {
            OldTheme = oldTheme;
            NewTheme = newTheme;
            ChangeTime = DateTime.Now;
        }
    }
} 