using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using UQ09.Enum;
using UQ09.Handle;
using UQ09.Model;
namespace UQ09
{
    public static class Program
    {
        private static Collector _collector = new Collector();
        private static Direction _direction = new Direction();
        private static FileHandle _fileHandler = new FileHandle();
        private static Mail _mail = new Mail();

        public static async Task Main(string[] args)
        {
            try
            {
                string side = args.Length > 0 ? args[0] : "VN";
                if (side == "VN" || side == "CN")
                {
                    Console.WriteLine($"Side: {side}");
                }
                else
                {
                    Console.WriteLine("Invalid side");
                    return;
                }

                var param = _collector.GetParam();
                UQAction action = _direction.WhatDo();
                Console.WriteLine($"Action: {action}");
                var merge = _collector.GetMergeParam();

                if (!ValidateParameters(param, merge, out var vnSide, out var cnSide))
                    return;
                if (side == "VN")
                {
                    await SingleProcess(vnSide, action);
                }
                else
                {
                    await SingleProcess(cnSide, action);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                await _mail.SendMailResult(ex.Message + ex.StackTrace);
            }
        }
        private static bool ValidateParameters(
            List<UQParam> param,
            UQMergeParam merge,
            out UQParam vnSide,
            out UQParam cnSide)
        {
            vnSide = param.Find(x => x.Area == UQArea.VN);
            cnSide = param.Find(x => x.Area == UQArea.CN);

            if (vnSide == null || cnSide == null)
            {
                _mail.SendMailResult("JSONParam is not correct", "", "", true).Wait();
                return false;
            }

            if ((merge.UQ09 && !(vnSide.UQ09 && cnSide.UQ09)) ||
                (merge.UQ09Production && !(vnSide.UQ09Production && cnSide.UQ09Production)))
            {
                _mail.SendMailResult("MergeParam is not correct", "", "", true).Wait();
                return false;
            }

            return true;
        }
        private static async Task SingleProcess(UQParam side, UQAction action)
        {
            var materialPO = await Start_UQ(side, action) ?? new List<MaterialPO>();
            var materialProduction = await Start_UQ_Production(side, action) ?? new List<MaterialProduction>();
            await SaveIndividual(materialPO, side, "MaterialPO", "UQ09_Merge");
            await SaveIndividual(materialProduction, side, "MaterialProductionInfo", "UQ09_Merge");
        }

        private static async Task ProcessMaterialPO(UQParam vnSide, UQParam cnSide, UQAction action, UQMergeParam merge)
        {
            var vnMaterial = await Start_UQ(vnSide, action) ?? new List<MaterialPO>();
            var cnMaterial = await Start_UQ(cnSide, action) ?? new List<MaterialPO>();
           
            if (merge.UQ09 && vnMaterial.Count > 0 && cnMaterial.Count > 0)
            {
                await MergeAndSave(vnMaterial.Concat(cnMaterial).ToList(), vnSide, merge, "MaterialPO", "UQ09_Merge");
            }
            else
            {
                await SaveIndividual(vnMaterial, vnSide, "MaterialPO", "UQ09_Merge");
                await SaveIndividual(cnMaterial, cnSide, "MaterialPO", "UQ09_Merge");
            }
        }

        private static async Task ProcessMaterialProduction(UQParam vnSide, UQParam cnSide, UQAction action, UQMergeParam merge)
        {
            var vnMaterialProduction = await Start_UQ_Production(vnSide, action) ?? new List<MaterialProduction>();
            var cnMaterialProduction = await Start_UQ_Production(cnSide, action) ?? new List<MaterialProduction>();

            if (merge.UQ09Production && vnMaterialProduction.Count > 0 && cnMaterialProduction.Count > 0)
            {
                await MergeAndSave(vnMaterialProduction.Concat(cnMaterialProduction).ToList(), vnSide, merge, "MaterialProductionInfo", "UQ09Production_Merge");
            }
            else
            {
                await SaveIndividual(vnMaterialProduction, vnSide, "MaterialProductionInfo", "UQ09_Merge");
                await SaveIndividual(cnMaterialProduction, cnSide, "MaterialProductionInfo", "UQ09_Merge");
            }
        }
        private static async Task MergeAndSave<T>(List<T> data, UQParam side, UQMergeParam merge, string fileNamePrefix, string destinationFolder)
        {
            var resultPath = _fileHandler.SaveCSV(merge.ResultPath, data, $"{fileNamePrefix}_{DateTime.Now:yyyyMMddHHmmssfff}.csv");
            await _fileHandler.CopyFile(resultPath, side.DestinationPath, destinationFolder);
        }

        static async Task SaveIndividual<T>(List<T> data, UQParam side, string fileNamePrefix, string destinationFolder)
        {
            if (data.Count > 0)
            {
                var resultPath = _fileHandler.SaveCSV(side.RequestPath, data, $"{fileNamePrefix}_{DateTime.Now:yyyyMMddHHmmssfff}.csv");
                await _fileHandler.CopyFile(resultPath, side.DestinationPath, destinationFolder);
            }
        }
        static async Task<List<MaterialPO>?> Start_UQ(UQParam uq, UQAction action)
        {
            string area = uq.Area.ToString();
            string step = $"{area}_UQ09_MCD";
            Console.WriteLine($"Step: {step}");
            try
            {
                if (action != UQAction.CheckFormat)
                {
                    Console.WriteLine("No action");
                    return null;
                }
                Console.WriteLine($"Connect to DB");
                var conn = new Connection();
                Console.WriteLine($"Get data");
                var (mcdId, data) = conn.GetMCDData(area);
                if (data.Count > 0)
                {
                    _fileHandler.SaveCSV(uq.ResultPath, data, $"MaterialPO_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.csv");
                    conn.MarkAsDoneMCD(mcdId);
                    return data;
                }
                else
                {
                    Console.WriteLine($"{area}: Không có dữ liệu", step);
                    await _mail.SendMailResult($"{area}: Không có dữ liệu", step);
                    return null;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _mail.SendMailResult(ex.Message + ex.StackTrace, step, "", true);
                return null;
            }

        }
        static async Task<List<MaterialProduction>?> Start_UQ_Production(UQParam uq, UQAction action)
        {
            string area = uq.Area.ToString();
            string step = $"{area}_UQ09_Production";
            Console.WriteLine($"Step: {step}");
            try
            {
                if (action != UQAction.CheckFormat)
                {
                    Console.WriteLine("No action");
                    return null;
                }
                Console.WriteLine($"Connect to DB");
                var conn = new Connection();
                Console.WriteLine($"Get data");
                var (productionId, data) = conn.GetData(area);
                if (data.Count > 0)
                {
                    _fileHandler.SaveCSV(uq.ResultPath, data, $"MaterialProductionInfo_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.csv");
                    conn.MarkAsDone(productionId);
                    return data;
                }
                else
                {
                    Console.WriteLine($"{area}: Không có dữ liệu", step);
                    await _mail.SendMailResult($"{area}: Không có dữ liệu", step);
                    return null;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _mail.SendMailResult(ex.Message + ex.StackTrace, step, "", true);
                return null;
            }


        }
    }
}
