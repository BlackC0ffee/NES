using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NES.RAM {
    internal class Memory {
        private Byte[] _data;
        public Memory() {
            _data = new Byte[2048];
        }
    }
}
