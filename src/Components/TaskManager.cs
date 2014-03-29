using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    /// <summary>
    /// Klasa reprezentuje stan TM zapamiętany w serwerze.
    /// </summary>
    class TaskManager : Node
    {
        /// <summary>
        /// Konstruktor TM.
        /// </summary>
        /// <param name="solvableProblems">Rozwiązywalne problemy.</param>
        /// <param name="parallelThreads">Liczba dostępnych wątków.</param>
        public TaskManager(List<string> solvableProblems, byte parallelThreads)
            : base(solvableProblems, parallelThreads)
        {
            // TO JEST KLASA TYLKO DLA SERWERA!
        }
    }
}
