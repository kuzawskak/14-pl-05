using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVRP
{
    class Program
    {
        static void Main(string[] args)
        {
            DVRP inst = DVRP.GetInstance(false);
            inst.Solve(null, inst.DivideProblem(null, 10)[0], new TimeSpan(0, 5, 0));
        }
    }
}
