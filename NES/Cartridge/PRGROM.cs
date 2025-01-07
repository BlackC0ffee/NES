using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES.Cartridge {
    internal class PRGROM {
        internal Byte[] Data { get; }
        public PRGROM(byte[] Data) {
            //this._data = new Byte[16384];
            this.Data = Data;
        }
    }
}
