using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Xps;

namespace NES.CPU {
    internal class CPUMemoryMap {

        //https://nesmaker.nerdboard.nl/2022/03/21/the-cpu-memory-map/
        //https://fceux.com/web/help/NESRAMMappingFindingValues.html

        //TODO: we could make this more realistic with storing the PRG-ROM banks, the Reset code...

        private Byte[] RAM;
        private Byte[] RAMMirror1;
        private Byte[] RAMMirror2;
        private Byte[] RAMMirror3;

        public CPUMemoryMap() {
            this.RAM = new Byte[2048];
            this.RAMMirror1 = new Byte[2048];
            this.RAMMirror2 = new Byte[2048];
            this.RAMMirror3 = new Byte[2048];
        }

        public int this[int index] {
            get {
                return 10;
            }
            set {
                switch(index){
                    case 0x00 and <= 0x07FF:
                        //Write to RAM
                        StoreInRam(index, value);
                        break;
                    case >= 0x2000 and <= 0x2007:
                        //Input / Output registers
                        throw new NotImplementedException();
                        break;
                    case >= 0x4000 and <= 0x401f:
                        //Input / Output registers
                        throw new NotImplementedException();
                        break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        private void StoreInRam(int index, int value) {
            throw new NotImplementedException();
        }
    }
}
