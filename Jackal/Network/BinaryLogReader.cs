using Jackal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Network
{
    public class BinaryLogReader : BinaryReader
    {
        public BinaryLogReader(NetLogStream stream):base(stream)
        {
            _writer = stream.Writer;
        }

        readonly StreamWriter? _writer;

        public bool ReadBoolean(char? end=' ')
        {
            bool b = base.ReadBoolean();
            _writer?.Write(b);
            if (end != null)
                _writer?.Write(end);
            return b;
        }
        public int ReadInt32(char? end=' ')
        {
            int i = base.ReadInt32();
            _writer?.Write(i);
            if (end != null)
                _writer?.Write(end);
            return i;
        }
        public char ReadChar(char? end = ' ')
        {
            char c = base.ReadChar();
            _writer?.Write(c);
            if (end != null)
                _writer?.Write(end);
            return c;
        }
        public string ReadString(char? end=' ')
        {
            string s = base.ReadString();
            _writer?.Write(s);
            if (end != null)
                _writer?.Write(end);
            return s;
        }

        public Team ReadTeam(char? end = ' ')
        {
            _writer?.Write("team=");
            return (Team)ReadInt32(end);
        }

        public AllianceIdentifier ReadAllianceIdentifier(char? end = ' ')
        {
            _writer?.Write("AllId=");
            return (AllianceIdentifier)ReadInt32(end);
        }
        public GameProperties ReadGameProperties()
        {
            _writer?.Write("Game Properties: ");
            Dictionary<string, (int, char)> pattern = new();
            if (ReadBoolean('\n'))
            {
                int count = ReadInt32('\n');
                for (int i = 0; i < count; i++)
                    pattern.Add(ReadString(), (ReadInt32(), ReadChar('\n')));
            }

            return new GameProperties()
            {
                Seed = ReadInt32(),
                MapType = (MapType)ReadInt32(),
                PatternName = ReadString(),
                Size = ReadInt32('\n'),
                MapPattern = pattern,
            };
        }
        public Player ReadPlayer(bool isControllable = false)
        {
            _writer?.Write("Player: ");
            return new Player(index: ReadInt32(),
                              name: ReadString(),
                              team: ReadTeam(),
                              isControllable)
            {
                AllianceIdentifier = ReadAllianceIdentifier(),
                IsReady = ReadBoolean(),
                Gold = ReadInt32(),
                Bottles = ReadInt32('\n')
            };
        }
        public NetMode ReadNetMode()
        {
            _writer?.Write("NetMode: ");
            return (NetMode)ReadInt32('\n');
        }
        public Coordinates ReadCoords(char? end=' ')
        {
            _writer?.Write('(');
            int i = ReadInt32();
            int j = ReadInt32(')');
            if (end != null)
                _writer?.Write(end);
            return new Coordinates(i, j);
        }
    }
}
