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

        public Cartridge(string catridgePath)
        {
            this.catridgePath = catridgePath;
        }

        public string CatridgePath { get => catridgePath; set => catridgePath = value; }
    }
}
