using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    public class Terminal
    {
        private List<Trip> _allTrips;
        private List<Bus> _allBuses;

        public bool IsBusAvailable(int busId, DateTime newDeparture, DateTime newArrival)
        {
            var busTrips = _allTrips.Where(t => t.BusId == busId);

            foreach (var trip in busTrips)
            {
                if (newDeparture < trip.EstimatedArrivalTime && newArrival > trip.DepartureTime)
                {
                    Console.WriteLine("Conflict: Bus is already scheduled for another trip at this time.");
                    return false;
                }
            }

            var bus = _allBuses.FirstOrDefault(b => b.BusId == busId);
            if (bus != null && !bus.IsOperational)
            {
                Console.WriteLine("Conflict: Bus is currently unavailable/under maintenance.");
                return false;
            }

            return true;
        }

        public bool IsSitAvailable(int tripId, string seatNumber)
        {
            var trip = _allTrips.FirstOrDefault(t => t.TripId == tripId);
            if (trip == null) return false;

            var seat = trip.Seats.FirstOrDefault(s => s.SeatNumber == seatNumber);

            if (seat == null)
            {
                Console.WriteLine("Error: Seat does not exist on this bus layout.");
                return false;
            }

            if (seat.IsOccupied)
            {
                Console.WriteLine("Conflict: Seat is already sold.");
                return false;
            }

            return true;
        }
    }
}
