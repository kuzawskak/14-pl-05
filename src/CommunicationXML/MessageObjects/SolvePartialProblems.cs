using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentuje wiadomości typu SolvePartialProblems
    /// </summary>
    [XmlRoot(Namespace=ADRES)]
    public class SolvePartialProblems : MessageObject
    {
        /// <summary>
        /// Nazwa typu problemu
        /// </summary>
        [XmlElement]
        public String ProblemType
        {
            get { return problemType; }
            set { problemType = value; }
        }
        private String problemType;

        /// <summary>
        /// Id instancji problemu nadane przez serwer
        /// </summary>
        [XmlElement]
        public UInt64 Id
        {
            get { return id; }
            set { id = value; }
        }
        private UInt64 id;

        /// <summary>
        /// Dane wspólne dla wszystkich części
        /// </summary>
        [XmlElement]
        public byte[] CommonData
        {
            get { return commonData; }
            set { commonData = value; }
        }
        private byte[] commonData;

        /// <summary>
        /// Czas na dostarczenie rozwiązania - null oznacza brak limitu
        /// </summary>
        [XmlElement]
        public UInt64? SolvingTimeout
        {
            get { return solvingTimeout; }
            set { solvingTimeout = value; }
        }
        private UInt64? solvingTimeout;

        /// <summary>
        /// Właściwość na potrzeby serializacji - zwraca informację czy SolvingTimeout jest ustawiony
        /// </summary>
        [XmlIgnore]
        public bool SolvingTimeoutSpecified { get { return solvingTimeout != null; } }

        /// <summary>
        /// Lista problemów częściowych
        /// </summary>
        [XmlArray]
        public List<PartialProblem> PartialProblems
        {
            get { return partialProblems; }
            set { partialProblems = value; }
        }
        private List<PartialProblem> partialProblems;

        /// <summary>
        /// Konstruktor obiektów klasy SolvePartialProblem
        /// </summary>
        /// <param name="_problemType">Nazwa typu problemu</param>
        /// <param name="_id">Id instancji problemu nadane przez serwer</param>
        /// <param name="_commonData">Dane wspólne dla problemów częściowych</param>
        /// <param name="_solvingTimeout">Czas dostarczenia rozwiązania - null oznacza brak limitu</param>
        /// <param name="_partialProblems">Kolekcja problemów częściowych</param>
        public SolvePartialProblems(string _problemType, UInt64 _id, byte [] _commonData,
            UInt64? _solvingTimeout, IEnumerable<PartialProblem> _partialProblems) : base()
        {
            if (_commonData == null || _partialProblems == null)
                throw new System.ArgumentNullException();

            problemType = _problemType;
            id = _id;
            commonData = _commonData;
            solvingTimeout = _solvingTimeout;
            partialProblems = new List<PartialProblem>(_partialProblems);
        }

        /// <summary>
        /// Konstruktor bezparametrowy na potrzeby serializacji
        /// </summary>
        public SolvePartialProblems() : base()
        {
            problemType = "";
            id = 0;
            commonData = new byte[0];
            solvingTimeout = null;
            partialProblems = new List<PartialProblem>();
        }


        public override byte[] GetXmlData()
        {
            if (partialProblems.Count < 1)
                throw new InvalidOperationException("PartialProblems can not be empty for serialization");

            XmlMessageSerializer serilizer = new XmlMessageSerializer();
            return serilizer.SerilizeMessageObject(this, typeof(SolvePartialProblems));
        }
    }
}
