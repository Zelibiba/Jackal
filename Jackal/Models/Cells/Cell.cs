using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class Cell : ReactiveObject
    {
        public Cell(int row, int column, string image)
        {
            this.row = row;
            this.column = column;
            Image = image;

            Pirates = new ObservableCollection<Pirate>
            {
                //new Pirate()
            };
        }

        protected int row;
        protected int column;
        public int Row => row;
        public int Column => column;

        [Reactive] public string Image { get; private set; }
        [Reactive] public int Gold { get; protected set; }
        [Reactive] public bool Galeon { get; protected set; }
        public ObservableCollection<Pirate> Pirates { get; protected set; }
        public bool CanBeSelected => false;
        public bool IsSelected => false;
        public bool Ship => false;
        public Team ShipTeam => Team.Yellow;
        public bool Treasure => false;
        public bool ContainsGold => false;
    }
}
