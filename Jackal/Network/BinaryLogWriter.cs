using Jackal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Network
{
    /// <summary>
    /// Класс записи бинарного потока с логированием.
    /// </summary>
    public class BinaryLogWriter : BinaryWriter
    {
        /// <summary>
        /// Класс записи бинарного потока с логированием.
        /// </summary>
        /// <param name="stream">Основной поток.</param>
        public BinaryLogWriter(NetLogStream stream): base(stream)
        {
            _writer = stream.Writer;
        }

        readonly StreamWriter? _writer;

        /// <summary>
        /// <inheritdoc cref="BinaryWriter.Write(bool)" path="/summary"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="end">Символ, идущий следом за записью в логе.</param>
        public void Write(bool value, char? end=' ')
        {
            _writer?.Write('^');
            _writer?.Write(value);
            if (end != null)
                _writer?.Write(end);
            base.Write(value);
        }
        /// <summary>
        /// <inheritdoc cref="BinaryWriter.Write(int)" path="/summary"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="end">Символ, идущий следом за записью в логе.</param>
        public void Write(int value, char? end = ' ')
        {
            _writer?.Write('^');
            _writer?.Write(value);
            if (end != null)
                _writer?.Write(end);
            base.Write(value);
        }
        /// <summary>
        /// <inheritdoc cref="BinaryWriter.Write(char)" path="/summary"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="end">Символ, идущий следом за записью в логе.</param>
        public void Write(char value, char? end = ' ')
        {
            _writer?.Write('^');
            _writer?.Write(value);
            if (end != null)
                _writer?.Write(end);
            base.Write(value);
        }
        /// <summary>
        /// <inheritdoc cref="BinaryWriter.Write(string)" path="/summary"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="end">Символ, идущий следом за записью в логе.</param>
        public void Write(string value, char? end = ' ')
        {
            _writer?.Write('^');
            _writer?.Write(value);
            if (end != null)
                _writer?.Write(end);
            base.Write(value);
        }

        /// <summary>
        /// Записывает <see cref="Team"/> в поток в виде целого числа.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="end">Символ, идущий следом за записью в логе.</param>
        public void Write(Team team, char? end=' ')
        {
            _writer?.Write("team=");
            Write((int)team, end);
        }
        /// <summary>
        /// Записывает <see cref="AllianceIdentifier"/> в поток в виде целого числа.
        /// </summary>
        /// <param name="alliance"></param>
        /// <param name="end">Символ, идущий следом за записью в логе.</param>
        public void Write(AllianceIdentifier alliance, char? end= ' ')
        {
            _writer?.Write("AllId=");
            Write((int)alliance, end);
        }
        /// <summary>
        /// Записывает <see cref="NetMode"/> в виде бита 10 и целого числа.
        /// </summary>
        /// <param name="mode"></param>
        public void Write(NetMode mode)
        {
            _writer?.Write("NetMode: ");
            base.Write((byte)10);
            Write((int)mode);
        }
        /// <summary>
        /// Записывает <see cref="GameProperties"/> в поток.
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="withPattern">Флаг того, что паттерн распределения клеток <see cref="GameProperties.MapPattern"/> тоже записывается</param>
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
        /// <summary>
        /// Записывает <see cref="Player"/> в поток.
        /// </summary>
        /// <param name="player"></param>
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
        /// <summary>
        /// Записывает <see cref="Coordinates"/> в поток в виде пары целых чисел.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="end">Символ, идущий следом за записью в логе.</param>
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
