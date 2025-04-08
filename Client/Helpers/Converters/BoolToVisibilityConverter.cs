using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Client.Helpers.Converters
{
    /// <summary>
    /// 布尔值转可见性转换器
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 将布尔值转换为可见性
        /// </summary>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // 如果有参数并且是"Invert"，则反转结果
                bool invert = parameter is string paramStr && paramStr.Equals("Invert", StringComparison.OrdinalIgnoreCase);
                bool result = invert ? !boolValue : boolValue;
                
                return result ? Avalonia.AvaloniaProperty.UnsetValue : Avalonia.AvaloniaProperty.UnsetValue;
            }
            
            return Avalonia.AvaloniaProperty.UnsetValue;
        }

        /// <summary>
        /// 将可见性转换为布尔值
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return false;
        }
    }
} 