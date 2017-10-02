using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Assembler
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
    enum MODRM : int
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
    class FileParser
    {
        SymbTable symbTab;
        List<byte> bin;
        int pos = 0;
        string sourceName;
        /// <summary>
        /// Основные действия парсинга
        /// </summary>
        public FileParser(string inpSourceName, string inpStabName)
        {
            symbTab = new SymbTable();
            symbTab.Load( inpStabName );

            bin = new List<byte>();

            sourceName = inpSourceName;
            string[] fileLines = File.ReadAllLines( inpSourceName );
            GenBin( fileLines );
            Console.WriteLine( "[SUCCES]: Сборка завершена!" );

            string dir = Path.GetDirectoryName( inpSourceName );
            string pureName = Path.GetFileNameWithoutExtension( inpSourceName );
            File.WriteAllBytes( Path.Combine( dir, pureName + ".bin" ), bin.ToArray() );
        }

        /// <summary>
        /// Второй проход - заполнение памяти командами и данными
        /// </summary>
        private void GenBin(string[] fileLines)
        {
            string commentSign = ";";

            pos = 0;
            foreach (string line in fileLines)
            {
                int commentStartPos = line.IndexOf( commentSign );
                string cleanLine = (commentStartPos == -1) ? line : line.Remove( commentStartPos );
                string[] words = cleanLine.Split( new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );
                if (words.Length != 0)
                    Assemble( words );
            }
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
                Define( first.Substring( 0, first.Length - 1 ), pos.ToString() );
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
                        pos += StrToValue( words[1] );
                        break;
                }

            //if command:
            else
            {
                if (words.Length == 2)
                    CheckCommand( first, words[1] );//pos++4
                if (words.Length == 1)
                    CheckCommand( first, "" );//pos++4
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
        /// Основная функция ассемблирования одной строки программы
        /// </summary>
        /// <param name="words">Слова текущей строки</param>
        void Assemble(string[] words)
        {
            string first = words[0];
            if (first[first.Length - 1] == ':')
            {
                if (words.Length > 1)
                    Assemble( words.Skip( 1 ).ToArray() );
            }
            else
            //if directive:
            if (first[0] == '.')
            {
                switch (first.ToLower())
                {
                    case ".define":
                        //if words.Lenght != 3 throwError
                        Define( words[1], words[2] );
                        break;
                    case ".data":
                        //if words.Lenght > 2 throwError
                        Data( words[1] );
                        break;
                    case ".org":
                        OrgMem( words[1] );
                        break;
                        //case ".export":
                        //    break;
                }
            }
            //if command:
            else
            {
                if (words.Length == 2)
                    Format( first, words[1] );
                if (words.Length == 1)
                    Format( first, "" );
            }
        }
        /// <summary>
        /// Пакует команду и записывает ее в память bin[pos]
        /// </summary>
        private void Format(string operation, string operand)
        {
            var res = StrToModRM( operand );
            int packedCommand = Pack( StrToCommand( operation ), res.Item2, res.Item1 );
            bin.Add( (byte)(packedCommand & 0xFF) ); pos++;
            bin.Add( (byte)(packedCommand >> 8 & 0xFF) ); pos++;
            bin.Add( (byte)(packedCommand >> 16 & 0xFF) ); pos++;
            bin.Add( (byte)(packedCommand >> 24 & 0xFF) ); pos++;
        }
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
        /// Возвращает Модификатор доступа
        /// </summary>
        /// <param name="operand">Строка, содержащая операнд</param>
        private Tuple<int, MODRM> StrToModRM(string operand)
        {
            if (operand == "")
                //no operand
                return new Tuple<int, MODRM>( 0, MODRM.NoOperand );
            if (operand.First() == '[' && operand.Last() == ']')
            {
                if (operand[1] == '[' && operand[operand.Length - 2] == ']')
                    //indirect
                    return new Tuple<int, MODRM>( StrToValue( operand.Substring( 2, operand.Length - 4 ) ), MODRM.Indirect );
                else
                    //direct
                    return new Tuple<int, MODRM>( StrToValue( operand.Substring( 1, operand.Length - 2 ) ), MODRM.Direct );
            }
            else //if operand.Find([ or ]) then throwError
                //inline
                return new Tuple<int, MODRM>( StrToValue( operand ), MODRM.Imed );

        }
        /// <summary>
        /// Резервирует count ячеек памяти
        /// </summary>
        /// <param name="count">Количество ячеек</param>
        private void OrgMem(string count)
        {
            int val = StrToValue( count );
            pos += val;
            for (int i = 0; i < val * 4; i++)//val*4? !
                bin.Add( 0 );
        }
        /// <summary>
        /// Определяет(Define) адрес начала программы
        /// </summary>
        private void Start(string addr)
        {
            Define( "start", addr );
        }
        /// <summary>
        /// Кладет данные в ячейку памяти bin[pos]
        /// </summary>
        private void Data(string dataVal)
        {
            bin.Add( (byte)StrToValue( dataVal ) );
            pos++;
            // Выравнивание до int
            bin.Add( 0 );
            pos++;
            bin.Add( 0 );
            pos++;
            bin.Add( 0 );
            pos++;
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
                string pureName = Path.GetFileNameWithoutExtension( sourceName );
                var v = symbTab.Get( pureName + val );//?
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
            symbTab.Put( sourceName + symb, StrToValue( val ) );
        }
        /// <summary>
        /// Пакует команду в int
        /// </summary>
        /// <param name="cop">Код операции</param>
        /// <param name="modrm">Модификатор доступа</param>
        /// <param name="operand">Операнд</param>
        /// <returns></returns>
        private static int Pack(COMAMND cop, MODRM modrm, int operand)
        {
            return ((int)cop) | ((int)modrm << 4) | (operand << 6);
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