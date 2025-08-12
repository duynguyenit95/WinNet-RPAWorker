using RPA.Core;
using RPA.Worker.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCifs.Smb;
using RPA.Tools;
using System.IO.Compression;
using CsvHelper.Configuration;
using CsvHelper;
//using Sha
namespace RPA12.Lib
{
    public class ZTF003Output
    {
        public string WERKS { private get; set; }
        public string Factory { get;  set; }
        public string PO { get; set; }
        public string MaterialNo { get; set; }
        public string Color { get; set; }
        public decimal Quantity { get; set; }
        public DateTime ProductionPlanDate { get; set; }
        public decimal AmountHKD { get; set; }

        public void SetFactory()
        {
            Factory = ConfigFactory();
        }

        private string ConfigFactory()
        {
            string factory = WERKS;
            if(WERKS == "8103")
            {
                var pStart = PO.Substring(0, 3);
                if(pStart == "570" || pStart == "574")
                {
                    factory = "2504";
                }
                else if (pStart == "591" || pStart == "598")
                {
                    factory = "2505";
                }
                else if (pStart == "551" || pStart == "558")
                {
                    factory = "2502";
                }
                else if (pStart == "731" || pStart == "738")
                {
                    factory = "2506";
                }
            }
            switch (factory)
            {
                case "2502":
                    {
                        return "A厂";
                    }
                case "2504":
                    {
                        return "C厂";
                    }
                case "2505":
                    {
                        return "D厂";
                    }
                case "2506":
                    {
                        return "E厂";
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }
    }
}