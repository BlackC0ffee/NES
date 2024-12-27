using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NES.Console
{
    internal class Console {
        private NES.CPU.Ricoh2A03 cpu;
        private Cartridge cartridge {  get; set; }
        //Cartridge c = new NES.Console.Cartridge(ofd.FileName);



        public Console(System.IO.FileInfo cartridgeFileInfo) {
            this.cpu = new CPU.Ricoh2A03();
            this.cartridge = new Cartridge(cartridgeFileInfo);
        }
        public void Reset() {
            cpu.Reset();
        }

        internal void Demo() {
            this.cpu.Demo();
        }
    }

}
