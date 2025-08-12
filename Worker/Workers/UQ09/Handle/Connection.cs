using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQ09.Model;

namespace UQ09.Handle
{
    public class Connection
    {
        private static IConfiguration _iconfiguration;
        private GetDB _get; 
        public Connection()
        {
            GetAppSettingFile();
            _get = new GetDB(_iconfiguration);
        }
        static void GetAppSettingFile()
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _iconfiguration = builder.Build();
        }
        public (int, List<MaterialProduction>) GetData(string area)
        {
            return _get.GetDetail(area);
        }
        public (int, List<MaterialPO>) GetMCDData(string area)
        {
            return _get.GetMCDDetail(area);
        }
        public bool MarkAsDone(int productionId)
        {
            return _get.MarkAsDone(productionId);
        }
        public bool MarkAsDoneMCD(int mcdId)
        {
            return _get.MarkAsDoneMCD(mcdId);
        }
    }
}
