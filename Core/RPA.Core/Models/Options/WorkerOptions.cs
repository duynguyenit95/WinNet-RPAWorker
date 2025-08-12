using System;
using System.Collections.Generic;

namespace RPA.Core
{
    public class WorkerOption
    {
        public NSFOptions NSFOpt { get; set; }
        public List<ScheduleOptions> ScheduleOpt { get; set; }
    }

    public class WorkerOptionValidateResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
    }
}
