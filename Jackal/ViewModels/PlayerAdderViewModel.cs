using Jacal;
using Jackal.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.ViewModels
{
    public class PlayerAdderViewModel : ViewModelBase
    {
        public PlayerAdderViewModel()
        {
            Player = new Player("Джон Псина", Team.White);
        }

        public Player Player { get; set; }
        public Team Team
        {
            get=>Player.Team;
            set
            {
                Player.Team = value;
                this.RaisePropertyChanged(nameof(Team));
            }
        }
        public string Name
        {
            get => Player.Name;
            set
            {
                Player.Name = value;
                this.RaisePropertyChanged(nameof(Name));
            }
        }
        [Reactive] public bool IsReady { get; set; }

        public void ChangeTeam()
        {
            if (Team == Team.Black)
                Team = Team.White;
            else
                Team = (Team)(2 * (int)Team);

        }
        public void GetReady()
        {
            IsReady = !IsReady;
        }
    }
}
