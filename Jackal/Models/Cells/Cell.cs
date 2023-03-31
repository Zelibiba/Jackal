using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class Cell : ReactiveObject
    {
        public string Image => "Field";
        public bool CanGoTo => false;
        public bool IsSelected => false;
        public bool Ship => false;
        public Team ShipTeam => Team.Yellow;
        public bool Treasure => false;
    }
}
