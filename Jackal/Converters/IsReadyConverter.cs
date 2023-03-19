using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace Jacal.Converters
{
    public class IsReadyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Brush.Parse("Green");
            else
                return Brush.Parse("Red");

        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
