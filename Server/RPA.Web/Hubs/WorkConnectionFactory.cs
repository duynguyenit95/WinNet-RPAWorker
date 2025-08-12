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
    public class WorkerConnectionFactory
    {
        private readonly ConcurrentDictionary<string, WorkerConnection> _workerConnections;

        public WorkerConnectionFactory()
        {
            _workerConnections = new ConcurrentDictionary<string, WorkerConnection>();
        }
        public WorkerConnection AddHubConnection(string connectionID, WorkerConnection client)
        {
            if (_workerConnections.TryAdd(connectionID, client))
            {
                return client;
            }
            return null;
        }

        public WorkerConnection RemoveHubConnection(string connectionID)
        {
            if (_workerConnections.TryRemove(connectionID, out WorkerConnection hubConnectionInfo))
            {
                return hubConnectionInfo;
            }
            else
            {
                return null;
            }
        }

        public WorkerConnection GetByConnectionId(string connectionID)
        {
            if (_workerConnections.Any(x => x.Key == connectionID))
            {
                return _workerConnections[connectionID];
            }
            return null;
        }

        public IEnumerable<WorkerConnection> GetConnections()
        {
            return _workerConnections.Values;
        }

    }





}
