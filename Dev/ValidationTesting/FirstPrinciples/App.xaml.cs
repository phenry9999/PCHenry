using System.Configuration;
using System.Data;
using System.Windows;
using FirstPrinciples.ViewModels;

namespace FirstPrinciples;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	private void Application_Startup(object sender, StartupEventArgs e)
	{
		MainWindowViewModel viewModel = new MainWindowViewModel();
		var mainWindow = new Windows.MainWindow(viewModel);
		mainWindow.Show();
	}

	//todo Need to add this to Atlas apps, don't forget to add to app.xaml part
	private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
	{
		MessageBox.Show("Unexpected error occurred." + Environment.NewLine +
			"Please inform the admin." + Environment.NewLine +
			e.Exception.Message,
			"Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
		e.Handled = true;
	}
}

