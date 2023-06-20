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
            try
            {
                _file = new FileStream("..//..//..//saves//autosave.txt", FileMode.Create, FileAccess.Write);
                _writer = new StreamWriter(_file);
            }
            catch (IOException ex) { return; }

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
        static int[] Coordinates(string str)
        {
            string[] words = str.Split(',');
            return new int[2]
            {
                int.Parse(words[0][1..]),
                int.Parse(words[1][..^1])
            };
        }
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
            _file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(_file);
            if (reader == null)
                throw new FileLoadException("Не удалось прочесть " + filename);

            int playersCount = int.Parse(reader.ReadLine().Trim().Split(' ')[^1]);
            Player[] players = new Player[playersCount];
            string[] words;
            for (int i=0;i<playersCount; i++)
            {
                string line = reader.ReadLine().Trim();
                words = line.Split(' ');
                players[i] = new Player(i, line.Split(',')[0],
                                        (Team)int.Parse(words[^2][..words[^2].IndexOf('(')]),
                                        isControllable: true)
                                        { IntAlliance = int.Parse(words[^1]) };
            }

            reader.ReadLine();
            words = reader.ReadLine().Trim().Split(' ');
            int seed = int.Parse(words[1]);
            Game.CreateMap(players, seed, autosave: false);

            reader.ReadLine();
            while(true)
            {
                if(reader.EndOfStream)
                    break;
                string line = reader.ReadLine().Trim();
                if (string.IsNullOrEmpty(line))
                    break;
                words = line[9..].Split(' ');
                if (words[0] == "move" && words[1].StartsWith("pirate"))
                {
                    int index = int.Parse(words[1].Split('=')[1]);
                    bool gold = int.Parse(words[2].Split('=')[1]) == 1;
                    bool galeon = int.Parse(words[3].Split('=')[1][..^1]) == 1;
                    int[] coords = Coordinates(words[9]);
                    Game.SelectPirate(index, gold, galeon, coords);
                    Game.SelectCell(coords);
                }
                else if (words[0] == "move" && words[1] == "ship")
                {
                    Game.SelectCell(Game.CurrentPlayer.ManagedShip);
                    Game.SelectCell(Coordinates(words[3]));
                }
                else if (words[0] == "earthQuake" || words[0] == "lightHouse")
                    Game.SelectCell(Coordinates(words[1]));
                else if (words[0] == "drinkRum")
                {
                    int index = int.Parse(words[1].Split('=')[1]);
                    ResidentType type = (ResidentType)int.Parse(words[2].Split('=')[1]);
                    Game.SelectPirate(index);
                    Game.GetDrunk(type);
                }
                else if (words[0] == "getBirth")
                {
                    int index = int.Parse(words[1].Split('=')[1]);
                    Game.SelectPirate(index);
                    Game.PirateBirth();
                }
                else
                    throw new Exception("Неверная строка: " + line);
            }

            reader.Close();
            _file.Close();
        }
    }
}
