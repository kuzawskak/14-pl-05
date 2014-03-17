using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentująca wiadomości typu SolutionRequestMessage
    /// </summary>
    public class SolutionRequest : MessageObject
    {
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
        /// Konstruktor obiektów SolutionRequest
        /// </summary>
        /// <param name="_id">Id nadane przez serwer</param>
        public SolutionRequest(UInt64 _id)
        {
            id = _id;
        }

        public override byte[] GetXmlData()
        {
            throw new NotImplementedException();
        }
    }
}
