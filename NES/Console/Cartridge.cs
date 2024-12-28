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
        private FileInfo catridgeFileInfo;
        private BinaryReader reader;
        private IDictionary romControl;
        private NES.Cartridge.Header Header { get; }
        private NES.Cartridge.PRGROM[] PRGROMBanks { get; }

        public Cartridge(FileInfo catridgeFileInfo)
        {
            this.catridgeFileInfo = catridgeFileInfo;
            romControl = new Dictionary<string, bool>();
            FileStream stream = new FileStream(catridgeFileInfo.FullName, FileMode.Open, FileAccess.Read);
            reader = new BinaryReader(stream);

            Header = new NES.Cartridge.Header(reader.ReadBytes(16));

            if (this.Header.Trainer) { throw  new NotImplementedException(); } else {
                PRGROMBanks = new NES.Cartridge.PRGROM[this.Header.NumberOfPRGROMBanks];
                for (int i = 0; i < PRGROMBanks.Length; i++) {
                    PRGROMBanks[i] = new NES.Cartridge.PRGROM(reader.ReadBytes(16384));
                }
            }
        }
    }
}
