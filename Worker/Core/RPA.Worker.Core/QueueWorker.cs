using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using RPA.Core;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Abstractions;
namespace RPA.Worker.Core
{
    /// <summary>
    /// Queue worker receive queue task from Server 
    /// Process each task one by one
    /// </summary>
    public class QueueWorker<T> : RPAWorkerCore<T> where T : WorkerOption
    {
        private readonly ConcurrentQueue<SimpleQueueItem> _queueItems = new ConcurrentQueue<SimpleQueueItem>();
        private readonly ConcurrentQueue<SimpleQueueItem> _priorityQueueItems = new ConcurrentQueue<SimpleQueueItem>();
        public bool StopOnError { get; set; } = true;
        public QueueWorker(ServerOption serverOption) : base(serverOption, WorkerBaseType.Queue)
        {

        }

        public override void HandleSAPError(string message)
        {
            base.HandleSAPError(message);
        }

        #region Hub
        public override void HubClientSetup()
        {
            base.HubClientSetup();
            HubConnection.On<SimpleQueueItem, bool>(WorkerActions.AddQueue, AddQueue);
            Logger?.Information("HubClientSetup");
            State = WorkerState.Queueing;
            _ = Run();
        }
        #endregion
        public void AddQueue(SimpleQueueItem item, bool isPriority)
        {
            if (isPriority)
            {
                _priorityQueueItems.Enqueue(item);
                HubLog($"New Priority Queue Added: {item}");
            }
            else
            {
                _queueItems.Enqueue(item);
                HubLog($"New Queue Added: {item}");
            }
        }

        private async Task Run()
        {
            while (true)
            {
                if (State == WorkerState.Queueing)
                {
                    // Process normal queue if no priorty item
                    if(await HandleQueueItem(_priorityQueueItems) == null)
                    {
                        await HandleQueueItem(_queueItems);
                    }
                }
                await Task.Delay(2000);
            }
        }

        private async Task<SimpleQueueItem> HandleQueueItem(ConcurrentQueue<SimpleQueueItem> queue)
        {
            if (queue.TryPeek(out SimpleQueueItem item))
            {
                try
                {
                    HubLog($"Process Queue: {item}");
                    await ProcessQueue(item);
                    HubLog($"Complete: {item}");
                }
                catch(Exception ex)
                {
                    HubLog(ex.ToString());
                    NotifyQueueProcessError(ex.ToString());
                }
                if (queue.TryDequeue(out _))
                {
                    HubLog($"Dequeue: {item}");
                }
                return item;
            }
            return null;
        }

        private void NotifyQueueProcessError(string v)
        {

        }

        private void NotifyQueueError(string v)
        {

        }

        public virtual Task ProcessQueue(SimpleQueueItem inputData)
        {
            return Task.CompletedTask;
        }

    }
}
