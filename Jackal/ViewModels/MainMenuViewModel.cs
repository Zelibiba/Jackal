using Avalonia.Controls;
using Jackal.Models;
using Jackal.Network;
using Jackal.Views;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
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

            //CreateServer(null);

            //(Player[], GameProperties, List<int[]>) data = SaveOperator.ReadSave(Path.Combine(Properties.SavesFolder, "test.txt"));
            //Content = new GameViewModel(data.Item1, data.Item2, data.Item3);
        }

        public string Version => "Версия:    " + Properties.Version;

        [Reactive] public ViewModelBase Content { get; private set; }
        ViewModelBase GetContent() => Content;
        void SetContent(ViewModelBase viewModel) => Content = viewModel;

        public void CreateServer(object param)
        {
            Server.Start();
            Client.Start(Server.IP, SetContent);
        }
        public async Task ConnectToServer(object param)
        {
            IPWindow dialog = new IPWindow();
            string ip = await dialog.ShowDialog<string>(param as Window);
            if (!string.IsNullOrEmpty(ip))
                Client.Start(ip, SetContent);
            //Client.Start(Server.IP, SetContent);
        }
        public async void LoadGame(object param)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Directory = Properties.SavesFolder;
            string[]? result = await dialog.ShowAsync(param as Window);
            if (result?.Length == 0 || result?[0] == null)
                return;

            (Player[], GameProperties,List<int[]>) data = SaveOperator.ReadSave(result[0]);
            Content = new GameViewModel(data.Item1, data.Item2, data.Item3);
        }
        public void Cansel()
        {
            Content = this;
            if (Server.IsServerHolder)
                Server.Stop();
            else
                Client.Stop();
        }
    }
}
