using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    /// <summary>
    /// Класс параметров карты для запуска игры.
    /// </summary>
    public class GameProperties : ReactiveObject
    {
        /// <summary>
        /// Сид карты.
        /// </summary>
        public int Seed { get; set; } = 0;

        /// <summary>
        /// Тип карты.
        /// </summary>
        [Reactive] public MapType MapType { get; set; } = MapType.Quadratic;

        /// <summary>
        /// Словарь соответсвия названий паттерна с файлом этого паттерна.
        /// </summary>
        public static Dictionary<string, string> PatternNames { get; } = new()
        {
            ["Фиксированный"] = "fix",
            ["Вероятностный"] = "var",
        };
        /// <summary>
        /// Выбранный паттерн.
        /// </summary>
        [Reactive] public string PatternName { get; set; } = "Фиксированный";
        /// <summary>
        /// Словарь паттерна ячеек.
        /// </summary>
        public Dictionary<string, (int count, char param)> MapPattern { get; set; } = new();
        /// <summary>
        /// Функция зачитывает паттерн распределения клеток.
        /// </summary>
        /// <param name="reader">Поток записи.</param>
        /// <param name="mainFix">Флаг того, что количество всех клетки фиксированны.</param>
        /// <returns>True, если чтение завершилось успешно.</returns>
        public bool ReadMapPattern(StreamReader reader, bool mainFix)
        {
            try
            {
                Dictionary<string, (int, char)> mapPattern = new();
                while (true)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        break;
                    string[] words = line.Split(':');
                    words[1] = words[1].Trim();
                    char param = mainFix ? '=' : ' ';
                    if (words[1][0] < '0' || words[1][0] > '9')
                    {
                        param = mainFix ? '=' : words[1][0];
                        words[1] = words[1].Remove(0, 1);
                    }
                    int count = int.Parse(words[1]);
                    mapPattern[words[0]] = (count, param);
                }
                MapPattern = mapPattern;
            }
            catch { return false; }
            return true;
        }
        /// <summary>
        /// Функция зачитывает паттерн распределения клеток из файла, определяемого выбранным паттерном (<see cref="PatternName"/>).
        /// </summary>
        /// <returns>True, если чтение завершилось успешно.</returns>
        public bool ReadMapPattern()
        {
            string filename = PatternNames[PatternName];
            bool mainFix = filename == "fix";
            if (filename == "fix" || filename == "var")
                filename = MapType.ToString();

            filename = Path.Combine(Properties.MapPatternsFolder, filename + ".txt");
            FileStream? file;
            try { file = new(filename, FileMode.Open, FileAccess.Read); }
            catch { return false; };
            StreamReader reader = new(file);
            return ReadMapPattern(reader, mainFix);
        }
        /// <summary>
        /// Функция записывает паттерн распределения клеток.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteMapPattern(StreamWriter? writer)
        {
            if (writer == null) return;
            foreach ((string name, (int count, char param)) in MapPattern)
                writer.WriteLine((name + ':').PadRight(12) + param + count.ToString());
        }

        [Reactive] public int Size { get; set; } = 11;
        public void NormaliseSize()
        {
            Size = MapType == MapType.Hexagonal ? 7 : 11;
        }
    }
}
