using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa odpowiadająca wiadomości Register message
    /// </summary>
    public class Register : MessageObject
    {
        /// <summary>
        /// Liczba wątków, które mogą być równolegle wykonywane
        /// </summary>
        public byte ParallelThreads
        {
            get { return parallelThreads; }
            set { parallelThreads = value; }
        }
        private byte parallelThreads;

        /// <summary>
        /// Lista nazw problemów, które komponent może obsługiwać
        /// </summary>
        public List<string> SolvableProblems
        {
            get { return solvableProblems; }
            set { solvableProblems = value; }
        }
        private List<string> solvableProblems;

        /// <summary>
        /// Typ komponentu, który się rejestruje
        /// </summary>
        public NodeType Type
        {
            get { return type; }
            set { type = value; }
        }
        private NodeType type;

        /// <summary>
        /// Konstruktor obiektu Register
        /// </summary>
        /// <param name="_type">Typ rejestrowanego komponentu</param>
        /// <param name="_parallelThreads">Liczba równoległych wątków, które mogą być efektywnie wykonane</param>
        /// <param name="problemNames">Lista nazw problemów, które komponent może rozwiązywać</param>
        public Register(NodeType _type, byte _parallelThreads, IEnumerable<String> problemNames)
        {
            //Sprawdzamy poprawność argumentu _type - jeśli jest nullem lub na złą wartość poleci wyjątek
            Enum.GetName(typeof(NodeType), _type);

            //Sprawdzenie poprawności arrgumentu _parallelThreads
            if (_parallelThreads < 1)
                throw new System.ArgumentException("_parallelThreads must be positive");
            
            //Sprawdzenie poprawności argumentu problemNames
            if (problemNames == null || problemNames.Count() < 1)
                throw new System.ArgumentException("problemNames argument was invalid");


            type = _type;
            parallelThreads = _parallelThreads;
            solvableProblems = new List<string>(problemNames);
        }
    }

    /// <summary>
    /// Wyliczenie, które zawiera nazwy elementów mogących zarejestrować się do Servera
    /// </summary>
    public enum NodeType
    {
        TaskManager,
        ComputationalNode
    };
}
