using Avalonia.Controls;
using Jackal.Network;
using Jackal.Views;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        public MainMenuViewModel()
        {
            Content = this;
            //Content = new GameViewModel();
        }

        [Reactive] public ViewModelBase Content { get; set; }

        public void CreateServer(object param)
        {
            Content = new WaitingRoomViewModel(SetContent);
        }
        public async Task ConnectToServer(object param)
        {
            //IPWindow dialog = new IPWindow();
            //string ip = await dialog.ShowDialog<string>(param as Window);
            //if (!string.IsNullOrEmpty(ip))
            //    Content = new WaitingRoomViewModel(SetContent, ip: ip);
            Content = new WaitingRoomViewModel(SetContent, ip: Network.Server.IP);
        }
        public async void LoadGame(object param)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            string path = Environment.CurrentDirectory;
            path = path[..path.LastIndexOf("Jackal\\")];
            dialog.Directory = path + "Jackal\\saves";
            string[]? result = await dialog.ShowAsync(param as Window);
            if (result?[0] == null)
                return;

            Content = new GameViewModel(result[0]);
        }
        public void Cansel()
        {
            Content = this;
            if (Server.IsServerHolder)
                Server.Stop();
            else
                Client.Stop();
        }

        void SetContent(ViewModelBase viewModel)
        {
            Content = viewModel;
        }
    }
}
