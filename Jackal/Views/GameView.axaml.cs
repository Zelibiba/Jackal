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

            #region »нициализаци€ анимации перемещени€ пирата
            _pirateAnimation = new Animation
            {
                Duration = TimeSpan.FromSeconds(0.3),
                IterationCount = new IterationCount(1),
                FillMode = FillMode.Forward
            };
            _pirateAnimation.Children.Add(new KeyFrame { KeyTime = TimeSpan.FromSeconds(0) });
            _pirateAnimation.Children[0].Setters.Add(new Setter() { Property = MarginProperty });
            _pirateAnimation.Children.Add(new KeyFrame { KeyTime = _pirateAnimation.Duration });
            _pirateAnimation.Children[1].Setters.Add(new Setter() { Property = MarginProperty });
            Game.StartPirateAnimation = StartPirateAnimation;
            Game.EndPirateMove = () => PirateAnimator.IsVisible = false;
            #endregion
        }


        readonly Animation _pirateAnimation;
        async Task StartPirateAnimation(Cell cell)
        {
            Cell[] cells = new Cell[2]
            {
                Game.SelectedPirate.Cell,
                cell
            };
            int[] pirateNumbers = new int[2]
            {
                _reversedIndexes[cells[0].Pirates.IndexOf(Game.SelectedPirate)],
                _reversedIndexes[cells[1].Pirates.Count]
            };
            double[] x = new double[2];
            double[] y = new double[2];
            for (int i = 0; i < 2; i++)
            {
                y[i] = cells[i].Y + (pirateNumbers[i] / 3) * PirateAnimator.Height + 11;
                x[i] = cells[i].X + (pirateNumbers[i] % 3) * PirateAnimator.Width + 4;
            }

            _pirateAnimation.Children[0].Setters[0].Value = new Thickness(0, y[0], x[0], 0);
            _pirateAnimation.Children[1].Setters[0].Value = new Thickness(0, y[1], x[1], 0);

            PirateAnimator.Margin = new Thickness(0, y[0], x[0], 0);
            PirateAnimator.DataContext = Game.SelectedPirate;
            PirateAnimator.IsVisible = true;
            Game.SelectedPirate.IsVisible = false;
            await _pirateAnimation.RunAsync(PirateAnimator, null);
        }
        int[] _reversedIndexes = new int[6]
        {
            2, 1, 0,
            5, 4, 3
        };
    }
}
