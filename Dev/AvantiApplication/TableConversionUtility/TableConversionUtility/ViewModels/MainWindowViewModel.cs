using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TableConversionUtility.Data.Providers;
using TableConversionUtility.Views;

namespace TableConversionUtility.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public ImportDataViewModel ImportDataViewModel { get; set; }
		public UserActionsViewModel UserActionsViewModel { get; set; }
		public EmployeesViewModel EmployeesViewModel { get; set; }

		public MainWindowViewModel()
		{
			EmployeesViewModel = new EmployeesViewModel(new EmployeeDataProvider());
			ImportDataViewModel = new ImportDataViewModel();
			UserActionsViewModel = new UserActionsViewModel();

			UserActionsViewModel.AddCommand = EmployeesViewModel.AddCommand;
			UserActionsViewModel.SortCommand = EmployeesViewModel.SortCommand;
			UserActionsViewModel.SaveAsXmlCommand = EmployeesViewModel.SaveAsXmlCommand;
			EmployeesViewModel.DataToImport = ImportDataViewModel.DataToImport;
		}
	}
}