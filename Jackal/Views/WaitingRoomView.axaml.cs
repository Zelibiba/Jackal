using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Jackal.Views
{
    public partial class WaitingRoomView : ReactiveUserControl<Views.WaitingRoomView>
    {
        public WaitingRoomView()
        {
            //InitializeComponent();
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
