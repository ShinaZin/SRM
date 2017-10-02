using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Machine
{
    public enum COMAMND : int
    {
        NOP,
        /// <summary>
        /// rx += mem[addr]
        /// </summary>
        ADD,
        /// <summary>
        /// rx -= mem[addr];
        /// </summary>
        SUB,
        /// <summary>
        /// mem[addr] += 1
        /// </summary>
        INC,
        /// <summary>
        /// mem[addr] -= 1
        /// </summary>
        DEC,
        LDR,
        STR,
        IN,
        OUT,
        JMP,
        IFZ, IFN, HALT
    }
    public enum MODRM : int
    {
        NoOperand,
        /// <summary>
        /// Данные, вшитые в код, константы (пр: метки для JMP)
        /// </summary>
        Imed,
        /// <summary>
        /// Данные из ячейки памяти
        /// </summary>
        Direct,
        Indirect,
    }
    class Program
    {
        /// <summary>
        /// Register ex
        /// </summary>
        static int rx;
        /// <summary>
        /// Programm counter
        /// </summary>
        static int pc;
        static int[] mem;

        static bool dbgIsOn = false;
        static int[] dbgMemEdges = null;
        static int dbgCodeScope;

        const int DEFAULT_SCOPE = 8;

        static void Main(string[] args)
        {
            #region Парсинг аргументов
            if (args.Length == 0)
            {
                Console.WriteLine( "[Использование]: machine <filename.bin> [-D ]" );
                return;
            }
            string inpFileName = args[0];
            inpFileName = Path.ChangeExtension( inpFileName, ".bin" );
            if (File.Exists( inpFileName ) == false)
            {
                Console.WriteLine( "[ERROR]: файл '"+ inpFileName + "' не существует" );
                return;
            }
            for (int i = 0; i < args.Length; i++)
            {
                if (Regex.IsMatch( args[i], @"/[a-zA-Z]+" )) {
                    args[i]=args[i].Replace('/','-');
                };
                
            }
            //-Debug
            if (args.Contains("-D"))
                dbgIsOn = true;
            else
                dbgIsOn = false;
            //-Dmemomry x..y
            int indexOfArg = Array.FindIndex( args, (x) => { return x == "-Dm"; } );
            string[] tmpMemEdges;
            if (indexOfArg > -1)
            {
                tmpMemEdges = args[indexOfArg + 1].Split( new string[] { ".." }, StringSplitOptions.RemoveEmptyEntries );
                dbgMemEdges = new int[2];
                dbgMemEdges[0] = int.Parse( tmpMemEdges[0] );
                dbgMemEdges[1] = int.Parse( tmpMemEdges[1] );
                Array.Sort( dbgMemEdges );
            };
            //-Dscope x
            indexOfArg = Array.FindIndex( args, (x) => { return x == "-Ds"; } );
            if (indexOfArg > -1)
            {
                dbgCodeScope = int.Parse( args[indexOfArg + 1] );
            }
            else
                dbgCodeScope = DEFAULT_SCOPE;
            #endregion
            Init( 256, inpFileName );
            Run();

            Console.WriteLine( "Конец программы".PadRight( 80, '_' ) );
            Console.ReadKey();
        }

        static void Add(int operand)
        {
            rx += operand;
        }
        static void Sub(int operand)
        {
            rx -= operand;
        }
        static void Inc(int addr)
        {
            mem[addr]++;
        }
        static void Dec(int addr)
        {
            mem[addr]--;
        }

        static void Ldr(int operand)
        {
            //rx = mem[addr];
            rx = operand;
        }
        static void Str(int addr)
        {
            mem[addr] = rx;
        }
        static void In()
        {
            string numberMarks = "#№-+";
            Console.Write( "\n> " );
            char inp = (char)Console.Read();
            if (char.IsWhiteSpace( inp ))
            {
                do
                    inp = (char)Console.Read();//skip \r\n
                while (char.IsWhiteSpace( inp ));
                //pc--;
                //return;
            }
            if (inp == '#' || inp == '№')
            {
                string res = "";
                while (char.IsNumber( inp ) || numberMarks.Contains( inp ))
                {
                    byte[] arr = new byte[1];
                    inp = (char)Console.Read();
                    arr[0] = (byte)inp;
                    res += Encoding.ASCII.GetString( arr );
                }
                rx = int.Parse( res );
            }
            else
                rx = inp;
        }
        static void Out()
        {
            string output = "";
            if (rx < 32)
                output = Convert.ToString( rx, 16 ) + "(" + rx + ") ";
            else
                output = (char)rx + "(" + rx + ") ";
            Debugger.lastOutput = output;//
            Console.Write( output );
        }

        static void Jmp(int addr)
        {
            pc = addr - 1;//
        }
        static void Ifz()
        {
            if (rx != 0) pc = pc + 1;
        }
        static void Ifn()
        {
            if (rx >= 0) pc = pc + 1;
        }

        static void HandleError(string message = "")
        {
            Console.WriteLine( "[ERROR]: " + message + ", pc=" + pc );
        }

        static void Run()
        {
            int state = 1;
            while (state != 0)
            {
                if (dbgIsOn)
                {
                    if (dbgMemEdges != null)
                        Debugger.Debug( rx, pc, mem, pc - dbgCodeScope, pc + dbgCodeScope, dbgMemEdges[0], dbgMemEdges[1] );//debug!!
                    else
                        Debugger.Debug( rx, pc, mem, pc - dbgCodeScope, pc + dbgCodeScope );
                }

                var unpackRes = UnPack( mem[pc] );
                COMAMND cop = unpackRes.Item1;
                MODRM modrm = unpackRes.Item2;
                int operand = unpackRes.Item3;

                #region Выбор по коду операции
                switch (cop)
                {
                    case COMAMND.NOP: break;
                    case COMAMND.ADD:
                        switch (modrm)
                        {
                            case MODRM.Imed: Add( operand ); break;
                            case MODRM.Direct: Add( mem[operand] ); break;
                            case MODRM.Indirect: Add( mem[mem[operand]] ); break;
                            default:
                                HandleError();
                                break;
                        }
                        break;

                    case COMAMND.SUB:
                        switch (modrm)
                        {
                            case MODRM.Imed: Sub( operand ); break;
                            case MODRM.Direct: Sub( mem[operand] ); break;
                            case MODRM.Indirect: Sub( mem[mem[operand]] ); break;
                            default:
                                HandleError();
                                break;
                        }
                        break;


                    case COMAMND.INC:
                        if (modrm == MODRM.Direct) Inc( operand );
                        else if (modrm == MODRM.Indirect) Inc( mem[operand] );
                        else HandleError();
                        break;
                    case COMAMND.DEC:
                        if (modrm == MODRM.Direct) Dec( operand );
                        else if (modrm == MODRM.Indirect) Dec( mem[operand] );
                        else HandleError();
                        break;

                    case COMAMND.LDR:
                        if (modrm == MODRM.Imed) Ldr( operand );
                        else if (modrm == MODRM.Direct) Ldr( mem[operand] );
                        else if (modrm == MODRM.Indirect) Ldr( mem[mem[operand]] );
                        else HandleError();
                        break;
                    case COMAMND.STR:
                        if (modrm == MODRM.Direct) Str( operand );
                        else if (modrm == MODRM.Indirect) Str( mem[operand] );
                        else HandleError();
                        break;

                    case COMAMND.IN:
                        if (modrm == MODRM.NoOperand) In();
                        else HandleError();
                        break;
                    case COMAMND.OUT:
                        if (modrm == MODRM.NoOperand) Out();
                        else HandleError();
                        break;

                    case COMAMND.JMP:
                        if (modrm == MODRM.Imed) Jmp( operand );
                        else if (modrm == MODRM.Indirect) Jmp( mem[operand] );
                        else HandleError();
                        break;
                    case COMAMND.IFZ:
                        if (modrm == MODRM.NoOperand) Ifz();
                        else HandleError();
                        break;
                    case COMAMND.IFN:
                        if (modrm == MODRM.NoOperand) Ifn();
                        else HandleError();
                        break;

                    case COMAMND.HALT:
                        if (modrm == MODRM.NoOperand) state = 0;
                        else HandleError();
                        break;
                    default:
                        if (modrm == MODRM.NoOperand) state = 0;
                        else HandleError();
                        break;
                }//switch
                #endregion
                pc = (pc + 1) % 256;
            }//while
        }
        static void Init(int memorySize = 256, string inpFileName = "")
        {
            rx = 0;
            mem = new int[memorySize];
            if (inpFileName == "")
            {
                Console.WriteLine( "[Использование]: machine <filename.bin>" );
                return;
            }
            else
                pc = Load( inpFileName );
        }

        public static Tuple<COMAMND,MODRM,int> UnPack(int memPiece)
        {
            COMAMND cop = (COMAMND)(memPiece & 0xf);
            MODRM modrm = (MODRM)((memPiece >> 4) & 0x3);
            int operand = (memPiece >> 6) & 0x3ffffff;
            return new Tuple<COMAMND, MODRM, int>( cop, modrm, operand );
        }
        private static int Pack(COMAMND cop, MODRM modrm, int operand)
        {
            return ((int)cop) | ((int)modrm << 4) | (operand << 6);
        }
        private static int Load(string binFileName)
        {
            byte[] binBytes = File.ReadAllBytes( binFileName );
            //HandleError( "Поврежденный входной файл (длина должна быть кратна 4 байт)" );

            int pos = 0;
            for (int i = 0; i < binBytes.Length; i += 4)
            {
                mem[pos] = binBytes[i] |
                    binBytes[i + 1] << 8 |
                    binBytes[i + 2] << 16 |
                    binBytes[i + 3] << 24;
                /*
                COMAMND cop = (COMAMND)(mem[pos] & 0xf);//debug info
                MODRM modrm = (MODRM)((mem[pos] >> 4) & 0x3);//debug info
                int operand = (mem[pos] >> 6) & 0x3ffffff;//debug info
                */
                pos++;
            }
            return 0;//?
        }
    
    }
}
