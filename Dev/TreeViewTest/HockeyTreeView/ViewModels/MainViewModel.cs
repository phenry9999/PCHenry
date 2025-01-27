using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HockeyTreeView.Commands;
using HockeyTreeView.Models;
using HockeyTreeView.Providers;

namespace HockeyTreeView.ViewModels;
public class MainViewModel : ViewModelBase
{
	public readonly ILeagueDataProvider _dataProvider;

	private ObservableCollection<League> leagues;
	public ObservableCollection<League> Leagues
	{
		get => leagues;
		set
		{
			leagues = value;
			RaisePropertyChanged();
		}
	}

	private Team selectedTeam;
	public Team SelectedTeam
	{
		get => selectedTeam;
		set
		{
			selectedTeam = value;
			RaisePropertyChanged();
		}
	}

	public ICommand CloseCommand { get; internal set; }

	public MainViewModel(ILeagueDataProvider dataProvider)
	{
		CloseCommand = new DelegateCommand(OnClose);

		_dataProvider = dataProvider;
		Leagues = new ObservableCollection<League>();
		LoadData();
	}

	private async void LoadData()
	{
		var league = await _dataProvider.GetAllAsync();
		Leagues.Add(league);
	}

	private void OnClose(object parameter)
	{
		Application.Current.Shutdown();
	}
}
