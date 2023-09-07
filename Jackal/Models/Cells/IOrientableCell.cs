using DynamicData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    /// <summary>
    /// Интерфейс для клеток с изменяемой ориетнацией.
    /// </summary>
    internal interface IOrientableCell
    {

        private static Coordinates[][] RotationFamilies => Map.Type == MapType.Quadratic ? _rotationFamiliesQuad : _rotationFamiliesHex;
        private readonly static Coordinates[][] _rotationFamiliesQuad = new Coordinates[2][]
        {
            new Coordinates[4]
            {
                new Coordinates(-1, 0),
                new Coordinates( 0,-1),
                new Coordinates(+1, 0),
                new Coordinates( 0,+1)
            },
            new Coordinates[4]
            {
                new Coordinates(-1,-1),
                new Coordinates(+1,-1),
                new Coordinates(+1,+1),
                new Coordinates(-1,+1)
            }
        };
        private readonly static Coordinates[][] _rotationFamiliesHex = new Coordinates[1][]
        {
            new Coordinates[6]
            {
                new Coordinates(-1,  0),
                new Coordinates( 0, -1),
                new Coordinates(+1, -1),
                new Coordinates(+1,  0),
                new Coordinates( 0, +1),
                new Coordinates(-1, +1)
            }
        };

        /// <summary>
        /// Массив направлений клетки.
        /// </summary>
        Coordinates[] Directions { get; }
        /// <summary>
        /// Метод поворачивает клетку и изменяет её ориентацию.
        /// </summary>
        /// <param name="rotation">Поворот клетки по часовой стрелки от 12.</param>
        /// <returns>Угол, на который повернулась клетка.</returns>
        int Rotate(int rotation)
        {
            if (rotation == 0)
                return 0;

            if (rotation < 0 || Map.Type == MapType.Quadratic && rotation > 3
                             || Map.Type == MapType.Hexagonal && rotation > 5)
                throw new ArgumentException("Wrong cell rotation!");

            for (int i = 0; i < Directions.Length; i++)
            {
                Coordinates[] rotationFamily = RotationFamilies.First(family => family.Any(coords => coords == Directions[i]));
                int index = Array.FindIndex(rotationFamily, coords => coords == Directions[i]);
                Directions[i] = rotationFamily[(index + rotation) % rotationFamily.Length];
            }

            int angleUnit = Map.Type == MapType.Quadratic ? 90 : 60;
            return rotation * angleUnit;
        }
    }
}
