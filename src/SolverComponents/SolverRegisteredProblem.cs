using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolverComponents;
using CommunicationXML;


namespace SolverComponents
{



    class SolverRegisteredProblem
    {

        private ulong problem_id;
        private List<PartialProblem> ongoing_problems = new List<PartialProblem>();


        public ulong ProblemId
        {
            get { return problem_id; }
            set { problem_id = value; }
        }

        public SolverRegisteredProblem(ulong problem_id, List<PartialProblem> ongoing_problems)
        {
            this.problem_id = problem_id;
            this.ongoing_problems = ongoing_problems;
        }

        public void MarkAsSolved(ulong task_id)
        {
            PartialProblem pp = ongoing_problems.Find(x=>x.TaskId ==task_id);
            if (pp != null)
                ongoing_problems.Remove(pp);
        }

        public bool IsProblemSolved()
        {
            return ongoing_problems.Count() == 0;
        }

        
    }
}
