using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TableConversionUtility.Data.Models;
using TableConversionUtility.Views;

namespace TableConversionUtility.Utilities
{
	public class SerializationUtility
	{
		public static void SerializeDataToXmlFile(ObservableCollection<EmployeeItemViewModel> employees)
		{
			var fullPath = Assembly.GetExecutingAssembly().Location;
			var folderPath = Path.GetDirectoryName(fullPath);
			var xmlFilePath = Path.Combine(folderPath, "EmployeeConversion" + ".xml");

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.ConformanceLevel = ConformanceLevel.Auto;
			settings.Indent = true;

			using(XmlWriter writer = XmlWriter.Create(xmlFilePath, settings))
			{
				writer.WriteStartElement("employees");
				foreach(var emp in employees)
				{
					writer.WriteStartElement("employee");
					writer.WriteElementString("FirstName", emp.FirstName);
					writer.WriteElementString("Department", emp.Department);
					writer.WriteElementString("Age", emp.Age);
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				writer.Flush();
			}
		}
	}
}
