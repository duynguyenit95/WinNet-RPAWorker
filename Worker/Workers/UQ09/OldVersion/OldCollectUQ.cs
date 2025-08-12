using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQ09.Enum;
using UQ09.Handle;
using UQ09.Model;

namespace UQ09.OldVersion
{
    internal class OldCollectUQ
    {
        private static FileHandle _fileHandler = new FileHandle();
        private static Mail _mail = new Mail();
        static async Task<List<MaterialPO>?> Start_UQ_Old(UQParam uq, UQAction action)
        {
            string area = uq.Area.ToString();
            string step = $"{area}_UQ09";
            Console.WriteLine($"Step: {step}");
            try
            {
                Check _checker = new Check(step);
                Console.WriteLine(uq.Area);
                switch (action)
                {
                    case UQAction.Nothing:
                        Console.WriteLine("No action");
                        break;
                    case UQAction.CheckFile:
                        Console.WriteLine("Check file");
                        await _checker.CheckFile(uq.RequestPath);
                        break;
                    case UQAction.CheckFormat:
                        Console.WriteLine("Check format");
                        var (result, data, sourceFilePath, fileName) = await _checker.CheckFormat(uq.RequestPath);
                        if (!result)
                        {
                            Console.WriteLine("Format not valid");
                            break;
                        }
                        var checkReceivedDateValid = await _checker.CheckReceivedDateValid(data);
                        if (!checkReceivedDateValid)
                        {
                            Console.WriteLine("Ship Date & Received Date is not valid");
                            break;
                        }
                        var checkDuplicateMONumber = await _checker.CheckDuplicateMO(data);
                        if (!checkDuplicateMONumber)
                        {
                            Console.WriteLine("Duplicate");
                            break;
                        }
                        _fileHandler.SaveCSV(uq.ResultPath, data, fileName);
                        _fileHandler.RemoveFile(sourceFilePath);
                        return data;
                }
                return null;
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
