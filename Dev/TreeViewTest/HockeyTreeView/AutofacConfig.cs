using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using HockeyTreeView.Providers;
using HockeyTreeView.ViewModels;
using HockeyTreeView.Views;

namespace HockeyTreeView;

public static class AutofacConfig
{
	public static IContainer Configure()
	{
		var builder = new ContainerBuilder();

		builder.RegisterType<MainViewModel>().AsSelf();
		builder.RegisterType<NhlLeagueDataProvider>().As<ILeagueDataProvider>();
		builder.RegisterType<MainWindow>().AsSelf();

		return builder.Build();
	}
}
