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

        public char ReadNextByte()
        {
            return Convert.ToChar(reader.ReadByte());
        }
    }
}
