using System;
using System.Collections.Generic;
using System.Linq;
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

			if(importer.IsValidParameterLine)
			{ }
			else


				return null;
		}
	}
}
