
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LoginValidation.Validations;

public class TelephoneValidationAttribute : ValidationAttribute
{

	//write method using regex to validate telephone number
	//return true if valid, false if not
	//use the following regex pattern: @"^\d{3}-\d{3}-\d{4}$"
	//use the following error message: "Telephone number must be in the format of ###-###-####"

	public override bool IsValid(object value)
	{
		if (value == null)
		{
			return false;
		}

		string pattern = @"^\d{3}-\d{3}-\d{4}$";
		Regex regex = new Regex(pattern);
		return regex.IsMatch(value.ToString());
	}
}