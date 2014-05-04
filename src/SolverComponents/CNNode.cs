using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CommunicationNetwork;
using CommunicationXML;
using System.IO;
using DVRP;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace SolverComponents
{
    public class CNNode : SolverNode
    {
        
        public CNNode(string address, int port, List<string> problem_names, byte computational_power): base (address,port,problem_names,computational_power)
        {
            type = NodeType.ComputationalNode;
        }

        static List<Solution> solution;

        static int counter = 0;

        public static void NodeThreadFunc(SolvePartialProblems msg,PartialProblem pp)
        {
            DateTime start_time = DateTime.Now;
            var asm = Assembly.LoadFile(Path.GetFullPath("DVRP.dll"));
            Type t = asm.GetType("DVRP.DVRP");

            var methodInfo = t.GetMethod("Solve");
            object[] constructor_params = new object[1];
            constructor_params[0] = msg.CommonData;
            var o = Activator.CreateInstance(t, constructor_params);

            if (methodInfo != null)
            {
                object[] param = new object[2];

                param[0] = pp.Data;
                if (msg.SolvingTimeout == null)
                    param[1] = null;
                else param[1] = new TimeSpan((long)msg.SolvingTimeout * 10000000);

                byte[] result = (byte[])
                    methodInfo.Invoke(o, param);

                TimeSpan ts = DateTime.Now - start_time;
                Solution s = new Solution(pp.TaskId, false, SolutionType.Partial, (ulong)ts.TotalSeconds, result);
               
                solution.Add(s);
                ++counter;
                   
     
            
            }
            else Console.WriteLine("Method equal to null");
        }
       
        /// <summary>
        /// Rozwiazuje nadeslany problem czesciowy
        /// </summary>
        public void SolveProblem(SolvePartialProblems msg)
        {
            bool is_solving = true;
            //get the problem with your id
           // List <Solution> 
                solution = new List<Solution>();
            List<PartialProblem> problems_list = msg.PartialProblems;
            //i dla kazdego z listy tworz nowy watek
          //  ThreadStart childref = new ThreadStart(NodeThreadFunc);
            Thread[] threadss = new Thread[problems_list.Capacity];

            int i = 0;
            if (problems_list != null)
            {
                foreach (PartialProblem pp in problems_list)
                {
                    threadss[i] = new Thread(() => NodeThreadFunc(msg,pp));
                    threadss[i].Start();                  
                    i++;
                                    
                }

                i = 0;
                foreach (PartialProblem pp in problems_list)

                {
                    threadss[i].Join();
                    i++;

                }
              
                
            }
            else
            {
                Console.WriteLine("PartialProblems list is null");
            }
            //after solving send Solutions Message

            {
                Console.WriteLine("sending partial solutions");
                Solutions solutions = new Solutions(msg.ProblemType, msg.Id, msg.CommonData, solution);
                foreach (ComputationalThread t in threads)
                {
                    t.State = ComputationalThreadState.Idle;
                }
                client.Work(solutions.GetXmlData());
            }
            
        }


        public void Start()
        {
            client = new NetworkClient(address, port);
            if (Register())
            {
                Console.WriteLine("Component registered successfully with id = {0}", id);
                Work();
            }
        }


        public void SendStatusMessage()
        {
            Status status_msg = new Status(id, threads);
            Console.WriteLine("CN: sending status message...");
            byte[] response = client.Work(status_msg.GetXmlData());
            if (response == null)
            {
                Console.WriteLine("CN: response is equal to null");
            }
            else
            {
                XMLParser parser = new XMLParser(response);
                switch (parser.MessageType)
                {
                    case MessageTypes.SolvePartialProblems:
                       // if (!is_solving)
                        {
                            Console.WriteLine("CN: Received solve partial problems message");
                            SolveProblem((SolvePartialProblems)parser.Message);
                        }
                        break;
                    default:
                        Console.WriteLine("Different message than SolvePartialProblems received");
                        Console.WriteLine(parser.MessageType);
                        break;
                }
            }
        }

        /// <summary>
        /// Wywoluje sendStatusMessage() co timeout 
        /// </summary>
        public void Work()
        {
            while (true)
            {
                Console.WriteLine("Sleep time = {0}", timeout.Second * 1000);
                Thread.Sleep(timeout_in_ms);
                SendStatusMessage();

            }
        }
    }
}
