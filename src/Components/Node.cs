using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationXML;

namespace Components
{
    /// <summary>
    /// Częściowy, tymczasowo zapamiętany problem.
    /// </summary>
    public class TempPartial
    {
        /// <summary>
        /// ID fragmentu
        /// </summary>
        public ulong PartialId { get; private set; }

        /// <summary>
        /// Status
        /// </summary>
        public PartialProblemStatus PartialStatus { get; private set; }

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="partialId">ID fragmentu</param>
        /// <param name="partialStatus">Status</param>
        public TempPartial(ulong partialId, PartialProblemStatus partialStatus)
        {
            PartialId = partialId;
            PartialStatus = partialStatus;
        }
    }

    /// <summary>
    /// Klasa reprezentuje status tymczasowo zapamiętanego problemu.
    /// </summary>
    public class TempProblem
    {
        /// <summary>
        /// ID problemu
        /// </summary>
        public ulong ProblemId { get; private set; }

        /// <summary>
        /// Status problemu
        /// </summary>
        public ProblemStatus Status { get; private set; }

        /// <summary>
        /// Lista PartialProblems
        /// </summary>
        public List<TempPartial> PartialProblems { get; private set; }

        /// <summary>
        /// Konstruktor tymczasowo wysłanego problemu.
        /// </summary>
        /// <param name="problemId">ID problemu</param>
        /// <param name="status">Status</param>
        /// <param name="partialProblems">Lista PartialProblems</param>
        public TempProblem(ulong problemId, ProblemStatus status, List<TempPartial> partialProblems)
        {
            ProblemId = problemId;
            Status = status;
            PartialProblems = partialProblems;
        }
    }

    /// <summary>
    /// Klasa bazowa dla zapamiętanego stanu CN i TM w serwerze.
    /// </summary>
    public class Node
    {
        private static ulong lastId = 0;

        /// <summary>
        /// ID modułu
        /// </summary>
        public ulong Id { get; private set; }

        /// <summary>
        /// Lista rozwiązywalnych problemów.
        /// </summary>
        public List<string> SolvableProblems { get; private set; }

        /// <summary>
        /// Ilość wątków.
        /// </summary>
        public byte ParallelThreads { get; private set; }

        /// <summary>
        /// Czas ostatniej aktywności.
        /// </summary>
        public DateTime LastTime { get; private set; }

        /// <summary>
        /// Lista stanu wątków.
        /// </summary>
        public List<ComputationalThread> Threads { get; private set; }

        /// <summary>
        /// Lista tymczasowo zapamiętanych problemów.
        /// Użycie jest konieczne w celu wycofania stanu wysłanego zadania w przypadku awarii CN/TM przed przesłaniem Statusu.
        /// </summary>
        public List<TempProblem> TemporaryProblems { get; private set; }

        /// <summary>
        /// Konstruktor stanu modułu.
        /// </summary>
        /// <param name="solvableProblems">Rozwiązywalne problemy</param>
        /// <param name="parallelThreads">Ilość wątków.</param>
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

        /// <summary>
        /// Aktualizacja czasu ostatniej aktywności.
        /// </summary>
        public void Update()
        {
            LastTime = DateTime.Now;
        }

        /// <summary>
        /// Aktualizacja stanu wątków.
        /// </summary>
        /// <param name="th">Wątki</param>
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

        /// <summary>
        /// Metoda obliczająca dostępne wątki.
        /// </summary>
        /// <returns>Dostępne wątki.</returns>
        public long GetAvailableThreads()
        {
            return (long)Threads.Where(x => x.State == CommunicationXML.ComputationalThreadState.Idle).Count();
        }
    }
}
