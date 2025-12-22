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
        private NES.CPU.CPUMemoryMap cpuMemoryMap;

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


        
        //public event Instrct

        // Variables
        private NES.Console.Cartridge cartridge;
        

        private bool brk; // temporary boolean we will use during programming

        
        public event InstructionExecutedEventHandler InstructionExecuted;
        private InstructionEventArgs instructionDetails;

        public Ricoh2A03(NES.CPU.CPUMemoryMap cpuMemoryMap) {
            this.cpuMemoryMap = cpuMemoryMap;

            instructionDetails = new InstructionEventArgs();
            sr = 0b00100000; // bit 5 has no name and is always set to 1
            //this.Reset();
            CpuCycleCounter = 0;
            brk = false;
            sp = 0xff;
            pc = (ushort)(this.cpuMemoryMap[0xfffc] | (this.cpuMemoryMap[0xfffd] << 8)); // Mapper 0
            //this.Run();
        }

        internal void Demo() {
            DEC(AddressingMode.ZeroPage);
        }



        public void ExecuteInstruction(int opcodes) {
            instructionDetails.ProgramCounter = $"${this.pc:X}";
            instructionDetails.Opcode = $"{opcodes:X}";
            switch (opcodes) {
                case 0x00: BRK(); break;
                case 0x01: ORA(AddressingMode.XIndirect); break;
                case 0x05: ORA(AddressingMode.ZeroPage); break;
                case 0x06: ASL(AddressingMode.ZeroPage); break;
                case 0x08: PHP(); break;
                case 0x09: ORA(AddressingMode.Immediate); break;
                case 0x0a: ASL(AddressingMode.Accumulator); break;
                case 0x0d: ORA(AddressingMode.Absolute); break;
                case 0x0e: ASL(AddressingMode.Absolute); break;
                case 0x10: BPL(); break;
                case 0x11: ORA(AddressingMode.IndirectY); break;
                case 0x15: ORA(AddressingMode.ZeroPageX); break;
                case 0x16: ASL(AddressingMode.ZeroPageX); break;
                case 0x19: ORA(AddressingMode.AbsoluteY); break;
                case 0x1d: ORA(AddressingMode.AbsoluteX); break;
                case 0x1e: ASL(AddressingMode.AbsoluteX); break;
                case 0x21: AND(AddressingMode.XIndirect); break;
                case 0x25: AND(AddressingMode.ZeroPage); break;
                case 0x28: PLP(); break;
                case 0x29: AND(AddressingMode.Immediate); break;
                case 0x30: BMI(); break;
                case 0x2c: BIT(AddressingMode.Absolute); break;
                case 0x2d: AND(AddressingMode.Absolute); break;
                case 0x31: AND(AddressingMode.IndirectY); break;
                case 0x35: AND(AddressingMode.ZeroPageX); break;
                case 0x39: AND(AddressingMode.AbsoluteY); break;
                case 0x3d: AND(AddressingMode.AbsoluteX); break;
                case 0x46: LSR(AddressingMode.ZeroPage); break;
                case 0x4a: LSR(AddressingMode.Accumulator); break;
                case 0x4c: JMP(AddressingMode.Absolute); break;
                case 0x4e: LSR(AddressingMode.Absolute); break;
                case 0x48: PHA(); break;
                case 0x50: BVC(); break;
                case 0x56: LSR(AddressingMode.ZeroPageX); break;
                case 0x5e: LSR(AddressingMode.Accumulator); break;
                case 0x61: ADC(AddressingMode.XIndirect); break;
                case 0x65: ADC(AddressingMode.ZeroPage); break;
                case 0x68: PLA(); break;
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
                case 0xa0: LDY(AddressingMode.Immediate); break;
                case 0xa1: LDA(AddressingMode.IndirectY); break;
                case 0xa2: LDX(AddressingMode.Immediate); break;
                case 0xa4: LDY(AddressingMode.ZeroPage); break;
                case 0xa5: LDA(AddressingMode.ZeroPage); break;
                case 0xa9: LDA(AddressingMode.Immediate); break;
                case 0xad: LDA(AddressingMode.Absolute); break;
                case 0xac: LDY(AddressingMode.Absolute); break;
                case 0xb0: BCS(); break;
                case 0xb1: LDA(AddressingMode.IndirectY); break;
                case 0xb4: LDY(AddressingMode.ZeroPageX); break;
                case 0xb5: LDA(AddressingMode.ZeroPageX); break;
                case 0xb8: CLV(); break;
                case 0xb9: LDA(AddressingMode.AbsoluteY); break;
                case 0xbc: LDY(AddressingMode.AbsoluteX); break;
                case 0xbd: LDA(AddressingMode.AbsoluteX); break;
                case 0xc1: CMP(AddressingMode.XIndirect); break;
                case 0xc5: CMP(AddressingMode.ZeroPage); break;
                case 0xc6: DEC(AddressingMode.ZeroPage); break;
                case 0xc9: CMP(AddressingMode.Immediate); break;
                case 0xca: DEX(); break;
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
                case 0xea: NOP(); break;
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
            pc = (ushort)(this.cpuMemoryMap[0xfffc] | (this.cpuMemoryMap[0xfffd] << 8));
                //cPUMemoryMap[0xfffc];
        }

        public void Step() {
            ExecuteInstruction(cpuMemoryMap[pc]);
            InstructionExecuted(this, instructionDetails);
            instructionDetails.Clear();
            //brk = true;
            // to check Status Register functions operand bytes

            //do {
            //    pc = 65535;
            //    pc++;
            //} while (pc < 65535); Should never happen I think thanks to bank switching...

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
                    Byte operand = cpuMemoryMap[operAnd];
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
            this.x = --this.x ;
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
            this.y++;
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
            int data;
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    data = Immediate();
                    this.ac = (Byte)data;
                    this.CpuCycleCounter += 2;
                    break;
                case AddressingMode.ZeroPage:
                    data = ZeroPage();
                    this.ac = (Byte)data;
                    this.CpuCycleCounter += 2;
                    break;
                case AddressingMode.ZeroPageX:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.Absolute:
                    data = Absolute();
                    this.ac = cpuMemoryMap[data];
                    this.CpuCycleCounter += 4;
                    break;
                case AddressingMode.AbsoluteX:
                    data = AbsoluteX();
                    this.ac = cpuMemoryMap[data];
                    break;
                case AddressingMode.AbsoluteY:
                    data = AbsoluteY();
                    this.ac = cpuMemoryMap[data];
                    break;
                case AddressingMode.XIndirect:
                    data = XIndirect();
                    this.ac = cpuMemoryMap[data];
                    break;
                case AddressingMode.IndirectY:
                    data = IndirectY();
                    this.ac = cpuMemoryMap[data];
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
        }

        public void LDX(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "LDX";
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    this.x = (Byte)Immediate(); // Load next byte into x register
                    this.CpuCycleCounter += 2;
                    break;
                case AddressingMode.ZeroPage:
                    this.x = (Byte)ZeroPage();
                    break;
                case AddressingMode.ZeroPageY:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.Absolute:
                    this.x = (Byte)Absolute();
                    break;
                case AddressingMode.AbsoluteY:
                    this.x = (Byte)AbsoluteY();
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
        }

        public void LDY(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "LDY";
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    this.y = (Byte)Immediate(); // Load next byte into x register
                    this.CpuCycleCounter += 2;
                    break;
                case AddressingMode.ZeroPage:
                    this.y = (Byte)ZeroPage();
                    break;
                case AddressingMode.ZeroPageY:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.Absolute:
                    this.y = (Byte)Absolute();
                    break;
                case AddressingMode.AbsoluteY:
                    this.y = (Byte)AbsoluteY();
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
        }

        public void LSR(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "LSR";
            int data;
            switch (addressingMode) {
                case AddressingMode.Accumulator:
                    data = this.ac;
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
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }

            //First set the carry flag based on the lsb:
            if((data & 0b00000001) == 1) {
                SetCarryFlag();
            } else {
                ClearCarryFlag();
            }

            //Next perform the left shift
            data = data >> 1;

            //Finaly we will save the value
            UpdateMemory(addressingMode, data);
        }

        public void NOP() {
            this.instructionDetails.Instruction = "NOP";
            // Does nothing
        }

        public void ORA(NES.CPU.AddressingMode addressingMode) {
            this.instructionDetails.Instruction = "ORA";
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
                    data = IndirectY();
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }

            this.ac = (Byte)(ac | data);
        }

        public void PHA() {
            this.instructionDetails.Instruction = "PHA";
            this.operand = (ushort)(sp-- | (0b0001 << 8));
            UpdateMemory(AddressingMode.Immediate, this.ac);
        }

        public void PHP() {
            this.instructionDetails.Instruction = "PHP";
            this.operand = (ushort)(sp-- | (0b0001 << 8));
            UpdateMemory(AddressingMode.Immediate, this.pc);
        }

        public void PLA() {
            this.instructionDetails.Instruction = "PLA";
            this.operand = (ushort)(++sp | (0b0001 << 8));
            ac = cpuMemoryMap[this.operand];
        }

        public void PLP() {
            this.instructionDetails.Instruction = "PLP";
            this.operand = (ushort)(++sp | (0b0001 << 8));
            this.pc = cpuMemoryMap[this.operand];
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
                    cpuMemoryMap[operand] = ac;
                    this.CpuCycleCounter += 3;
                    break;
                case AddressingMode.ZeroPageX:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.Absolute:
                    operand = Absolute();
                    this.cpuMemoryMap[operand] = this.ac;
                    this.CpuCycleCounter += 4;
                    break;
                case AddressingMode.AbsoluteX:
                    operand = AbsoluteX();
                    cpuMemoryMap[operand] = this.ac;
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
                    this.cpuMemoryMap[operAnd] = this.x;
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
            this.ac = this.x;
            this.CpuCycleCounter += 2;
            //this.pc++;
        }

        public void TXS() {
            this.instructionDetails.Instruction = "TXS";
            this.sp = this.x;
            this.CpuCycleCounter += 2;
            //this.pc++;
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

        private int Immediate() {
            //Immediate: The value at the rom address equals the the instruction value.
            //E.g LDA #07 Loads 07 into AC
            byte low = this.cpuMemoryMap[++pc];
            byte high = 0b0000;
            this.operand = (ushort)(low | (high << 8));
            this.instructionDetails.Operand = $"#{operand:X2}";
            return this.operand;
        }

        private int Absolute() {
            //Absolute provides a 16-bit address from a memory location
            //E.g. LDA $1020 loads the data @ address $1020 to AC (e.g. FF)
            byte low = this.cpuMemoryMap[++pc];
            byte high = this.cpuMemoryMap[++pc];
            this.operand = (ushort)(low | (high << 8));
            this.instructionDetails.Operand = $"${operand:X4}";
            return this.cpuMemoryMap[this.operand];
        }

        private int AbsoluteX() {
            //AbsoluteX is similar to Absolute, but adds the value from the x-register to the operand to define the address
            //E.g. if X contains 15 and we run the instruction LDA $1020, than the data will be loaded from $1035 to the AC
            byte low = this.cpuMemoryMap[++pc];
            byte high = this.cpuMemoryMap[++pc];
            this.operand = (ushort)(low | (high << 8));

            this.instructionDetails.Operand = $"${operand:X4},X";
            return this.cpuMemoryMap[this.operand + this.x];
        }

        private int AbsoluteY() {
            //AbsoluteY is similar to Absolute, but adds the value from the x-register to the operand to define the address
            //E.g. if Y contains 15 and we run the instruction LDA $1020, than the data will be loaded from $1035 to the AC
            byte low = this.cpuMemoryMap[++pc];
            byte high = this.cpuMemoryMap[++pc];
            this.operand = (ushort)(low | (high << 8));

            this.instructionDetails.Operand = $"${operand:X4},Y";
            return this.cpuMemoryMap[operand + this.y];
        }

        private int ZeroPage() {
            //ZeroPage is similar to Absolute, but loads from the zeropage.
            //Because this is in the 256 range of the memory map. the high-byte is 00 and dont require an increment of the program counter.
            //This makes this location "faster" to access as it require one CPU cycle less
            //E.g. LDA $10 loads the data @ address $0010 to the AC
            byte low = this.cpuMemoryMap[++pc];
            byte high = 0b0000;
            this.operand = (ushort)(low | (high << 8));
            this.instructionDetails.Operand = $"${this.operand:X2}";
            return this.cpuMemoryMap[this.operand];
        }
        private int ZeroPageX() {
            //ZeroPageX is similar to ZeroPage in combination with AbsoluteX where the X register data is added to the operand
            //E.g. if X contains the value 15, then LDA $10,X loads the data from $0025 into the AC
            byte low = this.cpuMemoryMap[++pc];
            byte high = 0b0000;
            this.operand = (ushort)(low | (high << 8));
            this.instructionDetails.Operand = $"${this.operand:X2},X";
            return this.cpuMemoryMap[this.operand + this.x];
        }

        private int Relative() {
            sbyte operand = (sbyte)this.cpuMemoryMap[++pc];
            this.instructionDetails.Operand = $"${operand:X2}";
            return operand;
        }

        private int Indirect() {
            //Indirect, uses the content of the address and the next adress to find the effective address.
            //Note: only JMP uses this function
            //E.g. JMP ($1020). If Address $1020 has value FF and adress $1021 has 15. Then the program counter wil become 15FF
            byte low = (byte)(this.cpuMemoryMap[++pc]);
            byte high = (byte)(this.cpuMemoryMap[++pc]);
            this.operand = (ushort)(low | (high << 8));
            this.instructionDetails.Operand = $"({operand:X4})";
            
            low = (byte)(this.cpuMemoryMap[this.operand]);
            high = (byte)(this.cpuMemoryMap[this.operand + 1]);
            //TODO: add operand overflow bug where eg 20FF becomes 2000 instead of 2100
            return (low | (high << 8));
        }

        private int XIndirect() {
            //XIndirect, is a combination of ZeroPageX and Indirect.
            //Once the "operand" is formed using the same method as ZeroPageX, a lookup is performed like Indirect
            //E.g. if X contains the value 15, then LDA ($10,X) gets the address is looked up in $0025 and $0026.
            //If $0025 contains 20 and $0026 contains 10. Then the data from $1020 will be loaded into the AC
            byte low = (byte)(this.cpuMemoryMap[++pc + this.x]);
            byte high = 0b0000;
            this.operand = (ushort)(low | (high << 8));
            this.instructionDetails.Operand = $"({operand:X2},X)";

            low = (byte)(this.cpuMemoryMap[this.operand] + this.x);
            high = (byte)(this.cpuMemoryMap[this.operand + this.x + 1]);
            return this.cpuMemoryMap[low | (high << 8)];
        }

        private int IndirectY() {
            //IndirectY is similar to XIndirect, with that difference that the X-register isn't used and the value in the Y-register
            //is added to the address of the Indirect part.
            //E.g. If Y contains the value 15 then LDA($10),Y will lookup the values from $0010 and $0011.
            //If $0010 contains 20 and $0011 contains 10. Then 15 (Y-Value) is added to $1020, loading the content from $1035 into the AC
            byte low = (byte)(this.cpuMemoryMap[++pc]);
            byte high = 0b0000;
            this.operand = (ushort)(low | (high << 8));
            this.instructionDetails.Operand = $"({operand:X2}),Y";
            low = (byte)(this.cpuMemoryMap[this.operand]);
            high = (byte)(this.cpuMemoryMap[this.operand + 1]);
            return this.cpuMemoryMap[(low | (high << 8)) + 1];
        }

        private void UpdateMemory(AddressingMode addressingMode, int memoryData) {
            switch (addressingMode) {
                case AddressingMode.Accumulator:
                    this.ac = (Byte)memoryData;
                    break;
                case AddressingMode.ZeroPage:
                    this.cpuMemoryMap[this.operand] = (byte)memoryData;
                    break;
                case AddressingMode.ZeroPageX:
                    this.cpuMemoryMap[this.operand+this.x] = (byte)memoryData;
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
