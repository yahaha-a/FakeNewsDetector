using Client.Helpers;
using Client.Helpers.Validation;
using Client.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Client.ViewModels.Base
{
    /// <summary>
    /// 支持验证的视图模型基类
    /// </summary>
    public abstract class ValidatableViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        /// <summary>
        /// 是否有错误
        /// </summary>
        public bool HasErrors => _errors.Count > 0;

        /// <summary>
        /// 错误状态变更事件
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <summary>
        /// 获取指定属性的错误
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <returns>错误集合</returns>
        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
                return Array.Empty<string>();
                
            return _errors[propertyName];
        }

        /// <summary>
        /// 设置属性值并验证
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="storage">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>是否改变</returns>
        protected new bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            bool changed = base.SetProperty(ref storage, value, propertyName);
            
            if (changed && !string.IsNullOrEmpty(propertyName))
                ValidatePropertyInternal(propertyName);
                
            return changed;
        }

        /// <summary>
        /// 验证所有属性
        /// </summary>
        /// <returns>是否验证通过</returns>
        protected bool ValidateAllProperties()
        {
            var validationResults = new List<ValidationResult>();
            bool isValid = this.Validate(validationResults);
            
            // 清空旧错误
            _errors.Clear();
            
            // 添加新错误
            foreach (var result in validationResults)
            {
                if (result.MemberNames != null && result.MemberNames.Any())
                {
                    foreach (var memberName in result.MemberNames)
                    {
                        AddError(memberName, result.ErrorMessage);
                    }
                }
                else if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    // 如果没有指定成员名，添加到全局错误
                    AddError(string.Empty, result.ErrorMessage);
                }
            }
            
            // 触发全局错误变更事件
            OnErrorsChanged(string.Empty);
            
            return isValid;
        }

        /// <summary>
        /// 验证单个属性
        /// </summary>
        /// <param name="propertyName">属性名</param>
        protected void ValidatePropertyInternal(string propertyName)
        {
            try
            {
                // 清空此属性的旧错误
                _errors.Remove(propertyName);
                
                // 获取验证错误
                var errorMessages = ValidationExtensions.ValidateProperty(this, propertyName);
                
                // 添加新错误
                foreach (var error in errorMessages)
                {
                    AddError(propertyName, error);
                }
                
                // 触发错误变更事件
                OnErrorsChanged(propertyName);
            }
            catch (Exception ex)
            {
                SerilogLoggerService.Instance.LogComponentError(
                    ex,
                    LogContext.Components.Validation,
                    LogContext.Actions.Validate,
                    $"属性验证失败 - 属性: {propertyName}, 类型: {GetType().Name}");
            }
        }

        /// <summary>
        /// 添加错误
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="errorMessage">错误消息</param>
        protected void AddError(string propertyName, string? errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                return;
                
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();
                
            if (!_errors[propertyName].Contains(errorMessage))
                _errors[propertyName].Add(errorMessage);
        }

        /// <summary>
        /// 触发错误变更事件
        /// </summary>
        /// <param name="propertyName">属性名</param>
        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            OnPropertyChanged(nameof(HasErrors));
        }

        /// <summary>
        /// 清除所有错误
        /// </summary>
        protected void ClearAllErrors()
        {
            var propertyNames = _errors.Keys.ToList();
            _errors.Clear();
            
            foreach (var propertyName in propertyNames)
            {
                OnErrorsChanged(propertyName);
            }
        }

        /// <summary>
        /// 清除指定属性错误
        /// </summary>
        /// <param name="propertyName">属性名</param>
        protected void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }
    }
} 