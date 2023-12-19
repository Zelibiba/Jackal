using Avalonia.Layout;
using Jackal.Models.Cells.Utilites;
using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class ArrowCell : Cell, IOrientableCell
    {
        public ArrowCell(int row, int column, ArrowType arrowType, int rotation, Func<Coordinates, MovementResult> continueMove) : base(row, column, "Arrow" + arrowType.ToString(), false)
        {
            _continueMove = continueMove;
            if (Map.Type == MapType.Quadratic)
            {
                Directions = arrowType switch
                {
                    ArrowType.Side1 =>  new Coordinates[] { new(-1, 0) },

                    ArrowType.Side2 =>  new Coordinates[] { new(-1, 0),
                                                            new(+1, 0) },

                    ArrowType.Side4 =>  new Coordinates[] { new(-1, 0),
                                                            new( 0,-1),
                                                            new(+1, 0),
                                                            new( 0,+1) },

                    ArrowType.Angle1 => new Coordinates[] { new(-1,-1) },

                    ArrowType.Angle2 => new Coordinates[] { new(-1,-1),
                                                            new(+1,+1) },

                    ArrowType.Angle3 => new Coordinates[] { new(-1,-1),
                                                            new(+1, 0),
                                                            new( 0,+ 1) },

                    ArrowType.Angle4 => new Coordinates[] { new(-1,-1),
                                                            new(+1,-1),
                                                            new(+1,+1),
                                                            new(-1,+1) },
                    _ => throw new ArgumentException("arrowType")
                };
            }
            else
            {
                Directions = arrowType switch
                {
                    ArrowType.Hex1 =>  new Coordinates[] { new(-1, 0) },

                    ArrowType.Hex2 =>  new Coordinates[] { new(-1, 0),
                                                           new(+1, 0) },

                    ArrowType.Hex3a => new Coordinates[] { new(-1, 0),
                                                           new(+1,-1),
                                                           new( 0,+1) },

                    ArrowType.Hex3b => new Coordinates[] { new(-1, 0),
                                                           new(+1, 0),
                                                           new( 0,-1) },

                    ArrowType.Hex3c => new Coordinates[] { new(-1, 0),
                                                           new(+1, 0),
                                                           new(-1,+1) },

                    ArrowType.Hex4a => new Coordinates[] { new(-1, 0),
                                                           new(+1, 0),
                                                           new(-1,+1),
                                                           new(+1,-1) },

                    ArrowType.Hex4b => new Coordinates[] { new(-1, 0),
                                                           new(+1, 0),
                                                           new(-1,+1),
                                                           new( 0,-1) },
                    _ => throw new ArgumentException("arrowType")
                };

            }

            _angle = (this as IOrientableCell).Rotate(rotation);
        }

        readonly Func<Coordinates, MovementResult> _continueMove;

        public Coordinates[] Directions { get; }
        public override int Angle => _angle;
        readonly int _angle;

        public override MovementResult AddPirate(Pirate pirate, int delay =0)
        {
            base.AddPirate(pirate, delay);
            if (pirate.IsInLoop)
            {
                pirate.LoopKill();
                return MovementResult.End;
            }
            if (SelectableCoords.Count == 1)
                return _continueMove(SelectableCoords[0]);
            else
                return MovementResult.Continue;
        }
        public override void SetSelectableCoords(Map map)
        {
            SelectableCoords.Clear();
            Coordinates coords = Coords;
            foreach (Coordinates direction in Directions)
                SelectableCoords.Add(coords + direction);
        }
    }
}
