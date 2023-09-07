using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class MazeNodeCell : Cell, ITrapCell
    {
        public MazeNodeCell(int row, int column, MazeCell owner, int step) : base(row, column, owner.Image[..5], number: step)
        {
            _owner = owner;
            AltSelectableCoords = new List<Coordinates>();
        }

        readonly MazeCell _owner;
        public List<Coordinates> AltSelectableCoords { get; }

        public override void Open()
        {
            base.Open();
            if (!_owner.IsOpened)
                _owner.Open();
        }

        public override void RemovePirate(Pirate pirate, bool withGold = true)
        {
            if(!_owner.Nodes.Contains(pirate.TargetCell))
                _owner.Pirates.Remove(pirate);
            base.RemovePirate(pirate, withGold);
        }
        public override MovementResult AddPirate(Pirate pirate)
        {
            if (!_owner.Pirates.Contains(pirate))
                _owner.Pirates.Add(pirate);
            return base.AddPirate(pirate);
        }

        public override void SetSelectableCoords(Map map)
        {
            if(_owner.Nodes.Count == Number)
                base.SetSelectableCoords(map);
            else
            {
                SelectableCoords.Clear();
                SelectableCoords.Add(Coords);
            }
            AltSelectableCoords.Clear();
            AltSelectableCoords.AddRange(_owner.SelectableCoords);
        }
    }
}
