using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    public class DivideProblem : MessageObject
    {
        /// <summary>
        /// Nazwa typu problemu
        /// </summary>
        public string ProblemType
        {
            get { return problemType; }
            set { problemType = value; }
        }
        private string problemType;

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
        /// Dane problemu
        /// </summary>
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }
        private byte[] data;

        /// <summary>
        /// Liczba dostępnych w danej chwili wątków
        /// </summary>
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
        public DivideProblem(string _problemType, UInt64 _id, byte [] _data, UInt64 _computationalNodes)
        {
            if (_data == null)
                throw new System.ArgumentNullException();

            problemType = _problemType;
            id = _id;
            data = _data;
            computationalNodes = _computationalNodes;
        }

        public override byte[] GetXmlData()
        {
            throw new NotImplementedException();
        }
    }
}
