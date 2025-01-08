using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xaml.Schema;

namespace NES.CPU {
    internal class Ricoh2A03 : NES.CPU.IMOS6502 {
        private NES.RAM.Memory memory;

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

        // Variables
        private NES.Console.Cartridge cartridge;
        private NES.Cartridge.PRGROM currentPRGROMBank;

        private bool brk; // temporary boolean we will use during programming

        public Ricoh2A03(NES.Console.Cartridge cartridge) {
            this.cartridge = cartridge;
            memory = new RAM.Memory();
            sr = 0b00100000; // bit 5 has no name and is always set to 1
            this.Reset();

            brk = false;
            this.Run();
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

        public void ExecuteInstruction(int instruction) {
            switch (instruction) {
                case 0x00: BRK(); break;
                case 0x01: ORA(); break;
                case 0x78: SEI(); break;
                case 0x8e: STX(AddressingMode.Absolute); break;
                case 0xa2: LDX(AddressingMode.Immediate); break;
                case 0xa9: LDA(); break;
                case 0xd8: CLD(); break;
                default: throw new NotImplementedException(); break;
            }
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
            pc = 0;
        }

        public void Run() {
            this.currentPRGROMBank = this.cartridge.PRGROMBanks[0]; // Load the first bank
            while (!brk){
                ExecuteInstruction(currentPRGROMBank.Data[pc]);

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
            Byte mask = 0b00000001;
            sr = (Byte)(sr ^ mask);
        }
        
        public void CLC() { // Set the carry flag to 0
            Byte mask = 0b11111110;
            sr = (Byte)(sr & mask);
            pc++;
        }

        public void SEI() { // Set Interrupt Disable flag to 1
            Byte mask = 0b00000100;
            sr = (Byte)(sr ^ mask);
            pc++;
        }
        public void CLI() { // Set Interrupt Disable flag to 0
            Byte mask = 0b11111011;
            sr = (Byte)(sr & mask);
        }

        public void SED() { // Set Decimal Mode flag to 1
            Byte mask = 0b00001000;
            sr = (Byte)(sr ^ mask);
        }
        public void CLD() { // Set Decimal Mode flag to 0
            Byte mask = 0b11110111;
            sr = (Byte)(sr & mask);
            pc++;
        }

        public void BRK() { // Set the Break Command flag to 1
            Byte mask = 0b00010000;
            sr = (Byte)(sr ^ mask);
        }
        #endregion
        public void ADC(NES.CPU.AddressingMode addressingMode, Byte Operand) {
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    break;
                case AddressingMode.ZeroPage:
                    break;
                case AddressingMode.ZeroPageX:
                    break;
                case AddressingMode.Absolute:
                    break;
                case AddressingMode.AbsoluteX:
                    break;
                case AddressingMode.AbsoluteY:
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
            throw new NotImplementedException();
        }

        public void BEQ() {
            throw new NotImplementedException();
        }

        public void BIT() {
            throw new NotImplementedException();
        }

        public void BMI() {
            throw new NotImplementedException();
        }

        public void BNE() {
            throw new NotImplementedException();
        }

        public void BPL() {
            throw new NotImplementedException();
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

        public void CMP() {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void INY() {
            throw new NotImplementedException();
        }

        public void JMP() {
            throw new NotImplementedException();
        }

        public void JSR() {
            throw new NotImplementedException();
        }

        public void LDA() {
            throw new NotImplementedException();
        }

        public void LDX(NES.CPU.AddressingMode addressingMode) {
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    this.x = currentPRGROMBank.Data[++pc]; // Load next byte into x register
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
            pc++; //Move to the programm counter to the next instruction
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

        public void STA() {
            throw new NotImplementedException();
        }

        public void STX(NES.CPU.AddressingMode addressingMode) {
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.ZeroPageX:
                    throw new NotImplementedException();
                    break;
                case AddressingMode.Absolute:
                    int MemoryAddress = this.currentPRGROMBank.Data[++pc] | (this.currentPRGROMBank.Data[++pc] << 8);
                    this.memory.Data[MemoryAddress] = this.x;
                    break;
                default:
                    throw new ArgumentException($"Invalid addressing mode: {addressingMode}");
            }
            pc++; //Move to the programm counter to the next instruction
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
            throw new NotImplementedException();
        }

        public void TXS() {
            throw new NotImplementedException();
        }

        public void TYA() {
            throw new NotImplementedException();
        }

    }
}
