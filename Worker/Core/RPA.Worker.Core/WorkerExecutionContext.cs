using Microsoft.AspNetCore.SignalR.Client;
using RPA.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RPA.Worker.Core
{
    public class WorkerExecutionContext<T> where T : WorkerOption
    {
        public WorkerExecutionContext(HubConnection hub)
        {
            Hub = hub;
            Hub.On<string, string>(WorkerActions.SAPSessionCreated, SAPSessionCreated);
        }
        public string JsonData { get; set; }
        public T WorkerOptions { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string ErrorMessage { get; set; }
        public List<string> Logs { get; set; } = new List<string>();
        public double ElapsedSeconds { get; set; }
        public Stopwatch StopWatch { get; set; } = new Stopwatch();
        public ConcurrentDictionary<string, string> SAPSessionID { get; set; } = new ConcurrentDictionary<string, string>();
        public HubConnection Hub { get; }

        public event Func<string, Task> WorkerExecutionLogAdd;
        public event Func<WorkerState, Task> WorkerStateChange;

        public void Log(string message)
        {
            Logs.Add(message);
            if (WorkerExecutionLogAdd != null)
            {
                WorkerExecutionLogAdd.Invoke(message);
            }
        }
        /// <summary>
        /// Used in VB.NET since Event Handler can't do inline
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public Task TaskLog(string msg)
        {
            Log(msg);
            return Task.CompletedTask;
        }

        #region Request Printing
        /// <summary>
        /// Request a print queue by Print Worker
        /// Eg: When request print a document in SAP
        /// We cannot catch the print request from it or from system so
        /// send a request to Print Worker to do it instead
        /// </summary>
        /// <returns></returns>
        private async Task RequestPrint()
        {
            await Hub.SendAsync(WorkerActions.RequestNewSAPSession);
        }
        #endregion

        #region Request SAP Session
        /// <summary>
        /// Request a new session from SAP Session Manager
        /// Send a request to server Hub then wait for answer from sever in 
        /// other thread by checking SAPSessionID
        /// </summary>
        /// <returns></returns>
        public async Task<string> RequestSAPSession(string contextId = "")
        {
            if (Hub.State == HubConnectionState.Connected)
            {
                await RequestUpdateWorkerState(WorkerState.WaitingForSAPSessions);
                await Hub.SendAsync(WorkerActions.RequestNewSAPSession, contextId);
                string sessionId;
                while (!SAPSessionID.TryGetValue(contextId, out sessionId))
                {
                    await Task.Delay(2000);
                }
                await RequestUpdateWorkerState(WorkerState.Executing);
                return sessionId;
            }
            else
            {
                await Task.Delay(2000);
                return await RequestSAPSession(contextId);
            }

        }



        /// <summary>
        /// Received new SAP session ID from server. Mark as finished
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        private Task SAPSessionCreated(string sessionID, string contextId = "")
        {
            SAPSessionID.TryAdd(contextId, sessionID);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Close session when it's done
        /// </summary>
        /// <returns></returns>
        public async Task RequestCloseSAPSession(string sessionId = "", string contextId = "")
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return;
            }
            if (Hub.State == HubConnectionState.Connected)
            {
                await Hub.SendAsync(WorkerActions.CloseSAPSession, sessionId);
                SAPSessionID.TryRemove(contextId, out string _);
            }
            else
            {
                await Task.Delay(2000);
                await RequestCloseSAPSession(sessionId, contextId);
            }
        }
        #endregion

        public async Task RequestUpdateWorkerState(WorkerState state)
        {
            if (WorkerStateChange != null)
            {
                await WorkerStateChange.Invoke(state);
            }
        }
    }
}
