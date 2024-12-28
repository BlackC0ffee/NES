using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES.Cartridge {
    internal class CHRROM {
        private Byte[] _data;
        public CHRROM(byte[] Data) {
            this._data = Data;
        }
    }
}
