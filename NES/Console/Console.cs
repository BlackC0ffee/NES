using NES.CPU;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NES.Console
{
    public delegate void CPUStepEventHandler(object sender, InstructionEventArgs e);
    internal class Console {
        private NES.CPU.Ricoh2A03 cpu;
        public string DebugOutput { get; set; }
        private Cartridge cartridge {  get; set; }
        //Cartridge c = new NES.Console.Cartridge(ofd.FileName);

        public event CPUStepEventHandler CPUStep;

        public Console(System.IO.FileInfo cartridgeFileInfo) {
            this.cartridge = new Cartridge(cartridgeFileInfo); // first load console. If succesfull we can "start" the console
            this.cpu = new CPU.Ricoh2A03(cartridge);
            this.cpu.InstructionExecuted += cpu_InstructionExecuted;

        }

        private void cpu_InstructionExecuted(object sender, InstructionEventArgs e) {
            CPUStep(this, e);
        }

        public void Reset() {
            cpu.Reset();
        }

        internal void Demo() {
            this.cpu.Demo();
        }

        public void Run() {
            this.cpu.Run();
        }
    }

}
