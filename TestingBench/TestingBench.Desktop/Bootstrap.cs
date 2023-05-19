using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.OData;

namespace TestingBench.Desktop;

public class Bootstrapper
{
    public IContainer Bootstrap()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<MainWindow>().AsSelf();

        builder.RegisterType<MainWindowViewModel>().AsSelf();

        //builder.RegisterType<InvoiceControlCodeDataService>().As<IInvoiceControlCodeDataService>();

        return builder.Build();
    }
}
