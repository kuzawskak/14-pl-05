using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components
{
    class MessagePrinter
    {
        public void Print(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("CS :: ");
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }
}
