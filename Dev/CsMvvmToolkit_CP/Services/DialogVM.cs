
using System.Collections.ObjectModel;

namespace CsMvvmToolkit_CP
{
    public class DialogVM : IDialog
    {
        public bool ShowMyDialog(ObservableCollection<Credits> myCredits)
        {
            bool ShowMyDialogRet = default;
            var myDialogWindow = new DialogWindow();
            myDialogWindow.ShowDialog(myCredits);
            ShowMyDialogRet = myDialogWindow.DialogResult == true;
            return ShowMyDialogRet;
        }
    }
}