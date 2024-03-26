using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal
{
    /// <summary>
    /// Класс с указанием версии и путей для различных файлов.
    /// </summary>
    public static class Properties
    {
        /// <summary>
        /// Версия программы.
        /// </summary>
        public static string Version => "1.1.1";

        static Properties()
        {
            string mainFolder = AppContext.BaseDirectory;
#if DEBUG
            mainFolder = Path.Combine(mainFolder, "..", "..", "..");
#endif
            ImageFolder = Path.Combine(mainFolder, "Assets", "Images");
            MapPatternsFolder = Path.Combine(mainFolder, "Assets", "Map Patterns");
            SoundsFolder = Path.Combine(mainFolder, "Assets", "Sounds");
            SavesFolder = Path.Combine(mainFolder, "Saves");
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
        /// <summary>
        /// Путь до папки с аудиофайлами.
        /// </summary>
        public static string SoundsFolder { get; }
    }
}
