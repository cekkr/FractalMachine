using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalMachineLib
{
    public class Reader
    {
        public void Read(string Str)
        {
            for(int i=0; i<Str.Length; i++)
            {

            }
        }

        public class Piece
        {
            public string Content;
            public int Line, Col;
        }

        public class Action
        {
            public Reader MyReader;
            public Action(Reader Reader)
            {
                MyReader = Reader;
            }
        }

        public class Trigger
        {
            public string Str;
        }
    }
}
