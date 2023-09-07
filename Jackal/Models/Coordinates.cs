using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    public class Coordinates
    {
        public Coordinates(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public int Row { get; }
        public int Column { get; }

        public int Distance()
        {
            return Math.Max(Math.Abs(Row), Math.Abs(Column));
        }

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
