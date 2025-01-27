using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LoginValidation.Commands;
using LoginValidation.Validations;

namespace LoginValidation.ViewModels;
public class MainWindowViewModel : ViewModelBase
{
	public DelegateCommand ExitCommand { get; private set; }
	public DelegateCommand SupportCommand { get; private set; }
	public DelegateCommand AboutCommand { get; private set; }
	public DelegateCommand SubmitCommand { get; private set; }
	public DelegateCommand AddContactInfoCommand { get; private set; }
	public DelegateCommand CancelContactInfoCommand { get; private set; }

	private string username;
	private string password;
	private string email;

	public MainWindowViewModel()
	{
		ExitCommand = new DelegateCommand(ExitApplication);
		SupportCommand = new DelegateCommand(ShowSupport);
		AboutCommand = new DelegateCommand(AboutApplication);
		SubmitCommand = new DelegateCommand(SubmitLogin, CanSubmit);
		AddContactInfoCommand = new DelegateCommand(AddNewContactInfo, CanAddNewContactInfo);
		CancelContactInfoCommand = new DelegateCommand(CancelNewContactInfo, CanCancelNewContactInfo);
	}

	[Required(ErrorMessage = "Username is required")]
	public string Username
	{
		get => username;
		set
		{
			username = value;
			Validate(nameof(Username), value);
		}
	}

	[Required]
	[MinLength(6, ErrorMessage = "Password must be 6 or more charactgers long and cannot be password.")]
	public string Password
	{
		get => password;
		set
		{
			password = value;

			if (password.ToUpper() == "password".ToUpper())
			{
				ClearErrors(nameof(Password));
				AddError(nameof(Password), "Password cannot be 'password'");
				AddError(nameof(Password), "Are you a noob?");
				AddError(nameof(Password), "Are you a Leafs fan?");
				AddError(nameof(Password), "Are you a Mac user?");
				OnErrorsChanged(nameof(Password));
				SubmitCommand.RaiseCanExecuteChanged();
			}
			else
			{
				Validate(nameof(Password), value);
			}
		}
	}

	[EmailValidation]
	public string Email
	{
		get => email;
		set
		{
			email = value;
			Validate(nameof(Email), value);
		}
	}

	private ContactViewModel _contactInfo;

	[Required]
	public ContactViewModel ContactInfo
	{
		get { return _contactInfo; }
		set
		{
			_contactInfo = value;
			OnPropertyChanged(nameof(ContactInfo));
			Validate(nameof(ContactInfo), value);
		}
	}

	private void ExitApplication(object? parameter)
	{
		Application.Current.Shutdown();
	}

	private void ShowSupport(object? parameter)
	{
		Process.Start(new ProcessStartInfo { FileName = "https://avantisystems.com/support-portal/", UseShellExecute = true });
	}

	private void AboutApplication(object? parameter)
	{
		Process.Start(new ProcessStartInfo { FileName = "https://avantisystems.com/", UseShellExecute = true });
	}

	private bool CanSubmit(object? parameter)
	{
		//bool allBlanks = string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Password);

		//var results = new List<ValidationResult>();
		//bool isValidViewModel = Validator.TryValidateObject(this, new ValidationContext(this), results);

		//if (results.Any())
		//{
		//	foreach (var result in results)
		//	{
		//		AddError(result.MemberNames.First(), result.ErrorMessage);
		//	}
		//}

		//return isValidViewModel && !results.Any() && !allBlanks;

		//bool isValid = Validator.TryValidateObject(this, new ValidationContext(this), null);

		var results = new List<ValidationResult>();
		bool isValid = Validator.TryValidateObject(this, new ValidationContext(this), results, true);

		return isValid;
	}

	private void SubmitLogin(object? parameter)
	{
		string message = $"Username: {Username}\nEmail: {Email}\nPassword: {Password}";
		MessageBox.Show($"Login submitted with {Environment.NewLine}{message}", "Login Validation", MessageBoxButton.OK, MessageBoxImage.Information);
	}

	private void AddNewContactInfo(object? obj)
	{
		this.ContactInfo = new ContactViewModel() { Address1 = "125 Rue de Carillon", Address2 = "Att Hull Olympiques", City = "Hull", Province = "QC", PostalCode = "J8X 2P8", Country = "Canada", Telephone = "819-777-0661" };
		AddContactInfoCommand.RaiseCanExecuteChanged();
		CancelContactInfoCommand.RaiseCanExecuteChanged();
		SubmitCommand.RaiseCanExecuteChanged();
	}

	private bool CanAddNewContactInfo(object? arg)
	{
		return this.ContactInfo == null;
	}

	private void CancelNewContactInfo(object? obj)
	{
		var result = MessageBox.Show("Do you really want to remove Contact Information?", "Delete Contact Information", MessageBoxButton.YesNo, MessageBoxImage.Question);

		if (result == MessageBoxResult.Yes)
		{
			this.ContactInfo = null;
			AddContactInfoCommand.RaiseCanExecuteChanged();
			CancelContactInfoCommand.RaiseCanExecuteChanged();
			SubmitCommand.RaiseCanExecuteChanged();
		}
	}

	private bool CanCancelNewContactInfo(object? arg)
	{
		return this.ContactInfo != null;
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
		//else
		//{
		//	ClearErrors(propertyName);
		//}

		SubmitCommand.RaiseCanExecuteChanged();
	}
}
