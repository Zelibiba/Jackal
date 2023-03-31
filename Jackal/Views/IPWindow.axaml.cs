using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Jackal.Views
{
    public partial class IPWindow : Window
    {
        public IPWindow()
        {
            InitializeComponent();
            _textBox = this.FindControl<TextBox>("TextBox");
        }

        readonly TextBox _textBox;

        public void TextBoxFocused(object sender, GotFocusEventArgs e)
        {
            if (_textBox.Background == Brush.Parse("Red"))
                _textBox.Background = Brush.Parse("White");
        }
        public void OkClick(object sender, RoutedEventArgs e)
        {
            string[] words = _textBox.Text.Split('.');

            bool isIp = false;
            if (words.Length == 4)
            {
                isIp = true;
                foreach (string word in words)
                {
                    byte i;
                    if (!byte.TryParse(word, out i))
                    {
                        isIp = false;
                        break;
                    }
                }
            }

            if (isIp)
                Close(_textBox.Text);
            else
                _textBox.Background = Brush.Parse("Red");
        }
        public void CanselClick(object sender, RoutedEventArgs e)
        {
            Close(string.Empty);
        }
    }
}
