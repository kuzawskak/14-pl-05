using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationXML;

namespace Components
{
    public class TempPartial
    {
        public ulong PartialId { get; private set; }
        public PartialProblemStatus PartialStatus { get; private set; }

        public TempPartial(ulong partialId, PartialProblemStatus partialStatus)
        {
            PartialId = partialId;
            PartialStatus = partialStatus;
        }
    }

    public class TempProblem
    {
        public ulong ProblemId { get; private set; }
        public ProblemStatus Status { get; private set; }
        public List<TempPartial> PartialProblems { get; private set; }

        public TempProblem(ulong problemId, ProblemStatus status, List<TempPartial> partialProblems)
        {
            ProblemId = problemId;
            Status = status;
            PartialProblems = partialProblems;
        }
    }

    public class Node
    {
        static ulong lastId = 0;

        public ulong Id { get; private set; }
        public List<string> SolvableProblems { get; private set; }
        public byte ParallelThreads { get; private set; }
        public DateTime LastTime { get; private set; }
        public List<ComputationalThread> Threads { get; private set; }

        public List<TempProblem> TemporaryProblems { get; private set; }

        public Node(List<string> solvableProblems, byte parallelThreads)
        {
            Id = ++lastId;
            SolvableProblems = solvableProblems;
            ParallelThreads = parallelThreads;
            Threads = new List<ComputationalThread>();
            for (int i = 0; i < ParallelThreads; ++i) 
                Threads.Add(new ComputationalThread());
            TemporaryProblems = new List<TempProblem>();

            Update();
        }

        public void Update()
        {
            LastTime = DateTime.Now;
        }

        public void Update(List<ComputationalThread> th)
        {
            Threads = th;

            // Aktualizacja listy tymczasowo zapisanych problemów.
            // Usunięcnie z listy Temp elementów, o których jest informacja w StatusMsg
            foreach (var t in th)
            {
                if (t.ProblemInstanceId != null)
                {
                    var tmpProb = TemporaryProblems.Find(x => x.ProblemId == t.ProblemInstanceId);

                    if (tmpProb != null)
                    {
                        if (tmpProb.PartialProblems != null)
                        {
                            tmpProb.PartialProblems.RemoveAll(x => x.PartialId == t.TaskId);

                            if (tmpProb.PartialProblems.Count == 0)
                                TemporaryProblems.Remove(tmpProb);
                        }
                        else
                        {
                            TemporaryProblems.Remove(tmpProb);
                        }
                    }
                }
            }

            Update();
        }

        public long GetAvailableThreads()
        {
            return (long)Threads.Where(x => x.State == CommunicationXML.ComputationalThreadState.Idle).Count();
        }
    }
}
