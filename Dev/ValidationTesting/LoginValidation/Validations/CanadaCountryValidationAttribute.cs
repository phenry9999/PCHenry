
using System.ComponentModel.DataAnnotations;

namespace LoginValidation.Validations;

public class CanadaCountryValidationAttribute : ValidationAttribute
{
	public override bool IsValid(object value)
	{
		if (value == null)
		{
			return false;
		}

		string[] validCountries = new string[]
		{
			"Canada", "CA"
		};

		return validCountries.Contains(value.ToString());
	}
}
