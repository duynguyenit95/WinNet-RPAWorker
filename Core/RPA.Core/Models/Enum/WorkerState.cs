using System;

namespace RPA.Core
{
    public enum WorkerState
    {
        Setup,
        Stopped,
        Idle,
        Executing,
        Error,
        Good,
        Queueing,
        WaitingForSAPSessions,
        WaitingForPrint,
    }
}
