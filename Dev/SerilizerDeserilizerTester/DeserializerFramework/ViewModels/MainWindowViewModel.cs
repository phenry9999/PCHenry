using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DeserializerFramework.ViewModels
{
	[ObservableObject]
	public partial class MainWindowViewModel
	{
		public MainWindowViewModel()
		{
			//LoadTestData();
		}

		[ObservableProperty]
		private string _customerFrom;

		[ObservableProperty]
		private string _customerTo;

		[ObservableProperty]
		private string _customerPrefix;

		[RelayCommand]
		private void Deserialize()
		{
			MessageBox.Show("Testing");
		}

		[RelayCommand]
		private void Exit()
		{
			System.Windows.Application.Current.Shutdown();
		}

	}
}
