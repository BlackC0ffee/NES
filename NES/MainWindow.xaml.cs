using Microsoft.Win32;
using NES.Console;
using NES.CPU;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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


namespace NES
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private NES.Console.Console console;
        private Thread consoleThread;
        public ObservableCollection<CPU.InstructionEventArgs> InstructionCollection { get; } = new ObservableCollection<CPU.InstructionEventArgs>();


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == true)
            {
                console = new NES.Console.Console(new System.IO.FileInfo(ofd.FileName));
                console.CPUStep += Console_CPUStep;
                consoleThread = new(new ThreadStart(console.Run));
                consoleThread.Start();
                
                //console.Run();
            }
        }

        private void Console_CPUStep(object sender, CPU.InstructionEventArgs e) {
            this.memoryMapDataGrid.Dispatcher.Invoke(() => {
                this.InstructionCollection.Add(e);
                this.memoryMapDataGrid.ScrollIntoView(this.memoryMapDataGrid.Items[this.memoryMapDataGrid.Items.Count - 1]);
            });
            
            Thread.Sleep(500);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            console.Demo();
            //console.Reset();
            //this.DebugOutput = "hello World!!!";
        }

    }
}