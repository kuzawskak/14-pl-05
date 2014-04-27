using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CommunicationNetwork;
using CommunicationXML;

namespace SolverComponents
{
    public class CNNode : SolverNode
    {
        
        public CNNode(string address, int port, List<string> problem_names, byte computational_power): base (address,port,problem_names,computational_power)
        {
            type = NodeType.ComputationalNode;
        }

       
        /// <summary>
        /// Rozwiazuje nadeslany problem czesciowy
        /// </summary>
        public void SolveProblem(SolvePartialProblems msg)
        {

            //get the problem with your id
            List <Solution> solution = new List<Solution>();
            List<PartialProblem> problems_list = msg.PartialProblems;

            var asm = Assembly.LoadFile("DVRP.dll");
            Type t = asm.GetType("DVRP.DVRP");

            var methodInfo = t.GetMethod("Solve", new Type[] { typeof(byte[]), typeof(int) });
            if (problems_list != null)
            {
                foreach (PartialProblem pp in problems_list)
                {
                    var o = Activator.CreateInstance(t);
                    object[] param = new object[3];
                    
                    param[0] = msg.CommonData;
                    param[1] = pp.Data;
                    param[2] = msg.SolvingTimeout;
                    var result = methodInfo.Invoke(o, param);
                    byte[] ans =(byte[])result;

                    Solution s = new Solution(pp.TaskId, false, SolutionType.Partial, 1000, ans);
                    
                    solution.Add(s);
                }
            }
            else
            {
                Console.WriteLine("PartialProblems list is null");
            }
            //after solving send Solutions Message
            Solutions solutions = new Solutions(msg.ProblemType, msg.Id, msg.CommonData, solution);
            client.Work(solutions.GetXmlData());
            
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
                        Console.WriteLine("CN: Received solve partial problems message"); 
                        SolveProblem((SolvePartialProblems)parser.Message);
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
