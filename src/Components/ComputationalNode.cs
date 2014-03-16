using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    public class ComputationalNode : Node
    {
        public ComputationalNode(List<string> solvableProblems, byte parallelThreads)
            : base(solvableProblems, parallelThreads)
        {
        }

        
    }
}
