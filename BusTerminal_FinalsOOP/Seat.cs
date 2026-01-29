using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    public class Seat
    {
        public int SeatId { get; set; } // PK (Unique ID for the database row)
        public string SeatNumber { get; set; } // "A1", "B2"
        public bool IsOccupied { get; set; }

        // Link back to Trip
        public int TripId { get; set; }
    }
}
