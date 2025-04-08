using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Client.Constants;
using Client.Models;
using System;
using System.Globalization;
using Material.Icons;

namespace Client.Helpers.Converters
{
    /// <summary>
    /// 结果类型转文本转换器
    /// </summary>
    public class ResultTypeToTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ResultType resultType)
            {
                return resultType switch
                {
                    ResultType.Real => "真实新闻",
                    ResultType.Suspicious => "可疑新闻",
                    ResultType.Fake => "虚假新闻",
                    _ => "未知"
                };
            }
            
            return "未知";
        }

        /// <summary>
        /// 不支持从文本转换回结果类型
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 这是一个单向转换器，不支持反向转换
            return BindingOperations.DoNothing;
        }
    }

    /// <summary>
    /// 结果类型转颜色转换器
    /// </summary>
    public class ResultTypeToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ResultType resultType)
            {
                string colorHex = resultType switch
                {
                    ResultType.Real => AppConstants.ResultColors.Real,
                    ResultType.Suspicious => AppConstants.ResultColors.Suspicious,
                    ResultType.Fake => AppConstants.ResultColors.Fake,
                    _ => AppConstants.ResultColors.Unknown
                };
                
                return Color.Parse(colorHex);
            }
            
            return Color.Parse(AppConstants.ResultColors.Unknown);
        }

        /// <summary>
        /// 不支持从颜色转换回结果类型
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 这是一个单向转换器，不支持反向转换
            return BindingOperations.DoNothing;
        }
    }

    /// <summary>
    /// 结果类型转图标转换器
    /// </summary>
    public class ResultTypeToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ResultType resultType)
            {
                return resultType switch
                {
                    ResultType.Real => MaterialIconKind.CheckCircle,
                    ResultType.Suspicious => MaterialIconKind.AlertCircle,
                    ResultType.Fake => MaterialIconKind.Cancel,
                    _ => MaterialIconKind.HelpCircle
                };
            }
            
            return MaterialIconKind.HelpCircle;
        }

        /// <summary>
        /// 不支持从图标转换回结果类型
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 这是一个单向转换器，不支持反向转换
            return BindingOperations.DoNothing;
        }
    }
} 