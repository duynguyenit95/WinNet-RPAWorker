using MIR7InvoiceWorker.Model;
using MIR7Invoice.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPA.Core.Models.SAP.ZACCOUNT;

namespace MIR7Invoice.Services
{
    public class MIR7InvoiceServices
    {
        public MIR7InvoiceServices()
        {

        }
        private async Task<bool> ExecuteWithRetryAsync(Func<Task> action, int maxRetries = 3, int delayMilliseconds = 5000)
        {
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    await action();
                    return true; // Return true if the action is successful
                }
                catch
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        return false; // Return false after all retries have been exhausted
                    }
                    else
                    {
                        await Task.Delay(delayMilliseconds); // Wait before retrying
                    }
                }
            }

            return false; // In case of unexpected loop exit
        }

        public async Task<bool> MIR7RecordProcess(PID135_InvoiceResult input)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                using (var db = new MIR7InvoiceContext())
                {
                    db.PID135_InvoiceResults.Add(input);
                    db.SaveChanges();
                }
            });
        }
        public async Task<bool> ZMMR0005RecordResult(List<PID135_ZMMR0005Result> results)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                using (var db = new MIR7InvoiceContext())
                {
                    db.PID135_ZMMR0005Results.AddRange(results);
                    db.SaveChanges();
                }
            });
        }

        public async Task<bool> ZAccountRecord(List<ZAccountResult> results)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                using (var db = new MIR7InvoiceContext())
                {
                    //Remove all existing records and add new ones
                    db.ZAccountResults.RemoveRange(db.ZAccountResults);
                    db.ZAccountResults.AddRange(results);
                    await db.SaveChangesAsync();
                }
            });
        }
    }
}
