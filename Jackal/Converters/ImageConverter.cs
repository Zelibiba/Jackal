﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Jacal.Converters
{
    public class ImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string name = value.ToString();
            string[] words = name.Split('_');

            if (words.Length == 1)
                return new Avalonia.Media.Imaging.Bitmap(Path.Combine(new string[]
                {
                    "..", "..", "..", "Assets", "Images", words[0] + ".png"
                }));
            if (words[1] == "gray")
            {
                Avalonia.Media.Imaging.Bitmap bitmap=new(Path.Combine(new string[]
                {
                    "..", "..", "..", "Assets", "Images", words[0] + ".png"
                }));
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



                //Avalonia.Media.Imaging.Bitmap bitmap;
                //Bitmap origBitmap = new("..\\..\\..\\Assets\\Images\\" + words[0] + ".png");
                //Bitmap newBitmap = new(origBitmap.Width, origBitmap.Height);
                //using (Graphics graphics = Graphics.FromImage(newBitmap))
                //{
                //    ColorMatrix matrix = new ColorMatrix(new float[][]
                //    {
                //        new float[] {.3f, .3f, .3f, 0, 0},
                //        new float[] {.59f, .59f, .59f, 0, 0},
                //        new float[] {.11f, .11f, .11f, 0, 0},
                //        new float[] {0, 0, 0, 1, 0},
                //        new float[] {0, 0, 0, 0, 1}
                //    });
                //    ImageAttributes attributes = new ImageAttributes();
                //    attributes.SetColorMatrix(matrix);
                //    graphics.DrawImage(origBitmap, new Rectangle(0, 0, origBitmap.Width, origBitmap.Height),
                //                       0, 0, origBitmap.Width, origBitmap.Height, GraphicsUnit.Pixel, attributes);
                //}
                //using (MemoryStream stream = new())
                //{
                //    newBitmap.Save(stream, ImageFormat.Png);
                //    stream.Position = 0;
                //    bitmap = new(stream);
                //}
                //return bitmap;
            }
            throw new NotImplementedException();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
