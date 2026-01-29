using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    public class Ticket
    {
        public int TicketId { get; set; } // PK
        public string PassengerName { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PricePaid { get; set; }

        // Link to Trip
        public int TripId { get; set; }
        public Trip Trip { get; set; }

        // Link to specific Seat
        public int SeatId { get; set; }
        public Seat Seat { get; set; }

        // Route Segments
        public string BoardingLocation { get; set; }
        public string DropOffLocation { get; set; }
    }
}
