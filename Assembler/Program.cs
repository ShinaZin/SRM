using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler
{
    class Program
    {
        static void Main(string[] args)
        {
            string
                inpSourceName = "",
                inpStabName = "";
            if (args.Length == 1)
            {
                string dir = Path.GetDirectoryName( args[0] );
                inpSourceName = Path.GetFileNameWithoutExtension( args[0] ) + ".asm";
                inpStabName = Path.GetFileNameWithoutExtension( args[0] ) + ".stab";
                inpSourceName = Path.Combine( dir, inpSourceName );
                inpStabName = Path.Combine( dir, inpStabName );
            }
            if (args.Length == 2)
            {
                if (Path.GetExtension( args[0] ) == ".asm") inpSourceName = args[0];
                if (Path.GetExtension( args[1] ) == ".asm") inpSourceName = args[1];

                if (Path.GetExtension( args[0] ) == ".stab") inpStabName = args[0];
                if (Path.GetExtension( args[1] ) == ".stab") inpStabName = args[1];

            }
            if (args.Length == 0)
            {
                Console.WriteLine( "[Использование]:\n assembler <входной файл>" +
                    "\nили\n assembler <входной файл.asm> <входной файл.stab>" );
                Console.ReadKey();//
                return;
            }
            if (File.Exists( inpSourceName ) == false)
            {
                Console.WriteLine( "[ERROR]: файл '" + inpSourceName + "' не найден" );
                return;
            }
            if (File.Exists( inpStabName ) == false)
            {
                Console.WriteLine( "[ERROR]: файл '" + inpStabName + "' не найден" );
                return;
            }

            FileParser fp = new FileParser( inpSourceName, inpStabName );//проверить!!!
        }
    }
}
