﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NES.Cartridge {
    internal class Header {
        public int NumberOfPRGROMBanks { get; }
        private int NumberOfCHRROMBanks { get; }
        private bool VerticalMirrored { get; }
        private bool BatteryBackedRam { get; }
        private bool Trainer { get; }
        private bool AlternativeNametableLayout { get; }
        private bool VSUnisystem { get; }
        private bool PlayChoice10 { get; }


        public Header(byte[] header) {
            // Header validation
            if(header == null) { throw new ArgumentNullException(nameof(header), "Header cannot be null."); }
            if(header.Length != 16) { throw new ArgumentException("Header must be exactly 16 bytes long.", nameof(header)); }
            //TODO improve next two lines to check for 0x4e, 0x45, 0x53 and 0x1a
            if(Encoding.UTF8.GetString(header, 0, 3) != "NES") {  throw new ArgumentException("Header does not start with 'NES'", nameof (header)); }
            if(header[3] != 0x1A) { throw new ArgumentException("Forth byte of header is not 0x1A", nameof(header)); }

            this.NumberOfPRGROMBanks = header[4];
            this.NumberOfCHRROMBanks = header[5];

            //Flag 6
            this.VerticalMirrored = ReturnBit(header[6], 0); // True or 1 = horizontal arrangement ("vertically mirrored") (CIRAM A10 = PPU A10)
            this.BatteryBackedRam = ReturnBit(header[6], 1); // Cartridge contains battery-backed PRG RAM ($6000-7FFF) or other persistent memory
            this.Trainer = ReturnBit(header[6], 2); // 512-byte trainer at $7000-$71FF (stored before PRG data)
            //TODO alternativeNametableLayout is unclear and will require more work, see https://www.nesdev.org/wiki/INES#Flags_6
            this.AlternativeNametableLayout = ReturnBit(header[6], 3); //The exact meaning of the "Alternative nametable layout" bit varies by the mapper, with both current and historical (deprecated) uses of this bit. Some mappers have a 4-screen variation of the board, which is specified with bit 3

            //Flag 7
            this.VSUnisystem = ReturnBit(header[7], 0); //VS Unisystem
            this.PlayChoice10 = ReturnBit(header[7], 1); //PlayChoice-10 (8 KB of Hint Screen data stored after CHR data)

        }

        private static bool ReturnBit(byte b, int index) {
            //Some explenation here, because this is a shift operator.
            //(1 << index) creates a single bit and moves it to the left based on the steps. (e.g. index = 3 creates: 1000)
            //Then the (b & 1000) performs a bitwise AND Operations. So if b equels 01010, the output becomes 01000
            //Finally, 01000 returns true if it is something else than 0 and in this case this is true.
            //Now if we had 10101 for b, the bitwise AND would return 0 and this would return false in the final step.
            //More info: https://en.wikipedia.org/wiki/Bitwise_operation
            //Final note, this is using LSb (so we cound index from right to left)
            return (b & 1 << index) != 0;
        }
    }
}
