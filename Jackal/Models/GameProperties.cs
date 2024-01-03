using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
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
        public Dictionary<string, (int count, bool fix)> MapPattern { get; set; } = new();

        [Reactive] public int Size { get; set; } = 11;
        public void NormaliseSize()
        {
            Size = MapType == MapType.Hexagonal ? 7 : 11;
        }
    }
}
