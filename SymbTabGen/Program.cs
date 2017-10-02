using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SymbTabGen
{
    class Program
    {
        static void Main(string[] args)
        {
            string inpFileName = "";
            if (args.Length > 0) inpFileName = args[0];
            if (args.Length == 0 || args[0] == "" || args[0] == null)
                do
                {
                    Console.Write( "[Использование]: symbgen <входной файл>" + "\n[Имя входного файла]: " );
                    inpFileName = Console.ReadLine().Replace( "\"", "" ).Replace("'","");
                } while (File.Exists( inpFileName ) == false);
            FileParser fp = new FileParser( inpFileName );

        }
    }
}
