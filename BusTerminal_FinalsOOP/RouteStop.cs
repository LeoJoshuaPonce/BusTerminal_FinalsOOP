using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusTerminal_FinalsOOP;

namespace BusTerminal_FinalsOOP
{
    public class RouteStop
    {
        public int DestinationId { get; set; }
        public Destination Destination { get; set; }
        public int SequenceOrder { get; set; } 
        public TimeSpan ETAFromOrigin { get; set; }
    }
}
