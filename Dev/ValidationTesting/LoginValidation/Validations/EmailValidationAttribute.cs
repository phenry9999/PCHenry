using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LoginValidation.Validations;

public class EmailValidationAttribute : ValidationAttribute
{
	protected override ValidationResult IsValid(object value, ValidationContext validationContext)
	{
		if (value == null || string.IsNullOrEmpty(value.ToString()))
		{
			return ValidationResult.Success;
		}

		string email = value.ToString();

		const string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
		if (!Regex.IsMatch(email, pattern))
		{
			return new ValidationResult("Email is not valid");
		}

		return ValidationResult.Success;
	}
}