using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace Jackal.Models
{
    public class Pirate : ReactiveObject
    {
        public Team Team => Team.Yellow;
        public bool IsBlocked => false;
        public int MazeNodeNumber => 0;
        public bool IsInMaze => false;
    }
}
