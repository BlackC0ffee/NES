using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES.Console
{
    internal class Console {
        private NES.CPU.Ricoh2A03 cpu;
        
        public Console() {
            this.cpu = new CPU.Ricoh2A03();
            
        }
        public void Reset() {
            cpu.Reset();
        }
    }

}
