using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentująca obiekty Solution w wiadomościach
    /// </summary>
    [XmlRoot(Namespace=MessageObject.ADRES)]
    public class Solution
    {
        /// <summary>
        /// Id podproblemu nadane przez Task Solver
        /// </summary>
        [XmlElement]
        public UInt64? TaskId
        {
            get { return id; }
            set { id = value; }
        }
        private UInt64? id;

        /// <summary>
        /// Właściwość na potrzeby serializacji - zwraca informację o ustawieniu Id
        /// </summary>
        [XmlIgnore]
        public bool TaskIdSpecified { get { return id != null; } }

        /// <summary>
        /// Informacja czy wystąpił timeout
        /// </summary>
        [XmlElement]
        public Boolean TimeoutOccured
        {
            get { return timeoutOccured; }
            set { timeoutOccured = value; }
        }
        private Boolean timeoutOccured;

        /// <summary>
        /// Typ rozwiązania - częściowe, całkowite, jeszcze wykonywane
        /// </summary>
        [XmlIgnore]
        public SolutionType Type
        {
            get { return type; }
            set { type = value; }
        }
        private SolutionType type;

        /// <summary>
        /// Właściwość na potrzeby serializacji - zwraca Type jako string
        /// </summary>
        [XmlElement(ElementName="Type")]
        public string XmlType 
        {
            get
            {
                return Enum.GetName(typeof(SolutionType), type);
            } 
            set
            {
                type = (SolutionType)Enum.Parse(typeof(SolutionType), value);
            }
        }

        /// <summary>
        /// Sumaryczny czas obliczeń przez wszystkie wątki
        /// </summary>
        [XmlElement]
        public UInt64 ComputationsTime
        {
            get { return computationsTime; }
            set { computationsTime = value; }
        }
        private UInt64 computationsTime;

        /// <summary>
        /// Dane rozwiązania
        /// </summary>
        [XmlElement]
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }
        private byte[] data;

        /// <summary>
        /// Konstruktor obiektów Solution
        /// </summary>
        /// <param name="_id">Id podproblemu nadane przez Task Solver</param>
        /// <param name="_timeoutOccured">Czy wystąpił timeout</param>
        /// <param name="_type">Typ rozwiązania</param>
        /// <param name="_computationsTime">Sumaryczny czas obliczeń przez wątki</param>
        /// <param name="_data">Dane rozwiązania</param>
        public Solution(UInt64? _id, Boolean _timeoutOccured, SolutionType _type, UInt64 _computationsTime, byte[] _data)
        {
            if (_data == null)
                throw new System.ArgumentNullException();

            id = _id;
            timeoutOccured = _timeoutOccured;
            type = _type;
            computationsTime = _computationsTime;
            data = _data;
        }

        /// <summary>
        /// Konstruktor obiektów Solution
        /// </summary>
        /// <param name="_timeoutOccured">Czy wystąpił timeout</param>
        /// <param name="_type">Typ rozwiązania</param>
        /// <param name="_computationsTime">Sumaryczny czas obliczeń przez wątki</param>
        /// <param name="_data">Dane rozwiązania</param>
        public Solution(Boolean _timeoutOccured, SolutionType _type, UInt64 _computationsTime, byte[] _data)
            : this(null, _timeoutOccured, _type, _computationsTime, _data)
        {
        }

        /// <summary>
        /// Konstruktor bezparametrowy na potrzeby serializacji
        /// </summary>
        public Solution()
        {
            id = null;
            timeoutOccured = false;
            type = SolutionType.Final;
            computationsTime = 0;
            data = new byte[0];
        }

        /// <summary>
        /// Metoda zwraca pusty obiekt Solution
        /// </summary>
        /// <returns>Obiekt solution</returns>
        public static Solution GetEmptySolution()
        {
            Solution s = new Solution();
            s.ComputationsTime = ulong.MaxValue;

            return s;
        }
    }

    /// <summary>
    /// Wyliczneie typów rozwiązań
    /// </summary>
    public enum SolutionType
    {
        Ongoing,
        Partial,
        Final
    };
}
