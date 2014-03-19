using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentująca obiekty wiadomości typu StatusMessage
    /// </summary>
    [XmlRoot(Namespace=ADRES)]
    public class Status : MessageObject
    {
        /// <summary>
        /// Id komponentu nadane mu przez serwer
        /// </summary>
        [XmlElement]
        public UInt64 Id
        {
            get { return id; }
            set { id = value; }
        }
        private UInt64 id;

        /// <summary>
        /// Statusy wątków
        /// </summary>
        [XmlArray]
        [XmlArrayItem(ElementName=ComputationalThread.THREAD)]
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
        public Status(UInt64 _id, IEnumerable<ComputationalThread> _threads) : base()
        {
            if (_threads == null)
                threads = new List<ComputationalThread>();
            else
                threads = new List<ComputationalThread>(_threads);

            id = _id;
        }

        /// <summary>
        /// Konstruktor bezparametrowy na potrzeby serializacji
        /// </summary>
        public Status() : base()
        {
            id = 0;
            threads = new List<ComputationalThread>();
        }



        public override byte[] GetXmlData()
        {
            XmlMessageSerializer serializer = new XmlMessageSerializer();
            return serializer.SerilizeMessageObject(this, typeof(Status));
        }
    }
}
