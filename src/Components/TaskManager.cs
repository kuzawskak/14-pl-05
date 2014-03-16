using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    public class TaskManager : Node
    {
        public TaskManager(List<string> solvableProblems, byte parallelThreads)
            : base(solvableProblems, parallelThreads)
        {
        }
    }
}
