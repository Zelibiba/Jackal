using Avalonia.Media.Imaging;
using Jackal.Models.Pirates;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    /// <summary>
    /// Базовый класс ячейки.
    /// </summary>
    public class Cell : ReactiveObject
    {
        public Cell(int row, int column, string image)
        {
            this.row = row;
            this.column = column;
            Image = image;
            Pirates = new ObservableCollection<Pirate>();
            SelectableCoords = new List<int[]>();

            _containsGold = this.WhenAnyValue(c => c.Gold)
                              .Select(gold => gold > 0)
                              .ToProperty(this, c => c.ContainsGold);
            _treasure = this.WhenAnyValue(c => c.Gold, c => c.Galeon)
                            .Select(t => t.Item1 > 0 || t.Item2)
                            .ToProperty(this, c => c.Treasure);
            IsOpened = true;
        }

        protected int row;
        protected int column;

        public int Row => row;
        public int Column => column;
        [Reactive] public string Image { get; private set; }
        public virtual int Angle => 0;

        [Reactive] public bool IsOpened { get; set; }
        [Reactive] public bool CanBeSelected { get; set; }

        public void SetGold(int g) => Gold = g;
        [Reactive] public int Gold { get; protected set; }
        [Reactive] public bool Galeon { get; protected set; }
        public bool ContainsGold => _containsGold.Value;
        readonly ObservableAsPropertyHelper<bool> _containsGold;
        public bool Treasure => _treasure.Value;
        readonly ObservableAsPropertyHelper<bool> _treasure;

        public ObservableCollection<Pirate> Pirates { get; protected set; }
        public List<int[]> SelectableCoords { get; }

        public bool IsSelected => false;
        public virtual bool IsShip => false;
        public virtual Team ShipTeam => Team.None;



        public void AddPirate(Pirate pirate)
        {
            if(!IsOpened)
                IsOpened = true;

            Pirates.Add(pirate);
            pirate.Cell = this;

            if (pirate.Gold)
                Gold++;
            else if (pirate.Galeon)
                Galeon = true;
        }
        public void RemovePirate(Pirate pirate)
        {
            Pirates.Remove(pirate);

            if (pirate.Gold)
                Gold--;
            else if (pirate.Galeon)
                Galeon = false;
        }

        public void SetCoordinates(int row,int column)
        {
            this.row = row;
            this.column = column;
        }
        public virtual void SetSelectableCoords(ObservableMap map)
        {
            SelectableCoords.Clear();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if ((i == 0 && j == 0) || map[row + i, column + j] is SeaCell)
                        continue;
                    SelectableCoords.Add(new int[2] { row + i, column + j });
                }
            }
        }
    }
}
