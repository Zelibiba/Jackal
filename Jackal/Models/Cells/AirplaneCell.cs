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
                SelectableCoords.RemoveAll(coords => HasSameCoords(coords));
            else if (IsActive && (Pirates.Count == 0 || Math.Abs(Row - pirate.TargetCell.Row) > 1 || Math.Abs(Column - pirate.TargetCell.Column) > 1))
            {
                IsActive = false;
                SelectableCoords.RemoveAll(coords => Math.Abs(Row - coords[0]) > 1 || Math.Abs(Column - coords[1]) > 1 ||
                                                     HasSameCoords(coords));
            }
        }
        public override MovementResult AddPirate(Pirate pirate)
        {
            bool isOpened = IsOpened;
            base.AddPirate(pirate);
            return isOpened ? MovementResult.End : MovementResult.Continue;
        }
        public override void SetSelectableCoords(ObservableMap map)
        {
            SelectableCoords.Clear();
            for (int i = 0; i < map.MapSize; i++)
            {
                for (int j = 0; j < map.MapSize; j++)
                {
                    if (map[i, j] is not SeaCell && map[i, j] is not ShipCell)
                        SelectableCoords.Add(new int[2] { i, j });
                }
            }
        }
    }
}
