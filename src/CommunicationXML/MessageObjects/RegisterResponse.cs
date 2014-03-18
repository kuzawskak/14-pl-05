using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa odpowiadająca wiadomości ReisterResponseMessage
    /// </summary>
    [XmlRoot(Namespace = ADRES)]
    public class RegisterResponse : MessageObject
    {
        
        /// <summary>
        /// Id komponentu nadane przez Server
        /// </summary>
        [XmlElement]
        public UInt64 Id
        {
            get { return id; }
            set { id = value; }
        }
        private UInt64 id;

        /// <summary>
        /// Timeout serwera
        /// </summary>
        [XmlElement(DataType="time")]
		public DateTime Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }
        private DateTime timeout;

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="id">Id komponentu, który nadał Server</param>
        /// <param name="timeout">Timeout serwera</param>
        public RegisterResponse(UInt64 _id, TimeSpan _timeout)
            : base()
        {
            //Sprawdzenie poprawności parametrów
            if (_timeout == null)
                throw new System.ArgumentNullException();

            id = _id;
            timeout = Convert.ToDateTime(_timeout.ToString());
        }

        /// <summary>
        /// Bezparametrowy konstruktor na potrzeby serializacji Xml
        /// </summary>
        public RegisterResponse() : base()
        {
            timeout = new DateTime(0);
            id = 0;
        }


        public override byte[] GetXmlData()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(RegisterResponse));
            StringBuilder sb = new StringBuilder();
            StringWriter stringWriter = new StringWriter(sb);
            serializer.Serialize(stringWriter, this);

            return StringToBytesConverter.GetBytes(sb.ToString());
        }
    }
}
