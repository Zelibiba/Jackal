using Jacal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.IO;

namespace Jackal.Models
{
    public class Player : ReactiveObject
    {
        public Player(int index, string name, Team team)
        {
            Index = index;
            Name = name;
            Team = team;
        }
        public Player(BinaryReader reader)
        {
            Index = reader.ReadInt32();
            Name = reader.ReadString();
            Team = (Team)reader.ReadInt32();
            IsReady = reader.ReadBoolean();
        }

        public readonly int Index;
        [Reactive] public string Name { get; set; }
        [Reactive] public Team Team { get; set; }
        [Reactive] public bool IsReady { get; set; }

        public void NetWrite(BinaryWriter writer)
        {
            writer.Write(Index);
            writer.Write(Name);
            writer.Write((int)Team);
            writer.Write(IsReady);
        }
    }
}
