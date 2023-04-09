using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Jackal.Models.Cells;
using System.Reactive.Linq;

namespace Jackal.Models.Pirates
{
    public class Pirate : ReactiveObject
    {
        public static readonly Pirate Empty = new(Team.None) { IsVisible = true };
        public Pirate(Team team)
        {
            Team = team;
            IsVisible = true;

            _atHorse = this.WhenAnyValue(p => p.Cell)
                           .Skip(1)
                           .Select(cell => cell is HorseCell || (cell is LakeCell && AtHorse))
                           .ToProperty(this, p => p.AtHorse);
        }

        [Reactive] public bool IsSelected { get; set; }
        [Reactive] public bool IsVisible { get; set; }

        [Reactive] public Cell Cell { get; set; }
        public int Row => Cell.Row;
        public int Column => Cell.Column;

        [Reactive] public Team Team { get; set; }
        public virtual bool CanDriveShip => true;
        public bool AtHorse => _atHorse.Value;
        ObservableAsPropertyHelper<bool> _atHorse;

        [Reactive] public bool Gold { get; set; }
        [Reactive] public bool Galeon { get; set; }
        public bool Treasure => Gold || Galeon;



        public bool IsBlocked => false;
        public int MazeNodeNumber => 0;
        public bool IsInMaze => false;

        public void RemoveFromCell() => Cell.RemovePirate(this);
    }
}
