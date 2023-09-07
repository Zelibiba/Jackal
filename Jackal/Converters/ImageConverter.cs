using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using Avalonia;
using Avalonia.Platform;
using Jackal.Models;

namespace Jackal.Converters
{
    public class ImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string[] words = value.ToString().Split('_');

            Bitmap bitmap;
            string path;
#if DEBUG
            path = Path.Combine("..", "..", "..", "Assets", "Images");
#else
            path = Path.Combine("Assets", "Images");
#endif
            path = Path.Combine(path, Map.Type.ToString(), words[0] + ".png");
            bitmap = new(path);

            if (words.Length == 1)
                return bitmap;
            if (words[1] == "gray")
            {
                SKBitmap SkBitmap;
                using(MemoryStream stream = new())
                {
                    bitmap.Save(stream);
                    stream.Position = 0;
                    SkBitmap = SKBitmap.Decode(stream);
                }
                using (SKCanvas canvas=new(SkBitmap))
                {
                    using (SKPaint paint = new())
                    {
                        paint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                                            {
                                                0.21f, 0.72f, 0.07f, 0, 0,
                                                0.21f, 0.72f, 0.07f, 0, 0,
                                                0.21f, 0.72f, 0.07f, 0, 0,
                                                0,     0,     0,     1, 0
                                            });
                        SKImageInfo info = new(SkBitmap.Width, SkBitmap.Height);
                        canvas.DrawBitmap(SkBitmap, info.Rect, paint: paint);
                    }
                }
                SKImage image = SKImage.FromBitmap(SkBitmap);
                using (Stream stream = image.Encode().AsStream())
                {
                    stream.Position = 0;
                    bitmap = new(stream);
                }
                return bitmap;
            }
            throw new NotImplementedException();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
