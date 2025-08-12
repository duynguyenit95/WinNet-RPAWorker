using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using RPA.Core;

namespace RPA.Worker.Core
{
    public class ScheduleTimer
    {
        public DateTime NextRunTime
        {
            get
            {
                return _nextRunTime;
            }
            private set
            {
                _nextRunTime = value;
                if (NextRunTimeUpdate != null)
                {
                    NextRunTimeUpdate.Invoke(value);
                }
            }
        }
        public string NextRunJSONParam { get; private set; }
        public event Func<string, Task> TimerLogs;
        public List<ScheduleOptions> Options => _options;

        public Func<string,Task> TimerHandler;
        public Func<DateTime, Task> NextRunTimeUpdate;

        private Timer _timer;
        private DateTime _nextRunTime;

        private List<ScheduleOptions> _options = new List<ScheduleOptions>();

        public ScheduleTimer()
        {
            _timer = new Timer(ExecuteTimerCallBack, this, Timeout.Infinite, Timeout.Infinite);
        }

        void ExecuteTimerCallBack(object obj)
        {
            if (TimerHandler != null)
            {
                TimerHandler.Invoke(this.NextRunJSONParam);
            }
            TimerSetup();
        }

        public void UpdateScheduleOptions(List<ScheduleOptions> newOptions)
        {
            _options = newOptions;
            TimerSetup();
            Log($"Finish Update Schedule Options");
        }

        private void TimerSetup()
        {
            // Stop timer
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            var nextSchecule = GetNextSchedule();
            
            if (nextSchecule != null)
            {
                NextRunTime = nextSchecule.NextRunTime;
                Log($"Next Run Time:{NextRunTime}");
                NextRunJSONParam = nextSchecule.JSONParam;
                var tick = (long)(NextRunTime - DateTime.Now).TotalMilliseconds;
                _timer.Change(tick, Timeout.Infinite);
            }
            else
            {
                NextRunTime = DateTime.MaxValue;
                NextRunJSONParam = string.Empty;
            }
            Log($"Finish Timer Setup");
        }

        public ScheduleOptions GetNextSchedule()
        {
            return _options.Where(x => x.IsValid && x.NextRunTime > DateTime.Now).OrderBy(x => x.NextRunTime).FirstOrDefault();
        }

        public void Log(string message)
        {
            if (TimerLogs != null)
            {
                TimerLogs.Invoke("Schedule Timer: " + message);
            }
        }
    }
}
