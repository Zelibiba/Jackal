using Avalonia.Media.Imaging;
using Jackal.Models.Pirates;
using Microsoft.CodeAnalysis;
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
        public Cell(int row, int column, string image, bool isStandable = true)
        {
            Row = row;
            Column= column;
            Image = image;
            Pirates = new ObservableCollection<Pirate>();
            SelectableCoords = new List<int[]>();

            _treasure = this.WhenAnyValue(c => c.Gold, c => c.Galeon)
                            .Select(t => t.Item1 > 0 || t.Item2)
                            .ToProperty(this, c => c.Treasure);

            IsVisible = true;
            IsStandable = isStandable;
            Nodes = new ObservableCollection<Cell>() { this };
            Number = 0;
        }
        public static Cell Copy(Cell cell)
        {
            Cell _cell = new(cell.Row, cell.Column, cell.Image)
            {
                Pirates = cell.Pirates,
                IsOpened = cell.IsOpened,
                IsVisible = cell.IsVisible
            };

            return _cell;
        }

        [Reactive] public string Image { get; private set; }
        public virtual int Angle => 0;
        public int Row { get; protected set; }
        public int Column { get; protected set; }
        public int[] Coords => new int[] { Row, Column };
        public bool HasSameCoords(int row, int column) => row == Row && column == Column;
        public bool HasSameCoords(int[] coords) => coords.Length == 2 && HasSameCoords(coords[0], coords[1]);


        public ObservableCollection<Cell> Nodes { get; }
        public int Number { get; protected set; }
        public virtual Cell GetSelectedCell(Pirate pirate) => this;


        public readonly bool IsStandable;
        [Reactive] public bool IsVisible { get; set; }
        [Reactive] public bool IsPreOpened { get; set; }
        public bool IsOpened
        {
            get => __isOpened;
            protected set
            {
                __isOpened = value;
                IsPreOpened = value;
            }
        }
        bool __isOpened;
        public virtual void Open() => IsOpened = true;
        [Reactive] public bool CanBeSelected { get; set; }
        [Reactive] public bool IsSelected { get; set; }

        [Reactive] public virtual int Gold { get; set; }
        [Reactive] public virtual bool Galeon { get; set; }
        public bool Treasure => _treasure.Value;
        readonly ObservableAsPropertyHelper<bool> _treasure;
        public virtual bool IsGoldFriendly() => IsOpened;

        public ObservableCollection<Pirate> Pirates { get; protected set; }
        public List<int[]> SelectableCoords { get; }

        public virtual bool IsShip => false;
        public virtual Team ShipTeam => Team.None;


        public virtual void RemovePirate(Pirate pirate)
        {
            Pirates.Remove(pirate);

            if (pirate.Gold)
                Gold--;
            else if (pirate.Galeon)
                Galeon = false;
        }
        public virtual MovementResult AddPirate(Pirate pirate)
        {
            Pirates.Add(pirate);
            pirate.Cell = this;

            if (pirate.Gold)
                Gold++;
            else if (pirate.Galeon)
                Galeon = true;

            if (!IsOpened)
                Open();

            return IsStandable ? MovementResult.End : MovementResult.Continue;
        }


        public virtual void SetCoordinates(int row, int column)
        {
            Row = row;
            Column = column;
        }
        public virtual void SetSelectableCoords(ObservableMap map)
        {
            SelectableCoords.Clear();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if ((i == 0 && j == 0) || map[Row + i, Column + j] is SeaCell)
                        continue;
                    SelectableCoords.Add(new int[2] { Row + i, Column + j });
                }
            }
        }
    }
}
