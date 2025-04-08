using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Client.Constants;
using System;
using System.Globalization;

namespace Client.Helpers.Converters
{
    public class SeverityToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is NotificationSeverity severity)
            {
                return severity switch
                {
                    NotificationSeverity.Success => new SolidColorBrush(Color.Parse("#E6F4EA")),
                    NotificationSeverity.Info => new SolidColorBrush(Color.Parse("#E1F5FE")),
                    NotificationSeverity.Warning => new SolidColorBrush(Color.Parse("#FFF8E1")),
                    NotificationSeverity.Error => new SolidColorBrush(Color.Parse("#FFEBEE")),
                    _ => new SolidColorBrush(Colors.LightGray)
                };
            }
            
            return new SolidColorBrush(Colors.LightGray);
        }

        /// <summary>
        /// 从Brush转换回严重性级别，这是一个单向转换，不支持反向转换
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 这是一个单向转换器，不支持从Brush转换回严重性级别
            return BindingOperations.DoNothing;
        }
    }
} 