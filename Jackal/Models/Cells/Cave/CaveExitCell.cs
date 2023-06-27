using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells.Cave
{
    public class CaveExitCell : Cell
    {
        public CaveExitCell(CaveCell cave) : base(cave.Row, cave.Column, "Cave", number: 0)
        {
            _cave = cave;
        }

        readonly CaveCell _cave;
        CaveEnterCell Enter => _cave.Enter;
        CaveTreasureCell TreasureCell => _cave.TreasureCell;
        CaveCell[] Caves => _cave.Caves;


        public override void Open()
        {
            base.Open();
            _cave.Open();
            //Enter.Open();
            TreasureCell.Open();
        }

        public override bool CanBeSelectedBy(Pirate pirate)
        {
            if (pirate.Cell is CaveEnterCell)
                return true;
            return base.CanBeSelectedBy(pirate) && Enter.IsFriendlyTo(pirate);
        }
        public override bool IsGoldFriendly(Pirate pirate)
        {
            if (pirate.Cell is CaveEnterCell)
                return true;
            return base.IsGoldFriendly(pirate) && Enter.IsFriendlyTo(pirate);
        }

        public override void RemovePirate(Pirate pirate, bool withGold = true)
        {
            _cave.Pirates.Remove(pirate);
            base.RemovePirate(pirate, withGold);

            // если выход теперь открыт, проверить остальные пещеры на застрявших пиратов
            if (Pirates.Count == 0)
            {
                CaveCell? closedCave = Caves.FirstOrDefault(cave => cave.IsClosed, null);
                closedCave?.Enter.SendPiratesTo(this);
            }
        }
        public override MovementResult AddPirate(Pirate pirate)
        {
            _cave.Pirates.Add(pirate);

            // если пират пришёл из другой пещеры
            if (pirate.Cell is CaveEnterCell enter)
            {
                // если есть застрявшие пираты
                Enter.SendPiratesTo(enter.Exit);
                return base.AddPirate(pirate);
            }
            // если пират пришёл со стороны
            else
            {
                bool goToTreasureCell = Treasure && !pirate.Treasure;

                if (!IsOpened)
                    Open();
                else
                    HitPirates(pirate, allPirates: false);

                // если есть возможность взять монетку
                if (goToTreasureCell)
                    return TreasureCell.AddPirate(pirate);

                return Enter.AddPirate(pirate);
            }
        }
        public void AddDrunkMissioner(Pirate pirate)
        {
            _cave.Pirates.Add(pirate);
            Pirates.Add(pirate);
            pirate.Cell = this;
        }
    }
}
