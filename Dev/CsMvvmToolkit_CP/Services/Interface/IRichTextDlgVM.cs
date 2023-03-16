
namespace CsMvvmToolkit_CP
{
    public interface IRichTextDlgVM
    {
        bool SearchDlg(object ActiveTBox);
        
        bool PrintDlg(object ActiveTBox);
        
        bool PasteImageDlg(object ActiveTBox);

        bool FontsDlg(object ActiveTBox);

        bool ColorDlg(object ActiveTBox);
    }
}