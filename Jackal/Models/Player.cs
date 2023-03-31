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
            IntAlliance = index;
            Team = team;
        }

        public readonly int Index;
        [Reactive] public string Name { get; set; }
        [Reactive] public int IntAlliance { get; set; }
        [Reactive] public Team Team { get; set; }
        [Reactive] public bool IsReady { get; set; }

        public void Copy(Player player)
        {
            Name = player.Name;
            IntAlliance = player.IntAlliance;
            Team = player.Team;
            IsReady = player.IsReady;
        }
    }
}
