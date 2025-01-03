﻿using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NES.CPU {
    internal class Ricoh2A03 : NES.CPU.IMOS6502 {
        private NES.RAM.Memory memory;

        // Instruction set shizzle
        //     https://www.masswerk.at/6502/6502_instruction_set.html
        //     http://www.6502.org/users/obelisk/6502/reference.html
        
        // Registers
        private Int16 pc; // program counter
        private Byte ac; // accumulator
        private Byte x; // X register
        private Byte y; // Y register
        private Byte sr; // status register [NV-BDIZC]
        private Byte sp; // stack pointer

        public Ricoh2A03() {
            memory = new RAM.Memory();
            sr = 0b00100000; // bit 5 has no name and is always set to 1
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
                case 0xa9: LDA(); break;
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
        }

        public void SEI() { // Set Interrupt Disable flag to 1
            Byte mask = 0b00000100;
            sr = (Byte)(sr ^ mask);
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
        }

        public void BRK() { // Set the Break Command flag to 1
            Byte mask = 0b00010000;
            sr = (Byte)(sr ^ mask);
        }
        #endregion
        public void ADC() {
            throw new NotImplementedException();
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

        public void LDX() {
            throw new NotImplementedException();
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

        public void STX() {
            throw new NotImplementedException();
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
