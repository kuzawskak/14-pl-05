using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    enum ProblemStatus
    {
        New,
        WaitingForDivision,
        Divided,
        WaitingForPartialSolutions,
        PartiallySolved,
        WaitingForSolutions,
        Solved
    }

    class Problem
    {
        private static ulong lastId = 0;

        public ulong Id { get; private set; }
        public byte[] Data { get; private set; }    // base64 string | Data i SolutionData
        public byte[] CommonData { get; private set; } // base64 string
        public ulong? SolvingTimeout { get; private set; }
        public string ProblemType { get; private set; }
        public ProblemStatus Status { get; set; }
        public List<PartialProblem> PartialProblems { get; private set; }

        public bool TimeoutOccured { get; private set; }
        public ulong ComputationsTime { get; private set; }

        /// <summary>
        /// Konstruktor Problemu
        /// </summary>
        /// <param name="problemType">Typ problemu</param>
        /// <param name="data">Dane</param>
        /// <param name="timeout">Timeout</param>
        public Problem(string problemType, byte[] data, ulong? timeout = null)
        {
            Id = ++lastId;
            Data = data;
            SolvingTimeout = timeout;
            ProblemType = problemType;
            CommonData = null;
            PartialProblems = null;
            ComputationsTime = 0;
            TimeoutOccured = false;

            Status = ProblemStatus.New;
        }

        /// <summary>
        /// Zapisywanie rozwiązań częściowych i rozwiązania ogólnego.
        /// </summary>
        /// <param name="solutions">Lista rozwiązań</param>
        public void SetSolutions(List<CommunicationXML.Solution> solutions)
        {
            CommunicationXML.Solution finalSolution = solutions.Find(x => x.Type == CommunicationXML.SolutionType.Final);

            if (finalSolution != null)
            {
                TimeoutOccured = finalSolution.TimeoutOccured;

                if (TimeoutOccured)
                {
                    Status = ProblemStatus.Solved;
                    return;
                }

                Data = finalSolution.Data;
                Status = ProblemStatus.Solved;
                ComputationsTime = finalSolution.ComputationsTime;
                // TODO: ComputationsTime sumowane w TM? TAK?
            }
            else
            {
                foreach (CommunicationXML.Solution s in solutions)
                {
                    if (s.TaskId == null)
                        continue; // Błąd który nie powinien wystąpić

                    // Timeout dla wątku oznacza timeout dla zadania? NIE!
                    //if(s.TimeoutOccured)
                    //{
                    //    TimeoutOccured = true;
                    //    Status = ProblemStatus.Solved;
                    //    //return;
                    //}

                    PartialProblem pp = PartialProblems.Find(x => x.TaskId == s.TaskId);

                    if (pp != null)
                    {
                        // TODO: które wątki skończyły pracę? NIE?
                        pp.PartialProblemStatus = s.Type == CommunicationXML.SolutionType.Partial ? 
                            PartialProblemStatuses.Solved : pp.PartialProblemStatus;

                        pp.ComputationsTime = s.ComputationsTime;
                        pp.TimeoutOccured = s.TimeoutOccured;

                        if (s.Type == CommunicationXML.SolutionType.Partial && !s.TimeoutOccured)
                            pp.Data = s.Data;
                    }
                }

                // jesli wszystkie solved to zadanie partially solved
                if (PartialProblems.Where(x => x.PartialProblemStatus == PartialProblemStatuses.Solved).Count() == PartialProblems.Count)
                {
                    // Jeśli solved to był timeout - mozna to usunac jesli timeout dla watku nie oznacza timeout dla zadania!
                    if(Status != ProblemStatus.Solved)
                        Status = ProblemStatus.PartiallySolved;
                }
            }
        }

        /// <summary>
        /// Dodaje odebrane Partial Problems
        /// </summary>
        /// <param name="commonData">Common Data</param>
        /// <param name="partialProblems">Podzielone fragmenty problemow</param>
        public void SetPartialProblems(byte[] commonData, List<CommunicationXML.PartialProblem> partialProblems)
        {
            CommonData = commonData;
            PartialProblems = new List<PartialProblem>();

            foreach (var x in partialProblems)
                PartialProblems.Add(new PartialProblem(x.TaskId, x.Data));

            Status = ProblemStatus.Divided;
        }

        /// <summary>
        /// Tworzy listę podproblemów do rozwiązania dla CN.
        /// </summary>
        /// <param name="maxCount">Dostepna liczba wątków - max liczba podproblemów do odesłania</param>
        /// <returns>Lista problemów</returns>
        public List<CommunicationXML.PartialProblem> GetPartialProblemListToSolve(long maxCount)
        {
            List<CommunicationXML.PartialProblem> problems = new List<CommunicationXML.PartialProblem>();

            for (int i = 0; i < maxCount; ++i)
            {
                PartialProblem pp = PartialProblems.Find(x => x.PartialProblemStatus == PartialProblemStatuses.New);

                if (pp == null)
                    break;

                problems.Add(new CommunicationXML.PartialProblem(pp.TaskId, pp.Data));

                pp.PartialProblemStatus = PartialProblemStatuses.Sended;
            }

            if (PartialProblems.Where(x => x.PartialProblemStatus == PartialProblemStatuses.New).Count() == 0)
                Status = ProblemStatus.WaitingForPartialSolutions;

            return problems;
        }
    }
}
