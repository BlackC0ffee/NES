using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    internal class Cartridge
    {
        private string catridgePath;
        private BinaryReader reader;

        public Cartridge(string catridgePath)
        {
            this.catridgePath = catridgePath;
            FileStream stream = new FileStream(catridgePath, FileMode.Open, FileAccess.Read);
            reader = new BinaryReader(stream);
        }

        public string CatridgePath { get => catridgePath; }

        private static bool ReturnBit(byte b, int index) {
            //Some explenation here, because this is a shift operator.
            //(1 << index) creates a single bit and moves it to the left based on the steps. (e.g. index = 3 creates: 1000)
            //Then the (b & 1000) performs a bitwise AND Operations. So if b equels 01010, the output becomes 01000
            //Finally, 01000 returns true if it is something else than 0 and in this case this is true.
            //Now if we had 10101 for b, the bitwise AND would return 0 and this would return false in the final step.
            //More info: https://en.wikipedia.org/wiki/Bitwise_operation
            //Final note, this is using LSb (so we cound index from right to left)
            return (b & (1 << index)) != 0;
        }

        public string ReturnHeader(){
            string localstring = "";
            byte[] nes = new byte[3];
            reader.Read(nes, 0, 3);
            localstring += "iNES file identifier: " + System.Text.Encoding.Default.GetString(nes) + System.Environment.NewLine;

            nes = new byte[1];
            reader.Read(nes, 0, 1);
            localstring += "$1A file identifier: $" + Convert.ToHexString(nes) + System.Environment.NewLine;

            localstring += "Number of 16 KB PRG-ROM banks: " + Convert.ToDecimal(reader.ReadByte()) + System.Environment.NewLine;

            localstring += "Number of 8 KB CHR-ROM / VROM banks: " + Convert.ToDecimal(reader.ReadByte()) + System.Environment.NewLine;

            byte b = reader.ReadByte(); //Add some explenation
            localstring += "ROM Control Byte 1: " + System.Environment.NewLine;
            localstring += "\t- Bit 0: " + ReturnBit(b, 0) + System.Environment.NewLine;
            localstring += "\t- Bit 1: " + ReturnBit(b, 1) + System.Environment.NewLine;
            localstring += "\t- Bit 2: " + ReturnBit(b, 2) + System.Environment.NewLine;
            localstring += "\t- Bit 3: " + ReturnBit(b, 3) + System.Environment.NewLine;
            localstring += "\t- Bit 4: " + ReturnBit(b, 4) + System.Environment.NewLine;
            localstring += "\t- Bit 5: " + ReturnBit(b, 5) + System.Environment.NewLine;
            localstring += "\t- Bit 6: " + ReturnBit(b, 6) + System.Environment.NewLine;
            localstring += "\t- Bit 7: " + ReturnBit(b, 7) + System.Environment.NewLine;

            b = reader.ReadByte(); //Add some explenation
            localstring += "ROM Control Byte 2: " + System.Environment.NewLine;
            localstring += "\t- Bit 0: " + ReturnBit(b, 0) + System.Environment.NewLine;
            localstring += "\t- Bit 1: " + ReturnBit(b, 1) + System.Environment.NewLine;
            localstring += "\t- Bit 2: " + ReturnBit(b, 2) + System.Environment.NewLine;
            localstring += "\t- Bit 3: " + ReturnBit(b, 3) + System.Environment.NewLine;
            localstring += "\t- Bit 4: " + ReturnBit(b, 4) + System.Environment.NewLine;
            localstring += "\t- Bit 5: " + ReturnBit(b, 5) + System.Environment.NewLine;
            localstring += "\t- Bit 6: " + ReturnBit(b, 6) + System.Environment.NewLine;
            localstring += "\t- Bit 7: " + ReturnBit(b, 7) + System.Environment.NewLine;

            return localstring;
        }
    }
}
