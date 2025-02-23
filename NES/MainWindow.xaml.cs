using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NES.Console;
using System.Diagnostics;
using System.ComponentModel;


namespace NES
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        private NES.Console.Console console;

        public event PropertyChangedEventHandler PropertyChanged;
        private string debugOutput;
        public string DebugOutput { get { return debugOutput; }
            set {
                debugOutput = value;
                OnPropertyChanged(nameof(DebugOutput));
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            //console = null;

            //debugTextBlock.
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == true)
            {
                console = new NES.Console.Console(new System.IO.FileInfo(ofd.FileName));
                console.Run();

            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            //console.Reset();
            this.DebugOutput = "hello World!!!";
        }



        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}