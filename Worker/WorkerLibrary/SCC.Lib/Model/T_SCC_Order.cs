using System;

namespace SCCWorker.Model
{
    public class T_SCC_Order
    {
        public int Id { get; set; }
        public DateTime DateRequest { get; set; }
        public string Status { get; set; }
        public string UpdateUser { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public int Pilot { get; set; }
    }
}
