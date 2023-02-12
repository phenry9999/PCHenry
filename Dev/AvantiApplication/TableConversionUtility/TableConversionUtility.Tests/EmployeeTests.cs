using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TableConversionUtility.Data.Models;

namespace TableConversionUtility.Tests
{
	[TestClass]
	public class EmployeeTests
	{
		[TestMethod]
		public void DefaultEmployeeCreate()
		{
			Employee employee = new Employee();

			Assert.IsNotNull(employee);
			Assert.IsNotNull(employee.FirstName);
			Assert.IsNull(employee.Department);
			Assert.AreEqual(employee.Age, 0);
		}

		[TestMethod]
		public void CreateSpecificEmployeeIsCorrectlyRead()
		{
			Employee employee = new Employee { FirstName = "Martin", Department = "Development", Age = 27 };

			Assert.AreEqual(employee.FirstName, "Martin");
			Assert.AreEqual(employee.Department, "Development");
			Assert.AreEqual(employee.Age, 27);
		}

		[TestMethod]
		public void CreateEmptyEmployeeThenDefaultNameIsCorrect()
		{
			Employee employee = new Employee();

			Assert.IsNotNull(employee);
			Assert.IsNotNull(employee.FirstName);
			Assert.AreEqual(employee.FirstName, "DEFAULT");
			Assert.AreNotEqual(employee.FirstName, "Default");
			Assert.AreNotEqual(employee.FirstName, "default");
		}
	}
}

