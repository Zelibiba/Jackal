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
        Black = 8,
        Green = 16,
        Purple = 32,
    }

    /// <summary>
    /// Перечисление возможных команд игроков.
    /// </summary>
    public enum AllianceIdentifier
    {
        None,
        Red,
        Blue,
        Green
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
        ChangeGameProperties,
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
        Angle4,

        Hex1,
        Hex2a,
        Hex2b,
        Hex3a,
        Hex3b,
        Hex3c,
        Hex4a,
        Hex4b
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
    /// <summary>
    /// Перечисление типов карты.
    /// </summary>
    /// <remarks>Карта может быть из квадратных или гексагональных ячеек.</remarks>
    public enum MapType
    {
        Quadratic,
        Hexagonal
    }
}
