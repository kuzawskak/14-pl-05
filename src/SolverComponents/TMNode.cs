using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationNetwork;
using CommunicationXML;

namespace SolverComponents
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




        //konstruktor
        public TMNode(List<NodeType> solvalable_problems, int port_number)
        {
            SolvalableProblems = solvalable_problems;
            ComputationalPower = Environment.ProcessorCount;
            listener.Start();
            listener = new Listener(port_number, ConnectionHandler);
           
        }


        //implementacja handlera dla TM
        //wiadomosci jakie moze otrzymac klient:
        //potwierdzenie rejestracji, status zadania (zawarte w nim ewentualnie rozwiazanie koncowe)
        private void ConnectionHandler(byte[] data, ConnectionContext ctx)
        {
            XMLParser parser = new XMLParser(data);
            switch (parser.MessageType)
            {
                case MessageTypes.RegisterResponse:
                   // parser.Message.XMLData.getComponentId()
                    // parser.Message.XMLData.getTimeout()
         
                    break;
            }

        }
        


        //rejestracja u CS
        public void RegisterComponent()
        {
            //send xml with 
            byte[] problem_data = null;
            ConnectionContext cc = new ConnectionContext(System.Threading.Thread.CurrentThread);
            cc.Send(problem_data);  
            //wyslij wiadomosc 
        }
        public void SendStatusMessage()
        {

        }


        //podziel problem
        public void DivideProblem()
        {
        }

        //polacz rozwiazania
        public void MergeSolution()
        {
        }

        //

    }
}
