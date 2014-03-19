using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    public enum PartialProblemStatuses
    {
        New,
        Sended, // ongoing
        Solved
    }

    class PartialProblem
    {
        public ulong TaskId { get; private set; }
        public byte[] Data { get; set; }
        public PartialProblemStatuses PartialProblemStatus { get; set; }
        public ulong ComputationsTime { get; set; }
        public bool TimeoutOccured { get; set; }

        public PartialProblem(ulong taskId, byte[] data)
        {
            TaskId = taskId;
            Data = data;
            PartialProblemStatus = PartialProblemStatuses.New;
            ComputationsTime = 0;
            TimeoutOccured = false;
        }
    }
}
