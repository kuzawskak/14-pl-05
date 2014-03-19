using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    class ComputationalNode : Node
    {
        public ComputationalNode(List<string> solvableProblems, byte parallelThreads)
            : base(solvableProblems, parallelThreads)
        {
            // TO JEST KLASA TYLKO DLA SERWERA!
            // moze trzeba ją przenieść?
        }
    }
}
