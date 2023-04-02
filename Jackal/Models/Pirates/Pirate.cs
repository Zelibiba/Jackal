using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Jackal.Models.Cells;

namespace Jackal.Models.Pirates
{
    public class Pirate : ReactiveObject
    {
        public Pirate(Team team)
        {
            Team = team;
        }

        public Cell Cell { get; set; }

        [Reactive] public Team Team { get; set; }

        [Reactive] public bool Gold { get; set; }
        [Reactive] public bool Galeon { get; set; }
        public bool Treasure => Gold || Galeon;



        public bool IsBlocked => false;
        public int MazeNodeNumber => 0;
        public bool IsInMaze => false;

        public void RemoveFromCell() => Cell.RemovePirate(this);
    }
}
