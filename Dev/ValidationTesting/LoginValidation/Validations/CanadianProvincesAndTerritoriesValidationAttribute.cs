
using System.ComponentModel.DataAnnotations;

namespace LoginValidation.Validations;

public class CanadianProvincesAndTerritoriesValidationAttribute : ValidationAttribute
{
	public override bool IsValid(object value)
	{
		if (value == null)
		{
			return false;
		}

		string[] validProvinces = new string[]
		{
			"Alberta", "AB",
			"British Columbia", "BC",
			"Manitoba", "MB",
			"New Brunswick", "NB",
			"Newfoundland and Labrador", "NL",
			"Nova Scotia", "NS",
			"Ontario", "ON",
			"Prince Edward Island", "PE",
			"Quebec", "QC",
			"Saskatchewan", "SK",
			"Northwest Territories", "NT",
			"Nunavut", "NU",
			"Yukon", "YT"
		};

		return validProvinces.Contains(value.ToString());
	}
}
