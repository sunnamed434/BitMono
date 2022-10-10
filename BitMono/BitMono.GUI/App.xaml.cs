using Microsoft.Maui.Controls;

namespace BitMono.GUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }
    }
}