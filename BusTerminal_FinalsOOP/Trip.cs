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
        public int RouteId { get; set; }
        public Route Route { get; set; } // Link

        public int AssignedBusId { get; set; }
        public Bus AssignedBus { get; set; } // Link

        public int AssignedDriverId { get; set; }
        public Driver AssignedDriver { get; set; } // Link

        public DateTime DepartureTime { get; set; }
        public decimal BasePrice { get; set; }
        public List<Seat> Seats { get; set; } = new List<Seat>();
    }
}
