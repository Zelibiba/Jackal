using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    public class Coordinates
    {
        static Coordinates()
        {
            if (Map.Type == MapType.Quadratic)
                _Abs = coord => Math.Max(Math.Abs(coord.Row), Math.Abs(coord.Column));
            else
                _Abs = coord => Math.Max(Math.Max(Math.Abs(coord.Row), Math.Abs(coord.Column)),
                                         Math.Abs(-coord.Row - coord.Column));
        }
        public Coordinates(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public int Row { get; }
        public int Column { get; }

        readonly static Func<Coordinates, int> _Abs;
        public int Abs() => _Abs(this);

        public static Coordinates operator +(Coordinates a, Coordinates b)
        {
            return new Coordinates(a.Row + b.Row, a.Column + b.Column);
        }
        public static Coordinates operator -(Coordinates a, Coordinates b)
        {
            return new Coordinates(a.Row - b.Row, a.Column - b.Column);
        }
        public static bool operator ==(Coordinates? a, Coordinates? b)
        {
            return a?.Row == b?.Row && a?.Column == b?.Column;
        }
        public static bool operator !=(Coordinates? a, Coordinates? b)
        {
            return a?.Row != b?.Row || a?.Column != b?.Column;
        }
    }
}
