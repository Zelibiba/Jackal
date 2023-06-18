using Avalonia.Media;
using Jackal.Models.Cells;
using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    /// <summary>
    /// Класс чтения и записи файла автосохранения.
    /// </summary>
    public static class FileHandler
    {
        static FileStream _file;
        static StreamWriter _writer;

        public static void StartAutosave(IEnumerable<Player> players, int seed)
        {
            _file = new FileStream("..//..//..//saves//autosave.txt", FileMode.Create, FileAccess.Write);
            _writer = new StreamWriter(_file);

            _writer?.WriteLine("players: " + players.Count());
            foreach (Player player in players)
                _writer?.WriteLine(string.Format("    {0}, {1}({2}), {3}", player.Name, (int)player.Team, player.Team, player.IntAlliance));

            _writer?.WriteLine();
            _writer?.WriteLine("seed: " + seed);
            _writer?.WriteLine();
            _writer?.Flush();
        }
        public static void Close()
        {
            _writer?.Close();
            _file?.Close();
        }

        static void WritePlayer(Player player) => _writer?.Write(string.Format("{0}:", player.Team).PadRight(9));
        static string Coordinates(int[] coords) => string.Format("({0},{1})", coords[0], coords[1]);
        static int PirateIndex(Pirate pirate) => Game.CurrentPlayer.Pirates.IndexOf(pirate);

        public static void MovePirate(Pirate pirate, Cell targetCell)
        {
            WritePlayer(Game.CurrentPlayer);
            _writer?.WriteLine(
                string.Format("move pirate={0} (gold={1} galeon={2}) ", PirateIndex(pirate), Convert.ToInt16(pirate.Gold), Convert.ToInt16(pirate.Galeon)) +
                string.Format("from {0} {1} to {2} {3}", pirate.Cell.Image, Coordinates(pirate.Cell.Coords), targetCell.Image, Coordinates(targetCell.Coords)));
            _writer?.Flush();
        }
        public static void MoveShip(Cell Cell)
        {
            WritePlayer(Game.CurrentPlayer);
            _writer?.WriteLine("move ship to " + Coordinates(Cell.Coords));
            _writer?.Flush();
        }
        public static void EarthQuake(Cell cell)
        {
            WritePlayer(Game.CurrentPlayer);
            _writer?.WriteLine("earthQuake " + Coordinates(cell.Coords));
            _writer?.Flush();
        }
        public static void LightHouse(Cell cell)
        {
            WritePlayer(Game.CurrentPlayer);
            _writer?.WriteLine("lightHouse " + Coordinates(cell.Coords));
            _writer?.Flush();
        }
        public static void DrinkRum(Pirate pirate, ResidentType residentType)
        {
            WritePlayer(Game.CurrentPlayer);
            _writer?.WriteLine(string.Format("drinkRum pirate={0} type={1}", PirateIndex(pirate), (int)residentType));
            _writer?.Flush();
        }
        public static void GetBirth(Pirate pirate)
        {
            WritePlayer(Game.CurrentPlayer);
            _writer?.WriteLine("getBirth pirate=" + PirateIndex(pirate));
            _writer?.Flush();
        }

        public static void ReadSave(string filename)
        {

        }
    }
}
