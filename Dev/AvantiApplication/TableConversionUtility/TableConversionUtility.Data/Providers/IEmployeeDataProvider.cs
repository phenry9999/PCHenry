using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TableConversionUtility.Data.Models;

namespace TableConversionUtility.Data.Providers
{
	public interface IEmployeeDataProvider
	{
		Task<IEnumerable<Employee>?> GetAllAsync();
	}

	public class EmployeeDataProvider : IEmployeeDataProvider
	{
		public async Task<IEnumerable<Employee>?> GetAllAsync()
		{
			return new List<Employee>{
				new Employee{ FirstName="Peter", Department="Development", Age=50 },
				new Employee{ FirstName="Amir", Department="Development", Age=50 },
				new Employee{ FirstName="Omid", Department="Development", Age=50 },
				new Employee{ FirstName="Zeheva", Department="Management", Age=50 },
				new Employee{ FirstName="Brent", Department="Sales", Age=65 },
				new Employee{ FirstName="Andrew", Department="Marketing", Age=49 },
				new Employee{ FirstName="Maria", Department="Sales", Age=51 },
				new Employee{ FirstName="Katerina", Department="Sales", Age=48 },
				new Employee{ FirstName="Dan", Department="Support", Age=51 },
			};
		}
	}
}
