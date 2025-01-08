using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NES.RAM {
    internal class Memory {
        private Byte[] RAM { get; set; }
        public Memory() {
            this.RAM = new Byte[2048];
        }

        public int this[int index] {
            get {
                return 10;
            }
            set { }
        }
    }
}
