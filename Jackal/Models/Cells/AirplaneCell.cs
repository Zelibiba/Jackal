using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class AirplaneCell : Cell
    {
        public AirplaneCell(int row, int column) : base(row, column, "Airplane")
        {
            IsActive = true;
        }

        /// <summary>
        /// Флаг того, что самолёт может быть использован.
        /// </summary>
        public bool IsActive { get; private set; }

        public override void RemovePirate(Pirate pirate, bool withGold = true)
        {
            base.RemovePirate(pirate, withGold);

            if (pirate.TargetCell == this)
                SelectableCoords.RemoveAll(coords => coords == Coords);
            else if (IsActive && (Pirates.Count == 0 || (pirate.TargetCell.Coords - Coords).Abs() > 1))
            {
                IsActive = false;
                SelectableCoords.RemoveAll(coords => (coords - Coords).Abs() > 1 || coords == Coords);
            }
        }
        public override MovementResult AddPirate(Pirate pirate, int delay =0)
        {
            bool isOpened = IsOpened;
            base.AddPirate(pirate, delay);
            return isOpened ? MovementResult.End : MovementResult.Continue;
        }
        public override void SetSelectableCoords(Map map)
        {
            if (IsActive)
            {
                SelectableCoords.Clear();
                SelectableCoords.AddRange(map.GroundCoordinates());
            }
            else
                base.SetSelectableCoords(map);
        }
    }
}
