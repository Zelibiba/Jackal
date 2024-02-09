using Avalonia.Threading;
using Jackal.Models.Cells;
using Jackal.Models.Cells.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Pirates
{
    public class Friday : Pirate
    {
        public Friday(Cell cell, Player owner, Player manager) : base(cell, owner, manager, image: "Friday", isFighter: false) { }

        public override bool CanDrinkRum => false;
        public override bool CanHaveSex => false;

        public override List<Coordinates> SelectableCoords
        {
            get
            {
                if (Cell is ITrapCell trapCell)
                    return trapCell.AltSelectableCoords;
                return Cell.SelectableCoords;
            }
        }

        public void SetNewOwner(Player owner, Player manager)
        {
            Manager.Pirates.Remove(this);
            Dispatcher.UIThread.Post(() => Owner = owner);
            Manager = manager;
            Manager.Pirates.Add(this);
        }
    }
}
