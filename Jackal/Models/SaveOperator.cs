using Avalonia.Media;
using DynamicData;
using Jackal.Models.Cells;
using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    /// <summary>
    /// Класс чтения и записи файла автосохранения.
    /// </summary>
    public static class SaveOperator
    {
        static FileStream _file;
        static StreamWriter _writer;

        static public List<int[]> Operations { get; private set; }

        public static void StartAutosave(IEnumerable<Player> players, int seed)
        {
            try
            {
                _file = new FileStream("..//..//..//saves//autosave.txt", FileMode.Create, FileAccess.Write);
                _writer = new StreamWriter(_file);
            }
            catch (IOException) { return; }

            _writer?.WriteLine("players: " + players.Count());
            foreach (Player player in players)
                _writer?.WriteLine(string.Format("    {0}, {1}({2}), {3}", player.Name, (int)player.Team, player.Team, (int)player.AllianceIdentifier));

            _writer?.WriteLine();
            _writer?.WriteLine("seed: " + seed);
            _writer?.WriteLine();
            _writer?.Flush();

            Operations = new List<int[]>();
        }
        public static void Close()
        {
            _writer?.Close();
            _file?.Close();
        }

        static void WriteOperation(IEnumerable<int> operation)
        {
            string str = "";
            foreach (int i in operation)
                str += i.ToString() + "  ";
            _writer?.Write(str.PadRight(30) + "|  ");
        }
        static void WritePlayer() => _writer?.Write(string.Format("{0}:", Game.CurrentPlayer.Team).PadRight(9));
        static int PirateIndex(Pirate pirate) => Game.CurrentPlayer.Pirates.IndexOf(pirate);

        public static void MovePirate(Pirate pirate, Cell targetCell)
        {
            int index = PirateIndex(pirate);
            int gold = Convert.ToInt16(pirate.Gold);
            int galeon = Convert.ToInt16(pirate.Galeon);
            int[] operation = new int[] { (int)Actions.MovePirate, index, gold, galeon, targetCell.Row, targetCell.Column };
            Operations?.Add(operation);

            WriteOperation(operation);
            WritePlayer();
            _writer?.WriteLine(string.Format("move pirate={0} (gold={1} galeon={2}) from {3} to {4}",
                                             index, gold, galeon, pirate.Cell.Image, targetCell.Image));
            _writer?.Flush();
        }
        public static void MoveShip(Cell Cell)
        {
            int[] operation = new int[] { (int)Actions.MoveShip, Cell.Row, Cell.Column };
            Operations?.Add(operation);

            WriteOperation(operation);
            WritePlayer();
            _writer?.WriteLine(string.Format("move ship to ({0},{1})", Cell.Row, Cell.Column));
            _writer?.Flush();
        }
        public static void EarthQuake(Cell cell)
        {
            int[] operation = new int[] { (int)Actions.CellSelection, cell.Row, cell.Column };
            Operations?.Add(operation);

            WriteOperation(operation);
            _writer?.WriteLine(string.Format("earthQuake ({0},{1})", cell.Row, cell.Column));
            _writer?.Flush();
        }
        public static void LightHouse(Cell cell)
        {
            int[] operation = new int[] { (int)Actions.CellSelection, cell.Row, cell.Column };
            Operations?.Add(operation);

            WriteOperation(operation);
            _writer?.WriteLine(string.Format("lighthouse ({0},{1})", cell.Row, cell.Column));
            _writer?.Flush();
        }
        public static void DrinkRum(Pirate pirate, ResidentType residentType)
        {
            int index = PirateIndex(pirate);
            int[] operation = new int[] { (int)Actions.DrinkRum, index, (int)residentType };
            Operations?.Add(operation);

            WriteOperation(operation);
            _writer?.WriteLine(string.Format("drink rum pirate={0} type={1}", index, (int)residentType));
            _writer?.Flush();
        }
        public static void GetBirth(Pirate pirate)
        {
            int index = PirateIndex(pirate);
            int[] operation = new int[] { (int)Actions.DrinkRum, index };
            Operations?.Add(operation);

            WriteOperation(operation);
            _writer?.WriteLine(string.Format("get birth pirate={0}", index));
            _writer?.Flush();
        }

        public static (Player[], int, List<int[]>) ReadSave(string filename)
        {
            _file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(_file);
            if (reader == null)
                throw new FileLoadException("Не удалось прочесть " + filename);

            int playersCount = int.Parse(reader.ReadLine().Trim().Split(' ')[^1]);
            Player[] players = new Player[playersCount];
            string[] words;
            for (int i = 0; i < playersCount; i++)
            {
                string line = reader.ReadLine().Trim();
                words = line.Split(' ');
                players[i] = new Player(i, line.Split(',')[0],
                                        (Team)int.Parse(words[^2][..words[^2].IndexOf('(')]),
                                        isControllable: true)
                { AllianceIdentifier = (AllianceIdentifier)int.Parse(words[^1]) };
            }

            reader.ReadLine();
            words = reader.ReadLine().Trim().Split(' ');
            int seed = int.Parse(words[1]);

            List<int[]> operations = new List<int[]>();
            reader.ReadLine();
            while(true)
            {
                if(reader.EndOfStream)
                    break;
                string line = reader.ReadLine().Split('|')[0].Trim();
                if (string.IsNullOrEmpty(line))
                    break;

                operations.Add(line.Split("  ").Select(str => int.Parse(str)).ToArray());
            }

            reader.Close();
            _file.Close();

            return (players, seed, operations);
        }
    }
}
