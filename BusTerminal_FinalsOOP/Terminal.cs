using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    public class Terminal
    {
        private List<Route> _routes = new List<Route>();
        private List<Bus> _buses = new List<Bus>();
        private List<Driver> _drivers = new List<Driver>();
        private List<Trip> _trips = new List<Trip>();

        private FileManager _fmRoutes = new FileManager("routes.csv");
        private FileManager _fmBuses = new FileManager("buses.csv");
        private FileManager _fmDrivers = new FileManager("drivers.csv");
        private FileManager _fmTrips = new FileManager("trips.csv");
        private FileManager _fmSeats = new FileManager("seats.csv");

        public Terminal()
        {
            LoadDatabase();
        }

        public void Run()
        {
            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("BUS TERMINAL SYSTEM");
                Console.WriteLine("Choose Action");
                Console.WriteLine(" 1 DASHBOARD");
                Console.WriteLine(" 2 POS");
                Console.WriteLine(" 3 DISPATCH");
                Console.WriteLine(" 4 MANAGE BUSES");
                Console.WriteLine(" 5 EXIT");
                Console.Write("Select: ");

                switch (Console.ReadLine())
                {
                    case "1": ShowDashboard(); break;
                    case "2": RunPOS(); break;
                    case "3": RunDispatch(); break;
                    case "4": RunManageBuses(); break;
                    case "5": running = false; break;
                }
            }
        }

        private void ShowDashboard()
        {
            Console.Clear();
            Console.WriteLine("DASHBOARD");
            Console.WriteLine("{0,-6} {1,-8} {2,-15} {3,-10} {4,-25}", "ID", "TIME", "ROUTE", "BUS", "STOPS");
            Console.WriteLine(new string('-', 70));

            foreach (var t in _trips)
            {
                if (t.AssignedBus != null && !t.AssignedBus.IsDeleted)
                {
                    var stopNames = t.Route.RouteStops.OrderBy(s => s.SequenceOrder).Select(s => s.Destination.Name);
                    string stops = string.Join("..", stopNames);

                    if (stops.Length > 25) stops = stops.Substring(0, 22) + "...";

                    Console.WriteLine("{0,-6} {1,-8} {2,-15} {3,-10} {4,-25}",
                        t.TripId, t.DepartureTime.ToString("HH:mm"), t.Route.Name, t.AssignedBus.PlateNumber, stops);
                }
            }
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        private void RunPOS()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("POS");
                Console.WriteLine("AVAILABLE TRIPS");
                Console.WriteLine("{0,-6} {1,-8} {2,-15} {3,-10}", "ID", "TIME", "ROUTE", "BUS");

                foreach (var t in _trips)
                {
                    if (t.AssignedBus != null && !t.AssignedBus.IsDeleted)
                    {
                        Console.WriteLine("{0,-6} {1,-8} {2,-15} {3,-10}", t.TripId, t.DepartureTime.ToString("HH:mm"), t.Route.Name, t.AssignedBus.PlateNumber);
                    }
                }

                Trip trip = null;
                while (trip == null)
                {
                    Console.Write("\nEnter Trip ID (or 0 to return): ");
                    string input = Console.ReadLine();
                    if (input == "0") return;

                    if (int.TryParse(input, out int tid))
                    {
                        trip = _trips.FirstOrDefault(t => t.TripId == tid);
                        if (trip == null || trip.AssignedBus.IsDeleted)
                        {
                            Console.WriteLine("Error: Trip unavailable. Try again.");
                            trip = null;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID format.");
                    }
                }

                Console.WriteLine("\nSelect Boarding/Exit Points");
                var stops = trip.Route.RouteStops.OrderBy(s => s.SequenceOrder).ToList();

                for (int i = 0; i < stops.Count; i++)
                {
                    string eta = trip.DepartureTime.Add(stops[i].ETAFromOrigin).ToString("HH:mm");
                    Console.WriteLine($" [{i}] {stops[i].Destination.Name} (ETA: {eta})");
                }

                int startIdx = -1, endIdx = -1;
                bool validSegment = false;
                bool canceled = false;

                while (!validSegment && !canceled)
                {
                    Console.Write("\nEnter Boarding/Origin Index (or -1 to cancel): ");
                    if (!int.TryParse(Console.ReadLine(), out startIdx)) { Console.WriteLine("Invalid input."); continue; }
                    if (startIdx == -1) { canceled = true; break; }

                    Console.Write("Enter Drop-off/Destination Index (or -1 to cancel): ");
                    if (!int.TryParse(Console.ReadLine(), out endIdx)) { Console.WriteLine("Invalid input."); continue; }
                    if (endIdx == -1) { canceled = true; break; }

                    if (endIdx <= startIdx || startIdx < 0 || endIdx >= stops.Count)
                    {
                        Console.WriteLine("Error: Invalid segment range (Drop-off must be after Boarding). Try again.");
                    }
                    else
                    {
                        validSegment = true;
                    }
                }

                if (canceled) continue;

                Console.WriteLine("\nAvailable Seats: " + string.Join(", ", trip.Seats.Where(s => !s.IsOccupied).Select(s => s.SeatNumber)));

                bool seatSold = false;
                while (!seatSold && !canceled)
                {
                    Console.Write("Enter Seat (or 0 to cancel): ");
                    string sNum = Console.ReadLine();
                    if (sNum == "0") { canceled = true; break; }

                    if (ValidateSeatSale(trip.TripId, sNum))
                    {
                        var seat = trip.Seats.First(s => s.SeatNumber.Equals(sNum, StringComparison.OrdinalIgnoreCase));
                        seat.IsOccupied = true;
                        SaveDatabase();
                        Console.WriteLine($"\nTicket Sold from {stops[startIdx].Destination.Name} to {stops[endIdx].Destination.Name}!");
                        seatSold = true;
                    }
                }

                Console.WriteLine("Press any key to make another transaction (or Esc to return).");
                if (Console.ReadKey().Key == ConsoleKey.Escape) return;
            }
        }

        public bool ValidateSeatSale(int tripId, string seatNumber)
        {
            var trip = _trips.FirstOrDefault(t => t.TripId == tripId);
            if (trip == null) return false;

            var seat = trip.Seats.FirstOrDefault(s => s.SeatNumber.Equals(seatNumber, StringComparison.OrdinalIgnoreCase));
            if (seat == null)
            {
                Console.WriteLine("Error: Seat invalid.");
                return false;
            }

            if (seat.IsOccupied)
            {
                Console.WriteLine("Error: Seat taken.");
                return false;
            }

            return true;
        }

        private void RunDispatch()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("DISPATCH");
                Console.WriteLine("{0,-6} {1,-15} {2,-20}", "ID", "ROUTE", "CURRENT DRIVER");
                foreach (var t in _trips)
                    if (t.AssignedBus != null) Console.WriteLine("{0,-6} {1,-15} {2,-20}", t.TripId, t.Route.Name, t.AssignedDriver.FullName);

                Trip trip = null;
                while (trip == null)
                {
                    Console.Write("\nEnter Trip ID (or 0 to return): ");
                    string input = Console.ReadLine();
                    if (input == "0") return;

                    if (int.TryParse(input, out int tid))
                    {
                        trip = _trips.FirstOrDefault(t => t.TripId == tid);
                        if (trip == null) Console.WriteLine("Error: Trip not found.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid Input.");
                    }
                }

                Console.WriteLine("Available Drivers:");
                foreach (var d in _drivers) Console.WriteLine($"{d.DriverId}: {d.FullName}");

                bool driverAssigned = false;
                while (!driverAssigned)
                {
                    Console.Write("New Driver ID (0 to cancel): ");
                    if (int.TryParse(Console.ReadLine(), out int did))
                    {
                        if (did == 0) break;

                        var driver = _drivers.FirstOrDefault(d => d.DriverId == did);
                        if (driver != null)
                        {
                            trip.AssignedDriver = driver;
                            trip.AssignedDriverId = did;
                            SaveDatabase();
                            Console.WriteLine("Success: Driver updated.");
                            driverAssigned = true;
                        }
                        else
                        {
                            Console.WriteLine("Error: Driver ID not found.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid input.");
                    }
                }

                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
        }

        private void RunManageBuses()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("MANAGE BUSES");
                Console.WriteLine("1. LIST");
                Console.WriteLine("2. ADD");
                Console.WriteLine("3. EDIT");
                Console.WriteLine("4. DELETE");
                Console.WriteLine("5. BACK");
                Console.Write("Select: ");

                string choice = Console.ReadLine();
                if (choice == "5") break;

                switch (choice)
                {
                    case "1":
                        Console.Clear();
                        Console.WriteLine("{0,-6} {1,-12} {2,-5} {3,-10}", "ID", "PLATE", "CAP", "LAYOUT");
                        Console.WriteLine(new string('-', 40));
                        foreach (var b in _buses.Where(b => !b.IsDeleted))
                        {
                            Console.WriteLine("{0,-6} {1,-12} {2,-5} {3,-10}", b.BusId, b.PlateNumber, b.Capacity, b.SeatingLayout);
                        }
                        Console.ReadKey();
                        break;

                    case "2":
                        Console.Clear();
                        int newId = _buses.Any() ? _buses.Max(b => b.BusId) + 1 : 100;

                        string plate = "";
                        bool addCancelled = false;
                        while (string.IsNullOrWhiteSpace(plate))
                        {
                            Console.Write("Enter Plate (0 to cancel): ");
                            string input = Console.ReadLine();
                            if (input == "0") { addCancelled = true; break; }

                            if (_buses.Any(b => b.PlateNumber == input && !b.IsDeleted))
                                Console.WriteLine("Error: Bus with this plate already exists.");
                            else
                                plate = input;
                        }
                        if (addCancelled) break;

                        int cap = 0;
                        while (cap <= 0)
                        {
                            Console.Write("Enter Capacity (0 to cancel): ");
                            if (!int.TryParse(Console.ReadLine(), out cap))
                            {
                                Console.WriteLine("Invalid number.");
                                continue;
                            }
                            if (cap == 0) { addCancelled = true; break; }
                        }
                        if (addCancelled) break;

                        Console.Write("Enter Layout: ");
                        string layout = Console.ReadLine();

                        _buses.Add(new Bus { BusId = newId, PlateNumber = plate, Capacity = cap, SeatingLayout = layout, IsOperational = true, IsDeleted = false });
                        SaveDatabase();
                        Console.WriteLine("Bus Added!");
                        Console.ReadKey();
                        break;

                    case "3":
                        Console.Clear();
                        Bus busToEdit = null;
                        while (busToEdit == null)
                        {
                            Console.Write("Bus ID to Edit (0 to cancel): ");
                            if (int.TryParse(Console.ReadLine(), out int editId))
                            {
                                if (editId == 0) break;
                                busToEdit = _buses.FirstOrDefault(b => b.BusId == editId && !b.IsDeleted);
                                if (busToEdit == null) Console.WriteLine("Bus not found.");
                            }
                            else Console.WriteLine("Invalid ID.");
                        }

                        if (busToEdit != null)
                        {
                            Console.Write($"New Plate ({busToEdit.PlateNumber}): ");
                            string pInput = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(pInput)) busToEdit.PlateNumber = pInput;

                            Console.Write($"New Layout ({busToEdit.SeatingLayout}): ");
                            string lInput = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(lInput)) busToEdit.SeatingLayout = lInput;

                            SaveDatabase();
                            Console.WriteLine("Bus Updated.");
                        }
                        Console.ReadKey();
                        break;

                    case "4":
                        Console.Clear();
                        Bus busDel = null;
                        while (busDel == null)
                        {
                            Console.Write("Bus ID to Delete (0 to cancel): ");
                            if (int.TryParse(Console.ReadLine(), out int delId))
                            {
                                if (delId == 0) break;
                                busDel = _buses.FirstOrDefault(b => b.BusId == delId);
                                if (busDel == null) Console.WriteLine("Bus not found.");
                            }
                            else Console.WriteLine("Invalid ID.");
                        }

                        if (busDel != null)
                        {
                            busDel.IsDeleted = true;
                            SaveDatabase();
                            Console.WriteLine("Deleted.");
                        }
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void LoadDatabase()
        {
            if (_fmRoutes.getStatus())
            {
                foreach (string line in _fmRoutes.getLines())
                {
                    var p = line.Split(',');
                    if (p.Length >= 3)
                    {
                        Route r = new Route { RouteId = int.Parse(p[0]), Name = p[1] };
                        string[] stopNames = p[2].Split('|');
                        for (int i = 0; i < stopNames.Length; i++)
                        {
                            Destination dest = new Destination { DestinationId = i, Name = stopNames[i] };
                            RouteStop stop = new RouteStop
                            {
                                DestinationId = dest.DestinationId,
                                Destination = dest,
                                SequenceOrder = i + 1,
                                ETAFromOrigin = TimeSpan.FromHours(i)
                            };
                            r.RouteStops.Add(stop);
                        }
                        _routes.Add(r);
                    }
                }
            }

            if (_fmBuses.getStatus())
            {
                foreach (string line in _fmBuses.getLines())
                {
                    var p = line.Split(',');
                    bool isDel = p.Length > 5 ? bool.Parse(p[5]) : false;
                    _buses.Add(new Bus { BusId = int.Parse(p[0]), PlateNumber = p[1], Capacity = int.Parse(p[2]), SeatingLayout = p[3], IsOperational = bool.Parse(p[4]), IsDeleted = isDel });
                }
            }

            if (_fmDrivers.getStatus())
            {
                foreach (string line in _fmDrivers.getLines())
                {
                    var p = line.Split(',');
                    _drivers.Add(new Driver { DriverId = int.Parse(p[0]), FullName = p[1] });
                }
            }

            if (_fmTrips.getStatus())
            {
                foreach (string line in _fmTrips.getLines())
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
                    t.Route = _routes.FirstOrDefault(r => r.RouteId == t.RouteId);
                    t.AssignedBus = _buses.FirstOrDefault(b => b.BusId == t.AssignedBusId);
                    t.AssignedDriver = _drivers.FirstOrDefault(d => d.DriverId == t.AssignedDriverId);
                    _trips.Add(t);
                }
            }

            if (_fmSeats.getStatus())
            {
                foreach (string line in _fmSeats.getLines())
                {
                    var p = line.Split(',');
                    var t = _trips.FirstOrDefault(x => x.TripId == int.Parse(p[1]));
                    if (t != null) t.Seats.Add(new Seat { SeatId = int.Parse(p[0]), SeatNumber = p[2], IsOccupied = bool.Parse(p[3]) });
                }
            }
        }

        private void SaveDatabase()
        {
            List<string> busLines = new List<string>();
            foreach (var b in _buses) busLines.Add($"{b.BusId},{b.PlateNumber},{b.Capacity},{b.SeatingLayout},{b.IsOperational},{b.IsDeleted}");
            _fmBuses.Write(busLines, false);

            List<string> tripLines = new List<string>();
            foreach (var t in _trips) tripLines.Add($"{t.TripId},{t.RouteId},{t.AssignedBusId},{t.AssignedDriverId},{t.DepartureTime},{t.BasePrice}");
            _fmTrips.Write(tripLines, false);

            List<string> seatLines = new List<string>();
            foreach (var t in _trips) foreach (var s in t.Seats) seatLines.Add($"{s.SeatId},{t.TripId},{s.SeatNumber},{s.IsOccupied}");
            _fmSeats.Write(seatLines, false);
        }
    }
}