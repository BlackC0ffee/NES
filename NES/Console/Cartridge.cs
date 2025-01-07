using NES.Cartridge;
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
        //private string catridgePath;
        private FileInfo cartridgeFileInfo;
        //private BinaryReader reader; 
        //private IDictionary romControl;
        internal NES.Cartridge.Header Header { get; }
        internal NES.Cartridge.PRGROM[] PRGROMBanks { get; }
        internal NES.Cartridge.CHRROM[] CHRROMBanks { get; }

        public Cartridge(FileInfo cartridgeFileInfo)
        {
            this.cartridgeFileInfo = cartridgeFileInfo;
            IDictionary romControl = new Dictionary<string, bool>();
            FileStream stream = new FileStream(cartridgeFileInfo.FullName, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);

            Header = new NES.Cartridge.Header(reader.ReadBytes(16));

            if (this.Header.Trainer) { throw  new NotImplementedException(); } else {
                PRGROMBanks = new NES.Cartridge.PRGROM[this.Header.NumberOfPRGROMBanks];
                for (int i = 0; i < PRGROMBanks.Length; i++) {
                    PRGROMBanks[i] = new NES.Cartridge.PRGROM(reader.ReadBytes(16384));
                }
            }
            
            if (this.Header.NumberOfCHRROMBanks > 0) {
                this.CHRROMBanks = new NES.Cartridge.CHRROM[this.Header.NumberOfCHRROMBanks];
                for (int i = 0; i < CHRROMBanks.Length; i++) {
                    CHRROMBanks[i] = new NES.Cartridge.CHRROM(reader.ReadBytes(8192));
                }
            }
            if(reader.BaseStream.Position != reader.BaseStream.Length) { throw new Exception("Not all iNes data has been processed. More data can be found in the ROM"); }
            // Cartridge loaded succesfull we can start the console
            
        }
    }
}
