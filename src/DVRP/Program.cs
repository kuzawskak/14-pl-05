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
            //TestFileParser parser = new TestFileParser("example2_vt.txt");
            DVRP inst = DVRP.GetInstance(false);
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("dvrp_problems\\okul12D.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();
            var dp = inst.DivideProblem(data, 100000);
            List<byte[]> ans = new List<byte[]>();
            foreach (var x in dp)
                ans.Add(inst.Solve(data, x, new TimeSpan(10, 0, 0)));

            byte[] ans_byte = inst.MergeSolution(ans.ToArray());
            double a = (double)new BinaryFormatter().Deserialize(new MemoryStream(ans_byte));
            Console.WriteLine("MinCost = " + a);
        }
    }
}
