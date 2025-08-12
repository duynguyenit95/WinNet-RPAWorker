using System.Collections.Generic;

namespace RPA.Core
{
    public class ServerOption
    {
        public string ServerUrl { get; set; } = "http://localhost:5000/";
        public string WorkerName { get; set; } = "SampleWorker";
        public string WorkerVersion { get; set; } = "0.0.0.0";
        public string Token { get; set; } = "RMIV.RPA.Worker";
        public string HubUrl => ServerUrl + "rpahub";
        public string GetOptionUrl => ServerUrl + "api/workeroption/get";
        public string RegisterOptionUrl => ServerUrl + "api/workeroption/register";
        public string WorkerInfoUrl => ServerUrl + "api/worker/" + WorkerName;
        public int MinRetryTime { get; set; } = 0;
        public int MaxRetryTime { get; set; } = 60;
    }
}
