using System;

namespace Client.Helpers
{
    /// <summary>
    /// 日志上下文常量
    /// </summary>
    public static class LogContext
    {
        /// <summary>
        /// 组件常量
        /// </summary>
        public static class Components
        {
            public const string App = "App";
            public const string Program = "Program";
            public const string DependencyContainer = "DependencyContainer";
            public const string MainWindow = "MainWindow";
            public const string HomeView = "HomeView";
            public const string Navigation = "Navigation";
            public const string Settings = "Settings";
            public const string Analysis = "Analysis";
            public const string History = "History";
            public const string Statistics = "Statistics";
            public const string Detection = "Detection";
            public const string Test = "Test";
            public const string EventAggregator = "EventAggregator";
            public const string ErrorNotification = "ErrorNotification";
            public const string Validation = "Validation";
        }
        
        /// <summary>
        /// 操作常量
        /// </summary>
        public static class Actions
        {
            public const string Initialize = "初始化";
            public const string Start = "启动";
            public const string Load = "加载";
            public const string Save = "保存";
            public const string Update = "更新";
            public const string Create = "创建";
            public const string Delete = "删除";
            public const string Reset = "重置";
            public const string Navigate = "导航";
            public const string Execute = "执行";
            public const string Process = "处理";
            public const string Validate = "验证";
            public const string Analyze = "分析";
            public const string Configure = "配置";
            public const string Shutdown = "关闭";
            public const string Cleanup = "清理";
            public const string ChangeTheme = "更改主题";
            public const string ChangeLanguage = "更改语言";
            public const string ChangeSettings = "更改设置";
        }
        
        /// <summary>
        /// 状态常量
        /// </summary>
        public static class States
        {
            public const string Started = "已启动";
            public const string Completed = "已完成";
            public const string Failed = "失败";
            public const string InProgress = "进行中";
            public const string Success = "成功";
        }
    }
} 