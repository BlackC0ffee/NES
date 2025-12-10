using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NES.CPU
{
    public enum AddressingMode { //https://wiki.cdot.senecapolytechnic.ca/wiki/6502_Addressing_Modes#:~:text=The%206502%20processor%20has%2013%20Addressing%20Modes%2C%20which,load%20the%20accumulator%20from%20memory%20address%200x8005%20%2832773%29.
        Accumulator,
        Absolute,
        AbsoluteX,
        AbsoluteY,
        Immediate,
        Implied,
        Indirect,
        XIndirect,
        IndirectY,
        Relative,
        ZeroPage,
        ZeroPageX,
        ZeroPageY
    }
    internal interface IMOS6502
    {
        void Reset();
        void ADC(NES.CPU.AddressingMode addressingMode); //add with carry
        void AND(NES.CPU.AddressingMode addressingMode); //and(with accumulator)
        void ASL(NES.CPU.AddressingMode addressingMode); //arithmetic shift left
        void BCC(NES.CPU.AddressingMode addressingMode); //branch on carry clear
        void BCS(); //branch on carry set
        void BEQ(); //branch on equal(zero set)
        void BIT(NES.CPU.AddressingMode addressingMode); //bit test
        void BMI(); //branch on minus(negative set)
        void BNE(); //branch on not equal(zero clear)
        void BPL(); //branch on plus(negative clear)
        void BRK(); //break / interrupt
        void BVC(); //branch on overflow clear
        void BVS(); //branch on overflow set
        void CLC(); //clear carry
        void CLD(); //clear decimal
        void CLI(); //clear interrupt disable
        void CLV(); //clear overflow
        void CMP(NES.CPU.AddressingMode addressingMod); //compare(with accumulator)
        void CPX(NES.CPU.AddressingMode addressingMode); //compare with X
        void CPY(NES.CPU.AddressingMode addressingMode); //compare with Y
        void DEC(NES.CPU.AddressingMode addressingMode); //decrement
        void DEX(); //decrement X
        void DEY(); //decrement Y
        void EOR(); //exclusive or(with accumulator)
        void INC(); //increment
        void INX(); //increment X
        void INY(); //increment Y
        void JMP(NES.CPU.AddressingMode addressingMode); //jump
        void JSR(); //jump subroutine
        void LDA(NES.CPU.AddressingMode addressingMode); //load accumulator
        void LDX(NES.CPU.AddressingMode addressingMode); //load X
        void LDY(NES.CPU.AddressingMode addressingMode); //load Y
        void LSR(NES.CPU.AddressingMode addressingMode); //logical shift right
        void NOP(); //no operation
        void ORA(NES.CPU.AddressingMode addressingMode); //or with accumulator
        void PHA(); //push accumulator
        void PHP(); //push processor status(SR)
        void PLA(); //pull accumulator
        void PLP(); //pull processor status(SR)
        void ROL(); //rotate left
        void ROR(); //rotate right
        void RTI(); //return from interrupt
        void RTS(); //return from subroutine
        void SBC(); //subtract with carry
        void SEC(); //set carry
        void SED(); //set decimal
        void SEI(); //set interrupt disable
        void STA(NES.CPU.AddressingMode addressingMode); //store accumulator
        void STX(NES.CPU.AddressingMode addressingMode); //store X
        void STY(); //store Y
        void TAX(); //transfer accumulator to X
        void TAY(); //transfer accumulator to Y
        void TSX(); //transfer stack pointer to X
        void TXA(); //transfer X to accumulator
        void TXS(); //transfer X to stack pointer
        void TYA(); //transfer Y to accumulator
    }
}
