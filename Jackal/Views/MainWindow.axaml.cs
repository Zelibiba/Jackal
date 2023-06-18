using Avalonia.Controls;
using Jackal.Models;
using Jackal.Network;
using Jackal.ViewModels;
using System;

namespace Jackal.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainMenuViewModel();

            Closed += ServerStop;
        }

        void ServerStop(object? sender, EventArgs e)
        {
            if (Server.IsServerHolder)
                Server.Stop();
            else
                Client.Stop();
            
            FileHandler.Close();

            Closed -= ServerStop;
        }
    }
}