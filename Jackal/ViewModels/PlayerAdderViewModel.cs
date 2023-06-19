using Avalonia.Threading;
using Jackal.Models;
using Jackal.Network;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.ViewModels
{
    public class PlayerAdderViewModel : ViewModelBase
    {
        public PlayerAdderViewModel(Player player)
        {
            Player = player;
            if (player.IsControllable)
            {
                this.WhenAnyValue(vm => vm.Player.Name, vm => vm.Player.Team, vm => vm.Player.IntAlliance, vm => vm.Player.IsReady)
                    .Skip(1)
                    .Subscribe(x => Client.UpdatePlayer(Player));
            }
        }

        public Player Player { get; }
        public bool IsControllable => Player.IsControllable;

        public void ChangeAlliance()
        {
            if (Player.IntAlliance == 4)
                Player.IntAlliance = 1;
            else
                Player.IntAlliance++;
        }
        public void ChangeTeam()
        {
            if (Player.Team == Team.Black)
                Player.Team = Team.White;
            else
                Player.Team = (Team)(2 * (int)Player.Team);

        }
        public void SetReady()
        {
            Player.IsReady = !Player.IsReady;
        }
    }
}
