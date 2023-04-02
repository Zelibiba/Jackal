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
        None = 0,
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
        UpRight = Up | Right,       // 3
        RightDown = Right | Down,   // 6
        LeftUp = Left | Up,         // 9
        LeftDown = Down | Left      // 12
    }
}
