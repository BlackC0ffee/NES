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
        //https://smaldragon.github.io/blog/nes.html

        //TODO: we could make this more realistic with storing the PRG-ROM banks, the Reset code...

        private NES.Console.Cartridge cartridge;
        private NES.Cartridge.PRGROM currentPRGROMBank;

        private Byte[] RAM; // $0000 - $07FF
        private Byte[] PPURegisters; // $2000 – $2007
        private Byte[] TwoA03Registers; // $4000 - $401F
        private Byte[] CartridgeSpace; // $4020 – $FFFF



        public CPUMemoryMap(NES.Console.Cartridge cartridge) {
            this.cartridge = cartridge;
            this.RAM = new Byte[2048];
            this.PPURegisters = new Byte[8];
            this.TwoA03Registers = new Byte[32];
            this.CartridgeSpace = new Byte[49120];

            this.currentPRGROMBank = this.cartridge.PRGROMBanks[0];

        }

        public Byte this[int index] {
            get {
                switch (index) {
                    case >= 0x2000 and <= 0x2007:
                        return this.PPURegisters[index - 0x2000];
                    case >= 0x8000 and <= 0xBFFF:
                        return this.currentPRGROMBank.Data[index + (~0x8000 + 1)]; 
                    case 0xfffc:
                        return 0x00; // This is the reset vector. For now we will point it to 0x8000 covering lots of games. Later we will need to make this more dynamic, based on the mapper.
                    case 0xfffd:
                        return 0x80;
                        break;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                switch(index){
                    case 0x00 and <= 0x07FF:
                        //Write to RAM
                        StoreInRam(index, value);
                        break;
                    case >= 0x2000 and <= 0x2007:
                        //Input / Output registers
                        StoreInPPURegisters(index, value);
                        break;
                    case >= 0x4000 and <= 0x401f:
                        //Input / Output registers
                        StoreInCartridgeSpace(index, value);
                        break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        private void StoreInRam(int index, int value) {
            throw new NotImplementedException();
        }

        private void StoreInCartridgeSpace(int index, Byte value) {
            int address = index - 0x4000;
            if (address < 0 || address > 0xFFFF) { throw new IndexOutOfRangeException(); }
            this.CartridgeSpace[address] = value;
        }

        private void StoreInPPURegisters(int index, Byte value) {
            int address = index - 0x2000;
            if (address < 0 || address > 0x7) { throw new IndexOutOfRangeException(); }
            this.PPURegisters[address] = value;
        }
    }
}
