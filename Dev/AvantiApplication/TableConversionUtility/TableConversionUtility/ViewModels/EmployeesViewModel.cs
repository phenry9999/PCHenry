using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using TableConversionUtility.Commands;
using TableConversionUtility.Data.Models;
using TableConversionUtility.Data.Providers;
using TableConversionUtility.Utilities;
using TableConversionUtility.Views;

namespace TableConversionUtility.ViewModels
{
	public class EmployeesViewModel : ViewModelBase
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

		public ICommand SortCommand { get; }
		public ICommand AddCommand { get; internal set; }
		public ICommand SaveAsXmlCommand { get; internal set; }

		public EmployeesViewModel(IEmployeeDataProvider employeeDataProvider)
		{
			this.employeeDataProvider = employeeDataProvider;
			AddCommand = new DelegateCommand(Add);
			SortCommand = new DelegateCommand(Sort);
			SaveAsXmlCommand = new DelegateCommand(SaveAsXml);
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

		private void Add(object? parameter)
		{
			MessageBox.Show("Add");
		}

		private void Sort(object? parameter)
		{
			var sortedEmployees = Employees.OrderBy(n => n.FirstName).ToList();
			Employees.Clear();

			foreach(var emp in sortedEmployees)
			{
				Employees.Add(emp);
			}
		}

		private void SaveAsXml(object? parameter)
		{
			SerializationUtility.SerializeDataToXmlFile(Employees);
		}
	}
}
