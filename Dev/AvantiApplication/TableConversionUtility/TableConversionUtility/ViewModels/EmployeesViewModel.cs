using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableConversionUtility.Data.Models;
using TableConversionUtility.Data.Providers;

namespace TableConversionUtility.ViewModels
{
	public class EmployeesViewModel
	{
		private readonly IEmployeeDataProvider employeeDataProvider;

		public ObservableCollection<Employee> Employees { get; }

		public EmployeesViewModel(IEmployeeDataProvider employeeDataProvider)
		{
			Employees = new();
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
					Employees.Add(emp);
				}
			}
		}
	}
}
