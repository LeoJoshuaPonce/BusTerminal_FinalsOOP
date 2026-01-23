using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    public class Seat
    {
        public string SeatNumber { get; set; } // e.g., "A1", "B2"
        public bool IsOccupied { get; set; }
        public int? TicketId { get; set; } // Null if empty
    }
}
