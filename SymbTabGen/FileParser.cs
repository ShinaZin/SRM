using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SymbTabGen
{
    enum COMAMND : int
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
    class FileParser
    {
        SymbTable symbTab;
        List<byte> bin;
        int pos = 0;
        string fName;
        /// <summary>
        /// Основные действия парсинга
        /// </summary>
        public FileParser(string fileName)
        {
            symbTab = new SymbTable();
            bin = new List<byte>();
            fName = fileName;
            string[] fileLines = File.ReadAllLines( fileName );
            GenSymbTable( fileLines );
            Console.WriteLine( "[SUCCES]: Генерация таблицы символов завершена!" );
            string file = Path.GetFileNameWithoutExtension( fName );
            string dir = Path.GetDirectoryName( fName );
            symbTab.Save( Path.Combine(dir, file + ".stab" ));
        }
        /// <summary>
        /// Первый проход - генерация таблицы символов
        /// </summary>
        private void GenSymbTable(string[] fileLines)
        {
            string commentSign = ";";
            foreach (string line in fileLines)
            {
                int commentStartPos = line.IndexOf( commentSign );
                string cleanLine = (commentStartPos == -1) ? line : line.Remove( commentStartPos );
                string[] words = cleanLine.Split( new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );
                if (words.Length != 0)
                    GenSymbols( words );
            }
        }
       
        //Функции первого прохода
        /// <summary>
        /// Парсит строку, получая из нее идентификаторы
        /// </summary>
        private void GenSymbols(string[] words)
        {
            string first = words[0];
            //if label:
            if (first[first.Length - 1] == ':')
            {
                Define( first.Substring( 0, first.Length - 1 ), (pos/4).ToString() );// pos/4: int -> byte
                if (words.Length > 1)
                    GenSymbols( words.Skip( 1 ).ToArray() );
            }

            //if directive:
            else if (first[0] == '.')
                switch (first.ToLower())
                {
                    case ".define":
                        //if words.Lenght != 3 throwError
                        Define( words[1], words[2] );
                        break;
                    case ".data":
                        //if words.Lenght > 2 throwError
                        pos++;
                        break;
                    case ".org":
                        pos += StrToValue( words[1] )*4;
                        break;
                }

            //if command:
            else
            {
                if (words.Length == 2)
                    CheckCommand( first, words[1] );//pos++4, int -> byte
                if (words.Length == 1)
                    CheckCommand( first, "" );//pos++4, int -> byte
            }
        }
        /// <summary>
        /// Увеличивает счетчик команд и проверяет валидность мнемоники
        /// </summary>
        private void CheckCommand(string operation, string operand)
        {
            StrToCommand( operation );
            pos += 4;
        }

        //Функции второго прохода
        /// <summary>
        /// Преобразует строковую мнемонику команды в Код команды
        /// </summary>
        /// <param name="operation">Мнемоника</param>
        private COMAMND StrToCommand(string operation)
        {
            switch (operation.ToUpper())
            {
                case "ADD": return COMAMND.ADD;
                case "SUB": return COMAMND.SUB;

                case "INC": return COMAMND.INC;
                case "DEC": return COMAMND.DEC;

                case "HALT": return COMAMND.HALT;
                case "IFN": return COMAMND.IFN;
                case "IFZ": return COMAMND.IFZ;
                case "JMP": return COMAMND.JMP;

                case "IN": return COMAMND.IN;
                case "OUT": return COMAMND.OUT;

                case "LDR": return COMAMND.LDR;
                case "STR": return COMAMND.STR;

                case "NOP": return COMAMND.NOP;
            }
            ThrowError( "Неизвестная команда!", operation );
            return COMAMND.HALT;
        }
        /// <summary>
        /// Вытаскивает значение из таблицы символов или непосредственно из строки
        /// </summary>
        private int StrToValue(string val)
        {

            if (val == "")
                ThrowError( "Пустое значение!" );

            //if val == currentPos
            if (val == "$")
                return pos;

            //if val == defined variable
            if (val[0] == '$')
            {
                var v = symbTab.Get( fName + val );//
                if (v == SymbTable.NULL)
                    ThrowError( "Неизвестный идентификатор!", val );
                return v;
            }

            //if val == raw value (number)
            int res = 0;
            try
            {
                res = Convert.ToInt32( val );
            }
            catch { ThrowError( "Некорректное значение!", val ); }
            return res;
        }
        /// <summary>
        /// Кладет данные в таблицу символов
        /// </summary>
        /// <param name="symb">Символ</param>
        /// <param name="val">Данные</param>
        private void Define(string symb, string val)
        {
            string file = Path.GetFileNameWithoutExtension( fName );
            symbTab.Put( file + symb, StrToValue( val ) );//..fName + symb,
        }
        /// <summary>
        /// Выводит в консоль сообщение об ошибке
        /// </summary>
        /// <param name="errorMessage">Сообщение</param>
        /// <param name="line">Дополнительная информация</param>
        private static void ThrowError(string errorMessage, string line = "")
        {
            if (line == "")
                Console.WriteLine( "[ERROR]: " + errorMessage );
            else
                Console.WriteLine( "[ERROR]: " + errorMessage + "( '" + line + "' )" );
        }
    }
}
