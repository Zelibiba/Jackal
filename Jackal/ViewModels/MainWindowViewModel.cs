using Avalonia.Controls;
using Jackal.Views;
using ReactiveUI.Fody.Helpers;
using System.Threading.Tasks;

namespace Jackal.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            //Content = new MainMenuViewModel();
            Content = new GameViewModel();
        }
        [Reactive] public ViewModelBase Content { get; set; }

        public async Task ConnectToServer(object param)
        {
            //IPWindow dialog = new IPWindow();
            //string ip = await dialog.ShowDialog<string>(param as Window);
            //if (!string.IsNullOrEmpty(ip))
            //    Content = new WaitingRoomViewModel(false, ip);
            Content = new WaitingRoomViewModel(false, Network.Server.IP);
        }
        public void CreateServer(object param)
        {
            Content = new WaitingRoomViewModel(true, string.Empty);
        }
        public void Cansel()
        {
            Content = new MainMenuViewModel();
        }
    }
}