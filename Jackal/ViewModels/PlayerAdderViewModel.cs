using Jackal.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        }

        public Player Player { get; }
        [Reactive] public bool IsControllable { get; set; }

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
