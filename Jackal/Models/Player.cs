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
using Jackal.Models.Pirates;
using Jackal.Models.Cells;
using DynamicData;

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

            Alliance = team;

            Pirates = new List<Pirate>()
            {
                new Pirate(this),
                new Pirate(this),
                new Pirate(this)
            };
        }

        public readonly int Index;
        [Reactive] public string Name { get; set; }

        [Reactive] public Team Team { get; set; }
        [Reactive] public int IntAlliance { get; set; }
        public Team Alliance { get;private set; }

        [Reactive] public bool IsReady { get; set; }
        public void Copy(Player player)
        {
            Name = player.Name;
            IntAlliance = player.IntAlliance;
            Team = player.Team;
            IsReady = player.IsReady;
        }


        public List<Pirate> Pirates { get; }
        public ShipCell Ship { get; private set; }
        public void SetShip(ShipCell ship)
        {
            Ship = ship;
            foreach (Pirate pirate in Pirates)
                Ship.AddPirate(pirate);
        }

        [Reactive] public bool Turn { get; set; }

        [Reactive] public int Gold { get; set; }
        [Reactive] public int Bottles { get; set; }
    }
}
