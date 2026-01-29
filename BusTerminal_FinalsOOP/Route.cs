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
        public int RouteId { get; set; } // PK
        public string RouteName { get; set; }

        // Foreign Keys for linking
        public int OriginLocationId { get; set; }
        public Destination Origin { get; set; }

        public int DestinationLocationId { get; set; }
        public Destination Destination { get; set; }

        // Many-to-Many relationship often handled by a join table in DB, 
        // but for now, a list is fine for the object model.
        public List<Destination> IntermediateStops { get; set; } = new List<Destination>();
    }
}
