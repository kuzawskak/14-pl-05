using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentująca wiadomości typu Solutions
    /// </summary>
    [XmlRoot(Namespace=ADRES)]
    public class Solutions : MessageObject
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
        /// Dane wspólne
        /// </summary>
        [XmlElement]
        public byte[] CommonData
        {
            get { return commonData; }
            set { commonData = value; }
        }
        private byte[] commonData;

        /// <summary>
        /// Lista rozwiązań
        /// </summary>
        [XmlArray(ElementName="Solutions")]
        public List<Solution> SolutionsList
        {
            get { return solutions; }
            set { solutions = value; }
        }
        private List<Solution> solutions;

        /// <summary>
        /// Konstruktor obiektów Solutions
        /// </summary>
        /// <param name="_problemType">Nazwa typu problemu</param>
        /// <param name="_id">Id instancji problemu nadane przez serwer</param>
        /// <param name="_commonData">Dane wspólne</param>
        /// <param name="_solutions">Lista rozwiązań</param>
        public Solutions(String _problemType, UInt64 _id, byte[] _commonData, IEnumerable<Solution> _solutions) : base()
        {
            if (_commonData == null || _solutions == null)
                throw new System.ArgumentNullException();

            problemType = _problemType;
            id = _id;
            commonData = _commonData;
            solutions = new List<Solution>(_solutions);
            solutions.Add(Solution.GetEmptySolution());
        }

        /// <summary>
        /// Konstruktor bezparametrowy na potrzeby serializacji
        /// </summary>
        public Solutions() : base()
        {
            problemType = "";
            id = 0;
            commonData = new byte[0];
            solutions = new List<Solution>();
        }

        public override byte[] GetXmlData()
        {
            XmlMessageSerializer serializer = new XmlMessageSerializer();
            return serializer.SerilizeMessageObject(this, typeof(Solutions));
        }
    }
}
