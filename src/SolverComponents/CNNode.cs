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
        /// Rozwiazuje nadeslany problem czesciowy
        /// </summary>
        public void SolveProblem()
        {
            //after solving send Solutions Message
            Solutions solutions = new Solutions();
            client.Work(solutions.GetXmlData());
            
        }

        public void SendStatusMessage()
        {
            Status status_msg = new Status(id, threads);
            byte[] response = client.Work(status_msg.GetXmlData());
            XMLParser parser = new XMLParser(response);
            switch (parser.MessageType)
            {
                case MessageTypes.SolvePartialProblems:
                    break;
            }
        }

    }
}
