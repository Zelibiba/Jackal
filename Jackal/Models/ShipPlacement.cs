using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    /// <summary>
    /// Класс описания начального положения корабля, его направления и области движения.
    /// </summary>
    public class ShipPlacement
    {
        /// <summary>
        /// Инициализирует экземпляр класса.
        /// </summary>
        /// <param name="directions"><inheritdoc cref="Directions" path="/summary"/></param>
        /// <param name="regionLength">Длина списка области движения корабля.</param>
        public ShipPlacement(Coordinates[] directions, int regionLength)
        {
            Directions = directions;
            Region = new Coordinates[regionLength];
        }

        /// <summary>
        /// Массив направления схода с корабля.
        /// </summary>
        public readonly Coordinates[] Directions;
        /// <summary>
        /// Область движения корабля.
        /// </summary>
        public readonly Coordinates[] Region;
        /// <summary>
        /// Координаты корабля на начало игры.
        /// </summary>
        public Coordinates InitialCoordinates { get; set; }
    }
}
