using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using RPA.Core;
using System.Collections.Generic;
using System.Text.Json;

namespace RPA.Worker.Core
{
    /// <summary>
    /// Worker will run by schedule or requested by user
    /// </summary>
    public class TimerWorker<T> : RPAWorkerCore<T> where T : WorkerOption
    {
        private int _debugSleepTime = 0;
        public TimerWorker(ServerOption serverOption) : base(serverOption, WorkerBaseType.Timer)
        {
#if DEBUG
            _debugSleepTime = 2000;
#endif
            WorkerOptionLoaded += TimerWorker_WorkerOptionLoaded;
        }

        private Task TimerWorker_WorkerOptionLoaded()
        {
            if (WorkerOption.ScheduleOpt != null)
            {
                TimerSetup(WorkerOption.ScheduleOpt);
            }
            return Task.CompletedTask;
        }

        public override void HandleSAPError(string message)
        {
            base.HandleSAPError(message);
        }

        #region Hub
        public override void HubClientSetup()
        {
            base.HubClientSetup();
            HubConnection.On<string>(WorkerActions.Execute, Execute);
        }
        #endregion

        #region Execution
        public ExecutionResult ExecutionResult { get; private set; } = ExecutionResult.Unknown;
        public event Func<string, Task> ExecutionLogAdded;

        public async void Execute(string jsonData)
        {
            if (State == WorkerState.Executing)
            {
                HubLog("Worker is running ! Please wait it complete");
                return;
            }
            if (!ServerConnected())
            {
                HubLog("Cannot execute when not connected to server ");
                return;
            }
            var context = new WorkerExecutionContext<T>(HubConnection);
            context.JsonData = jsonData;
            context.WorkerOptions = WorkerOption;
            context.WorkerExecutionLogAdd += ExecutionLogAdded;
            context.WorkerExecutionLogAdd += (msg) =>
            {
                ProcessLogger.Information(msg);
                return Task.CompletedTask;
            };
            context.WorkerStateChange += Context_WorkerStateChange;
            await StartExecute(context);
        }

        private Task Context_WorkerStateChange(WorkerState state)
        {
            Logger?.Information("Context_WorkerStateChange");
            State = state;
            return Task.CompletedTask;
        }

        public async Task StartExecute(WorkerExecutionContext<T> context)
        {
            Logger?.Information("StartExecute");
            State = WorkerState.Executing;
            context.StopWatch.Start();
#if !DEBUG
            try
            {
#endif
            context = await PreExecute(context);
            context = await ExecutionHandler(context);
            context = await AfterExecute(context);
#if !DEBUG
            }
            catch (Exception ex)
            {
                context.IsSuccess = false;
                context.Log($"Error:{ex}");
            }
#endif


            if (context.IsSuccess)
            {
                ExecutionResult = ExecutionResult.Success;
                context.Log($"Success !!!!!");
            }
            else
            {
                ExecutionResult = ExecutionResult.Failure;
                context.Log($"Error !!!!!");
            }
            Logger?.Information("End Executed");
            State = WorkerState.Idle;
            context.StopWatch.Stop();
            context.Log($"Execution Time {context.StopWatch.Elapsed.TotalSeconds}");
            context.ElapsedSeconds = context.StopWatch.Elapsed.TotalSeconds;

            _ = SaveLog(context);
        }
        public virtual async Task<WorkerExecutionContext<T>> PreExecute(WorkerExecutionContext<T> context)
        {
            context.Log("PreExecute");
            await Sleep();
            return context;
        }
        public virtual async Task<WorkerExecutionContext<T>> ExecutionHandler(WorkerExecutionContext<T> context)
        {
            context.Log("Execution");
            await Sleep();
            return context;
        }
        public virtual async Task<WorkerExecutionContext<T>> AfterExecute(WorkerExecutionContext<T> context)
        {
            context.Log("AfterExecute");
            await Sleep();
            return context;
        }
        private async Task SaveLog(WorkerExecutionContext<T> context)
        {
            await Task.Delay(_debugSleepTime);
        }
        #endregion

        #region Timer
        public ScheduleTimer WorkerTimer { get; set; }
        public DateTime NextRunTime => GetNextRunTime();
        DateTime GetNextRunTime()
        {
            if (WorkerTimer != null)
            {
                return WorkerTimer.NextRunTime;
            }
            else
            {
                return DateTime.MaxValue;
            }
        }
        public event Func<DateTime, Task> NextRunTimeChanged;
        void TimerSetup(List<ScheduleOptions> schedule = null)
        {
            if (WorkerTimer is null)
            {
                WorkerTimer = new ScheduleTimer();
                WorkerTimer.NextRunTimeUpdate += UpdateNextRunTime;
                WorkerTimer.TimerHandler = (jsonParam) =>
                {
                    Execute(jsonParam);
                    return Task.CompletedTask;
                };
            }
            WorkerTimer.UpdateScheduleOptions(schedule);
        }
        private async Task UpdateNextRunTime(DateTime value)
        {
            await NextRunTimeChanged.Invoke(value);
            await UpdateData();
        }
        #endregion

        #region Utilities
        private async Task Sleep()
        {
            if (_debugSleepTime > 0)
            {
                Logger.Information($"Sleep");
                await Task.Delay(_debugSleepTime);
            }
        }
        public async Task UpdateData()
        {
            if (ServerConnected())
            {
                await HubConnection.SendAsync(WorkerActions.WorkerDataUpdate, JsonSerializer.Serialize(new { NextRunTime, ExecutionResult }));
            }
        }
        #endregion
    }
}
