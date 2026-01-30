using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusTerminal_FinalsOOP
{
    class Program
    {
        // --- GLOBAL DATA ---
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
            LoadDatabase();

            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("=================================================");
                Console.WriteLine("   BUS TERMINAL SYSTEM (COMPLETE INTEGRATION)");
                Console.WriteLine("=================================================");
                Console.WriteLine(" [1] DASHBOARD     : View Departures & Stops");
                Console.WriteLine(" [2] POS           : Buy Ticket (RouteStop Logic)");
                Console.WriteLine(" [3] DISPATCH      : Assign Drivers");
                Console.WriteLine(" [4] MANAGE BUSES  : Create, Edit, Delete");
                Console.WriteLine(" [5] EXIT");
                Console.WriteLine("-------------------------------------------------");
                Console.Write("Select an option: ");

                switch (Console.ReadLine())
                {
                    case "1": Tab_Dashboard(); break;
                    case "2": Tab_POS(); break;
                    case "3": Tab_Dispatch(); break;
                    case "4": Tab_ManageBuses(); break;
                    case "5": running = false; break;
                }
            }
        }

        // ==========================================
        //  FEATURE 1: DASHBOARD
        // ==========================================
        static void Tab_Dashboard()
        {
            Console.Clear();
            Console.WriteLine(">>> DASHBOARD <<<");
            Console.WriteLine("{0,-6} {1,-8} {2,-15} {3,-10} {4,-25}", "ID", "TIME", "ROUTE", "BUS", "STOPS");
            Console.WriteLine(new string('-', 70));

            foreach (var t in allTrips)
            {
                if (t.AssignedBus != null && !t.AssignedBus.IsDeleted)
                {
                    // Use RouteStop objects to get names
                    var stopNames = t.Route.RouteStops.OrderBy(s => s.SequenceOrder).Select(s => s.Destination.Name);
                    string stops = string.Join("..", stopNames);

                    if (stops.Length > 25) stops = stops.Substring(0, 22) + "...";

                    Console.WriteLine("{0,-6} {1,-8} {2,-15} {3,-10} {4,-25}",
                        t.TripId, t.DepartureTime.ToString("HH:mm"), t.Route.Name, t.AssignedBus.PlateNumber, stops);
                }
            }
            Console.ReadKey();
        }

        // ==========================================
        //  FEATURE 2: POS (Using RouteStop Logic)
        // ==========================================
        static void Tab_POS()
        {
            Console.Clear();
            Console.WriteLine(">>> POS (POINT OF SALE) <<<");
            Console.WriteLine("--- AVAILABLE TRIPS ---");
            Console.WriteLine("{0,-6} {1,-8} {2,-15} {3,-10}", "ID", "TIME", "ROUTE", "BUS");

            foreach (var t in allTrips)
            {
                if (t.AssignedBus != null && !t.AssignedBus.IsDeleted)
                {
                    Console.WriteLine("{0,-6} {1,-8} {2,-15} {3,-10}", t.TripId, t.DepartureTime.ToString("HH:mm"), t.Route.Name, t.AssignedBus.PlateNumber);
                }
            }

            Console.Write("\nEnter Trip ID: ");
            if (!int.TryParse(Console.ReadLine(), out int tid)) return;

            var trip = allTrips.FirstOrDefault(t => t.TripId == tid);
            if (trip == null || trip.AssignedBus.IsDeleted) { Console.WriteLine("Trip unavailable."); Console.ReadKey(); return; }

            // Using RouteStop objects for selection
            Console.WriteLine("\n--- Select Boarding/Exit Points ---");
            var stops = trip.Route.RouteStops.OrderBy(s => s.SequenceOrder).ToList();

            for (int i = 0; i < stops.Count; i++)
            {
                string eta = trip.DepartureTime.Add(stops[i].ETAFromOrigin).ToString("HH:mm");
                Console.WriteLine($" [{i}] {stops[i].Destination.Name} (ETA: {eta})");
            }

            Console.Write("\nEnter Boarding Index: ");
            int startIdx = int.Parse(Console.ReadLine());
            Console.Write("Enter Drop-off Index: ");
            int endIdx = int.Parse(Console.ReadLine());

            if (endIdx <= startIdx || endIdx >= stops.Count)
            {
                Console.WriteLine("Invalid segment."); Console.ReadKey(); return;
            }

            Console.WriteLine("\nAvailable Seats: " + string.Join(", ", trip.Seats.Where(s => !s.IsOccupied).Select(s => s.SeatNumber)));
            Console.Write("Enter Seat: ");
            string sNum = Console.ReadLine();

            var seat = trip.Seats.FirstOrDefault(s => s.SeatNumber == sNum);
            if (seat != null && !seat.IsOccupied)
            {
                seat.IsOccupied = true;
                SaveDatabase();
                Console.WriteLine($"\nTicket Sold from {stops[startIdx].Destination.Name} to {stops[endIdx].Destination.Name}!");
            }
            Console.ReadKey();
        }

        // ==========================================
        //  FEATURE 3: DISPATCH
        // ==========================================
        static void Tab_Dispatch()
        {
            Console.Clear();
            Console.WriteLine(">>> DISPATCH <<<");
            Console.WriteLine("{0,-6} {1,-15} {2,-20}", "ID", "ROUTE", "CURRENT DRIVER");
            foreach (var t in allTrips)
                if (t.AssignedBus != null) Console.WriteLine("{0,-6} {1,-15} {2,-20}", t.TripId, t.Route.Name, t.AssignedDriver.FullName);

            Console.Write("\nEnter Trip ID: ");
            int tid = int.Parse(Console.ReadLine());
            var trip = allTrips.FirstOrDefault(t => t.TripId == tid);
            if (trip == null) return;

            Console.WriteLine("Available Drivers:");
            foreach (var d in allDrivers) Console.WriteLine($"{d.DriverId}: {d.FullName}");
            Console.Write("New Driver ID: ");
            int did = int.Parse(Console.ReadLine());

            var driver = allDrivers.FirstOrDefault(d => d.DriverId == did);
            if (driver != null) { trip.AssignedDriver = driver; trip.AssignedDriverId = did; SaveDatabase(); Console.WriteLine("Saved."); }
            Console.ReadKey();
        }

        // ==========================================
        //  FEATURE 4: MANAGE BUSES (FULL CRUD RESTORED)
        // ==========================================
        static void Tab_ManageBuses()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(">>> MANAGE BUSES (CRUD) <<<");
                Console.WriteLine("1. LIST (Read) - View all active buses");
                Console.WriteLine("2. ADD  (Create) - Register new bus");
                Console.WriteLine("3. EDIT (Update) - Modify details");
                Console.WriteLine("4. DELETE (Soft) - Remove bus");
                Console.WriteLine("5. BACK");
                Console.Write("Select: ");

                string choice = Console.ReadLine();
                if (choice == "5") break;

                switch (choice)
                {
                    case "1": // READ
                        Console.Clear();
                        Console.WriteLine("--- ACTIVE BUSES LIST ---");
                        Console.WriteLine("{0,-6} {1,-12} {2,-5} {3,-10}", "ID", "PLATE", "CAP", "LAYOUT");
                        Console.WriteLine(new string('-', 40));
                        foreach (var b in allBuses.Where(b => !b.IsDeleted))
                        {
                            Console.WriteLine("{0,-6} {1,-12} {2,-5} {3,-10}", b.BusId, b.PlateNumber, b.Capacity, b.SeatingLayout);
                        }
                        Console.WriteLine("\nPress any key...");
                        Console.ReadKey();
                        break;

                    case "2": // CREATE
                        Console.Clear();
                        Console.WriteLine("\n--- Add New Bus ---");
                        int newId = allBuses.Any() ? allBuses.Max(b => b.BusId) + 1 : 100;

                        Console.Write("Enter Plate Number: ");
                        string plate = Console.ReadLine();
                        Console.Write("Enter Capacity: ");
                        int cap = int.Parse(Console.ReadLine());
                        Console.Write("Enter Layout (e.g., 2x2): ");
                        string layout = Console.ReadLine();

                        allBuses.Add(new Bus
                        {
                            BusId = newId,
                            PlateNumber = plate,
                            Capacity = cap,
                            SeatingLayout = layout,
                            IsOperational = true,
                            IsDeleted = false
                        });
                        SaveDatabase();
                        Console.WriteLine("Bus Added!");
                        Console.ReadKey();
                        break;

                    case "3": // UPDATE
                        Console.Clear();
                        Console.WriteLine("--- EDIT BUS ---");
                        Console.WriteLine("{0,-6} {1,-12} {2,-5} {3,-10}", "ID", "PLATE", "CAP", "LAYOUT");
                        foreach (var b in allBuses.Where(b => !b.IsDeleted))
                        {
                            Console.WriteLine("{0,-6} {1,-12} {2,-5} {3,-10}", b.BusId, b.PlateNumber, b.Capacity, b.SeatingLayout);
                        }

                        Console.Write("\nEnter Bus ID to Edit: ");
                        int editId = int.Parse(Console.ReadLine());
                        var busToEdit = allBuses.FirstOrDefault(b => b.BusId == editId && !b.IsDeleted);

                        if (busToEdit != null)
                        {
                            Console.WriteLine($"Editing Bus: {busToEdit.PlateNumber}");
                            Console.Write($"New Plate (Enter to keep '{busToEdit.PlateNumber}'): ");
                            string pInput = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(pInput)) busToEdit.PlateNumber = pInput;

                            Console.Write($"New Layout (Enter to keep '{busToEdit.SeatingLayout}'): ");
                            string lInput = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(lInput)) busToEdit.SeatingLayout = lInput;

                            SaveDatabase();
                            Console.WriteLine("Bus Updated.");
                        }
                        else Console.WriteLine("Bus not found.");
                        Console.ReadKey();
                        break;

                    case "4": // SOFT DELETE
                        Console.Clear();
                        Console.WriteLine("--- DELETE BUS ---");
                        Console.WriteLine("{0,-6} {1,-12} {2,-5} {3,-10}", "ID", "PLATE", "CAP", "LAYOUT");
                        foreach (var b in allBuses.Where(b => !b.IsDeleted))
                        {
                            Console.WriteLine("{0,-6} {1,-12} {2,-5} {3,-10}", b.BusId, b.PlateNumber, b.Capacity, b.SeatingLayout);
                        }

                        Console.Write("\nEnter Bus ID to Delete: ");
                        int delId = int.Parse(Console.ReadLine());
                        var busDel = allBuses.FirstOrDefault(b => b.BusId == delId);

                        if (busDel != null)
                        {
                            busDel.IsDeleted = true;
                            SaveDatabase();
                            Console.WriteLine($"Bus {busDel.PlateNumber} has been soft deleted.");
                        }
                        else Console.WriteLine("Bus not found.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // ==========================================
        //  DATABASE ENGINE (RouteStop Parsed Here)
        // ==========================================
        static void LoadDatabase()
        {
            // 1. LOAD ROUTES
            if (fmRoutes.getStatus())
            {
                foreach (string line in fmRoutes.getLines())
                {
                    var p = line.Split(',');
                    if (p.Length >= 3)
                    {
                        Route r = new Route { RouteId = int.Parse(p[0]), Name = p[1] };

                        string[] stopNames = p[2].Split('|');
                        for (int i = 0; i < stopNames.Length; i++)
                        {
                            Destination dest = new Destination { DestinationId = i, Name = stopNames[i] };

                            // Using your RouteStop Class
                            RouteStop stop = new RouteStop
                            {
                                DestinationId = dest.DestinationId,
                                Destination = dest,
                                SequenceOrder = i + 1,
                                ETAFromOrigin = TimeSpan.FromHours(i) // Auto-gen ETA
                            };
                            r.RouteStops.Add(stop);
                        }
                        allRoutes.Add(r);
                    }
                }
            }

            // 2. LOAD BUSES
            if (fmBuses.getStatus())
            {
                foreach (string line in fmBuses.getLines())
                {
                    var p = line.Split(',');
                    bool isDel = p.Length > 5 ? bool.Parse(p[5]) : false;
                    allBuses.Add(new Bus { BusId = int.Parse(p[0]), PlateNumber = p[1], Capacity = int.Parse(p[2]), SeatingLayout = p[3], IsOperational = bool.Parse(p[4]), IsDeleted = isDel });
                }
            }

            // 3. LOAD DRIVERS
            if (fmDrivers.getStatus())
            {
                foreach (string line in fmDrivers.getLines())
                {
                    var p = line.Split(',');
                    allDrivers.Add(new Driver { DriverId = int.Parse(p[0]), FullName = p[1] });
                }
            }

            // 4. LOAD TRIPS
            if (fmTrips.getStatus())
            {
                foreach (string line in fmTrips.getLines())
                {
                    var p = line.Split(',');
                    var t = new Trip
                    {
                        TripId = int.Parse(p[0]),
                        RouteId = int.Parse(p[1]),
                        AssignedBusId = int.Parse(p[2]),
                        AssignedDriverId = int.Parse(p[3]),
                        DepartureTime = DateTime.Parse(p[4]),
                        BasePrice = decimal.Parse(p[5])
                    };
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
                    var p = line.Split(',');
                    var t = allTrips.FirstOrDefault(x => x.TripId == int.Parse(p[1]));
                    if (t != null) t.Seats.Add(new Seat { SeatId = int.Parse(p[0]), SeatNumber = p[2], IsOccupied = bool.Parse(p[3]) });
                }
            }
        }

        static void SaveDatabase()
        {
            List<string> busLines = new List<string>();
            foreach (var b in allBuses) busLines.Add($"{b.BusId},{b.PlateNumber},{b.Capacity},{b.SeatingLayout},{b.IsOperational},{b.IsDeleted}");
            fmBuses.Write(busLines, false);

            List<string> tripLines = new List<string>();
            foreach (var t in allTrips) tripLines.Add($"{t.TripId},{t.RouteId},{t.AssignedBusId},{t.AssignedDriverId},{t.DepartureTime},{t.BasePrice}");
            fmTrips.Write(tripLines, false);

            List<string> seatLines = new List<string>();
            foreach (var t in allTrips) foreach (var s in t.Seats) seatLines.Add($"{s.SeatId},{t.TripId},{s.SeatNumber},{s.IsOccupied}");
            fmSeats.Write(seatLines, false);
        }
    }

}