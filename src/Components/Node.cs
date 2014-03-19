using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationXML;

namespace Components
{
    public class Node
    {
        static ulong lastId = 0;

        public ulong Id { get; private set; }
        public List<string> SolvableProblems { get; private set; }
        public byte ParallelThreads { get; private set; }
        public DateTime LastTime { get; private set; }
        public List<ComputationalThread> Threads { get; private set; }

        public List<Tuple<ulong, List<ulong>>> TemporaryProblems { get; private set; }

        public Node(List<string> solvableProblems, byte parallelThreads)
        {
            Id = ++lastId;
            SolvableProblems = solvableProblems;
            ParallelThreads = parallelThreads;
            Threads = new List<ComputationalThread>();
            TemporaryProblems = new List<Tuple<ulong, List<ulong>>>();

            Update();
        }

        public void Update()
        {
            LastTime = DateTime.Now;
        }

        public void Update(List<ComputationalThread> th)
        {
            Threads = th;

            // Aktualizacja listy tymczasowo zapisanych problemów problemów.
            foreach (var t in th)
            {
                if (t.ProblemInstanceId != null)
                {
                    var tmpProb = TemporaryProblems.Find(x => x.Item1 == t.ProblemInstanceId);

                    if (tmpProb != null)
                    {
                        if (tmpProb.Item2 != null)
                        {
                            tmpProb.Item2.RemoveAll(x => x == t.TaskId);

                            if (tmpProb.Item2.Count == 0)
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
