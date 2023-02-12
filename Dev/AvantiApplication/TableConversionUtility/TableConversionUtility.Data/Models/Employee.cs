using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableConversionUtility.Data.Models
{
	public class Employee
	{
		[Required(ErrorMessage = "This is the PK and is required.")]
		[DisplayName("First Name")]
		public string FirstName { get; set; }

		[StringLength(100, ErrorMessage = "Department name needs to be less than 100 characters to be valid.")]
		public string? Department { get; set; }

		[Range(1, 99, ErrorMessage = "Age range is 1 to 99 years old.")]
		public int Age { get; set; }

		public Employee()
		{
			this.FirstName = "DEFAULT";
		}
	}
}
