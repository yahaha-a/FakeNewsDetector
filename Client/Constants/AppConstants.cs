namespace Client.Constants
{
    /// <summary>
    /// 应用常量定义
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public const string AppName = "虚假新闻检测器";

        /// <summary>
        /// 应用版本
        /// </summary>
        public const string AppVersion = "1.0.0";

        /// <summary>
        /// 视图名称
        /// </summary>
        public static class ViewNames
        {
            public const string Home = "Home";
            public const string News = "News";
            public const string Analysis = "Analysis";
            public const string History = "History";
            public const string Settings = "Settings";
        }

        /// <summary>
        /// 设置键名
        /// </summary>
        public static class SettingKeys
        {
            public const string ApiBaseUrl = "ApiBaseUrl";
            public const string IsDarkMode = "IsDarkMode";
            public const string AutoSaveHistory = "AutoSaveHistory";
            public const string LastUsername = "LastUsername";
        }

        /// <summary>
        /// 结果类型对应的颜色
        /// </summary>
        public static class ResultColors
        {
            public const string Real = "#2ECC71";       // 绿色
            public const string Suspicious = "#F39C12"; // 黄色
            public const string Fake = "#E74C3C";       // 红色
            public const string Unknown = "#95A5A6";    // 灰色
        }
    }
} 