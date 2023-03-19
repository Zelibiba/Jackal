using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.ViewModels
{
    public class WaitingRoomViewModel : ViewModelBase
    {
        public WaitingRoomViewModel()
        {
            Players = new ObservableCollection<PlayerAdderViewModel>
            {
                new PlayerAdderViewModel()
            };
        }
        public ObservableCollection<PlayerAdderViewModel> Players { get; set; }
    }
}
