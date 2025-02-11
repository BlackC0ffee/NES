using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NES.Console
{
    internal class Console {
        private NES.CPU.Ricoh2A03 cpu;
        public string DebugOutput { get; set; }
        private Cartridge cartridge {  get; set; }
        //Cartridge c = new NES.Console.Cartridge(ofd.FileName);

        public Console(System.IO.FileInfo cartridgeFileInfo) {
            this.cartridge = new Cartridge(cartridgeFileInfo); // first load console. If succesfull we can "start" the console
            this.cpu = new CPU.Ricoh2A03(cartridge);
            
        }

        public void Reset() {
            cpu.Reset();
        }

        internal void Demo() {
            this.cpu.Demo();
        }

        public void Run() {
            DebugOutput = "Hello World!!!";
            this.cpu.Run();
        }
    }

}
