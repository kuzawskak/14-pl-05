using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UCCTaskSolver;

namespace DVRP
{
    /// <summary>
    /// Wyjątek rzucany w przypadku timeout.
    /// </summary>
    class TimeoutException : Exception
    {
    }

    /// <summary>
    /// Klasa zawierająca rozwiązanie.
    /// </summary>
    [Serializable]
    class SolutionContainer : IComparable<SolutionContainer>
    {
        /// <summary>
        /// Minimalny koszt.
        /// </summary>
        public float MinCost { get; set; }

        /// <summary>
        /// Zawiera ciąg lokacji odwiedzonych przez pojazdy (wraz z zajezdniami).
        /// </summary>
        public List<int>[] MinPath { get; set; }

        /// <summary>
        /// Czasy dojazdu do lokacji.
        /// </summary>
        public List<float>[] Times { get; set; }

        /// <summary>
        /// Porównanie. Ułatwia znalezienie minimum.
        /// </summary>
        /// <param name="c">SolutionContainer.</param>
        /// <returns>Porównanie.</returns>
        public int CompareTo(SolutionContainer c)
        {
            return c.MinCost == MinCost ? 0 : (MinCost < c.MinCost ? -1 : 1);
        }
    }

    /// <summary>
    /// DVRP.
    /// </summary>
    class DVRP : TaskSolver
    {
        /// <summary>
        /// Ograniczenia dla pojazdów. Definiuje ograniczenia dla wszystkich pojazdów.
        /// </summary>
        private float capacities;  

        /// <summary>
        /// Liczba pojazdów.
        /// </summary>
        private int vehiclesCount;  

        /// <summary>
        /// Liczba zajezdni.
        /// </summary>
        private int depotsCount;   

        /// <summary>
        /// Liczba miejsc do odwiedzenia (pickup and delivery points).
        /// </summary>
        private int visitsCount;    

        /// <summary>
        /// Liczba lokacji (visitsCount + depotsCount).
        /// </summary>
        private int locationsCount;   

        /// <summary>
        /// Położenie lokacji (x, y). Używane gdy nie ma podanych wag krawędzi. (przeliczymy na wagi)
        /// </summary>
        private System.Windows.Point[] locationsCoords; 

        /// <summary>
        /// "Numery wierzchołków" zajezdni.
        /// </summary>
        private int[] depots;   

        /// <summary>
        /// Numery wierzchołków miejsc do odwiedzenia.
        /// </summary>
        private int[] visits;       

        /// <summary>
        /// Waga ładunku do odebrania/zostawienia w każdej lokacji. (ujemna = dostarczenie)
        /// </summary>
        private float[] visitsWeight;  

        /// <summary>
        /// Wagi krawędzi
        /// </summary>
        private float[,] weights;  

        /// <summary>
        /// Czas otwarcia i zamknięcia zajezdni.
        /// </summary>
        private Tuple<float, float>[] depotsTimeWindow;  

        /// <summary>
        /// Czas kiedy wizyta będzie znana.
        /// </summary>
        private float[] visitAvailableTime;  

        /// <summary>
        /// Prędkość służąca do zmiany wag krawędzi w czas. Domyślnie 1.
        /// </summary>
        private float speed;   

        /// <summary>
        /// Czas przebywania/rozładunku.
        /// </summary>
        private float[] visitsDuration;   

        /// <summary>
        /// Najlepszy cykl.
        /// </summary>
        private List<int>[] cycles;

        /// <summary>
        /// Czasy dojazdów do lokacji przez pojazdy.
        /// </summary>
        private List<float>[] times;

        /// <summary>
        /// Koszt najmniejszej ścieżki.
        /// </summary>
        private float min_cost;

        /// <summary>
        /// Timeout.
        /// </summary>
        private TimeSpan timeout;

        /// <summary>
        /// Czas rozpoczęcia obliczeń.
        /// </summary>
        private DateTime startTime;

        /// <summary>
        /// Czy trwają obliczenia?
        /// </summary>
        private bool isSolving;

        /// <summary>
        /// Czy był timeout?
        /// </summary>
        private bool isTimeout;

        /// <summary>
        /// Task odpowiedzialny za wykrycie timeout.
        /// </summary>
        private Task backgroundTask;

        /// <summary>
        /// Konstrukcja nowego TaskSolvera.
        /// </summary>
        /// <param name="data">Dane problemu.</param>
        public DVRP(byte[] data)
            : base(data)
        {
            State = TaskSolverState.Idle;
            if (data != null)
            {
                SetProblemData(data);
                Initialize();
            }
        }

        /// <summary>
        /// Metoda obsługująca timeout.
        /// </summary>
        void TimeoutCheck()
        {
            while (isSolving)
            {
                if (startTime + timeout < DateTime.Now)
                {
                    isTimeout = true;
                    break;
                }
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// Szuka najlepszego rozwiązania dla danych na liście.
        /// Lista zawiera tablice, które opisują liczbę lokacji do obsłużenia przez konkretne pojazdy.
        /// Przykładowa zawartość listy: { [5, 1, 0], [4, 2, 0], [4, 1, 1] } oznacza 3 sytuacje do obsłużenia:
        ///   1. Pojazd 1 obsługuje 5 lokacji (dowolnych), pojazd 2 jedną lokację, pojazd 3 zero lokacji.
        ///   2. Pojazd 1 obsługuje 4 lokacje, pojazd 2 dwie, pojazd 3 zero.
        ///   3. Pojazd 2 musi obsłużyć 4, pojazdy 2 i 3 po jednej.
        /// Zbiory są wygenerowane tak, aby wszystkie lokacje zostały obsłużone.
        /// Lokacje nie mogą się powtarzać. Lokacje oznaczają tutaj wizyty, zajezdnie mogą występować dowolnie.
        /// </summary>
        /// <param name="partialData">Lista tablic opisujących liczbę lokacji do odwiedzenia przez każdy pojazd.</param>
        /// <param name="timeout">Maksymalny czas rozwiązywania zadania.</param>
        /// <returns></returns>
        public override byte[] Solve(byte[] partialData, TimeSpan timeout)
        {
            MemoryStream ms = new MemoryStream();
            this.timeout = timeout;
            startTime = DateTime.Now;
            isSolving = true;
            isTimeout = false;

            // Task odpowiedzialny za przerwanie obliczeń w przypadku timeout.
            backgroundTask = Task.Factory.StartNew(() => TimeoutCheck());

            try
            {
                State = TaskSolverState.Solving;

                // Odbieranie partial data.
                MemoryStream m = new MemoryStream(partialData);
                List<int[]> list = (List<int[]>)new BinaryFormatter().Deserialize(m);

                min_cost = float.MaxValue;

                // Dla każdego zbioru oznaczającego liczbę lokacji do odwiedzenia przez pojazd
                // wywoływane jest sprawdzanie kombinacji zajezdni, z których wyjechać ma pojazd (a następnie dalsze obliczenia).
                foreach (int[] div in list)
                {
                    if (isTimeout)
                        throw new TimeoutException();
                    TestDepotsCombinations(0, vehiclesCount, new int[vehiclesCount], div);
                }

                Console.WriteLine();
                Console.WriteLine(min_cost);

                // Zapisywanie najlepszego rozwiązania.
                SolutionContainer sc = new SolutionContainer()
                {
                    MinCost = min_cost,
                    MinPath = cycles,
                    Times = times
                };

                // Serializacja.
                new BinaryFormatter().Serialize(ms, sc);
                Solution = ms.ToArray();
                State = TaskSolverState.Idle;
                if (ProblemDividingFinished != null)
                    ProblemSolvingFinished(new EventArgs(), this);
            }
            catch (TimeoutException e)
            {
                // Zapisywanie najlepszego rozwiązania i serializacja w przypadku timeout.
                SolutionContainer sc = new SolutionContainer()
                {
                    MinCost = min_cost,
                    MinPath = cycles,
                    Times = times
                };

                State = TaskSolverState.Timeout | TaskSolverState.Idle;
                new BinaryFormatter().Serialize(ms, sc);
                Solution = ms.ToArray();
                if (ProblemDividingFinished != null)
                    ProblemSolvingFinished(new EventArgs(), this);
            }
            catch (Exception e)
            {
                State = TaskSolverState.Error | TaskSolverState.Idle;
                if (ErrorOccured != null)
                    ErrorOccured(this, new UnhandledExceptionEventArgs(e, true));
            }

            isSolving = false;
            backgroundTask.Wait();

            return Solution;
        }

        /// <summary>
        /// Metoda wyznacza wszystkie kombinacje zajezdni z których mają wyjechać pojazdy.
        /// </summary>
        /// <param name="n">Numer pojazdu</param>
        /// <param name="depotsCombinations">taablica zawierająca zajezdnie z których mają wyjechać pojazdy.</param>
        /// <param name="div">Przydział liczby lokacji do odwiedzenia przez każdy z pojazdów.</param>
        void TestDepotsCombinations(int n, int[] depotsCombinations, int[] div)
        {
            // Jeżeli dla każdego pojazdu jest już przypisana zajezdnia z której ma wyjechać to przechodzimy do kolejnego etapu.
            if (n >= vehiclesCount)
            {
                // Przydział lokacji dla pojazdów.
                SplitVisits(depotsCombinations, div);
                return;
            }

            // Testowanie każdej zajezdni dla pojazdu 'num'.
            for (int d = 0; d < depotsCount; ++d)
            {
                if (isTimeout)
                    throw new TimeoutException();

                // Wywoływanie metody dla pojazdu 'num + 1'.
                depotsCombinations[n] = d;
                TestDepotsCombinations(n + 1, depotsCombinations, div);
            }
        }

        /// <summary>
        /// Sprawdza każdą kombinację lokacji dla każdego pojazdu, zgodznie z przydziałem liczby loacji dla pojazdów.
        /// </summary>
        /// <param name="depotsCombinations">Zajezdnie z których musi wyjechać każdy z pojazdów.</param>
        /// <param name="div">Liczba lokacji, którą musi odwiedzić każdy pojazd.</param>
        void SplitVisits(int[] depotsCombinations, int[] div)
        {
            // splits - tablica podzialow dla kazdego z pociagow
            // splits[0] - ilosc punktow odwiedzanych przez pociag
            int[][] splits = new int[vehiclesCount][];
            for (int i = 0; i < vehiclesCount; ++i)
                splits[i] = new int[div[i]];
            // wywolanie dla kazdego 
            VisitsForEachVehicle(0, vehiclesCount, new bool[visitsCount], splits, div, depotsCombinations);
        }

        void VisitsForEachVehicle(int veh, int num_veh, bool[] free_visits, int[][] splits, int[] div, int[] depotsCombinations)
        {
            if (veh == 0)
                Console.WriteLine("==================================");
            if (veh == 1)
                Console.WriteLine(string.Join("; ", splits.Select(x => string.Join(" ", x.Select(y => y.ToString())))));

            // jak wszystkie sie podzielily, to licz tsp
            if (veh >= num_veh)
            {
                // wynik z algo
                Tuple<float, List<int>[], List<float>[]> ret = TSPWrapper(depotsCombinations, splits);
                if (ret.Item1 < min_cost)
                {
                    min_cost = ret.Item1;
                    cycles = ret.Item2;
                    times = ret.Item3;
                }
                return;
            }

            if (isTimeout)
                throw new TimeoutException();

            // jak nie, to dziel
            VisitsCombinationForVehicle(0, div[veh], 0, splits, veh, free_visits, div, depotsCombinations);
        }

        // visit - numer wizyty dla danego pojazdu, veh_visits - powinienien odwiedziec lokacji, num_visits - musi byc 
        // odwiedzono lokacji sumarycznie,
        // start - poczatek sprawdzania, veh - numer pojazdu, free_visits - tablica dostepnosci punktow
        void VisitsCombinationForVehicle(int visit, int veh_visits, int start, int[][] splits, int veh, bool[] free_visits, int[] div, int[] combinations)
        {
            // jezeli przydzielil swoje, to dziel dla nastepnego
            if (visit >= veh_visits)
            {
                VisitsForEachVehicle(veh + 1, vehiclesCount, free_visits, splits, div, combinations);
                return;
            }

            for (int i = start; i < visitsCount; ++i)
            {
                // jezeli juz zajety, to pierdol sie
                if (free_visits[i])
                    continue;

                splits[veh][visit] = i;
                free_visits[i] = true;

                // kolejny punkt
                VisitsCombinationForVehicle(visit + 1, veh_visits, i + 1, splits, veh, free_visits, div, combinations);

                free_visits[i] = false;
            }
        }

        // ------------------------------------- wrapper dla TSP (nie trzeba bedzie wywolywac dla kazdego osobno)
        // combinations - mhmhm, poczatki sciezek :)
        Tuple<float, List<int>[], List<float>[]> TSPWrapper(int[] combinations, int[][] splits)
        {
            float total_min_cost = 0;
            float min_cost;
            List<int>[] cycle = new List<int>[vehiclesCount];
            //List<int>[] path = new List<int>[vehiclesCount];
            List<float>[] times = new List<float>[vehiclesCount];

            for (int i = 0; i < vehiclesCount; ++i)
            {
                min_cost = Single.MaxValue;
                bool[] to_visit = new bool[visitsCount]; //new bool[splits[i].Length];
                //cycle[i] = new int[splits[i].Length];
                // path[i] = new List<int>();
                //List<int> tmp_path = new List<int>(); tmp_path.Add(depots[combinations[i]]);
                int[] tmp_cycle = new int[2 * visitsCount + 1];
                tmp_cycle[0] = depots[combinations[i]];
                float[] tmp_times = new float[2 * visitsCount + 1];
                tmp_times[0] = depotsTimeWindow[combinations[i]].Item1;
                FTSPFS(depots[combinations[i]], splits[i], to_visit, 0, 0, ref min_cost, tmp_cycle, 1, ref cycle[i], tmp_times, ref times[i],
                    depotsTimeWindow[combinations[i]].Item1, capacities/*, tmp_path, ref path[i]*/);
                total_min_cost += min_cost;
                if (total_min_cost >= Single.MaxValue)
                    break;
            }

            return new Tuple<float, List<int>[], List<float>[]>(total_min_cost, cycle, times);
        }

        //--------------------------------------FTSTPFS capitan (-:
        // cos musi z czasem jescze byc dodane pewnie
        void FTSPFS(int v, int[] to_visit, bool[] vis, int visited, float len, ref float min_len, int [] tmp_cycle, int cycle_pos, ref List<int> cycle, float[] tmp_times, ref List<float> times, float time, float cap/*, List<int> path, ref List<int> min_path*/)
        {
            if (len > min_len)
                return;

            if (visited == to_visit.Length)
            {
                // TODO: dodanie elementu w miejscu ostatniej zajezdni
                // dodaj nowy najlepszy cykl
                for (int i = 0; i < depotsCount; ++i)
                    if (time + weights[v, depots[i]] <= depotsTimeWindow[i].Item2 && min_len > len + weights[v, depots[i]])
                    {
                        min_len = len + weights[v, depots[i]];
                        /*path.Add(depots[i]);
                        min_path = new List<int>(path);
                        path.RemoveAt(path.Count - 1);*/
                        tmp_cycle[cycle_pos] = depots[i];
                        tmp_times[cycle_pos] = time + weights[v, depots[i]];
                        cycle = new List<int>(tmp_cycle.Take(cycle_pos + 1));
                        times = new List<float>(tmp_times.Take(cycle_pos + 1));
                    }
                return;
            }

            if (isTimeout)
                throw new TimeoutException();

            // dla kazdego sasiada
            // mozna dodac sprawdzanie zajezdni i odrzucac, jak dla wszystkich juz sie nie da dojechac 
            // (poki co jest pomysl tylko na rozwiazanie tego problemu)
            foreach (int w in to_visit)
            {
                if (!vis[w] /*&& visitAvailableTime[w] < time*/)
                {
                    float www = 0;
                    int last_v = v;
                    float last_cap = cap;
                    // TODO: pierwszy depot, musi byc zmienione
                    if (cap + visitsWeight[w] < 0)
                    {
                        cap = capacities;
                        www = weights[v, depots[0]];
                        v = depots[0];
                        tmp_times[cycle_pos] = time + www;
                        tmp_cycle[cycle_pos++] = v;
                        //path.Add(v);
                    }

                    // czekaj na dostepnosc
                    if (visitAvailableTime[w] > time)
                        time = visitAvailableTime[w];

                    vis[w] = true;
                    //cycle[visited] = w;
                    //path.Add(visits[w]);
                    tmp_cycle[cycle_pos] = visits[w];
                    tmp_times[cycle_pos] = time + weights[v, visits[w]] + www;
                    FTSPFS(visits[w], to_visit, vis, visited + 1, len + weights[v, visits[w]] + www, ref min_len, tmp_cycle, cycle_pos + 1, ref cycle, tmp_times, ref times,
                        time + weights[v, visits[w]] + visitsDuration[w] + www, cap + visitsWeight[w]/*, path, ref min_path*/);
                    //path.RemoveAt(path.Count - 1);
                    if (www != 0)
                    {
                        //path.RemoveAt(path.Count - 1);
                        v = last_v;
                        cap = last_cap;
                        cycle_pos--;
                    }

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

            visitsWeight = new float[visitsCount];
            visitAvailableTime = new float[visitsCount];
            visitsDuration = new float[visitsCount];

            for (int i = 0; i < visitsCount; ++i)
            {
                visitsWeight[i] = problemParser.VisitQuantity[problemParser.Visits[i]];
                visitsDuration[i] = problemParser.VisitsDuration[problemParser.Visits[i]];
                if (problemParser.TimeAvail == null)
                    visitAvailableTime[i] = 0;
                else
                    visitAvailableTime[i] = problemParser.TimeAvail[problemParser.Visits[i]];
            }

            depotsTimeWindow = new Tuple<float, float>[depotsCount];

            for (int i = 0; i < depotsCount; ++i)
            {
                depotsTimeWindow[i] = problemParser.DepotsTimeWindow[problemParser.Depots[i]];
            }
        }

        public override byte[][] DivideProblem(int threadCount)
        {
            byte[][] ret = null;

            var f = new StreamWriter("a.txt", true);

            for (int i = 0; i < visitsCount; ++i)
                f.Write(i + "\t");
            f.WriteLine();
            for (int i = 0; i < visitsCount; ++i)
                f.Write(visitAvailableTime[i] + "\t");
            f.WriteLine();
            f.WriteLine();
            f.Write("\t");
            for (int i = 0; i < locationsCount; ++i)
                f.Write(i + "\t");
            f.WriteLine();
            for (int i = 0; i < locationsCount; ++i)
            {
                f.Write(i + ":\t");
                for (int j = 0; j < locationsCount; ++j)
                {
                    f.Write("{0:N2}\t", weights[i, j]);
                }

                f.WriteLine();
            }

            f.Close();

            try
            {
                State = TaskSolverState.Dividing;

                List<int[]> list = new List<int[]>();

                if (visitsCount <= 40)
                    GenerateSets(visitsCount, vehiclesCount, list, visitsCount, 0, new int[vehiclesCount], 0);
                else
                {
                    int w = visitsCount <= 60 ? 4 : (visitsCount <= 100 ? 3 : 2);
                    int vehiclesLimit = vehiclesCount <= w ? vehiclesCount : w;
                    GenerateSets(visitsCount, vehiclesLimit, list, visitsCount, 0, new int[vehiclesLimit], vehiclesCount - vehiclesLimit);
                }

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

                ret = new byte[tc][];
                for (int i = 0; i < tc; ++i)
                {
                    MemoryStream s = new MemoryStream();
                    new BinaryFormatter().Serialize(s, r[i]);
                    ret[i] = s.ToArray();
                }
                PartialProblems = ret;

                State = TaskSolverState.Idle;
                if (ProblemDividingFinished != null)
                    ProblemDividingFinished(new EventArgs(), this);
            }
            catch (Exception e)
            {
                State = TaskSolverState.Error;
                if (ErrorOccured != null)
                    ErrorOccured(this, new UnhandledExceptionEventArgs(e, true));
            }

            return ret;
        }

        private void Initialize()
        {
            if (weights == null)
            {
                weights = new float[locationsCount, locationsCount];
                bool isManhattan = false;

                for (int i = 0; i < locationsCount; ++i)
                {
                    for (int j = 0; j < locationsCount; ++j)
                    {
                        weights[i, j] = //isManhattan ?
                            //(Math.Abs(locationsCoords[i].X - locationsCoords[j].X) + Math.Abs(locationsCoords[i].Y - locationsCoords[j].Y)) :
                            (float)(Math.Sqrt(Math.Pow(locationsCoords[i].X - locationsCoords[j].X, 2) + Math.Pow(locationsCoords[i].Y - locationsCoords[j].Y, 2)));
                    }
                }
            }
            // init cut-off stuff

            float max_time = Single.MinValue;
            foreach (Tuple<float, float> time in depotsTimeWindow)
                if (max_time < time.Item2)
                    max_time = time.Item2;

            float cut_off = max_time * 0.5f;

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


        //--------------------------------------Merge

        public override void MergeSolution(byte[][] solutions)
        {
            try
            {
                State = TaskSolverState.Merging;

                BinaryFormatter bf = new BinaryFormatter();
                List<SolutionContainer> sl = new List<SolutionContainer>();
                //List<float> min_costs = new List<float>();

                foreach (byte[] x in solutions)
                    sl.Add((SolutionContainer)bf.Deserialize(new MemoryStream(x)));

                SolutionContainer mc = sl.Min();

                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, mc);
                Solution = ms.ToArray();

                State = TaskSolverState.Idle;
                if (SolutionsMergingFinished != null)
                    SolutionsMergingFinished(new EventArgs(), this);
            }
            catch (Exception e)
            {
                State = TaskSolverState.Error;
                if (ErrorOccured != null)
                    ErrorOccured(this, new UnhandledExceptionEventArgs(e, true));
            }

            return;
        }

        public override string Name
        {
            get { return "DVRP"; }
        }

        public override event UnhandledExceptionEventHandler ErrorOccured;

        public override event ComputationsFinishedEventHandler ProblemDividingFinished;

        public override event ComputationsFinishedEventHandler ProblemSolvingFinished;

        public override event ComputationsFinishedEventHandler SolutionsMergingFinished;
    }
}
