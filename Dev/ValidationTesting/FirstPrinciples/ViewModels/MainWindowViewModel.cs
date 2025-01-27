using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FirstPrinciples.Commands;

namespace FirstPrinciples.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	public DelegateCommand ExitCommand { get; private set; }
	public DelegateCommand AddCommand { get; private set; }
	public DelegateCommand SaveCommand { get; private set; }
	public DelegateCommand DeleteCommand { get; private set; }
	public DelegateCommand HelpCommand { get; private set; }

	public MainWindowViewModel()
	{
		ExitCommand = new DelegateCommand(ExitApplication);
		AddCommand = new DelegateCommand(AddTeam, CanAddTeam);
		SaveCommand = new DelegateCommand(SaveTeam, CanSave);
		DeleteCommand = new DelegateCommand(DeleteTeam, CanDelete);
		HelpCommand = new DelegateCommand(ShowHelp);

		LoadTeams();
	}

	private void ShowHelp(object? parameter)
	{
		Process.Start(new ProcessStartInfo { FileName = "https://avantisystems.com/support-portal/", UseShellExecute = true });
	}

	private bool CanDelete(object? parameter)
	{
		return SelectedTeam != null;
	}

	private void DeleteTeam(object? parameter)
	{
		if (SelectedTeam != null)
		{
			Teams.Remove(SelectedTeam);
		}
	}

	private bool CanSave(object? parameter)
	{
		return SelectedTeam != null && IsDirty;
	}

	private void SaveTeam(object? parameter)
	{
		if (SelectedTeam.Id == 0)
		{
			SelectedTeam.Id = Teams.Max(x => x.Id) + 1;
		}

		if (Teams.Count(x => x.Id == SelectedTeam.Id) == 0)
		{
			Teams.Add(selectedTeam);
		}
	}
	private bool CanAddTeam(object? parameter)
	{
		return !IsDirty;
	}

	private void AddTeam(object? parameter)
	{
		var newTeam = new TeamViewModel() { TeamName = "Default_Name", InauguralYear = DateTime.Now.Year, WebsiteUrl = "https://www.nhl.com/", LogoUrl = "https://www.nhl.com/assets/icons/fav/nhl/favicon.ico" };
		newTeam.Id = 0;
		SelectedTeam = newTeam;
	}

	private void ExitApplication(object? parameter)
	{
		Application.Current.Shutdown();
	}

	private ObservableCollection<TeamViewModel> teams = new ObservableCollection<TeamViewModel>();

	public ObservableCollection<TeamViewModel> Teams
	{
		get { return teams; }
		set
		{
			teams = value;
			OnPropertyChanged();
		}
	}

	private TeamViewModel selectedTeam;
	public TeamViewModel SelectedTeam
	{
		get { return selectedTeam; }
		set
		{
			selectedTeam = value;
			//Team_PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedTeam)));
			OnPropertyChanged();
		}
	}

	private bool isDirty;

	public bool IsDirty
	{
		get { return isDirty; }
		set
		{
			isDirty = value;
			OnPropertyChanged();
		}
	}

	public bool IsTeamSelected
	{
		get
		{
			return SelectedTeam != null;
		}
	}

	protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		base.OnPropertyChanged(propertyName);
		NotifyAllPropertyChanged();
	}

	//private void Team_PropertyChanged(object sender, PropertyChangedEventArgs e)
	//{
	//	isDirty = true;

	//	OnSelectedTeamChanging(null, selectedTeam);
	//	OnSelectedTeamChanged(selectedTeam);
	//}

	//public void OnSelectedTeamChanging(TeamViewModel? oldValue, TeamViewModel? newValue)
	//{
	//	IsDirty = false;

	//	if (oldValue != null)
	//	{
	//		oldValue.PropertyChanged -= Team_PropertyChanged;
	//	}
	//}

	//public void OnSelectedTeamChanged(TeamViewModel team)
	//{
	//	if (team != null)
	//	{
	//		team.PropertyChanged += Team_PropertyChanged;
	//		SelectedTeam = team;
	//	}

	//	NotifyAllPropertyChanged();

	//	OnPropertyChanged(nameof(IsDirty));
	//	OnPropertyChanged(nameof(IsTeamSelected));
	//}

	public void NotifyAllPropertyChanged()
	{
		AddCommand.RaiseCanExecuteChanged();
		SaveCommand.RaiseCanExecuteChanged();
		DeleteCommand.RaiseCanExecuteChanged();
	}

	public async Task LoadAsync()
	{

		await LoadTeams();
	}

	public async Task LoadTeams()
	{
		Teams.Clear();

		Teams.Add(new TeamViewModel
		{
			Id = 7,
			TeamName = "Seattle Kraken",
			ArenaName = "Climate Pledge Arena",
			InauguralYear = 2021,
			Nickname = "Kraken",
			Mascot = "Kraken",
			WebsiteUrl = "https://www.nhl.com/kraken",
			LogoUrl = "https://www.nhl.com/assets/icons/fav/teams/55/favicon.ico"
		});

		Teams.Add(new TeamViewModel
		{
			Id = 5,
			TeamName = "Vancouver Canucks",
			ArenaName = "Rogers Arena",
			InauguralYear = 1970,
			Nickname = "Canucks",
			Mascot = "Fin the Whale",
			WebsiteUrl = "https://www.nhl.com/canucks",
			LogoUrl = "https://www.nhl.com/assets/icons/fav/teams/23/favicon.ico"
		});

		Teams.Add(new TeamViewModel
		{
			Id = 3,
			TeamName = "Calgary Flames",
			ArenaName = "Scotiabank Saddledome",
			InauguralYear = 1972,
			Nickname = "Flames",
			Mascot = "Harvey the Hound",
			WebsiteUrl = "https://www.nhl.com/flames",
			LogoUrl = "https://www.nhl.com/assets/icons/fav/teams/20/favicon.ico"
		});

		Teams.Add(new TeamViewModel
		{
			Id = 2,
			TeamName = "Edmonton Oilers",
			ArenaName = "Rogers Place",
			InauguralYear = 1972,
			Nickname = "Oilers",
			Mascot = "Hunter",
			WebsiteUrl = "https://www.nhl.com/oilers",
			LogoUrl = "https://www.nhl.com/assets/icons/fav/teams/22/favicon.ico"
		});

		Teams.Add(new TeamViewModel
		{
			Id = 6,
			TeamName = "Winnipeg Jets",
			ArenaName = "Canada Life Centre",
			InauguralYear = 2011,
			Nickname = "Jets",
			Mascot = "Mick E. Moose",
			WebsiteUrl = "https://www.nhl.com/jets",
			LogoUrl = "https://www.nhl.com/assets/icons/fav/teams/52/favicon.ico"
		});

		Teams.Add(new TeamViewModel
		{
			Id = 4,
			TeamName = "Toronto Maple Leafs",
			ArenaName = "Scotiabank Arena",
			InauguralYear = 1917,
			Nickname = "Maple Leafs",
			Mascot = "Carlton the Bear",
			WebsiteUrl = "https://www.nhl.com/mapleleafs",
			LogoUrl = "https://www.nhl.com/assets/icons/fav/teams/10/favicon.ico"
		});

		Teams.Add(new TeamViewModel
		{
			Id = 8,
			TeamName = "Ottawa Senators",
			ArenaName = "Canadian Tire Centre",
			InauguralYear = 1992,
			Nickname = "Senators",
			Mascot = "Spartacat",
			WebsiteUrl = "https://www.nhl.com/senators",
			LogoUrl = "https://www.nhl.com/assets/icons/fav/teams/9/favicon.ico"
		});

		Teams.Add(new TeamViewModel
		{
			Id = 1,
			TeamName = "Montreal Canadiens",
			ArenaName = "Bell Centre",
			InauguralYear = 1909,
			Nickname = "Canadiens",
			Mascot = "Youppi!",
			WebsiteUrl = "https://www.nhl.com/canadiens",
			LogoUrl = "https://www.nhl.com/assets/icons/fav/teams/8/favicon.ico"
		});
	}
}
