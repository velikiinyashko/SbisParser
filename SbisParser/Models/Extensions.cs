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
        public static DataTable ToDataTable<T>(this List<T> self, string tableName)
        {
            var properties = typeof(T).GetProperties();

            DataTable dt = new();
            dt.TableName = tableName;

            foreach (var info in properties)
                dt.Columns.Add(info.Name, typeof(string));

            foreach (var entity in self)
                dt.Rows.Add(properties.Select(p => p.GetValue(entity)?.ToString()).ToArray());

            return dt;
        }

        public static string ReturnTempString(this string str) => string.IsNullOrEmpty(str) ? "" : str;

        public static void MoveFile(this string file, string toMove)
        {
            if (!File.Exists(file))
            {
                if (!Directory.Exists(toMove))
                    Directory.CreateDirectory(toMove);
                string fileName = Path.GetFileName(file);
                File.Copy(file, $"{toMove}\\{fileName}", true);
            }
        }

        public static string GetSqlCommandCreateTable(this DataTable table)
        {
            string command = $"CREATE TABLE {table.TableName}";
            
            return null;
        }
    }
}
