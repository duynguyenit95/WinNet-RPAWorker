using System;

namespace SCCWorker.Model
{
    public class T_SCC_LogHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string LogMessage { get; set; }
        public DateTime LogTime { get; set; }
    }
}
