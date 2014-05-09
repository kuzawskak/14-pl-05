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
    public class SolutionContainer : IComparable<SolutionContainer>
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
    public class DVRP : TaskSolver
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
        /// Pole przechowujące nowy cykl.
        /// </summary>
        List<int>[] new_cycle;

        /// <summary>
        /// Pole przechowujące nowe czasy przyjazdu.
        /// </summary>
        List<float>[] new_times;

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
                Thread.Sleep(1000);
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
            if(timeout != null)
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
                    TestDepotsCombinations(0, new int[vehiclesCount], div);
                }

                //Console.WriteLine();
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
                if (ProblemSolvingFinished != null)
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
                if (ProblemSolvingFinished != null)
                    ProblemSolvingFinished(new EventArgs(), this);
            }
            catch (Exception e)
            {
                State = TaskSolverState.Error | TaskSolverState.Idle;
                if (ErrorOccured != null)
                    ErrorOccured(this, new UnhandledExceptionEventArgs(e, true));
            }

            isSolving = false;
            if(timeout != null)
                backgroundTask.Wait();

            return Solution;
        }

        /// <summary>
        /// Metoda wyznacza wszystkie kombinacje zajezdni z których mają wyjechać pojazdy.
        /// </summary>
        /// <param name="n">Numer pojazdu</param>
        /// <param name="depotsCombinations">taablica zawierająca zajezdnie z których mają wyjechać pojazdy.</param>
        /// <param name="numLocForVeh">Przydział liczby lokacji do odwiedzenia przez każdy z pojazdów.</param>
        void TestDepotsCombinations(int n, int[] depotsCombinations, int[] numLocForVeh)
        {
            // Jeżeli dla każdego pojazdu jest już przypisana zajezdnia z której ma wyjechać to przechodzimy do kolejnego etapu.
            if (n >= vehiclesCount)
            {
                // Przydział lokacji dla pojazdów.
                SplitVisits(depotsCombinations, numLocForVeh);
                return;
            }

            // Testowanie każdej zajezdni dla pojazdu 'num'.
            for (int d = 0; d < depotsCount; ++d)
            {
                if (isTimeout)
                    throw new TimeoutException();

                // Wywoływanie metody dla pojazdu 'num + 1'.
                depotsCombinations[n] = d;
                TestDepotsCombinations(n + 1, depotsCombinations, numLocForVeh);
            }
        }

        /// <summary>
        /// Sprawdza każdą kombinację lokacji dla każdego pojazdu, zgodznie z przydziałem liczby loacji dla pojazdów.
        /// </summary>
        /// <param name="depotsCombinations">Zajezdnie z których musi wyjechać każdy z pojazdów.</param>
        /// <param name="numLocationsForVehicles">Liczba lokacji, którą musi odwiedzić każdy pojazd.</param>
        void SplitVisits(int[] depotsCombinations, int[] numLocationsForVehicles)
        {
            // Lokacje do odwiedzenia przez pojazdy (splits[pojazd][numer] = lokacja).
            int[][] splits = new int[vehiclesCount][];

            for (int i = 0; i < vehiclesCount; ++i)
                splits[i] = new int[numLocationsForVehicles[i]];
            
            // Przydzielanie wizyt dla pojazdów.
            VisitsForEachVehicle(0, new bool[visitsCount], splits, numLocationsForVehicles, depotsCombinations, 0);
        }

        /// <summary>
        /// Przydzielanie wizyt dla pojazdów.
        /// Każdy pojazd będzie miał przydzielone wszystkie kombinacje odpowiedniej liczby lokacji do odwiedzenia.
        /// </summary>
        /// <param name="vehicle">Numer pojazdu, dla którego przydzielane są lokacje.</param>
        /// <param name="taken_visits">Tablica do oznaczanie już przypisanych lokacji.</param>
        /// <param name="splits">Przydział lokacji dla pojazdów. Tablica do wypełnienia.</param>
        /// <param name="numLocationsForVehicles">Liczba lokacji do odwiedzenia przez każdy pojazd.</param>
        /// <param name="depotsCombinations">Zajezdnie z których mają wyjechać pojazdy.</param>
        /// <param name="totalMinCost">Minimalny koszt ścieżki.</param>
        void VisitsForEachVehicle(int vehicle, bool[] taken_visits, int[][] splits, int[] numLocationsForVehicles, int[] depotsCombinations, float totalMinCost)
        {
            //if (vehicle == 0)
            //    Console.WriteLine("==================================");
            //if (vehicle == 1)
            //    Console.WriteLine(string.Join("; ", splits.Select(x => string.Join(" ", x.Select(y => y.ToString())))));

            // Jeżeli wszystkie pojazdy mają policzone TSP.
            if (vehicle >= vehiclesCount)
            {
                // Jeżeli nowy najmniejszy koszt, to zapisujemy.
                if (totalMinCost < min_cost)
                {
                    min_cost = totalMinCost;
                    cycles = new List<int>[vehiclesCount];
                    times = new List<float>[vehiclesCount];

                    for (int i = 0; i < vehiclesCount; ++i)
                    {
                        cycles[i] = new List<int>(new_cycle[i]);
                        times[i] = new List<float>(new_times[i]);
                    }
                }

                return;
            }

            if (isTimeout)
                throw new TimeoutException();

            // Wybieranie lokacji dla pojazdu 'vehicle'.
            VisitsCombinationForVehicle(0, 0, splits, vehicle, taken_visits, numLocationsForVehicles, depotsCombinations, totalMinCost);
        }

        /// <summary>
        /// Kombinacja wizyt dla pojazdu.
        /// Przydziela pojazdowi kombinacje odpowiedniej liczby wizyt.
        /// </summary>
        /// <param name="visit">Numer wizyty dla pojazdu.</param>
        /// <param name="start">Indeks wizyty. Zaczyna szukać od tej wizyty pomijając poprzednie - sprawdzone wcześniej.</param>
        /// <param name="splits">Przydzielone wizyty - tablica do wypełnienia.</param>
        /// <param name="vehicle">Numer pojazdu, dla którego przydzielane są wizyty.</param>
        /// <param name="taken_visits">Wizyty już zajęte.</param>
        /// <param name="numLocationsForVehicle">Liczba lokacji do odwiedzenia przez każdy pojazd.</param>
        /// <param name="depotsCombinations">Zajezdnie, z których ma wyruszyć każdy z pojazdów.</param>
        /// <param name="totalMinCost">Minimalny koszt ścieżki.</param>
        void VisitsCombinationForVehicle(int visit, int start, int[][] splits, int vehicle, bool[] taken_visits, int[] numLocationsForVehicle, int[] depotsCombinations, float totalMinCost)
        {
            // Jeżeli wszystkie wizyty przydzielone dla tego pojazdu, to liczymy TSP i przydzielamy dla nastpnego.
            if (visit >= numLocationsForVehicle[vehicle])
            {
                float minCost = Single.MaxValue;

                // Tymczasowy cykl. Po wypełnieniu porównywany z najmniejszym lokalnym (dla pojazdu).
                int[] tmp_cycle = new int[2 * visitsCount + 1];
                tmp_cycle[0] = depots[depotsCombinations[vehicle]];
                new_cycle[vehicle] = null;

                // Tymczasowe czasy odwiedzin.
                float[] tmp_times = new float[2 * visitsCount + 1];
                tmp_times[0] = depotsTimeWindow[depotsCombinations[vehicle]].Item1;

                // Wywołanie TSP dla pojazdu 'vehicle'.
                FTSPFS(depots[depotsCombinations[vehicle]], splits[vehicle], new bool[visitsCount], 0, 0, ref minCost,
                    tmp_cycle, 1, ref new_cycle[vehicle], tmp_times, ref new_times[vehicle], depotsTimeWindow[depotsCombinations[vehicle]].Item1, capacities);

                if (minCost + totalMinCost > Single.MaxValue)
                    return;

                if (minCost + totalMinCost > min_cost)
                    return;

                // Liczymy dla następnego.
                VisitsForEachVehicle(vehicle + 1, taken_visits, splits, numLocationsForVehicle, depotsCombinations, minCost + totalMinCost);
                return;
            }

            // Każda dostępna i nie sprawdzona wizyta jest wpisywana jako wizyta numer 'visit' dla pojazdu 'vehicle'.
            for (int i = start; i < visitsCount; ++i)
            {
                // Jeżeli już zajęta to pomijamy.
                if (taken_visits[i])
                    continue;

                splits[vehicle][visit] = i;
                taken_visits[i] = true;

                // Przydział kolejnej wizyty dla pojazdu.
                VisitsCombinationForVehicle(visit + 1, i + 1, splits, vehicle, taken_visits, numLocationsForVehicle, depotsCombinations, totalMinCost);

                taken_visits[i] = false;
            }
        }

        /// <summary>
        /// TSP z ograniczeniami liczone dla jednego pojazdu.
        /// </summary>
        /// <param name="v">Wierzchołek źrófłowy (podany jako lokacja - nie wizyta!).</param>
        /// <param name="to_visit">Wizyty do odwiedzenia.</param>
        /// <param name="vis">Lokacje odwiedzone/nieodwiedzone.</param>
        /// <param name="visited">Liczba odwiedzonych lokacji.</param>
        /// <param name="len">Długość ścieżki.</param>
        /// <param name="min_len">Minimalna ścieżka dla tego pojazdu.</param>
        /// <param name="tmp_cycle">Znajdowana ścieżka.</param>
        /// <param name="cycle_pos">Aktualna pozycja na ścieżce.</param>
        /// <param name="cycle">Najlepsza ścieżka dla tego pojazdu.</param>
        /// <param name="tmp_times">Znajdowane czasy dojazdów do lokacji na ścieżce dla pojazdu.</param>
        /// <param name="times">Najlepsze czasy dojazdu dla tego pojazdu.</param>
        /// <param name="time">Aktualny czas pojazdu.</param>
        /// <param name="cap">Aktualna waga ładunku w pojeździe.</param>
        void FTSPFS(int v, int[] to_visit, bool[] vis, int visited, float len, ref float min_len, int [] tmp_cycle, int cycle_pos, ref List<int> cycle, float[] tmp_times, ref List<float> times, float time, float cap/*, List<int> path, ref List<int> min_path*/)
        {
            // Jeżeli waga ścieżki jest większa niż aktualna najmniejsza to wracamy.
            if (len > min_len)
                return;

            // Jeżeli wszystkie już odwiedzone.
            if (visited == to_visit.Length)
            {
                // Szukanie najbliższej zajezdni, do której można wrócić (która jest jeszcze otwarta).
                for (int i = 0; i < depotsCount; ++i)
                {
                    if (time + weights[v, depots[i]] <= depotsTimeWindow[i].Item2 && min_len > len + weights[v, depots[i]])
                    {
                        // Dodawanie nowej najmniejszej ścieżki i czasów.
                        min_len = len + weights[v, depots[i]];
                        tmp_cycle[cycle_pos] = depots[i];
                        tmp_times[cycle_pos] = time + weights[v, depots[i]];
                        cycle = new List<int>(tmp_cycle.Take(cycle_pos + 1));
                        times = new List<float>(tmp_times.Take(cycle_pos + 1));
                    }
                }

                return;
            }

            if (isTimeout)
                throw new TimeoutException();

            // Dla każdej lokacji do odwiedzenia przez pojazd.
            foreach (int w in to_visit)
            {
                // Jeżeli nie została już odwiedzona.
                if (!vis[w])
                {
                    float www = 0;
                    int last_v = v;
                    float last_cap = cap;
                    
                    // Jeżeli pojazd ma za mało ładunku to jedziemy do zajezdni.
                    if (cap + visitsWeight[w] < 0)
                    {
                        float depot_len = float.MaxValue;
                        for (int i = 0; i < depotsCount; ++i)
                        {
                            if (time + weights[last_v, depots[i]] <= depotsTimeWindow[i].Item2 && depot_len > weights[last_v, depots[i]] + weights[depots[i], visits[w]])
                            {
                                depot_len = weights[last_v, depots[i]] + weights[depots[i], visits[w]];

                                // Waga do zajezdni.
                                www = weights[last_v, depots[i]];

                                // v jest równy zajezdni.
                                v = depots[i];

                                // Dodanie do ścieżki i czasu.
                                tmp_times[cycle_pos] = time + www;
                                tmp_cycle[cycle_pos] = v;
                            }
                        }

                        // Jeżeli nie znaleziono zajezdni.
                        if (depot_len == float.MaxValue)
                        {
                            v = last_v;
                            continue;
                        }

                        // Dodanie do cyklu.
                        cycle_pos++;

                        // Wypełnienie pojazdu.
                        cap = capacities;
                    }

                    // Jeżeli wizyta nie jest dostępna to czekamy.
                    if (visitAvailableTime[w] > time)
                        time = visitAvailableTime[w];

                    vis[w] = true;
                    
                    // Dodanie kolejnego punktu do ścieżki i czasu.
                    tmp_cycle[cycle_pos] = visits[w];
                    tmp_times[cycle_pos] = time + weights[v, visits[w]] + www;
                    
                    // Wywołanie TSP dla kolejnego punktu.
                    FTSPFS(visits[w], to_visit, vis, visited + 1, len + weights[v, visits[w]] + www, ref min_len, tmp_cycle, 
                        cycle_pos + 1, ref cycle, tmp_times, ref times, time + weights[v, visits[w]] + visitsDuration[w] + www, cap + visitsWeight[w]);
                   
                    // Jeżeli odwiedziliśmy zajezdnię to się cofamy.
                    if (www != 0)
                    {
                        v = last_v;
                        cap = last_cap;
                        cycle_pos--;
                    }

                    vis[w] = false;
                }
            }
        }

        /// <summary>
        /// Metoda wywoływana w konstruktorze. Parsuje dane i wypełnia pola klasy DVRP.
        /// </summary>
        /// <param name="data">Dane problemu.</param>
        private void SetProblemData(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            TestFileParser problemParser = new TestFileParser(stream);

            capacities = problemParser.Capacities;
            vehiclesCount = problemParser.NumVehicles;
            depotsCount = problemParser.NumDepots;
            visitsCount = problemParser.NumVisits;
            locationsCount = depotsCount + visitsCount;

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

        /// <summary>
        /// Podział problemu na podproblemy.
        /// Podział jest wykonywany na zasadzie podziału liczby na składniki. W tym przypadku liczbą jest liczba lokacji,
        /// a składnikami na ile należy ją podzielić liczba pojazdów. Dzięki temu uzyskamy listę tablic zawierających liczbę
        /// lokacji do odwiedzenia przez kazdy z pojazdów.
        /// Dla przykładowego problemu 3 pojazdów i 5 lokacji uzyskamy:
        /// [5, 0, 0]
        /// [4, 1, 0]
        /// [3, 2, 0]
        /// [3, 1, 1]
        /// [2, 2, 1]
        /// 
        /// Pojazdy są nierozróżnialne, więc zbiory [x, y, z], [z, y, x], [y, z, x] itd będą takie same.
        /// Algorytm dodatkowo tworzy listy podproblemów dla odpowiedniej liczby CN.
        /// </summary>
        /// <param name="threadCount">Liczba dostępnych wątków.</param>
        /// <returns>Zserializowany podział problemów.</returns>
        public override byte[][] DivideProblem(int threadCount)
        {
            byte[][] ret = null;

            try
            {
                State = TaskSolverState.Dividing;

                List<int[]> list = new List<int[]>();

                //if (visitsCount <= 40)
                // Generowanie zbiorów.
                GenerateSets(visitsCount, vehiclesCount, list, visitsCount, 0, new int[vehiclesCount]/*, 0*/);
                //else
                //{
                //    int w = visitsCount <= 60 ? 4 : (visitsCount <= 100 ? 3 : 2);
                //    int vehiclesLimit = vehiclesCount <= w ? vehiclesCount : w;
                //    GenerateSets(visitsCount, vehiclesLimit, list, visitsCount, 0, new int[vehiclesLimit], vehiclesCount - vehiclesLimit);
                //}

                // Problem zostanie podzielony na tc części.
                int tc = threadCount < list.Count ? threadCount : list.Count;

                List<int[]>[] r = new List<int[]>[tc];

                for (int i = 0; i < tc; ++i)
                {
                    r[i] = new List<int[]>();
                }

                Random rand = new Random();
                int n = 0;
                
                // Podział problemów. Podział losowy - może nie być optymalnie, jednak jest wystarczająco, ponieważ nie wiemy
                // ile czasu będzie rozwiązywać się każda grupa.
                while (list.Count > 0)
                {
                    int x = rand.Next(list.Count);
                    r[n].Add(list[x]);
                    list.RemoveAt(x);
                    ++n;
                    if (n == tc)
                        n = 0;
                }

                // Serializacja danych.
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
                State = TaskSolverState.Error | TaskSolverState.Idle;
                if (ErrorOccured != null)
                    ErrorOccured(this, new UnhandledExceptionEventArgs(e, true));
            }

            return ret;
        }

        /// <summary>
        /// Inicjalizacja danych.
        /// </summary>
        private void Initialize()
        {
            // Obliczanie wag krawędzi.
            if (weights == null)
            {
                weights = new float[locationsCount, locationsCount];

                for (int i = 0; i < locationsCount; ++i)
                {
                    for (int j = 0; j < locationsCount; ++j)
                    {
                        weights[i, j] = (float)(Math.Sqrt(Math.Pow(locationsCoords[i].X - locationsCoords[j].X, 2) 
                            + Math.Pow(locationsCoords[i].Y - locationsCoords[j].Y, 2)));
                    }
                }
            }

            // Warunek cut-off time.
            float max_time = Single.MinValue;
            foreach (Tuple<float, float> time in depotsTimeWindow)
                if (max_time < time.Item2)
                    max_time = time.Item2;

            float cut_off = max_time * 0.5f;

            for (int i = 0; i < visitAvailableTime.Length; ++i)
                if (visitAvailableTime[i] > cut_off)
                    visitAvailableTime[i] = 0;

            new_cycle = new List<int>[vehiclesCount];
            new_times = new List<float>[vehiclesCount];
        }

        /// <summary>
        /// Generowanie zbiorów podproblemów.
        /// </summary>
        /// <param name="n">Liczba wizyt.</param>
        /// <param name="k">Liczba pojazdów.</param>
        /// <param name="list">Lista podzbiorów.</param>
        /// <param name="left">Pozostała liczba wizyt do przydzielenia.</param>
        /// <param name="i">Aktualny pojazd.</param>
        /// <param name="tmp">Tymczasowy zbiów z podziałem.</param>
        private void GenerateSets(int n, int k, List<int[]> list, int left, int i, int[] tmp/*, int more*/)
        {
            if (i == k)
            {
                if (left == 0 /*|| tmp[i - 1] * more >= left*/)
                    list.Add((int[])tmp.Clone());
                return;
            }

            // Nierozróżnialność pojazdów - kolejna liczba nie większa niż poprzednia.
            int put = (i == 0 || tmp[i - 1] >= left) ? left : tmp[i - 1];
            left = left - put;

            while (put >= 0)
            {
                if (put == 0 && left != 0)
                    return;

                tmp[i] = put;
                
                GenerateSets(n, k, list, left, i + 1, tmp/*, more*/);
                put--;
                left++;
            }
        }

        /// <summary>
        /// Wybieranie najleszego wyniku z listy wyników.
        /// </summary>
        /// <param name="solutions">Rozwiązania.</param>
        public override void MergeSolution(byte[][] solutions)
        {
            try
            {
                State = TaskSolverState.Merging;

                // Deserializacja.
                BinaryFormatter bf = new BinaryFormatter();
                List<SolutionContainer> sl = new List<SolutionContainer>();

                foreach (byte[] x in solutions)
                    sl.Add((SolutionContainer)bf.Deserialize(new MemoryStream(x)));

                // Wybieranie minimum.
                SolutionContainer mc = sl.Min();

                // Serializacja.
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, mc);
                Solution = ms.ToArray();

                State = TaskSolverState.Idle;
                if (SolutionsMergingFinished != null)
                    SolutionsMergingFinished(new EventArgs(), this);

                //Task.Factory.StartNew(() => System.Windows.Forms.MessageBox.Show("OK"));
            }
            catch (Exception e)
            {
                Task.Factory.StartNew(() => System.Windows.Forms.MessageBox.Show(e.ToString()));
                State = TaskSolverState.Error | TaskSolverState.Idle;
                if (ErrorOccured != null)
                    ErrorOccured(this, new UnhandledExceptionEventArgs(e, true));
            }

            return;
        }

        /// <summary>
        /// Nazwa problemu.
        /// </summary>
        public override string Name
        {
            get { return "DVRP"; }
        }

        /// <summary>
        /// Wywołanie w przypadku błędu.
        /// </summary>
        public override event UnhandledExceptionEventHandler ErrorOccured;

        /// <summary>
        /// Wywołanie w przypadku zakończenia dzielenia problemu.
        /// </summary>
        public override event ComputationsFinishedEventHandler ProblemDividingFinished;

        /// <summary>
        /// Wywołanie w przypadku zakończenia rozwiązywania problemu.
        /// </summary>
        public override event ComputationsFinishedEventHandler ProblemSolvingFinished;

        /// <summary>
        /// Wywołanie w przypadku zakończnenia łączenia rozwiązań.
        /// </summary>
        public override event ComputationsFinishedEventHandler SolutionsMergingFinished;
    }
}
