using System;
using System.Text.RegularExpressions;

namespace RPA.Core
{
    public class QueueTask : Entity
    {
        public string ActionName { get; set; }
        public string RequestGroup { get; set; }
        public string RequestWorker { get; set; }
        public DateTime RequestTime { get; set; } = DateTime.Now;
        public string Input { get; set; }
        public string ProcessGroup { get; set; }
        public string ProcessWorker { get; set; }
        public DateTime? FinishTime { get; set; } = null;
        public string Output { get; set; }
        public QueueStatus Status { get; set; } = QueueStatus.InQueue;
        public string Note { get; set; }
    }
}
