using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal
{
    /// <summary>
    /// Класс с указание путей для различных файлов.
    /// </summary>
    public static class Properties
    {
        static Properties()
        {
            string mainFolder = Environment.CurrentDirectory;
#if DEBUG
            mainFolder = Path.Combine("..", "..", "..");
#endif
            ImageFolder = Path.Combine(mainFolder, "Assets", "Images");
            MapPatternsFolder = Path.Combine(mainFolder, "Assets", "Map Patterns");
            SavesFolder = Path.Combine(mainFolder, "saves");
        }
        /// <summary>
        /// Путь до папки с изображениями клеток.
        /// </summary>
        public static string ImageFolder { get; }
        /// <summary>
        /// Путь до папки с файлами, задающими количество ячеек.
        /// </summary>
        public static string MapPatternsFolder { get; }
        /// <summary>
        /// Путь до папки с сохранениями.
        /// </summary>
        public static string SavesFolder { get; }
    }
}
