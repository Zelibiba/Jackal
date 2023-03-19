using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace Jacal.Converters
{
    public class TeamConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return 0;
            switch (value.ToString())
            {
                case "White":
                    return Brush.Parse("AntiqueWhite");
                case "Yellow":
                    return Brush.Parse("#FFECEC2C");
                case "Red":
                    return Brush.Parse("Red");
                case "Black":
                    return Brush.Parse("#FF3A3A3A");
                case "Green":
                    return Brush.Parse("Green");
                case "Brown":
                    return Brush.Parse("Brown");
                case "Blue":
                    return Brush.Parse("Blue");
                case "DrunkMissioner":
                    return Brush.Parse("LightBlue");
            }
            return 0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
