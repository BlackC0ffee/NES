using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES.Cartridge {
    internal class PRGROM {
        private Byte[] _data;
        public PRGROM(byte[] Data) {
            //this._data = new Byte[16384];
            this._data = Data;
        }
    }
}
