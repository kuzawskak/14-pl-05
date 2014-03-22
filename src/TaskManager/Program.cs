using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolverComponents;

namespace TaskManager
{
    class Program
    {
        static void Main(string[] args)
        {
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

            TMNode new_task_manager = new TMNode(ip_address, int.Parse(port), problem_names, byte.Parse(thread_number));
            new_task_manager.Start();

        }
    }
}
