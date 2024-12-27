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


namespace NES
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private NES.Console.Console console;
        public MainWindow()
        {
            InitializeComponent();
            //console = new NES.Console.Console();
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == true)
            {
                //string output = "";
                //Cartridge c = new NES.Console.Cartridge(ofd.FileName);
                console = new NES.Console.Console(new System.IO.FileInfo(ofd.FileName));
                //output += c.ReturnHeader();
                //mainTextBlock.Text = output;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            //console.Reset();
            console.Demo();
        }

    }
}