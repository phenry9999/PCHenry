using System.Windows;
using Microsoft.VisualBasic;

namespace CsMvvmToolkit_CP
{
    public interface IMsgBoxService
    {
        MessageBoxResult Show(string msgBoxText, string title = Constants.vbNullString, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage img = MessageBoxImage.Information, MessageBoxResult defaultReturn = MessageBoxResult.None);
    }
}