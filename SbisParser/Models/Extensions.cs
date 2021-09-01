using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbisParser.Models
{
    public static class Extensions
    {
        public static DataTable ToDataTable<T>(this List<T> self)
        {
            var properties = typeof(T).GetProperties();
            DataTable dt = new();
            foreach (var info in properties)
                dt.Columns.Add(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType);
            foreach (var entity in self)
                dt.Rows.Add(properties.Select(p => p.GetValue(entity)).ToArray());

            return dt;
        }

        public static string ReturnTempString(this string str) => string.IsNullOrEmpty(str) ? "" : str;

        public static void MoveFile(this string file, string toMove)
        {
            if (File.Exists(file))
            {
                if (!Directory.Exists(toMove))
                    Directory.CreateDirectory(toMove);
                string fileName = Path.GetFileName(file);
                File.Move(file, $"{toMove}\\{fileName}");
            }
        }

        public static string GetSqlCommandCreateTable(this DataTable table)
        {
            string command = $"CREATE TABLE {table.TableName}";
            
            return null;
        }
    }
}
