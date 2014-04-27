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
        private double capacities;  // x

        // Liczba pojazdów
        private int vehiclesCount;   //x

        // Liczba zajezdni
        private int depotsCount;   //x

        // Liczba miejsc do odwiedzenia (pickup and delivery points)
        private int visitsCount;     //x

        // Liczba lokacji (visitsCount + depotsCount)
        private int locationsCount;    //x

        // Położenie lokacji (x, y). Używane gdy nie ma podanych wag krawędzi. (przeliczymy na wagi)
        private System.Windows.Point[] locationsCoords;  //x

        // "Numery wierzchołków" zajezdni
        private int[] depots;    //x

        // Numery wierzchołków miejsc do odwiedzenia.
        private int[] visits;      //x  

        // Waga ładunku do odebrania/zostawienia w każdej lokacji. (ujemna = dostarczenie)
        private double[] visitsWeight;   //x

        // Wagi krawędzi
        private double[,] weights;   //x

        // Czas otwarcia i zamknięcia zajezdni.
        private Tuple<double, double>[] depotsTimeWindow;  //x

        // Czas kiedy wizyta będzie znana.
        private double[] visitAvailableTime;   //x

        // Prędkość służąca do zmiany wag krawędzi w czas. Domyślnie 1.
        private double speed;    //x

        // Czas przebywania/rozładunku
        private double[] visitsDuration;   //x

        // najlepszy cykl
        private int[][] cycles;

        // koszt najmniejszej sciezki
        private double min_cost;

        //--------------------------------------

        private DVRP()
        {
        }

        public override TaskSolver GetInstance()
        {
            return DVRP.GetInstance(false);
        }

        public static DVRP GetInstance(bool b)
        {
            return new DVRP();

            //// Przykładowy problem.
            //const int visits = 40;
            //const int vehicles = 40;
            //const int depots = 1;

            //return new DVRP(
            //    vehicles,
            //    new int[depots] { 0 },
            //    new int[visits] /*{ 1, 2, 3, 4, 5 }*/,
            //    new double[visits] /*{ 10, 10, 10, 10, 10 }*/,
            //    new double[visits],
            //    100,
            //    new double[visits + depots, visits + depots]
            //    /*{
            //        {5,2,4,7,3,5},
            //        {2,3,4,1,7,8},
            //        {5,2,9,1,3,3},
            //        {4,5,4,6,3,3},
            //        {3,2,4,5,3,2},
            //        {4,3,5,4,2,2}
            //    }*/,
            //    new Tuple<double, double>[depots]
            //    /*{
            //        new Tuple<double, double>(0, double.PositiveInfinity)
            //    }*/,
            //    new double[visits] /*{ 0, 5, 3, 0, 0 }*/);
        }

        //--------------------------------------Solve

        public override byte[] Solve(byte[] commonData, byte[] partialData, TimeSpan timeout)
        {
            SetProblemData(commonData);
            Initialize();

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
             * NP: dla 10 pojazdów, 10 lokacji i tablic 3 elementowych:
             * { [9, 1, 0], [5, 1, 1], [2, 2, 2] }
             * Oznacza 3 sytuacje do sprawdzenia:
             *   1) pojazd 1 odwiedza 9 lokacji, pojazd 2 jedną, pojazd 3 zero. pozostałe 7 pojazdów nie odwiedza żadnej
             *   2) pojazd 1 5 lokacji, pojazdy 2 i 3 po jednej, pozostałe 7 pojazdów 3 lokacje: wynikowa tablica do sprawdzenia: [5, 1, 1, 1, 1, 1, 0, 0, 0, 0]
             *   3) pojazdy 1,2,3 odwiedzają po 2 lokacje, pozostałe 4 lokacje muszą być sprawdzone przez pozostałe pojazdy, 
             *      wszystkie możliwości do sprawdzenia w Solve:
             *      [2, 2, 2, 2, 2, 0, 0, 0, 0, 0]
             *      [2, 2, 2, 2, 1, 1, 0, 0, 0, 0]
             *      [2, 2, 2, 1, 1, 1, 1, 0, 0, 0]
             *      
             * Wygenerowane tablice (i dalsze części do wygenerowania) to rozkład liczby lokacji do odwiedzenia na składniki.
             * Tablice mogą być wygenerowane do końca w Divide lub należy je generować jeszcze w Solve: trzeba sprawdzić przed rozpoczęciem!
             */

            min_cost = double.MaxValue;
            foreach (int[] div in list)
            {
                Brute(0, vehiclesCount, new int[vehiclesCount], div);
            }

            Console.WriteLine(min_cost);

            MemoryStream ms = new MemoryStream();
            new BinaryFormatter().Serialize(ms, min_cost);

            return ms.ToArray();
        }

        void SplitVisits(int[] combinations, int[] div)
        {
            // splits - tablica podzialow dla kazdego z pociagow
            // splits[0] - ilosc punktow odwiedzanych przez pociag
            int[][] splits = new int[vehiclesCount][];
            for (int i = 0; i < vehiclesCount; ++i)
                splits[i] = new int[div[i]];
            // wywolanie dla kazdego 
            ForEachTrain(0, vehiclesCount, new bool[visitsCount], splits, div, combinations);
        }

        void ForEachTrain(int veh, int num_veh, bool[] free_visits, int[][] splits, int[] div, int[] combinations)
        {
            if (veh == 0)
                Console.WriteLine("==================================");
            if (veh == 1)
                Console.WriteLine(string.Join("; ", splits.Select(x => string.Join(" ", x.Select(y => y.ToString())))));

            // jak wszystkie sie podzielily, to licz tsp
            if (veh >= num_veh)
            {
                // wynik z algo
                Tuple<double, int[][]> ret = TSPWrapper(combinations, splits);
                if (ret.Item1 < min_cost)
                {
                    min_cost = ret.Item1;
                    cycles = (int[][])ret.Item2.Clone();
                }
                return;
            }
            // jak nie, to dziel
            OneLocation(0, div[veh], visitsCount, 0, splits, veh, free_visits, div, combinations);
        }

        // visit - numer wizyty dla danego pojazdu, veh_visits - powinienien odwiedziec lokacji, num_visits - musi byc 
        // odwiedzono lokacji sumarycznie,
        // start - poczatek sprawdzania, veh - numer pojazdu, free_visits - tablica dostepnosci punktow
        void OneLocation(int visit, int veh_visits, int num_visits, int start, int[][] splits, int veh, bool[] free_visits, int[] div, int[] combinations)
        {
            // jezeli przydzielil swoje, to dziel dla nastepnego
            if (visit >= veh_visits)
            {
                ForEachTrain(veh + 1, vehiclesCount, free_visits, splits, div, combinations);
                return;
            }

            for (int i = start; i < num_visits; ++i)
            {
                // jezeli juz zajety, to pierdol sie
                if (free_visits[i])
                    continue;

                splits[veh][visit] = i;
                free_visits[i] = true;

                // kolejny punkt
                OneLocation(visit + 1, veh_visits, num_visits, i + 1, splits, veh, free_visits, div, combinations);

                free_visits[i] = false;
            }
        }

        void Brute(int num, int num_veh, int[] combinations, int[] div)
        {
            if (num >= num_veh)
            {
                // etap 2 (wybor punktow)
                SplitVisits(combinations, div);
                return;
            }

            foreach (int d in depots)
            {
                combinations[num] = d;
                Brute(num + 1, num_veh, combinations, div);
            }
        }

        // ------------------------------------- wrapper dla TSP (nie trzeba bedzie wywolywac dla kazdego osobno)
        // combinations - mhmhm, poczatki sciezek :)
        Tuple<double, int[][]> TSPWrapper(int[] combinations, int[][] splits)
        {
            double total_min_cost = 0;
            double min_cost;
            int[][] cycle = new int[combinations.Length][];

            for (int i = 0; i < combinations.Length; ++i)
            {
                min_cost = Double.MaxValue;
                bool[] to_visit = new bool[visitsCount]; //new bool[splits[i].Length];
                cycle[i] = new int[splits[i].Length];
                FTSPFS(depots[combinations[i]], splits[i], to_visit, 0, 0, ref min_cost, cycle[i], 
                    depotsTimeWindow[combinations[i]].Item1, capacities);
                total_min_cost += min_cost;
                if (total_min_cost >= Double.MaxValue)
                    break;
            }

            return new Tuple<double, int[][]>(total_min_cost, cycle);
        }

        //--------------------------------------FTSTPFS capitan (-:
        // cos musi z czasem jescze byc dodane pewnie
        void FTSPFS(int v, int[] to_visit, bool[] vis, int visited, double len, ref double min_len, int[] cycle, double time, double cap)
        {
            if (len > min_len)
                return;
            if (visited == to_visit.Length)
            {
                // dodaj nowy najlepszy cykl
                for(int i = 0; i < depotsCount; ++i)
                    if (time + weights[v, depots[i]] < depotsTimeWindow[i].Item2 && min_len > len + weights[v, depots[i]])
                        min_len = len + weights[v, depots[i]];
                return;
            }

            // dla kazdego sasiada
            // mozna dodac sprawdzanie zajezdni i odrzucac, jak dla wszystkich juz sie nie da dojechac 
            // (poki co jest pomysl tylko na rozwiazanie tego problemu)
            foreach (int w in to_visit)
            {
                if (!vis[w] /*&& visitAvailableTime[w] < time*/)
                {
                    double www = 0;
                    int last_v = v;
                    // TODO: pierwszy depot, musi byc zmienione
                    if (cap + visitsWeight[w] < 0) {
                        cap = capacities;
                        www = weights[v, depots[0]];
                        v = depots[0];
                    }

                    // czekaj na dostepnosc
                    if (visitAvailableTime[w] > time)
                        time = visitAvailableTime[w];
                    vis[w] = true;
                    cycle[visited] = w;
                    FTSPFS(visits[w], to_visit, vis, visited + 1, len + weights[v, visits[w]] + www, ref min_len, cycle,
                        time + weights[v, visits[w]] + visitsDuration[w] + www, cap + visitsWeight[w]);

                    if (www != 0)
                        v = last_v;

                    vis[w] = false;
                }
            }
        }

        // ---------------------------- odpowiedzialna za przydzial lokacji, przerobic ze wzgedu na chujowow implementacje
        // sposob dzialania: [3, 1, 2]
        // splited[0] = wszystkie podzialy dla 3, splited[1] = wszystkie podzaily dla 1, splited[2] = wszystkie podzialy dla 2
        // wiec trzeb abedzie potem sprawdzac, czy sa wzajemnie pelne
        void SplitLocations(int[] div, out List<System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<int>>> splited)
        {
            splited = new List<System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<int>>>();
            foreach (int d in div)
            {
                var res = Combination.Combinations(visits, d);
                splited.Add(res);
            }
        }

        //--------------------------------------Divide

        private void SetProblemData(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            TestFileParser problemParser = new TestFileParser(stream);

            capacities = problemParser.Capacities;
            vehiclesCount = problemParser.NumVehicles;
            depotsCount = problemParser.NumDepots;
            visitsCount = problemParser.NumVisits;
            locationsCount = depotsCount + visitsCount;//problemParser.NumLocations;

            if (problemParser.EdgeWeights != null)
                weights = problemParser.EdgeWeights;

            speed = problemParser.Speed;
            depots = new int[depotsCount];
            visits = new int[visitsCount];
            locationsCoords = new System.Windows.Point[locationsCount];

            for (int i = 0; i < depotsCount; ++i)
            {
                if (problemParser.LocationIds == null)
                    depots[i] = i;
                else
                    depots[i] = Array.IndexOf(problemParser.LocationIds, problemParser.DepotsLocation[problemParser.Depots[i]]);
            }

            for (int i = 0; i < visitsCount; ++i)
            {
                if (problemParser.LocationIds == null)
                    visits[i] = i + depotsCount;
                else
                    visits[i] = Array.IndexOf(problemParser.LocationIds, problemParser.VisitLocations[problemParser.Visits[i]]);
            }

            if (problemParser.LocationsCoords != null)
            {
                for (int i = 0; i < locationsCount; ++i)
                {
                    locationsCoords[i] = problemParser.LocationsCoords[problemParser.LocationIds[i]];
                }
            }

            visitsWeight = new double[visitsCount];
            visitAvailableTime = new double[visitsCount];
            visitsDuration = new double[visitsCount];

            for (int i = 0; i < visitsCount; ++i)
            {
                visitsWeight[i] = problemParser.VisitQuantity[problemParser.Visits[i]];
                visitsDuration[i] = problemParser.VisitsDuration[problemParser.Visits[i]];
                if (problemParser.TimeAvail == null)
                    visitAvailableTime[i] = 0;
                else
                    visitAvailableTime[i] = problemParser.TimeAvail[problemParser.Visits[i]];
            }

            depotsTimeWindow = new Tuple<double, double>[depotsCount];

            for (int i = 0; i < depotsCount; ++i)
            {
                depotsTimeWindow[i] = problemParser.DepotsTimeWindow[problemParser.Depots[i]];
            }
        }

        public override byte[][] DivideProblem(byte[] data, int threadCount)
        {
            SetProblemData(data);
            Initialize();

            List<int[]> list = new List<int[]>();

            //int vehiclesLimit = 10;// - (visitsCount - 50);

            if (visitsCount <= 40)
                GenerateSets(visitsCount, vehiclesCount, list, visitsCount, 0, new int[vehiclesCount], 0);
            else
            {
                int w = visitsCount <= 60 ? 4 : (visitsCount <= 100 ? 3 : 2);
                int vehiclesLimit = vehiclesCount <= w ? vehiclesCount : w;
                GenerateSets(visitsCount, vehiclesLimit, list, visitsCount, 0, new int[vehiclesLimit], vehiclesCount - vehiclesLimit);
            }

            //RemoveInvalid(list, (vehiclesCount <= vehiclesLimit) ? vehiclesCount : vehiclesLimit);

            //Console.WriteLine(list.Count);
            ////foreach (int[] a in list)
            ////{
            ////    foreach (int i in a)
            ////        Console.Write(i + " ");
            ////    Console.WriteLine();
            ////}

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
                ++n;
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
            // init cut-off stuff
            
            double max_time = Double.MinValue;
            foreach(Tuple<double, double> time in depotsTimeWindow)
                if (max_time < time.Item2)
                    max_time = time.Item2;

            double cut_off = max_time * 0.5;

            for (int i = 0; i < visitAvailableTime.Length; ++i)
                if (visitAvailableTime[i] > cut_off)
                    visitAvailableTime[i] = 0;
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

            int put = (i == 0 || tmp[i - 1] >= left) ? left : tmp[i - 1];
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

        //private void RemoveInvalid(List<int[]> list, int v)
        //{
        //    for (int i = 0; i < v; ++i)
        //    {
        //        for (int j = i + 1; j < v; ++j)
        //        {
        //            RemovePair(i, j, list, v);
        //        }
        //    }
        //}

        //private void RemovePair(int i, int j, List<int[]> list, int v)
        //{
        //    bool[] deleted = new bool[list.Count];

        //    int count = list.Count;
        //    for (int k = 0; k < count; ++k)
        //    {
        //        int ki = list[k][i];
        //        int kj = list[k][j];

        //        if (!deleted[k] && ki != kj)
        //        {
        //            for (int z = k + 1; z < count; ++z)
        //            {
        //                if (!deleted[z] && ki == list[z][j] && kj == list[z][i])
        //                {
        //                    bool dif = false;

        //                    for (int y = 0; y < v; ++y)
        //                    {
        //                        if (y != i && y != j && list[k][y] != list[z][y])
        //                        {
        //                            dif = true;
        //                            break;
        //                        }
        //                    }

        //                    if (!dif)
        //                    {
        //                        deleted[z] = true;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    List<int[]> newlist = new List<int[]>();
        //    for (int k = 0; k < list.Count; ++k)
        //        if (!deleted[k])
        //            newlist.Add(list[k]);

        //    list.Clear();
        //    list.AddRange(newlist);
        //}

        //--------------------------------------Merge

        public override byte[] MergeSolution(byte[][] solutions)
        {
            BinaryFormatter bf = new BinaryFormatter();
            List<double> min_costs = new List<double>();

            foreach (byte[] x in solutions)
                min_costs.Add((double)bf.Deserialize(new MemoryStream(x)));

            double mc = min_costs.Min();

            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, mc);

            return ms.ToArray();
        }
    }
}
