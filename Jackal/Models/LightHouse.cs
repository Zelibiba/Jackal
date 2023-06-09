using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jackal.Models.Cells;

namespace Jackal.Models
{
    public class LightHouse
    {
        /// <summary>
        /// Клетка маяка.
        /// </summary>
        public LightHouseCell Lighthouse { get; set; }
        /// <summary>
        /// Флаг того, что происходит ход маяка.
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Список клеток, открытых маяком.
        /// </summary>
        public List<Cell> SelectedCells { get; set; }
        /// <summary>
        /// Клетка, выбранная в ходе маяка для перемещения.
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
