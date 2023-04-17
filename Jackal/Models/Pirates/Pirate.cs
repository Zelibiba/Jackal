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
        public static readonly Pirate Empty = new() { IsVisible = true };
        public Pirate() { }
        public Pirate(Player owner, string? image = null)
        {
            Owner = owner;
            Manager = owner;
            Image = image ?? Team.ToString();
            IsVisible = true;

            this.WhenAnyValue(p => p.Gold)
                .Skip(1)
                .Subscribe(x =>
                {
                    if (!Cell.IsStandable)
                    {
                        Cell.Gold--;
                        StartCell.Gold++;
                    }
                });
            this.WhenAnyValue(p => p.Galeon)
                .Skip(1)
                .Subscribe(x =>
                {
                    if (!Cell.IsStandable)
                    {
                        Cell.Galeon = false;
                        StartCell.Galeon = true;
                    }
                });

            _atHorse = this.WhenAnyValue(p => p.Cell)
                           .Skip(1)
                           .Select(cell => cell is HorseCell || (cell is LakeCell && AtHorse))
                           .ToProperty(this, p => p.AtHorse);
            _atAirplane = this.WhenAnyValue(p => p.Cell)
                              .Skip(1)
                              .Select(cell => cell is AirplaneCell airpane && airpane.IsActive
                                              || (cell is LakeCell && AtAirplane))
                              .ToProperty(this, p => p.AtAirplane);
            _mazeNodeNumber = this.WhenAnyValue(p => p.Cell)
                                  .Skip(1)
                                  .Select(cell => cell.Number)
                                  .ToProperty(this, p => p.MazeNodeNumber);
        }

        public Player Owner { get; protected set; }
        public Player Manager { get; private set; }
        public Team Team => Owner?.Team ?? Team.None;
        public Team Alliance => Owner.Alliance;

        [Reactive] public bool IsSelected { get; set; }
        [Reactive] public bool IsVisible { get; set; }
        public string Image { get; }


        [Reactive] public Cell Cell { get; set; }
        public int Row => Cell.Row;
        public int Column => Cell.Column;
        public int MazeNodeNumber => _mazeNodeNumber?.Value ?? 0;
        readonly ObservableAsPropertyHelper<int> _mazeNodeNumber;
        public Cell StartCell { get; protected set; }
        public void Set_StartCell() => StartCell = Cell;
        public Cell TargetCell;

        public virtual bool CanDriveShip => true;
        public bool AtHorse => _atHorse.Value;
        readonly ObservableAsPropertyHelper<bool> _atHorse;
        public bool AtAirplane => _atAirplane.Value;
        readonly ObservableAsPropertyHelper<bool> _atAirplane;

        [Reactive] public bool Gold { get; set; }
        [Reactive] public bool Galeon { get; set; }
        public bool Treasure => Gold || Galeon;



        public bool IsBlocked => false;

        public void RemoveFromCell() => Cell.RemovePirate(this);

        public void Kill()
        {
            Cell.Pirates.Remove(this);
            Manager.Pirates.Remove(this);
        }
    }
}
