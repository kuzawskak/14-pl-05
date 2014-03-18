using Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ushort port = 0;
            int timeout = 0;

            string p, d;

            if (args.Length == 3)
            {
                p = args[1];
                d = args[2];
            }
            else
            {
                Console.Write("Port number: ");
                p = Console.ReadLine();
                Console.Write("Timeout: ");
                d = Console.ReadLine();
            }

            try
            {
                port = ushort.Parse(p);
                timeout = int.Parse(d);
            }
            catch
            {
                Console.WriteLine("Wrong port number or timeout format.");
                return;
            }

            Server s = new Server(port, new TimeSpan(0, 0, timeout));

            Task serverThread = Task.Factory.StartNew(s.Start);

            Console.WriteLine("Server is running on port {0}. Press ESC to stop...", port);
            while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;

            s.Stop();
        }
    }
}
