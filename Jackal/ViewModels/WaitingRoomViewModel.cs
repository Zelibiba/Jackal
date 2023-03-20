using Jacal;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DynamicData;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Jackal.Models;
using DynamicData.Binding;

namespace Jackal.ViewModels
{
    public class WaitingRoomViewModel : ViewModelBase
    {
        public WaitingRoomViewModel()
        {
            Players = new ObservableCollection<Player>();
            CanStartServer = Players
                           .ToObservableChangeSet()
                           .AutoRefresh(player => player.IsReady)
                           .ToCollection()
                           .Select(x => x.All(player => player.IsReady));

            StartServerCommand = ReactiveCommand.Create(() => { }, CanStartServer);
        }
        public ObservableCollection<Player> Players { get; }
        public IObservable<bool> CanStartServer { get; private set; }

        public ReactiveCommand<Unit, Unit> StartServerCommand { get; }
        public void AddPlayer()
        {
            Players.Add(new Player("Джон Псина", Team.White));
        }
    }
}
