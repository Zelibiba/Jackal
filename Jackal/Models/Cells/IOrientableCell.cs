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
        /// <summary>
        /// Метод поворачивает клетку и изменяет её ориентацию.
        /// </summary>
        /// <param name="rotation">Поворот клетки по часовой стрелки от 12. Принимает значения 0, 1, 2, 3.</param>
        /// <param name="orientation">Ориентация клетки.</param>
        /// <returns>Угол, на который повернулась клетка.</returns>
        int Rotate(int rotation, ref Orientation orientation)
        {
            if (rotation == 0)
                return 0;

            if (rotation < 0 || rotation > 3)
                throw new ArgumentException("Wrong cell rotation!");

            int orient = orientation == Orientation.LeftUp ? 24 : (int)orientation;
            orient *= (int)Math.Pow(2, rotation);
            if (orient / 16 != 0 && orient != 24)
                orient /= 16;
            orientation = orient == 24 ? Orientation.LeftUp : (Orientation)orient;

            return rotation * 90;
        }
        /// <summary>
        /// <inheritdoc cref="Rotate(int, ref Orientation)" path="/summary"/>
        /// </summary>
        /// <param name="rotation"><inheritdoc cref="Rotate(int, ref Orientation)" path="/param"/></param>
        /// <param name="orientations"></param>
        /// <returns><inheritdoc cref="Rotate(int, ref Orientation)" path="/returns"/></returns>
        int Rotate(int rotation, ref List<Orientation> orientations)
        {
            for (int i = 0; i < orientations.Count; i++)
            {
                Orientation orientation = orientations[i];
                Rotate(rotation, ref orientation);
                orientations[i] = orientation;
            }

            return rotation * 90;
        }
    }
}
