using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Jacal.Converters
{
    public class ImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string name = value.ToString();
            using (var imageStream = File.OpenRead("..\\..\\..\\Assets\\Images\\"+name+".png"))
            {
                return new Bitmap(imageStream);
            }

            //string image = value.ToString();
            //string[] words = image.Split('_');
            //if (words.Length == 1)
            //    return "Images\\" + image + ".png";

                //if (words[1] == "gray")
                //{
                //    FormatConvertedBitmap btm = new FormatConvertedBitmap();
                //    btm.BeginInit();
                //    btm.Source = new BitmapImage(new Uri("..\\..\\Images\\Cells\\" + words[0] + ".png", UriKind.Relative));
                //    btm.DestinationFormat = PixelFormats.Gray8;
                //    btm.EndInit();
                //    return btm;
                //}
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
