using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusTerminal_FinalsOOP
{
    class Program
    {
        // --- GLOBAL DATA LISTS ---
        static List<Route> allRoutes = new List<Route>();
        static List<Bus> allBuses = new List<Bus>();
        static List<Driver> allDrivers = new List<Driver>();
        static List<Trip> allTrips = new List<Trip>();

        // --- FILE MANAGERS ---
        static FileManager fmRoutes = new FileManager("routes.csv");
        static FileManager fmBuses = new FileManager("buses.csv");
        static FileManager fmDrivers = new FileManager("drivers.csv");
        static FileManager fmTrips = new FileManager("trips.csv");
        static FileManager fmSeats = new FileManager("seats.csv");

        static void Main(string[] args)
        {
            LoadDatabase(); // Load all CSVs into Lists

            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("=================================================");
                Console.WriteLine("   BUS TERMINAL SYSTEM (CSV CONNECTED)");
                Console.WriteLine("=================================================");
                Console.WriteLine(" [1] DASHBOARD : View Departures, Stops & Drivers");
                Console.WriteLine(" [2] POS       : Sell Ticket (Board/Drop-off)");
                Console.WriteLine(" [3] DISPATCH  : Assign Drivers to Buses");
                Console.WriteLine(" [4] EXIT");
                Console.WriteLine("-------------------------------------------------");
                Console.Write("Select an option: ");

                switch (Console.ReadLine())
                {
                    case "1": Tab_Dashboard(); break;
                    case "2": Tab_POS(); break;
                    case "3": Tab_Dispatch(); break;
                    case "4": running = false; break;
                }
            }
        }

        // ==========================================
        //  FEATURE 1: DASHBOARD
        //  Shows Stops, Times, and Assignments
        // ==========================================
        static void Tab_Dashboard()
        {
            Console.Clear();
            Console.WriteLine(">>> DEPARTURE DASHBOARD <<<");
            Console.WriteLine("{0,-6} {1,-8} {2,-15} {3,-25} {4,-15}", "Trip", "Time", "Route", "Stops", "Driver");
            Console.WriteLine(new string('-', 75));

            foreach (var t in allTrips)
            {
                // Format stops string (e.g., "Manila..Tarlac..Baguio")
                string stopDisplay = string.Join("..", t.Route.Stops);
                if (stopDisplay.Length > 22) stopDisplay = stopDisplay.Substring(0, 22) + "...";

                Console.WriteLine("{0,-6} {1,-8} {2,-15} {3,-25} {4,-15}",
                    t.TripId,
                    t.DepartureTime.ToString("HH:mm"),
                    t.Route.Name,
                    stopDisplay,
                    t.AssignedDriver.FullName);
            }
            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        // ==========================================
        //  FEATURE 2: POS (BOARDING/DROP-OFF)
        // ==========================================
        static void Tab_POS()
        {
            Console.Clear();
            Console.WriteLine(">>> POINT OF SALE <<<");
            Console.Write("Enter Trip ID: ");
            int tid;
            if (!int.TryParse(Console.ReadLine(), out tid)) return;

            var trip = allTrips.FirstOrDefault(t => t.TripId == tid);
            if (trip == null) { Console.WriteLine("Trip not found."); Console.ReadKey(); return; }

            // 1. SELECT ROUTE SEGMENT (Boarding / Drop Off)
            Console.WriteLine("\n--- Select Boarding/Exit Points ---");
            for (int i = 0; i < trip.Route.Stops.Count; i++)
            {
                Console.WriteLine($" [{i}] {trip.Route.Stops[i]}");
            }

            Console.Write("\nEnter Boarding Index (0-{0}): ", trip.Route.Stops.Count - 1);
            int startIdx = int.Parse(Console.ReadLine());

            Console.Write("Enter Drop-off Index ({0}-{1}): ", startIdx + 1, trip.Route.Stops.Count - 1);
            int endIdx = int.Parse(Console.ReadLine());

            if (endIdx <= startIdx || endIdx >= trip.Route.Stops.Count)
            {
                Console.WriteLine("Invalid route segment."); Console.ReadKey(); return;
            }

            // 2. SELECT SEAT
            Console.WriteLine($"\n--- Select Seat (Layout: {trip.AssignedBus.SeatingLayout}) ---");
            var available = trip.Seats.Where(s => !s.IsOccupied).Select(s => s.SeatNumber);
            Console.WriteLine("Available: " + string.Join(", ", available));

            Console.Write("Enter Seat Number: ");
            string seatNum = Console.ReadLine();

            var seat = trip.Seats.FirstOrDefault(s => s.SeatNumber == seatNum);
            if (seat != null && !seat.IsOccupied)
            {
                // 3. FINALIZE & SAVE
                seat.IsOccupied = true;
                SaveDatabase(); // <--- WRITES TO CSV

                Console.WriteLine("\n*********************************");
                Console.WriteLine(" TICKET PRINTED");
                Console.WriteLine($" Route: {trip.Route.Name}");
                Console.WriteLine($" From:  {trip.Route.Stops[startIdx]}");
                Console.WriteLine($" To:    {trip.Route.Stops[endIdx]}");
                Console.WriteLine($" Seat:  {seatNum}");
                Console.WriteLine($" Price: {trip.BasePrice}");
                Console.WriteLine("*********************************");
            }
            else
            {
                Console.WriteLine("Error: Seat taken or invalid.");
            }
            Console.ReadKey();
        }

        // ==========================================
        //  FEATURE 3: DISPATCH (ASSIGN DRIVER)
        // ==========================================
        static void Tab_Dispatch()
        {
            Console.Clear();
            Console.WriteLine(">>> DISPATCH MANAGER <<<");
            Console.Write("Enter Trip ID to modify: ");
            int tid = int.Parse(Console.ReadLine());
            var trip = allTrips.FirstOrDefault(t => t.TripId == tid);
            if (trip == null) return;

            Console.WriteLine($"Current Bus: {trip.AssignedBus.PlateNumber}");
            Console.WriteLine($"Current Driver: {trip.AssignedDriver.FullName}");
            Console.WriteLine("--------------------------------");

            // Show list of all drivers
            Console.WriteLine("Available Drivers List:");
            foreach (var d in allDrivers)
                Console.WriteLine($" ID: {d.DriverId} | Name: {d.FullName}");

            Console.Write("\nEnter New Driver ID: ");
            int newDriverId = int.Parse(Console.ReadLine());
            var newDriver = allDrivers.FirstOrDefault(d => d.DriverId == newDriverId);

            if (newDriver != null)
            {
                trip.AssignedDriver = newDriver;
                trip.AssignedDriverId = newDriver.DriverId; // Update ID for CSV

                SaveDatabase(); // <--- SAVES NEW DRIVER ASSIGNMENT TO FILE
                Console.WriteLine("Success: Driver Re-assigned.");
            }
            else
            {
                Console.WriteLine("Driver ID not found.");
            }
            Console.ReadKey();
        }

        // ==========================================
        //  DATABASE ENGINE (LOAD/SAVE)
        // ==========================================
        static void LoadDatabase()
        {
            // 1. LOAD ROUTES (Parses "Manila|Tarlac|Baguio")
            if (fmRoutes.getStatus())
            {
                foreach (string line in fmRoutes.getLines())
                {
                    var parts = line.Split(',');
                    if (parts.Length < 3) continue;
                    allRoutes.Add(new Route
                    {
                        RouteId = int.Parse(parts[0]),
                        Name = parts[1],
                        Stops = parts[2].Split('|').ToList() // Split stops by pipe character
                    });
                }
            }

            // 2. LOAD BUSES
            if (fmBuses.getStatus())
            {
                foreach (string line in fmBuses.getLines())
                {
                    var parts = line.Split(',');
                    allBuses.Add(new Bus
                    {
                        BusId = int.Parse(parts[0]),
                        PlateNumber = parts[1],
                        Capacity = int.Parse(parts[2]),
                        SeatingLayout = parts[3]
                    });
                }
            }

            // 3. LOAD DRIVERS
            if (fmDrivers.getStatus())
            {
                foreach (string line in fmDrivers.getLines())
                {
                    var parts = line.Split(',');
                    allDrivers.Add(new Driver
                    {
                        DriverId = int.Parse(parts[0]),
                        FullName = parts[1]
                    });
                }
            }

            // 4. LOAD TRIPS (AND LINK EVERYTHING)
            if (fmTrips.getStatus())
            {
                foreach (string line in fmTrips.getLines())
                {
                    var parts = line.Split(',');
                    var t = new Trip
                    {
                        TripId = int.Parse(parts[0]),
                        RouteId = int.Parse(parts[1]),
                        AssignedBusId = int.Parse(parts[2]),
                        AssignedDriverId = int.Parse(parts[3]),
                        DepartureTime = DateTime.Parse(parts[4]),
                        BasePrice = decimal.Parse(parts[5])
                    };

                    // LINKING: Find the actual object in the lists using IDs
                    t.Route = allRoutes.FirstOrDefault(r => r.RouteId == t.RouteId);
                    t.AssignedBus = allBuses.FirstOrDefault(b => b.BusId == t.AssignedBusId);
                    t.AssignedDriver = allDrivers.FirstOrDefault(d => d.DriverId == t.AssignedDriverId);

                    allTrips.Add(t);
                }
            }

            // 5. LOAD SEATS
            if (fmSeats.getStatus())
            {
                foreach (string line in fmSeats.getLines())
                {
                    var parts = line.Split(',');
                    int tripId = int.Parse(parts[1]);
                    var trip = allTrips.FirstOrDefault(t => t.TripId == tripId);

                    if (trip != null)
                    {
                        trip.Seats.Add(new Seat
                        {
                            SeatId = int.Parse(parts[0]),
                            SeatNumber = parts[2],
                            IsOccupied = bool.Parse(parts[3])
                        });
                    }
                }
            }
        }

        static void SaveDatabase()
        {
            // SAVING TRIPS (Updates Driver Assignments)
            List<string> tripLines = new List<string>();
            foreach (var t in allTrips)
            {
                tripLines.Add($"{t.TripId},{t.RouteId},{t.AssignedBusId},{t.AssignedDriverId},{t.DepartureTime},{t.BasePrice}");
            }
            fmTrips.Write(tripLines, false); // false = overwrite mode

            // SAVING SEATS (Updates Sold Tickets)
            List<string> seatLines = new List<string>();
            foreach (var t in allTrips)
            {
                foreach (var s in t.Seats)
                {
                    seatLines.Add($"{s.SeatId},{t.TripId},{s.SeatNumber},{s.IsOccupied}");
                }
            }
            fmSeats.Write(seatLines, false);
        }
    }

    // ==========================================
    //  YOUR FILE MANAGER CLASS
    // ==========================================
    

    // ==========================================
    //  DOMAIN MODELS
    // ==========================================
   

    

    

    

    
}