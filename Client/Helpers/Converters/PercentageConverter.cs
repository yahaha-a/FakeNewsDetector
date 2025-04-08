using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Client.Helpers.Converters
{
    /// <summary>
    /// 将0-1之间的小数转换为百分比显示的转换器
    /// </summary>
    public class PercentageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                // 转换为百分比并保留指定小数位数
                int decimalPlaces = 0;
                if (parameter is int intParam)
                {
                    decimalPlaces = intParam;
                }
                else if (parameter is string strParam && int.TryParse(strParam, out int parsedValue))
                {
                    decimalPlaces = parsedValue;
                }

                string format = $"P{decimalPlaces}";
                return doubleValue.ToString(format, culture);
            }
            
            return "0%";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                // 去除百分号并转换回小数
                stringValue = stringValue.Trim().TrimEnd('%');
                
                if (double.TryParse(stringValue, NumberStyles.Any, culture, out double result))
                {
                    return result / 100.0;
                }
            }
            
            return 0.0;
        }
    }

    /// <summary>
    /// 将0-1之间的小数转换为进度条值(0-100)的转换器
    /// </summary>
    public class ProgressValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                // 将0-1的值转换为0-100
                return Math.Clamp(doubleValue * 100, 0, 100);
            }
            
            return 0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                // 将0-100的值转换为0-1
                return doubleValue / 100.0;
            }
            
            return 0.0;
        }
    }
} 