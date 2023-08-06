using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    /// <summary>
    /// Перечисление команд.
    /// </summary>
    public enum Team
    {
        None = 0,
        White = 1,
        Yellow = 2,
        Red = 4,
        Black = 8
    }

    /// <summary>
    /// Перечисление типов сообщений клиент-сервер.
    /// </summary>
    public enum NetMode
    {
        None,
        Disconnect,
        NewPlayer,
        GetPlayer,
        UpdatePlayer,
        DeletePlayer,
        StartGame,
        MovePirate,
        MoveShip,
        EathQuake,
        LightHouse,
        DrinkRum,
        PirateBirth
    }
    /// <summary>
    /// Перечисление результатов перемещения пирата.
    /// </summary>
    public enum MovementResult
    {
        End,        // Полный конец движения
        Continue,   // Необходимо выбрать клетку, куда перемещаться дальше
        EarthQuake, // Происходит землетрясение
        LightHouse, // Происходит ход маяка
        Cannabis    // Происходит ход конопли
    }
    /// <summary>
    /// Перечисление ориентаций поворота клетки.
    /// </summary>
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
    /// <summary>
    /// Перечисление типов стрелок.
    /// </summary>
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
    /// <summary>
    /// Перечисление типов клеток с сокровищем.
    /// </summary>
    public enum GoldType
    {
        Gold1 = 1,
        Gold2,
        Gold3,
        Gold4,
        Gold5,
        Galeon
    }
    /// <summary>
    /// Перечисление типов местных жителей.
    /// </summary>
    public enum ResidentType
    {
        Ben,
        Friday,
        Missioner
    }
    /// <summary>
    /// Перечисление операций, совершнных игроком.
    /// </summary>
    /// <remarks>Необходимо для <see cref="SaveOperator"/>.</remarks>
    public enum Actions
    {
        MovePirate,
        MoveShip,
        CellSelection,
        DrinkRum,
        GetBirth
    }
}
