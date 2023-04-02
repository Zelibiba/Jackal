using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Jackal.Models.Cells;

namespace Jackal.Models
{
    public class Pirate : ReactiveObject
    {
        public Pirate(Team team)
        {
            Team = team;
        }

        public Cell Cell { get; set; }

        public Team Team {get;set;}
        public bool IsBlocked => false;
        public int MazeNodeNumber => 0;
        public bool IsInMaze => false;

        public void RemoveFromCell()
        {
            Cell.Pirates.Remove(this);
        }
    }
}
