using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Client.Helpers.Validation
{
    /// <summary>
    /// 验证工具扩展方法
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// 验证对象是否有效
        /// </summary>
        /// <param name="obj">要验证的对象</param>
        /// <param name="validationResults">验证结果集合</param>
        /// <returns>是否验证通过</returns>
        public static bool Validate(this object obj, ICollection<ValidationResult>? validationResults = null)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var results = validationResults ?? new List<ValidationResult>();
            var context = new ValidationContext(obj, null, null);
            
            return Validator.TryValidateObject(obj, context, results, true);
        }

        /// <summary>
        /// 验证对象属性是否有效
        /// </summary>
        /// <param name="obj">要验证的对象</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>验证错误消息列表</returns>
        public static IEnumerable<string> ValidateProperty(this object obj, string propertyName)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
                
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("属性名不能为空", nameof(propertyName));
                
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null)
                throw new ArgumentException($"类型 {obj.GetType().Name} 没有属性 {propertyName}", nameof(propertyName));

            var value = property.GetValue(obj);
            var context = new ValidationContext(obj, null, null) { MemberName = propertyName };
            var results = new List<ValidationResult>();
            
            Validator.TryValidateProperty(value, context, results);
            
            return results.Select(r => r.ErrorMessage).Where(m => !string.IsNullOrEmpty(m)).ToList()!;
        }

        /// <summary>
        /// 获取属性显示名称
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>显示名称</returns>
        public static string GetDisplayName(this Type type, string propertyName)
        {
            var property = type.GetProperty(propertyName);
            if (property == null)
                return propertyName;
                
            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null && !string.IsNullOrEmpty(displayAttribute.Name))
                return displayAttribute.Name;
                
            var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();
            if (displayNameAttribute != null && !string.IsNullOrEmpty(displayNameAttribute.DisplayName))
                return displayNameAttribute.DisplayName;
                
            return propertyName;
        }

        /// <summary>
        /// 获取属性的验证错误消息
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>错误消息</returns>
        public static string GetErrorMessage(this object obj, string propertyName)
        {
            var errors = obj.ValidateProperty(propertyName);
            return string.Join(Environment.NewLine, errors);
        }
    }
} 