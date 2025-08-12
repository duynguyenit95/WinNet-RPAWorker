using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using RPA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace RPA.Web.Hubs
{
    public class WorkerHubBase : Hub
    {
        public readonly WorkerConnectionFactory _connectionFactory;
        public readonly string _workerToken;

        public WorkerHubBase(IConfiguration configuration,
            WorkerConnectionFactory connectionFactory) : base()
        {
            _connectionFactory = connectionFactory;
            _workerToken = configuration["WorkerToken"];
        }


        #region Connect/Disconnect
        /// <summary>
        /// Check if connection is a worker using worker token
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        bool IsWorker(HttpContext httpContext)
        {
            var requestToken = httpContext.Request.Headers["Token"].ToString();
            return requestToken == _workerToken;
        }
        /// <summary>
        /// Check if connection is a login user
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        bool IsUser(HttpContext httpContext)
        {
            return !string.IsNullOrEmpty(httpContext.User.Identity.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual async Task<(WorkerConnection connect, object data)> HandleConnected()
        {
            var http = Context.GetHttpContext();
            var isUser = IsUser(http);
            // Return if it's not worker with token or user
            var connect = new WorkerConnection()
            {
                ConnectionId = Context.ConnectionId,
                Groups = new List<string> { }
            };

            if (IsWorker(http))
            {
                connect.WorkerId = Guid.Parse(http.Request.Headers["WorkerId"].ToString());
                connect.Device = http.Request.Headers["DeviceID"].ToString();
                connect.Name = http.Request.Headers["WorkerName"].ToString();
                connect.Version = http.Request.Headers["Version"].ToString();
                connect.BaseType = (WorkerBaseType)Enum.Parse(typeof(WorkerBaseType), http.Request.Headers["BaseType"].ToString());
                connect.ConnectionUnit = ConstValue.WorkerGroup;
                connect.Groups.Add(ConstValue.WorkerCentral);
            }
            // Identity User
            else if (isUser)
            {
                connect.Device = ConstValue.UserGroup;
                connect.Name = http.User.Identity.Name;
                connect.ConnectionUnit = ConstValue.UserGroup;
            }
            else
            {
                connect.Device = ConstValue.UnknownGroup;
                connect.Name = ConstValue.UnknownGroup;
                connect.ConnectionUnit = ConstValue.UnknownGroup;
            }


            var viewGroup = http.Request.Query["Group"].ToString();
            if (!string.IsNullOrEmpty(viewGroup))
            {
                connect.Groups.Add(viewGroup);
            }

            return (connect, null);
        }
        private async Task HandleNewHubConnection(WorkerConnection connect, object data = null)
        {
            // Add to monitor service
            _connectionFactory.AddHubConnection(connect.ConnectionId, connect);
            foreach (var group in connect.Groups)
            {
                // Join group
                await JoinGroup(group, connect.Name);
                // Notify to online user
                await GroupUpdate(group, WorkerActions.WorkerOnline, data ?? connect);
            }
        }
        public override async Task OnConnectedAsync()
        {
            var (connect, data) = await HandleConnected();
            await HandleNewHubConnection(connect, new { connect, data });
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Remove from monitor service
            var connect = _connectionFactory.RemoveHubConnection(Context.ConnectionId);
            object data = null;
            if (connect != null)
            {
                foreach (var group in connect.Groups)
                {
                    // Leave Worker Machine Group
                    await LeaveGroup(group, connect.Name);
                    await GroupUpdate(group, WorkerActions.WorkerOffline, new { connect, data });
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region Data Exchange

        [HubMethodName(WorkerActions.CutMachineDataUpdate)]


        /// <summary>
        /// Notify User about Worker Update
        /// </summary>
        /// <param name="action"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task GroupUpdate(string group, string action, object data)
        {
            // Request update to all web user
            await Clients
                .Group(group)
                .SendAsync(WorkerActions.WorkerUpdate, new
                {
                    Action = action,
                    Data = data
                });
        }

        [HubMethodName(WorkerActions.WorkerStateChanged)]
        public async Task HandleWorkerStateChanged(WorkerState state)
        {
            // Update connection state
            var workerConnection = _connectionFactory.GetByConnectionId(Context.ConnectionId);
            workerConnection.WorkerState = state;
            foreach (var group in workerConnection.Groups)
            {
                // Request update to all web user
                await GroupUpdate(group, WorkerActions.WorkerStateChanged, workerConnection);
            }
        }
        #endregion

        #region Option Validation
        [HubMethodName(WorkerActions.ValidateOptionsResult)]
        public async Task HandleValidateOptionsResult(string requestConnectionId, WorkerOptionValidateResult data)
        {
            await Clients.Client(requestConnectionId)
                .SendAsync(WorkerActions.WorkerUpdate,
                new
                {
                    Action = WorkerActions.ValidateOptionsResult,
                    Data = data
                });

        }
        #endregion


        #region SAP Session
        [HubMethodName(WorkerActions.RequestNewSAPSession)]
        public async Task WorkerRequestSAPSession(string contextId = "")
        {
            // find SAP Session Manager In Group
            var workerConnection = _connectionFactory.GetByConnectionId(Context.ConnectionId);
            var sapSessionManager = _connectionFactory.GetConnections()
                                    .Where(x => x.Device == workerConnection.Device
                                            && x.Name == WorkerNames.SAPSessionManager)
                                    .FirstOrDefault();
            if (sapSessionManager != null)
            {
                var newqueue = new SimpleQueueItem()
                {
                    Name = WorkerActions.RequestNewSAPSession,
                    Data = Context.ConnectionId,
                    ContextId = contextId
                };
                await Clients.Client(sapSessionManager.ConnectionId)
                            .SendAsync(WorkerActions.AddQueue, newqueue, false);
            }
        }

        [HubMethodName(WorkerActions.SAPError)]
        public async Task SAPError(string message)
        {
            await Clients.All.SendAsync(WorkerActions.SAPError, message);
        }

        [HubMethodName(WorkerActions.SAPSessionCreated)]
        public async Task WorkerSAPSessionCreated(string requestConnectionId, string newSessionId, string contextId)
        {
            await Clients.Client(requestConnectionId)
                        //Request connection ID
                        .SendAsync(WorkerActions.SAPSessionCreated, newSessionId, contextId);
        }

        [HubMethodName(WorkerActions.CloseSAPSession)]
        public async Task RequestCloseSAPSession(string sessionID)
        {
            // find SAP Session Manager In Group
            var workerConnection = _connectionFactory.GetByConnectionId(Context.ConnectionId);
            var sapSessionManager = _connectionFactory.GetConnections()
                                    .Where(x => x.Device == workerConnection.Device
                                            && x.Name == WorkerNames.SAPSessionManager)
                                    .FirstOrDefault();
            if (sapSessionManager != null)
            {
                var newqueue = new SimpleQueueItem()
                {
                    Name = WorkerActions.CloseSAPSession,
                    Data = sessionID
                };
                await Clients.Client(sapSessionManager.ConnectionId)
                            .SendAsync(WorkerActions.AddQueue, newqueue, true);
            }
        }
        #endregion


        #region Default
        public Task Send(string name, string message)
        {
            return Clients.All.SendAsync("Send", $"{name}: {message}");
        }

        public Task SendToOthers(string name, string message)
        {
            return Clients.Others.SendAsync("Send", $"{name}: {message}");
        }

        public Task SendToConnection(string connectionId, string name, string message)
        {
            return Clients.Client(connectionId).SendAsync("Send", $"Private message from {name}: {message}");
        }

        public Task SendToGroup(string groupName, string name, string message)
        {
            return Clients.Group(groupName).SendAsync("Send", $"{name}@{groupName}: {message}");
        }

        public Task SendToOthersInGroup(string groupName, string name, string message)
        {
            return Clients.OthersInGroup(groupName).SendAsync("Send", $"{name}@{groupName}: {message}");
        }

        public async Task JoinGroup(string groupName, string name)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Send", $"{name} joined {groupName}");
        }

        public async Task LeaveGroup(string groupName, string name)
        {
            await Clients.Group(groupName).SendAsync("Send", $"{name} left {groupName}");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public Task Echo(string name, string message)
        {
            return Clients.Caller.SendAsync("Send", $"{name}: {message}");
        }
        #endregion

    }
}
