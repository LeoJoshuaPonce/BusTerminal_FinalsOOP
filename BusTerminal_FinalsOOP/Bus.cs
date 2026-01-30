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
        public string SeatingLayout { get; set; }
        public bool IsOperational { get; set; }
        public bool IsDeleted { get; set; } // <--- New Property for Soft Delete
    }
}
