using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    public class Trip
    {
        public int TripId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime EstimatedArrivalTime { get; set; }

        // Relationships
        public int RouteId { get; set; }
        public Route Route { get; set; }

        public int BusId { get; set; }
        public Bus AssignedBus { get; set; }

        public int DriverId { get; set; }
        public Driver AssignedDriver { get; set; }

        // Manages the seating for this specific trip
        public List<Seat> Seats { get; set; } = new List<Seat>();

        public decimal BasePrice { get; set; }
    }
}
