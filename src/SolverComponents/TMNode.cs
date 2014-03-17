using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationNetwork;
using CommunicationXML;

namespace SolverComponents
{
    public class TMNode : SolverNode
    {

        //konstruktor
        public TMNode(int port_number,List<NodeType> solvalable_problems):base(port_number, solvalable_problems)
        {
        }

        //implementacja handlera dla TM
        private void ConnectionHandler(byte[] data, ConnectionContext ctx)
        {
            XMLParser parser = new XMLParser(data);
            switch (parser.MessageType)
            {
                case MessageTypes.RegisterResponse:
                   // parser.Message.XMLData.getComponentId()
                    // parser.Message.XMLData.getTimeout()
                    break;
                case MessageTypes.DivideProblem:
                    //DivideProblem();
                    break;

                    break;
            }

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
