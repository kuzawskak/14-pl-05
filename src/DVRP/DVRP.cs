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
        // Liczba ograniczeń pojemności - domyślnie 1 - waga
        private int capacitiesCount;    // 1

        // Ograniczenia dla pojazdów (domyślnie double[1] - waga). Definiuje ograniczenia dla wszystkich pojazdów. 
        // Aby zdefiniować każdy osobno używamy vehicleCapacities
        // Jeżeli jest podane, wypełniana jest tablica vehicleCapacities tą samą wartością
        private double[] capacities;    // 2

        // Liczba pojazdów
        private int vehiclesCount;      // 3

        // Liczba zajezdni
        private int depotsCount;        // 4

        // Liczba miejsc do odwiedzenia (pickup and delivery points)
        private int visitsCount;        // 5

        // Liczba lokacji (visitsCount + depotsCount)
        private int locationsCount;     // 6

        // Max czas pojedyńczego przejazdu.
        private double maxTime;         // 7

        // Położenie lokacji (x, y). Używane gdy nie ma podanych wag krawędzi. (przeliczymy na wagi)
        private System.Windows.Point[] locationsCoords;     // 8

        // "Numery wierzchołków" zajezdni
        private int[] depots;           // 9

        // Numery wierzchołków miejsc do odwiedzenia.
        private int[] visits;           // 10

        // Waga ładunku do odebrania/zostawienia w każdej lokacji. (ujemna = dostarczenie)
        // Domyślnie int[1][] - tylko waga
        private double[][] visitsWeight;    // 11

        // Pojemności pojzadów - domyślnie int[1][] - tylko pojemność
        private double[][] vehiclesCapacity;    // 12

        // Wagi krawędzi
        private double[,] weights;      // 13

        // Czas otwarcia/zamknięcia miejsc do odwiedzenia <otwarcie, zamkniecie>
        private Tuple<double, double>[][] timeWindows;  // 14

        // Czas otwarcia i zamknięcia zajezdni.
        private Tuple<double, double>[] depotsTimeWindow;   // 15

        // Czas dostępności pojazdów. null == cly czas
        private Tuple<double, double>[][] vehiclesTimeWindow;   // 16

        // Czas kiedy wizyta będzie znana.
        private double[] visitAvailableTime;        // 17

        //---------------------------------------Mniej ważne :P

        // Prędkość służąca do zmiany wag krawędzi w czas. Domyślnie 1.
        // Jeżeli podana prędkość, to dla każdego pojazdu jest wpisywana do tablicy.
        private double speed;       // 18

        // Prędkość każdego pojazdu.
        // Jest mnożona z wagami
        private double[] vehicleSpeed;   // 19

        // Koszt użycia pojazdu (domyślnie 0).
        private double[] vehicleUseCost;    // 20

        // Koszt dystandu dla pojazdów (domyślnie 1).
        private double[] vehicleDistanceCost;   // 21

        // Koszt czasu podróży pojazdów (domyślnie 0).
        private double[] vehicleTimeCost;       // 22

        // Czas przebywania - domyślnie 0.
        private double[] visitsDuration;        // 23

        // Czas przebywania w lokacjach przez każdy z pojazdów. int[visitId][vehicleId]. null - nie ma
        private double[][] durationByVehicle;   // 24

        // Zajezdnia pojazdu. <czy istotna, idZajezdni>[idPojazdu]. null - nie istotne
        private Tuple<bool, int>[] vehicleDepot;    // 25

        //----------------------------------------

        // Kolejność. <visit1, relacja, visit2, margines>. Wizyta 1 musi być przed wizytą 2, różnica musi być w relacji z marginesem. ????????
        private Tuple<int, string, int, double>[/**/] order;    // 26

        // Wizyty opcjonalne i koszt nieodwiedzenia. <id wizyty, koszt>
        private Tuple<int, double>[/**/] optionalVisits;        // 27

        // Wizyty muszą/nie mogą być obsłużone przez ten sam pojazd. <wizyta1, wizyta2, true - muszą, false - nie mogą.
        private Tuple<int, int, bool>[/**/] visitCompat;        // 28

        // Wyzyty które muszą być obsłużone z odpowiedniej zajezdni. <wizyta, zajezdnia, musi/nie może>
        private Tuple<int, int, bool>[/**/] depotCompat;        // 29

        // Wizyty muszą/nie mogą być obsłużone przez pojazdy. <wizyta, pojazd, musi/nie może>
        private Tuple<int, int, bool>[/**/] vehicleCompat;      // 30

        //---------------------------------------Pomocnicze (po podziale)

        //--------------------------------------

        private DVRP()
        {
        }

        private DVRP(int[] _depots, int[] _visits, double[] _visitsWeight, double[] _vehiclesCapacity, double[,] _weights, Tuple<double, double>[][] _timeWindows, Tuple<double, double>[] _depotsTimeWindow, double[] _visitAvailableTime)
        {
            capacitiesCount = 1;
            capacities = null;
            vehiclesCount = _vehiclesCapacity.Length;
            depotsCount = _depots.Length;
            visitsCount = _visits.Length;
            locationsCount = visitsCount + depotsCount;
            maxTime = double.PositiveInfinity;
            locationsCoords = null;
            depots = _depots;
            visits = _visits;
            visitsWeight = new double[capacitiesCount][];
            visitsWeight[0] = _visitsWeight;
            vehiclesCapacity = new double[capacitiesCount][];
            vehiclesCapacity[0] = _vehiclesCapacity;
            weights = _weights;
            timeWindows = _timeWindows;
            depotsTimeWindow = _depotsTimeWindow;
            vehiclesTimeWindow = null;
            visitAvailableTime = _visitAvailableTime;

            speed = 1;
            vehicleSpeed = null;
            vehicleUseCost = null; // 0
            vehicleDistanceCost = null; // 1
            vehicleTimeCost = null; // 0
            visitsDuration = null; // 0
            durationByVehicle = null;
            vehicleDepot = null;
            order = new Tuple<int, string, int, double>[0];
            optionalVisits = new Tuple<int, double>[0];
            visitCompat = new Tuple<int, int, bool>[0];
            depotCompat = new Tuple<int, int, bool>[0];
            vehicleCompat = new Tuple<int, int, bool>[0];
        }

        public override TaskSolver GetInstance()
        {
            return DVRP.GetInstance(false);
        }

        public static DVRP GetInstance(bool b)
        {
            //return new DVRP();

            // Przykładowy problem.
            const int visits = 20;
            const int vehicles = 10;
            const int depots = 1;

            return new DVRP(
                new int[depots] { 0 },
                new int[visits] /*{ 1, 2, 3, 4, 5 }*/,
                new double[visits] /*{ 10, 10, 10, 10, 10 }*/,
                new double[vehicles] /*{ 20, 20, 20 }*/,
                new double[visits + depots, visits + depots]
                /*{
                    {5,2,4,7,3,5},
                    {2,3,4,1,7,8},
                    {5,2,9,1,3,3},
                    {4,5,4,6,3,3},
                    {3,2,4,5,3,2},
                    {4,3,5,4,2,2}
                }*/,
                new Tuple<double, double>[visits][]
                /*{
                    new Tuple<double, double>[1] { new Tuple<double, double>(0, double.PositiveInfinity) },
                    new Tuple<double, double>[1] { new Tuple<double, double>(0, double.PositiveInfinity) },
                    new Tuple<double, double>[1] { new Tuple<double, double>(0, double.PositiveInfinity) },
                    new Tuple<double, double>[1] { new Tuple<double, double>(0, double.PositiveInfinity) },
                    new Tuple<double, double>[1] { new Tuple<double, double>(0, double.PositiveInfinity) }
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

            return null;
        }

        //--------------------------------------Divide

        public override byte[][] DivideProblem(byte[] data, int threadCount)
        {
            Initialize();

            List<int[]> list = new List<int[]>();

            const int vehiclesLimit = 5;

            if (vehiclesCount <= vehiclesLimit)
            {
                GenerateSets(visitsCount, vehiclesCount, list, visitsCount, 0, new int[vehiclesCount], false);

                if (optionalVisits != null && optionalVisits.Length > 0)
                {
                    for (int i = visitsCount - 1; i >= visitsCount - optionalVisits.Length; --i)
                        GenerateSets(i, vehiclesCount, list, i, 0, new int[vehiclesCount], false);
                }
            }
            else
            {
                GenerateSets(visitsCount, vehiclesLimit, list, visitsCount, 0, new int[vehiclesLimit], true);
            }

            RemoveInvalid(list, (vehiclesCount <= vehiclesLimit) ? vehiclesCount : vehiclesLimit);

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

            //Console.WriteLine(list.Count);
            //foreach (int[] a in list)
            //{
            //    foreach (int i in a)
            //        Console.Write(i + " ");
            //    Console.WriteLine();
            //}

            return ret;
        }

        private void Initialize()
        {
            if (vehicleSpeed == null)
            {
                vehicleSpeed = new double[vehiclesCount];
                for (int i = 0; i < vehiclesCount; ++i)
                    vehicleSpeed[i] = speed;
            }

            if (vehiclesCapacity == null)
            {
                vehiclesCapacity = new double[capacitiesCount][];
                for (int i = 0; i < capacitiesCount; ++i)
                {
                    vehiclesCapacity[i] = new double[vehiclesCount];

                    for (int j = 0; j < vehiclesCount; ++j)
                        vehiclesCapacity[i][j] = capacities == null ? double.PositiveInfinity : capacities[j];
                }
            }

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

        private void GenerateSets(int n, int k, List<int[]> list, int left/* = -1*/, int i/* = 0*/, int[] tmp/* = null*/, bool isMore)
        {
            if (i == k)
            {
                if (isMore || left == 0)
                    list.Add((int[])tmp.Clone());
                return;
            }

            int put = left;
            left = 0;

            while (put >= 0)
            {
                tmp[i] = put;
                GenerateSets(n, k, list, left, i + 1, tmp, isMore);
                put--;
                left++;
            }
        }

        private void RemoveInvalid(List<int[]> list, int v)
        {
            Func<int, int, bool> cond12 = (int _i, int _j) =>
            {
                foreach (var c in vehiclesCapacity)
                    if (c[_i] != c[_j])
                        return false;
                return true;
            };

            Func<int, int, bool> cond16 = (int _i, int _j) =>
            {
                if (vehiclesTimeWindow == null)
                    return true;

                if (vehiclesTimeWindow[_i] == null && vehiclesTimeWindow[_j] == null)
                    return true;

                if (vehiclesTimeWindow[_i] == null || vehiclesTimeWindow[_j] == null)
                    return false;

                if (vehiclesTimeWindow[_i].Length != vehiclesTimeWindow[_j].Length)
                    return false;

                for (int k = 0; k < vehiclesTimeWindow[_i].Length; ++k)
                    if (vehiclesTimeWindow[_i][k].Item1 != vehiclesTimeWindow[_j][k].Item1 || vehiclesTimeWindow[_i][k].Item2 != vehiclesTimeWindow[_j][k].Item2)
                        return false;

                return true;
            };

            Func<int, int, bool> cond24 = (int _i, int _j) =>
            {
                if (durationByVehicle == null)
                    return true;

                foreach (var c in durationByVehicle)
                    if (c[_i] != c[_j])
                        return false;

                return true;
            };

            Func<int, int, bool> cond25 = (int _i, int _j) =>
            {
                if (vehicleDepot == null)
                    return true;

                if (vehicleDepot[_i].Item1 == false && vehicleDepot[_j].Item1 == false)
                    return true;

                if (vehicleDepot[_i].Item1 == false || vehicleDepot[_j].Item1 == false)
                    return false;

                if (vehicleDepot[_i].Item2 != vehicleDepot[_j].Item2)
                    return false;

                return true;
            };

            Func<int, int, bool> cond30 = (int _i, int _j) =>
            {
                if (vehicleCompat == null)
                    return true;

                var lit = vehicleCompat.Where(x => x.Item2 == _i && x.Item3 == true).Select(x => x.Item1).ToList();
                var ljt = vehicleCompat.Where(x => x.Item2 == _j && x.Item3 == true).Select(x => x.Item1).ToList();

                if (lit.Count != ljt.Count)
                    return false;

                lit.Sort();
                ljt.Sort();

                if (!lit.SequenceEqual(ljt))
                    return false;

                var lif = vehicleCompat.Where(x => x.Item2 == _i && x.Item3 == false).Select(x => x.Item1).ToList();
                var ljf = vehicleCompat.Where(x => x.Item2 == _j && x.Item3 == false).Select(x => x.Item1).ToList();

                if (lif.Count != ljf.Count)
                    return false;

                ljf.Sort();
                lif.Sort();

                if (!lif.SequenceEqual(ljf))
                    return false;

                return true;
            };

            for (int i = 0; i < v; ++i)
            {
                for (int j = i + 1; j < v; ++j)
                {
                    if (cond12(i, j) && cond16(i, j) &&
                        vehicleSpeed[i] == vehicleSpeed[j] &&
                        (vehicleUseCost == null || vehicleUseCost[i] == vehicleUseCost[j]) &&
                        (vehicleTimeCost == null || vehicleTimeCost[i] == vehicleTimeCost[j]) &&
                        (vehicleDistanceCost == null || vehicleDistanceCost[i] == vehicleDistanceCost[j]) &&
                        cond24(i, j) && cond25(i, j) && cond30(i, j))
                    {
                        RemovePair(i, j, list, v);
                    }
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

            //List<int[]> tmp = new List<int[]>(list);

            //foreach (var c in tmp)
            //{
            //    if (c[i] != c[j] && list.Contains(c))
            //    {
            //        list.RemoveAll(x =>
            //            {
            //                if (x[i] == c[j] && x[j] == c[i])
            //                {
            //                    for (int k = 0; k < v; ++k)
            //                        if (k != i && k != j && x[k] != c[k])
            //                            return false;

            //                    return true;
            //                }
            //                return false;
            //            });
            //    }
            //}
        }

        //--------------------------------------Merge

        public override byte[] MergeSolution(byte[][] solutions)
        {

            return null;
        }
    }
}
