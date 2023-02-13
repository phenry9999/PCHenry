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
using TableConversionUtility.Behaviors;
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

		private string dataToImport;
		public string DataToImport
		{
			get => dataToImport;
			set
			{
				dataToImport = value;
				RaisePropertyChanged();
			}
		}

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
			LoadEmployees(employees);
		}

		private void Add(object? parameter)
		{
			var importedEmployees = ConversionDataParser.Parse(DataToImport);

			Employees.Clear();
			LoadEmployees(importedEmployees);
		}

		private void Sort(object? parameter)
		{
			var sortedEmployees = Employees.OrderBy(n => n.FirstName).ToList();
			LoadEmployees(sortedEmployees);
		}

		private void SaveAsXml(object? parameter)
		{
			SerializationUtility.SerializeDataToXmlFile(Employees);
		}
		private void LoadEmployees(IEnumerable<Employee>? employees)
		{
			if(employees != null)
			{
				Employees.Clear();

				foreach(var emp in employees)
				{
					Employees.Add(new EmployeeItemViewModel(emp));
				}
			}
		}

		private void LoadEmployees(IEnumerable<EmployeeItemViewModel> employees)
		{
			if(employees != null)
			{
				Employees.Clear();

				foreach(var emp in employees)
				{
					Employees.Add(emp);
				}
			}
		}
	}
}
