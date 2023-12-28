using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Layout;
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
            MapType mapType = values[0] is MapType type ? type : MapType.Quadratic;
            Orientation orientation = values[1] is Orientation o ? o : Orientation.Vertical;
            double w = (double)values[2];
            double h = (double)values[3];
            double x = ((double?)values.ElementAtOrDefault(4) ?? 0) / 2;

            if (w <= 0 || h <= 0)
                return null;

            if (mapType == MapType.Quadratic)
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

            if (orientation == Orientation.Vertical)
                return new PathGeometry
                {
                    Figures =
                    {
                        new PathFigure
                        {
                            StartPoint = new Point(w * 0.5, x),
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

            return new PathGeometry
            {
                Figures =
                {
                    new PathFigure
                    {
                        StartPoint = new Point(x, h/2),
                        IsClosed = true,
                        Segments =
                        {
                            new LineSegment { Point = new Point((w + x) * 0.25, x) },
                            new LineSegment { Point = new Point((w - x) * 0.75, x) },
                            new LineSegment { Point = new Point(w - x, h * 0.5) },
                            new LineSegment { Point = new Point((w - x) * 0.75, h - x) },
                            new LineSegment { Point = new Point((w + x) * 0.25, h - x) },
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
