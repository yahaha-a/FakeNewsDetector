using Avalonia.Data;
using Avalonia.Data.Converters;
using Client.Constants;
using System;
using System.Globalization;

namespace Client.Helpers.Converters
{
    public class SeverityToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is NotificationSeverity severity)
            {
                return severity switch
                {
                    NotificationSeverity.Success => "✓",
                    NotificationSeverity.Info => "ℹ",
                    NotificationSeverity.Warning => "⚠",
                    NotificationSeverity.Error => "✗",
                    _ => "ℹ"
                };
            }
            
            return "ℹ";
        }

        /// <summary>
        /// 从图标转换回严重性级别，这是一个单向转换，不支持反向转换
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 这是一个单向转换器，不支持从图标转换回严重性级别
            return BindingOperations.DoNothing;
        }
    }
} 