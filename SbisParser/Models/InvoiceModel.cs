using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbisParser.Models
{
    public class InvoiceModel
    {
        public string Number { get; set; }
        public string DateInvoice { get; set; }
        public string INNOrg { get; set; }
        public string INNSupplier { get; set; }
        public string NumDogovor { get; set; }
        public decimal SumIncVat { get; set; }
        public decimal SumExtVat { get; set; }

    }
}
