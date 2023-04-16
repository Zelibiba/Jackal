using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class MazeNodeCell : Cell
    {
        public MazeNodeCell(int row, int column, MazeCell owner, int step) : base(row, column, string.Empty)
        {
            _owner = owner;
            Number = step;
        }

        MazeCell _owner;

        public override void Open()
        {
            base.Open();
            if (!_owner.IsOpened)
                _owner.Open();
        }

        public override void RemovePirate(Pirate pirate)
        {
            if (_owner.Nodes.Count == Number)
                _owner.Pirates.Remove(pirate);
            base.RemovePirate(pirate);
        }
        public override bool AddPirate(Pirate pirate)
        {
            if(Number == 1)
                _owner.Pirates.Add(pirate);
            return base.AddPirate(pirate);
        }

        public override void SetSelectableCoords(ObservableMap map)
        {
            if(_owner.Nodes.Count == Number)
                base.SetSelectableCoords(map);
            else
            {
                SelectableCoords.Clear();
                SelectableCoords.Add(Coords);
            }
        }
    }
}
