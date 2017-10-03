using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TotalWarPackReader
{
    public static class KnownFileTypes
    {
        public enum Types
        {
            TXT,
            PNG,
            DDS,
            XML,
            UNK
        }

        public static Types IdentifyFile(string name)
        {
            string ext = Path.GetExtension(name);
            Console.WriteLine("Extenstion:" + ext);
            switch (ext)
            {
                case ".png":
                    return Types.PNG;
                case ".txt":
                    return Types.TXT;
                case ".dds":
                    return Types.DDS;
                case ".xml":
                    return Types.XML;
                default:
                    return Types.UNK;
            }
        }
    }
}
