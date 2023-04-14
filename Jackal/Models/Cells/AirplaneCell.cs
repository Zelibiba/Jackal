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
        public AirplaneCell (int row,int column) : base(row, column, "Airplane")
        {
            _isActive = true;
            _opening = true;
        }

        bool _isActive;
        bool _opening;

        public override void RemovePirate(Pirate pirate)
        {
            _opening = false;
            base.RemovePirate(pirate);

            if (pirate.TargetCell == this)
                SelectableCoords.RemoveAll(coords => HasSameCoords(coords));
            else if (_isActive && (Pirates.Count == 0 || Math.Abs(Row - pirate.TargetCell.Row) > 1 || Math.Abs(Column - pirate.TargetCell.Column) > 1))
            {
                _isActive = false;
                SelectableCoords.RemoveAll(coords => Math.Abs(Row - coords[0]) > 1 || Math.Abs(Column - coords[1]) > 1 ||
                                                     HasSameCoords(coords));
            }
        }
        public override bool AddPirate(Pirate pirate)
        {
            base.AddPirate(pirate);
            return !_opening;
        }
        public override void SetSelectableCoords(ObservableMap map)
        {
            if (_isActive)
            {
                SelectableCoords.Clear();
                for (int i = 0; i < map.MapSize; i++)
                {
                    for (int j = 0; j < map.MapSize; j++)
                    {
                        if (map[i, j] is not SeaCell && map[i, j] is not ShipCell
                            && !(HasSameCoords(i, j) && !_opening))
                            SelectableCoords.Add(new int[2] { i, j });
                    }
                }
            }
            else
                base.SetSelectableCoords(map);
        }
    }
}
