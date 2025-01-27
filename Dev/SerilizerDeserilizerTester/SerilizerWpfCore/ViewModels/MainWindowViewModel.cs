using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeserializerFramework.Models;

namespace SerilizerWpfCore.ViewModels;

[ObservableObject]
public partial class MainWindowViewModel
{
	public MainWindowViewModel()
	{
	}

	[ObservableProperty]
	private string _connectionString;

	[ObservableProperty]
	private string _customerFrom;

	[ObservableProperty]
	private string _customerTo;

	[ObservableProperty]
	private string _customerPrefix;

	[RelayCommand]
	private void Serialize()
	{
		MessageBox.Show("Testing");
	}

	[RelayCommand]
	private void LaunchDeserialize()
	{
		var pathToDeserialize = "C:\\GitHub\\PCHenry\\Dev\\SerilizerDeserilizerTester\\DeserializerFramework\\bin\\Debug";
		var processInfo = new ProcessStartInfo(Path.Combine(pathToDeserialize, "DeserializerFramework.exe"));

		var reportParameters = new ReportParameters();

		reportParameters.ConnectionString = ConnectionString;

		reportParameters.Add("Customer From", CustomerFrom);
		reportParameters.Add("Customer To", CustomerTo);
		reportParameters.Add("Customer Prefix", CustomerPrefix);



		Process.Start(processInfo);
	}

	[RelayCommand]
	private void Exit()
	{
		System.Windows.Application.Current.Shutdown();
	}
}

#if DEBUG
public class MainWindowViewModelDesignDataDefault : MainWindowViewModel
{
	public MainWindowViewModelDesignDataDefault()
	{
	}
}

public class MainWindowViewModelDesignDataPopulated : MainWindowViewModel
{
	public MainWindowViewModelDesignDataPopulated()
	{
		CustomerFrom = "Albuquerque";
		CustomerTo = "Xerox";
		CustomerPrefix = "CUST";

		ConnectionString = "Data Source=thestig;Initial Catalog=Zza;Integrated Security=True";
	}
}
#endif