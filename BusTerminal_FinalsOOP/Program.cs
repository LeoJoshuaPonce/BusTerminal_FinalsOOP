using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    internal class Program
    {
        static List<Trip> allTrips = new List<Trip>();
        static Terminal manager = new Terminal();
        static void Main(string[] args)
        {
            // 1. Setup Dummy Data (The "Database")
            SetupData();

            // 2. Main Menu Loop
            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("=== BUS TERMINAL SYSTEM ===");
                Console.WriteLine("1. View Dashboard (Departures)");
                Console.WriteLine("2. Buy Ticket (POS)");
                Console.WriteLine("3. Exit");
                Console.Write("Select an option: ");

                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ShowDashboard();
                        break;
                    case "2":
                        OpenPOS();
                        break;
                    case "3":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }
        static void ShowDashboard()
        {
            Console.Clear();
            Console.WriteLine("--- DEPARTURE DASHBOARD ---");
            Console.WriteLine("{0,-10} {1,-20} {2,-20} {3,-15}", "TripID", "Time", "Destination", "Driver");
            Console.WriteLine(new string('-', 65));

            foreach (var trip in allTrips)
            {
                Console.WriteLine("{0,-10} {1,-20} {2,-20} {3,-15}",
                    trip.TripId,
                    trip.DepartureTime.ToString("HH:mm"),
                    trip.Route.Destination.Name,
                    trip.AssignedDriver.FullName);
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        
        static void OpenPOS()
        {
            Console.Clear();
            Console.WriteLine("--- POINT OF SALE ---");

            Console.Write("Enter Trip ID: ");
            int tripId = int.Parse(Console.ReadLine());

            Console.Write("Enter Seat Number (e.g. A1): ");
            string seatNum = Console.ReadLine();

            // Use the Manager Class logic we defined earlier
            bool isSuccess = manager.IsSitAvailable(tripId, seatNum);

            if (isSuccess)
            {
                // Simulate Payment and Printing
                Console.WriteLine("Payment processed successfully.");
                Console.WriteLine("Printing Ticket...");
                Console.WriteLine("-----------------------------");
                Console.WriteLine($"TICKET: Trip {tripId} | Seat {seatNum}");
                Console.WriteLine("-----------------------------");

                // Actually mark it sold in the data
                var trip = allTrips.First(t => t.TripId == tripId);
                var seat = trip.Seats.First(s => s.SeatNumber == seatNum);
                seat.IsOccupied = true;
            }
            else
            {
                Console.WriteLine("TRANSACTION FAILED: Seat unavailable or invalid.");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        static void SetupData()
        {
            // Initialize your lists (Buses, Routes, Trips) here for testing
            // This is where you manually add 'new Bus()...' to test the logic.
        }
    }
}
