using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TableConversionUtility.Commands;
using TableConversionUtility.Data.Models;
using TableConversionUtility.Data.Providers;
using TableConversionUtility.Views;

namespace TableConversionUtility.ViewModels
{
	public class UserActionsViewModel : ViewModelBase
	{
		public ICommand AddCommand { get; set; }
		public ICommand SortCommand { get; set; }
		public ICommand SaveAsXmlCommand { get; set; }

		public UserActionsViewModel()
		{
			//AddCommand = new DelegateCommand(Add);
			//SortCommand = new DelegateCommand(Sort);
			//SaveAsXmlCommand = new DelegateCommand(SaveAsXml);
		}

		private void Add(object? parameter)
		{
			MessageBox.Show("Add");
		}



		private void SaveAsXml(object? parameter)
		{
			MessageBox.Show("SaveAsXml");
		}
	}
}
