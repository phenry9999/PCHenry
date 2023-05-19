using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.Model.Themes;

namespace TestingBench.Desktop;

[ObservableObject]
public partial class MainWindowViewModel
{
    public MainWindowViewModel()
    {

    }

    [ObservableProperty]
    string _themeName = "Default";

    [RelayCommand]
    private void ChangeTheme(object theme)
    {
        ThemeName = theme.ToString();

        var themePath = @"C:\Program Files (x86)\Progress\Telerik UI for WPF R1 2023\Themes.Implicit\WPF\{0}\Themes\System.Windows.xaml";

        Application.Current.Resources.Source = new Uri(string.Format(themePath, ThemeName), UriKind.RelativeOrAbsolute);
    }
}
