using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbisParser.Models
{
    public class NameTableDataBase
    {
        public string invoiceTable { get; set; }
        public string invoiceItemTable { get; set; }
        public bool IsCreateTable { get; set; }
    }
}
