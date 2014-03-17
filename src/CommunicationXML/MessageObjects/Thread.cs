using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    /// <summary>
    /// Stany w jakich może znajdować się wątek
    /// </summary>
    public enum ComputationalThreadState 
    { 
        Idle = 0, 
        Busy = 1 
    }

    /// <summary>
    /// Klasa reprezentuje wątek po stronie Task Solvera
    /// </summary>
    public class ComputationalThread
    {
        /// <summary>
        /// Stan wątku
        /// </summary>
        public ComputationalThreadState State
        {
            get { return state; }
            set { state = value; }
        }
        private ComputationalThreadState state;

        /// <summary>
        /// Czas w milisekundach mówiący jak długo wątek jest w aktualnym stanie
        /// </summary>
        public UInt64 HowLong
        {
            get { return howLong; }
            set { howLong = value; }
        }
        private UInt64 howLong;

        /// <summary>
        /// Id instancji problemu - null oznacza, że nie ma akurat żadnej instancji
        /// </summary>
        public UInt64? ProblemInstanceId
        {
            get { return problemInstanceId; }
            set { problemInstanceId = value; }
        }
        private UInt64? problemInstanceId;

        /// <summary>
        /// Id taska, którym zajmuje się wątek - null oznacza, że nie ma akurat żadnej instanjci
        /// </summary>
        public UInt64? TaskId
        {
            get { return taskId; }
            set { taskId = value; }
        }
        private UInt64? taskId;

        /// <summary>
        /// Nazwa aktualnie rozwiązywanego problemu - null oznacza, że aktualnie nie ma żadnego
        /// </summary>
        public string ProblemType
        {
            get { return problemType; }
            set { problemType = value; }
        }
        private string problemType;

        /// <summary>
        /// Konstruktor obiektów ComputationalThread
        /// </summary>
        /// <param name="_state">Stan wątku</param>
        /// <param name="_howLong">Jak długo w tym stanie (w milisekundach)</param>
        /// <param name="_problemInstanceId">Id instanjci problemu</param>
        /// <param name="_taskId">Id taska</param>
        /// <param name="_problemType">Nazwa problemu</param>
        public ComputationalThread(ComputationalThreadState _state, UInt64 _howLong, UInt64? _problemInstanceId,
            UInt64? _taskId, string _problemType)
        {
            if (_state == ComputationalThreadState.Busy)
                if (_problemInstanceId == null || _taskId == null || _problemType == null)
                    throw new System.ArgumentException();

            state = _state;
            howLong = _howLong;
            problemInstanceId = _problemInstanceId;
            taskId = _taskId;
            problemType = _problemType;
        }

        /// <summary>
        /// Konstruktor bezparametrowy - tworzy obiekt reprezentujący bezczynny wątek
        /// </summary>
        public ComputationalThread()
        {
            state = ComputationalThreadState.Idle;
            howLong = 0;
            problemInstanceId = taskId = null;
            problemType = null;
        }
    }
}
