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
using System.Linq.Expressions;

namespace SolverComponents
{
    public class CNNode : SolverNode
    {

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



        public CNNode(string address, int port, List<string> problem_names, byte computational_power)
            : base(address, port, problem_names, computational_power)
        {
            type = NodeType.ComputationalNode;
            threadss = new Thread[computational_power];

        }

        List<Solution> solution;
        Thread[] threadss;

        public void NodeThreadFunc(/*object o, MethodInfo methodInfo,*/ SolvePartialProblems msg, PartialProblem pp, ComputationalThread ct)
        {

            DateTime start_time = DateTime.Now;

            var asm = Assembly.Load(AssemblyName.GetAssemblyName(Path.GetFullPath("DVRP.dll")));
            Type t = asm.GetType("DVRP.DVRP");

            var methodInfo = t.GetMethod("Solve");
            object[] constructor_params = new object[1];
            constructor_params[0] = msg.CommonData;
            var o = Activator.CreateInstance(t, constructor_params);
            /*********event handler*/

            var eventInfo = t.GetEvent("SolutionsMergingFinished");
            Type tDelegate = eventInfo.EventHandlerType;

            MethodInfo addHandler = eventInfo.GetAddMethod();

            Type returnType = GetDelegateReturnType(tDelegate);
            Console.WriteLine(returnType.ToString());

            DynamicMethod handler = new DynamicMethod("", null,
                                  GetDelegateParameterTypes(tDelegate), t);

            ILGenerator ilgen = handler.GetILGenerator();

            Type[] showParameters = { typeof(String) };
            MethodInfo simpleShow = typeof(CNNode).GetMethod("SetComputationalThreadIdle");
            Console.WriteLine(simpleShow.ToString());

            ilgen.Emit(OpCodes.Ldstr, "string");//ct.ProblemInstanceId.Value);//Ldstr,"This event handler was constructed at run time.");
            ilgen.Emit(OpCodes.Call, simpleShow);
            //   ilgen.Emit(OpCodes.Pop);
            ilgen.Emit(OpCodes.Ret);

            // Complete the dynamic method by calling its CreateDelegate
            // method. Use the "add" accessor to add the delegate to
            // the invocation list for the event.
            //
            Delegate dEmitted = handler.CreateDelegate(tDelegate);
            addHandler.Invoke(o, new Object[] { dEmitted });




            if (methodInfo != null)
            {
                object[] param = new object[2];

                param[0] = pp.Data;
                if (msg.SolvingTimeout == null)
                    param[1] = null;
                else param[1] = new TimeSpan((long)msg.SolvingTimeout * 10000000);

                byte[] result = (byte[])methodInfo.Invoke(o, param);

                TimeSpan ts = DateTime.Now - start_time;
                Solution s = new Solution(pp.TaskId, false, SolutionType.Partial, (ulong)ts.TotalSeconds, result);

                solution.Add(s);
                Console.WriteLine("sending partial solutions");
                Solutions solutions = new Solutions(msg.ProblemType, msg.Id, msg.CommonData, solution);

                client.Work(solutions.GetXmlData());

                SetComputationalThreadIdle(msg.Id, pp.TaskId);
            }
            else Console.WriteLine("Method equal to null");
        }

        public void SetComputationalThreadIdle(ulong problemid, ulong taskid)
        {
            Console.WriteLine("Setting Computationalthread to idle");
            ComputationalThread ct = threads.Find(x => (x.ProblemInstanceId == problemid && x.TaskId == taskid));
            threads.Remove(ct);
            threads.Add(new ComputationalThread(ComputationalThreadState.Idle, 1, null, null, problem_names[0]));

        }


        /// <summary>
        /// Rozwiazuje nadeslany problem czesciowy
        /// </summary>
        public void SolveProblem(SolvePartialProblems msg)
        {

            //get the problem with your id
            solution = new List<Solution>();
            List<PartialProblem> problems_list = msg.PartialProblems;
            //i dla kazdego z listy tworz nowy watek
           // Thread[] threadss = new Thread[problems_list.Capacity];

            int i = 0;
            if (problems_list != null)
            {
                foreach (PartialProblem pp in problems_list)
                {
                    ComputationalThread ct = threads.Find(x => x.State == ComputationalThreadState.Idle);
                    threads.Remove(ct);
                    ComputationalThread new_thread = new ComputationalThread(ComputationalThreadState.Busy, 0, msg.Id, pp.TaskId, msg.ProblemType);
                    threads.Add(new_thread);

                    threadss[i] = new Thread(() => NodeThreadFunc(/*o,methodInfo*,*/msg, pp, new_thread));
                    threadss[i].Start();
                    i++;

                }

            }
            else
            {
                Console.WriteLine("PartialProblems list is null");
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
                        // Thread problem_solve_thread = new 
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


        public void StopWork()
        {
            for (int i = 0; i < threadss.Length; i++)
            {
                threadss[i].Join();
            }
        }
    }
}
