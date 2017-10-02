using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine
{
    public static class Debugger
    {
        public static string lastOutput = "";
        public static string Disassemble(int command)
        {
            string res = "";
            var unpackRes = UnPack( command );
            res = CommandToStr( unpackRes.Item1 );//cmd
            res += " "+ModRMToStr(unpackRes.Item2, unpackRes.Item3.ToString());//cmd+modrm+op
            return res;
        }

        public static void Debug(int rx,int pc, int[] mem, int disasmStart=0, int disasmEnd = 255, int dumpMemStart = 0, int dumpMemEnd = 256)//255?
        {
            //Console.Clear();
            if (disasmStart < 0)
                disasmStart = 0;
            if (disasmEnd > mem.Length)
                disasmEnd = mem.Length;

            Console.WriteLine( "\n[REGISTERS]: \n rx=" + MemToStr( rx ) + "\n pc="+MemToStr(pc) );
            Console.WriteLine( "\n[MEMORY]:" );
            DumpMem( mem, dumpMemStart, dumpMemEnd );
            Console.WriteLine( "\n[DISASSEMBLED]:" );
            for (int i = disasmStart; i < disasmEnd; i++)
            {
                string disassembled = Disassemble( mem[i] );
                if (i == pc)
                    Console.WriteLine( ">" + i.ToString().PadLeft( 3 ) + ": " + disassembled );
                else
                    Console.WriteLine( " " + i.ToString().PadLeft( 3 ) + ": " + disassembled );
            }
            Console.WriteLine("[LAST OUT]: "+ lastOutput );
            Console.ReadKey();
        }

        public static void DumpMem(int[] mem, int dumpStart, int dumpEnd)
        {
            if (dumpEnd > mem.Length)
                dumpEnd = mem.Length;
            for (int i = dumpStart - dumpStart % 8; i < dumpEnd; i++)// dumpStart - dumpStart % 8 ?
            {
                if (i % 8 == 0)
                    Console.Write( "\n" + i.ToString().PadLeft( 6, '0' ) + ":" );
                Console.Write( MemToStr( mem[i]));

            }
        }

        private static string MemToStr(int mem)
        {
            char chMem = (mem < 32) ? '?' : (char)mem;
            return Convert.ToString( mem, 16 ).PadLeft( 6, '0' ) + "=" + chMem + " ";
        }

        private static string ModRMToStr(MODRM modrm, string operand)
        {
            switch (modrm)
            {
                case MODRM.Direct: return "["+operand+"]";
                case MODRM.Imed: return operand;
                case MODRM.Indirect: return "[[" + operand + "]]";
                case MODRM.NoOperand: return "";
            }
            return "??";
        }

        public static string CommandToStr(COMAMND cmd)
        {
            switch (cmd)
            {
                case COMAMND.ADD: return "ADD";
                case COMAMND.DEC: return "DEC";
                case COMAMND.HALT: return "HALT";
                case COMAMND.IFN: return "IFN";
                case COMAMND.IFZ: return "IFZ";
                case COMAMND.IN: return "IN";
                case COMAMND.INC: return "INC";
                case COMAMND.JMP: return "JMP";
                case COMAMND.LDR: return "LDR";
                case COMAMND.NOP: return "NOP";
                case COMAMND.OUT: return "OUT";
                case COMAMND.STR: return "STR";
                case COMAMND.SUB: return "SUB";
            }
            return "???";
        }

        private static Tuple<COMAMND, MODRM, int> UnPack(int memPiece)
        {
            COMAMND cop = (COMAMND)(memPiece & 0xf);
            MODRM modrm = (MODRM)((memPiece >> 4) & 0x3);
            int operand = (memPiece >> 6) & 0x3ffffff;
            return new Tuple<COMAMND, MODRM, int>( cop, modrm, operand );
        }


    }
}
