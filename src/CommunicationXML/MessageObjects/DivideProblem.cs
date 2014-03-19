using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentuje obiekty wiadomości typu DivideProblemMessage
    /// </summary>
    [XmlRoot(Namespace=ADRES)]
    public class DivideProblem : MessageObject
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
        /// Dane problemu
        /// </summary>
        [XmlElement]
        public byte[] Data
        {
            get { return data; }
            set
            {
                if (value == null)
                    throw new InvalidOperationException("data can not be null");

                data = value; 
            }
        }
        private byte[] data;

        /// <summary>
        /// Liczba dostępnych w danej chwili wątków
        /// </summary>
        [XmlElement]
        public UInt64 ComputationalNodes
        {
            get { return computationalNodes; }
            set { computationalNodes = value; }
        }
        private UInt64 computationalNodes;

        /// <summary>
        /// Konstruktor obiektu DivideProblem
        /// </summary>
        /// <param name="_problemType">Typ problemu</param>
        /// <param name="_id">Id problemu nadane przez serwer</param>
        /// <param name="_data">Dane problemu</param>
        /// <param name="_computationalNodes">Liczba dostępnych wątków</param>
        public DivideProblem(string _problemType, UInt64 _id, byte [] _data, UInt64 _computationalNodes) : base()
        {
            if (_data == null)
                throw new System.ArgumentNullException();

            problemType = _problemType;
            id = _id;
            data = _data;
            computationalNodes = _computationalNodes;
        }

        /// <summary>
        /// Konstruktor bezparamentrowy na potrzeby serializacji
        /// </summary>
        public DivideProblem() : base()
        {
            problemType = "";
            id = computationalNodes = 0;
            data = new byte[0];
        }

        public override byte[] GetXmlData()
        {
            if (data == null)
                throw new InvalidOperationException("data can not be null");

            XmlMessageSerializer serilizer = new XmlMessageSerializer();
            return serilizer.SerilizeMessageObject(this, typeof(DivideProblem));
        }
    }
}
