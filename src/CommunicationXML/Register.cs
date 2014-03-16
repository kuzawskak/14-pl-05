using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    public enum NodeType
    {
        TaskManager,
        ComputationalNode
    };

    public class Register : MessageObject
    {
        private NodeType type;
        private List<string> solvableProblems;
        private byte parallelThreads;

        public byte ParallelThreads
        {
            get { return parallelThreads; }
            set { parallelThreads = value; }
        }

        public List<string> SolvableProblems
        {
            get { return solvableProblems; }
            set { solvableProblems = value; }
        }

        public NodeType Type
        {
            get { return type; }
            set { type = value; }
        }
    }
}
