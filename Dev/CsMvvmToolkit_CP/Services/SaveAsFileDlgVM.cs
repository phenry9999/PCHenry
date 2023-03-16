using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace CsMvvmToolkit_CP
{
    public class SaveAsFileDlgVM : ISaveAsFileDlgVM
    {
        private RichTextBox myRTB;
        private TextBox myTB;

        public bool SaveAsFileDlg(string sText, object ActiveTBox)
        {

        // https://www.codeproject.com/Articles/15585/Building-a-Simple-Word-Processor-Around-an-Extende

        FileDialog dialog = new SaveFileDialog();
            dialog.Filter = "All Files(*.*)|*.*|RTF Files (*.rtf)|*.rtf";
            dialog.FilterIndex = 1;
            try
            {
                dialog.ShowDialog();
                if (string.IsNullOrEmpty(dialog.FileName))
                    return default;
                string strExt;
                strExt = System.IO.Path.GetExtension(dialog.FileName);
                strExt = strExt.ToUpper();
                if (ActiveTBox.GetType().ToString() == "System.Windows.Controls.RichTextBox") // IsNot Nothing Then
                {
                    myRTB = (RichTextBox)ActiveTBox;
                    switch (strExt ?? "")
                    {
                        case ".RTF":
                            {
                                var t = new TextRange((TextPointer)myRTB.Document.ContentStart, (TextPointer)myRTB.Document.ContentEnd);
                                var file = new FileStream(dialog.FileName, FileMode.Create);
                                t.Save(file, DataFormats.Rtf);
                                file.Close();
                                break;
                            }

                        default:
                            {
                                var t = new TextRange((TextPointer)myRTB.Document.ContentStart, (TextPointer)myRTB.Document.ContentEnd);
                                var file = new FileStream(dialog.FileName, FileMode.Create);
                                t.Save(file, DataFormats.Text);
                                file.Close();
                                break;
                            }
                    }

                    string currentFile = dialog.FileName;
                    dialog.Title = "Editor: " + currentFile.ToString();
                }
                else if (ActiveTBox.GetType().ToString() == "System.Windows.Controls.TextBox")
                {
                    //myTB = (TextBox)ActiveTBox;
                    File.WriteAllText(dialog.FileName, sText);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(Mod_Public.sAppPath + @"\Log.txt", string.Format("{0}{1}", Environment.NewLine, DateAndTime.Now.ToString() + "; " + ex.ToString()));
                // https://stackovergo.com/de/q/1029597/mvvm-toolkit---how-do-i-get-a-messagebox-to-display-an-error-from-the-callback-if-the-wcf-service-fails
                var MsgCmd = new RelayCommand<string>(m => MessageBox.Show("Unexpected error:" + Constants.vbNewLine + Constants.vbNewLine + ex.ToString()));
                MsgCmd.Execute("");
            }

            return default;
        }
    }
}