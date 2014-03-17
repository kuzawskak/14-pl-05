﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolverComponents;

namespace ComputationalNode
{
    class Program
    {
        static void Main(string[] args)
        {
            string port;
            Console.Write("Port number: ");
            port = Console.ReadLine();

            CNNode new_task_manager = new CNNode(int.Parse(port), null);
            new_task_manager.RegisterNode();
        }
    }
}
