using Avalonia.Controls;
using Jackal.Models;
using Jackal.Views;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jackal.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Content = new MainMenuViewModel();
            //Content = new GameViewModel();
        }
        [Reactive] public ViewModelBase Content { get; set; }

        public void CreateServer(object param)
        {
            Content = new WaitingRoomViewModel(true, string.Empty);
        }
        public async Task ConnectToServer(object param)
        {
            IPWindow dialog = new IPWindow();
            string ip = await dialog.ShowDialog<string>(param as Window);
            if (!string.IsNullOrEmpty(ip))
                Content = new WaitingRoomViewModel(false, ip);
            Content = new WaitingRoomViewModel(false, Network.Server.IP);
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

            //vm.OpenGame(openFileDialog.FileName);
        }
        public void Cansel()
        {
            Content = new MainMenuViewModel();
        }
    }
}