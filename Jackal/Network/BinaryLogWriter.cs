using Jackal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Network
{
    public class BinaryLogWriter : BinaryWriter
    {
        public BinaryLogWriter(NetLogStream stream): base(stream)
        {
            _writer = stream.Writer;
        }

        readonly StreamWriter? _writer;

        public void Write(bool value, char? end=' ')
        {
            _writer?.Write('^');
            _writer?.Write(value);
            if (end != null)
                _writer?.Write(end);
            base.Write(value);
        }
        public void Write(int value, char? end = ' ')
        {
            _writer?.Write('^');
            _writer?.Write(value);
            if (end != null)
                _writer?.Write(end);
            base.Write(value);
        }
        public void Write(char value, char? end = ' ')
        {
            _writer?.Write('^');
            _writer?.Write(value);
            if (end != null)
                _writer?.Write(end);
            base.Write(value);
        }
        public void Write(string value, char? end = ' ')
        {
            _writer?.Write('^');
            _writer?.Write(value);
            if (end != null)
                _writer?.Write(end);
            base.Write(value);
        }

        public void Write(Team team, char? end=' ')
        {
            _writer?.Write("team=");
            Write((int)team, end);
        }
        public void Write(AllianceIdentifier alliance, char? end= ' ')
        {
            _writer?.Write("AllId=");
            Write((int)alliance, end);
        }
        public void Write(GameProperties properties, bool withPattern = false)
        {
            _writer?.Write("GameProperties: ");
            Write(withPattern, '\n');
            if (withPattern)
            {
                Write(properties.MapPattern.Count, '\n');
                foreach ((string name, var value) in properties.MapPattern)
                {
                    Write(name);
                    Write(value.count);
                    Write(value.param, '\n');
                }
            }
            
            Write(properties.Seed);
            Write((int)properties.MapType);
            Write(properties.PatternName);
            Write(properties.Size, '\n');
        }
        public void Write(Player player)
        {
            _writer?.Write("Player: ");
            Write(player.Index);
            Write(player.Name);
            Write(player.Team);
            Write(player.AllianceIdentifier);
            Write(player.IsReady);
            Write(player.Gold);
            Write(player.Bottles,'\n');
        }
        public void Write(NetMode mode)
        {
            _writer?.Write("NetMode: ");
            base.Write((byte)10);
            Write((int)mode);
        }
        public void Write(Coordinates coords, char? end=' ')
        {
            _writer?.Write('(');
            Write(coords.Row);
            Write(coords.Column, ')');
            if (end != null)
                _writer?.Write(end);
        }
    }
}
