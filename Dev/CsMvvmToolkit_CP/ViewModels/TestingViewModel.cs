
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualBasic;

namespace CsMvvmToolkit_CP
{
    public class TestingViewModel : ObservableRecipient, INotifyPropertyChanged
    {

        #region  fields

        private ObservableCollection<Credits> _credit = new ObservableCollection<Credits>();
        private TextData _textData = new TextData();
        private string msg;
        private string _text;
        private string _dialogMessage;
        private string _OnPropertyChangedTest;

        private ParamCommand parameterizedCommand;
        private ParamCommand paramCommandTBox;
        private ParamCommand paramCommandWnd;
        private ParamCommand paramCommandRbn;

        #endregion


        #region  constructors

        public TestingViewModel()
        {

            #endregion


            #region  properties

            _cmdN = new Command(New_Click);
            _cmdE = new Command(Exit_Click);
            _cmdD = new Command(Print_Click);
            _cmdI = new Command(Info_Click);
            _cmdBG = new Command(BackgroundGreenRibbonButton_Click);
            _cmdBW = new Command(BackgroundWhiteRibbonButton_Click);
            _cmdQAT = new Command(RestoreQAT_Click);
            _cmdMsg = new Command(SendMsgRibbonButton_Click);
            _cmdErr = new Command(GetErrorButton_Click);
            _cmdClear = new Command(ClearListboxButton_Click);
            _cmdLog = new Command(ReadLog_Click);
            _cmdReadXml = new Command(ReadXml_Click);
            _cmdSaveXml = new Command(SaveXml_Click);
            _cmdL = new Command(App_Loaded);

            try
            {

                Messenger.Register<DialogMessage>(this, (r, m) => DialogMessage = m.NewStatus);
                Messenger.Register<StatusMessage>(this, (r, m) => StatusBarMessage = m.NewStatus);
                _credit = new ObservableCollection<Credits>()
                {
                    new Credits()
                    {
                        Item = "MVVM Toolkit",
                        Note = "Microsoft",
                        Link = "https://docs.microsoft.com/en-us/windows/communitytoolkit/mvvm/introduction"
                    },
                    new Credits()
                    {
                        Item = "MVVMLight",
                        Note = "GalaSoft",
                        Link = "https://www.codeproject.com/Articles/768427/The-big-MVVM-Template"
                    },
                    new Credits()
                    {
                        Item = "ICommand with MVVM pattern",
                        Note = "CPOL",
                        Link = "https://www.codeproject.com/Articles/863671/Using-ICommand-with-MVVM-pattern"
                    },
                    new Credits()
                    {
                        Item = "C# WPF WYSIWYG HTML Editor - CodeProject",
                        Note = "CPOL",
                        Link = "https://www.codeproject.com/Tips/870549/Csharp-WPF-WYSIWYG-HTML-Editor"
                    },
                    new Credits()
                    {
                        Item = "SearchDialog",
                        Note = "Forum Msg",
                        Link = "https://social.msdn.microsoft.com/forums/vstudio/en-US/fc46affc-9dc9-4a8f-b845-89a024b263bc/how-to-find-and-replace-words-in-wpf-richtextbox"
                    }
                };

                // https://social.msdn.microsoft.com/Forums/en-US/b97b70ad-edd1-4345-bcc0-f90e3096126b/vb-net-wpf-mvvm-remove-item-from-observable-collection-when-not-in-same-viewmodel

                RelayCommand cmdFont = new RelayCommand(FontDlg);
                FontDlgCommand = cmdFont;
                RelayCommand cmdSearch = new RelayCommand(SearchDialog);
                SearchDlgCommand = cmdSearch;
                RelayCommand cmdOFD = new RelayCommand(OpenFileDialog);
                OpenFileDlgCommand = cmdOFD;
                RelayCommand cmdSAFD = new RelayCommand(SaveAsFileDialog);
                SaveAsFileDlgCommand = cmdSAFD;
                RelayCommand cmdI = new RelayCommand(OpenUserInput);
                OpenUserInputCommand = cmdI;
                GetCredits = _credit;
                SaveXml_Click();
                // 
                // Create the parameterized commands.
                parameterizedCommand = new ParamCommand(DoParameterisedCommand);
                paramCommandTBox = new ParamCommand(DoParamCommandTBox);
                paramCommandWnd = new ParamCommand(DoParamCommandWnd);
                paramCommandRbn = new ParamCommand(DoParamCommandRbn);
            }
            catch (Exception ex)
            {
                File.AppendAllText(Mod_Public.sAppPath + @"\Log.txt", string.Format("{0}{1}", Environment.NewLine, DateAndTime.Now.ToString() + "; " + ex.ToString()));
                var msgBoxService = Ioc.Default.GetService<IMsgBoxService>();
                msgBoxService.Show("Unexpected error:" + Constants.vbNewLine + Constants.vbNewLine + ex.ToString(), img: MessageBoxImage.Error);
            }
        }

        private Command _cmdN;
        private Command _cmdE;
        private Command _cmdD;
        private Command _cmdI;
        private Command _cmdBG;
        private Command _cmdBW;
        private Command _cmdQAT;
        private Command _cmdMsg;
        private Command _cmdErr;
        private Command _cmdClear;
        private Command _cmdLog;
        private Command _cmdReadXml;
        private Command _cmdSaveXml;
        private Command _cmdL;

        public ICommand SearchDlgCommand { get; set; }
        public ICommand OpenUserInputCommand { get; set; }
        public ICommand RibbonToFormCommand { get; set; }
        public ICommand OpenFileDlgCommand { get; set; }
        public ICommand SaveAsFileDlgCommand { get; set; }
        public ICommand PrintFileDlgCommand { get; set; }
        public ICommand FontDlgCommand { get; set; }

        public MainWindow MyMainWindow
        {
            get
            {
                return _textData.MyMainWindow;
            }

            set
            {
                _textData.MyMainWindow = (MainWindow)value;
            }
        }

        public Ribbon MyRibbonWPF
        {
            get
            {
                return _textData.MyRibbonWPF;
            }

            set
            {
                _textData.MyRibbonWPF = value;
            }
        }

        public RichTextBox ActiveRichTextBox
        {
            get
            {
                return _textData.ActiveRichTextBox;
            }

            set
            {
                _textData.ActiveRichTextBox = value;
                OnPropertyChanged("ActiveRichTextBox");
                if (value is object)
                NotifyTestbox.Text = "RichText";
            }
        }

        public TextBox ActiveTextBox
        {
            get
            {
                return _textData.ActiveTextBox;
            }

            set
            {
                _textData.ActiveTextBox = value;
                OnPropertyChanged("ActiveTextBox");
                if (value is object)
                NotifyTestbox.Text = "PlainText";
            }
        }

        public string GetText
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
                _textData.GetText = value;
                OnPropertyChanged("GetText");
            }
        }

        public string OnPropertyChangedTest
        {
            get
            {
                return _OnPropertyChangedTest;
            }

            set
            {
                _OnPropertyChangedTest = value;
                OnPropertyChanged("OnPropertyChangedTest");
            }
        }

        public RibbonTextBox NotifyTestbox
        {
            get
            {
                return _textData.NotifyTestbox;
            }

            set
            {
                _textData.NotifyTestbox = value;
                OnPropertyChanged("NotifyTestbox");
            }
        }

        public ObservableCollection<Credits> GetCredits
        {
            get
            {
                return _credit;
            }

            set
            {
                _credit = value;
                OnPropertyChanged("GetCredits");
            }
        }

        public ICommand SaveXml
        {
            get
            {
                return _cmdSaveXml;
            }
        }

        public ICommand ReadXml
        {
            get
            {
                return _cmdReadXml;
            }
        }

        public ICommand ReadLog
        {
            get
            {
                return _cmdLog;
            }
        }

        public ICommand SendMsg
        {
            get
            {
                return _cmdMsg;
            }
        }

        public ICommand GetError
        {
            get
            {
                return _cmdErr;
            }
        }

        public ICommand ClearListbox
        {
            get
            {
                return _cmdClear;
            }
        }

        public ICommand GreenBackground
        {
            get
            {
                return _cmdBG;
            }
        }

        public ICommand WhiteBackground
        {
            get
            {
                return _cmdBW;
            }
        }

        public ICommand RestoreQAT
        {
            get
            {
                return _cmdQAT;
            }
        }

        public ICommand Info
        {
            get
            {
                return _cmdI;
            }
        }

        public ICommand NewFile
        {
            get
            {
                return _cmdN;
            }
        }

        public ICommand ExitApp
        {
            get
            {
                return _cmdE;
            }
        }

        public ICommand Apploaded
        {
            get
            {
                return _cmdL;
            }
        }

        public ICommand Print
        {
            get
            {
                return _cmdD;
            }
        }


        #endregion


        #region  Dialogs

        private  void FontDlg()
        {
            var dialog = Ioc.Default.GetService<IRichTextDlgVM>();
             if (ActiveRichTextBox is object)
            {
                dialog.FontsDlg(ActiveRichTextBox);
            }

            if (ActiveTextBox is object) // .IsFocused Then
            {
                dialog.FontsDlg(ActiveTextBox);
            }
        }

        private void SearchDialog()
        {
            var dialog = Ioc.Default.GetService<IRichTextDlgVM>();
            if (ActiveRichTextBox is object)
            {
                dialog.SearchDlg(ActiveRichTextBox);
            }

            if (ActiveTextBox is object) // .IsFocused Then
            {
                dialog.SearchDlg(ActiveTextBox);
            }
        }

        private void OpenFileDialog()
        {
           string sPath;
            var dialog = Ioc.Default.GetService<IOpenFileDlgVM>();
            if (ActiveRichTextBox is object) // .IsFocused Then
            {
                sPath = dialog.OpenFileDlg(ActiveRichTextBox);
            }
            else
            {

            }

            if (ActiveTextBox is object) // .IsFocused Then
            {
                sPath = dialog.OpenFileDlg(ActiveTextBox);
                if (Strings.Len(sPath) > 1)
                    GetText = Mod_Public.ReadTextLines(sPath);
            }
        }

        private void SaveAsFileDialog()
        {
            var dialog = Ioc.Default.GetService<ISaveAsFileDlgVM>();

            if (ActiveRichTextBox is object) 
            {
                dialog.SaveAsFileDlg(_textData.RichText, ActiveRichTextBox);
            }

            if (ActiveTextBox is object) 
            {
                dialog.SaveAsFileDlg(_textData.GetText, ActiveTextBox);
            }
        }

        private void OpenUserInput()
        {
            var dialog = Ioc.Default.GetService<IDialog>();
            dialog.ShowMyDialog(GetCredits);
        }

        #endregion


        #region  ParameterisedCommand

        public ParamCommand ParamCommandMainWnd
        {
            get
            {
                return paramCommandWnd;
            }
        }

        private void DoParamCommandWnd(object parameter)
        {
            _textData.MyMainWindow = (MainWindow)parameter;
        }

        public ParamCommand ParamCommandRibn
        {
            get
            {
                return paramCommandRbn;
            }
        }

        private void DoParamCommandRbn(object parameter)
        {
            _textData.MyRibbonWPF = (Ribbon)parameter;
        }

        public ParamCommand ParamCommandTBx
        {
            get
            {
                return paramCommandTBox;
            }
        }

        private void DoParamCommandTBox(object parameter)
        {
            _textData.ActiveTextBox = (TextBox)parameter;
            _textData.ActiveRichTextBox = null;
            OnPropertyChangedTest = "PlainText";
        }

        public ParamCommand ParameterisedCommand
        {
            get
            {
                return parameterizedCommand;
            }
        }

        private void DoParameterisedCommand(object parameter)
        {
            _textData.ActiveRichTextBox = (RichTextBox)parameter;
            _textData.ActiveTextBox = null;
            OnPropertyChangedTest = "RichText";
        }

        #endregion


        #region  methods

        private void RestoreQAT_Click()
        {
            try
            {

                if (MyRibbonWPF is object)
                {
                    if (MyMainWindow.AppCmdNewQAT.IsLoaded == false)
                    {
                        MyRibbonWPF.QuickAccessToolBar.Items.Add(MyMainWindow.AppCmdNewQAT);
                    }

                    if (MyMainWindow.AppCmdOpenQAT.IsLoaded == false)
                    {
                        MyRibbonWPF.QuickAccessToolBar.Items.Add(MyMainWindow.AppCmdOpenQAT);
                    }

                    if (MyMainWindow.AppCmdSaveAsQAT.IsLoaded == false)
                    {
                        MyRibbonWPF.QuickAccessToolBar.Items.Add(MyMainWindow.AppCmdSaveAsQAT);
                    }

                    if (MyMainWindow.AppCmdCloseQAT.IsLoaded == false)
                    {
                        MyRibbonWPF.QuickAccessToolBar.Items.Add(MyMainWindow.AppCmdCloseQAT);
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(Mod_Public.sAppPath + @"\_Log.txt", string.Format("{0}{1}", Environment.NewLine, DateAndTime.Now.ToString() + "; " + ex.ToString()));
                var msgBoxService = Ioc.Default.GetService<IMsgBoxService>();
                msgBoxService.Show("Unexpected error:" + Constants.vbNewLine + Constants.vbNewLine + ex.ToString(), img: MessageBoxImage.Error);
            }
        }

        private void ClearListboxButton_Click()
        {
            try
            {
                _credit = GetCredits;

                // Remove first line
                // _credit.RemoveAt(0)

                // ' Remove current line
                // Dim n As Integer = MyMainWindow.lstCredits.SelectedIndex
                // GetCredits.RemoveAt(n)

                // Remove all
                GetCredits.Clear();
            }
            catch (Exception ex)
            {
                SetStatus("TestingViewModel", ex.ToString());
                Mod_Public.ErrHandler(ex.ToString());
            }
        }

        private void SaveXml_Click()
        {
            try
            {
                // Save to xml
                var xs = new XmlSerializer(typeof(ObservableCollection<Credits>));
                using (var wr = new StreamWriter(Mod_Public.sAppPath + @"\MyCredits.xml"))
                {
                    xs.Serialize(wr, _credit);
                }
            }
            catch (Exception ex)
            {
                SetStatus("TestingViewModel", ex.ToString());
                Mod_Public.ErrHandler(ex.ToString());
            }
        }

        private void ReadXml_Click()
        {
            try
            {
                GetCredits.Clear();
                // read xml
                var xs = new XmlSerializer(typeof(ObservableCollection<Credits>));
                using (var rd = new StreamReader(Mod_Public.sAppPath + @"\MyCredits.xml"))
                {
                    GetCredits = xs.Deserialize(rd) as ObservableCollection<Credits>;
                }
            }
            catch (Exception ex)
            {
                SetStatus("TestingViewModel", ex.ToString());
                Mod_Public.ErrHandler(ex.ToString());
            }
        }

        private void ReadLog_Click()
        {
            try
            {
                GetText = Mod_Public.ReadTextLines(Mod_Public.sAppPath + @"\Log.txt");
            }
            catch (Exception ex)
            {
                SetStatus("TestingViewModel", ex.ToString());
                Mod_Public.ErrHandler(ex.ToString());
            }
        }

        private void Info_Click()
        {
            msg = "Demo created by Jo_vb.net, v2.0, Feb. 2022, License CPOL.";
            var msgBoxService = Ioc.Default.GetService<IMsgBoxService>();
            msgBoxService.Show(Constants.vbNewLine + msg, "Info");
        }

        private void New_Click()
        {
            try
            {
                if (ActiveRichTextBox is object)
                {
                    ActiveRichTextBox.Document.Blocks.Clear();
                }

                if (ActiveTextBox is object)
                {
                    GetText = Constants.vbNullString;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(Mod_Public.sAppPath + @"\_Log.txt", string.Format("{0}{1}", Environment.NewLine, DateAndTime.Now.ToString() + "; " + ex.ToString()));
                var msgBoxService = Ioc.Default.GetService<IMsgBoxService>();
                msgBoxService.Show("Unexpected error:" + Constants.vbNewLine + Constants.vbNewLine + ex.ToString(), img: MessageBoxImage.Error);
            }
        }

        private void BackgroundGreenRibbonButton_Click()
        {
            if (MyRibbonWPF is object)
            {
                MyRibbonWPF.Background = new SolidColorBrush(Color.FromRgb(120, 190, 96));
            }
        }

        private void BackgroundWhiteRibbonButton_Click()
        {
            if (MyRibbonWPF is object)
            {
                MyRibbonWPF.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
        }

        private void Print_Click()
        {
            var dialog = Ioc.Default.GetService<IRichTextDlgVM>();

            if (ActiveRichTextBox is object)
            {
                dialog.PrintDlg(ActiveRichTextBox);
            }

            if (ActiveTextBox is object) // .IsFocused Then
            {
                dialog.PrintDlg(ActiveTextBox);
            }
        }
        
            //// Print RichTextBox content
            //// https://www.codeproject.com/Tips/829368/A-Simple-RTF-Print-Form


        private void App_Loaded()
        {

            try
            {
                if (MyMainWindow.RibbonWPF is object)
                {
                    if (Properties.Settings.Default.AppCmdNewQAT_Visible == false)
                    {
                        MyRibbonWPF.QuickAccessToolBar.Items.Remove(MyMainWindow.AppCmdNewQAT);
                    }
                    if (Properties.Settings.Default.AppCmdOpenQAT_Visible == false)
                    {
                        MyRibbonWPF.QuickAccessToolBar.Items.Remove(MyMainWindow.AppCmdOpenQAT);
                    }
                    if (Properties.Settings.Default.AppCmdSaveAsQAT_Visible == false)
                    {
                        MyRibbonWPF.QuickAccessToolBar.Items.Remove(MyMainWindow.AppCmdSaveAsQAT);
                    }
                    if (Properties.Settings.Default.AppCmdCloseQAT_Visible == false)
                    {
                        MyRibbonWPF.QuickAccessToolBar.Items.Remove(MyMainWindow.AppCmdCloseQAT);
                    }
                }
            }

            catch (Exception ex)
            {
                File.AppendAllText(Mod_Public.sAppPath + @"\Log.txt", string.Format("{0}{1}", Environment.NewLine, DateAndTime.Now.ToString() + "; " + ex.ToString()));
                var msgBoxService = Ioc.Default.GetService<IMsgBoxService>();
                msgBoxService.Show("Unexpected error:" + Constants.vbNewLine + Constants.vbNewLine + ex.ToString(), img: MessageBoxImage.Error);
            }
        }

        private void Exit_Click()
        {
            try
            {
                {
                    var withBlock = MyMainWindow;
                    if (withBlock.AppCmdNewQAT.IsLoaded == false)
                    {
                        Properties.Settings.Default.AppCmdNewQAT_Visible = false;
                    }
                    else if (withBlock.AppCmdNewQAT.IsLoaded == true)
                    {
                        Properties.Settings.Default.AppCmdNewQAT_Visible = true;
                    }

                    if (withBlock.AppCmdOpenQAT.IsLoaded == false)
                    {
                        Properties.Settings.Default.AppCmdOpenQAT_Visible = false;
                    }
                    else if (withBlock.AppCmdOpenQAT.IsLoaded == true)
                    {
                        Properties.Settings.Default.AppCmdOpenQAT_Visible = true;
                    }

                    if (withBlock.AppCmdSaveAsQAT.IsLoaded == false)
                    {
                        Properties.Settings.Default.AppCmdSaveAsQAT_Visible = false;
                    }
                    else if (withBlock.AppCmdSaveAsQAT.IsLoaded == true)
                    {
                        Properties.Settings.Default.AppCmdSaveAsQAT_Visible = true;
                    }

                    if (withBlock.AppCmdCloseQAT.IsLoaded == false)
                    {
                        Properties.Settings.Default.AppCmdCloseQAT_Visible = false;
                    }
                    else if (withBlock.AppCmdCloseQAT.IsLoaded == true)
                    {
                        Properties.Settings.Default.AppCmdCloseQAT_Visible = true;
                    }
                }

                Properties.Settings.Default.Save();


            }
            catch (Exception ex)
            {
                File.AppendAllText(Mod_Public.sAppPath + @"\Log.txt", string.Format("{0}{1}", Environment.NewLine, DateAndTime.Now.ToString() + "; " + ex.ToString()));
            }

            System.Windows.Application.Current.Shutdown();
        }

        private void GetErrorButton_Click()
        {
            try
            {
                Ioc.Default.ConfigureServices(new ServiceCollection().AddSingleton<IMsgBoxService, MsgBoxService>().AddSingleton((IMsgBoxService)new MsgBoxService()).BuildServiceProvider());
            }
            catch (Exception ex)
            {
                SetStatus("TestingViewModel", ex.ToString());
                Mod_Public.ErrHandler(ex.ToString());
            }
        }

        private void SendMsgRibbonButton_Click()
        {
            try
            {
                // DataExchange / Messenger
                string msg = "Test Msg...";
                SetStatus("TestingViewModel", msg);
            }
            catch (Exception ex)
            {
                SetStatus("TestingViewModel", ex.ToString());
                Mod_Public.ErrHandler(ex.ToString());
            }
        }

        public void SetStatus(string r, string m)
        {
            try
            {
                Messenger.Send(new DialogMessage(m));
            }
            catch (Exception ex)
            {
                SetStatus("TestingViewModel", ex.ToString());
                Mod_Public.ErrHandler(ex.ToString());
            }
        }

            #endregion


        #region  Dialog Code

        // Used to bind any incoming dialog messages to the viewmodel.

        public string DialogMessage
        {
            get
            {
                return _dialogMessage;
            }

            set
            {
                 if (Equals(value, _dialogMessage))
                    return;
                _dialogMessage = value;
                GetText = value;
                OnPropertyChanged("DialogMessage");
            }
        }

        private string _statusBarMessage;

        public string StatusBarMessage
        {
            get
            {
                return _statusBarMessage;
            }

            set
            {
                if (Equals(value, _statusBarMessage))
                    return;
                _statusBarMessage = value;
                OnPropertyChanged("StatusBarMessage");
            }
        }

        ~TestingViewModel()
        {
            Messenger.Unregister<StatusMessage>(this);
            Messenger.Unregister<DialogMessage>(this);
        }

        #endregion

    }

    public class StatusMessage
    {
        public StatusMessage(string status)
        {
            NewStatus = status;
        }

        public string NewStatus { get; set; }
    }

    public class Credits
    {
        public string Item { get; set; }
        public string Note { get; set; }
        public string Link { get; set; }
    }


    #region  Command Classes

    public class Command : ICommand
    {
        private Action _action;

        public Command(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action();
        }
    }

    public class ParamCommand : ICommand
    {
        private Action<object> _action; // String)

        public ParamCommand(Action<object> action) // String))
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action(parameter); // .ToString)
        }
    }
}

#endregion

