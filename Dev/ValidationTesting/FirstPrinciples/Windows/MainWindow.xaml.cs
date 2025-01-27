using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FirstPrinciples.ViewModels;

namespace FirstPrinciples.Windows;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private MainWindowViewModel _viewModel;

	public MainWindow(MainWindowViewModel viewModel)
	{
		InitializeComponent();

		_viewModel = viewModel;
		DataContext = _viewModel;
		Loaded += MainWindow_Loaded;
	}

	private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
	{
		await _viewModel.LoadAsync();
	}
}