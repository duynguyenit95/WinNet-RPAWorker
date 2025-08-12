using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UQ09.Model;

namespace UQ09.Handle
{
    public class Convert
    {
        private readonly string standardFormat = "yyyy-MM-dd";
        public (bool, DateTime) ConvertStringToDate(string input)
        {
            DateTime date;

            if (DateTime.TryParseExact(input, standardFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return (true, date);
            }

            // Return default DateTime value if input is not a valid date
            return (false, date);
        }
        public string ConvertDate(string input)
        {
            DateTime date;

            if (DateTime.TryParseExact(input, standardFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return date.ToString(standardFormat);
            }

            // Try to parse input to a valid date
            if (DateTime.TryParse(input, out date))
            {
                return date.ToString(standardFormat);
            }

            // Return blank if input is not a valid date
            return string.Empty;
        }
        public string ConvertDate(DateTime date)
        {
            return date.ToString(standardFormat);
        }
        public string ConvertNumber(string input)
        {
            return decimal.Parse(input, CultureInfo.InvariantCulture).ToString("F3", CultureInfo.InvariantCulture);
        }
        public string ConvertNumber(decimal num)
        {
            return num.ToString("F3", CultureInfo.InvariantCulture);
        }
        public bool CheckValid(MaterialPO input)
        {
            if (input == null) return false;
            // Get all properties of the input object
            PropertyInfo[] properties = typeof(MaterialPO).GetProperties();

            // Check if any property is null or empty
            foreach (var property in properties)
            {
                var value = property.GetValue(input)?.ToString();
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }
            }
            return true;
        }
        public int GetWeekId()
        {
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Thursday);
        }
    }
}
