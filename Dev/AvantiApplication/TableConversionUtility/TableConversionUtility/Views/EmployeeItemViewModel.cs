using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableConversionUtility.Data.Models;
using TableConversionUtility.ViewModels;

namespace TableConversionUtility.Views
{
	/// <summary>
	/// Using this as intermediary between Employee model and what is displayed
	/// </summary>
	public class EmployeeItemViewModel : ViewModelBase
	{
		private Employee model;

		public EmployeeItemViewModel(Employee model)
		{
			this.model = model;
		}

		public string FirstName
		{
			get => model.FirstName;

			set
			{
				model.FirstName = value;
				RaisePropertyChanged();
			}
		}
		public string? Department
		{
			get => model.Department;

			set
			{
				model.Department = value;
				RaisePropertyChanged();
			}
		}

		public string? Age
		{
			get => model.Age;

			set
			{
				model.Age = value;
				RaisePropertyChanged();
			}
		}
	}
}
