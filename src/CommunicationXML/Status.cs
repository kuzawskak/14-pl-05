using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    public class Status : MessageObject
    {
        private ulong id;
        private List<Thread> threads;

        public List<Thread> Threads
        {
            get { return threads; }
            set { threads = value; }
        }

        public ulong Id
        {
            get { return id; }
            set { id = value; }
        }
    }
}
