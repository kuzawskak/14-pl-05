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
using System.Reflection.Emit;
using System.Windows.Forms;

namespace SolverComponents
{
    //TODO: dla Taskmanagera potrzebna bedzie struktura pozwalajaca na wykrycie czy nadeslane dane od CC juz moga byc mergowane



    public class TMNode : SolverNode
    {

        //static DateTime start_time;
        private static List<SolverRegisteredProblem> ongoing_problems = new List<SolverRegisteredProblem>();

        private Dictionary<UInt64, List<byte[]>> PartialSolutions = new Dictionary<ulong, List<byte[]>>();

        public TMNode(string address, int port, List<string> problem_names, byte computational_power)
            : base(address, port, problem_names, computational_power)
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
        public void DivideProblemSimulation(DivideProblem msg, ComputationalThread thread)
        {
            if (msg == null)
            {
                Console.WriteLine("TM: DivideProblemSimulation failure - msg is null");
                return;
            }
            ulong? timeout_in_miliseconds = timeout != null ? (ulong?)timeout.Millisecond : null;
            ulong computational_nodes = msg.ComputationalNodes;

            var asm = Assembly.Load(AssemblyName.GetAssemblyName(Path.GetFullPath("DVRP.dll")));
            Type t = asm.GetType("DVRP.DVRP");

            var methodInfo = t.GetMethod("DivideProblem");
            object[] constructor_params = new object[1];
            constructor_params[0] = msg.Data;
            var o = Activator.CreateInstance(t, constructor_params);

            object[] param = new object[1];
            param[0] = (int)msg.ComputationalNodes;
            byte[][] result = (byte[][])methodInfo.Invoke(o, param);

            // start_time = DateTime.Now;

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

            SolverRegisteredProblem srp = new SolverRegisteredProblem(msg.Id, divided_problems);
            ongoing_problems.Add(srp);
            SolvePartialProblems solve_partial_problems_msg = new SolvePartialProblems(msg.ProblemType, msg.Id, msg.Data, timeout_in_miliseconds, divided_problems);

            byte[] response = client.Work(solve_partial_problems_msg.GetXmlData());
            //in this case it's expected value
            if (response == null)
            {
                Console.WriteLine("TM: DivideProblems message sent successfully");
            }
            SetComputationalThreadIdle((ulong)thread.ProblemInstanceId, (ulong)thread.TaskId);



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


        private Type[] GetDelegateParameterTypes(Type d)
        {
            if (d.BaseType != typeof(MulticastDelegate))
                throw new ApplicationException("Not a delegate.");

            MethodInfo invoke = d.GetMethod("Invoke");
            if (invoke == null)
                throw new ApplicationException("Not a delegate.");

            ParameterInfo[] parameters = invoke.GetParameters();
            Type[] typeParameters = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                typeParameters[i] = parameters[i].ParameterType;
            }
            return typeParameters;
        }

        private Type GetDelegateReturnType(Type d)
        {
            if (d.BaseType != typeof(MulticastDelegate))
                throw new ApplicationException("Not a delegate.");

            MethodInfo invoke = d.GetMethod("Invoke");
            if (invoke == null)
                throw new ApplicationException("Not a delegate.");

            return invoke.ReturnType;
        }



        public void DivideThreadFunc(DivideProblem msg, ComputationalThread thread)
        {
            if (msg == null)
            {
                Console.WriteLine("TM: DivideProblemSimulation failure - msg is null");
                return;
            }
            ulong? timeout_in_miliseconds = timeout != null ? (ulong?)timeout.Millisecond : null;
            ulong computational_nodes = msg.ComputationalNodes;

            var asm = Assembly.Load(AssemblyName.GetAssemblyName(Path.GetFullPath(msg.ProblemType + ".dll")));
            if (asm == null)
                throw new ArgumentException("Nie mozna znalezc modulu " + msg.ProblemType + ".dll");
            //Type t = asm.GetType("DVRP.DVRP");
            Type t = asm.GetTypes().Where(x => x.IsSubclassOf(typeof(UCCTaskSolver.TaskSolver))).FirstOrDefault();
            if (t == null)
                throw new ArgumentException("Brak odpowiedniej klasy w module.");

            var methodInfo = t.GetMethod("DivideProblem");
            object[] constructor_params = new object[1];
            constructor_params[0] = msg.Data;
            var o = Activator.CreateInstance(t, constructor_params);
            /*********event handler*/

            //var eventInfo = t.GetEvent("ProblemDividingFinished");
            //Type tDelegate = eventInfo.EventHandlerType;

            //MethodInfo addHandler = eventInfo.GetAddMethod();

            //Type returnType = GetDelegateReturnType(tDelegate);
            //Console.WriteLine(returnType.ToString());

            //DynamicMethod handler = new DynamicMethod("", null,
            //                      GetDelegateParameterTypes(tDelegate), t);

            //ILGenerator ilgen = handler.GetILGenerator();

            //Type[] showParameters = { typeof(String) };
            //MethodInfo simpleShow = typeof(CNNode).GetMethod("SetComputationalThreadIdle");
            //Console.WriteLine(simpleShow.ToString());

            //ilgen.Emit(OpCodes.Ldstr, "string");//ct.ProblemInstanceId.Value);//Ldstr,"This event handler was constructed at run time.");
            //ilgen.Emit(OpCodes.Call, simpleShow);
            ////   ilgen.Emit(OpCodes.Pop);
            //ilgen.Emit(OpCodes.Ret);

            //// Complete the dynamic method by calling its CreateDelegate
            //// method. Use the "add" accessor to add the delegate to
            //// the invocation list for the event.
            ////
            //Delegate dEmitted = handler.CreateDelegate(tDelegate);
            //addHandler.Invoke(o, new Object[] { dEmitted });







            object[] param = new object[1];
            param[0] = (int)msg.ComputationalNodes;
            byte[][] result = null;

            try
            {
                result = (byte[][])methodInfo.Invoke(o, param);
            }
            catch (Exception e)
            {
                MessageBox.Show("Moduł '" + msg.ProblemType + ".dll' zakończył działanie z błędem:\n\n" + e.InnerException.Message, "Błąd modułu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            //start_time = DateTime.Now;

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

            SolverRegisteredProblem srp = new SolverRegisteredProblem(msg.Id, divided_problems);
            ongoing_problems.Add(srp);
            SolvePartialProblems solve_partial_problems_msg = new SolvePartialProblems(msg.ProblemType, msg.Id, msg.Data, timeout_in_miliseconds, divided_problems);

            byte[] response = client.Work(solve_partial_problems_msg.GetXmlData());
            //in this case it's expected value
            if (response == null)
            {
                Console.WriteLine("TM: DivideProblems message sent successfully");
            }
            SetComputationalThreadIdle((ulong)thread.ProblemInstanceId, (ulong)thread.TaskId);

        }


        public void SetComputationalThreadIdle(ulong problemid, ulong taskid)
        {
            Console.WriteLine("Setting Computationalthread to idle");
            ComputationalThread ct = threads.Find(x => (x.ProblemInstanceId == problemid && x.TaskId == taskid));
            threads.Remove(ct);
            threads.Add(new ComputationalThread(ComputationalThreadState.Idle, 1, null, null, problem_names[0]));

        }

        public void MergeThreadFunc(Solutions msg, ComputationalThread thread)
        {

            var asm = Assembly.Load(AssemblyName.GetAssemblyName(Path.GetFullPath(msg.ProblemType + ".dll")));//Assembly.LoadFile(Path.GetFullPath("DVRP.dll"));
            //Type t = asm.GetType("DVRP.DVRP");
            Type t = asm.GetTypes().Where(x => x.IsSubclassOf(typeof(UCCTaskSolver.TaskSolver))).FirstOrDefault();
            if (t == null)
                throw new ArgumentException("Brak odpowiedniej klasy w module.");

            var methodInfo = t.GetMethod("MergeSolution");

            object[] constructor_param = new object[1];
            constructor_param[0] = msg.CommonData;

            var o = Activator.CreateInstance(t, constructor_param);

            Solutions solutions_msg;
            object[] param = new object[1];
            param[0] = PartialSolutions[msg.Id].ToArray();
            try
            {
                methodInfo.Invoke(o, param);
            }
            catch (Exception e)
            {
                MessageBox.Show("Moduł '" + msg.ProblemType + ".dll' zakończył działanie z błędem:\n\n" + e.InnerException.Message, "Błąd modułu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            var meth = t.GetMethod("get_Solution");

            byte[] ans = (byte[])meth.Invoke(o, null);

            // TimeSpan ts = DateTime.Now - start_time;

            Solution final_solution = new Solution(msg.Id, false, SolutionType.Final, thread.HowLong, ans);
            List<Solution> solution_to_send = new List<Solution>();
            solution_to_send.Add(final_solution);

            solutions_msg = new Solutions(msg.ProblemType, msg.Id, msg.CommonData, solution_to_send);


            client.Work(solutions_msg.GetXmlData());
            SetComputationalThreadIdle((ulong)thread.ProblemInstanceId, (ulong)thread.TaskId);

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
                        DivideProblem msg = (DivideProblem)parser.Message;
                        ComputationalThread ct = threads.Find(x => x.State == ComputationalThreadState.Idle);
                        threads.Remove(ct);
                        ComputationalThread new_thread = new ComputationalThread(ComputationalThreadState.Busy, 0, msg.Id, 1, msg.ProblemType);
                        threads.Add(new_thread);
                        Thread t = new Thread(() => DivideThreadFunc(msg, new_thread));
                        t.Start();
                        break;
                    case MessageTypes.Solutions:
                        Console.WriteLine("TM: try merge solutions is starting");
                        Solutions sol_msg = (Solutions)parser.Message;

                        Console.WriteLine("TM try to Merge solution of the problem with id = {0}  into final", sol_msg.Id);
                        //check if solved problems number is equal to 

                        if (!PartialSolutions.ContainsKey(sol_msg.Id))
                            PartialSolutions.Add(sol_msg.Id, new List<byte[]>());

                        SolverRegisteredProblem p = ongoing_problems.Find(x => sol_msg.Id == x.ProblemId);
                        if (p != null)
                        {
                            foreach (Solution s in sol_msg.SolutionsList)
                            {
                                if (s.Data != null)
                                {
                                    p.MarkAsSolved((ulong)s.TaskId);
                                    PartialSolutions[sol_msg.Id].Add(s.Data);
                                    p.SetComputationsTime(s.ComputationsTime);

                                }
                            }
                        }
                        else Console.WriteLine("Not foung Solver registered problem");


                        if (p != null && p.IsProblemSolved())
                        {

                            ongoing_problems.Remove(p);
                            Console.WriteLine("TM: Ready to merge solution");
                            //one common solution

                            ComputationalThread sol_ct = threads.Find(x => x.State == ComputationalThreadState.Idle);
                            threads.Remove(sol_ct);
                            ComputationalThread new_sol_thread = new ComputationalThread(ComputationalThreadState.Busy, p.computation_time, sol_msg.Id, 1, sol_msg.ProblemType);
                            threads.Add(new_sol_thread);
                            Thread sol_t = new Thread(() => MergeThreadFunc(sol_msg, new_sol_thread));
                            sol_t.Start();
                        }

                        else
                        {
                            client.Work(sol_msg.GetXmlData());
                        }

                        break;
                    default:
                        Console.WriteLine("received other message: " + response.GetType().ToString());
                        client.Work(status_msg.GetXmlData());
                        break;


                }
            }
        }
    }
}

