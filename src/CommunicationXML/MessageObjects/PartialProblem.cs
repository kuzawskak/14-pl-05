using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa reprezentuje problem częściowy w wiadomościach
    /// </summary>
    public class PartialProblem
    {
        /// <summary>
        /// Id podproblemu nadane przez Task Solver
        /// </summary>
        public UInt64 TaskId
        {
            get { return taskId; }
            set { taskId = value; }
        }
        private UInt64 taskId;

        /// <summary>
        /// Dane podproblemu
        /// </summary>
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
    }
}
