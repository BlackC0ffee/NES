using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES.PPU {
    internal class Ricoh2C02 {

        private NES.Console.Cartridge Cartridge;
        public Ricoh2C02(NES.Console.Cartridge cartridge) {
            Cartridge = cartridge;
        }

        internal void Step() {
            throw new NotImplementedException();
        }
    }
}
