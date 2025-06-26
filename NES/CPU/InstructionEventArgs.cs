using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES.CPU {
    public class InstructionEventArgs: EventArgs {
        public string ProgramCounter { get; set; }
        public string opcode { get; set; }
        public string Instruction { get; set; }
        public string operand { get; set; }
    }
}
