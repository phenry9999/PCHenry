using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;

namespace CsMvvmToolkit_CP
{

    public class TextData : ObservableRecipient, INotifyPropertyChanged
    {
        private string _text;
        private string _richText;
        private RibbonTextBox _NotifyTest;
        private string _readText;
        private TextBox _ActiveTextBox;
        private RichTextBox __ActiveRichTextBox;

        private RichTextBox _ActiveRichTextBox
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return __ActiveRichTextBox;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                __ActiveRichTextBox = value;
            }
        }

        private Ribbon _MyRibbonWPF;
        private MainWindow _MyMainWindow;

        public TextData()
        {
            // 
        }

            public MainWindow MyMainWindow
        {
            get
            {
                return _MyMainWindow;
            }

            set
            {
                _MyMainWindow = (MainWindow)value;
            }
        }

        public Ribbon MyRibbonWPF
        {
            get
            {
                return _MyRibbonWPF;
            }

            set
            {
                _MyRibbonWPF = value;
            }
        }

        public RibbonTextBox NotifyTestbox
        {
            get
            {
                return _NotifyTest;
            }

            set
            {
                _NotifyTest = value;
            }
        }

        public string RichText
        {
            get
            {
                return _richText;
            }

            set
            {
                _richText = value;
                OnPropertyChanged("RichText");
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
                OnPropertyChanged("GetText");
            }
        }

        public string ReadText
        {
            get
            {
                return _readText;
            }

            set
            {
                _readText = value;
                GetText = _readText;
                OnPropertyChanged("ReadText");
            }
        }

        public TextBox ActiveTextBox
        {
            get
            {
                return _ActiveTextBox;
            }

            set
            {
                _ActiveTextBox = value;
                OnPropertyChanged("ActiveTextBox");
            }
        }

        public RichTextBox ActiveRichTextBox
        {
            get
            {
                return _ActiveRichTextBox;
            }

            set
            {
                _ActiveRichTextBox = value;
            }
        }
    }
}