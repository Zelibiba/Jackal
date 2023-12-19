using Jackal.Models.Cells.Utilites;
using Jackal.Models.Pirates;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class CaveCell : NodeOwnerCell
    {
        public CaveCell(int row, int column, Func<Coordinates, MovementResult> continueMove) : base(row, column, "Cave")
        {
            Enter = new CaveEnterCell(this, continueMove);
            Exit = new CaveExitCell(this);
            TreasureCell = new CaveTreasureCell(this);

            Nodes.Clear();
            Nodes.Add(Exit);
            Nodes.Add(Enter);
            Nodes.Add(TreasureCell);
        }

        public bool IsClosed { get; set; }
        public CaveEnterCell Enter { get; }
        public CaveExitCell Exit { get; }
        public CaveTreasureCell TreasureCell { get; }
        public CaveCell[] Caves { get; private set; }
        public void LinkCaves(IEnumerable<CaveCell> caves) => Caves = caves.Where(cave => cave != this).ToArray();

        public override Cell GetSelectedCell(Pirate pirate) => pirate.Cell == TreasureCell ? Enter : Exit;
    }

    public class CaveExitCell : NodeCell
    {
        public CaveExitCell(CaveCell cave) : base(cave) { }

        CaveCell Cave => _owner as CaveCell;
        CaveEnterCell Enter => Cave.Enter;
        CaveTreasureCell TreasureCell => Cave.TreasureCell;
        CaveCell[] Caves => Cave.Caves;

        public override bool CanBeSelectedBy(Pirate pirate)
        {
            if (pirate.Cell is CaveEnterCell)
                return IsOpened && IsFriendlyTo(pirate);
            return base.CanBeSelectedBy(pirate) && Enter.IsFriendlyTo(pirate);
        }
        public override bool IsGoldFriendly(Pirate pirate)
        {
            if (pirate.Cell is CaveEnterCell)
                return base.IsGoldFriendly(pirate);
            return base.IsGoldFriendly(pirate) && Enter.IsFriendlyTo(pirate);
        }

        public override void RemovePirate(Pirate pirate, bool withGold = true)
        {
            base.RemovePirate(pirate, withGold);

            // если выход теперь открыт, проверить остальные пещеры на застрявших пиратов
            if (Pirates.Count == 0)
            {
                CaveCell? closedCave = Caves.FirstOrDefault(cave => cave.IsClosed, null);
                closedCave?.Enter.SendPiratesTo(this);
            }
        }
        public override MovementResult AddPirate(Pirate pirate, int delay = 0)
        {
            // если пират пришёл из другой пещеры
            if (pirate.Cell is CaveEnterCell enter)
            {
                // если есть застрявшие пираты
                Enter.SendPiratesTo(enter.Exit);
                return base.AddPirate(pirate, delay);
            }
            // если пират пришёл со стороны
            else
            {
                HitPirates(pirate, allPirates: true);

                // если есть возможность взять монетку
                if (!pirate.Treasure && (Gold > 0 || Galeon))
                    return TreasureCell.AddPirate(pirate);

                return Enter.AddPirate(pirate, delay);
            }
        }
        public void AddDrunkMissioner(Pirate pirate)
        {
            pirate.Cell = this;
            Pirates.Add(pirate);
        }
    }

    public class CaveTreasureCell : NodeCell
    {
        public CaveTreasureCell(CaveCell cave) : base(cave, isStandable: false) { }

        CaveExitCell Exit => (_owner as CaveCell).Exit;

        public override int Gold
        {
            get => Pirates.Count > 0 ? Exit.Gold : 0;
            set => Exit.Gold = value;
        }
        public override bool Galeon
        {
            get => Pirates.Count > 0 ? Exit.Galeon : false;
            set => Exit.Galeon = value;
        }

        public override void SetSelectableCoords(Map map)
        {
            SelectableCoords.Clear();
            SelectableCoords.Add(Coords);
        }

        public override MovementResult AddPirate(Pirate pirate, int delay = 0)
        {
            base.AddPirate(pirate, delay);
            pirate.PrepareToMove();
            return MovementResult.Continue;
        }
    }

    public class CaveEnterCell : NodeCell
    {
        public CaveEnterCell(CaveCell cave, Func<Coordinates, MovementResult> continueMove) : base(cave)
        {
            _continueMove = continueMove;
        }

        CaveCell Cave => _owner as CaveCell;
        public CaveExitCell Exit => Cave.Exit;
        CaveCell[] Caves => Cave.Caves;

        readonly Func<Coordinates, MovementResult> _continueMove;

        public override void SetSelectableCoords(Map map)
        {
            UpdateSelectableCoord();
            foreach (CaveCell cave in Caves)
                cave.Enter.UpdateSelectableCoord();
        }
        void UpdateSelectableCoord()
        {
            SelectableCoords.Clear();
            foreach (CaveCell cave in Caves)
                SelectableCoords.Add(cave.Coords);
        }

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
            Cave.IsClosed = false;
        }
        public override void RemovePirate(Pirate pirate, bool withGold = true)
        {
            base.RemovePirate(pirate, withGold);

            // если выход открыт, проверить остальные пещеры на застрявших пиратов
            //if (Pirates.Count == 0 && Exit.Pirates.Count == 0)
            //{
            //    CaveCell? closedCave = Caves.FirstOrDefault(cave => cave.IsClosed, null);
            //    closedCave?.Enter.SendPiratesTo(Exit, 200);
            //}
        }
        public override MovementResult AddPirate(Pirate pirate, int delay = 0)
        {
            bool wasOpened = IsOpened;
            base.AddPirate(pirate, delay);

            // если пещера только октрыта, проверить другие пещеры на наличие застрявших пиратов
            if (!wasOpened)
            {
                CaveCell? closedCave = Caves.FirstOrDefault(cave => cave.IsClosed 
                                                                    && cave.Exit.IsFriendlyTo(pirate)
                                                                    && Exit.CanBeSelectedBy(cave.Enter.Pirates[0]), null);
                if (closedCave != null)
                    return _continueMove(closedCave.Coords);
            }

            CaveCell[] availableCaves = Caves.Where(cave => cave.Exit.CanBeSelectedBy(pirate) && !cave.IsClosed).ToArray();
            if (availableCaves.Length == 1)
                return _continueMove(availableCaves[0].Coords);
            if (availableCaves.Length > 1)
                return MovementResult.Continue;

            // если не нашлось подходящих пещер для перемещения
            Cave.IsClosed = true;
            pirate.IsBlocked = true;
            pirate.IsEnabled = false;
            return MovementResult.End;
        }
    }
}
