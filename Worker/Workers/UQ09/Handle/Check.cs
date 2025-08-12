using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using RPA.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UQ09.Model;


namespace UQ09.Handle
{
    public class Check
    {
        private readonly Mail _mail = new Mail();
        private readonly Convert _convert = new Convert();
        private readonly string _step;
        public Check(string step)
        {
            _step = step;
        }
        public async Task<bool> CheckFile(string requestPath)
        {
            try
            {
                string message = $"共享盘 Ổ chung: {requestPath}";
                var files = new DirectoryInfo(requestPath).GetFiles("*.csv").ToList();
                string attachFiles = string.Join(",", files);
                if (files.Count == 0) return false;
                else if (files.Count > 1)
                {
                    await _mail.SendMailResult(message +
                                $"<br>在共享盘有多余一个文件 Có nhiều hơn một file trong ổ chung" +
                                $"<br>请检查保留要发送给客人文件 Vui lòng kiểm tra ổ chung và giữ lại một file cần gửi duy nhất",
                                _step, attachFiles);
                    return false;
                }
                else
                {
                    string fileName = files[0].FullName;
                    await _mail.SendMailResult(message +
                                $"<br>SAP已导出文件放在共享盘 SAP đã xuất file ra ổ chung" +
                                $"<br>请检查及修改 Vui lòng tiếp nhận và thêm thông tin cần thiết" +
                                $"<br>文件名 Tên file {fileName}",
                                _step, fileName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        public async Task<(bool, List<MaterialPO>, string, string)> CheckFormat(string path)
        {
            var results = new List<MaterialPO>();
            try
            {
                string message = $"共享盘 Ổ chung: {path}";

                var directory = new DirectoryInfo(path);
                var file = directory.GetFiles("*.csv").FirstOrDefault();
                if (file == null)
                {
                    await _mail.SendMailResult(message +
                                $"<br>在共享盘没有.csv文件 Không có file .csv trong ổ chung",
                                _step);
                    return (false, results, string.Empty, string.Empty);
                }

                message += $"<br>File: {file.Name}";
                using (var reader = new StreamReader(file.FullName))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<MaterialPO>().ToList();
                    for (var i = 0; i < records.Count; i++)
                    {
                        Console.WriteLine($"Row: {i + 1} / {records.Count}");
                        if (string.IsNullOrEmpty(records[i].MO_NUM)) continue;
                        records[i].MO_DTL_NUM = records[i].MO_DTL_NUM.PadLeft(4,'0');
                        records[i].SHIP_DLVR_DATE = _convert.ConvertDate(records[i].SHIP_DLVR_DATE);
                        records[i].RCV_DLVR_DATE = _convert.ConvertDate(records[i].RCV_DLVR_DATE);
                        records[i].MO_QTY = _convert.ConvertNumber(records[i].MO_QTY);
                        records[i].MO_ISSD_DATE = _convert.ConvertDate(records[i].MO_ISSD_DATE);
                        if (_convert.CheckValid(records[i]))
                        {
                            results.Add(records[i]);
                        }
                        else
                        {
                            Console.WriteLine("Fail");
                            await _mail.SendMailResult(message +
                                        $"<br>格式或文件资料有问题 Lỗi định dạng hoặc thiếu dữ liệu" +
                                        $"<br>请检查文件资料 Vui lòng kiểm tra lại dữ liệu file",
                                        _step, file.FullName);
                            return (false, results, string.Empty, string.Empty);
                        }
                    }
                    reader.Close();
                }
                return (true, results, file.FullName, file.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await Task.Delay(10000);
                return (false, results, string.Empty, string.Empty);
            }
        }

        public async Task<bool> CheckReceivedDateValid(List<MaterialPO> materialPOs)
        {
            List<string> messages = new List<string>();
            foreach(var item in materialPOs)
            {
                var (parseShipDateResult, shipDate )= _convert.ConvertStringToDate(item.SHIP_DLVR_DATE);
                if (!parseShipDateResult)
                {
                    messages.Add($"{item.MO_NUM} - SHIP_DLVR_DATE: WrongFormat ({item.SHIP_DLVR_DATE})");
                }
                var (parseReceivedDateResult, receivedDate) = _convert.ConvertStringToDate(item.RCV_DLVR_DATE);
                if (!parseReceivedDateResult)
                {
                    messages.Add($"{item.MO_NUM} - RCV_DLVR_DATE: WrongFormat ({item.RCV_DLVR_DATE})");
                }
                if (parseShipDateResult && parseReceivedDateResult && receivedDate < shipDate)
                {
                    messages.Add($"{item.MO_NUM} - RCV_DLVR_DATE({item.RCV_DLVR_DATE}) greater than SHIP_DLVR_DATE({item.SHIP_DLVR_DATE})");
                }
            }
            if ( messages.Count > 0 )
            {
                Console.WriteLine("Fail");
                await _mail.SendMailResult(string.Join("<br>", messages),_step);
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<bool> CheckDuplicateMO(List<MaterialPO> materialPOs)
        {
            var grouped = materialPOs.GroupBy(x => new { x.MO_NUM, x.MO_DTL_NUM })
                                     .Where(g => g.Count() > 1)
                                     .Select(g => $"Duplicated: MO_NUM: {g.Key.MO_NUM}, MO_DTL_NUM: {g.Key.MO_DTL_NUM}")
                                     .ToList();
            if (grouped.Count > 0)
            {
                await _mail.SendMailResult(string.Join("<br>", grouped), _step);
                return false;
            }
            else return true;
        }
    }
    
}
