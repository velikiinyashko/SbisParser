using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbisParser.Models
{
    public class DataBaseSettings
    {
        public string DataSource { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        public string InitialCatalog { get; set; }
        public int ConnectTimeout { get; set; }
        public string ApplicationName { get; set; }
        public bool MultipleActiveResultSets { get; set; }
    }
}
