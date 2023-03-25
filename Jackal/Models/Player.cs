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
        public Player(string name, Team team)
        {
            Name = name;
            Team = team;
        }

        int _index;

        [Reactive] public string Name { get; set; }
        [Reactive] public Team Team { get; set; }
        [Reactive] public bool IsReady { get; set; }

        public void ChangeTeam()
        {
            if (Team == Team.Black)
                Team = Team.White;
            else
                Team = (Team)(2 * (int)Team);

        }
        public void SetReady()
        {
            IsReady = !IsReady;
        }

        public void NetWrite(BinaryWriter writer)
        {
            writer.Write(_index);
            writer.Write(Name);
            writer.Write((int)Team);
            writer.Write(IsReady);
        }

        public static Player NetRead(BinaryReader reader)
        {
            int index= reader.ReadInt32();
            string name= reader.ReadString();
            Team team=(Team)reader.ReadInt32();
            bool isReady=reader.ReadBoolean();

            return new Player(name, team)
                       {
                           Team = team,
                           IsReady = isReady
                       };
        }
    }
}
