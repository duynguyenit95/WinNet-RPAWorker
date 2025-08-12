using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using RPA.Core;
using RPA.Core.Data;
using RPA.Web.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace RPA.Web.Hubs
{
    //public class SAPHub : WorkerHubBase
    //{
    //    public SAPHub(IConfiguration configuration) : base(configuration)
    //    {
    //    }

        //[HubMethodName(WorkerActions.WorkerStateChanged)]
        //public async Task HandleWorkerStatusChanged(string statusInfo)
        //{
        //    var infos = statusInfo.Split(";");
        //    // Update connection state
        //    var workerConnection = _connectionFactory.GetHubConnectionByID(Context.ConnectionId);
        //    workerConnection.State = infos[0];
        //    if (infos.Length > 1)
        //    {
        //        workerConnection.NextRunTime = infos[1];
        //        workerConnection.LastRunResult = infos[2];
        //    }
        //    foreach (var group in workerConnection.Groups)
        //    {
        //        // Request update to all web user
        //        await User_WorkerUpdate(group, WorkerActions.WorkerStateChanged, workerConnection);
        //    }
        //}

        //#region SAP Session
        //[HubMethodName(WorkerActions.RequestNewSAPSession)]
        //public async Task WorkerRequestSAPSession()
        //{
        //    // find SAP Session Manager In Group
        //    var workerConnection = _connectionFactory.GetByConnectionId(Context.ConnectionId);
        //    var sapSessionManager = _connectionFactory.HubConnections
        //                            .Where(x => x.Value.Device == workerConnection.Device
        //                                    && x.Value.Name == WorkerNames.SAPSessionManager)
        //                            .FirstOrDefault();
        //    if (sapSessionManager.Key != default)
        //    {
        //        var newqueue = new SimpleQueueItem()
        //        {
        //            Name = WorkerActions.RequestNewSAPSession,
        //            Data = Context.ConnectionId
        //        };
        //        await Clients.Client(sapSessionManager.Key)
        //                    .SendAsync(WorkerActions.AddQueue, newqueue, false);
        //    }
        //}

        //[HubMethodName(WorkerActions.SAPSessionCreated)]
        //public async Task WorkerSAPSessionCreated(string requestConnectionId, string newSessionId)
        //{
        //    await Clients.Client(requestConnectionId)
        //                //Request connection ID
        //                .SendAsync(WorkerActions.SAPSessionCreated, newSessionId);
        //}

        //[HubMethodName(WorkerActions.CloseSAPSession)]
        //public async Task RequestCloseSAPSession(string sessionID)
        //{
        //    // find SAP Session Manager In Group
        //    var workerConnection = _connectionFactory.GetHubConnectionByID(Context.ConnectionId);
        //    var sapSessionManager = _connectionFactory.HubConnections
        //                            .Where(x => x.Value.Device == workerConnection.Device
        //                                    && x.Value.Name == WorkerNames.SAPSessionManager)
        //                            .FirstOrDefault();
        //    if (sapSessionManager.Key != default)
        //    {
        //        var newqueue = new SimpleQueueItem()
        //        {
        //            Name = WorkerActions.CloseSAPSession,
        //            Data = sessionID
        //        };
        //        await Clients.Client(sapSessionManager.Key)
        //                    .SendAsync(WorkerActions.AddQueue, newqueue, true);
        //    }
        //}
        //#endregion
   // }
}
