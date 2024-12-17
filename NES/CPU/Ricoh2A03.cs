using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NES.CPU
{
    internal class Ricoh2A03 : NES.CPU.IMOS6502
    {
        public Ricoh2A03() {
            
        }
        public void Reset() { //Runs the reset program based on https://www.nesdev.org/wiki/Init_code
            SEI(); // Ignore IRQs
            CLC(); // Disable decimal mode
            LDX("#$40");
            STX("$4017"); // Disable APU frame IRQ
            LDX("#$ff");
            TXS(); // Set up stack
            INX(); //now X = 0
            STX("$2000"); // Disable NMI
            STX("$2001"); // Disable rendering
            STX("$4010"); // Disable DMC IRQs

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

        public void BRK() {
            throw new NotImplementedException();
        }

        public void BVC() {
            throw new NotImplementedException();
        }

        public void BVS() {
            throw new NotImplementedException();
        }

        public void CLC() {
            throw new NotImplementedException();
        }

        public void CLD() {
            throw new NotImplementedException();
        }

        public void CLI() {
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

        public void SEC() {
            throw new NotImplementedException();
        }

        public void SED() {
            throw new NotImplementedException();
        }

        public void SEI() {
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
