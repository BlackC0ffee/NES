using Microsoft.VisualBasic;
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

        private ulong CpuCycleCounter;

        private NES.CPU.CPUMemoryMap cPUMemoryMap;

        // Variables
        private NES.Console.Cartridge cartridge;
        

        private bool brk; // temporary boolean we will use during programming

        public Ricoh2A03(NES.Console.Cartridge cartridge) {
            this.cartridge = cartridge;
            cPUMemoryMap = new CPUMemoryMap(this.cartridge);
            sr = 0b00100000; // bit 5 has no name and is always set to 1
            //this.Reset();
            CpuCycleCounter = 0;
            brk = false;
            pc = (ushort)(this.cPUMemoryMap[0xfffc] | (this.cPUMemoryMap[0xfffd] << 8)); // Mapper 0
            //this.Run();
        }

        internal void Demo() {
            SEC();
            SED();
            SEI();
            BRK();
            Debug.WriteLine(Convert.ToString(sr, toBase: 2));
            CLC();
            Debug.WriteLine(Convert.ToString(sr, toBase: 2));
            CLI();
            Debug.WriteLine(Convert.ToString(sr, toBase: 2));
            CLD();
            Debug.WriteLine(Convert.ToString(sr, toBase: 2));
        }

        public void ExecuteInstruction(int opcodes) {
            Debug.Write($"${this.pc:X}: "); // Writes the current Programcounter
            switch (opcodes) {
                case 0x00: BRK(); break;
                case 0x01: ORA(); break;
                case 0x10: BPL(); break;
                case 0x2c: BIT(AddressingMode.Absolute); break;
                case 0x4c: JMP(AddressingMode.Absolute); break;
                case 0x78: SEI(); break;
                case 0x85: STA(AddressingMode.ZeroPage); break;
                case 0x8a: TXA(); break;
                case 0x8d: STA(AddressingMode.Absolute); break;
                case 0x8e: STX(AddressingMode.Absolute); break;
                case 0x95: STA(AddressingMode.ZeroPage); break;
                case 0x9d: STA(AddressingMode.AbsoluteX); break;
                case 0x9a: TXS(); break;
                case 0xa2: LDX(AddressingMode.Immediate); break;
                case 0xa5: LDA(AddressingMode.ZeroPage); break;
                case 0xa9: LDA(AddressingMode.Immediate); break;
                case 0xad: LDA(AddressingMode.Absolute); break;
                case 0xb0: BCS(); break;
                case 0xc9: CMP(AddressingMode.Immediate); break;
                case 0xd0: BNE(); break;
                case 0xd8: CLD(); break;
                case 0xe8: INX(); break;
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
            Debug.WriteLine("SEC");
            Byte mask = 0b00000001;
            sr = (Byte)(sr ^ mask);
            this.CpuCycleCounter += 2;
        }
        
        public void CLC() { // Set the carry flag to 0
            Debug.WriteLine("CLC");
            Byte mask = 0b11111110;
            sr = (Byte)(sr & mask);
            this.CpuCycleCounter += 2;
        }

        public void SEI() { // Set Interrupt Disable flag to 1
            Debug.WriteLine("SEI");
            Byte mask = 0b00000100;
            sr = (Byte)(sr ^ mask);
            this.CpuCycleCounter += 2;
        }
        public void CLI() { // Set Interrupt Disable flag to 0
            Debug.WriteLine("CLI");
            Byte mask = 0b11111011;
            sr = (Byte)(sr & mask);
            this.CpuCycleCounter += 2;
        }

        public void SED() { // Set Decimal Mode flag to 1
            Debug.WriteLine("SED");
            Byte mask = 0b00001000;
            sr = (Byte)(sr ^ mask);
            this.CpuCycleCounter += 2;
        }
        public void CLD() { // Set Decimal Mode flag to 0
            Debug.WriteLine("CLD");
            Byte mask = 0b11110111;
            sr = (Byte)(sr & mask);
            this.CpuCycleCounter += 2;
        }

        public void BRK() { // Set the Break Command flag to 1
            Debug.WriteLine("BRK");
            Byte mask = 0b00010000;
            sr = (Byte)(sr ^ mask);
            this.CpuCycleCounter += 7;
        }
        #endregion
        public void ADC(NES.CPU.AddressingMode addressingMode, Byte Operand) {
            Debug.Write("ADC ");
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
                    break;
                case AddressingMode.IndirectY:
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
        }

        public void AND() {
            throw new NotImplementedException();
        }

        public void ASL() {
            throw new NotImplementedException();
        }

        public void BCC() {
            throw new NotImplementedException();
        }

        public void BCS() {
            Debug.Write("BCS ");
            int operand = Relative();
            if ((this.sr & 0b00000001) == 1) {
                pc = (ushort)(pc + operand);
                this.CpuCycleCounter++;
            }
            this.CpuCycleCounter += 2;
        }

        public void BEQ() {
            throw new NotImplementedException();
        }

        public void BIT(NES.CPU.AddressingMode addressingMode) {
            Debug.Write("BIT ");
            int operAnd;
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.Absolute:
                    operAnd = Absolute();
                    Byte operand = cPUMemoryMap[operAnd];
                    if((this.ac & operand) == 0) { SetZero(); }
                    this.sp = (Byte)(operand | 0b11000000); // this checks bit 7 and 6 and place the overflow and negative flag. (I don't really understand why, so this could be buggy) 
                    this.CpuCycleCounter += 4;
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
        }

        public void BMI() {
            throw new NotImplementedException();
        }

        public void BNE() {
            Debug.Write("BNE ");
            int operand = Relative();
            if ((this.sr & 0b00000010) == 0) {
                pc = (ushort)(pc + operand);
                this.CpuCycleCounter++;
            }
            this.CpuCycleCounter += 2;
        }

        public void BPL() {
            Debug.Write("BPL ");
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
            throw new NotImplementedException();
        }

        public void BVS() {
            throw new NotImplementedException();
        }

        public void CLV() {
            throw new NotImplementedException();
        }

        public void CMP(NES.CPU.AddressingMode addressingMode) {
            Debug.Write("CMP ");
            int data;
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    data = Immediate();                  
                    this.CpuCycleCounter += 2;
                    break;
                case AddressingMode.ZeroPage:
                    data = ZeroPage();
                    this.CpuCycleCounter += 3;
                    break;
                case AddressingMode.ZeroPageX:
                    
                    throw new NotImplementedException();
                    break;
                case AddressingMode.Absolute:
                    throw new NotImplementedException();
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

            // http://www.6502.org/tutorials/compare_instructions.html
            this.sr = (Byte)(sr & 0b10111111); // clears the N-bit (7) in the Status Register. For this operation we start with a 0
            int n = (data | 0b01000000); // determine the 7th bit (negative flag) of the operand data.
            if (this.ac < data) {
                this.sr = (Byte)(sr | n); // set the N-Bit with the result of the 
            } else if (this.ac == data) {
                this.sr = (Byte)(sr | 0b00000011); //Sets the zero and carry bit to 1. the N-bit stays 0
            } else if (this.ac > data) {
                this.sr = (Byte)(sr | 0b00000010); //Sets the zero bit to 1. the N-bit stays gets
                this.sr = (Byte)(sr | n); // set the N-Bit with the result of the
            } else {
                throw new Exception("How?");
            }
        }

        public void CPX() {
            throw new NotImplementedException();
        }

        public void CPY() {
            throw new NotImplementedException();
        }

        public void DEC() {
            throw new NotImplementedException();
        }

        public void DEX() {
            throw new NotImplementedException();
        }

        public void DEY() {
            throw new NotImplementedException();
        }

        public void EOR() {
            throw new NotImplementedException();
        }

        public void INC() {
            throw new NotImplementedException();
        }

        public void INX() {
            Debug.WriteLine("INX");
            this.x++;
            this.CpuCycleCounter += 2;
        }

        public void INY() {
            throw new NotImplementedException();
        }

        public void JMP(NES.CPU.AddressingMode addressingMode) {
            Debug.Write("JMP ");
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
            throw new NotImplementedException();
        }

        public void LDA(NES.CPU.AddressingMode addressingMode) {
            Debug.Write("LDA ");
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
            Debug.Write("LDX ");
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
            throw new NotImplementedException();
        }

        public void LSR() {
            throw new NotImplementedException();
        }

        public void NOP() {
            throw new NotImplementedException();
        }

        public void ORA() {
            throw new NotImplementedException();
        }

        public void PHA() {
            throw new NotImplementedException();
        }

        public void PHP() {
            throw new NotImplementedException();
        }

        public void PLA() {
            throw new NotImplementedException();
        }

        public void PLP() {
            throw new NotImplementedException();
        }

        public void ROL() {
            throw new NotImplementedException();
        }

        public void ROR() {
            throw new NotImplementedException();
        }

        public void RTI() {
            throw new NotImplementedException();
        }

        public void RTS() {
            throw new NotImplementedException();
        }

        public void SBC() {
            throw new NotImplementedException();
        }

        public void STA(NES.CPU.AddressingMode addressingMode) {
            Debug.Write("STA ");
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
            Debug.Write("STX ");
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
            throw new NotImplementedException();
        }

        public void TAX() {
            throw new NotImplementedException();
        }

        public void TAY() {
            throw new NotImplementedException();
        }

        public void TSX() {
            throw new NotImplementedException();
        }

        public void TXA() {
            Debug.WriteLine("TXA");
            this.ac = this.x;
            this.CpuCycleCounter += 2;
        }

        public void TXS() {
            Debug.WriteLine("TXS");
            this.sp = this.x;
            this.CpuCycleCounter += 2;
        }

        public void TYA() {
            throw new NotImplementedException();
        }

        #region Helper Functions
        void SetZero() {
            Byte mask = 0b0000010;
            this.sr = (Byte)(this.sr | mask);
        }

        private int Absolute() {
            int operand = this.cPUMemoryMap[++pc] | (this.cPUMemoryMap[++pc] << 8);
            Debug.WriteLine($"${operand:X4}");
            return operand;
        }

        private int AbsoluteX() {
            int operand =this.cPUMemoryMap[++pc] | (this.cPUMemoryMap[++pc] << 8);
            Debug.WriteLine($"${operand:X4},X");
            return operand + this.x;
        }

        private int AbsoluteY() {
            int operand = this.cPUMemoryMap[++pc] | (this.cPUMemoryMap[++pc] << 8);
            Debug.WriteLine($"${operand:X4},Y");
            return operand + this.y;
        }

        private int Immediate() {
            int operand = cPUMemoryMap[++pc];
            Debug.WriteLine($"#{operand:X2}");
            return operand;
        }

        private int Relative() {
            sbyte operand = (sbyte)this.cPUMemoryMap[++pc];
            Debug.WriteLine($"${operand:X2}");
            return operand;
        }

        private int ZeroPage() {
            int operand = this.cPUMemoryMap[++pc];
            Debug.WriteLine($"${operand:X2}");
            return (0b00000000 | operand);
        }

        private int ZeroPageX() {
            Byte operand = this.cPUMemoryMap[++pc];
            Debug.WriteLine($"${operand:X2},X");
            return (0b00000000 | (Byte)(operand + this.x));
        }

        private int Indirect() {
            int operand = this.cPUMemoryMap[++pc] | (this.cPUMemoryMap[++pc] << 8);
            Debug.WriteLine($"({operand:X4})");
            //TODO: add operand overflow bug where eg 20FF becomes 2000 instead of 2100
            return this.cPUMemoryMap[operand] | (this.cPUMemoryMap[++operand] << 8);
        }

        private int XIndirect() {
            int operand = this.cPUMemoryMap[++pc];
            Debug.WriteLine($"({operand:X2},X)");
            int zeroPageAdr = operand + x;
            return this.cPUMemoryMap[zeroPageAdr] | (this.cPUMemoryMap[++zeroPageAdr] << 8);
        }

        private int IndirectY() {
            int operand = this.cPUMemoryMap[++pc];
            Debug.WriteLine($"({operand:X2}),Y");
            return (this.cPUMemoryMap[operand] | (this.cPUMemoryMap[++operand] << 8)) + y;
        }
        #endregion
    }
}
