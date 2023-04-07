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

            _pirateAnimation = new Animation
            {
                Duration = _pirateAnimationDuration,
                IterationCount = new IterationCount(1),
                FillMode = FillMode.None,
            };
            _pirateAnimation.Children.Add(new KeyFrame { KeyTime = TimeSpan.FromSeconds(0) });
            _pirateAnimation.Children[0].Setters.Add(new Setter() { Property = MarginProperty });
            _pirateAnimation.Children.Add(new KeyFrame { KeyTime = _pirateAnimationDuration });
            _pirateAnimation.Children[1].Setters.Add(new Setter() { Property = MarginProperty });
            Game.StartPirateAnimation += StartPirateAnimation;
        }
        
        double ÑellSize => MapControl.Bounds.Height / Game.MapSize;

        readonly Animation _pirateAnimation;
        readonly TimeSpan _pirateAnimationDuration = TimeSpan.FromSeconds(0.3);



        private async void StartPirateAnimation(object? sender, CellArgs e)
        {
            IsEnabled = false;
            PirateAnimator.IsVisible = true;
            PirateAnimator.Width = ÑellSize / 3;

            Cell[] cells = new Cell[2]
            {
                Game.SelectedPirate.Cell,
                e.Cell
            };
            int[] pirateNumbers = new int[2]
            {
                cells[0].Pirates.IndexOf(Game.SelectedPirate),
                cells[1].Pirates.Count
            };
            double[] x = new double[2];
            double[] y = new double[2];
            for (int i = 0; i < 2; i++)
            {
                y[i] = cells[i].Row * ÑellSize + (pirateNumbers[i] / 3) * PirateAnimator.Bounds.Height + 7;
                x[i] = cells[i].Column * ÑellSize + (pirateNumbers[i] % 3) * PirateAnimator.Bounds.Width + 3;
            }

            _pirateAnimation.Children[0].Setters[0].Value = new Thickness(x[0], y[0], 0, 0);
            _pirateAnimation.Children[1].Setters[0].Value = new Thickness(x[1], y[1], 0, 0);
            await _pirateAnimation.RunAsync(PirateAnimator, null);

            Game.MovePirate(e.Cell);
            PirateAnimator.IsVisible = false;
            IsEnabled = true;
        }
    }
}
