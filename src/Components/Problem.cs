﻿using System;
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
        public string Data { get; private set; }    // base64 string | Data i SolutionData
        public string CommonData { get; private set; } // base64 string
        public ulong? SolvingTimeout { get; private set; }
        public string ProblemType { get; private set; }
        public ProblemStatus Status { get; set; }
        public List<PartialProblem> PartialProblems { get; private set; }

        // Timeout dla jednego watku oznacza timeout dla calego zadania!
        public bool TimeoutOccured { get; private set; }
        public ulong ComputationsTime { get; private set; }

        public Problem(string problemType, string data, ulong? timeout = null)
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

        /* FOR: Albert
        public void SetSolutions(List<Solution> solutions)
        {
            Solution finalSolution = solutions.Find(x => x.Type == SolutionTypes.Final);

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
                foreach (Solution s in solutions)
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
                        pp.PartialProblemStatus = s.Type == SolutionTypes.Partial ? 
                            PartialProblemStatuses.Solved : pp.PartialProblemStatus;

                        pp.ComputationsTime = s.ComputationsTime;
                        pp.TimeoutOccured = s.TimeoutOccured;

                        if (s.Type == SolutionTypes.Partial && !s.TimeoutOccured)
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
        }*/

        /* FOR: Albert :: trzeba to XMLPartialProblem jakos ladnie nazwac bo koliduje z moim. Albo zmienic nazwe mojego :P
        public void SetPartialProblems(string commonData, List<XMLPartialProblem> partialProblems)
        {
            CommonData = commonData;
            PartialProblems = new List<PartialProblem>();

            foreach (var x in partialProblems)
                PartialProblems.Add(new PartialProblem(x.TaskId, x.Data));

            Status = ProblemStatus.Divided;
        }*/

        /* FOR: Albert :: klasy do wygenerowania i ewentualnie zmien prosze nazwy bo jest kolizja a nie wiem jak zrobic zeby bylo ladnie :)
        public List<XMLPartialProblem> GetPartialProblemListToSolve(long maxCount) // maxCount - liczba wątków CN
        {
            List<XMLPartialProblem> problems = new List<XMLPartialProblem>();

            for (int i = 0; i < maxCount; ++i)
            {
                PartialProblem pp = PartialProblems.Find(x => x.PartialProblemStatus == PartialProblemStatuses.New);

                if (pp == null)
                    break;

                problems.Add(new XMLPartialProblem(pp.TaskId, pp.Data));

                pp.PartialProblemStatus = PartialProblemStatuses.Sended;
            }

            if (PartialProblems.Where(x => x.PartialProblemStatus == PartialProblemStatuses.New).Count() == 0)
                Status = ProblemStatus.WaitingForPartialSolutions;

            return problems;
        }*/
    }
}
