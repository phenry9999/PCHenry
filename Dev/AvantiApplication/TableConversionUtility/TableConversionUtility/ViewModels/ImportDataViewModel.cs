using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableConversionUtility.ViewModels
{
	public class ImportDataViewModel : ViewModelBase
	{
		public ImportDataViewModel()
		{
			DataToImport = "2 3 &quot;First Name&quot; Department Age Mary &quot;Human Resources&quot; 25 George Development 36";
		}

		private string? importData;

		public string? DataToImport
		{
			get { return importData; }
			set
			{
				importData = value;
				RaisePropertyChanged();
			}
		}
	}
}
