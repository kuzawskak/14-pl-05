using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa służąca do serializacji obiektów pochodnych MessageObject
    /// </summary>
    public class XmlMessageSerializer
    {
        /// <summary>
        /// Mwtoda serializuje obiekty pochodne MessageObject
        /// </summary>
        /// <param name="message">Obiekt do serializacji</param>
        /// <param name="type">Typ wiadomości</param>
        /// <returns></returns>
        public byte[] SerilizeMessageObject(MessageObject message, Type type)
        {
            XmlSerializer serializer = new XmlSerializer(type);
            StringBuilder sb = new StringBuilder();
            StringWriter stringWriter = new StringWriter(sb);
            serializer.Serialize(stringWriter, message);

           // return StringToBytesConverter.GetBytes(sb.ToString());
            return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
