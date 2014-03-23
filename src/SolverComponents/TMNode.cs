using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CommunicationNetwork;
using CommunicationXML;

namespace SolverComponents
{
    //TODO: dla Taskmanagera potrzebna bedzie struktura pozwalajaca na wykrycie czy nadeslane dane od CC juz moga byc mergowane

    public class TMNode : SolverNode
    {

        public TMNode(string address, int port, List<string> problem_names, byte computational_power): base (address,port,problem_names,computational_power)
        {
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

            List<PartialProblem> divided_problems = new List<PartialProblem>();
            //tworzymy tyle podproblemow ile dostepnych nodów 
            
            for (int i = 0; i < (int)computational_nodes; i++)
            {
                Console.WriteLine("adding partial problem to divided problems");
                PartialProblem pp = new PartialProblem((ulong)i, msg.Data);
                divided_problems.Add(pp);
            }

            if (divided_problems.Count == 0)
            {
                Console.WriteLine("TM: Cannot send msg with no partial problems set");
                return;
            }
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
            //one common solution
            Solution final_solution  =  new Solution(null,false,SolutionType.Final,1000,msg.CommonData);
            List<Solution> solution_to_send = new List<Solution>();
            solution_to_send.Add(final_solution);

            Solutions solutions_msg = new Solutions(msg.ProblemType, msg.Id, msg.CommonData, solution_to_send);
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
                        //DivideProblem((DivideProblem)parser.Message);
                        Thread.Sleep(2000);
                        Console.WriteLine("TM divides and send problem to CS");
                        DivideProblemSimulation((DivideProblem)parser.Message);
                        break;
                    case MessageTypes.Solutions:
                        Console.WriteLine("TM: try merge solutions is starting");
                        //wiadomosc od CC o obliczeniach, wysylana do TM po zakonczeniu kazdego zadania
                        Thread.Sleep(2000);

                        tryMergeSolution((Solutions)parser.Message);
                        break;

                }
            }
        }
    }
}
