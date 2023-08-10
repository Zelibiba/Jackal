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


        public static int[] ReadCoords(this BinaryReader reader)
        {
            return new int[] { reader.ReadInt32(), reader.ReadInt32() };
        }
        public static void Write(this BinaryWriter writer, int[] coords)
        {
            writer.Write(coords[0]);
            writer.Write(coords[1]);
        }
    }
}
