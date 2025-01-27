using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace FirstPrinciples.ViewModels;
public class TeamViewModel : ViewModelBase
{
	private int _id;

	[Required(ErrorMessage = "ID must be valid integer above 0.")]
	[Key]
	public int Id
	{
		get { return _id; }
		set
		{
			SetProperty(ref _id, value);
		}
	}

	private string teamName;

	[Required(ErrorMessage = "Team name must have a valid name.")]
	public string TeamName
	{
		get { return teamName; }
		set
		{
			if (SetProperty(ref teamName, value))
			{
				OnPropertyChanged(nameof(DisplayMember));
			}
		}
	}

	private string _arenaName;

	public string ArenaName
	{
		get { return _arenaName; }
		set
		{
			ValidateArenaName(value, this);
			SetProperty(ref _arenaName, value);
		}
	}

	private int _inauguralYear;
	[Required]
	[Range(1900, 2024, ErrorMessage = "Year must be in the 21st century")]
	public int InauguralYear
	{
		get { return _inauguralYear; }
		set
		{
			if (SetProperty(ref _inauguralYear, value))
			{
				OnPropertyChanged(nameof(DisplayMember));
			}
		}
	}

	private string _nickname;
	[MinLength(3)]
	public string Nickname
	{
		get { return _nickname; }
		set
		{
			if (SetProperty(ref _nickname, value))
			{
				OnPropertyChanged(nameof(DisplayMember));
			}
		}
	}

	private string _mascot;

	[MaxLength(20)]
	[MinLength(2)]
	public string Mascot
	{
		get { return _mascot; }
		set
		{
			SetProperty(ref _mascot, value);
		}
	}

	private string _websiteUrl;
	[Required(ErrorMessage = "Must have a website.")]
	[Url(ErrorMessage = "Must have a valid URL for the website.")]
	public string WebsiteUrl
	{
		get { return _websiteUrl; }
		set
		{
			SetProperty(ref _websiteUrl, value);
		}
	}

	private string _logoUrl;
	[Url(ErrorMessage = "Must have a valid URL for the logo.")]
	public string LogoUrl
	{
		get { return _logoUrl; }
		set
		{
			SetProperty(ref _logoUrl, value);
		}
	}

	public string DisplayMember
	{
		get
		{
			StringBuilder displayMember = new StringBuilder(teamName);

			if (!string.IsNullOrWhiteSpace(Nickname))
			{
				displayMember.Append($" ({Nickname})");
			}

			return displayMember.ToString();
		}
	}

	protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		base.OnPropertyChanged(propertyName);

		if (propertyName != nameof(HasErrors))
		{
			Validate();
		}
	}

	public static void ValidateArenaName(string value, ICollectErrors errors)
	{
		errors.ClearErrors(nameof(ArenaName));

		if (string.Equals(value, "Vanier", StringComparison.OrdinalIgnoreCase))
		{
			errors.AddError(nameof(ArenaName), "Oh no bro, not in that 'hood?!");
		}
	}

	protected virtual void Validate()
	{
		ValidateArenaName(_arenaName, this);

		//Validator.TryValidateObject(this, new ValidationContext(this), Errors, true);

		OnErrorsChanged(nameof(Id));
		OnErrorsChanged(nameof(TeamName));
		OnErrorsChanged(nameof(Nickname));
		OnErrorsChanged(nameof(ArenaName));
		OnErrorsChanged(nameof(Mascot));
		OnErrorsChanged(nameof(WebsiteUrl));
		OnErrorsChanged(nameof(LogoUrl));
		OnErrorsChanged(nameof(InauguralYear));
		OnErrorsChanged(nameof(WebsiteUrl));
	}
}
