using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTerminal_FinalsOOP
{
    public class Receipt
    {
        public int ReceiptId { get; set; }
        public int TicketId { get; set; }
        public DateTime TimeCreated { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal DiscountApplied { get; set; }
        public string PaymentMethod { get; set; } 
    }
}
