using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentująca wiadomości typu SolveRequestResponseMessage
    /// </summary>
    public class SolveRequestResponse : MessageObject
    {
        /// <summary>
        /// Id nadane przez serwer problemowi zgłoszonemu przez klienta
        /// </summary>
        public UInt64 Id
        {
            get { return id; }
            set { id = value; }
        }
        private UInt64 id;

        /// <summary>
        /// Konstruktor obiektów SolveRequestResponse
        /// </summary>
        /// <param name="_id">Id problemu nadane przez serwer</param>
        public SolveRequestResponse(UInt64 _id)
        {
            id = _id;
        }

        public override byte[] GetXmlData()
        {
            throw new NotImplementedException();
        }
    }
}
