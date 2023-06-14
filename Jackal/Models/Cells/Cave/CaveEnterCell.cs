using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells.Cave
{
    public class CaveEnterCell : Cell
    {
        public CaveEnterCell(CaveCell cave, Func<int[], MovementResult> continueMove) : base(cave.Row, cave.Column, string.Empty, number: 0)
        {
            _cave = cave;
            _continueMove = continueMove;
        }

        readonly CaveCell _cave;
        public CaveExitCell Exit => _cave.Exit;
        CaveCell[] Caves => _cave.Caves;

        Func<int[], MovementResult> _continueMove;

        public void SendPiratesTo(CaveExitCell exit)
        {
            while (Pirates.Count > 0)
            {
                Pirate pir = Pirates[0];
                pir.IsBlocked = false;
                pir.IsEnabled = true;
                pir.RemoveFromCell();
                exit.AddPirate(pir);
            }
            _cave.IsClosed = false;
        }
        public override void RemovePirate(Pirate pirate, bool withGold = true)
        {
            _cave.Pirates.Remove(pirate);
            base.RemovePirate(pirate, withGold);

            // если выход открыт, проверить остальные пещеры на застрявших пиратов
            if (Pirates.Count == 0 && Exit.Pirates.Count == 0)
            {
                CaveCell? closedCave = Caves.FirstOrDefault(cave => cave.IsClosed, null);
                closedCave?.Enter.SendPiratesTo(Exit);
            }
        }
        public override MovementResult AddPirate(Pirate pirate)
        {
            bool wasOpened = IsOpened;
            base.AddPirate(pirate);
            SelectableCoords.Clear();

            // если пещера только октрыта, проверить другие пещеры на наличие застрявших пиратов
            if (!wasOpened)
            {
                CaveCell? closedCave = Caves.FirstOrDefault(cave => cave.IsClosed && cave.Exit.IsFriendlyTo(pirate), null);
                if (closedCave != null)
                    return _continueMove(closedCave.Coords);
            }

            foreach (CaveCell cave in Caves)
            {
                if (cave.IsOpened && !cave.IsClosed && cave.Exit.IsFriendlyTo(pirate))
                    SelectableCoords.Add(cave.Coords);
            }
            if (SelectableCoords.Count == 1)
                return _continueMove(SelectableCoords[0]);
            if (SelectableCoords.Count > 1)
                return MovementResult.Continue;

            // если не нашлось подходящих пещер для перемещения
            _cave.IsClosed = true;
            pirate.IsBlocked = true;
            pirate.IsEnabled = false;
            return MovementResult.End;
        }
    }
}
