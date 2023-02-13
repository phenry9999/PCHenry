
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TableConversionUtility.Data.Models
{
	public class DataImportParameters
	{
		const int RowIndex = 0;
		const int ColumnIndex = 1;
		const string rowAndColumnCountsPattern = "(?<RowCount>\\d)\\s(?<ColumnCount>\\d)\\s(?<ImportData>.*)";
		const string importDataPattern = "";

		public int RowCount { get; set; }
		public int ColumnCount { get; set; }

		public string ImportData { get; set; }

		public List<string> Headings { get; set; }

		public List<string> RowData { get; set; }

		public DataImportParameters(string parameterLine)
		{
			ArgumentNullException.ThrowIfNull(parameterLine);

			var regEx = new Regex(rowAndColumnCountsPattern);
			MatchCollection matches = regEx.Matches(parameterLine);

			RowCount = GetCount("RowCount", matches);
			ColumnCount = GetCount("ColumnCount", matches);

			ImportData = GetData("ImportData", matches);


		}

		private string? GetData(string columnName, MatchCollection matches)
		{
			var data = matches.First().Groups[columnName].Value;
			return data;
		}

		private int GetCount(string columnName, MatchCollection matches)
		{
			ArgumentNullException.ThrowIfNull(columnName);

			var countString = matches.First().Groups[columnName].Value;
			var count = Int32.Parse(countString);
			return count;
		}
	}
}
