using Avalonia.Threading;
using Client.Helpers.Events;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia.Controls;
using Client.Constants;
using Client.ViewModels.Base;

namespace Client.ViewModels.Controls
{
    /// <summary>
    /// 错误通知视图模型
    /// </summary>
    public class ErrorNotificationViewModel : ViewModelBase
    {
        private string _title = string.Empty;
        private string _message = string.Empty;
        private NotificationSeverity _severity = NotificationSeverity.Info;

        /// <summary>
        /// 标题
        /// </summary>
        public new string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        /// <summary>
        /// 严重性级别
        /// </summary>
        public NotificationSeverity Severity
        {
            get => _severity;
            set => SetProperty(ref _severity, value);
        }

        /// <summary>
        /// 关闭命令
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// 显示详情命令
        /// </summary>
        public ICommand? ShowDetailsCommand { get; }

        /// <summary>
        /// 创建一个新的错误通知视图模型
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息</param>
        /// <param name="severity">严重性</param>
        /// <param name="onClose">关闭回调</param>
        /// <param name="onShowDetails">显示详情回调（可选）</param>
        public ErrorNotificationViewModel(
            string title,
            string message,
            NotificationSeverity severity,
            Action onClose,
            Action? onShowDetails = null)
        {
            _title = title;
            _message = message;
            _severity = severity;
            
            CloseCommand = new RelayCommand(onClose);
            
            if (onShowDetails != null)
            {
                ShowDetailsCommand = new RelayCommand(onShowDetails);
            }
        }
        
        /// <summary>
        /// 用于设计时的默认构造函数
        /// </summary>
        public ErrorNotificationViewModel()
            : this("通知标题", "这是一条通知消息", NotificationSeverity.Info, () => { })
        {
        }
    }
} 