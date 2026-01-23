using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    public class Bus
    {
        public int BusId { get; set; }
        public string PlateNumber { get; set; }
        public int Capacity { get; set; }
        public string SeatingLayoutType { get; set; } // e.g., "2x2", "1x2"
        public bool IsOperational { get; set; } // To check if available or in maintenance

        // Links to the schedule to check for overlap availability
        public List<Trip> ScheduledTrips { get; set; } = new List<Trip>();
    }
}
