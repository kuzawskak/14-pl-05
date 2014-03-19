using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa odpowiadająca wiadomości Register message
    /// </summary>
    [XmlRoot(Namespace = ADRES)]
    public class Register : MessageObject
    {
        private const string PROBLEM_NAME = "ProblemName";

        /// <summary>
        /// Typ komponentu, który się rejestruje
        /// </summary>
        [XmlIgnore]
        public NodeType Type
        {
            get { return type; }
            set { type = value; }
        }
        private NodeType type;

        /// <summary>
        /// Właściwość potrzebna do poprawnego serializowania właściwości Type
        /// </summary>
        [XmlElement(ElementName = "Type")]
        public string XmlNodeType
        {
            get
            {
                return Enum.GetName(typeof(NodeType), type);
            }

            set
            {
                type = (NodeType)Enum.Parse(typeof(NodeType), value);
            }

        }

        /// <summary>
        /// Lista nazw problemów, które komponent może obsługiwać
        /// </summary>
        [XmlArray]
        [XmlArrayItem(PROBLEM_NAME)]
        public List<String> SolvableProblems
        {
            get { return solvableProblems; }
            set 
            {
                if (value == null)
                    throw new System.ArgumentNullException("Solvable problems cannot be null");
                solvableProblems = value; 
            }
        }
        private List<String> solvableProblems;

        /// <summary>
        /// Liczba wątków, które mogą być równolegle wykonywane
        /// </summary>
        [XmlElement]
        public byte ParallelThreads
        {
            get { return parallelThreads; }
            set { parallelThreads = value; }
        }
        private byte parallelThreads;

        /// <summary>
        /// Konstruktor obiektu Register
        /// </summary>
        /// <param name="_type">Typ rejestrowanego komponentu</param>
        /// <param name="_parallelThreads">Liczba równoległych wątków, które mogą być efektywnie wykonane</param>
        /// <param name="problemNames">Lista nazw problemów, które komponent może rozwiązywać</param>
        public Register(NodeType _type, byte _parallelThreads, IEnumerable<String> problemNames) : base()
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
            solvableProblems = new List<String>(problemNames);
        }

        /// <summary>
        /// Bezparametrowy konstruktor na potrzeby serializacji Xml
        /// </summary>
        public Register() : base()
        {
            type = 0;
            parallelThreads = 0;
            solvableProblems = new List<string>();
        }

        public override byte[] GetXmlData()
        {
            if (solvableProblems.Count == 0)
                throw new System.InvalidOperationException("Solvable problems cannot be empty");

            XmlMessageSerializer serializer = new XmlMessageSerializer();

            return serializer.SerilizeMessageObject(this, typeof(Register));
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
