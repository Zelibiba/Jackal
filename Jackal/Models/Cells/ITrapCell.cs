using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public interface ITrapCell
    {
        /// <summary>
        /// Альтернативные координаты, куда можно перейти с этой клетки.
        /// </summary>
        /// <remarks>
        /// Необходимы для механики выпивания рома.
        /// </remarks>
        List<int[]> AltSelectableCoords { get; }
    }
}
