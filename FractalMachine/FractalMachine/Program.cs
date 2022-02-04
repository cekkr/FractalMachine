using FractalMachineLib;
using System;
using System.IO;

namespace FractalMachine
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var code = File.ReadAllText("../../../Assets/test.ula");
            var inter = new Interpreter();
            var res = inter.Interpret(code);

            Console.WriteLine();
        }
    }
}
