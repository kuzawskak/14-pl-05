using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DVRP
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime t = DateTime.Now;

            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("dvrp_problems\\okul12D.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();

            DVRP dvrpDivide = new DVRP(data);
            dvrpDivide.DivideProblem(1000);

            Console.WriteLine("Sets: " + dvrpDivide.PartialProblems.Length);
            //Console.ReadKey();

            List<byte[]> ans = new List<byte[]>();
            foreach (var x in dvrpDivide.PartialProblems)
            {
                DVRP dvrpSolve = new DVRP(data);
                dvrpSolve.Solve(x, new TimeSpan(0, 0, 1));
                ans.Add(dvrpSolve.Solution);
            }


            DVRP dvrpMerge = new DVRP(data);
            dvrpMerge.MergeSolution(ans.ToArray());

            SolutionContainer a = (SolutionContainer)new BinaryFormatter().Deserialize(new MemoryStream(dvrpMerge.Solution));
            Console.WriteLine();
            Console.WriteLine("MinCost = " + a.MinCost);
            Console.WriteLine();
            foreach (var q in a.MinPath)
            {
                foreach (var w in q)
                    Console.Write(w + ", ");
                Console.WriteLine();
            }
            Console.WriteLine();
            foreach (var q in a.Times)
            {
                foreach (var w in q)
                    Console.Write(w + ", ");
                Console.WriteLine();
            }

            Console.WriteLine();
            TimeSpan ts = DateTime.Now - t;
            Console.WriteLine("Time: " + ts.ToString());
        }
    }
}
