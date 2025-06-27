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
    public partial class MainWindow : Window {
        private NES.Console.Console console;
        private Thread consoleThread;

        public MainWindow()
        {
            InitializeComponent();

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
            // Check if we are on the UI thread
            if (this.debugTextBlock.Dispatcher.CheckAccess()) {
                // Update the TextBlock directly
                this.debugTextBlock.Text += e.ProgramCounter + " " + e.opcode + " " + e.Instruction + " " + e.operand + Environment.NewLine;
            } else {
                // If not on the UI thread, use Dispatcher to update
                this.debugTextBlock.Dispatcher.Invoke(() => {
                    this.debugTextBlock.Text += e.ProgramCounter + " " + e.opcode + " " + e.Instruction + " " + e.operand + Environment.NewLine;
                });
            }
            Thread.Sleep(2000);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            //console.Reset();
            //this.DebugOutput = "hello World!!!";
        }

    }
}