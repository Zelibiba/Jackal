using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Pirates
{
    public class Missioner : Pirate
    {
        public Missioner(Player owner, Player manager) : base(owner, manager, image: "Missioner", isFighter: false) { }

        public override bool CanDrinkRum => false;
        public override bool CanHaveSex => false;

        public override bool CanGrabGold => false;
        public override bool CanGrabGaleon => false;

        /// <summary>
        /// Метод превращает миссионера в пирата.
        /// </summary>
        public void ConverToPirate()
        {
            Kill();
            Cell.AddPirate(new Ben(Owner, Manager, image: "DrunkMissioner"));
        }
    }
}
