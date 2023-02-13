using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TableConversionUtility.Data.Models;
using TableConversionUtility.Views;

namespace TableConversionUtility.Behaviors
{
	public class ConversionDataParser
	{
		public static List<Employee> Parse(string parameterLine)
		{
			DataImportParameters importer = new DataImportParameters(parameterLine);

			List<Employee> employees = new List<Employee>();

			if(importer.IsValidParameterLine && ConversionDataParser.IsHeadingsConsistentAndValidForEmployee(importer.Headings))
			{
				for(int i = 0; i < importer.ContentData.Count; i++)
				{
					var newEmployee = ImportDataConverter.Convert(importer.Headings, importer.ContentData[i]);
					employees.Add(newEmployee);
				}
			}

			return employees;
		}

		private static bool IsHeadingsConsistentAndValidForEmployee(List<string> headings)
		{
			bool isValid = (headings.Count == 3 &&
				headings.Contains("FirstName") && headings.Contains("Department") && headings.Contains("Age"));     //I could make this more generic and dynamic, but I'm tired and it's late
			return isValid;
		}
	}
}
