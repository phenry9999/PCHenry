using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using TableConversionUtility.Data.Models;

namespace TableConversionUtility.ViewModels
{
	public class EmployeeViewModel
	{
		private List<Employee> repository = new List<Employee>();

		public ObservableCollection<Employee> Employees { get; set; }

		public Employee CurrentEmployee { get; set; }

		public EmployeeViewModel()
		{
			Employees = new ObservableCollection<Employee>();
			repository = new List<Employee>();

			if(!DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
			{
				repository.Add(new Employee { FirstName = "Martin", Department = "Development", Age = 27 });
				repository.Add(new Employee { FirstName = "Maria", Department = "Marketing", Age = 26 });
				repository.Add(new Employee { FirstName = "Katerina", Department = "Sales", Age = 26 });
				repository.Add(new Employee { FirstName = "Joan", Department = "Development", Age = 29 });
				repository.Add(new Employee { FirstName = "Andrew", Department = "Support", Age = 26 });
				repository.Add(new Employee { FirstName = "Pierre", Department = "Development", Age = 28 });

				foreach(var employee in repository)
				{
					Employees.Add(employee);
				}

				CurrentEmployee = Employees.First();
			}
		}
	}
}
