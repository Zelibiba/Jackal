﻿using Jackal.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        public static void Write(this BinaryWriter writer, Team team)
        {
            writer.Write((int)team);
        }


        public static Player ReadPlayer(this BinaryReader reader, bool isControllable = false)
        {
            return new Player(index: reader.ReadInt32(),
                              name: reader.ReadString(),
                              team: reader.ReadTeam(),
                              isControllable)
            {
                IntAlliance = reader.ReadInt32(),
                IsReady = reader.ReadBoolean()
            };
        }
        public static void Write(this BinaryWriter writer, Player player)
        {
            writer.Write(player.Index);
            writer.Write(player.Name);
            writer.Write(player.Team);
            writer.Write(player.IntAlliance);
            writer.Write(player.IsReady);
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
