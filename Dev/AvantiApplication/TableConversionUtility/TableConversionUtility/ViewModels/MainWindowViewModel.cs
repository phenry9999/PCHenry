using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableConversionUtility.Data.Providers;
using TableConversionUtility.Views;

namespace TableConversionUtility.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private readonly IEmployeeDataProvider employeeDataProvider;
		private EmployeeItemViewModel? selectedEmployee;

		public ObservableCollection<EmployeeItemViewModel> Employees { get; } = new();

		public EmployeeItemViewModel? SelectedEmployee
		{
			get => selectedEmployee;
			set
			{
				selectedEmployee = value;
				RaisePropertyChanged();
			}
		}

		public MainWindowViewModel(IEmployeeDataProvider employeeDataProvider)
		{
			this.employeeDataProvider = employeeDataProvider;
		}

		public async Task LoadAsync()
		{
			if(Employees.Any())
			{
				return;
			}

			var employees = await employeeDataProvider.GetAllAsync();

			if(employees != null)
			{
				foreach(var emp in employees)
				{
					Employees.Add(new EmployeeItemViewModel(emp));
				}
			}
		}
	}
}
