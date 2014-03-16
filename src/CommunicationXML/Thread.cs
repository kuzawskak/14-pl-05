using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    public enum ThreadState { Idle = 0, Busy = 1 }

    public class Thread
    {
        public ThreadState State { get; private set; }
        public TimeSpan HowLong { get; private set; }
        public ulong? ProblemInstanceId { get; private set; }
        public ulong? TaskId { get; private set; }
        public string ProblemType { get; private set; }

        public Thread()
        {
            State = ThreadState.Idle;
            HowLong = new TimeSpan(0);
            ProblemInstanceId = null;
            TaskId = null;
            ProblemType = null;
        }
    }
}
