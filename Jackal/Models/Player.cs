using Jacal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    public class Player
    {
        public Player(string name, Team team)
        {
            Name = name;
            Team = team;
        }

        public string Name { get; set; }
        public Team Team { get; set; }

    }
}
