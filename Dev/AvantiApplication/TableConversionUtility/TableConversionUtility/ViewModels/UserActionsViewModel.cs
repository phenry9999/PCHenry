using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TableConversionUtility.Commands;
using TableConversionUtility.Data.Models;
using TableConversionUtility.Data.Providers;
using TableConversionUtility.Views;

namespace TableConversionUtility.ViewModels
{
	public class UserActionsViewModel : ViewModelBase
	{
		public DelegateCommand AddCommand { get; }
		public DelegateCommand SortCommand { get; }
		public DelegateCommand SaveAsXmlCommand { get; }

		public UserActionsViewModel()
		{
			AddCommand = new DelegateCommand(Add);
			SortCommand = new DelegateCommand(Sort);
			SaveAsXmlCommand = new DelegateCommand(SaveAsXml);
		}

		private void Add(object? parameter)
		{
			MessageBox.Show("Add");
		}

		private void Sort(object? parameter)
		{
			MessageBox.Show("Sort");
		}

		private void SaveAsXml(object? parameter)
		{
			MessageBox.Show("SaveAsXml");
		}
	}
}
