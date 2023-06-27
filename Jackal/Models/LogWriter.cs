using Jackal.Models.Cells;
using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    /// <summary>
    /// Класс для логирования действий игроков в игре.
    /// </summary>
    public static class LogWriter
    {
        static Action<string>? WriteLog => Game.WriteLog;

        static string Coordinates(Cell cell) => string.Format("({0},{1})", cell.Row, cell.Column);

        public static void MovePirate(Pirate pirate, Cell targetCell)
        {
            WriteLog?.Invoke(string.Format("{0}: юнит переместился с {1} {2} на {3} {4}.",
                Game.CurrentPlayer.Name, pirate.Cell.Image.Split('_')[0], Coordinates(pirate.Cell),
                targetCell.Image.Split('_')[0], Coordinates(targetCell)));
        }
        public static void MoveShip(Cell cell)
        {
            WriteLog?.Invoke(string.Format("{0}: корабль переместился на клетку {1}.",
                Game.CurrentPlayer.Name, Coordinates(cell)));
        }
        public static void EarthQuake(Cell cell1, Cell cell2)
        {
            WriteLog?.Invoke(string.Format("{0}: землетрясение поменяло {1} {2} и {3} {4} местами.",
                Game.CurrentPlayer.Name,
                cell1.Image.Contains("_gray") ? cell1.Image : "", Coordinates(cell1),
                cell2.Image.Contains("_gray") ? cell2.Image : "", Coordinates(cell2)));
        }
        public static void DrinkRum(ResidentType type)
        {
            string str = "";
            switch (type)
            {
                case ResidentType.Ben:
                    str = "пирата"; break;
                case ResidentType.Friday:
                    str = "Пятницу"; break;
                case ResidentType.Missioner:
                    str = "миссионера"; break;
            }
            WriteLog?.Invoke(string.Format("{0}: споил {1}.",
                Game.CurrentPlayer.Name, str));
        }
        public static void GetBirth()
        {
            WriteLog?.Invoke(string.Format("{0}: родил пирата.", Game.CurrentPlayer.Name));
        }
    }
}
