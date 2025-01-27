using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoginValidation.Validations;

namespace LoginValidation.ViewModels;

public class ContactViewModel : ViewModelBase
{
	private string _address1;

	[Required]
	public string Address1
	{
		get { return _address1; }
		set
		{
			_address1 = value;
			Validate(nameof(Address1), value);
		}
	}

	private string _address2;

	public string Address2
	{
		get { return _address2; }
		set
		{
			_address2 = value;
			Validate(nameof(Address2), value);
		}
	}

	private string _city;

	[Required]
	public string City
	{
		get { return _city; }
		set
		{
			_city = value;
			Validate(nameof(City), value);
		}
	}

	private string _province;

	[Required]
	[CanadianProvincesAndTerritoriesValidation]
	public string Province
	{
		get { return _province; }
		set
		{
			_province = value;
			Validate(nameof(Province), value);
		}
	}

	private string _postalCode;

	[Required]
	public string PostalCode
	{
		get { return _postalCode; }
		set
		{
			_postalCode = value;
			Validate(nameof(PostalCode), value);
		}
	}

	private string _county;

	[Required]
	[CanadaCountryValidation]
	public string Country
	{
		get { return _county; }
		set { _county = value; Validate(nameof(Country), value); }
	}


	private string _telephone;

	[Required]
	[TelephoneValidationAttribute]
	public string Telephone
	{
		get { return _telephone; }
		set { _telephone = value; Validate(nameof(Telephone), value); }
	}

	public void Validate(string propertyName, object propertyValue)
	{
		ClearErrors();

		var results = new List<ValidationResult>();
		Validator.TryValidateProperty(propertyValue, new ValidationContext(this) { MemberName = propertyName }, results);

		if (results.Any())
		{
			foreach (var result in results)
			{
				AddError(propertyName, result.ErrorMessage);
			}
			OnErrorsChanged(propertyName);
		}
		else
		{
			ClearErrors(propertyName);
		}
	}
}