using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    public class Route
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } 
        public Destination Origin { get; set; }
        public Destination Destination { get; set; }

        // Handles "intermediate stops" requirement
        public List<RouteStop> Stops { get; set; } = new List<RouteStop>();
    }
}
