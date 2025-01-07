using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NES.RAM {
    internal class Memory {
        internal Byte[] Data {  get; set; }
        public Memory() {
            this.Data = new Byte[2048];
        }
    }
}
