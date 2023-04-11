﻿using Avalonia.Layout;
using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class ArrowCell : Cell, IOrientable
    {
        public ArrowCell(int row, int column, ArrowType arrowType, int rotation, Predicate<int[]> continueMove) : base(row, column, "Arrow" + arrowType.ToString(), false)
        {
            _continueMove = continueMove;
            switch (arrowType)
            {
                case ArrowType.Side1:
                    _orientations.Add(Orientation.Up);
                    break;
                case ArrowType.Side2:
                    _orientations.Add(Orientation.Up);
                    _orientations.Add(Orientation.Down);
                    break;
                case ArrowType.Side4:
                    _orientations.Add(Orientation.Up);
                    _orientations.Add(Orientation.Right);
                    _orientations.Add(Orientation.Down);
                    _orientations.Add(Orientation.Left);
                    break;
                case ArrowType.Angle1:
                    _orientations.Add(Orientation.RightUp);
                    break;
                case ArrowType.Angle2:
                    _orientations.Add(Orientation.RightUp);
                    _orientations.Add(Orientation.LeftDown);
                    break;
                case ArrowType.Angle3:
                    _orientations.Add(Orientation.RightUp);
                    _orientations.Add(Orientation.Down);
                    _orientations.Add(Orientation.Left);
                    break;
                case ArrowType.Angle4:
                    _orientations.Add(Orientation.RightUp);
                    _orientations.Add(Orientation.RightDown);
                    _orientations.Add(Orientation.LeftUp);
                    _orientations.Add(Orientation.LeftDown);
                    break;
            }

            _angle = (this as IOrientable).Rotate(rotation);
        }

        readonly Predicate<int[]> _continueMove;

        public override int Angle => _angle;
        readonly int _angle;
        List<Orientation> IOrientable.Orientations { get; } = new List<Orientation>();
        List<Orientation> _orientations => (this as IOrientable).Orientations;

        public override bool AddPirate(Pirate pirate)
        {
            base.AddPirate(pirate);
            if (SelectableCoords.Count == 1)
                return _continueMove(SelectableCoords[0]);
            else
                return false;
        }
        public override void SetSelectableCoords(ObservableMap map)
        {
            foreach (Orientation orientation in _orientations)
            {
                int r = Row;
                int c = Column;
                if (orientation.HasFlag(Orientation.Up))
                    r--;
                if (orientation.HasFlag(Orientation.Down))
                    r++;
                if (orientation.HasFlag(Orientation.Right))
                    c++;
                if (orientation.HasFlag(Orientation.Left))
                    c--;
                SelectableCoords.Add(new int[] { r, c });
            }
        }
    }
}
