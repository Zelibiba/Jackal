﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Pirates
{
    public class Ben : Pirate
    {
        public Ben(Player owner, Player manager, string image = "Ben") : base(owner, manager, image: image) { }

        public override bool CanHaveSex => false;
    }
}
