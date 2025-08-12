using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using RPA.Core;

namespace RPA.Web.Hubs
{
    public class WorkerConnection
    {
        public Guid WorkerId { get; set; }
        public WorkerBaseType BaseType { get; set; }
        public string ConnectionId { get; set; }
        public string ConnectionUnit { get; set; }
        public WorkerState WorkerState { private get; set; }
        public string State => WorkerState.ToString();
        public string Name { get; set; }
        public string Version { get; set; }
        public string Device { get; set; }

        public string NextRunTime { get; set; }
        public string LastRunResult { get; set; }

        public string Exception { get; set; }
        public object Entity { get; set; }
        public string JSONData { get; set; }
        public List<string> Groups { get; set; }

    }

}
