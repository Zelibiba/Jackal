using Avalonia.Controls;
using Jackal.Network;
using System;

namespace Jackal.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainWindowViewModel();

            Closing += ServerStop;
        }

        void ServerStop(object? sender, EventArgs e)
        {
            Server.Stop();
            Closed -= ServerStop;
        }
    }
}