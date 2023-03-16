
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.VisualBasic;

namespace CsMvvmToolkit_CP
{
    public class RichTextDlgVM : IRichTextDlgVM
    {
        private RichTextBox myRTB;
        private TextBox myTB;
        //private object ForegroundColor;

        public bool PrintDlg(object ActiveTBox)
        {
            {
                // Settings for printer dialog 
                System.Windows.Controls.PrintDialog dlg = new System.Windows.Controls.PrintDialog();
                dlg.PageRangeSelection = PageRangeSelection.AllPages;
                dlg.UserPageRangeEnabled = true;

                // Show and process dialog
                if (dlg.ShowDialog() == true)
                {
                    dlg.PrintVisual(ActiveTBox as Visual, "printing as visual");
                }
            }
            return default;
        }


        public bool PasteImageDlg(object ActiveTBox)
        {
            return default;
        }

        //https://stackoverflow.com/questions/17294236/how-to-open-color-and-font-dialog-box-using-wpf
        public bool FontsDlg(object ActiveTBox)
            
        {
            if (ActiveTBox.GetType().ToString() == "System.Windows.Controls.RichTextBox") myRTB = (RichTextBox)ActiveTBox;
            if (myRTB != null)
            {
                using (System.Windows.Forms.FontDialog FD = new System.Windows.Forms.FontDialog())
                {

                    FD.ShowColor = true;
                    //FD.ShowEffects = false;


                    if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)

                    {
                        myRTB.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(FD.Font.Name));
                        myRTB.Selection.ApplyPropertyValue(Run.FontSizeProperty,   FD.Font.Size * 5.0 / 4.0);
                        myRTB.Selection.ApplyPropertyValue(Run.FontWeightProperty, FD.Font.Bold ? FontWeights.Bold : FontWeights.Regular);
                        myRTB.Selection.ApplyPropertyValue(Run.FontStyleProperty, FD.Font.Italic ? FontStyles.Italic : FontStyles.Normal);

                        TextDecorationCollection tdc = new TextDecorationCollection();
                        if (FD.Font.Underline) tdc.Add(TextDecorations.Underline);
                        if (FD.Font.Strikeout) tdc.Add(TextDecorations.Strikethrough);
                        myRTB.Selection.ApplyPropertyValue(Run.TextDecorationsProperty, tdc);

                        TextRange range = new TextRange(myRTB.Selection.Start,
                            myRTB.Selection.End);

                        range.ApplyPropertyValue(FlowDocument.ForegroundProperty,
                            new SolidColorBrush(Color.FromArgb(FD.Color.A, FD.Color.R, FD.Color.G, FD.Color.B)));
                    }

                    //{
                    //    myRTB.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, FD.Font.FontFamily.Name);
                    //    myRTB.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, Convert.ToDouble(FD.Font.Size));
                    //    //myRTB.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FD.Font.Style);


                    //    //if (FD.Font.Style.ToString() == "Bold")
                    //    //{
                    //    //    myRTB.Selection.ApplyPropertyValue(Run.FontWeightProperty, FontWeights.Bold);
                    //    //}
                    //    //if (FD.Font.Style.ToString() == "Normal")
                    //    //{
                    //    //    myRTB.Selection.ApplyPropertyValue(Run.FontWeightProperty, FontWeights.Normal);
                    //    //}

                    //    if (FD.Font.Style.ToString() == "Italic")
                    //    {
                    //        myRTB.Selection.ApplyPropertyValue(Run.FontStyleProperty, FontStyles.Italic);
                    //    }
                    //    else if (FD.Font.Style.ToString() == "Oblique")
                    //    {
                    //        myRTB.Selection.ApplyPropertyValue(Run.FontStyleProperty, FontStyles.Oblique);
                    //    }
                    //    else if (FD.Font.Style.ToString() == "Normal")
                    //    {
                    //        myRTB.Selection.ApplyPropertyValue(Run.FontStyleProperty, FontStyles.Normal);
                    //    }

                    //    if (FD.Font.Style.ToString() == "Underline")
                    //    {
                    //        myRTB.Selection.ApplyPropertyValue(Run.TextDecorationsProperty, TextDecorations.Underline);
                    //    }
                    //    else if (FD.Font.Style.ToString() == "Strikeout")
                    //    {
                    //        myRTB.Selection.ApplyPropertyValue(Run.TextDecorationsProperty, TextDecorations.Strikethrough);
                    //    }

                    //    if (FD.Font.Style.ToString() == "Bold")
                    //    {
                    //        myRTB.Selection.ApplyPropertyValue(Run.FontWeightProperty, FontWeights.Bold);
                    //    }
                    //    else if (FD.Font.Style.ToString() == "Normal")
                    //    {
                    //        myRTB.Selection.ApplyPropertyValue(Run.FontWeightProperty, FontWeights.Normal);
                    //    }
                    //}
                }
            }
            else
            {
                string msg = "The Font Dialog works only for the RichTextBox!";
                var msgBoxService = Ioc.Default.GetService<IMsgBoxService>();
                msgBoxService.Show(Constants.vbNewLine + msg, "Info");
            }
            return default;
        }


        public bool ColorDlg(object ActiveTBox)
        {
            return default;
        }


        public bool SearchDlg(object ActiveTBox)
             
        {
            if (ActiveTBox.GetType().ToString() == "System.Windows.Controls.RichTextBox") myRTB = (RichTextBox)ActiveTBox;
            
            if (myRTB is object)
            {
                string sInput = Interaction.InputBox("Search for:", "Find");

            //Source: https://social.msdn.microsoft.com/forums/vstudio/en-US/fc46affc-9dc9-4a8f-b845-89a024b263bc/how-to-find-and-replace-words-in-wpf-richtextbox

                var text = new TextRange((TextPointer)myRTB.Document.ContentStart, (TextPointer)myRTB.Document.ContentEnd);
                var current = text.Start.GetInsertionPosition(LogicalDirection.Forward);
                while (current is object)
                {
                    string textInRun = current.GetTextInRun(LogicalDirection.Forward);
                    if (!string.IsNullOrWhiteSpace(textInRun))
                    {
                        int index = textInRun.IndexOf(sInput);
                        if (index != -1)
                        {
                            var selectionStart = current.GetPositionAtOffset(index, LogicalDirection.Forward);
                            var selectionEnd = selectionStart.GetPositionAtOffset(sInput.Length, LogicalDirection.Forward);
                            var selection = new TextRange(selectionStart, selectionEnd);
                            selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                            selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
                            myRTB.Selection.Select(selection.Start, selection.End);
                            myRTB.Focus();
                        }
                    }
                    current = current.GetNextContextPosition(LogicalDirection.Forward);
                }
            }
            else
            {
                string msg = "The Search Dialog works only for the RichTextBox!";
                var msgBoxService = Ioc.Default.GetService<IMsgBoxService>();
                msgBoxService.Show(Constants.vbNewLine + msg, "Info");
            }

            return default;
        }
    }
}