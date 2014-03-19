using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CommunicationXML
{
    public class XMLParser
    {
        private string xmlMessage;

        /// <summary>
        /// Publiczny konstruktor obiektu XmlParser
        /// </summary>
        /// <param name="data">Dane Xml jako tablica bajtów, które chcemy zmienić na obiekt jednego z typów pochodnych MessageObject</param>
        public XMLParser(byte [] data)
        {
            xmlMessage = StringToBytesConverter.GetString(data);
            object result;

            if(null != (result = TryDeserilize(typeof(Register))))
            {
                messageType = MessageTypes.Register;
                message = (Register)result;
                return;
            }

            if (null != (result = TryDeserilize(typeof(RegisterResponse))))
            {
                messageType = MessageTypes.RegisterResponse;
                message = (RegisterResponse)result;
                return;
            }

            if(null != (result = TryDeserilize(typeof(SolutionRequest))))
            {
                messageType = MessageTypes.SolutionRequest;
                message = (SolutionRequest)result;
                return;
            }

            throw new ArgumentException("data can not be deserialized");
        }

        
        /// <summary>
        /// Właściwość zawiera typ zdeserializowanej wiadomości
        /// </summary>
        public MessageTypes MessageType
        {
            get { return messageType; }
            set { messageType = value; }
        }
        private MessageTypes messageType;

        
        /// <summary>
        /// Właściwość zawiera obiekt zdeserializowanej wiadomości
        /// </summary>
        public MessageObject Message
        {
            get { return message; }
            set { message = value; }
        }
        private MessageObject message;

        private object TryDeserilize(Type type)
        {
            XmlSerializer serializer = new XmlSerializer(type);

            try
            {
                TextReader reader = new StringReader(xmlMessage);
                return serializer.Deserialize(reader);
            }
            catch(InvalidOperationException)
            {
                return null;
            }
        }
    }
}
