using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableConversionUtility.Data.Models;

namespace TableConversionUtility.Behaviors
{
	public static class ImportDataConverter
	{
		public static Employee Convert(List<string> headings, List<string> importData)
		{
			Employee employee = new Employee();

			employee.FirstName = importData[GetColumnIndex(headings, "FirstName")];
			employee.Department = importData[GetColumnIndex(headings, "Department")];
			employee.Age = importData[GetColumnIndex(headings, "Age")];

			return employee;
		}

		private static int GetColumnIndex(List<string> headings, string columnHeadingToFind)
		{
			int colIndex = -1;

			if(headings.Contains(columnHeadingToFind))
			{
				colIndex = headings.IndexOf(columnHeadingToFind);
			}

			return colIndex;
		}
	}
}

