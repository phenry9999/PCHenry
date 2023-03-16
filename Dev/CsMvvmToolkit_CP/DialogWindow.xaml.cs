using System.Windows;
using System.Collections.ObjectModel;
using CsMvvmToolkit_CP;
using System;

namespace CsMvvmToolkit_CP
{
    public partial class DialogWindow
    {
        private ObservableCollection<Credits> credits;

        public DialogWindow()
        {
            InitializeComponent();
        }

        public void ShowDialog(ObservableCollection<Credits> myCredits)
        {
            credits = myCredits;
            this.ShowDialog();
        }

        private void OkClick(object sender, RoutedEventArgs e)

        {
            try
            {
                credits.Add(new Credits
                {
                    Item = txtBoxItem.Text,
                    Note = txtBoxNote.Text,
                    Link = txtBoxLink.Text
                });

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                Mod_Public.ErrHandler(ex.ToString());
            }
        }
    }
}    

