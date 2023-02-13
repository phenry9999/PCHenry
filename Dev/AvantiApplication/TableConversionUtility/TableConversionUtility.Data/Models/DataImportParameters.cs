
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
		/// <summary>
		/// using reget testing site = https://regex101.com/r/tm4iXp/1
		/// input pattern = 
		/*
				^((?<Word>\w+)\s+(?<RestOfLine>.*))|(("")(?<Word>\w+\s*\w*)("")(\s*)(?<RestOfLine>.*))|((')(?<Word>\w+\s*\w*)(')(\s*)(?<RestOfLine>.*))$
		*/
		/// testing data
		/// 
		/*
FirstName Department Age Mary HumanResources 25 George Development 36
FirstName    Department    Age    Mary    HumanResources    25    George    Development    36
FirstName Department Age Mary "Human Resources" 25 George Development 36
FirstName Department Age Mary HumanResources 25 George Development 36
FirstName Department Age Mary 'Human Resources' 25 George Development 36
FirstName Department Age Mary 'Human Resources' 25 George Development 36 
FirstName Department Age Mary 'Human Resources' 25 George Development 36    
1234 Department Age Mary HumanResources 25 George Development 36

"FirstName" Department Age Mary "HumanResources" 25 George Development 36
"First Name" Department Age Mary "Human Resources" 25 George Development 36
"First    Name" Department Age Mary "Human    Resources" 25 George Development 36
"First Name" Department Age Mary HumanResources 25 George Development 36
"First Name"    Department    Age    Mary    HumanResources    25    George    Development    36
"First Name"    Department    Age    Mary    HumanResources    25    George    Development    36 
"First Name"    Department    Age    Mary    HumanResources    25    George    Development    36    
"1234" Department Age Mary HumanResources 25 George Development 36
"1234"... Department ...Age... Mary ...HumanResources... 25 ...George ...Development ...36

'First Name' Department Age Mary 'Human Resources' 25 George Development 36
'First    Name' Department Age Mary... HumanResources 25 George Development 36
'First Name' Department Age Mary HumanResources 25 George Development 36
'First Name' Department Age Mary HumanResources 25 George Development 36 
'First Name' Department Age Mary HumanResources 25 George Development 36    
'First Name'    Department    Age    Mary    HumanResources    25    George    Development    36
'1234' Department Age Mary HumanResources 25 George Development 36

		*/
		/// </summary>

		const int RowIndex = 0;
		const int ColumnIndex = 1;
		const string rowAndColumnCountsPattern = "(?<RowCount>\\d)\\s(?<ColumnCount>\\d)\\s(?<ImportData>.*)";
		const string importDataSchemaPattern = "";
		const string importDataPattern = @"^((?<Word>\w+\s+)(?<RestOfLine>.*))|((?<Word>("")\w+\s*\w*("")(\s*))(?<RestOfLine>.*))|((?<Word>(')\w+\s*\w*(')(\s*))(?<RestOfLine>.*))$";

		public bool IsValidParameterLine = false;

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

			if(RowCount == 0 && ColumnCount == 0)
			{
				return;
			}
			else
			{
				Headings = new List<string>(ColumnCount);
				RowData = new List<string>(ColumnCount);
			}

			ImportData = GetData("ImportData", matches);
			ParseImportData(Headings, RowData, ImportData);
		}

		private void ParseImportData(List<string>? headings, List<string>? rowData, string? importData)
		{
			if(string.IsNullOrEmpty(importData))
				return;

			importData = importData.Replace("&quot;", "\"");

			try
			{
				for(int i = 0; i < RowCount; i++)
				{
					for(int j = 0; j < ColumnCount; j++)
					{

						//loop through each parameter, I know this is not the most efficient way,
						//but it's late, I'm tired, and I want to prove I can do this :> and I know this will work today
						var regEx = new Regex(importDataPattern);
						var matches = regEx.Matches(importData);

						var nextQuotedWord = matches.First().Groups["Word"].Value;
						var nextWord = nextQuotedWord.Replace("\"", "").Trim();

						if(i == 0)
						{
							Headings.Add(nextWord);
						}
						else
						{
							RowData.Add(nextWord);
						}

						importData = importData.Substring(nextQuotedWord.Length);       //need to strip off the quoted word
					}
				}
			}
			catch(Exception ex)
			{
				//at this point, if anything fails...sorry, the import data is wrong\bad\corrupt\drunk
				IsValidParameterLine = false;   //it's already false, but just making it explicit to anyone reading
			}

			IsValidParameterLine = true;        //if you reach here, you've got a good import
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
