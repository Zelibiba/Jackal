using Jackal.Views;
using ReactiveUI.Fody.Helpers;

namespace Jackal.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Content = new MainMenuViewModel();
        }
        [Reactive] public ViewModelBase Content { get; set; }

        public void CreateServer(object param)
        {
            Content = new WaitingRoomViewModel();
        }
        public void Cansel()
        {
            Content = new MainMenuViewModel();
        }
    }
}