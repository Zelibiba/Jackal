using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Animators;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Jackal.Models;
using Jackal.Models.Cells;
using Jackal.Models.Pirates;
using Jackal.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jackal.Views
{
    public partial class GameView : ReactiveUserControl<GameViewModel>
    {
        public GameView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });

            PointerPressed += (s, e) =>
            {
                PointerPoint point = e.GetCurrentPoint(this);
                if (point.Properties.IsRightButtonPressed)
                    ViewModel.Deselect();
            };
        }
    }
}
