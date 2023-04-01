using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Jackal.ViewModels;
using ReactiveUI;

namespace Jackal.Views
{
    public partial class GameView : ReactiveUserControl<GameViewModel>
    {
        public GameView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }
    }
}
