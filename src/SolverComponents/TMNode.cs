using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CommunicationNetwork;
using CommunicationXML;
using DVRP;
using System.IO;

namespace SolverComponents
{
    //TODO: dla Taskmanagera potrzebna bedzie struktura pozwalajaca na wykrycie czy nadeslane dane od CC juz moga byc mergowane



    public class TMNode : SolverNode
    {

        DateTime start_time;
        private static List<SolverRegisteredProblem> ongoing_problems = new List<SolverRegisteredProblem>();

        private List<byte[]> PartialSolutions = new List<byte[]>();
        public TMNode(string address, int port, List<string> problem_names, byte computational_power): base (address,port,problem_names,computational_power)
        {
            type = NodeType.TaskManager;
        }

        
        //podziel problem
        public void DivideProblem(DivideProblem msg)
        {

            //extract data from message
            //start appropriate method implementing TaskSolver
            ///after dividing problem send PartialProblems message           
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

        /// <summary>
        /// metoda na potzreby symulacji to TEST-weeku
        /// dlatego korzystamy w sumie tylko z liczby CN
        /// </summary>
        /// <param name="msg">DivideProblem message object</param>
        public void DivideProblemSimulation(DivideProblem msg)
        {
            if (msg == null)
            {
                Console.WriteLine("TM: DivideProblemSimulation failure - msg is null");
                return;
            }
            ulong? timeout_in_miliseconds = timeout != null ? (ulong?)timeout.Millisecond : null;
            ulong computational_nodes = msg.ComputationalNodes;

            var asm = Assembly.LoadFile(Path.GetFullPath("DVRP.dll"));
            Type t = asm.GetType("DVRP.DVRP");

            var methodInfo = t.GetMethod("DivideProblem");
            object[] constructor_params = new object[1];
            constructor_params[0] = msg.Data;           
            var o = Activator.CreateInstance(t,constructor_params);

            object[] param = new object[1];
            param[0] = (int)msg.ComputationalNodes;
            byte[][] result = (byte[][])methodInfo.Invoke(o, param);

            start_time = DateTime.Now;
        
            List<PartialProblem> divided_problems = new List<PartialProblem>();
            //tworzymy tyle podproblemow ile dostepnych nodów 
            
            for (int i = 0; i < (int)computational_nodes; i++)
            {
                Console.WriteLine("adding partial problem to divided problems");
                PartialProblem pp = new PartialProblem((ulong)i, result[i]);           
                divided_problems.Add(pp);
            }


            if (divided_problems.Count == 0)
            {
                Console.WriteLine("TM: Cannot send msg with no partial problems set");
                return;
            }

            SolverRegisteredProblem srp = new SolverRegisteredProblem(msg.Id,divided_problems);
            ongoing_problems.Add(srp);
            SolvePartialProblems solve_partial_problems_msg = new SolvePartialProblems(msg.ProblemType,msg.Id, msg.Data, timeout_in_miliseconds, divided_problems);

            byte[] response = client.Work(solve_partial_problems_msg.GetXmlData());
            //in this case it's expected value
            if (response == null)
            {
                Console.WriteLine("TM: DivideProblems message sent successfully");
            }


        }

        /// <summary>
        /// Wywoluje sendStatusMessage() co timeout 
        /// </summary>
        public void Work()
        {
            while (true)
            {
                Thread.Sleep(timeout_in_ms);
                SendStatusMessage();

            }
        }

        /// <summary>
        /// laczy rozwiazania czesciowe (konieczna implementacja interfejsu z TaskSolvera)
        /// i wysyla do serwera
        /// </summary>
        public void MergeSolution()
        {

        }
        public void tryMergeSolution(Solutions msg)
        {
            Console.WriteLine("TM try to Merge solution of the problem with id = {0}  into final", msg.Id);
            //check if solved problems number is equal to 

            SolverRegisteredProblem p = ongoing_problems.Find(x => msg.Id ==x.ProblemId);
            if (p != null)
            {
                foreach (Solution s in msg.SolutionsList)
                {
                    p.MarkAsSolved((ulong)s.TaskId);
                    PartialSolutions.Add(s.Data);
                }
            }
            else Console.WriteLine("Not foung Solver registered problem");

            Solutions solutions_msg = null;
            if (p!=null && p.IsProblemSolved())
            {

                ongoing_problems.Remove(p);
                Console.WriteLine("TM: Ready to merge solution");
                //one common solution

                var asm = Assembly.LoadFile(Path.GetFullPath("DVRP.dll"));
                Type t = asm.GetType("DVRP.DVRP");

                var methodInfo = t.GetMethod("MergeSolution");
               
                object[] constructor_param = new object[1];
                constructor_param[0] =  msg.CommonData;
                var o = Activator.CreateInstance(t,constructor_param);
              
                object[] param = new object[1];
                param[0] = PartialSolutions.ToArray();               
                methodInfo.Invoke(o, param);
                var meth = t.GetMethod("get_Solution");


                byte[] ans = (byte[])meth.Invoke(o, null);

                TimeSpan ts = DateTime.Now - start_time;

                Solution final_solution = new Solution(msg.Id, false, SolutionType.Final,(ulong)ts.TotalSeconds, ans);
                List<Solution> solution_to_send = new List<Solution>();
                solution_to_send.Add(final_solution);

                solutions_msg = new Solutions(msg.ProblemType, msg.Id, msg.CommonData, solution_to_send);
            }

            else
            {
                Console.WriteLine("TM: unready to merge");
                solutions_msg = msg;
            }

            byte[] response = client.Work(solutions_msg.GetXmlData());
            if (response == null)
            {
                Console.WriteLine("final solutions sent successfully");
            }

        }
        public void SendStatusMessage()
        {
            Status status_msg = new Status(id, threads);
            Console.WriteLine("TM: sending status message...");
            byte[] response = client.Work(status_msg.GetXmlData());
            if (response == null)
            {
                Console.WriteLine("TM: response is set to null");
            }
            else
            {
                XMLParser parser = new XMLParser(response);
                switch (parser.MessageType)
                {
                    case MessageTypes.DivideProblem:
                        Console.WriteLine("TM divides and send problem to CS");
                        DivideProblemSimulation((DivideProblem)parser.Message);
                        break;
                    case MessageTypes.Solutions:
                        Console.WriteLine("TM: try merge solutions is starting");
                        //wiadomosc od CC o obliczeniach, wysylana do TM po zakonczeniu kazdego zadania
                      //  Thread.Sleep(2000);

                        tryMergeSolution((Solutions)parser.Message);
                        break;

                }
            }
        }
    }
}
