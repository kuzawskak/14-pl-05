using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using CommunicationNetwork;
using CommunicationXML;

namespace Components
{

    public class ComputationalClient
    {
        private NetworkClient client;
        private ulong problem_id;
        private ulong? solving_timeout;
        private string problem_type;
        byte[] problem_data;
        private string address;
        private int port;
        /// <summary>
        /// Konstruktor klasy ComputationalClient do komunikacji z użytkownikiem klastra
        /// </summary>
        /// <param name="address">Adres ip serwera</param>
        /// <param name="port_number">Port nasłuchu</param>
        /// <param name="solving_timeout">Opcjonalny maksymalny czas przetwarzania problemu</param>
        public ComputationalClient(string address, int port_number,ulong? solving_timeout,string problem_type)
        {
            this.address = address;
            this.port = port_number;
            this.problem_type = problem_type;
            this.solving_timeout = solving_timeout;
            client = new NetworkClient(address, port_number);
        }
       

       
        /// <summary>
        /// Rejestruje problem umieszczony w pliku o podanej sciezce w Serwerze
        /// </summary>
        /// <param name="problem_data_filepath">sciezka do pliku XML z danymi problemu</param>
        /// <returns> wartosc boolowska - czy problem zostal zarejestrowany pomyslnie</returns>
        public bool registerProblem(string problem_data_filepath)
        {


            XmlDocument problem_data_xml = new XmlDocument();
            problem_data_xml.Load(problem_data_filepath);
            problem_data = Encoding.Default.GetBytes(problem_data_xml.OuterXml);
            problem_data = new byte[10];
            //will be changed
            SolveRequest solve_request = new SolveRequest(problem_type, problem_data,solving_timeout);
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
         //   while (true)
            {
               // uspij na czas przetwarzania
              //  if (solving_timeout != null)
             //   {
                    Thread.Sleep((int)solving_timeout);

                    //TODO: FIX IT!!
                    //jak uzywamy po raz drugi networkclienta, wiesza sie na metodzie getproblemstatus
                    getProblemStatus();

            }
            
        }


        //metoda obslugujaca zamkniecie CC na czas przetwarzania
        public void TemporaryCloseForComputation()
        {
            //zapis stanu (port na ktorym nasluchuje i ProblemId) do XML lub innego pliku
        }

    }
}
