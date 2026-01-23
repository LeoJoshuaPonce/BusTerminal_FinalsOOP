using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public int TripId { get; set; }
        public string PassengerName { get; set; }
        public string SeatNumber { get; set; }
        public Destination BoardingPoint { get; set; } // Can be origin or intermediate stop
        public Destination DropOffPoint { get; set; }
        public decimal PricePaid { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
