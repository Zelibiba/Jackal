using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Jackal.Models.Cells.Utilites;
using Jackal.Models.Pirates;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class MazeCell : NodeOwnerCell
    {
        public MazeCell(int row, int column, int mazeLevel) : base(row, column, "Maze" + mazeLevel)
        {
            if (mazeLevel < 2 || mazeLevel > 5)
                throw new ArgumentException("Wrong mazeLevel!");

            Nodes.Clear();
            for (int i = 1; i <= mazeLevel; i++)
                Nodes.Add(new MazeNodeCell(this, i));
        }

        public override Cell GetSelectedCell(Pirate pirate)
        {
            int nodeNumber = pirate.Cell.Coords == Coords ? pirate.MazeNodeNumber : 0;
            return Nodes[nodeNumber];
        }
    }

    public class MazeNodeCell : NodeCell, ITrapCell
    {
        public MazeNodeCell(MazeCell owner, int step) : base(owner, number: step)
        {
            AltSelectableCoords = new List<Coordinates>();
        }

        public List<Coordinates> AltSelectableCoords { get; }

        public override void SetSelectableCoords(Map map)
        {
            if (_owner.Nodes.Count == Number)
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
