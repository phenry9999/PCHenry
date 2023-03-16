
using System.Collections.ObjectModel;

namespace CsMvvmToolkit_CP
{
    public interface IDialog
    {
        bool ShowMyDialog(ObservableCollection<Credits> myCredits);
        //bool ShowMyDialog();
    }
}