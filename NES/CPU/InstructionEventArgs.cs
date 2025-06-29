using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES.CPU {
    public class InstructionEventArgs: EventArgs {
        public string ProgramCounter { get; set; }
        public string Opcode { get; set; }
        public string Instruction { get; set; }
        public string Operand { get; set; }
        public void Clear() {
            ProgramCounter = "";
            Opcode = "";
            Instruction = "";
            Operand = "";
        }
    }
}
