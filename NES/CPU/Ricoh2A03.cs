//using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xaml.Schema;

namespace NES.CPU {

    public delegate void InstructionExecutedEventHandler(object sender, CPU.InstructionEventArgs e);
    internal class Ricoh2A03 : NES.CPU.IMOS6502 {
        //private NES.RAM.Memory memory;

        // Instruction set shizzle
        //     https://www.masswerk.at/6502/6502_instruction_set.html
        //     http://www.6502.org/users/obelisk/6502/reference.html
        
        // Registers
        private UInt16 pc; // 16bit or 2 byte program counter. Technically these are 2 seperate program counters on the IMOS6502: https://stackoverflow.com/questions/46646929/how-do-the-two-program-counter-registers-work-in-the-6502
        private Byte ac; // accumulator
        private Byte x; // X register
        private Byte y; // Y register
        private Byte sr; // status register [NV-BDIZC]
        private Byte sp; // stack pointer

        private ushort operand;
        private ulong CpuCycleCounter;


        private NES.CPU.CPUMemoryMap cPUMemoryMap;
        //public event Instrct

        // Variables
        private NES.Console.Cartridge cartridge;
        

        private bool brk; // temporary boolean we will use during programming

        
        public event InstructionExecutedEventHandler InstructionExecuted;
        private InstructionEventArgs instructionDetails;

        public Ricoh2A03(NES.Console.Cartridge cartridge) {
            this.cartridge = cartridge;
            cPUMemoryMap = new CPUMemoryMap(this.cartridge);
            instructionDetails = new InstructionEventArgs();
            sr = 0b00100000; // bit 5 has no name and is always set to 1
            //this.Reset();
            CpuCycleCounter = 0;
            brk = false;
            pc = (ushort)(this.cPUMemoryMap[0xfffc] | (this.cPUMemoryMap[0xfffd] << 8)); // Mapper 0
            //this.Run();
        }

        internal void Demo() {
            DEC(AddressingMode.ZeroPage);
        }



        public void ExecuteInstruction(int opcodes) {
            Debug.Write($"${this.pc:X}: "); // Writes the current Programcounter
            instructionDetails.ProgramCounter = $"${this.pc:X}";
            instructionDetails.Opcode = $"{opcodes:X}";
            switch (opcodes) {
                case 0x00: BRK(); break;
                case 0x01: ORA(); break;
                case 0x06: ASL(AddressingMode.ZeroPage); break;
                case 0x0A: ASL(AddressingMode.Accumulator); break;
                case 0x0E: ASL(AddressingMode.Absolute); break;
                case 0x10: BPL(); break;
                case 0x16: ASL(AddressingMode.ZeroPageX); break;
                case 0x1E: ASL(AddressingMode.AbsoluteX); break;
                case 0x21: AND(AddressingMode.XIndirect); break;
                case 0x25: AND(AddressingMode.ZeroPage); break;
                case 0x29: AND(AddressingMode.Immediate); break;
                case 0x30: BMI(); break;
                case 0x2c: BIT(AddressingMode.Absolute); break;
                case 0x2d: AND(AddressingMode.Absolute); break;
                case 0x31: AND(AddressingMode.IndirectY); break;
                case 0x35: AND(AddressingMode.ZeroPageX); break;
                case 0x39: AND(AddressingMode.AbsoluteY); break;
                case 0x3d: AND(AddressingMode.AbsoluteX); break;
                case 0x4c: JMP(AddressingMode.Absolute); break;
                case 0x50: BVC(); break;
                case 0x61: ADC(AddressingMode.XIndirect); break;
                case 0x65: ADC(AddressingMode.ZeroPage); break;
                case 0x69: ADC(AddressingMode.Immediate); break;
                case 0x6D: ADC(AddressingMode.Absolute); break;
                case 0x70: BVS(); break;
                case 0x71: ADC(AddressingMode.IndirectY); break;
                case 0x75: ADC(AddressingMode.ZeroPageX); break;
                case 0x78: SEI(); break;
                case 0x79: ADC(AddressingMode.AbsoluteY); break;
                case 0x7D: ADC(AddressingMode.AbsoluteX); break;
                case 0x85: STA(AddressingMode.ZeroPage); break;
                case 0x8a: TXA(); break;
                case 0x8d: STA(AddressingMode.Absolute); break;
                case 0x8e: STX(AddressingMode.Absolute); break;
                case 0x90: BCC(AddressingMode.Relative); break;
                case 0x95: STA(AddressingMode.ZeroPage); break;
                case 0x9d: STA(AddressingMode.AbsoluteX); break;
                case 0x9a: TXS(); break;
                case 0xa2: LDX(AddressingMode.Immediate); break;
                case 0xa5: LDA(AddressingMode.ZeroPage); break;
                case 0xa9: LDA(AddressingMode.Immediate); break;
                case 0xad: LDA(AddressingMode.Absolute); break;
                case 0xb0: BCS(); break;
                case 0xb8: CLV(); break;
                case 0xc1: CMP(AddressingMode.XIndirect); break;
                case 0xc5: CMP(AddressingMode.ZeroPage); break;
                case 0xc6: DEC(AddressingMode.ZeroPage); break;
                case 0xc9: CMP(AddressingMode.Immediate); break;
                case 0xcd: CMP(AddressingMode.Absolute); break;
                case 0xce: DEC(AddressingMode.Absolute); break;
                case 0xd0: BNE(); break;
                case 0xd1: CMP(AddressingMode.IndirectY); break;
                case 0xd5: CMP(AddressingMode.ZeroPageX); break;
                case 0xd6: DEC(AddressingMode.ZeroPageX); break;
                case 0xd8: CLD(); break;
                case 0xd9: CMP(AddressingMode.AbsoluteY); break;
                case 0xdd: CMP(AddressingMode.AbsoluteX); break;
                case 0xde: DEC(AddressingMode.AbsoluteX); break;
                case 0xe8: INX(); break;
                case 0xf0: BEQ(); break;
                default: throw new NotImplementedException($"Instruction with opcode {opcodes:X} not found"); break;
            }
            pc++; //increment program counter at the end of the cycle
        }

        public void Reset() { //Runs the reset program based on https://www.nesdev.org/wiki/Init_code
            SEI(); // Ignore IRQs
            CLC(); // Disable decimal mode
                   //LDX("#$40");
                   //STX("$4017"); // Disable APU frame IRQ
                   //LDX("#$ff");
                   //TXS(); // Set up stack
                   //INX(); //now X = 0
                   //STX("$2000"); // Disable NMI
                   //STX("$2001"); // Disable rendering
                   //STX("$4010"); // Disable DMC IRQs

            /**@vblankwait1:
                bit $2002
        bpl @vblankwait1

        ; We now have about 30,000 cycles to burn before the PPU stabilizes.
        ; One thing we can do with this time is put RAM in a known state.
        ; Here we fill it with $00, which matches what(say) a C compiler
        ; expects for BSS.Conveniently, X is still 0.
        txa
@clrmem:
        sta $000, x
        sta $100, x
        sta $200, x
        sta $300, x
        sta $400, x
        sta $500, x
        sta $600, x
        sta $700, x
        inx
        bne @clrmem

        ; Other things you can do between vblank waits are set up audio
        ; or set up other mapper registers. 


@vblankwait2:
                bit $2002
        bpl @vblankwait2 **/
            pc = (ushort)(this.cPUMemoryMap[0xfffc] | (this.cPUMemoryMap[0xfffd] << 8));
                //cPUMemoryMap[0xfffc];
        }

        public void Run() {
            while (!brk){
                ExecuteInstruction(cPUMemoryMap[pc]);
                InstructionExecuted(this, instructionDetails);
                instructionDetails.Clear();
                //brk = true;
                // to check Status Register functions operand bytes

                //do {
                //    pc = 65535;
                //    pc++;
                //} while (pc < 65535); Should never happen I think thanks to bank switching...
            }
        }

        #region Status Register functions (sr)
        // NV1B DIZC
        // |||| ||||
        // |||| |||+- Carry
        // |||| ||+-- Zero
        // |||| |+--- Interrupt Disable
        // |||| +---- Decimal
        // |||+------ Break
        // ||+------- (No CPU effect; always pushed as 1)
        // |+-------- Overflow
        // +--------- Negative
        public void SEC() { // Set the carry flag to 1
            this.instructionDetails.Instruction = "SEC";
            Byte mask = 0b00000001;
            sr = (Byte)(sr ^ mask);
            this.CpuCycleCounter += 2;
        }
        
        public void CLC() { // Set the carry flag to 0
            this.instructionDetails.Instruction = "CLC";
            Byte mask = 0b11111110;
            sr = (Byte)(sr & mask);
            this.CpuCycleCounter += 2;
        }

        public void SEI() { // Set Interrupt Disable flag to 1
            this.instructionDetails.Instruction = "SEI";
            Byte mask = 0b00000100;
            sr = (Byte)(sr ^ mask);
            this.CpuCycleCounter += 2;
        }
        public void CLI() { // Set Interrupt Disable flag to 0
            this.instructionDetails.Instruction = "CLI";
            Byte mask = 0b11111011;
            sr = (Byte)(sr & mask);
            this.CpuCycleCounter += 2;
        }

        public void SED() { // Set Decimal Mode flag to 1
            this.instructionDetails.Instruction = "SED";
            Byte mask = 0b00001000;
            sr = (Byte)(sr ^ mask);
            this.CpuCycleCounter += 2;
        }
        public void CLD() { // Set Decimal Mode flag to 0
            this.instructionDetails.Instruction = "CLD";
            Byte mask = 0b11110111;
            sr = (Byte)(sr & mask);
            this.CpuCycleCounter += 2;
        }

        public void BRK() { // Set the Break Command flag to 1
            this.instructionDetails.Instruction = "BRK";
            Byte mask = 0b00010000;
            sr = (Byte)(sr ^ mask);
            this.CpuCycleCounter += 7;
        }

        public void CLV() {
            this.instructionDetails.Instruction = "CLV";
            Byte mask = 0b11111110;
            sr = (Byte)(sr & mask);
            this.CpuCycleCounter += 2;
        }
        #endregion
        public void ADC(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "ADC";
            int data;
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    data = Immediate();
                    break;
                case AddressingMode.ZeroPage:
                    data = ZeroPage();
                    break;
                case AddressingMode.ZeroPageX:
                    data = ZeroPageX();
                    break;
                case AddressingMode.Absolute:
                    data = Absolute();
                    break;
                case AddressingMode.AbsoluteX:
                    data = AbsoluteX();
                    break;
                case AddressingMode.AbsoluteY:
                    data = AbsoluteY();
                    break;
                case AddressingMode.XIndirect:
                    data = XIndirect();
                    break;
                case AddressingMode.IndirectY:
                    data= IndirectY();
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
            int c = this.sr & 0b00000001;
            


            int result = this.ac + data + c;
            

            if (result < 0 || result > 255) { // this checksif the results is less or more in case of an unsigned Byte. The carry flag doesn't know or cares if the result is signed or unsigned
                SetCarryFlag();
            } else {
                ClearCarryFlag();
            }

            //next check for the overflow flag (https://www.youtube.com/watch?v=8XmxKPJDGU0)
            Boolean a = GetMostSignificantBit(ac) != 0;
            Boolean d = GetMostSignificantBit((Byte)data) != 0;
            Boolean r = GetMostSignificantBit((Byte)result) != 0;
            if ((a^r)&!(a^r)){
                SetOverFlowFlag();
            }

            //finaly set the new accumulator
            this.ac = (Byte)result; // This removes the unwanted bits, because the ac is only one byte.


            //if(result > 0xff) {
            //    sr = (Byte)(sr | 0b00000001); // th
            //}
            throw new NotImplementedException();

        }

        public void AND(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "AND";
            int data;
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    data = Immediate();
                    break;
                case AddressingMode.Absolute:
                    data = Absolute();
                    break;
                case AddressingMode.ZeroPage:
                    data = ZeroPage();
                    break;
                case AddressingMode.AbsoluteX:
                    data = AbsoluteX();
                    break;
                case AddressingMode.AbsoluteY:
                    data = AbsoluteY();
                    break;
                case AddressingMode.XIndirect:
                    data = XIndirect();
                    break;
                case AddressingMode.IndirectY:
                    data = IndirectY();
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
            this.ac = (Byte)(this.ac & data);
            if (this.ac == 0){ SetZeroFlag(); }
            if(isNegative(this.ac)){ SetNegativeFlag(); }
        }

        public void ASL(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "ASL";
            int data;
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    data = ZeroPage();
                    break;
                case AddressingMode.Accumulator:
                    data = this.ac;
                    break;
                case AddressingMode.Absolute:
                    data = Absolute();
                    break;
                case AddressingMode.ZeroPageX:
                    data = ZeroPageX();
                    break;
                case AddressingMode.AbsoluteX:
                    data = AbsoluteX();
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
            if (GetMostSignificantBit(this.ac)==1) {
                SetCarryFlag();
            }
            this.ac = (Byte)(data << 1);
            if(this.ac == 0){ this.SetZeroFlag(); }
        }

        public void BCC(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "BCC";
            if (addressingMode != AddressingMode.Relative) { throw new ArgumentException($"Invalid addressing mode: {addressingMode}"); }
            int data = Relative();
            if (GetCarryFlag() == 0) {
                this.pc = (Byte)(this.pc + data);
            }
        }

        public void BCS() {
            this.instructionDetails.Instruction = "BCS";
            int data = Relative();
            if ((this.sr & 0b00000001) == 1) {
                pc = (ushort)(pc + data);
                this.CpuCycleCounter++;
            }
            this.CpuCycleCounter += 2;
        }

        public void BEQ() {
            this.instructionDetails.Instruction = "BEQ";
            int data = Relative();
            if(GetZeroFlag() == 1) {
                this.pc = (Byte)(this.pc + data);
            }
        }

        public void BIT(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "BIT";
            int operAnd;
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.Absolute:
                    operAnd = Absolute();
                    Byte operand = cPUMemoryMap[operAnd];
                    if((this.ac & operand) == 0) { SetZeroFlag(); }
                    this.sp = (Byte)(operand | 0b11000000); // this checks bit 7 and 6 and place the overflow and negative flag. (I don't really understand why, so this could be buggy) 
                    this.CpuCycleCounter += 4;
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
        }

        public void BMI() {
            this.instructionDetails.Instruction = "BMI";
            int operand = Relative();
            if (GetNegativeFlag() == 1) {
                pc = (ushort)(pc + operand);
                this.CpuCycleCounter++;
            }
            this.CpuCycleCounter += 2;
        }

        public void BNE() {
            this.instructionDetails.Instruction = "BNE";
            int operand = Relative();
            if ((this.sr & 0b00000010) == 0) {
                pc = (ushort)(pc + operand);
                this.CpuCycleCounter++;
            }
            this.CpuCycleCounter += 2;
        }

        public void BPL() {
            this.instructionDetails.Instruction = "BPL";
            int operand = Relative();
            if ((this.sr & 0b10000000) == 0) {
                pc = (ushort)(pc + operand);
                this.CpuCycleCounter++;
            }
            this.CpuCycleCounter += 2;

            //Small hack to prevent bootloop at start
            if (this.CpuCycleCounter >= 50) {
                Byte mask = 0b10000000;
                sr = (Byte)(sr ^ mask);
            }
        }

        public void BVC() {
            this.instructionDetails.Instruction = "BVC";
            int operand = Relative();
            if (GetCarryFlag() == 0) {
                pc = (ushort)(pc + operand);
            }
        }

        public void BVS() {
            this.instructionDetails.Instruction = "BVS";
            int operand = Relative();
            if (GetOverFlag() == 1) {
                pc = (ushort)(pc + operand);
            }
        }



        public void CMP(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "CMP";
            int operand;
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    operand = Immediate();                  
                    this.CpuCycleCounter += 2;
                    break;
                case AddressingMode.ZeroPage:
                    operand = ZeroPage();
                    this.CpuCycleCounter += 3;
                    break;
                case AddressingMode.ZeroPageX:
                    operand = ZeroPageX();
                    this.CpuCycleCounter += 4;
                    break;
                case AddressingMode.Absolute:
                    operand = Absolute();
                    this.CpuCycleCounter += 4;
                    break;
                case AddressingMode.AbsoluteX:
                    operand= AbsoluteX();
                    this.CpuCycleCounter += 4; // Bugged
                    break;
                case AddressingMode.AbsoluteY:
                    operand = AbsoluteY();
                    this.CpuCycleCounter += 4; // Bugged
                    break;
                case AddressingMode.XIndirect:
                    operand = XIndirect();
                    this.CpuCycleCounter += 6;
                    break;
                case AddressingMode.IndirectY:
                    operand = IndirectY();
                    this.CpuCycleCounter += 5; //bugged
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }

            // http://www.6502.org/tutorials/compare_instructions.html
            this.sr = (Byte)(sr & 0b10111111); // clears the N-bit (7) in the Status Register. For this operation we start with a 0
            Compare(operand, ref this.ac);
        }

        public void CPX(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "CPX";
            int operand;
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    operand = Immediate();
                    this.CpuCycleCounter += 2;
                    break;
                case AddressingMode.ZeroPage:
                    operand = ZeroPage();
                    this.CpuCycleCounter += 3;
                    break;
                case AddressingMode.Absolute:
                    operand = Absolute();
                    this.CpuCycleCounter += 4;
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
            Compare(operand, ref this.x);
        }

        public void CPY(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "CPY";
            int operand;
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    operand = Immediate();
                    this.CpuCycleCounter += 2;
                    break;
                case AddressingMode.ZeroPage:
                    operand = ZeroPage();
                    this.CpuCycleCounter += 3;
                    break;
                case AddressingMode.Absolute:
                    operand = Absolute();
                    this.CpuCycleCounter += 4;
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
            Compare(operand, ref this.y);
        }

        public void DEC(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "DEC";
            int memory;
            ushort data;
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    memory = ZeroPage();
                    break;
                case AddressingMode.ZeroPageX:
                    memory = ZeroPageX();
                    break;
                case AddressingMode.Absolute:
                    memory = Absolute();
                    break;
                case AddressingMode.AbsoluteX:
                    memory = AbsoluteX();
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }

            data = (ushort)memory;
            data = --data;
            UpdateMemory(addressingMode, data);
        }

        public void DEX() {
            this.instructionDetails.Instruction = "DEX";
            throw new NotImplementedException();
        }

        public void DEY() {
            this.instructionDetails.Instruction = "DEY";
            throw new NotImplementedException();
        }

        public void EOR() {
            this.instructionDetails.Instruction = "EOR";
            throw new NotImplementedException();
        }

        public void INC() {
            this.instructionDetails.Instruction = "INC";
            throw new NotImplementedException();
        }

        public void INX() {
            this.instructionDetails.Instruction = "INX";
            this.x++;
            this.CpuCycleCounter += 2;
        }

        public void INY() {
            this.instructionDetails.Instruction = "INY";
            throw new NotImplementedException();
        }

        public void JMP(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "JMP";
            int operand;
            switch (addressingMode) {
                case AddressingMode.Absolute:
                    operand = Absolute();
                    this.pc = (ushort)(operand - 1);
                    this.CpuCycleCounter += 3;
                    break;
                case AddressingMode.Indirect:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
        }

        public void JSR() {
            this.instructionDetails.Instruction = "JSR";
            throw new NotImplementedException();
        }

        public void LDA(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "LDA";
            int operand;
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    operand = Immediate();
                    this.ac = (Byte)operand;
                    this.CpuCycleCounter += 2;
                    break;
                case AddressingMode.ZeroPage:
                    operand = ZeroPage();
                    this.ac = (Byte)operand;
                    this.CpuCycleCounter += 2;
                    break;
                case AddressingMode.ZeroPageX:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.Absolute:
                    operand = Absolute();
                    this.ac = cPUMemoryMap[operand];
                    this.CpuCycleCounter += 4;
                    break;
                case AddressingMode.AbsoluteX:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.AbsoluteY:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.XIndirect:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.IndirectY:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
        }

        public void LDX(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "LDX";
            int operand;
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    operand = Immediate();
                    this.x = (Byte)operand; // Load next byte into x register
                    this.CpuCycleCounter += 2;
                    break;
                case AddressingMode.ZeroPage:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.ZeroPageY:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.Absolute:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.AbsoluteY:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
        }

        public void LDY() {
            this.instructionDetails.Instruction = "LDY";
            throw new NotImplementedException();
        }

        public void LSR() {
            this.instructionDetails.Instruction = "LSR";
            throw new NotImplementedException();
        }

        public void NOP() {
            this.instructionDetails.Instruction = "NOP";
            throw new NotImplementedException();
        }

        public void ORA() {
            this.instructionDetails.Instruction = "ORA";
            throw new NotImplementedException();
        }

        public void PHA() {
            this.instructionDetails.Instruction = "PHA";
            throw new NotImplementedException();
        }

        public void PHP() {
            this.instructionDetails.Instruction = "PHP";
            throw new NotImplementedException();
        }

        public void PLA() {
            this.instructionDetails.Instruction = "PLA";
            throw new NotImplementedException();
        }

        public void PLP() {
            this.instructionDetails.Instruction = "PLP";
            throw new NotImplementedException();
        }

        public void ROL() {
            this.instructionDetails.Instruction = "ROL";
            throw new NotImplementedException();
        }

        public void ROR() {
            this.instructionDetails.Instruction = "ROR";
            throw new NotImplementedException();
        }

        public void RTI() {
            this.instructionDetails.Instruction = "RTI";
            throw new NotImplementedException();
        }

        public void RTS() {
            this.instructionDetails.Instruction = "RTS";
            throw new NotImplementedException();
        }

        public void SBC() {
            this.instructionDetails.Instruction = "SBC";
            throw new NotImplementedException();
        }

        public void STA(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "STA";
            int operand;
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    operand = ZeroPage();
                    cPUMemoryMap[operand] = ac;
                    this.CpuCycleCounter += 3;
                    break;
                case AddressingMode.ZeroPageX:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.Absolute:
                    operand = Absolute();
                    this.cPUMemoryMap[operand] = this.ac;
                    this.CpuCycleCounter += 4;
                    break;
                case AddressingMode.AbsoluteX:
                    operand = AbsoluteX();
                    cPUMemoryMap[operand] = this.ac;
                    this.CpuCycleCounter += 5;
                    break;
                case AddressingMode.AbsoluteY:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.XIndirect:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.IndirectY:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
        }

        public void STX(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "STX";
            int operAnd;
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.ZeroPageX:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.Absolute:
                    operAnd = Absolute();
                    this.cPUMemoryMap[operAnd] = this.x;
                    this.CpuCycleCounter += 4;
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");

            }
        }
        public void STY() {
            this.instructionDetails.Instruction = "STY";
            throw new NotImplementedException();
        }

        public void TAX() {
            this.instructionDetails.Instruction = "TAX";
            throw new NotImplementedException();
        }

        public void TAY() {
            this.instructionDetails.Instruction = "TAY";
            throw new NotImplementedException();
        }

        public void TSX() {
            this.instructionDetails.Instruction = "TSX";
            throw new NotImplementedException();
        }

        public void TXA() {
            this.instructionDetails.Instruction = "TXA";
            Debug.WriteLine("TXA");
            this.ac = this.x;
            this.CpuCycleCounter += 2;
        }

        public void TXS() {
            this.instructionDetails.Instruction = "TXS";
            Debug.WriteLine("TXS");
            this.sp = this.x;
            this.CpuCycleCounter += 2;
        }

        public void TYA() {
            this.instructionDetails.Instruction = "TYA";
            throw new NotImplementedException();
        }

        #region Helper Functions
        private void SetCarryFlag() { this.sr = (Byte)(this.sr | 0b00000001); }

        private void ClearCarryFlag() { this.sr = (Byte)(this.sr & 0b11111110); }

        private int GetCarryFlag() { return (this.sr & 0b00000001); }

        void SetZeroFlag() { this.sr = (Byte)(this.sr | 0b0000010); }

        public int GetZeroFlag() { return (this.sr & 0b00000010) >> 1; }

        private void SetInterruptDisableFlag() { this.sr = (Byte)(this.sr | 0b00000100); }

        private int GetInterruptDisableFlag() { return (this.sr & 0b00000100) >> 2; }

        private void SetDecimalFlag() { this.sr = (Byte)(this.sr | 0b00001000); }

        // Use SED/CLD  private int GetDecimalFlag() { return (this.sr & 0b00001000) >> 3; }
        
        private void SetBreakFlag() { this.sr = (Byte)(this.sr | 0b00010000); }

        private int GetBreakFlag() { return (this.sr & 0b00010000) >> 4; }

        private void SetOverFlowFlag() { this.sr = (Byte)(this.sr | 0b01000000); }

        private int GetOverFlag() { return (this.sr & 0b01000000) >> 6; }

        private void SetNegativeFlag() { this.sr = (Byte)(this.sr | 0b10000000); }

        private int GetNegativeFlag() { return (this.sr & 0b10000000) >> 7; }

        private int GetMostSignificantBit(Byte x) { return (x & 0b10000000) >> 7; }

        private bool isNegative(Byte input) {
            if (((input & 0b10000000) >> 7) == 1) {
                return true;
            } else {
                return false;
            }
        }

        private int Absolute() {
            byte low = this.cPUMemoryMap[++pc];
            byte high = this.cPUMemoryMap[++pc];
            this.operand = (ushort)(low | (high << 8));
            this.instructionDetails.Operand = $"${operand:X4}";

            return operand;
        }

        //private int AbsoluteX() {
            int operand =this.cPUMemoryMap[++pc] | (this.cPUMemoryMap[++pc] << 8);
            this.instructionDetails.Operand = $"${operand:X4},X";
            Debug.WriteLine($"${operand:X4},X");
            return operand + this.x;
        }

        //private int AbsoluteY() {
            int operand = this.cPUMemoryMap[++pc] | (this.cPUMemoryMap[++pc] << 8);
            this.instructionDetails.Operand = $"${operand:X4},Y";
            Debug.WriteLine($"${operand:X4},Y");
            return operand + this.y;
        }

        private int Immediate() { //Immediate: The value at the rom address equals the the instruction value
            byte low = this.cPUMemoryMap[++pc];
            byte high = 0b0000;
            this.operand = (ushort)(low | (high << 8));
            this.instructionDetails.Operand = $"#{operand:X2}";

            return this.operand;
        }

        private int Relative() {
            sbyte operand = (sbyte)this.cPUMemoryMap[++pc];
            this.instructionDetails.Operand = $"${operand:X2}";
            Debug.WriteLine($"${operand:X2}");
            return operand;
        }

        private int ZeroPage() { //
            byte low = this.cPUMemoryMap[++pc];
            byte high = 0b0000;
            this.operand = (ushort)(low | (high << 8));
            this.instructionDetails.Operand = $"${this.operand:X2}";
            
            int memoryValue = this.cPUMemoryMap[this.operand]; //Gets value from the ZeroPage in RAM
            return memoryValue;
        }

        private int ZeroPageX() {
            byte low = (byte)(this.cPUMemoryMap[++pc] + this.x);
            byte high = 0b0000;
            this.operand = (ushort)(low | (high << 8));
            this.instructionDetails.Operand = $"${this.operand:X2},X";

            int memoryValue = this.cPUMemoryMap[this.operand];
            return memoryValue;
        }

        private int Indirect() {
            byte low = (byte)(this.cPUMemoryMap[++pc]);
            byte high = (byte)(this.cPUMemoryMap[++pc]);
            this.operand = (ushort)(low | (high << 8));
            this.instructionDetails.Operand = $"({operand:X4})";

            //TODO: add operand overflow bug where eg 20FF becomes 2000 instead of 2100
            return this.operand;
        }

        private int XIndirect() {
            byte low = (byte)(this.cPUMemoryMap[++pc + this.x]);
            byte high = (byte)(this.cPUMemoryMap[++pc] + this.x);
            this.operand = (ushort)(low | (high << 8));

            this.instructionDetails.Operand = $"({operand:X2},X)";
            return this.operand;
        }

        //private int IndirectY() {
            int operand = this.cPUMemoryMap[++pc];
            this.instructionDetails.Operand = $"({operand:X2}),Y";
            Debug.WriteLine($"({operand:X2}),Y");
            return (this.cPUMemoryMap[operand] | (this.cPUMemoryMap[++operand] << 8)) + y;
        }

        //private void UpdateMemory(AddressingMode addressingMode, int memoryData) {
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    this.cPUMemoryMap[this.operand] = (byte)memoryData;
                    break;
                case AddressingMode.ZeroPageX:
                    this.cPUMemoryMap[this.operand+this.x] = (byte)memoryData;
                    break;
                case AddressingMode.Absolute:
                    throw new NotImplementedException();
                case AddressingMode.AbsoluteX:
                    throw new NotImplementedException();
                case AddressingMode.AbsoluteY:
                    throw new NotImplementedException();
                case AddressingMode.XIndirect:
                    throw new NotImplementedException();
                case AddressingMode.IndirectY:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }

        }

        private void Compare(int Operand, ref Byte CompareReference) {
            int testData = CompareReference - Operand;
            int negativeBit = (testData | 0b01000000); // determine the 7th bit (negative flag) of the operand data.
            if (CompareReference < testData) {
                this.sr = (Byte)(sr | negativeBit); // set the N-Bit with the result of the 
            } else if (CompareReference == testData) {
                this.sr = (Byte)(sr | 0b00000011); //Sets the zero and carry bit to 1. the N-bit stays 0
            } else if (CompareReference > testData) {
                this.sr = (Byte)(sr | 0b00000010); //Sets the zero bit to 1. the N-bit stays gets
                this.sr = (Byte)(sr | negativeBit); // set the N-Bit with the result of the
            } else {
                throw new Exception("How?");
            }
        }
        #endregion
    }
}
