using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace Jackal.Converters
{
    public class TeamConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "White" => Brush.Parse("AntiqueWhite"),
                "Yellow" => Brush.Parse("#FFECEC2C"),
                "Red" => Brush.Parse("Red"),
                "Black" => Brush.Parse("#FF3A3A3A"),
                "Green" => Brush.Parse("#00FF1D"),
                "Purple" => Brush.Parse("#7700FF"),
                "Ben" => Brush.Parse("Green"),
                "Friday" => Brush.Parse("Brown"),
                "Missioner" => Brush.Parse("Blue"),
                "DrunkMissioner" => Brush.Parse("LightBlue"),
                _ => Brush.Parse("Transparent")
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
