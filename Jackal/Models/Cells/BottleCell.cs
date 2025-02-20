﻿using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class BottleCell : Cell
    {
        public BottleCell(int row, int column, int count) : base(row, column, "Bottle" + count)
        {
            if (count < 1 || count > 3)
                throw new ArgumentException("Wrong bottles count!");
            _count = count;
            enterSound = Sounds.Bottles; 
        }

        readonly int _count;

        public override void Open()
        {
            base.Open();
            enterSound = Sounds.Usual;
            Game.HiddenBottles -= _count;
            int bottles = _count;
            Pirate pirate = Pirates[0];
            if (pirate is Friday)
            {
                bottles--;
                pirate.Kill();
            }
            else if (pirate is Missioner missioner)
            {
                bottles--;
                missioner.ConverToPirate();
            }

            pirate.Owner.Bottles += bottles;
        }
    }
}
