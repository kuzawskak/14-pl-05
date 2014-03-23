using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CommunicationNetwork;
using CommunicationXML;

namespace SolverComponents
{  
    
    /// <summary>
    /// Klasa bazowa dla komponentow TaskSolvera - TaskManagera i ComputationalNode
    /// </summary>
        public class SolverNode
        {
            protected NodeType type;
            //lista problemow na razie ustalana przez uzytkownika
            protected List<string> problem_names = new List<string>();
            //UWAGA: trzeba spytac czy ten parametr ma byc ustalany przez uzytkownika czy ustalany programowo
            //na razie ustalany przez usera - isttotne bedzie dopiero na poziomie implementacji algorytmow
            protected List<ComputationalThread> threads = new List<ComputationalThread>();
            protected byte computational_power;

            //client is for sending messages every <timeout> seconds to inform of being alive
            protected NetworkClient client;
            protected ulong component_id;
            protected int port;
            protected string address;
            protected ulong id;
            protected DateTime timeout;
            protected int timeout_in_ms;
           
            //konstruktor
            public SolverNode(string address, int port,List<string> problem_names, byte computational_power)
            {
                //ComputationalPower = Environment.ProcessorCount;
                this.address = address;
                this.port = port;
                this.problem_names = problem_names;
                Console.WriteLine("comp power: {0}",(int)computational_power);
                this.computational_power = computational_power;
                for (int i = 0; i < (int)computational_power; i++)
                {
                    threads.Add(new ComputationalThread(ComputationalThreadState.Idle, 1, null, null, problem_names[0]));

                }
            }



            /// <summary>
            /// Rejestracja komponentu u CS
            /// </summary>
            /// <returns></returns>
            public bool Register()
            {

                Register register_message = new Register(type,computational_power,problem_names);
                byte[] register_response = client.Work(register_message.GetXmlData());
                XMLParser parser = new XMLParser(register_response);
                if (parser.MessageType == MessageTypes.RegisterResponse)
                {
                    RegisterResponse register_response_msg = parser.Message as RegisterResponse;
                    id = register_response_msg.Id;
                    timeout = register_response_msg.Timeout;
                    timeout_in_ms =(timeout.Hour *3600+timeout.Minute*60 + timeout.Second) * 1000 + timeout.Millisecond;
                    Console.WriteLine("Received register values: id = {0}, timeout = {1} ms", id, timeout_in_ms);
                }
                else
                {
                    Console.WriteLine("SolverNode: registration failed");
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Wysylanie cyklicznej wiadomosci  o stanie przetwarzania danych w komponencie
            /// </summary>
            public void SendStatusMessage()
            {            
                          
            }       

        }

}
