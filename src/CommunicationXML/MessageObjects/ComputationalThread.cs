using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
    [XmlRoot(Namespace = MessageObject.ADRES)]
    public class ComputationalThread
    {
        public const string THREAD = "Thread";
        /// <summary>
        /// Stan wątku
        /// </summary>
        [XmlIgnore]
        public ComputationalThreadState State
        {
            get { return state; }
            set { state = value; }
        }
        private ComputationalThreadState state;

        /// <summary>
        /// Właściwość na potrzeby poprawnej serializacji
        /// </summary>
        [XmlElement(ElementName="State")]
        public string XmlState 
        { 
            get
            {
                return Enum.GetName(typeof(ComputationalThreadState), state);
            }
            set
            {
                state = (ComputationalThreadState)Enum.Parse(typeof(ComputationalThreadState), value);
            }
        }

        /// <summary>
        /// Czas w milisekundach mówiący jak długo wątek jest w aktualnym stanie
        /// </summary>
        [XmlElement]
        public UInt64 HowLong
        {
            get { return howLong; }
            set { howLong = value; }
        }
        private UInt64 howLong;

        /// <summary>
        /// Id instancji problemu - null oznacza, że nie ma akurat żadnej instancji
        /// </summary>
        [XmlElement]
        public UInt64? ProblemInstanceId
        {
            get { return problemInstanceId; }
            set { problemInstanceId = value; }
        }
        private UInt64? problemInstanceId;

        /// <summary>
        /// Właściwość na potrzeby serializacji - zwraca informację czy ProblemInstanceId jest ustawione
        /// </summary>
        [XmlIgnore]
        public bool ProblemInstanceIdSpecified { get { return problemInstanceId != null; } }

        /// <summary>
        /// Id taska, którym zajmuje się wątek - null oznacza, że nie ma akurat żadnej instanjci
        /// </summary>
        [XmlElement]
        public UInt64? TaskId
        {
            get { return taskId; }
            set { taskId = value; }
        }
        private UInt64? taskId;

        /// <summary>
        /// Właściwość na potrzeby serializacji - zwraca informację o tym, czy TaskId jest ustawione
        /// </summary>
        public bool TaskIdSpecified { get { return taskId != null; } }

        /// <summary>
        /// Nazwa aktualnie rozwiązywanego problemu - null oznacza, że aktualnie nie ma żadnego
        /// </summary>
        [XmlElement]
        public String ProblemType
        {
            get { return problemType; }
            set { problemType = value; }
        }
        private String problemType;

        /// <summary>
        /// Konstruktor obiektów ComputationalThread
        /// </summary>
        /// <param name="_state">Stan wątku</param>
        /// <param name="_howLong">Jak długo w tym stanie (w milisekundach)</param>
        /// <param name="_problemInstanceId">Id instanjci problemu</param>
        /// <param name="_taskId">Id taska</param>
        /// <param name="_problemType">Nazwa problemu</param>
        public ComputationalThread(ComputationalThreadState _state, UInt64 _howLong, UInt64? _problemInstanceId,
            UInt64? _taskId, String _problemType)
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
