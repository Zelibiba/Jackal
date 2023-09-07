using Jackal.Models.Cells.Cave;
using Jackal.Models.Pirates;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class CaveCell : Cell
    {
        public CaveCell(int row, int column, Func<Coordinates, MovementResult> continueMove) : base(row, column, "Cave")
        {
            Enter = new CaveEnterCell(this, continueMove);
            Exit = new CaveExitCell(this);
            TreasureCell = new CaveTreasureCell(this);

            Nodes.Clear();
            Nodes.Add(Exit);
            Nodes.Add(Enter);

            LinkCellWithNodes();
        }

        public bool IsClosed { get; set; }
        public CaveEnterCell Enter { get; }
        public CaveExitCell Exit { get; }
        public CaveTreasureCell TreasureCell { get; }
        public CaveCell[] Caves { get; private set; }
        public void LinkCaves(IEnumerable<CaveCell> caves) => Caves = caves.Where(cave => cave != this).ToArray();

        public override void SetCoordinates(int row, int column)
        {
            base.SetCoordinates(row, column);
            Exit.SetCoordinates(row, column);
            Enter.SetCoordinates(row, column);
            TreasureCell.SetCoordinates(row, column);
        }
        public override void SetSelectableCoords(Map map)
        {
            base.SetSelectableCoords(map);
            Exit.SetSelectableCoords(map);
            Enter.SetSelectableCoords(map);
            TreasureCell.SetSelectableCoords(map);
        }
        public override Cell GetSelectedCell(Pirate pirate) => pirate.Cell == TreasureCell ? Enter : Exit;
    }
}
