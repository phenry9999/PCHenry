using System.Windows;
using Autofac;
using HockeyTreeView.Views;

namespace HockeyTreeView;

public partial class App : Application
{
	private IContainer container;

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		container = AutofacConfig.Configure();

		var mainWindow = container.Resolve<MainWindow>();
		mainWindow.Show();
	}
}

