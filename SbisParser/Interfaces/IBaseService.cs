using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbisParser.Interfaces
{
    public interface IBaseService
    {
        Task<bool> WriteDataToBase(string TableName, bool IsCreateTable, DataTable Data);
    }
}
