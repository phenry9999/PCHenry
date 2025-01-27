using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeserializerFramework.Models
{

	public class ReportParameters
	{
		public ReportParameters()
		{
			Parameters = new Dictionary<string, string>();
		}

		public string ConnectionString { get; set; }

		public Dictionary<string, string> Parameters { get; set; }

		public void Add(string name, string value)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			Parameters.Add(name, value);
		}
	}
}