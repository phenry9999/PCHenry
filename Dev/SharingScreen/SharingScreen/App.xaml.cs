using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Forms = System.Windows.Forms;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace SharingScreen
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			NotifyIcon icon = new();

			icon.Icon = ProjectResources.IconLogo;
			icon.Visible = true;
			icon.Click += new EventHandler(TrayIconOnClick);

			base.OnStartup(e);

			void TrayIconOnClick(object? sender, EventArgs e) => MessageBox.Show("Tray icon clicked!");
		}
	}
}
