using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TableConversionUtility.ViewModels;

namespace TableConversionUtility
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainWindowViewModel viewModel;

		public MainWindow()
		{
			InitializeComponent();
			viewModel = new MainWindowViewModel();
			DataContext = viewModel;
			Loaded += this.OnLoad;
		}

		private void OnLoad(object sender, RoutedEventArgs e)
		{
			var t = viewModel.EmployeesViewModel.LoadAsync();
			t.Wait();
		}
	}
}
