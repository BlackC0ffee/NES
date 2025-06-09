# Ricoh2A03 CPU Emulator Documentation

Note: AI generated

The `Ricoh2A03` class emulates the Ricoh 2A03 CPU—a variant of the MOS 6502 used in the Nintendo Entertainment System (NES). It implements the `IMOS6502` interface and provides functionalities for instruction execution, flag manipulation, addressing modes, and cycle counting. Although many instructions are fully implemented, several methods throw a `NotImplementedException`, highlighting areas that may require further development.

---

## Table of Contents

- [Class Overview](#class-overview)
- [Key Fields and Registers](#key-fields-and-registers)
- [Constructor](#constructor)
- [Public Methods](#public-methods)
  - [Demo](#demo)
  - [ExecuteInstruction](#executeinstruction)
  - [Reset](#reset)
  - [Run](#run)
  - [CPU Instruction Methods](#cpu-instruction-methods)
- [Status Register (Flag) Functions](#status-register-flag-functions)
- [Arithmetic and Logical Instructions](#arithmetic-and-logical-instructions)
- [Branch, Jump and Comparison Instructions](#branch-jump-and-comparison-instructions)
- [Memory Load/Store and Transfer Instructions](#memory-loadstore-and-transfer-instructions)
- [Helper Functions](#helper-functions)
  - [Flag Helper Functions](#flag-helper-functions)
  - [Addressing Mode Helper Functions](#addressing-mode-helper-functions)
- [Notes and Future Improvements](#notes-and-future-improvements)

---

## Class Overview

The `Ricoh2A03` class is responsible for emulating a Ricoh 2A03 processor. It:

- Maintains the CPU state using registers (program counter `pc`, accumulator `ac`, registers `x` and `y`, status register `sr`, and stack pointer `sp`).
- Implements CPU instructions such as arithmetic (ADC, AND), branch instructions (BCS, BNE, BPL), jump instructions (JMP), and various flag manipulation operations (SEC, CLC, SEI, CLI, SED, CLD, BRK).
- Utilizes a CPU memory map (`cPUMemoryMap`) to abstract how the processor accesses memory.
- Uses a cycle counter (`CpuCycleCounter`) to mimic instruction timing.

---

## Key Fields and Registers

- **pc (UInt16):**  
  The 16-bit program counter which points to the next instruction to execute. It is initially set by reading the reset vector from memory (addresses `$FFFC` and `$FFFD`).

- **ac (Byte):**  
  The accumulator register used for most arithmetic and logical operations.

- **x and y (Byte):**  
  The index registers that are used in many addressing modes and transfer instructions.

- **sr (Byte):**  
  The status register, with bitwise flags representing:  
  - *Negative (Bit 7)*  
  - *Overflow (Bit 6)*  
  - *(Unused – always set to 1)*  
  - *Break (Bit 4)*  
  - *Decimal (Bit 3)*  
  - *Interrupt Disable (Bit 2)*  
  - *Zero (Bit 1)*  
  - *Carry (Bit 0)*

- **sp (Byte):**  
  The stack pointer used for stack-based operations.

- **CpuCycleCounter (ulong):**  
  A counter that simulates the number of cycles consumed during CPU operations.

- **cPUMemoryMap:**  
  Abstracts the CPU memory locations used during instruction execution.

- **cartridge (NES.Console.Cartridge):**  
  A reference to the NES ROM cartridge that provides the program data.

- **brk (bool):**  
  A temporary flag used to control when the CPU should exit the execution loop.

---

## Constructor

### `Ricoh2A03(NES.Console.Cartridge cartridge)`

- **Description:**  
  Initializes the CPU emulator with the provided cartridge. It sets up the memory map, initializes the status register (ensuring that bit 5 is always set), initializes the cycle counter, and fetches the initial program counter using the reset vector from memory.

- **Parameters:**  
  - `cartridge`: An instance of `NES.Console.Cartridge` containing the ROM data.

- **Usage Example:**  
  ```csharp
  var cartridge = new NES.Console.Cartridge("path/to/rom.nes");
  var cpu = new Ricoh2A03(cartridge);
  ```

---

## Public Methods

### Demo

#### `Demo()`

- **Description:**  
  Demonstrates a sequence of status register flag operations. It calls functions such as `SEC`, `SED`, `SEI`, `BRK`, `CLC`, `CLI`, and `CLD`, printing the binary value of the status register after each change.  
- **Purpose:**  
  Used primarily for debugging and understanding how flag methods affect the status register.

---

### ExecuteInstruction

#### `ExecuteInstruction(int opcodes)`

- **Description:**  
  Executes an instruction based on the given opcode. It prints the current program counter and then selects an instruction to execute via a switch-case. If the opcode is not implemented, it throws a `NotImplementedException`.

- **Parameters:**  
  - `opcodes`: The opcode integer that specifies the instruction to execute.

- **Notes:**  
  The method increases the program counter at the end of each instruction cycle to move on to the next instruction.

---

### Reset

#### `Reset()`

- **Description:**  
  Resets the CPU state following an NES system reset sequence:
  - Disables interrupts (using `SEI`).
  - Clears the carry flag (using `CLC`).
  - Loads the program counter from the reset vector located at `$FFFC` and `$FFFD`.
  
- **Additional Information:**  
  The method contains commented-out pseudo-code for initializing RAM and disabling various interrupts and rendering functions. This can guide further implementation details.

---

### Run

#### `Run()`

- **Description:**  
  Enters a continuous loop where the CPU executes instructions one by one using the `ExecuteInstruction` method.  
- **Loop Control:**  
  The loop runs until the `brk` flag is set to true, which indicates a break (or halt) condition in processing.

---

## CPU Instruction Methods

The class implements several CPU instructions, grouped by functionality. Some are fully implemented, whereas others throw `NotImplementedException`.

### Status Register (Flag) Functions

These functions directly manipulate the bits in the status register (`sr`):

- **`SEC()`**  
  - **Purpose:** Sets the Carry flag (bit 0) to 1.  
  - **Details:** Uses an XOR mask (`0b00000001`) and adds 2 cycles.

- **`CLC()`**  
  - **Purpose:** Clears the Carry flag (bit 0) to 0.  
  - **Details:** Uses an AND mask (`0b11111110`) and adds 2 cycles.

- **`SEI()`**  
  - **Purpose:** Sets the Interrupt Disable flag (bit 2) to 1, disabling interrupts.
  
- **`CLI()`**  
  - **Purpose:** Clears the Interrupt Disable flag (bit 2) to 0, allowing interrupts.
  
- **`SED()`**  
  - **Purpose:** Sets the Decimal Mode flag (bit 3) to 1.
  
- **`CLD()`**  
  - **Purpose:** Clears the Decimal Mode flag (bit 3) to 0.
  
- **`BRK()`**  
  - **Purpose:** Sets the Break Command flag (bit 4) to indicate a break or interrupt.  
  - **Cycle Impact:** Increments the CPU cycle counter by 7.

---

### Arithmetic and Logical Instructions

- **`ADC(AddressingMode addressingMode)`**  
  - **Purpose:** Adds a memory-value operand (fetched using the specified addressing mode) and the current Carry flag to the accumulator (`ac`), updating Carry and Overflow flags appropriately.  
  - **Details:**  
    - Fetches the data based on the addressing mode (Immediate, ZeroPage, Absolute, etc.).  
    - Calculates a result including the carry bit.  
    - Updates the Carry flag if the result overflows an 8-bit boundary.  
    - Contains experimental code to adjust the Overflow flag, though further refinement may be needed.  
  - **Note:** Ends with a `NotImplementedException`, indicating that further development is needed to finalize the operation.

- **`AND(AddressingMode addressingMode)`**  
  - **Purpose:** Performs a bitwise AND operation between the accumulator and a fetched operand.  
  - **Post-Operation:**  
    - Sets the Zero flag if the result is zero.  
    - Sets the Negative flag if the most significant bit of the result is 1.

- **`ASL(AddressingMode addressingMode)`**  
  - **Purpose:** Shifts the operand one bit to the left (arithmetic shift left).  
  - **Behavior:**  
    - Checks whether the most significant bit of the operand is set; if so, it sets the Carry flag.  
    - If the result is zero, the Zero flag is set.
  - **Usage:** Can target both the accumulator and memory (depending on the addressing mode).

---

### Branch, Jump, and Comparison Instructions

- **`BCC()`**  
  - **Description:** Branch if Carry Clear.  
  - **Status:** Not implemented.

- **`BCS()`**  
  - **Purpose:** Branches if the Carry flag is set.  
  - **Operation:**  
    - Uses relative addressing to adjust the program counter if the Carry flag is 1.  
    - Increments the cycle counter accordingly.

- **`BEQ()`**  
  - **Description:** Branch if Equal (i.e., if the Zero flag is set).  
  - **Status:** Not implemented.

- **`BIT(AddressingMode addressingMode)`**  
  - **Purpose:** Performs a bit test between the accumulator and a memory operand.  
  - **Operation:**  
    - Checks if the result of `ac AND memory_operand` is zero (setting the Zero flag if true).  
    - Combines parts of the operand into the stack pointer with overflow and negative flag interpretations (this part is experimental and might need revision).  
    - Only Absolute addressing mode is supported.
  
- **`BMI()`**  
  - **Description:** Branch if Minus (if the Negative flag is set).  
  - **Status:** Not implemented.

- **`BNE()`**  
  - **Purpose:** Branch if Not Equal (if the Zero flag is clear).  
  - **Operation:**  
    - Fetches a relative jump operand.
    - Adjusts the program counter if the Zero flag is 0.
  
- **`BPL()`**  
  - **Purpose:** Branch if Plus (if the Negative flag is clear).  
  - **Notes:**  
    - Uses relative addressing.  
    - Contains a small hack (toggling the Negative flag if the cycle counter is high) to prevent boot loops.

- **`BVC()`** and **`BVS()`**  
  - **Description:** Branch if Overflow Clear/Set.  
  - **Status:** Not implemented.

- **`CLV()`**  
  - **Description:** Clears the Overflow flag.  
  - **Status:** Not implemented.

- **`CMP(AddressingMode addressingMode)`**  
  - **Purpose:** Compares the accumulator with an operand fetched from memory.  
  - **Operation:**  
    - Updates the Zero, Carry, and Negative flags based on whether the accumulator is less than, equal to, or greater than the operand.  
    - Supports Immediate and ZeroPage addressing modes (others are not implemented).

- **`CPX()` and `CPY()`**  
  - **Description:** Compare the X and Y registers with a fetched operand, respectively.  
  - **Status:** Not implemented.

- **`DEC()`, `DEX()`, `DEY()`**  
  - **Description:** Decrement memory or registers.  
  - **Note:** Only the increment for X (`INX`) is implemented; the decrement variants are not fully implemented.

- **`EOR()`**  
  - **Description:** Executes a bitwise Exclusive OR (XOR) between the accumulator and memory operand.  
  - **Status:** Not implemented.

- **`INC()`**  
  - **Description:** Increments a value stored in memory.  
  - **Status:** Not implemented.

- **`INX()`**  
  - **Purpose:** Increments the X register by one.  
  - **Operation:**  
    - Increases `x` by 1.
    - Adds 2 cycles to the cycle counter.

- **`INY()`**  
  - **Description:** Increments the Y register.  
  - **Status:** Not implemented.

- **`JMP(AddressingMode addressingMode)`**  
  - **Purpose:** Performs an unconditional jump to a new address.  
  - **Operation:**  
    - Supports Absolute addressing mode.
    - Reads a 16-bit address and resets the program counter (with a slight adjustment to account for the increment).
    - Increases cycle count.

- **`JSR()`**  
  - **Description:** Jumps to a subroutine.  
  - **Status:** Not implemented.

---

### Memory Load/Store and Transfer Instructions

- **`LDA(AddressingMode addressingMode)`**  
  - **Purpose:** Loads a value into the accumulator.  
  - **Supported Addressing Modes:** Immediate, ZeroPage, and Absolute.
  - **Operation:**  
    - Fetches the operand based on the addressing mode.
    - Loads it into the accumulator and updates CPU cycles.

- **`LDX(AddressingMode addressingMode)`**  
  - **Purpose:** Loads a value into the X register.  
  - **Supported Addressing Modes:** Only Immediate is implemented.
  - **Operation:**  
    - Reads the next byte and assigns it to `x`, updating cycle count.

- **`LDY()`**  
  - **Description:** Loads a value into the Y register.  
  - **Status:** Not implemented.

- **`LSR()`**  
  - **Description:** Logical Shift Right on either a memory operand or the accumulator.  
  - **Status:** Not implemented.

- **`NOP()`**  
  - **Purpose:** No Operation; consumes cycles without changing CPU state.
  - **Status:** Not implemented.

- **`ORA()`**  
  - **Description:** Performs a bitwise OR between the accumulator and a memory operand.
  - **Status:** Not implemented.

- **Stack Operations (`PHA()`, `PHP()`, `PLA()`, `PLP()`):**  
  - **Purpose:** These methods are intended to push or pull the accumulator or status register to/from the stack.
  - **Status:** Not implemented.

- **Rotate Operations (`ROL()`, `ROR()`):**  
  - **Description:** Rotate bits left or right in either the accumulator or memory.
  - **Status:** Not implemented.

- **Return Operations (`RTI()`, `RTS()`):**  
  - **Description:** Return from Interrupt and Return from Subroutine.
  - **Status:** Not implemented.

- **`SBC()`**  
  - **Description:** Subtracts with Borrow from the accumulator.
  - **Status:** Not implemented.

- **`STA(AddressingMode addressingMode)`**  
  - **Purpose:** Stores the accumulator value into memory.  
  - **Supported Addressing Modes:** ZeroPage, Absolute, and AbsoluteX.  
  - **Operation:**  
    - Writes `ac` into the computed memory address.
    - Adjusts the cycle counter accordingly.

- **`STX(AddressingMode addressingMode)`**  
  - **Purpose:** Stores the X register value into memory.
  - **Supported Addressing Modes:** Only Absolute is implemented.
  - **Operation:**  
    - Writes `x` to the target address and updates the cycle count.

- **`STY()`**  
  - **Description:** Stores the Y register value into memory.
  - **Status:** Not implemented.

- **Transfer Instructions:**  
  - **`TAX()` and `TAY()`:** Intended to transfer values between registers.
  - **`TXA()`**  
    - **Purpose:** Transfers the value in the X register into the accumulator.
    - **Operation:** Sets `ac` equal to `x` and adds 2 cycles.
  - **`TXS()`**  
    - **Purpose:** Transfers the value in the X register into the stack pointer.
    - **Operation:** Sets `sp` equal to `x` and adds 2 cycles.
  - **`TSX()` and `TYA()`:**  
    - **Status:** Not implemented.

---

## Helper Functions

These private helper functions support various CPU operations—especially those relating to flag manipulation and addressing mode computations.

### Flag Helper Functions

- **`SetCarryFlag()`**  
  - **Functionality:** Sets the Carry flag (bit 0) in the status register.

- **`ClearCarryFlag()`**  
  - **Functionality:** Clears the Carry flag.

- **`GetCarryFlag()`**  
  - **Functionality:** Returns the current value (0 or 1) of the Carry flag.

- **`SetZeroFlag()`**  
  - **Functionality:** Sets the Zero flag (bit 1) in the status register.

- **`GetZeroFlag()`**  
  - **Functionality:** Returns the value of the Zero flag (right-shifted by one bit).

- **`SetInterruptDisableFlag()`**  
  - **Functionality:** Sets the Interrupt Disable flag (bit 2) in the status register.

- **`GetInterruptDisableFlag()`**  
  - **Functionality:** Retrieves the current state of the Interrupt Disable flag.

- **`SetDecimalFlag()`**  
  - **Functionality:** Sets the Decimal Mode flag (bit 3).

- **`SetBreakFlag()`**  
  - **Functionality:** Sets the Break flag (bit 4).

- **`GetBreakFlag()`**  
  - **Functionality:** Retrieves the Break flag’s current value.

- **`SetOverFlowFlag()`**  
  - **Functionality:** Sets the Overflow flag (bit 6).

- **`GetOverFlag()`**  
  - **Functionality:** Returns the Overflow flag's value.

- **`SetNegativeFlag()`**  
  - **Functionality:** Sets the Negative flag (bit 7).

- **`GetNegativeFlag()`**  
  - **Functionality:** Returns the Negative flag’s current value.

- **`GetMostSignificantBit(Byte x)`**  
  - **Functionality:** Returns the most significant bit of the provided byte (bit 7), useful for checking sign in 8-bit computations.

- **`isNegative(Byte input)`**  
  - **Functionality:** Returns `true` if the most significant bit of the input is 1 (indicating a negative value in two’s complement representation).

---

### Addressing Mode Helper Functions

These helper methods fetch operands from memory based on the required addressing mode:

- **`Absolute()`**  
  - **Description:** Reads a 16-bit address from memory by incrementing the program counter twice.  
  - **Usage:** Used by instructions that require an absolute memory address.

- **`AbsoluteX()`**  
  - **Description:** Reads a 16-bit address from memory and adds the X register value.  
  - **Usage:** Implements Absolute,X addressing mode.

- **`AbsoluteY()`**  
  - **Description:** Reads a 16-bit address and adds the Y register value.  
  - **Usage:** Implements Absolute,Y addressing mode.

- **`Immediate()`**  
  - **Description:** Fetches the next byte directly from memory as an immediate operand.  
  - **Usage:** Used for instructions that specify data immediately following the opcode.

- **`Relative()`**  
  - **Description:** Fetches a signed 8-bit operand used for branch instructions.  
  - **Usage:** The operand is used to compute a relative jump from the current program counter.

- **`ZeroPage()`**  
  - **Description:** Retrieves a byte operand located in the first 256 bytes of memory (zero page).  
  - **Usage:** Common in operations needing quicker access.

- **`ZeroPageX()`**  
  - **Description:** Retrieves an operand from zero-page memory offset by the X register.
  
- **`Indirect()`**  
  - **Description:** Implements indirect addressing by reading a pointer from two successive bytes and then fetching the final 16-bit address from that pointer.  
  - **Note:** Be aware of the 6502’s infamous page-boundary bug.

- **`XIndirect()`**  
  - **Description:** Implements the (Indirect, X) addressing mode.  
  - **Operation:**  
    - Reads a zero-page address, adds the X register, then fetches a 16-bit address from the resulting location.

- **`IndirectY()`**  
  - **Description:** Implements the (Indirect), Y addressing mode.  
  - **Operation:**  
    - Reads a zero-page pointer, fetches a 16-bit base address, and then adds the Y register to compute the final effective address.

---

## Notes and Future Improvements

- **Incomplete Implementations:**  
  Several instruction methods (e.g., `BCC()`, `BEQ()`, `BMI()`, `JSR()`, etc.) currently throw `NotImplementedException`. These will need to be fully implemented to support a complete NES CPU emulation.

- **Cycle Counting and Timing:**  
  The cycle counter (`CpuCycleCounter`) is updated for each operation. Ensuring that cycle counts are accurate is key to proper emulation.

- **Flag and Overflow Handling:**  
  Some flags, especially for arithmetic instructions like `ADC`, use experimental logic. Testing against known 6502 behavior will help ensure accuracy.

- **Refactoring Opportunity:**  
  Many addressing mode helper functions share common code patterns. Refactoring these into a more generic utility could reduce code duplication.

- **Debug and Testing:**  
  The `Demo()` method and debug prints (via `Debug.WriteLine`) help in verifying correctness but may be removed or replaced in a production-quality emulator.
