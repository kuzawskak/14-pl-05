using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components
{
    enum NodeName
    { 
        CS = ConsoleColor.Red,      // Communication Server
        NL = ConsoleColor.Green,    // Network Listener
        NC = ConsoleColor.Blue,     // Network Client
        XP = ConsoleColor.Yellow    // XML Parser
    }

    class MessagePrinter
    {
        private NodeName type;

        public MessagePrinter(NodeName t)
        {
            type = t;
        }

        public void Print(string message)
        {
            Console.ForegroundColor = (ConsoleColor)type;
            Console.Write("{0} :: ", type);
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }
}
