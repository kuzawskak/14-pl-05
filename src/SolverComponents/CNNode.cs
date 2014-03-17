using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationNetwork;
using CommunicationXML;

namespace SolverComponents
{
    public class CNNode : SolverNode
    {
        //konstruktor
        public CNNode(int port_number, List<NodeType> solvalable_problems)
            : base(port_number, solvalable_problems)
        {
        }

       
        //implementacja handlera dla CN
        private void ConnectionHandler(byte[] data, ConnectionContext ctx)
        {
            XMLParser parser = new XMLParser(data);
            switch (parser.MessageType)
            {
                case MessageTypes.RegisterResponse:
                    //FROM ALBERT: tutaj chyba raczej powinno być : ((RegisterResponse)parser.Message).Id czy coś takiego, bo GetXmlData zwraca binarki gotowe do wysłania
                    // parser.Message.GetXmlData().getComponentId()
                    // parser.Message.GetXmlData().getTimeout()
                    break;
            }
        }


        public void SolveProblem()
        {
        }

    }
}
