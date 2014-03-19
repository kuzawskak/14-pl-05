using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentuje wiadomość SolveRequestMessage
    /// </summary>
    [XmlRoot(Namespace=ADRES)]
    public class SolveRequest : MessageObject
    {
        /// <summary>
        /// Nazwa problemu do rozwiązania
        /// </summary>
        [XmlElement]
        public String ProblemType
        {
            get { return problemType; }
            set { problemType = value; }
        }
        private String problemType;

        /// <summary>
        /// Timeout w jakim powinno zostać dostarczone rozwiązanie - null oznacza brak timeoutu
        /// </summary>
        public UInt64? SolvingTimeout
        {
            get { return solvingTimeout; }
            set { solvingTimeout = value; }
        }
        private UInt64? solvingTimeout;

        /// <summary>
        /// Właściwość na potrzeby serializacji - zwraca informację czy SolvingTimeout jest ustawione
        /// </summary>
        [XmlIgnore]
        public bool SolvingTimeoutSpecified { get { return SolvingTimeout != null; } }

        /// <summary>
        /// Dane problemu w formacie binarnym
        /// </summary>
        [XmlElement]
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }
        private byte[] data;

        /// <summary>
        /// Konstruktor obiektu SolveRequest
        /// </summary>
        /// <param name="_problemType">Nnazwa problemu</param>
        /// <param name="_data">Dane problemu - nie może być null</param>
        /// <param name="_solvingTimeout">Timeout na dostarczenie rozwiązania - null oznacza brak timeoutu</param>
        public SolveRequest(string _problemType, byte[] _data, UInt64? _solvingTimeout = null) : base()
        {
            if (_data == null)
                throw new ArgumentNullException();

            problemType = _problemType;
            data = _data;
            solvingTimeout = _solvingTimeout;
        }

        /// <summary>
        /// Konstruktor bezparametrowy - tworzy pusty obiekt SolveRequest
        /// </summary>
        public SolveRequest() : base()
        {
            problemType = null;
            data = null;
            solvingTimeout = null;
        }

        public override byte[] GetXmlData()
        {
            XmlMessageSerializer serializer = new XmlMessageSerializer();
            return serializer.SerilizeMessageObject(this, typeof(SolveRequest));
        }
    }
}
