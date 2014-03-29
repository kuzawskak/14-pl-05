using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    /// <summary>
    /// Klasa reprezentująca stan podłaczonego CN w serwerze.
    /// </summary>
    class ComputationalNode : Node
    {
        /// <summary>
        /// Konstruktor tworzący nowy wpis o CN w serwerze.
        /// </summary>
        /// <param name="solvableProblems">Rozwiązywane problemy</param>
        /// <param name="parallelThreads">Liczba dostępnych wątków</param>
        public ComputationalNode(List<string> solvableProblems, byte parallelThreads)
            : base(solvableProblems, parallelThreads)
        {
            // KLASA TYLKO DLA SERWERA!
        }
    }
}
