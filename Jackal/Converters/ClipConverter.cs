using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Jackal.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Converters
{
    public class ClipConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            double w = (double)values[0];
            double h = (double)values[1];
            double x = ((double?)values.ElementAtOrDefault(2) ?? 0) / 2;

            if (w <= 0 || h <= 0)
                return null;

            if (Map.Type == MapType.Quadratic)
                return new PathGeometry
                {
                    Figures =
                    {
                        new PathFigure
                        {
                            StartPoint = new Point(x, x),
                            IsClosed = true,
                            Segments =
                            {
                                new LineSegment { Point = new Point(w - x, x) },
                                new LineSegment { Point = new Point(w - x, h - x) },
                                new LineSegment { Point = new Point(x, h - x) }
                            }
                        }
                    }
                };

            return new PathGeometry
            {
                Figures =
                {
                    new PathFigure
                    {
                        StartPoint = new Point(w/2, x),
                        IsClosed = true,
                        Segments =
                        {
                            new LineSegment { Point = new Point(w - x, (h + x) * 0.25) },
                            new LineSegment { Point = new Point(w - x, (h - x) * 0.75) },
                            new LineSegment { Point = new Point(w * 0.5, h - x) },
                            new LineSegment { Point = new Point(x, (h - x) * 0.75) },
                            new LineSegment { Point = new Point(x, (h + x) * 0.25) },
                        }
                    }
                }
            };
        }

        public object[] ConvertBack(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
