using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using Jackal.Models;

namespace Jackal.Converters
{
    public class AllianceIdentifierConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            AllianceIdentifier x = (AllianceIdentifier)value;
            if (x == AllianceIdentifier.None) return Brush.Parse("Lavender");
            if (x == AllianceIdentifier.Red) return Brush.Parse("#ff7d7d");
            if (x == AllianceIdentifier.Blue) return Brush.Parse("#55b6ff");
            if (x == AllianceIdentifier.Green) return Brush.Parse("#6BFF92");
            throw new NotImplementedException();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
