﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationNetwork;
using CommunicationXML;

namespace SolverComponents
{
    class SolverNode
    {

        enum NodeType
        {
            problemType1,
            problemType2
        }



        class TMNode : SolverNode
        {

            private List<NodeType> SolvalableProblems;
            private int ComputationalPower;
            private Listener listener;
            private int time;
            private ulong ComponentId;
            private System.Threading.ThreadPool working_threads;
            //konstruktor
            public TMNode(List<NodeType> solvalable_problems, int port_number)
            {
                SolvalableProblems = solvalable_problems;
                ComputationalPower = Environment.ProcessorCount;
                listener = new Listener(port_number, ConnectionHandler);
                listener.Start();
            }


            //implementacja handlera dla TM i CN beda sie roznily
            private void ConnectionHandler(byte[] data, ConnectionContext ctx)
            {

                XMLParser parser = new XMLParser(data);
                switch (parser.MessageType)
                {
                    case MessageTypes.RegisterResponse:
                        // parser.Message.XMLData.getComponentId()
                        // parser.Message.XMLData.getTimeout()
                        //jak otrzyma 
                        Work();
                        break;
                }
            }



            public void Work()
            {
                bool is_problem_solved = false;
              //TODO: start threads work

                while (!is_problem_solved)
                {
                    System.Threading.Thread.Sleep(time);

                   
                    SendStatusMessage();
                }
            }

            //rejestracja u CS
            public void RegisterNode()
            {
                //send xml as byte array with node parameters
                byte[] node_params = null;
                ConnectionContext cc = new ConnectionContext(System.Threading.Thread.CurrentThread);
                cc.Send(node_params);

            }

            //wysylanie statusu przetwarzania danych w komponencie
            public void SendStatusMessage()
            {
                byte[] problem_status = null;
                /*
                 * TODO: get status from every working thread
                 */
                ConnectionContext cc = new ConnectionContext(System.Threading.Thread.CurrentThread);
                cc.Send(problem_status); 
               
            }




        }
    }

    
}
