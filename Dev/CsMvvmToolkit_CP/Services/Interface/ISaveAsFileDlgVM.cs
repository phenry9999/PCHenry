
namespace CsMvvmToolkit_CP
{
    public interface ISaveAsFileDlgVM
    {
        // Function SaveAsFileDlg(ByVal DialogName As Window) As Boolean
        bool SaveAsFileDlg(string sText, object ActiveTBox);
    }
}