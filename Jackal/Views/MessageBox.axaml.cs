using Avalonia.Controls;
using Microsoft.VisualBasic;

namespace Jackal.Views
{
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
        }


        public static void Show(string message)
        {
            MessageBox msg=new();
            msg.FindControl<TextBlock>("Text").Text=message;
            msg.FindControl<Button>("Button").Click += (s, o) => msg.Close();
            msg.Show();
        }
    }
}
