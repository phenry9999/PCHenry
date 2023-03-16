
using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace CsMvvmToolkit_CP
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private bool blnReady;

        public App()
        {
            InitializeComponent();
            Exit += (_, __) => OnClosing();
            Startup += Application_Startup;

            try
            {
                Mod_Public.sAppPath = Directory.GetCurrentDirectory();
                Ioc.Default.ConfigureServices(
                    new ServiceCollection()
                    .AddSingleton<IMsgBoxService, MsgBoxService>()
                    .AddSingleton((IDialog)new DialogVM())
                    .AddSingleton((IOpenFileDlgVM)new OpenFileDlgVM())
                    .AddSingleton((ISaveAsFileDlgVM)new SaveAsFileDlgVM())
                    .AddSingleton((IRichTextDlgVM)new RichTextDlgVM())
                    .BuildServiceProvider());
            }
            catch (Exception ex)
            {
                File.AppendAllText(Mod_Public.sAppPath + @"\Log.txt", string.Format("{0}{1}", Environment.NewLine, DateAndTime.Now.ToString() + "; " + ex.ToString()));
                var msgBoxService = Ioc.Default.GetService<IMsgBoxService>();
                msgBoxService.Show("Unexpected error:" + Constants.vbNewLine + Constants.vbNewLine + ex.ToString(), img: MessageBoxImage.Error);

            }
        }

        private void OnClosing()
        {
        }

        private void Application_Startup(object sender, EventArgs e)
        {
            blnReady = true;
        }
    }
}
