using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SymbTabGen
{
    class SymbTable
    {
        StringBuilder table;
        public static int NULL = int.MaxValue - 1;
        public static int NAN = int.MinValue + 1;
        public static int INF_POS = int.MaxValue;
        public static int INF_NEG = int.MinValue;
        public SymbTable(int size = 16)
        {
            table = new StringBuilder( size );
        }
        public void Save(string path)
        {
            File.WriteAllText( path, table.ToString() );
        }
        public void Load(string path)
        {
            table = new StringBuilder( File.ReadAllText( path ) );
        }
        public bool Put(string name, int newVal)
        {
            int oldVal = Get( name );
            if (oldVal == NULL && newVal != NULL)
            {
                table.Append( name + "=" + Convert.ToString( newVal, 16 ) + "\n" );
            }
            else
            {
                if (newVal != NULL)
                {
                    table.Replace(
                        name + '=' + Convert.ToString( oldVal, 16 ),
                        name + '=' + Convert.ToString( newVal, 16 ),
                        0, table.Length
                    );
                }
                else
                {
                    table.Replace(
                        name + '=' + Convert.ToString( oldVal, 16 ),
                        "",
                        0, table.Length
                    );
                }
            }
            return oldVal != NULL;



        }
        public int Get(string name)
        {
            int pos = table.ToString().IndexOf( name );
            if (pos == -1) return NULL;
            pos = table.ToString().IndexOf( '=', pos ) + 1;
            //if (pos==-1) throw_error;
            int end = table.ToString().IndexOf( '\n', pos );
            //if (end==-1) throw_error;
            return Convert.ToInt32( table.ToString().Substring( pos, end - pos ), 16 );

        }

        //private void throwError(string text)
    }
}
