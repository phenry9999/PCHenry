using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestingBench.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost _host;
    public IServiceProvider ServiceProvider { get; private set; }
    public IServiceCollection ServiceColleciton { get; private set; }

    private void Application_Startup(object senderobject, StartupEventArgs e)
    {
        var bootstrapper = new Bootstrapper();
        var container = bootstrapper.Bootstrap();
        // Check input parameters and find different switches value e.g. /i: /u: /p: /cs: /db:
        // These switches come from AvMenu
        var mainWindow = container.Resolve<MainWindow>();
        mainWindow.Show();
    }
}