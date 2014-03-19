using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    class TaskManager : Node
    {
        public TaskManager(List<string> solvableProblems, byte parallelThreads)
            : base(solvableProblems, parallelThreads)
        {
            // TO JEST KLASA TYLKO DLA SERWERA!
            // moze trzeba ją przenieść?
        }
    }
}
