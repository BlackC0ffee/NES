using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES.PPU {
    internal class Ricoh2C02 {


        private NES.CPU.CPUMemoryMap cpuMemoryMap;
        public Ricoh2C02(NES.CPU.CPUMemoryMap cpuMemoryMap) {
            this.cpuMemoryMap = cpuMemoryMap;
        }

        internal void Step() {
            throw new NotImplementedException();
        }
    }
}
