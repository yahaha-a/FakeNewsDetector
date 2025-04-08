using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Client.Converters
{
    public class EqualityToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not string parameterString)
                return Brushes.Transparent;

            var parts = parameterString.Split(',');
            if (parts.Length < 3)
                return Brushes.Transparent;

            string compareValue = parts[0];
            string trueColor = parts[1];
            string falseColor = parts[2];

            bool isEqual = value?.ToString() == compareValue;

            if (targetType == typeof(IBrush) || targetType == typeof(ISolidColorBrush))
            {
                if (isEqual)
                    return SolidColorBrush.Parse(trueColor);
                else
                    return SolidColorBrush.Parse(falseColor);
            }
            else if (targetType == typeof(string))
            {
                return isEqual ? trueColor : falseColor;
            }

            return isEqual ? trueColor : falseColor;
        }

        /// <summary>
        /// 不支持从颜色转换回相等性比较结果
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 这是一个单向转换器，不支持反向转换
            return BindingOperations.DoNothing;
        }
    }

    public class EqualityToWeightConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not string parameterString)
                return FontWeight.Normal;

            var parts = parameterString.Split(',');
            if (parts.Length < 3)
                return FontWeight.Normal;

            string compareValue = parts[0];
            string trueWeight = parts[1];
            string falseWeight = parts[2];

            bool isEqual = value?.ToString() == compareValue;

            if (isEqual)
            {
                return trueWeight == "Bold" ? FontWeight.Bold : 
                       trueWeight == "Medium" ? FontWeight.Medium : 
                       FontWeight.Normal;
            }
            else
            {
                return falseWeight == "Bold" ? FontWeight.Bold : 
                       falseWeight == "Medium" ? FontWeight.Medium : 
                       FontWeight.Normal;
            }
        }

        /// <summary>
        /// 不支持从字体粗细转换回相等性比较结果
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 这是一个单向转换器，不支持反向转换
            return BindingOperations.DoNothing;
        }
    }
} 