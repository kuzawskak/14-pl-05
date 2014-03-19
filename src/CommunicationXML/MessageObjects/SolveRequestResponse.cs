using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentująca wiadomości typu SolveRequestResponseMessage
    /// </summary>
    [XmlRoot(Namespace=ADRES)]
    public class SolveRequestResponse : MessageObject
    {
        /// <summary>
        /// Id nadane przez serwer problemowi zgłoszonemu przez klienta
        /// </summary>
        [XmlElement]
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
        public SolveRequestResponse(UInt64 _id) : base()
        {
            id = _id;
        }

        /// <summary>
        /// Konstruktor bezparametrowy na potrzeby serializacji
        /// </summary>
        public SolveRequestResponse() : base()
        {
            id = 0;
        }

        public override byte[] GetXmlData()
        {
            XmlMessageSerializer serilizer = new XmlMessageSerializer();
            return serilizer.SerilizeMessageObject(this, typeof(SolveRequestResponse));
        }
    }
}
