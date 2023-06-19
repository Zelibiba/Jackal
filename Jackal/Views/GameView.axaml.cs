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

            #region Инициализация анимации перемещения пирата
            _pirateAnimation = new Animation
            {
                Duration = TimeSpan.FromSeconds(0.3),
                IterationCount = new IterationCount(1),
                FillMode = FillMode.None,
            };
            _pirateAnimation.Children.Add(new KeyFrame { KeyTime = TimeSpan.FromSeconds(0) });
            _pirateAnimation.Children[0].Setters.Add(new Setter() { Property = MarginProperty });
            _pirateAnimation.Children.Add(new KeyFrame { KeyTime = _pirateAnimation.Duration });
            _pirateAnimation.Children[1].Setters.Add(new Setter() { Property = MarginProperty });
            Game.StartPirateAnimation = StartPirateAnimation;
            #endregion

            #region Инициализация анимации перемещения клеток
            for (int i = 0; i < 2; i++)
            {
                _cellAnimation[i] = new Animation
                {
                    Duration = TimeSpan.FromSeconds(0.5),
                    IterationCount = new IterationCount(1),
                    FillMode = FillMode.None,
                    PlaybackDirection = PlaybackDirection.Reverse
                };
                _cellAnimation[i].Children.Add(new KeyFrame { KeyTime = TimeSpan.FromSeconds(0) });
                _cellAnimation[i].Children[0].Setters.Add(new Setter() { Property = MarginProperty });
                _cellAnimation[i].Children.Add(new KeyFrame { KeyTime = _cellAnimation[i].Duration });
                _cellAnimation[i].Children[1].Setters.Add(new Setter() { Property = MarginProperty });
            }
            Game.StartCellAnimation = StartCellAnimation;
            #endregion
        }
        
        double СellSize => MapControl.Bounds.Height / Game.MapSize;


        readonly Animation _pirateAnimation;
        async Task StartPirateAnimation(Cell cell)
        {
            PirateAnimator.Width = СellSize / 3;

            Cell[] cells = new Cell[2]
            {
                Game.SelectedPirate.Cell,
                cell
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
                y[i] = cells[i].Row * СellSize + (pirateNumbers[i] / 3) * PirateAnimator.Height + 7;
                x[i] = cells[i].Column * СellSize + (pirateNumbers[i] % 3) * PirateAnimator.Width + 3;
            }

            _pirateAnimation.Children[0].Setters[0].Value = new Thickness(x[0], y[0], 0, 0);
            _pirateAnimation.Children[1].Setters[0].Value = new Thickness(x[1], y[1], 0, 0);

            PirateAnimator.Margin = new Thickness(x[0], y[0], 0, 0);
            PirateAnimator.DataContext = Game.SelectedPirate;
            PirateAnimator.IsVisible = true;
            Game.SelectedPirate.IsVisible = false;
            await _pirateAnimation.RunAsync(PirateAnimator, null);
            PirateAnimator.IsVisible = false;
        }


        readonly Animation[] _cellAnimation = new Animation[2];
        async Task StartCellAnimation(Cell cell1, Cell cell2)
        {
            double[] x = new double[2];
            double[] y = new double[2];
            Cell[] cells = new Cell[2] { cell1, cell2 };
            CellView[] cellViews = new CellView[2] { CellAnimator1, CellAnimator2 };
            for (int i = 0; i < 2; i++)
            {
                Cell cell = Cell.Copy(cells[i]);
                cellViews[i].DataContext = cell;

                y[i] = cells[i].Row * СellSize;
                x[i] = cells[i].Column * СellSize;
            }

            Thickness[] thickness = new Thickness[2]
            {
                new Thickness(x[0], y[0], 0, 0),
                new Thickness(x[1], y[1], 0, 0)
            };

            _cellAnimation[0].Children[0].Setters[0].Value = thickness[0];
            _cellAnimation[0].Children[1].Setters[0].Value = thickness[1];
            _cellAnimation[1].Children[0].Setters[0].Value = thickness[1];
            _cellAnimation[1].Children[1].Setters[0].Value = thickness[0];

            Task[] tasks = new Task[2];
            for (int i = 0; i < 2; i++)
            {
                cellViews[i].Margin = thickness[1 - i];
                cellViews[i].IsVisible = cells[i] is not SeaCell;
                cells[i].IsVisible = false;
                tasks[i] = _cellAnimation[i].RunAsync(cellViews[i], null);
            }
            await Task.WhenAll(tasks);

            cellViews[0].IsVisible = false;
            cellViews[1].IsVisible = false;
            cells[0].IsVisible = true;
            cells[1].IsVisible = true;
        }
    }
}
