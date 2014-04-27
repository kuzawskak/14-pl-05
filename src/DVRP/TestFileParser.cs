using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace DVRP
{
    public enum EdgeWeightTypeEnum { EUC_2D, MAN_2D, MAX_2D, EXPLICIT }
    public enum EdgeWeightFormatEnum { FULL_MATRIX, LOWER_TRIANG, ADJ }
    public enum ObjectiveEnum { VEH_WEIGHT, WEIGHT, MIN_MAX_LEN }
    public enum OrderEnum { GT, GE, LT, LE, EQ }
    public enum FileSection { IGNORE,DEMAND, TIME_WINDOW, DURATION, LOCATION_COORD, DEPOT_LOCATION, VISIT_LOCATION, EDGE_WEIGHT, OTHER,DEPOT_TIME_WINDOW,TIME_AVAIL }

    class TestFileParser
    {

        private FileSection CurrSect = FileSection.OTHER;
        // List<double> temporary_double_val = new List<double>();

        //reprezentacja sekcji pliku testowego
        //w nawiasach kwadratowych (komentarze) sa wartosci domyslne

        //NUM_DEPOTS:
        public int NumDepots { get; private set; }

        //NUM_CAPACITIES: [1]
        public int NumCapacities { get; private set; } // teoretycznie można pominąć, ale w plikach jest

        //NUM_VISITS:
        public int NumVisits { get; private set; }

        //NUM_VEHICLES:
        public int NumVehicles { get; private set; }

        //NUM_LOCATIONS:
        public int NumLocations { get; private set; }

        //CAPACITIES:
        public double Capacities { get; private set; }

        //SPEED: [1.0]
        public double Speed { get; private set; } // w plikach nigdzie nie ma, ale chyba trzeba parsować

        //MAX_TIME: [inf]
        //public double MaxTime { get; private set; }

        //EDGE_WEIGHT_TYPE:
        public EdgeWeightTypeEnum EdgeWeightType { get; private set; } // chyba będzie zawsze 1, ale niech zostanie na wszelki wypadek

        //EDGE_WEIGHT_FORMAT:
        public EdgeWeightFormatEnum EdgeWeightFormat { get; private set; } // j.w.

        //OBJECTIVE: [WEIGHT]
        public ObjectiveEnum Objective { get; private set; } // chyba nie będzie, ale niech zostanie ;)


        //DEPOTS 
        public int[] Depots { get; private set; }

        //DEMAND_SECTION
        public int[] Visits { get; private set; }
        public Dictionary<int, double> VisitQuantity { get; private set; }


        //LOCATION_COORD_SECTION
        public int[] LocationIds { get; private set; }
        public Dictionary<int, System.Windows.Point> LocationsCoords { get; private set; }

        //VISIT_LOCATION_SECTION
        public Dictionary<int, int> VisitLocations { get; private set; }

        //DEPOT_LOCATION_SECTION
        public Dictionary<int,int> DepotsLocation { get; private set; }

        //EDGE_WEIGHT_SECTION
        public double[,] EdgeWeights { get; private set; } 

        //VEH_COMPAT_SECTION
        //public Tuple<int, int, bool>[] VehCompat { get; private set; }

        //UWAGA: to parametr string został zmieniony na ENUM z porownaniu do order z DVRP.cs
        //ORDER_SECTION
       // public Tuple<int, OrderEnum, int, double>[] VehOrder { get; private set; }

        //VEHICLE_CAPACITY_SECTION 
        //public double[] VehicleCapacity { get; private set; }

        //VEHICLE_SPEED_SECTION 
        //public double[] VehicleSpeed { get; private set; }

        //VEHICLE_COST_SECTION 
        //public double[] VehicleDistanceCost { get; private set; }
        //public double[] VehicleUseCost { get; private set; }
        //public double[] VehicleTimeCost { get; private set; }

        //TIME_WINDOW_SECTION 
        //public Tuple<double, double>[] TimeWindow { get; private set; }

        //DEPOT_TIME_WINDOW_SECTION
        public Dictionary<int, Tuple<double, double>> DepotsTimeWindow { get; private set; } // to trzeba parsować

        //VEH_TIME_WINDOW_SECTION

        //DURATION_SECTION
        public Dictionary<int, double> VisitsDuration { get; private set; }

        //TIME_AVAIL_SECTION 
        public Dictionary<int, double> TimeAvail { get; private set; } // to trzeba parsować

        //DURATION_BY_VEH_SECTION 
        //public double[][] DurationByVeh { get; private set; }

        //VISIT_COMPAT_SECTION 

        //DEPOT_COMPAT_SECTION 
        //?

        //VEH_DEPOT_SECTION 
        //?

        //OPTIONAL_VISIT_SECTION 
        //public Tuple<int, double>[] OptionalVisits { get; private set; }

        //VISIT_AVAIL_SECTION 
        //public double[] VisitAvailTime { get; private set; }

        /// <summary>
        /// Sets default values
        /// </summary>
        public TestFileParser()
        {
            this.NumDepots = 1;
            this.NumCapacities = 1;
            this.Speed = 1;
        }
        /// <summary>
        /// Sets values parsed from txt file
        /// </summary>
        /// <param name="path"></param>
        public TestFileParser(Stream stream) : this()
        {
            int line_count = 0;
            StreamReader reader = new StreamReader(stream);//System.IO.File.OpenText(path);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                string[] items = line.Split(' ');
                switch (items[0])
                {
                    case "NUM_DEPOTS:":
                        CurrSect = FileSection.OTHER;
                        this.NumDepots = Int32.Parse(items[1]);
                        break;
                    case "NUM_CAPACITIES:":
                        CurrSect = FileSection.OTHER;
                        this.NumCapacities = Int32.Parse(items[1]);
                        break;
                    case "SPEED:":
                        CurrSect = FileSection.OTHER;
                        this.Speed = Double.Parse(items[1],System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);
                        break;
                    case "NUM_VISITS:":
                        CurrSect = FileSection.OTHER;
                        this.NumVisits = Int32.Parse(items[1]);
                        break;
                    case "NUM_VEHICLES:":
                        CurrSect = FileSection.OTHER;
                        this.NumVehicles = Int32.Parse(items[1]);
                        break;
                    case "NUM_LOCATIONS:":
                        CurrSect = FileSection.OTHER;
                        this.NumLocations = Int32.Parse(items[1]);
                        break;
                    case "EDGE_WEIGHT_TYPE:":
                        CurrSect = FileSection.OTHER;
                        this.EdgeWeightType = (EdgeWeightTypeEnum)Enum.Parse(typeof(EdgeWeightTypeEnum), items[1]);
                        break;
                    case "EDGE_WEIGHT_FORMAT":
                        CurrSect = FileSection.OTHER;
                        this.EdgeWeightFormat = (EdgeWeightFormatEnum)Enum.Parse(typeof(EdgeWeightFormatEnum), items[1]);
                        break;
                    //case "MAX_TIME:":
                    //    CurrSect = FileSection.OTHER;
                    //    this.MaxTime = Int32.Parse(items[1]);
                    //    break;
                    case "CAPACITIES:":
                        CurrSect = FileSection.OTHER;
                        this.Capacities = Double.Parse(items[1]);
                        break;
                    case "DEPOTS":
                        CurrSect = FileSection.OTHER;
                        line = reader.ReadLine().Trim();
                        items = line.Split(' ');
                        this.Depots = new int[items.Count()];
                        int i = 0;
                        foreach (string s in items)
                        {
                            this.Depots[i++] = Int32.Parse(s);
                        }
                        break;
                    case "DEMAND_SECTION":
                        CurrSect = FileSection.DEMAND;
                        this.VisitQuantity = new Dictionary<int, double>(NumVisits);
                        this.Visits = new int[NumVisits];
                        line_count = 0;
                        break;
                    case "TIME_WINDOW_SECTION":
                        CurrSect = FileSection.IGNORE;
                        //this.TimeWindow = new Tuple<double, double>[this.NumVisits];
                        break;
                    case "DURATION_SECTION":
                        CurrSect = FileSection.DURATION;
                        line_count = 0;
                        this.VisitsDuration = new Dictionary<int, double>(NumVisits);
                        break;
                    case "LOCATION_COORD_SECTION":
                        CurrSect = FileSection.LOCATION_COORD;
                        line_count = 0;
                        this.LocationIds = new int[this.NumLocations];
                        this.LocationsCoords = new Dictionary<int, System.Windows.Point>(NumLocations);
                        break;
                    case "DEPOT_LOCATION_SECTION":
                        CurrSect = FileSection.DEPOT_LOCATION;
                        line_count = 0;
                        this.DepotsLocation = new Dictionary<int, int>(NumDepots);
                        break;
                    case "VISIT_LOCATION_SECTION":
                        CurrSect = FileSection.VISIT_LOCATION;
                        this.VisitLocations = new Dictionary<int, int>(NumVisits);
                        break;
                    case "EDGE_WEIGHT_SECTION":
                        CurrSect = FileSection.EDGE_WEIGHT;
                        EdgeWeights = new double[this.NumLocations, this.NumLocations];
                        line_count = 0;
                        break;
                    case "DEPOT_TIME_WINDOW_SECTION":
                        CurrSect = FileSection.DEPOT_TIME_WINDOW;
                        this.DepotsTimeWindow = new Dictionary<int, Tuple<double, double>>(NumDepots);
                        break;
                    case "TIME_AVAIL_SECTION":
                        CurrSect = FileSection.TIME_AVAIL;
                        this.TimeAvail = new Dictionary<int, double>(NumVisits);
                        break;
                    case "EOF":
                        CurrSect = FileSection.OTHER;
                        break;
                    case "COMMENT:":
                        break;
                    //TODO: uwzglednic pozostale pola ktore nie sa uwzglednione w probnym pliku

                    default: //numbers in line
                        {
                            switch (CurrSect)
                            {
                                case FileSection.DEMAND:
                                    VisitQuantity.Add(Int32.Parse(items[0]), Int32.Parse(items[1]));
                                    Visits[line_count] = Int32.Parse(items[0]);
                                    line_count++;
                                    break;
                                //case FileSection.TIME_WINDOW:
                                //    TimeWindow[Int32.Parse(items[0]) - 1] = new Tuple<double, double>(Double.Parse(items[1], System.Globalization.NumberStyles.AllowDecimalPoint), Double.Parse(items[2], System.Globalization.NumberStyles.AllowDecimalPoint));
                                //    break;
                                case FileSection.DURATION:
                                    VisitsDuration.Add(Int32.Parse(items[0]), Double.Parse(items[1], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo));
                                    break;
                                case FileSection.LOCATION_COORD:
                                    LocationIds[line_count] = Int32.Parse(items[0]);
                                    LocationsCoords.Add(Int32.Parse(items[0]), new System.Windows.Point(Int32.Parse(items[1]), Int32.Parse(items[2])));
                                    line_count++;
                                    break;
                                case FileSection.DEPOT_LOCATION:
                                    DepotsLocation.Add(Int32.Parse(items[0]), Int32.Parse(items[1]));
                                    break;
                                case FileSection.VISIT_LOCATION:
                                    VisitLocations.Add(Int32.Parse(items[0]), Int32.Parse(items[1]));
                                    break;
                                case FileSection.EDGE_WEIGHT:
                                    items = line.Split(' ');


                                    int k = 0;
                                    foreach (string s in items)
                                    {
                                        if (s != "")
                                            EdgeWeights[line_count, k++] = Int32.Parse(s);
                                    }
                                    line_count++;
                                    break;
                                case FileSection.DEPOT_TIME_WINDOW:
                                    DepotsTimeWindow.Add(Int32.Parse(items[0]), new Tuple<double, double>(Double.Parse(items[1], NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo),Double.Parse(items[2], NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo)));
                                    break;
                                case FileSection.TIME_AVAIL:
                                    TimeAvail.Add(Int32.Parse(items[0]), Double.Parse(items[1], NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo));
                                    break;

                            }
                        }
                        break;


                }
            }
            //check
        }
    }
}
