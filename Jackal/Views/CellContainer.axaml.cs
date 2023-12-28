using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Jackal.Models;

namespace Jackal.Views
{
    public partial class CellContainer : UserControl
    {
        public CellContainer()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<MapType> MapTypeProperty=
            AvaloniaProperty.Register<CellContainer,MapType>(nameof(MapType), defaultValue: MapType.Quadratic);
        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<CellContainer, Orientation>(nameof(Orientation), defaultValue: Orientation.Vertical);

        public MapType MapType
        {
            get => GetValue(MapTypeProperty);
            set => SetValue(MapTypeProperty, value);
        }
        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
    }
}
