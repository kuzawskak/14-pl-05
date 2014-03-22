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
        
        public CNNode(string address, int port, List<string> problem_names, byte computational_power): base (address,port,problem_names,computational_power)
        {
        }

       
        /// <summary>
        /// Implementacja handlera do komunikacji z serwerem dla CN
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ctx"></param>
        private void ConnectionHandler(byte[] data, ConnectionContext ctx)
        {
            XMLParser parser = new XMLParser(data);
            switch (parser.MessageType)
            {
                case MessageTypes.SolvePartialProblems:
                    break;
            }
        }

        /// <summary>
        /// Rozwiazuje nadeslany problem czesciowy
        /// </summary>
        public void SolveProblem()
        {
            //after solving send Solutions Message
            Solutions solutions = new Solutions();
            client.Work(solutions.GetXmlData());
            
        }

    }
}
