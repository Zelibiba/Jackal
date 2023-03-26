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
        public PlayerAdderViewModel(Player player, bool isControllable)
        {
            Player = player;
            IsControllable = isControllable;
        }

        public Player Player { get; }
        public bool IsControllable { get; }

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
