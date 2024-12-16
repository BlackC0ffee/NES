using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NES.Console
{
    internal class Cartridge
    {
        private string catridgePath;
        private FileInfo catridgeFileInfo;
        private BinaryReader reader;
        private IDictionary romControl;

        public Cartridge(string catridgePath)
        {
            this.catridgePath = catridgePath;
            this.catridgeFileInfo = new FileInfo(catridgePath);
            romControl = new Dictionary<string, bool>();
            FileStream stream = new FileStream(catridgePath, FileMode.Open, FileAccess.Read);
            reader = new BinaryReader(stream);
        }

        public string CatridgePath { get => catridgePath; }

        private static bool ReturnBit(byte b, int index)
        {
            //Some explenation here, because this is a shift operator.
            //(1 << index) creates a single bit and moves it to the left based on the steps. (e.g. index = 3 creates: 1000)
            //Then the (b & 1000) performs a bitwise AND Operations. So if b equels 01010, the output becomes 01000
            //Finally, 01000 returns true if it is something else than 0 and in this case this is true.
            //Now if we had 10101 for b, the bitwise AND would return 0 and this would return false in the final step.
            //More info: https://en.wikipedia.org/wiki/Bitwise_operation
            //Final note, this is using LSb (so we cound index from right to left)
            return (b & 1 << index) != 0;
        }

        public string ReturnHeader()
        {
            Debug.WriteLine("Loading Catridge: " + catridgeFileInfo.Name);
            string localstring = "";
            
            byte[] nes = new byte[3];
            reader.Read(nes, 0, 3);
            if(Encoding.Default.GetString(nes) == "NES"){ Debug.WriteLine("iNES file identifier found"); } else { throw new Exception("iNES file identifier not found"); }

            nes = new byte[1];
            reader.Read(nes, 0, 1);
            if (Convert.ToHexString(nes) == "1A") { Debug.WriteLine("$1A identifier found"); } else { throw new Exception("$1A identifier not found"); }

            Debug.WriteLine("Number of 16 KB PRG-ROM banks: " + Convert.ToDecimal(reader.ReadByte()));
            Debug.WriteLine("Number of 8 KB CHR-ROM / VROM banks: " + Convert.ToDecimal(reader.ReadByte()));
            //localstring += "Number of 16 KB PRG-ROM banks: " + Convert.ToDecimal(reader.ReadByte()) + Environment.NewLine;

            //localstring += "Number of 8 KB CHR-ROM / VROM banks: " + Convert.ToDecimal(reader.ReadByte()) + Environment.NewLine;
            byte b = reader.ReadByte(); // First ROM Control byte
            romControl["verticalMirroring"] = ReturnBit(b, 0);
            romControl["batteryBackedRam"] = ReturnBit(b, 1);
            romControl["trainer"] = ReturnBit(b, 2);
            romControl["fourScreenMirroring"] = ReturnBit(b, 3); //Overrides bit 0 (verticalMirroring) to indicate four screen mirroring

            localstring += "ROM Control Byte 1: " + Environment.NewLine;
            localstring += "\t- Bit 0: " + ReturnBit(b, 0) + Environment.NewLine;
            localstring += "\t- Bit 1: " + ReturnBit(b, 1) + Environment.NewLine;
            localstring += "\t- Bit 2: " + ReturnBit(b, 2) + Environment.NewLine;
            localstring += "\t- Bit 3: " + ReturnBit(b, 3) + Environment.NewLine;
            localstring += "\t- Bit 4: " + ReturnBit(b, 4) + Environment.NewLine;
            localstring += "\t- Bit 5: " + ReturnBit(b, 5) + Environment.NewLine;
            localstring += "\t- Bit 6: " + ReturnBit(b, 6) + Environment.NewLine;
            localstring += "\t- Bit 7: " + ReturnBit(b, 7) + Environment.NewLine;

            b = reader.ReadByte();
            localstring += "ROM Control Byte 2: " + Environment.NewLine;
            localstring += "\t- Bit 0: " + ReturnBit(b, 0) + Environment.NewLine;
            localstring += "\t- Bit 1: " + ReturnBit(b, 1) + Environment.NewLine;
            localstring += "\t- Bit 2: " + ReturnBit(b, 2) + Environment.NewLine;
            localstring += "\t- Bit 3: " + ReturnBit(b, 3) + Environment.NewLine;
            localstring += "\t- Bit 4: " + ReturnBit(b, 4) + Environment.NewLine;
            localstring += "\t- Bit 5: " + ReturnBit(b, 5) + Environment.NewLine;
            localstring += "\t- Bit 6: " + ReturnBit(b, 6) + Environment.NewLine;
            localstring += "\t- Bit 7: " + ReturnBit(b, 7) + Environment.NewLine;

            return localstring;
        }
    }
}
