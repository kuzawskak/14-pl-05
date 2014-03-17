using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentująca obiekty Solution w wiadomościach
    /// </summary>
    public class Solution
    {
        /// <summary>
        /// Id podproblemu nadane przez Task Solver
        /// </summary>
        public UInt64 Id
        {
            get { return id; }
            set { id = value; }
        }
        private UInt64 id;

        /// <summary>
        /// Informacja czy wystąpił timeout
        /// </summary>
        public Boolean TimeoutOccured
        {
            get { return timeoutOccured; }
            set { timeoutOccured = value; }
        }
        private Boolean timeoutOccured;

        /// <summary>
        /// Typ rozwiązania - częściowe, całkowite, jeszcze wykonywane
        /// </summary>
        public SolutionType Type
        {
            get { return type; }
            set { type = value; }
        }
        private SolutionType type;

        /// <summary>
        /// Sumaryczny czas obliczeń przez wszystkie wątki
        /// </summary>
        public UInt64 ComputationsTime
        {
            get { return computationsTime; }
            set { computationsTime = value; }
        }
        private UInt64 computationsTime;

        /// <summary>
        /// Dane rozwiązania
        /// </summary>
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
        public Solution(UInt64 _id, Boolean _timeoutOccured, SolutionType _type, UInt64 _computationsTime, byte[] _data)
        {
            if (_data == null)
                throw new System.ArgumentNullException();

            id = _id;
            timeoutOccured = _timeoutOccured;
            type = _type;
            computationsTime = _computationsTime;
            data = _data;
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
