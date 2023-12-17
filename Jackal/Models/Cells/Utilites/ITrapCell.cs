using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells.Utilites
{
    public interface ITrapCell
    {
        /// <summary>
        /// Альтернативные координаты, куда можно перейти с этой клетки.
        /// </summary>
        /// <remarks>
        /// Необходимы для механики выпивания рома.
        /// </remarks>
        List<Coordinates> AltSelectableCoords { get; }
    }
}
