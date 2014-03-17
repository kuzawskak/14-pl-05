using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    public class Status : MessageObject
    {
        /// <summary>
        /// Id komponentu nadane mu przez serwer
        /// </summary>
        public UInt64 Id
        {
            get { return id; }
            set { id = value; }
        }
        private UInt64 id;

        /// <summary>
        /// Statusy wątków
        /// </summary>
        public List<ComputationalThread> Threads
        {
            get { return threads; }
            set { threads = value; }
        }
        private List<ComputationalThread> threads;

        /// <summary>
        /// Konstruktor klasy Status
        /// </summary>
        /// <param name="_id">Id komponentu</param>
        /// <param name="_threads">Kolekcja stanów wątków</param>
        public Status(UInt64 _id, IEnumerable<ComputationalThread> _threads = null)
        {
            if (_threads == null)
                threads = new List<ComputationalThread>();
            else
                threads = new List<ComputationalThread>(_threads);

            id = _id;
        }



        public override byte[] GetXmlData()
        {
            throw new NotImplementedException();
        }
    }
}
