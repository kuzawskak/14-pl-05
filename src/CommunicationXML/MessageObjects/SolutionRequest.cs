using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentująca wiadomości typu SolutionRequestMessage
    /// </summary>
    [XmlRoot(Namespace = ADRES)]
    public class SolutionRequest : MessageObject
    {
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
        /// Konstruktor obiektów SolutionRequest
        /// </summary>
        /// <param name="_id">Id nadane przez serwer</param>
        public SolutionRequest(UInt64 _id) : base()
        {
            id = _id;
        }

        /// <summary>
        /// Konstruktor bezparametrowy na potrzeby serializacji
        /// </summary>
        public SolutionRequest() : base()
        {
            id = 0;
        }

        public override byte[] GetXmlData()
        {
            XmlMessageSerializer serilizer = new XmlMessageSerializer();
            return serilizer.SerilizeMessageObject(this, typeof(SolutionRequest));
        }
    }
}
