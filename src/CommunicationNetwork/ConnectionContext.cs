using CommunicationXML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CommunicationNetwork
{
    public class ConnectionContext
    {
        System.Threading.Thread t = null;
        byte[] msg = null;

        public void Send(byte[] message)
        {
            if (t != null)
                t.Interrupt();
            msg = message;
        }

        public ConnectionContext(System.Threading.Thread _t)
        {
            t = _t;
        }

        public byte[] GetMessage()
        {
            return msg;
        }
    }
}
