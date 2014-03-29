using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    /// <summary>
    /// Status fragmentu problemu.
    /// </summary>
    public enum PartialProblemStatus
    {
        New,
        Sended, // ongoing
        Solved
    }

    /// <summary>
    /// Partial Problem.
    /// </summary>
    public class PartialProblem
    {
        /// <summary>
        /// ID fragmentu.
        /// </summary>
        public ulong TaskId { get; private set; }

        /// <summary>
        /// Dane.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Status.
        /// </summary>
        public PartialProblemStatus PartialProblemStatus { get; set; }

        /// <summary>
        /// Czas obliczeń.
        /// </summary>
        public ulong ComputationsTime { get; set; }

        /// <summary>
        /// Właściwość informuje czy wystąpił timeout w czasie obliczeń.
        /// </summary>
        public bool TimeoutOccured { get; set; }

        public PartialProblem(ulong taskId, byte[] data)
        {
            TaskId = taskId;
            Data = data;
            PartialProblemStatus = PartialProblemStatus.New;
            ComputationsTime = 0;
            TimeoutOccured = false;
        }
    }
}
