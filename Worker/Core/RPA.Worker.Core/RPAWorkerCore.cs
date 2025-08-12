using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using RPA.Core;
using System.ComponentModel;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using SharpCifs.Smb;
using RPA.Tools;
using System.Net.Http;
using System.Reflection;


namespace RPA.Worker.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class RPAWorkerCore<T> where T : WorkerOption
    {
        public ServerOption ServerOptions { get; private set; }
        private T _workerOption;
        public T WorkerOption
        {
            get => _workerOption;
            private set
            {
                _workerOption = value;
                if (WorkerOptionLoaded != null)
                {
                    WorkerOptionLoaded.Invoke();
                }
            }
        }

        private Guid _workerId;
        public Guid WorkerId => _workerId;
        public RPAWorkerCore(ServerOption options, WorkerBaseType workerBaseType = WorkerBaseType.Default)
        {
            LoggerSetup();
            BaseType = workerBaseType;
            ServerOptions = options;
            _workerId = Guid.NewGuid();
            LoadSid();
            CreateWorkingFolder();
        }

        private void LoadSid()
        {
            string _workerIdFile = Path.Combine(AppContext.BaseDirectory, "_workerId.json");
            if (File.Exists(_workerIdFile))
            {
                _workerId = Guid.Parse(File.ReadAllText(_workerIdFile));
            }
            else
            {
                File.WriteAllText(_workerIdFile, _workerId.ToString());
            }
        }

        private bool isInit = false;
        private void Init()
        {
            ConfigHubConnectionHeaderKeys();
            try
            {
                HubClientSetup();
            }
            catch (Exception ex)
            {

            }
        }

        public WorkerBaseType BaseType { get; set; }

        #region Configuration
        public event Func<Task> WorkerOptionLoaded;
        private async Task LoadWorkerOption()
        {
            try
            {
                var serverOption = await ServerRequest.Get<T>(ServerOptions.GetOptionUrl
                                , $"/{System.Environment.MachineName}/{ServerOptions.WorkerName}"
                                , ServerOptions.Token);
                HubLog("Get Option Success !!!");
                if (serverOption != null)
                {
                    WorkerOption = serverOption;
                    HubLog("Set Option Success !!!");
                }
                else
                {
                    HubLog("Option Not Found !!!");
                    WorkerOption = LoadWorkerDefaultOption();
                    HubLog("Load Default Success !!!");
                    HubLog(JsonConvert.SerializeObject(WorkerOption));
                    await RegisterWorkerOption();
                }
                HubLog("Load configuration successfully !!!");
            }
            catch (Exception ex)
            {
                HubLog("Error:" + ex);
                HubLog("Could not load configuration. Using Default Option instead!!!");
                WorkerOption = LoadWorkerDefaultOption();
            }
        }

        private async Task RegisterWorkerOption()
        {
            HubLog("Start Register Option");
            var keys = new Dictionary<string, string>
            {
                { "workerId", _workerId.ToString() },
                { "workerName", ServerOptions.WorkerName },
                { "group", System.Environment.MachineName },
                { "jsonValues", JsonConvert.SerializeObject(WorkerOption) }
            };
            var form = new FormUrlEncodedContent(keys);
            var request = await ServerRequest.Post(ServerOptions.RegisterOptionUrl, "", form, ServerOptions.Token);
            if (request)
            {
                HubLog("Register Option successfully!!!");
            }
            else
            {
                HubLog("Register Option fail!!!");
            }
        }

        public virtual WorkerOptionValidateResult ValidateOptions(T option)
        {
            HubLog("Start Validate Option");
            var result = new WorkerOptionValidateResult();
            if (option.NSFOpt != null)
            {
                HubLog("Validate NSFOption");
                try
                {
                    var auth = new NtlmPasswordAuthentication(option.NSFOpt.AuthDomain, option.NSFOpt.AuthUsername, option.NSFOpt.AuthPassword);
                    if (!string.IsNullOrEmpty(option.NSFOpt.InputFolderURI)) new SmbFile(option.NSFOpt.InputFolderURI, auth).Exists();
                    if (!string.IsNullOrEmpty(option.NSFOpt.ProcessFolderURI)) new SmbFile(option.NSFOpt.ProcessFolderURI, auth).Exists();
                    if (!string.IsNullOrEmpty(option.NSFOpt.SuccessFolderURI)) new SmbFile(option.NSFOpt.SuccessFolderURI, auth).Exists();
                    if (!string.IsNullOrEmpty(option.NSFOpt.FailureFolderURI)) new SmbFile(option.NSFOpt.FailureFolderURI, auth).Exists();
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Errors.Add(ex.Message);
                }
            }
            HubLog("Finish Base Validate");
            return result;
        }
        public virtual T LoadWorkerDefaultOption()
        {
            return Activator.CreateInstance<T>();
        }
        private async Task ValidateWorkerOptionsHandler(string requestConnectionId, string json)
        {
            HubLog("ValidateWorkerOptions");
            var option = JsonConvert.DeserializeObject<T>(json);
            HubLog("Serialize option success");
            var result = ValidateOptions(option);
            HubLog("Finish Validate");
            if (result.IsValid)
            {
                HubLog("Option Is Valid");
                WorkerOption = option;
                await RegisterWorkerOption();
            }
            HubLog("Send Result");
            await HubConnection.SendAsync(WorkerActions.ValidateOptionsResult
                , requestConnectionId
                , result);
            HubLog("Finish Send Result");
        }
        #endregion

        #region Start/Stop
        public async Task Start()
        {
            if (!isInit)
            {
                Init();
                isInit = true;
            }
            try
            {
                if (!ServerConnected())
                {
                    HubLog("Connect to Server");
                    await HubConnection.StartAsync();
                    HubState = HubConnection.State;
                    HubLog("Connect to Server Successfully");
                    UpdateWorkerStateChanged();
                    await LoadWorkerOption();
                }
            }
            catch (Exception ex)
            {
                HubLog(ex.ToString());
                await Start();
            }
        }
        public async void Stop()
        {
            if (ServerConnected())
            {
                await HubConnection.StopAsync();
                HubState = HubConnection.State;
            }
        }
        #endregion

        #region Hub
        public HubConnection HubConnection { get; private set; }
        private HubConnectionState _hubState = HubConnectionState.Disconnected;
        public event Func<HubConnectionState, Task> HubStateChanged;

        public HubConnectionState HubState
        {
            get { return _hubState; }
            set
            {
                _hubState = value;
                if (HubStateChanged != null)
                {
                    HubStateChanged.Invoke(value);
                }
            }
        }

        public event Func<string, Task> HubLogAdded;
        public Dictionary<string, string> _hubConnectionKeys = new Dictionary<string, string>();
        public virtual void ConfigHubConnectionHeaderKeys()
        {
            //var gerberGroup = ConfigurationHelper.GetAppSettingsValue("GerberGroup");
            _hubConnectionKeys.Add(ConstValue.Token, ServerOptions.Token);
            _hubConnectionKeys.Add(ConstValue.BaseType, BaseType.ToString());
            _hubConnectionKeys.Add(ConstValue.WorkerName, ServerOptions.WorkerName);
            _hubConnectionKeys.Add(ConstValue.DeviceID, System.Environment.MachineName);
            _hubConnectionKeys.Add(ConstValue.Version, ServerOptions.WorkerVersion);
            _hubConnectionKeys.Add(ConstValue.WorkerId, _workerId.ToString());
            //if (!string.IsNullOrEmpty(gerberGroup))
            //{
            //    _hubConnectionKeys.Add(ConstValue.GerberGroup, gerberGroup);
            //}
        }
        public virtual void HubClientSetup()
        {
            HubConnection = new HubConnectionBuilder()
            .WithUrl(ServerOptions.HubUrl, opt =>
            {
                opt.UseDefaultCredentials = true;
                foreach (var item in _hubConnectionKeys)
                {
                    opt.Headers.Add(item.Key, item.Value);
                }
            })
            .WithAutomaticReconnect(new RandomRetryPolicy(ServerOptions.MinRetryTime, ServerOptions.MaxRetryTime))
            .Build();

            HubConnection.Closed += OnClosed;
            HubConnection.Reconnecting += OnReconnecting;
            HubConnection.Reconnected += OnReconnected;
            HubConnection.On<string>("Send", HandleReceivedMessage);
            HubConnection.On<WorkerState>(WorkerActions.SetWorkerState, (state) =>
            {
                Logger?.Information("WorkerActions.SetWorkerState");
                State = state;
            });
            HubConnection.On<string, string>(WorkerActions.ValidateOptions, ValidateWorkerOptionsHandler);
            HubConnection.On(WorkerActions.WorkerVersionAutoUpdate, AutoUpdate);
            HubConnection.On<string>(WorkerActions.SAPError, OnSAPError);
        }

        public virtual void HandleSAPError(string message)
        {

        }
        private void OnSAPError(string message)
        {
            HandleSAPError(message);
        }

        private void AutoUpdate()
        {
            if (AutoUpdateHandler != null)
            {
                AutoUpdateHandler.Invoke();
            }
        }

        public void HubLog(string message)
        {
            Logger.Information(message);
            if (HubLogAdded != null)
            {
                HubLogAdded.Invoke(message);
            }
        }
        private async Task OnClosed(Exception error)
        {
            if (error != null)
            {
                HubLog(error.Message);
            }

            HubLog("Connection Closed");
            HubState = HubConnection.State;

            await Start();
        }
        private Task OnReconnecting(Exception error)
        {
            if (error != null)
            {
                HubLog(error.Message);
            }
            HubLog("Reconnecting");
            HubState = HubConnection.State;
            return Task.CompletedTask;
        }
        private Task OnReconnected(string message)
        {
            if (message != null)
            {
                Logger.Information(message);
            }
            HubLog("Reconnected");

            HubState = HubConnection.State;
            UpdateWorkerStateChanged();
            return Task.CompletedTask;
        }
        private Task HandleReceivedMessage(string message)
        {
            HubLog(message);
            return Task.CompletedTask;
        }

        #endregion

        #region State

        private WorkerState _state = WorkerState.Idle;
        public WorkerState State
        {
            get { return _state; }
            set
            {
                _state = value;
                WorkerStateChanged?.Invoke(value);
                UpdateWorkerStateChanged();
                Logger?.Information($"WorkerState:{_state}");
            }
        }
        public event Func<WorkerState, Task> WorkerStateChanged;
        #endregion

        #region Utilities
        public string LocalProcessFolder { get; set; }
        public string LocalSuccessFolder { get; set; }
        public string LocalFailureFolder { get; set; }

        void CreateWorkingFolder()
        {
            LocalProcessFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkingFolder", "Process");
            LocalSuccessFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkingFolder", "Success");
            LocalFailureFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkingFolder", "Failure");

            CreateFolderIfNotExists(LocalProcessFolder);
            CreateFolderIfNotExists(LocalSuccessFolder);
            CreateFolderIfNotExists(LocalFailureFolder);

        }
        public void CreateFolderIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public virtual void UpdateWorkerStateChanged()
        {
            Logger.Information($"State Changed: {State}");
            if (ServerConnected())
            {
                HubConnection.SendAsync(WorkerActions.WorkerStateChanged, State);
            }
        }
        public bool ServerConnected()
        {
            return HubConnection.State == HubConnectionState.Connected;
        }
        #endregion

        #region Logger
        public ILogger Logger { get; private set; }
        public ILogger ProcessLogger { get; private set; }

        void LoggerSetup()
        {
            Logger = new LoggerConfiguration()
                .WriteTo.File(path: AppDomain.CurrentDomain.BaseDirectory + "/Log/log.txt",
                      rollingInterval: RollingInterval.Day,
                      rollOnFileSizeLimit: true,
                      retainedFileCountLimit: 30,
                      shared: true)
                .CreateLogger();
            ProcessLogger = new LoggerConfiguration()
                .WriteTo.File(path: AppDomain.CurrentDomain.BaseDirectory + "/ProcessLog/log.txt",
                      rollingInterval: RollingInterval.Day,
                      rollOnFileSizeLimit: true,
                      retainedFileCountLimit: 30,
                      shared: true)
                .CreateLogger();
        }


        #endregion

        public event Action AutoUpdateHandler;
    }
}
