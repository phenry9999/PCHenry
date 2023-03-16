using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace CsMvvmToolkit_CP
{
    public class OpenFileDlgVM : ObservableRecipient, IOpenFileDlgVM
    {
        private string msg;
        private RichTextBox myRTB;
        private TextBox myTB;

        public OpenFileDlgVM()
        {
            // 
        }

        public string OpenFileDlg(object ActiveTBox)
        {

            FileDialog dialog = new OpenFileDialog();
            try
            {
                dialog.Filter = "All Files(*.*)|*.*|RTF Files (*.rtf)|*.rtf";
                dialog.FilterIndex = 1;
                dialog.Title = "RTE - Open File";
                dialog.DefaultExt = "rtf";
                // dialog.Filter = "Rich Text Files|*.rtf|" &
                // "Text Files|*.txt|HTML Files|" &
                // "*.htm|All Files|*.*"
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
                                var file = new FileStream(dialog.FileName, FileMode.Open);
                                t.Load(file, DataFormats.Rtf);
                                file.Close();
                                break;
                            }

                        default:
                            {
                                var t = new TextRange((TextPointer)myRTB.Document.ContentStart, (TextPointer)myRTB.Document.ContentEnd);
                                var file = new FileStream(dialog.FileName, FileMode.Open);
                                t.Load(file, DataFormats.Text);
                                file.Close();
                                break;
                            }
                    }

                    string currentFile = dialog.FileName;
                    dialog.Title = "Editor: " + currentFile.ToString();
                }
                else if (ActiveTBox.GetType().ToString() == "System.Windows.Controls.TextBox")
                {
                    myTB = (TextBox)ActiveTBox;
                    string currentFile = dialog.FileName;
                    myTB.Text = Mod_Public.ReadTextLines(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(Mod_Public.sAppPath + @"\Log.txt", string.Format("{0}{1}", Environment.NewLine, DateAndTime.Now.ToString() + "; " + ex.ToString()));
                var MsgCmd = new RelayCommand<string>(m => MessageBox.Show("Unexpected error:" + Constants.vbNewLine + Constants.vbNewLine + ex.ToString()));
                MsgCmd.Execute("");
            }

            return default;
        }

        public void SetStatus(string r, string m)
        {
            try
            {
                var s = Messenger.Send(new DialogMessage(m));
            }
            catch (Exception ex)
            {
                SetStatus("OpenFileDlgVM", ex.ToString());
                Mod_Public.ErrHandler(ex.ToString());
            }
        }
    }

    public class DialogMessage
    {
        public DialogMessage(string status)
        {
            NewStatus = status;
        }

        public string NewStatus { get; set; }
    }
}