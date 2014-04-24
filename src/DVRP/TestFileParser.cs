﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVRP
{
    public enum EdgeWeightTypeEnum { FULL_MATRIX, LOWER_TRIANG,ADJ }
    public enum EdgeWeightFormatEnum { EUC_2D, MAN_2D, MAX_2D, EXPLICIT }
    public enum ObjectiveEnum {VEH_WEIGHT,WEIGHT,MIN_MAX_LEN}
    public enum OrderEnum {GT,GE,LT,LE,EQ}
    public enum FileSection { DEMAND, TIME_WINDOW, DURATION, LOCATION_COORD, DEPOT_LOCATION, VISIT_LOCATION, EDGE_WEIGHT, OTHER }

    class TestFileParser
    {
        
        private FileSection CurrSect = FileSection.OTHER;
       // List<double> temporary_double_val = new List<double>();

        //reprezentacja sekcji pliku testowego
        //w nawiasach kwadratowych (komentarze) sa wartosci domyslne

        //NUM_DEPOTS:
        public int NumDepots { get; private set;}

        //NUM_CAPACITIES: [1]
        public int NumCapacities { get; private set; }

        //NUM_VISITS:
        public int NumVisits {  get; private set; }

        //NUM_VEHICLES:
        public int NumVehicles {  get; private set; }

        //NUM_LOCATIONS:
        public int NumLocations {  get; private set; }

        //CAPACITIES:
        public double Capacities {  get; private set; }

        //SPEED: [1.0]
        public float Speed {  get; private set; }

        //MAX_TIME: [inf]
        public double MaxTime {  get; private set; }

        //EDGE_WEIGHT_TYPE:
        public EdgeWeightTypeEnum EdgeWeightType {  get; private set; }

        //EDGE_WEIGHT_FORMAT:
        public EdgeWeightFormatEnum EdgeWeightFormat {  get; private set; }

        //OBJECTIVE: [WEIGHT]
        public ObjectiveEnum Objective {  get; private set; }

        
        //DEPOTS 
        public int[] Depots {  get; private set; }

        //DEMAND_SECTION
        public double[] VisitQuantity {  get; private set; }


        //LOCATION_COORD_SECTION
        public int[] LocationIds {  get; private set; }
        public System.Windows.Point[] LocationsCoords {  get; private set; }

        //VISIT_LOCATION_SECTION
        public int[] VisitLocations {  get; private set; }

        //DEPOT_LOCATION_SECTION
        public double[][] DepotsLocation {  get; private set; }

        //EDGE_WEIGHT_SECTION
        public double[,] EdgeWeights {  get; private set; }

        //VEH_COMPAT_SECTION
        public Tuple<int, int, bool>[] VehCompat {  get; private set; }

        //UWAGA: to parametr string został zmieniony na ENUM z porownaniu do order z DVRP.cs
        //ORDER_SECTION
        public Tuple<int, OrderEnum, int, double>[] VehOrder {  get; private set; }

        //VEHICLE_CAPACITY_SECTION 
        public double[] VehicleCapacity {  get; private set; }

        //VEHICLE_SPEED_SECTION 
        public double[] VehicleSpeed {  get; private set; }

        //VEHICLE_COST_SECTION 
        public double[] VehicleDistanceCost {  get; private set; }
        public double[] VehicleUseCost {  get; private set; }
        public double[] VehicleTimeCost {  get; private set; }

        //TIME_WINDOW_SECTION 
        public Tuple<double, double>[] TimeWindow {  get; private set; }

        //DEPOT_TIME_WINDOW_SECTION
        public Tuple<double, double>[] DepotsTimeWindow {  get; private set; }

        //VEH_TIME_WINDOW_SECTION

        //DURATION_SECTION
        public double[] VisitsDuration {  get; private set; }

        //TIME_AVAIL_SECTION 
        public double[] TimeAvail {  get; private set; }

        //DURATION_BY_VEH_SECTION 
        public double[][] DurationByVeh {  get; private set; }
        
        //VISIT_COMPAT_SECTION 

        //DEPOT_COMPAT_SECTION 
        //?

        //VEH_DEPOT_SECTION 
        //?

        //OPTIONAL_VISIT_SECTION 
        public Tuple<int, double>[] OptionalVisits {  get; private set; }

        //VISIT_AVAIL_SECTION 
        public double[] VisitAvailTime {  get; private set; }

        /// <summary>
        /// Sets default values
        /// </summary>
        public TestFileParser()
        {
           this.NumDepots = 1;
           this.NumCapacities = 1;


        }
        /// <summary>
        /// Sets values parsed from txt file
        /// </summary>
        /// <param name="path"></param>
        public TestFileParser(string path)
        {
            int line_count = 0;
            StreamReader reader = System.IO.File.OpenText(path);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
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
                    case "MAX_TIME:":
                        CurrSect = FileSection.OTHER;
                        this.MaxTime = Int32.Parse(items[1]);
                        break;
                    case "CAPACITIES:":
                        CurrSect = FileSection.OTHER;
                        this.Capacities = Double.Parse(items[1]);
                        break;
                    case "DEPOTS":
                        CurrSect = FileSection.OTHER;
                        line = reader.ReadLine();
                        items = line.Split(' ');
                        this.Depots = new int[items.Count()];
                        int i = 0;
                        foreach(string s in items)
                        {
                            this.Depots[i++] = Int32.Parse(s);
                        }
                        break;
                    case "DEMAND_SECTION":
                        CurrSect = FileSection.DEMAND;
                        this.VisitQuantity = new double[this.NumVisits];
                        break;
                    case "TIME_WINDOW_SECTION":
                        CurrSect = FileSection.TIME_WINDOW;
                        this.TimeWindow = new Tuple<double, double>[this.NumVisits];                    
                        break;
                    case "DURATION_SECTION":
                        CurrSect = FileSection.DURATION;
                        this.VisitsDuration = new double[this.NumVisits];
                        break;
                    case "LOCATION_COORD_SECTION":
                        CurrSect = FileSection.LOCATION_COORD;
                        line_count = 0;
                        this.LocationIds = new int[this.NumLocations];
                        this.LocationsCoords = new System.Windows.Point[this.NumLocations];                        
                        break;
                    case "DEPOT_LOCATION_SECTION":
                        CurrSect = FileSection.DEPOT_LOCATION;
                        line_count = 0;
                        this.DepotsLocation = new double[this.Depots.Count()][];                       
                        break;
                    case "VISIT_LOCATION_SECTION":
                        CurrSect = FileSection.VISIT_LOCATION;
                        this.VisitLocations = new int[this.NumVisits];
                        break;
                    case "EDGE_WEIGHT_SECTION":
                        CurrSect = FileSection.EDGE_WEIGHT;
                        EdgeWeights = new double[this.NumLocations,this.NumLocations];
                        line_count = 0;
                        break;

                    //TODO: uwzglednic pozostale pola ktore nie sa uwzglednione w probnym pliku

                    default: //numbers in line
                        {
                            switch (CurrSect)
                            {
                                case FileSection.DEMAND:
                                    VisitQuantity[Int32.Parse(items[0])-1] = Int32.Parse(items[1]);
                                    break;
                                case FileSection.TIME_WINDOW:
                                    TimeWindow[Int32.Parse(items[0])-1]= new Tuple<double,double>(Double.Parse(items[1]),Double.Parse(items[2]));                         
                                    break;
                                case FileSection.DURATION:
                                    VisitsDuration[Int32.Parse(items[0]) - 1] = Double.Parse(items[1]);
                                    break;
                                case FileSection.LOCATION_COORD:
                                    LocationIds[line_count] = Int32.Parse(items[0]);
                                    LocationsCoords[line_count] = new System.Windows.Point(Int32.Parse(items[1]),Int32.Parse(items[2]));
                                    line_count++;
                                    break;
                                case FileSection.DEPOT_LOCATION:
                                    DepotsLocation[line_count][0] = Double.Parse(items[0]);
                                    DepotsLocation[line_count][1] = Double.Parse(items[1]);
                                    line_count++;
                                    break;
                                case FileSection.VISIT_LOCATION:
                                    VisitLocations[Int32.Parse(items[0]) - 1] = Int32.Parse(items[1]);
                                    break;
                                case FileSection.EDGE_WEIGHT:
                                    items = line.Split(' ');
                                    this.Depots = new int[items.Count()];
                                 
                                    int k = 0;
                                    foreach(string s in items)
                                    {
                                        EdgeWeights[line_count,k++]= Int32.Parse(s);
                                    }
                                    line_count++;
                                     break;

                            }
                        }
                        break;


                }
            }

        }
    }
}