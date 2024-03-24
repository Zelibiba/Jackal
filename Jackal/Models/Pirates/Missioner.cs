using Jackal.Models.Cells;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Pirates
{
    public class Missioner : Pirate
    {
        public Missioner(Cell cell, Player owner, Player manager) : base(cell, owner, manager, image: "Missioner", isFighter: false) { }

        public override bool CanDrinkRum => false;
        public override bool CanHaveSex => false;

        public override bool CanGrabGold => false;
        public override bool CanGrabGaleon => false;

        /// <summary>
        /// Метод превращает миссионера в пирата.
        /// </summary>
        public void ConverToPirate()
        {
            Game.AudioPlayer?.Play(Sounds.DrunkMissioner);
            Ben ben = new(Cell, Owner, Manager, image: "DrunkMissioner");
            if (Cell is CaveExitCell exit)
                exit.AddDrunkMissioner(ben);
            else
                Cell.AddPirate(ben);
            Kill();
        }
    }
}
