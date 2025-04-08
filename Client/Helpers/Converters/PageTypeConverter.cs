using Avalonia.Data;
using Avalonia.Data.Converters;
using Client.Services.Interfaces;
using System;
using System.Globalization;

namespace Client.Helpers.Converters
{
    /// <summary>
    /// 将PageType枚举转换为字符串的转换器
    /// </summary>
    public class PageTypeConverter : IValueConverter
    {
        /// <summary>
        /// 将PageType枚举转换为字符串
        /// </summary>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PageType pageType)
            {
                return pageType.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// 将字符串转换为PageType枚举
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string pageTypeString && Enum.TryParse<PageType>(pageTypeString, true, out var pageType))
            {
                return pageType;
            }

            return PageType.Home;
        }
    }
    
    /// <summary>
    /// 页面类型比较转换器，用于检查当前页面是否与指定的页面类型匹配
    /// </summary>
    public class PageTypeEqualityConverter : IValueConverter
    {
        /// <summary>
        /// 将PageType与参数中的页面类型字符串比较
        /// </summary>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PageType currentPage && parameter is string pageToCompare)
            {
                return currentPage.ToString() == pageToCompare;
            }

            return false;
        }

        /// <summary>
        /// 此方法不实现
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 页面转导航按钮边框颜色转换器
    /// </summary>
    public class PageToNavButtonBorderConverter : IValueConverter
    {
        /// <summary>
        /// 根据当前选中的页面设置对应按钮的边框颜色
        /// </summary>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PageType currentPage && parameter is string pageToCompare)
            {
                return currentPage.ToString() == pageToCompare ? "#1861DE" : "Transparent";
            }

            return "Transparent";
        }

        /// <summary>
        /// 此方法不实现
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// 相等条件颜色转换器
    /// </summary>
    public class EqualityToColorConverter : IValueConverter
    {
        /// <summary>
        /// 比较当前值与参数值是否相等，返回对应的颜色
        /// 参数格式："比较值,匹配时的颜色,不匹配时的颜色"
        /// </summary>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is string paramStr)
            {
                string[] parts = paramStr.Split(',');
                if (parts.Length >= 3)
                {
                    bool isEqual = value?.ToString() == parts[0];
                    return isEqual ? parts[1] : parts[2];
                }
            }
            
            return "Transparent";
        }

        /// <summary>
        /// 此方法不实现
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// 相等条件字体粗细转换器
    /// </summary>
    public class EqualityToWeightConverter : IValueConverter
    {
        /// <summary>
        /// 比较当前值与参数值是否相等，返回对应的字体粗细
        /// 参数格式："比较值,匹配时的粗细,不匹配时的粗细"
        /// </summary>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is string paramStr)
            {
                string[] parts = paramStr.Split(',');
                if (parts.Length >= 3)
                {
                    bool isEqual = value?.ToString() == parts[0];
                    return isEqual ? parts[1] : parts[2];
                }
            }
            
            return "Normal";
        }

        /// <summary>
        /// 此方法不实现
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 