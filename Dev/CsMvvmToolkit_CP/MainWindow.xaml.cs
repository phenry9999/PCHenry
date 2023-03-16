
using System.Windows;

namespace CsMvvmToolkit_CP
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new TestingViewModel();
        }
    }
}
