using Microsoft.Playwright;
using RPA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCWorker
{
    public class SCCWorkerOption : WorkerOption
    {
        public string ORPConnetion { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsCancel { get; set; } = false;
    }
}
