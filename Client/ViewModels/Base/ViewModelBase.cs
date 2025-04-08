using CommunityToolkit.Mvvm.ComponentModel;
using Client.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Client.ViewModels.Base
{
    /// <summary>
    /// 视图模型基类
    /// </summary>
    public abstract class ViewModelBase : ObservableObject
    {
        private bool _isBusy;
        private string _title = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isLoaded;

        /// <summary>
        /// 是否繁忙
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// 是否已加载
        /// </summary>
        public bool IsLoaded
        {
            get => _isLoaded;
            set => SetProperty(ref _isLoaded, value);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        protected ViewModelBase()
        {
            // 使用类型名作为默认标题（去除ViewModel后缀）
            string typeName = GetType().Name;
            if (typeName.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase))
            {
                Title = typeName.Substring(0, typeName.Length - "ViewModel".Length);
            }
            else
            {
                Title = typeName;
            }
        }

        /// <summary>
        /// 视图加载时调用
        /// </summary>
        public virtual void OnLoaded()
        {
            if (!IsLoaded)
            {
                LoadAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 异步加载数据
        /// </summary>
        protected virtual async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                await Task.Delay(10); // 确保UI线程释放
                IsLoaded = true;
            }
            catch (Exception ex)
            {
                SerilogLoggerService.Instance.Error(ex, "ViewModelBase: 加载异常 ({ViewModel})", GetType().Name);
                StatusMessage = $"加载出错: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// 视图卸载时调用
        /// </summary>
        public virtual void OnUnloaded()
        {
            // 视图卸载时可以执行清理操作
        }

        /// <summary>
        /// 安全执行异步任务的辅助方法
        /// </summary>
        protected async Task ExecuteAsync(Func<Task> task, string? busyMessage = null)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                if (!string.IsNullOrEmpty(busyMessage))
                {
                    StatusMessage = busyMessage;
                }

                await task();
            }
            catch (Exception ex)
            {
                SerilogLoggerService.Instance.Error(ex, "ViewModelBase: 执行异常 ({ViewModel})", GetType().Name);
                StatusMessage = $"操作出错: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                StatusMessage = string.Empty;
            }
        }

        /// <summary>
        /// 安全执行异步任务的辅助方法
        /// </summary>
        protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> task, string? busyMessage = null)
        {
            if (IsBusy)
                return default;

            try
            {
                IsBusy = true;
                if (!string.IsNullOrEmpty(busyMessage))
                {
                    StatusMessage = busyMessage;
                }

                return await task();
            }
            catch (Exception ex)
            {
                SerilogLoggerService.Instance.Error(ex, "ViewModelBase: 执行异常 ({ViewModel})", GetType().Name);
                StatusMessage = $"操作出错: {ex.Message}";
                return default;
            }
            finally
            {
                IsBusy = false;
                StatusMessage = string.Empty;
            }
        }
    }
} 