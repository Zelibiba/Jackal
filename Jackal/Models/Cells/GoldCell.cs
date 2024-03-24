using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class GoldCell : Cell
    {
        public GoldCell(int row, int column, GoldType goldType) : base(row, column, goldType.ToString())
        {
            _goldType = goldType;
            enterSound = _goldType == GoldType.Galeon ? Sounds.Galeon : Sounds.Gold;
        }

        readonly GoldType _goldType;

        public override void Open()
        {
            base.Open();
            enterSound = Sounds.Usual;
            if (_goldType == GoldType.Galeon)
            {
                Galeon = 1;
                Game.HiddenGold -= 3;
            }
            else
            {
                Gold = (int)_goldType;
                Game.HiddenGold -= Gold;
            }
        }
    }
}
