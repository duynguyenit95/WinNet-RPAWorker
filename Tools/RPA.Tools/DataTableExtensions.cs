using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using SharpCifs.Smb;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Globalization;
using System.Diagnostics;

namespace RPA.Tools
{
    public static class DataTableExtensions
    {
        public static List<T> ToClass<T>(this DataTable dt, Dictionary<string, string> colMapping
            , CultureInfo culture, string[] dateFormat = null)
        {
            var cols = dt.Columns.Cast<DataColumn>()
            .Select(c => c.ColumnName)
            .ToList();

            var properties = typeof(T).GetProperties();

            var result = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                var objT = Activator.CreateInstance<T>();
                //Debug.WriteLine($"Current Row : " + rowCount);
                foreach (var pro in properties)
                {
                    var colName = colMapping.FirstOrDefault(x => x.Key == pro.Name).Value ?? pro.Name;
                    if (cols.Contains(colName))
                    {
                        var value = row[colName].ToString().GeneralParser(pro.PropertyType, dateFormat, culture);
                        pro.SetValue(objT, value, null);
                    }
                }
                result.Add(objT);
            }
            return result;
        }
    }

    public static class DataParser
    {
        public static object GeneralParser(this string val, Type type, string[] dateFormat, CultureInfo cultureInfo)
        {
            if (type == typeof(string))
            {
                return val;
            }
            else if (type == typeof(int))
            {
                if (int.TryParse(val, out int res))
                {
                    return res;
                }
                return 0;
            }
            else if (type == typeof(DateTime?) || type == typeof(DateTime))
            {
                if (dateFormat.Length > 0)
                {
                    if (DateTime.TryParseExact(val, dateFormat, cultureInfo, DateTimeStyles.None, out DateTime res))
                    {
                        return res;
                    }
                }
                else
                {
                    if (DateTime.TryParse(val, out DateTime res))
                    {
                        return res;
                    }
                }
            }
            else if (type == typeof(decimal))
            {
                if (decimal.TryParse(val, out decimal res))
                {
                    return res;
                }
                return 0m;
            }

            return null;
        }

        public static DateTime? ParseDateExact(this string val, CultureInfo cultureInfo, string[] formats = null)
        {
            if (DateTime.TryParseExact(val, formats, cultureInfo, DateTimeStyles.None, out DateTime res))
            {
                return res;
            }
            return null;
        }
        public static DateTime? ParseDate(this string val)
        {

            return null;
        }
        public static int ParseInt(this string val)
        {
            if (int.TryParse(val, out int res))
            {
                return res;
            }
            return 0;
        }

        public static decimal ParseDecimal(this string val)
        {
            if (decimal.TryParse(val, out decimal res))
            {
                return res;
            }
            return 0;
        }

        public static decimal ParsePercent(this string val)
        {
            if (decimal.TryParse(val.Replace("%", string.Empty), out decimal res))
            {
                return res / 100;
            }
            return 0;
        }

    }

}