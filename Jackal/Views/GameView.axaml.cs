using Avalonia.Controls;

namespace Jackal.Views
{
    public partial class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
            DataContext = new ViewModels.GameViewModel();
        }
    }
}
