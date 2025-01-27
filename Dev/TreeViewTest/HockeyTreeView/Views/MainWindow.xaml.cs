using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HockeyTreeView.Models;
using HockeyTreeView.ViewModels;

namespace HockeyTreeView.Views;

public partial class MainWindow : Window
{
	public MainWindow(MainViewModel viewModel)
	{
		InitializeComponent();
		DataContext = viewModel;
	}

	private void LeagueTreeView_Loaded(object sender, RoutedEventArgs e)
	{
		ExpandAllItems(LeagueTreeView);
	}

	private void ExpandAllItems(ItemsControl parent)
	{
		foreach (var item in parent.Items)
		{
			var tvi = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
			if (tvi != null)
			{
				tvi.IsExpanded = true;
				ExpandAllItems(tvi);
			}
		}
	}

	private void LeagueTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		if (DataContext is MainViewModel viewModel)
		{
			viewModel.SelectedTeam = e.NewValue as Team;
		}
	}
}