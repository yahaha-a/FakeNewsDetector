using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Client.Helpers.Converters
{
    /// <summary>
    /// 将概率值转换为颜色笔刷的转换器
    /// </summary>
    public class ProbabilityToBrushConverter : IValueConverter
    {
        /// <summary>
        /// 将概率值转换为颜色刷子
        /// - 0-0.3: 绿色（安全）
        /// - 0.3-0.7: 橙色（警告）
        /// - 0.7-1.0: 红色（危险）
        /// </summary>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double probability)
            {
                if (probability < 0.3)
                    return new SolidColorBrush(Color.Parse("#28a745")); // 绿色
                else if (probability < 0.7)
                    return new SolidColorBrush(Color.Parse("#fd7e14")); // 橙色
                else
                    return new SolidColorBrush(Color.Parse("#dc3545")); // 红色
            }

            return new SolidColorBrush(Colors.Gray); // 默认颜色
        }

        /// <summary>
        /// 从刷子转换回概率值，这是一个单向转换，不支持反向转换
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 这是一个单向转换器，不支持从Brush转换回概率值
            return BindingOperations.DoNothing;
        }
    }
} 