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
        public string Name { get; set; }
        public List<RouteStop> RouteStops { get; set; } = new List<RouteStop>();
    }
}
