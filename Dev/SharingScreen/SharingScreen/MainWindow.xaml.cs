using System.Windows;

using Forms = System.Windows.Forms;

namespace SharingScreen
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			Forms.NotifyIcon notifyIcon = new Forms.NotifyIcon();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("You clicked me. You love me, you really love me!");
		}
	}
}
