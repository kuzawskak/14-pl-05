using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    public class XMLParser
    {
        public XMLParser(byte [] data)
        {
            
        }

        private MessageTypes messageType;

        public MessageTypes MessageType
        {
            get { return messageType; }
            set { messageType = value; }
        }

        private MessageObject message;

        public MessageObject Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
