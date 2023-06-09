using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jackal.Models.Cells;

namespace Jackal.Models
{
    public class EarthQuake
    {
        /// <summary>
        /// Флаг того, что происходит землетрясение.
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Клетка, выбранная в течение землетрясения.
        /// </summary>
        public Cell? SelectedCell { get; private set; }

        public void SelectCell(Cell cell)
        {
            cell.IsSelected = true;
            SelectedCell = cell;
        }
        public void DeselectCell()
        {
            SelectedCell.IsSelected = false;
            SelectedCell = null;
        }
    }
}
