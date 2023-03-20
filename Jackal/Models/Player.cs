using Jacal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;

namespace Jackal.Models
{
    public class Player : ReactiveObject
    {
        public Player(string name, Team team)
        {
            Name = name;
            Team = team;

            //IObservable<bool> can = this.WhenAnyValue(vm => vm.IsReady);
            //Command = ReactiveCommand.Create(() => { }, can);
        }

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
    }
}
