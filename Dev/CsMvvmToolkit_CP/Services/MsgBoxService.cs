using System.Windows;

namespace CsMvvmToolkit_CP
{
    internal class MsgBoxService : IMsgBoxService
    {
        MessageBoxResult IMsgBoxService.Show(string msgBoxText, string title, MessageBoxButton button, MessageBoxImage img, MessageBoxResult defaultReturn)
        {
            return MessageBox.Show(msgBoxText, title, button, img);
        }
    }
}