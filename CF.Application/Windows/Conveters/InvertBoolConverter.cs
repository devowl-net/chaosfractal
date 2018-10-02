using System;
using System.Globalization;
using System.Windows.Data;

namespace CF.Application.Windows.Conveters
{
    /// <summary>
    /// Boolean value converter.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertBoolConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is bool)
            {
                return !((bool)value);
            }

            return true;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}