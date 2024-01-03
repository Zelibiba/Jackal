using Jackal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal
{
    public static class StreamExtentions
    {
        public static Team ReadTeam(this BinaryReader reader)
        {
            return (Team)reader.ReadInt32();
        }
        public static AllianceIdentifier ReadAllianceIdentifier(this BinaryReader reader)
        {
            return (AllianceIdentifier)reader.ReadInt32();
        }
        public static void Write(this BinaryWriter writer, Team team)
        {
            writer.Write((int)team);
        }
        public static void Write(this BinaryWriter writer, AllianceIdentifier allianceIdentifier)
        {
            writer.Write((int)allianceIdentifier);
        }


        public static GameProperties ReadGameProperties(this BinaryReader reader)
        {
            Dictionary<string, (int, char)> pattern = new();
            if (reader.ReadBoolean())
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                    pattern.Add(reader.ReadString(), (reader.ReadInt32(), reader.ReadChar()));
            }

            return new GameProperties()
            {
                Seed = reader.ReadInt32(),
                MapType = (MapType)reader.ReadInt32(),
                PatternName = reader.ReadString(),
                Size = reader.ReadInt32(),
                MapPattern = pattern,
            };
        }
        public static void Write(this BinaryWriter writer, GameProperties properties, bool withPattern = false)
        {
            writer.Write(withPattern);
            if (withPattern)
            {
                writer.Write(properties.MapPattern.Count);
                foreach ((string name, var value) in properties.MapPattern)
                {
                    writer.Write(name);
                    writer.Write(value.count);
                    writer.Write(value.param);
                }
            }
            writer.Write(properties.Seed);
            writer.Write((int)properties.MapType);
            writer.Write(properties.PatternName);
            writer.Write(properties.Size);
        }



        public static Player ReadPlayer(this BinaryReader reader, bool isControllable = false)
        {
            return new Player(index: reader.ReadInt32(),
                              name: reader.ReadString(),
                              team: reader.ReadTeam(),
                              isControllable)
            {
                AllianceIdentifier = reader.ReadAllianceIdentifier(),
                IsReady = reader.ReadBoolean(),
                Gold = reader.ReadInt32(),
                Bottles = reader.ReadInt32()
            };
        }
        public static void Write(this BinaryWriter writer, Player player)
        {
            writer.Write(player.Index);
            writer.Write(player.Name);
            writer.Write(player.Team);
            writer.Write(player.AllianceIdentifier);
            writer.Write(player.IsReady);
            writer.Write(player.Gold);
            writer.Write(player.Bottles);
        }


        public static NetMode ReadNetMode(this BinaryReader reader)
        {
            return (NetMode)reader.ReadInt32();
        }
        public static void Write(this BinaryWriter writer, NetMode mode)
        {
            writer.Write((byte)10);
            writer.Write((int)mode);
        }


        public static Coordinates ReadCoords(this BinaryReader reader)
        {
            return new Coordinates(reader.ReadInt32(), reader.ReadInt32());
        }
        public static void Write(this BinaryWriter writer, Coordinates coords)
        {
            writer.Write(coords.Row);
            writer.Write(coords.Column);
        }
    }
}
