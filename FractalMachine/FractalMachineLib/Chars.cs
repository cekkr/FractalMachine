using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalMachineLib
{
    public static class Chars
    {
        public static bool IsLecterUpper(char ch)
        {
            return ch >= 65 && ch <= 90;
        }

        public static bool IsLecterLower(char ch)
        {
            return ch >= 97 && ch <= 122;
        }

        public static bool IsLecter(char ch)
        {
            return IsLecterUpper(ch) || IsLecterLower(ch);
        }

        public static bool IsNumber(char ch)
        {
            return ch >= 48 && ch <= 57;
        }
    }
}
