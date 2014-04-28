using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SolverComponents;

namespace ComputationalNode
{
    class Program
    {
        static void Main(string[] args)
        {

            var asm = Assembly.LoadFile(Path.GetFullPath("DVRP.dll"));
            Type t = asm.GetType("DVRP.DVRP");

            MethodInfo[] methodInfo = t.GetMethods();
            foreach (MethodInfo mi in methodInfo)
                Console.WriteLine(mi.ToString());
            string ip_address;
            string port;
            string thread_number;
            List<string> problem_names = new List<string>();

            Console.Write("IP Address ");
            ip_address = Console.ReadLine();
            Console.Write("Port number: ");
            port = Console.ReadLine();
            Console.Write("Thread number: ");
            thread_number = Console.ReadLine();
            //TYMCZASOWE : uzytkownik wpisuje typ problemu rozwiazywanego
            Console.Write("Problem_name: ");
            problem_names.Add(Console.ReadLine());

            CNNode new_computational_node = new CNNode(ip_address, int.Parse(port), problem_names, byte.Parse(thread_number));
            new_computational_node.Start();

        }
    }
}
