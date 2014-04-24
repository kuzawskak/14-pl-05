using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UCCTaskSolver;

namespace DVRP
{
    class DVRP : TaskSolver
    {
        // Ograniczenia dla pojazdów. Definiuje ograniczenia dla wszystkich pojazdów. 
        private double capacities;  

        // Liczba pojazdów
        private int vehiclesCount;   

        // Liczba zajezdni
        private int depotsCount;   

        // Liczba miejsc do odwiedzenia (pickup and delivery points)
        private int visitsCount;     

        // Liczba lokacji (visitsCount + depotsCount)
        private int locationsCount;    

        // Położenie lokacji (x, y). Używane gdy nie ma podanych wag krawędzi. (przeliczymy na wagi)
        private System.Windows.Point[] locationsCoords;  

        // "Numery wierzchołków" zajezdni
        private int[] depots;    

        // Numery wierzchołków miejsc do odwiedzenia.
        private int[] visits;        

        // Waga ładunku do odebrania/zostawienia w każdej lokacji. (ujemna = dostarczenie)
        private double[] visitsWeight;   

        // Wagi krawędzi
        private double[,] weights;   

        // Czas otwarcia i zamknięcia zajezdni.
        private Tuple<double, double>[] depotsTimeWindow;  

        // Czas kiedy wizyta będzie znana.
        private double[] visitAvailableTime;   

        // Prędkość służąca do zmiany wag krawędzi w czas. Domyślnie 1.
        private double speed;    

        // Czas przebywania/rozładunku
        private double[] visitsDuration;   

        //--------------------------------------

        private DVRP()
        {
        }

        private DVRP(int _vehiclesCount, int[] _depots, int[] _visits, double[] _visitsWeight, double[] _visitsDuration, double _capacities, double[,] _weights, Tuple<double, double>[] _depotsTimeWindow, double[] _visitAvailableTime)
        {
            capacities = _capacities;
            vehiclesCount = _vehiclesCount;
            depotsCount = _depots.Length;
            visitsCount = _visits.Length;
            locationsCount = visitsCount + depotsCount;
            locationsCoords = null; // to bedzie parsowane!
            depots = _depots;
            visits = _visits;
            visitsWeight = _visitsWeight;
            weights = _weights; // to bedzie obliczane
            depotsTimeWindow = _depotsTimeWindow;
            visitAvailableTime = _visitAvailableTime;
            speed = 1;
            visitsDuration = _visitsDuration;
        }

        public override TaskSolver GetInstance()
        {
            return DVRP.GetInstance(false);
        }

        public static DVRP GetInstance(bool b)
        {
            //return new DVRP();

            // Przykładowy problem.
            const int visits = 50;
            const int vehicles = 50;
            const int depots = 1;

            return new DVRP(
                vehicles,
                new int[depots] { 0 },
                new int[visits] /*{ 1, 2, 3, 4, 5 }*/,
                new double[visits] /*{ 10, 10, 10, 10, 10 }*/,
                new double[visits],
                100,
                new double[visits + depots, visits + depots]
                /*{
                    {5,2,4,7,3,5},
                    {2,3,4,1,7,8},
                    {5,2,9,1,3,3},
                    {4,5,4,6,3,3},
                    {3,2,4,5,3,2},
                    {4,3,5,4,2,2}
                }*/,
                new Tuple<double, double>[depots]
                /*{
                    new Tuple<double, double>(0, double.PositiveInfinity)
                }*/,
                new double[visits] /*{ 0, 5, 3, 0, 0 }*/);
        }

        //--------------------------------------Solve

        public override byte[] Solve(byte[] commonData, byte[] partialData, TimeSpan timeout)
        {
            DateTime startTime = DateTime.Now;

            MemoryStream m = new MemoryStream(partialData);
            List<int[]> list = (List<int[]>)new BinaryFormatter().Deserialize(m);

            // TODO -_-

            /* Tablice na liście zawierają ilość wizyt dla konkretnych pojazdów.
             * np zawartość { [3, 0, 0], [2, 1, 0] } oznacza, że testowane są dwie sytuacje:
             *  - pierwsza: pojazd 1 musi odwiedzić 3 lokacje, pojazd 2 i 3 zero lokacji,
             *  - druga: pojazd 1 odwiedza 2 lokacje, pojazd 2 jedną, pojazd 3 zero.
             * lokacje oznaczają wizyty - zajezdnie nie są wliczone.
             * 
             * tablice nie zawierają opisu dla wszystkich pojazdów!!!
             * Jeżeli jest np 10 pojazdów, a tablice są pięcioelementowe, oznacza to, że opisane jest tylko 5 pierwszych pojazdów -
             * pozostałe należy sprawdzać w Solve.
             * NP: dla 10 pojazdów i tablic 3 elementowych:
             * { [9, 1, 0], [5, 1, 1], [0, 0, 0] }
             * Oznacza 3 sytuacje do sprawdzenia:
             *   1) pojazd 1 odwiedza 9 lokacji, pojazd 2 jedną, pojazd 3 zero. pozostałe 7 pojazdów nie odwiedza żadnej
             *   2) pojazd 1 5 lokacji, pojazdy 2 i 3 po jednej, pozostałe 7 pojazdów 3 lokacje: należy sprawdzić wszystkie możliwości
             *   UWAGA: Usunięcie duplikatów zmniejsza ilość rozwiązań do sprawdzenia (pojazdy nie są rozróżnialne: ciągi [1, 0] i [0, 1] są takie same!!!)
             *   3) pojazd */

            return null;
        }

        //--------------------------------------Divide

        public override byte[][] DivideProblem(byte[] data, int threadCount)
        {
            Initialize();

            List<int[]> list = new List<int[]>();

            //const int vehiclesLimit = 10;

            if (visitsCount <= 50)
                GenerateSets(visitsCount, vehiclesCount, list, visitsCount, 0, new int[vehiclesCount], 0);
            //else
                //GenerateSets(visitsCount, vehiclesLimit, list, visitsCount, 0, new int[vehiclesLimit], true);

            //RemoveInvalid(list, (vehiclesCount <= vehiclesLimit) ? vehiclesCount : vehiclesLimit);

            Console.WriteLine(list.Count);
            //foreach (int[] a in list)
            //{
            //    foreach (int i in a)
            //        Console.Write(i + " ");
            //    Console.WriteLine();
            //}

            int tc = threadCount < list.Count ? threadCount : list.Count;

            List<int[]>[] r = new List<int[]>[tc];
            for (int i = 0; i < tc; ++i)
            {
                r[i] = new List<int[]>();
            }
            Random rand = new Random();
            int n = 0;
            while (list.Count > 0)
            {
                int x = rand.Next(list.Count);
                r[n].Add(list[x]);
                list.RemoveAt(x);
                n++;
                if (n == tc)
                    n = 0;
            }

            byte[][] ret = new byte[tc][];
            for (int i = 0; i < tc; ++i)
            {
                MemoryStream s = new MemoryStream();
                new BinaryFormatter().Serialize(s, r[i]);
                ret[i] = s.ToArray();
            }

            return ret;
        }

        private void Initialize()
        {
            if (weights == null)
            {
                weights = new double[locationsCount, locationsCount];
                bool isManhattan = false;

                for (int i = 0; i < locationsCount; ++i)
                {
                    for (int j = 0; j < locationsCount; ++j)
                    {
                        weights[i, j] = isManhattan ?
                            (Math.Abs(locationsCoords[i].X - locationsCoords[j].X) + Math.Abs(locationsCoords[i].Y - locationsCoords[j].Y)) :
                            (Math.Sqrt(Math.Pow(locationsCoords[i].X - locationsCoords[j].X, 2) + Math.Pow(locationsCoords[i].Y - locationsCoords[j].Y, 2)));
                    }
                }
            }
        }

        private void GenerateSets(int n, int k, List<int[]> list, int left, int i, int[] tmp, int more)
        {
            //if (i != 0 && tmp[i - 1] < left)
            //    return;

            if (i == k)
            {
                if (/*isMore ||*/ left == 0 || tmp[i - 1] * more >= left)
                    list.Add((int[])tmp.Clone());
                return;
            }

            int put = (i == 0 || tmp[i-1] >= left) ? left : tmp[i - 1];
            left = left - put;


            while (put >= 0)
            {
                if (put == 0 && left != 0)
                    return;

                tmp[i] = put;
                //if(i == 0 || tmp[i-1] >= put)
                    GenerateSets(n, k, list, left, i + 1, tmp, more);
                put--;
                left++;
            }
        }

        private void RemoveInvalid(List<int[]> list, int v)
        {
            for (int i = 0; i < v; ++i)
            {
                for (int j = i + 1; j < v; ++j)
                {
                    RemovePair(i, j, list, v);
                }
            }
        }

        private void RemovePair(int i, int j, List<int[]> list, int v)
        {
            bool[] deleted = new bool[list.Count];

            int count = list.Count;
            for (int k = 0; k < count; ++k)
            {
                int ki = list[k][i];
                int kj = list[k][j];

                if (!deleted[k] && ki != kj)
                {
                    for (int z = k + 1; z < count; ++z)
                    {
                        if (!deleted[z] && ki == list[z][j] && kj == list[z][i])
                        {
                            bool dif = false;

                            for (int y = 0; y < v; ++y)
                            {
                                if (y != i && y != j && list[k][y] != list[z][y])
                                {
                                    dif = true;
                                    break;
                                }
                            }

                            if (!dif)
                            {
                                deleted[z] = true;
                                break;
                            }
                        }
                    }
                }
            }

            List<int[]> newlist = new List<int[]>();
            for (int k = 0; k < list.Count; ++k)
                if (!deleted[k])
                    newlist.Add(list[k]);

            list.Clear();
            list.AddRange(newlist);
        }

        //--------------------------------------Merge

        public override byte[] MergeSolution(byte[][] solutions)
        {

            return null;
        }
    }
}
