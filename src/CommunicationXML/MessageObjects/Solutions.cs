using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentująca wiadomości typu Solutions
    /// </summary>
    public class Solutions : MessageObject
    {
        /// <summary>
        /// Nazwa typu problemu
        /// </summary>
        public String ProblemType
        {
            get { return problemType; }
            set { problemType = value; }
        }
        private String problemType;

        /// <summary>
        /// Id instancji problemu nadane przez serwer
        /// </summary>
        public UInt64 Id
        {
            get { return id; }
            set { id = value; }
        }
        private UInt64 id;

        /// <summary>
        /// Dane wspólne
        /// </summary>
        public byte[] CommonData
        {
            get { return commonData; }
            set { commonData = value; }
        }
        private byte[] commonData;

        /// <summary>
        /// Lista rozwiązań
        /// </summary>
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
        public Solutions(String _problemType, UInt64 _id, byte[] _commonData, IEnumerable<Solution> _solutions)
        {
            if (_commonData == null || _solutions == null)
                throw new System.ArgumentNullException();

            problemType = _problemType;
            id = _id;
            commonData = _commonData;
            solutions = new List<Solution>(_solutions);
        }

        public override byte[] GetXmlData()
        {
            throw new NotImplementedException();
        }
    }
}
