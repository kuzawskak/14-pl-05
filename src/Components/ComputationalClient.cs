﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using CommunicationNetwork;
using CommunicationXML;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DVRP;
using System.Reflection;
using System.Reflection.Emit;

namespace Components
{

    public class ComputationalClient
    {
        private NetworkClient client;
        private ulong problem_id;
        private ulong? solving_timeout;
        private string problem_type;
        private string address;
        private int port;
       

        /// <summary>
        /// Konstruktor uwzgledniajacy istnienie problemu (wtedy podajemy id)
        /// </summary>
        /// <param name="problem_id">Id przetwarzanego problemu</param>
        /// <param name="address">Adres ip serwera</param>
        /// <param name="port_number">Port nasłuchu</param>
        /// <param name="solving_timeout">Opcjonalny maksymalny czas przetwarzania problemu</param>
        public ComputationalClient(ulong? problem_id,string address, int port_number, ulong? solving_timeout, string problem_type)
        {
            if (problem_id != null)
            {
                this.problem_id = (ulong)problem_id;
            }
            this.address = address;
            this.port = port_number;
            this.problem_type = problem_type;
            this.solving_timeout = solving_timeout;
            client = new NetworkClient(address, port_number);
        

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


        /// <summary>
        /// Rejestruje problem umieszczony w pliku o podanej sciezce w Serwerze
        /// </summary>
        /// <param name="problem_data_filepath">sciezka do pliku XML z danymi problemu</param>
        /// <returns> wartosc boolowska - czy problem zostal zarejestrowany pomyslnie</returns>
        public bool registerProblem(string problem_data_filepath)
        {


           // XmlDocument problem_data_xml = new XmlDocument();
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead(problem_data_filepath).CopyTo(ms);
            byte[] data = ms.ToArray();

            SolveRequest solve_request = new SolveRequest(problem_type, /*problem_*/data,solving_timeout);

            byte[] register_response = client.Work(solve_request.GetXmlData());
            if (register_response != null)
            {
                XMLParser parser = new XMLParser(register_response);

                if (parser.MessageType == MessageTypes.SolveRequestResponse)
                {
                    SolveRequestResponse register_response_msg = parser.Message as SolveRequestResponse;
                    problem_id = register_response_msg.Id;
                }
            }
            else
            {
                Console.WriteLine("ComputationalClient: problem registration failed");
                return false;
            }

            Console.WriteLine("Problem registered with id = {0}", problem_id);
            return true;
        }

        /// <summary>
        /// Pobiera status przetwarzania problemu i wyswietla na konsole
        /// </summary>
        public void getProblemStatus()
        {
            SolutionRequest solution_request = new SolutionRequest(problem_id);

            byte[] solution_response = client.Work(solution_request.GetXmlData());
            if (solution_response != null)
            {
                XMLParser parser = new XMLParser(solution_response);
                if (parser.MessageType == MessageTypes.Solutions)
                {
                    Solutions solutions_status = parser.Message as Solutions;
                    if (solutions_status != null)
                    {
                        Console.WriteLine("Odebrano ID: {0}, problem {1}", solutions_status.Id, solutions_status.ProblemType);

                        string computing_status = null;
                        if (solutions_status.SolutionsList != null && solutions_status.SolutionsList.Count() == 0)
                            Console.WriteLine("Solution status received: no solutions available on the list");
                        else
                        foreach (Solution s in solutions_status.SolutionsList)
                        {
                            switch (s.Type)
                            {
                                case SolutionType.Final:
                                    computing_status = "Final";

                                    SolutionContainer a = (SolutionContainer)new BinaryFormatter().Deserialize(new MemoryStream(s.Data));
                                    
                                    Console.WriteLine();
                                    Console.WriteLine("MinCost = " + a.MinCost);
                                    Console.WriteLine();
                                    foreach (var q in a.MinPath)
                                    {
                                        foreach (var w in q)
                                            Console.Write(w + ", ");
                                        Console.WriteLine();
                                    }
                                    Console.WriteLine();
                                    foreach (var q in a.Times)
                                    {
                                        foreach (var w in q)
                                            Console.Write(w + ", ");
                                        Console.WriteLine();
                                    }

                                    TimeSpan ts = new TimeSpan((long)s.ComputationsTime*10000000);
                                     Console.WriteLine("Time: " + ts.ToString());

                                    break;
                                case SolutionType.Ongoing:
                                    computing_status = "OnGoing";
                                    break;
                                case SolutionType.Partial:
                                    computing_status = "Partial";
                                   
                                    break;
                            }

                            Console.WriteLine("Task Id: {0}, computation status: {1}", s.TaskId, computing_status);
                        }
                      
                    }
                }
            }
            else
            {
                Console.WriteLine("ComputationalClient: check solutions status failed");
            }
      
        }

        /// <summary>
        /// Usypia klienta na maksymalny czas przetwarzania problemu (SolvingTimeout)
        ///( jesli taki byl ustalony )
        /// </summary>
        public void Work()
        {
              getProblemStatus();         
        }


        //metoda obslugujaca zamkniecie CC na czas przetwarzania
        public void TemporaryCloseForComputation()
        {
            //zapis stanu (port na ktorym nasluchuje i ProblemId) do XML lub innego pliku
        }

    }
}
