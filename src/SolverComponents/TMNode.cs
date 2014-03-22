using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationNetwork;
using CommunicationXML;

namespace SolverComponents
{
    //TODO: dla Taskmanagera potrzebna bedzie struktura pozwalajaca na wykrycie czy nadeslane dane od CC juz moga byc mergowane

    public class TMNode : SolverNode
    {

        public TMNode(string address, int port, List<string> problem_names, byte computational_power): base (address,port,problem_names,computational_power)
        {
        }

        /// <summary>
        /// Implementacja handlera do komunikacji z serwerem dla TM
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ctx"></param>
        private void ConnectionHandler(byte[] data, ConnectionContext ctx)
        {
            XMLParser parser = new XMLParser(data);
            switch (parser.MessageType)
            {
                case MessageTypes.DivideProblem:
                    DivideProblem((DivideProblem)parser.Message);
                    break;
                case MessageTypes.Solutions:
                    MergeSolution();
                    break;        
                    
            }

        }
        
        //podziel problem
        public void DivideProblem(DivideProblem msg)
        {
            //extract data from message
            //start appropriate method implementing TaskSolver
            ///after dividing problem send PartialProblems message           
        }

        /// <summary>
        /// laczy rozwiazania czesciowe (konieczna implementacja interfejsu z TaskSolvera)
        /// i wysyla do serwera
        /// </summary>
        public void MergeSolution()
        {

        }
    }
}
