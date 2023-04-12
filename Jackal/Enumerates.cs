using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal
{
    public enum Team
    {
        None = 0,
        White = 1,
        Yellow = 2,
        Red = 4,
        Black = 8
    }

    public enum NetMode
    {
        None,
        Disconnect,
        NewPlayer,
        UpdatePlayer,
        DeletePlayer
    }
    public enum Orientation
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 4,
        Left = 8,
        RightUp = Right | Up,       // 3
        RightDown = Right | Down,   // 6
        LeftUp = Left | Up,         // 9
        LeftDown = Left | Down      // 12
    }
    public enum ArrowType
    {
        Side1,
        Side2,
        Side4,
        Angle1,
        Angle2,
        Angle3,
        Angle4
    }
    public enum Gold
    {
        Gold1 = 1,
        Gold2,
        Gold3,
        Gold4,
        Gold5,
        Galeon
    }
}
