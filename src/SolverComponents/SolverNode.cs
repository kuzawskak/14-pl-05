﻿using System;
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
           
            //konstruktor
            public SolverNode(string address, int port,List<string> problem_names, byte computational_power)
            {
                //ComputationalPower = Environment.ProcessorCount;
                this.address = address;
                this.port = port;
                this.problem_names = problem_names;
                this.computational_power = computational_power;
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
            /// Wywoluje sendStatusMessage() co timeout 
            /// </summary>
            public void Work()
            {
                while(true)
                {
                    Thread.Sleep(timeout.Millisecond);
                    SendStatusMessage();
                    
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
