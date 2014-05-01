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
            //TestFileParser parser = new TestFileParser("example2_vt.txt");

            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("dvrp_problems\\okul12D.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();

            DVRP dvrpDivide = new DVRP(data);
            dvrpDivide.DivideProblem(10000);

            List<byte[]> ans = new List<byte[]>();
            foreach (var x in dvrpDivide.PartialProblems)
            {
                DVRP dvrpSolve = new DVRP(data);
                dvrpSolve.Solve(x, new TimeSpan(2, 0, 0));
                ans.Add(dvrpSolve.Solution);
            }

            //DVRP dvrpSolve = new DVRP(data);
            //dvrpSolve.Solve(dvrpDivide.PartialProblems[0], new TimeSpan(2, 0, 0));
            //ans.Add(dvrpSolve.Solution);

            //dvrpSolve = new DVRP(data);
            //dvrpSolve.Solve(dvrpDivide.PartialProblems[1], new TimeSpan(2, 0, 0));
            //ans.Add(dvrpSolve.Solution);

            DVRP dvrpMerge = new DVRP(data);
            dvrpMerge.MergeSolution(ans.ToArray());

            SolutionContainer a = (SolutionContainer)new BinaryFormatter().Deserialize(new MemoryStream(dvrpMerge.Solution));
            Console.WriteLine("MinCost = " + a.MinCost);
            foreach (var q in a.MinPath)
            {
                foreach (var w in q)
                    Console.Write(w + ", ");
                Console.WriteLine();
            }

            TimeSpan ts = DateTime.Now - t;
            Console.WriteLine(ts.ToString());

            //DVRP inst = DVRP.GetInstance(false);
            //MemoryStream ms = new MemoryStream();
            //System.IO.File.OpenRead("okul12D.vrp").CopyTo(ms);
            //byte[] data = ms.ToArray();
            //var dp = inst.DivideProblem(data, 10000);
            //List<byte[]> ans = new List<byte[]>();
            //foreach (var x in dp)
            //    ans.Add(inst.Solve(data, x, new TimeSpan(10, 0, 0)));

            //byte[] ans_byte = inst.MergeSolution(ans.ToArray());
            //float a = (float)new BinaryFormatter().Deserialize(new MemoryStream(ans_byte));
            //Console.WriteLine("MinCost = " + a);
        }
    }
}
