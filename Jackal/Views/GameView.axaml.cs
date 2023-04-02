using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Jackal.Models;
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

            PointerPressed += (s,e) =>
            {
                PointerPoint point = e.GetCurrentPoint(this);
                if(point.Properties.IsRightButtonPressed)
                {
                    (DataContext as GameViewModel).Deselect();
                }
            };
        }
    }
}
