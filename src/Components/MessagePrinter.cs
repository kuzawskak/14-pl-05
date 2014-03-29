using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components
{
    /// <summary>
    /// Wyliczenie reprezentujące typ modułu.
    /// </summary>
    enum NodeName
    { 
        CS = ConsoleColor.Red,      // Communication Server
        NL = ConsoleColor.Green,    // Network Listener
        NC = ConsoleColor.Blue,     // Network Client
        XP = ConsoleColor.Yellow    // XML Parser
    }

    /// <summary>
    /// Klasa pomocnicza na potrzeby wypisywania wiadomości.
    /// </summary>
    class MessagePrinter
    {
        private static object lockObj = new object();
        private NodeName type;

        /// <summary>
        /// Domyślny konstruktor.
        /// </summary>
        /// <param name="t">Typ modułu</param>
        public MessagePrinter(NodeName t)
        {
            type = t;
        }

        /// <summary>
        /// Metoda wypisująca wiadomość.
        /// </summary>
        /// <param name="message"></param>
        public void Print(string message)
        {
            lock (lockObj)
            {
                Console.ForegroundColor = (ConsoleColor)type;
                Console.Write("{0} :: ", type);
                Console.ResetColor();
                Console.WriteLine(message);
            }
        }
    }
}
