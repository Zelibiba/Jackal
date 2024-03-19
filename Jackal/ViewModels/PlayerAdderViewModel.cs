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
        public PlayerAdderViewModel(Player player, bool isControllable = false)
        {
            Player = player;
            IsControllable = isControllable;
            if (IsControllable)
            {
                this.WhenAnyValue(vm => vm.Player.Name, vm => vm.Player.Team, vm => vm.Player.AllianceIdentifier, vm => vm.Player.IsReady)
                    .Skip(1)
                    .Subscribe(x => Client.UpdatePlayer(Player));
            }
        }

        public Player Player { get; }
        public bool IsControllable { get; }

        public void ChangeAlliance()
        {
            if (Player.AllianceIdentifier == AllianceIdentifier.None)
                Player.AllianceIdentifier = AllianceIdentifier.Green;
            else
                Player.AllianceIdentifier--;
        }
        public void ChangeTeam()
        {
            if (Player.Team == Team.Purple)
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
