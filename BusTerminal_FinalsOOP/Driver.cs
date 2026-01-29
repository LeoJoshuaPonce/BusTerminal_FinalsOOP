using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    public class Driver
    {
        public int DriverId { get; set; } // PK
        public string FullName { get; set; }
        public string LicenseNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
