using System;
using System.Collections.Generic;
using System.Linq;

namespace BusTerminal_FinalsOOP
{
    class Program
    {
        static List<Trip> allTrips = new List<Trip>();
        static List<Bus> allBuses = new List<Bus>();
        static List<Driver> allDrivers = new List<Driver>();
        static Terminal manager;

        static void Main(string[] args)
        {
            SetupData(); // Initialize Logic and Data
            manager = new Terminal(allTrips, allBuses);

            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("======================================================");
                Console.WriteLine("      BUS TERMINAL MANAGEMENT SYSTEM (FINALIZED)");
                Console.WriteLine("======================================================");
                Console.WriteLine(" [1] DASHBOARD TAB   : View Departures & Schedules");
                Console.WriteLine(" [2] POS TAB         : Sell Tickets (Select Board/Exit)");
                Console.WriteLine(" [3] DISPATCH TAB    : Assign Drivers & Manage Buses");
                Console.WriteLine(" [4] ADMIN TAB       : Conflict Detection & Status");
                Console.WriteLine(" [5] EXIT");
                Console.WriteLine("------------------------------------------------------");
                Console.Write("Select a Tab: ");

                switch (Console.ReadLine())
                {
                    case "1": Tab_Dashboard(); break;
                    case "2": Tab_POS(); break;
                    case "3": Tab_Dispatch(); break;
                    case "4": Tab_Admin(); break;
                    case "5": running = false; break;
                    default: Console.WriteLine("Invalid selection."); break;
                }
            }
        }

        // ==========================================
        // TAB 1: DASHBOARD
        // ==========================================
        static void Tab_Dashboard()
        {
            Console.Clear();
            Console.WriteLine(">>> DASHBOARD: UPCOMING DEPARTURES <<<");
            Console.WriteLine("");
            // Formatting headers
            Console.WriteLine("{0,-6} {1,-10} {2,-18} {3,-25} {4,-15} {5,-10}",
                "TripID", "Time", "Route", "Stops", "Driver", "Bus Plate");
            Console.WriteLine(new string('-', 95));

            foreach (var trip in allTrips.OrderBy(t => t.DepartureTime))
            {
                // Format stops for display
                string stops = string.Join(", ", trip.Route.IntermediateStops.Select(s => s.Name));
                if (string.IsNullOrEmpty(stops)) stops = "(Direct)";

                Console.WriteLine("{0,-6} {1,-10} {2,-18} {3,-25} {4,-15} {5,-10}",
                    trip.TripId,
                    trip.DepartureTime.ToString("HH:mm"),
                    trip.Route.RouteName,
                    stops.Length > 22 ? stops.Substring(0, 22) + "..." : stops,
                    trip.AssignedDriver.FullName,
                    trip.AssignedBus.PlateNumber);
            }
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        // ==========================================
        // TAB 2: POINT OF SALE (POS)
        // ==========================================
        static void Tab_POS()
        {
            Console.Clear();
            Console.WriteLine(">>> POS: TICKET SALES <<<");

            // 1. Select Trip
            Console.Write("Enter Trip ID: ");
            if (!int.TryParse(Console.ReadLine(), out int tripId)) return;

            var trip = allTrips.FirstOrDefault(t => t.TripId == tripId);
            if (trip == null) { Console.WriteLine("Trip not found."); Console.ReadKey(); return; }

            // 2. Select Boarding and Drop-off
            Console.WriteLine("\n--- Select Boarding & Exit Points ---");

            // Build list of all possible points in order
            var allPoints = new List<Destination> { trip.Route.Origin };
            allPoints.AddRange(trip.Route.IntermediateStops);
            allPoints.AddRange(new List<Destination> { trip.Route.Destination });

            for (int i = 0; i < allPoints.Count; i++)
            {
                Console.WriteLine($" [{i}] {allPoints[i].Name}");
            }

            Console.Write("\nSelect Boarding Point ID (0-{0}): ", allPoints.Count - 1);
            int startIdx = int.Parse(Console.ReadLine());

            Console.Write("Select Drop-off Point ID ({0}-{1}): ", startIdx + 1, allPoints.Count - 1);
            int endIdx = int.Parse(Console.ReadLine());

            // Validate Route logic
            if (endIdx <= startIdx || startIdx < 0 || endIdx >= allPoints.Count)
            {
                Console.WriteLine("Invalid route segment selection.");
                Console.ReadKey();
                return;
            }

            string boardingLoc = allPoints[startIdx].Name;
            string dropOffLoc = allPoints[endIdx].Name;

            // 3. Select Seat
            Console.WriteLine($"\n--- Seat Selection (Layout: {trip.AssignedBus.SeatingLayout}) ---");
            var availableSeats = trip.Seats.Where(s => !s.IsOccupied).Select(s => s.SeatNumber);
            Console.WriteLine("Available: " + string.Join(", ", availableSeats));

            Console.Write("Enter Seat Number: ");
            string seatNum = Console.ReadLine();

            // 4. Finalize
            if (manager.ValidateSeatSale(tripId, seatNum))
            {
                var seat = trip.Seats.First(s => s.SeatNumber == seatNum);
                seat.IsOccupied = true;

                // Print Ticket
                Console.Clear();
                Console.WriteLine("****************************************");
                Console.WriteLine("             OFFICIAL TICKET            ");
                Console.WriteLine("****************************************");
                Console.WriteLine($"Ticket #:     {new Random().Next(10000, 99999)}");
                Console.WriteLine($"Bus Route:    {trip.Route.RouteName}");
                Console.WriteLine($"Bus Plate:    {trip.AssignedBus.PlateNumber}");
                Console.WriteLine($"Driver:       {trip.AssignedDriver.FullName}");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"FROM:         {boardingLoc}");
                Console.WriteLine($"TO:           {dropOffLoc}");
                Console.WriteLine($"DEPARTURE:    {trip.DepartureTime}");
                Console.WriteLine($"SEAT:         {seatNum}");
                Console.WriteLine($"PRICE:        {trip.BasePrice:C}"); // Simplified price
                Console.WriteLine("****************************************");
                Console.WriteLine("        Thank you for traveling!        ");
                Console.WriteLine("****************************************");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        // ==========================================
        // TAB 3: DISPATCH (ASSIGN DRIVERS)
        // ==========================================
        static void Tab_Dispatch()
        {
            Console.Clear();
            Console.WriteLine(">>> DISPATCH: ASSIGN DRIVERS <<<");

            // List Trips and Drivers
            foreach (var t in allTrips)
            {
                Console.WriteLine($"Trip [{t.TripId}]: Bus {t.AssignedBus.PlateNumber} is driven by [{t.AssignedDriver.FullName}]");
            }
            Console.WriteLine(new string('-', 40));

            Console.Write("Enter Trip ID to Re-assign: ");
            if (!int.TryParse(Console.ReadLine(), out int tripId)) return;

            var trip = allTrips.FirstOrDefault(t => t.TripId == tripId);
            if (trip == null) return;

            Console.WriteLine("\nAvailable Drivers:");
            foreach (var d in allDrivers)
            {
                Console.WriteLine($"ID: {d.DriverId} | Name: {d.FullName}");
            }

            Console.Write("\nEnter New Driver ID: ");
            int driverId = int.Parse(Console.ReadLine());
            var newDriver = allDrivers.FirstOrDefault(d => d.DriverId == driverId);

            if (newDriver != null)
            {
                trip.AssignedDriver = newDriver;
                Console.WriteLine("Success: Driver updated.");
            }
            else
            {
                Console.WriteLine("Error: Driver ID not found.");
            }

            Console.ReadKey();
        }

        // ==========================================
        // TAB 4: ADMIN (CONFLICTS)
        // ==========================================
        static void Tab_Admin()
        {
            Console.Clear();
            Console.WriteLine(">>> ADMIN: CONFLICT DETECTION <<<");

            // Check Schedule Overlaps (Simple Logic)
            Console.WriteLine("Checking for overlapping schedules on buses...");
            var busGroups = allTrips.GroupBy(t => t.AssignedBus.BusId);
            bool conflictFound = false;

            foreach (var group in busGroups)
            {
                if (group.Count() > 1)
                {
                    // In a real app, you would compare start/end times here
                    Console.WriteLine($"Warning: Bus {group.First().AssignedBus.PlateNumber} has multiple trips assigned.");
                    conflictFound = true;
                }
            }
            if (!conflictFound) Console.WriteLine("No schedule overlaps detected.");

            Console.WriteLine("\nChecking Bus Operational Status...");
            foreach (var bus in allBuses)
            {
                string status = bus.IsOperational ? "OK" : "MAINTENANCE";
                Console.WriteLine($"Bus {bus.PlateNumber}: {status}");
            }

            Console.ReadKey();
        }

        // ==========================================
        // DATA SETUP & CLASSES
        // ==========================================
        static void SetupData()
        {
            // 1. Locations
            var locManila = new Destination { Name = "Manila (Origin)" };
            var locTarlac = new Destination { Name = "Tarlac (Stop)" };
            var locPangasinan = new Destination { Name = "Pangasinan (Stop)" };
            var locBaguio = new Destination { Name = "Baguio (Dest)" };

            // 2. Routes
            var route1 = new Route { RouteId = 1, RouteName = "Manila-Baguio", Origin = locManila, Destination = locBaguio };
            route1.IntermediateStops.Add(locTarlac);
            route1.IntermediateStops.Add(locPangasinan);

            // 3. Drivers
            var d1 = new Driver { DriverId = 1, FullName = "Juan Cruz" };
            var d2 = new Driver { DriverId = 2, FullName = "Pedro Santos" };
            var d3 = new Driver { DriverId = 3, FullName = "Maria Reyes" };
            allDrivers.AddRange(new[] { d1, d2, d3 });

            // 4. Buses (Different Layouts)
            var bus1 = new Bus { BusId = 101, PlateNumber = "BUS-101", Capacity = 4, SeatingLayout = "2x2", IsOperational = true };
            var bus2 = new Bus { BusId = 102, PlateNumber = "VIP-999", Capacity = 3, SeatingLayout = "1x2", IsOperational = true };
            allBuses.Add(bus1);
            allBuses.Add(bus2);

            // 5. Create Trips & Generate Seats based on layout
            var trip1 = new Trip
            {
                TripId = 500,
                Route = route1,
                AssignedBus = bus1,
                AssignedDriver = d1,
                DepartureTime = DateTime.Now.AddHours(2),
                BasePrice = 500
            };
            GenerateSeats(trip1); // Helper to fill seats

            var trip2 = new Trip
            {
                TripId = 501,
                Route = route1,
                AssignedBus = bus2, // Different Bus
                AssignedDriver = d2, // Different Driver
                DepartureTime = DateTime.Now.AddHours(5),
                BasePrice = 850
            };
            GenerateSeats(trip2);

            allTrips.Add(trip1);
            allTrips.Add(trip2);
        }

        static void GenerateSeats(Trip trip)
        {
            // Simple logic: Create generic seats based on capacity
            // In a real app, "2x2" logic would determine naming (A1, A2 vs A1)
            for (int i = 1; i <= trip.AssignedBus.Capacity; i++)
            {
                trip.Seats.Add(new Seat { SeatNumber = "S" + i, IsOccupied = false });
            }
        }
    }
}