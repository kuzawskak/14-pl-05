using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCCTaskSolver;

namespace DVRP
{
    class DVRP : TaskSolver
    {
        // Liczba ograniczeń pojemności - domyślnie 1 - waga
        private int capacitiesCount;

        // Ograniczenia dla pojazdów (domyślnie int[1] - waga). Definiuje ograniczenia dla wszystkich pojazdów. 
        // Aby zdefiniować każdy osobno używamy vehicleCapacities
        private int[] capacities;

        // Liczba pojazdów
        private int vehiclesCount;

        // Liczba zajezdni
        private int depotsCount;

        // Liczba miejsc do odwiedzenia (pickup and delivery points)
        private int visitsCount;

        // Liczba lokacji (visitsCount + depotsCount)
        private int locationsCount;

        // Max czas pojedyńczego przejazdu. 0 dla nieskończoności.
        private int maxTime;

        // Położenie lokacji (x, y). Używane gdy nie ma podanych wag krawędzi.
        private System.Drawing.Point[] locationsCoords;

        // "Numery wierzchołków" zajezdni
        private int[] depots;

        // Numery wierzchołków miejsc do odwiedzenia.
        private int[] visits;

        // Waga ładunku do odebrania/zostawienia w każdej lokacji. (ujemna = dostarczenie)
        // Domyślnie int[1][] - tylko waga
        private int[][] visitsWeight;

        // Pojemności pojzadów - domyślnie int[1][] - tylko pojemność
        private int[][] vehiclesCapacity;

        // Wagi krawędzi
        private int[,] weight;

        // Czas otwarcia/zamknięcia miejsc do odwiedzenia <otwarcie, zamkniecie>
        private Tuple<int, int>[][] timeWindows;

        // Czas otwarcia i zamknięcia zajezdni.
        private Tuple<int, int>[] depotsTimeWindow;

        // Czas dostępności pojazdów.
        private Tuple<int, int>[][] vehiclesTimeWindow;

        // Czas kiedy wizyta będzie znana.
        private int[] visitAvailableTime;

        //---------------------------------------

        // Prędkość służąca do zmiany wag krawędzi w czas. Domyślnie 1.
        private int speed;

        // Prędkość każdego pojazdu.
        private int[] vehicleSpeed;

        // Koszt użycia pojazdu (domyślnie 0).
        private float[] vehicleUseCost;

        // Koszt dystandu dla pojazdów (domyślnie 1).
        private float[] vehicleDistanceCost;

        // Koszt czasu podróży pojazdów (domyślnie 0).
        private float[] vehicleTimeCost;

        // Czas przebywania - domyślnie 0.
        private int[] visitsDuration;

        // Czas przebywania w lokacjach przez każdy z pojazdów. int[visitId][vehicleId]. null - nie ma
        private int[][] durationByVehicle;

        // Zajezdnia pojazdu. <czy istotna, idZajezdni>[idPojazdu]. null - nie istotne
        private Tuple<bool, int>[] vehicleDepot;

        // Kolejność. <visit1, relacja, visit2, margines>. Wizyta 1 musi być przed wizytą 2, różnica musi być w relacji z marginesem. ????????
        private Tuple<int, string, int, float>[/**/] order;

        // Wizyty opcjonalne i koszt nieodwiedzenia. <id wizyty, koszt>
        private Tuple<int, int>[/**/] optionalVisits;

        // Wizyty muszą/nie mogą być obsłużone przez ten sam pojazd. <wizyta1, wizyta2, true - muszą, false - nie mogą.
        private Tuple<int, int, bool>[/**/] visitCompat;

        // Wyzyty które muszą być obsłużone z odpowiedniej zajezdni. <wizyta, zajezdnia, musi/nie może>
        private Tuple<int, int, bool>[/**/] depotCompat;

        // Wizyty muszą/nie mogą być obsłużone przez pojazdy. <wizyta, pojazd, musi/nie może>
        private Tuple<int, int, bool>[/**/] vehicleCompat;

        //---------------------------------------

        private DVRP()
        {
        }

        public DVRP GetInstance()
        {
            return new DVRP();
        }

        public byte[] Solve(byte[] commonData, byte[] partialData, TimeSpan timeout)
        {

            return null;
        }

        public byte[][] DivideProblem(byte[] data, int threadCount)
        {

            return null;
        }

        public byte[] MergeSolution(byte[][] solutions)
        {

            return null;
        }
    }
}
