using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SCC.Lib;
using SCCWorker.Model;

namespace SCCWorker.SCCServices
{
    public class SCCServices
    {
        public SCCServices()
        {

        }
        public async Task<T_SCC_Order> GetOrder(int key)
        {
            using (var db = new SCCContext())
            {
                var data = await db.T_SCC_Orders.Where(x => x.Id== key).FirstOrDefaultAsync();
                return data;
            }
        }
        public async Task<List<T_SCC_Pilot1_OrderInfo>> GetPilot1OrderInfo(int orderId)
        {
            using (var db = new SCCContext())
            {
                var data = await db.T_SCC_Pilot1_OrderInfos.Where(x => x.OrderId == orderId).ToListAsync();
                return data;
            }
        }
        public async Task<List<T_SCC_Pilot1_OrderInfoDetail>> GetPilot1OrderInfoDetail(Guid InfoGuid)
        {
            using (var db = new SCCContext())
            {
                var data = await db.T_SCC_Pilot1_OrderInfoDetails.Where(x => x.InfoGuid == InfoGuid).ToListAsync();
                return data;
            }
        }
        public async void UpdatePilot1OrderInfor(int key, string status)
        {
            using (var db = new SCCContext())
            {
                var data = await db.T_SCC_Pilot1_OrderInfos.Where(x => x.Id == key).FirstOrDefaultAsync();
                if(data == null)  return;
                data.Status = status;
                await db.SaveChangesAsync();
            }
        }
        public async Task<List<T_SCC_Pilot2_OrderInfo>> GetPilot2OrderInfo(int orderId)
        {
            using (var db = new SCCContext())
            {
                var data = await db.T_SCC_Pilot2_OrderInfos.Where(x => x.OrderId == orderId).ToListAsync();
                return data;
            }
        }
        public async void SaveLog(int orderId, string message)
        {
            using (var db = new SCCContext())
            {
                var newLog = new T_SCC_LogHistory();
                newLog.OrderId = orderId;
                newLog.LogMessage= message;
                newLog.LogTime = DateTime.Now;
                db.T_SCC_LogHistories.Add(newLog);
                await db.SaveChangesAsync();
            }
        }
    }
}
