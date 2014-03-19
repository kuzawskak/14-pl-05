using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentuje problem częściowy w wiadomościach
    /// </summary>
    [XmlRoot(Namespace=MessageObject.ADRES)]
    public class PartialProblem
    {
        /// <summary>
        /// Id podproblemu nadane przez Task Solver
        /// </summary>
        [XmlElement]
        public UInt64 TaskId
        {
            get { return taskId; }
            set { taskId = value; }
        }
        private UInt64 taskId;

        /// <summary>
        /// Dane podproblemu
        /// </summary>
        [XmlElement]
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }
        private byte[] data;

        /// <summary>
        /// Konstruktor obiektów PartialProblem
        /// </summary>
        /// <param name="_taskId">Id podproblemu nadane przez Task SOlver</param>
        /// <param name="_data">Dane podproblemu</param>
        public PartialProblem(UInt64 _taskId, byte [] _data)
        {
            if (_data == null)
                throw new System.ArgumentNullException();

            taskId = _taskId;
            data = _data;
        }

        /// <summary>
        /// Konstruktor bezparametrowy na potrzeby serializacji
        /// </summary>
        public PartialProblem()
        {
            taskId = 0;
            data = new byte[0];
        }
    }
}
