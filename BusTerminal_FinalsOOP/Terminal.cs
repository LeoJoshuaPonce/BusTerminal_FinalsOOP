using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    public class Terminal
    {
        private List<Trip> _trips;
        private List<Bus> _buses;

        public Terminal(List<Trip> trips, List<Bus> buses)
        {
            _trips = trips;
            _buses = buses;
        }

        public bool ValidateSeatSale(int tripId, string seatNumber)
        {
            var trip = _trips.FirstOrDefault(t => t.TripId == tripId);
            if (trip == null) return false;
            var seat = trip.Seats.FirstOrDefault(s => s.SeatNumber.Equals(seatNumber, StringComparison.OrdinalIgnoreCase));

            if (seat == null) { Console.WriteLine("Error: Seat invalid."); return false; }
            if (seat.IsOccupied) { Console.WriteLine("Error: Seat taken."); return false; }

            return true;
        }
    }

}
