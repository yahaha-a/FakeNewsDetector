using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Client.Helpers.Events;
using Client.Services;
using Client.ViewModels.Controls;
using System;
using Client.Constants;

namespace Client.Views.Controls
{
    public partial class ErrorNotification : UserControl
    {
        public ErrorNotification()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// 从错误事件创建通知并添加到通知区域
        /// </summary>
        /// <param name="notificationHost">通知宿主容器</param>
        /// <param name="errorEvent">错误事件</param>
        /// <returns>创建的通知控件</returns>
        public static ErrorNotification FromEvent(Panel notificationHost, ErrorOccurredEvent errorEvent)
        {
            // 创建通知
            var notification = new ErrorNotification();
            
            // 将错误严重程度转换为通知严重程度
            var severity = ConvertErrorSeverity(errorEvent.Severity);
            
            // 创建通知视图模型
            notification.DataContext = new ErrorNotificationViewModel(
                $"来自 {errorEvent.Source} 的{GetSeverityText(severity)}",
                errorEvent.Message,
                severity,
                () => notificationHost.Children.Remove(notification),
                errorEvent.Exception != null ? () => ShowExceptionDetails(errorEvent.Exception) : null
            );
            
            // 添加到通知区域
            notificationHost.Children.Add(notification);
            
            return notification;
        }
        
        /// <summary>
        /// 显示异常详情
        /// </summary>
        /// <param name="exception">异常</param>
        private static void ShowExceptionDetails(Exception exception)
        {
            // 在实际应用中，这里可以显示一个模态对话框
            // 使用Serilog记录异常信息
            SerilogLoggerService.Instance.Debug("ErrorNotification: 异常详情:");
            SerilogLoggerService.Instance.Debug("ErrorNotification: 类型: {ExceptionType}", exception.GetType().FullName);
            SerilogLoggerService.Instance.Debug("ErrorNotification: 消息: {ExceptionMessage}", exception.Message);
            SerilogLoggerService.Instance.Debug("ErrorNotification: 堆栈跟踪: {StackTrace}", exception.StackTrace);
            
            if (exception.InnerException != null)
            {
                SerilogLoggerService.Instance.Debug("ErrorNotification: 内部异常:");
                SerilogLoggerService.Instance.Debug("ErrorNotification: 类型: {InnerExceptionType}", exception.InnerException.GetType().FullName);
                SerilogLoggerService.Instance.Debug("ErrorNotification: 消息: {InnerExceptionMessage}", exception.InnerException.Message);
                SerilogLoggerService.Instance.Debug("ErrorNotification: 堆栈跟踪: {InnerStackTrace}", exception.InnerException.StackTrace);
            }
        }
        
        /// <summary>
        /// 将ErrorSeverity转换为NotificationSeverity
        /// </summary>
        private static NotificationSeverity ConvertErrorSeverity(ErrorSeverity severity)
        {
            return severity switch
            {
                ErrorSeverity.Info => NotificationSeverity.Info,
                ErrorSeverity.Warning => NotificationSeverity.Warning,
                ErrorSeverity.Error => NotificationSeverity.Error,
                ErrorSeverity.Critical => NotificationSeverity.Error,
                _ => NotificationSeverity.Error
            };
        }
        
        /// <summary>
        /// 获取严重程度对应的文本描述
        /// </summary>
        private static string GetSeverityText(NotificationSeverity severity)
        {
            return severity switch
            {
                NotificationSeverity.Success => "成功",
                NotificationSeverity.Info => "信息",
                NotificationSeverity.Warning => "警告",
                NotificationSeverity.Error => "错误",
                _ => "通知"
            };
        }

        /// <summary>
        /// 创建一个错误通知
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息</param>
        /// <param name="onClose">关闭回调</param>
        /// <returns>创建的通知控件</returns>
        public static ErrorNotification CreateError(string title, string message, System.Action onClose)
        {
            var notification = new ErrorNotification();
            notification.DataContext = new ErrorNotificationViewModel(
                title, 
                message, 
                NotificationSeverity.Error, 
                onClose);
            return notification;
        }

        /// <summary>
        /// 创建一个警告通知
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息</param>
        /// <param name="onClose">关闭回调</param>
        /// <returns>创建的通知控件</returns>
        public static ErrorNotification CreateWarning(string title, string message, System.Action onClose)
        {
            var notification = new ErrorNotification();
            notification.DataContext = new ErrorNotificationViewModel(
                title, 
                message, 
                NotificationSeverity.Warning, 
                onClose);
            return notification;
        }

        /// <summary>
        /// 创建一个信息通知
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息</param>
        /// <param name="onClose">关闭回调</param>
        /// <returns>创建的通知控件</returns>
        public static ErrorNotification CreateInfo(string title, string message, System.Action onClose)
        {
            var notification = new ErrorNotification();
            notification.DataContext = new ErrorNotificationViewModel(
                title, 
                message, 
                NotificationSeverity.Info, 
                onClose);
            return notification;
        }

        /// <summary>
        /// 创建一个成功通知
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息</param>
        /// <param name="onClose">关闭回调</param>
        /// <returns>创建的通知控件</returns>
        public static ErrorNotification CreateSuccess(string title, string message, System.Action onClose)
        {
            var notification = new ErrorNotification();
            notification.DataContext = new ErrorNotificationViewModel(
                title, 
                message, 
                NotificationSeverity.Success, 
                onClose);
            return notification;
        }

        /// <summary>
        /// 创建一个错误通知并添加到面板
        /// </summary>
        /// <param name="panel">目标面板</param>
        /// <param name="title">标题</param>
        /// <param name="message">消息</param>
        /// <param name="severity">严重性</param>
        /// <returns>创建的通知控件</returns>
        public static ErrorNotification AddToPanel(Panel panel, string title, string message, NotificationSeverity severity)
        {
            var notification = new ErrorNotification();
            notification.DataContext = new ErrorNotificationViewModel(
                title, 
                message, 
                severity, 
                () => panel.Children.Remove(notification)
            );
            
            panel.Children.Add(notification);
            return notification;
        }
    }
} 