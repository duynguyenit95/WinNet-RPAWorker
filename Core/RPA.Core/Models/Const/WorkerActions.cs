namespace RPA.Core
{
    public class WorkerActions
    {
        public const string WorkerUpdate = "WorkerUpdate";
        public const string WorkerVersionAutoUpdate = "WorkerVersionAutoUpdate";
        public const string WorkerDataUpdate = "WorkerDataUpdate";
        

        public const string LoadTimerConfiguration = "LoadConfiguration";
        public const string ValidateOptions = "ValidateOptions";
        public const string ValidateOptionsResult = "ValidateOptionsResult";
        
        public const string Execute = "Execute";

        public const string WorkerStateChanged = "WorkerStateChanged";
        public const string SetWorkerState = "SetWorkerState";
 
        public const string WorkerOnline = "WorkerOnline";
        public const string WorkerOffline = "WorkerOffline";

        public const string UpdateSchedule = "UpdateSchedule";
        public const string UpdateParamater = "UpdateParamater";

        public const string RequestNewSAPSession = "RequestNewSAPSession";
        public const string SAPSessionCreated = "SAPSessionCreated";
        public const string CloseSAPSession = "CloseSAPSession";
        public const string RestartSAP = "RestartSAP";
        public const string TestSAPCredential = "TestSAPCredential";

        public const string AddQueue = "AddQueue";

        public const string GerberExport = "GerberExport";
        public const string GerberCuttingState = "GerberCuttingState";
        public const string ReadCutMachineState = "ReadCutMachineState";
        public const string CutMachineStateUpdate = "CutMachineStateUpdate";

        public const string CutMachineDataUpdate = "CutMachineDataUpdate";
        public const string CutMachineShowAbnormalForm = "CutMachineShowAbnormalForm";
        public const string ShowOEEForm = "ShowOEEForm";

        public const string SAPError = "SAPError";
    }

    public class ConstValue
    {
        public const string CutMachineSid = "CutMachineSid";
        public const string CutMachineSoftware = "CutMachineSoftware";
        public const string CutMachineSoftwareVersion = "CutMachineSoftwareVersion";
        public const string GerberGroup = "GerberGroup";
        public const string Token = "Token";
        public const string BaseType = "BaseType";
        public const string WorkerName = "WorkerName";
        public const string DeviceID = "DeviceID";
        public const string Version = "Version";
        public const string WorkerId = "WorkerId";
        public const string WorkerCentral = "WorkerCentral";
        public const string UserGroup = "UserGroup";
        public const string WorkerGroup = "WorkerGroup";
        public const string UnknownGroup = "Unknown";
    }
}
