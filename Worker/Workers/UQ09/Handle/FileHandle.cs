using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQ09.Model;

namespace UQ09.Handle
{
    public class FileHandle
    {

        private readonly Mail _mail = new Mail();
        public string SaveCSV<T>(string folder, List<T> inputs, string fileName)
        {
            var savePath = Path.Combine(folder, fileName); //$"MaterialPO_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.csv"
            using (var writer = new StreamWriter(savePath, false, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = args => args.Row.HeaderRecord != null,
            }))
            {
                csv.WriteHeader<T>();
                csv.NextRecord();
                foreach (var record in inputs)
                {
                    var normalizedRecord = NormalizeRecord(record);
                    csv.WriteRecord(normalizedRecord);
                    csv.NextRecord();
                }
                writer.Flush();
            }

            return savePath;
        }
        private string NormalizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Thay khoảng trắng không ngắt (U+00A0) bằng khoảng trắng thông thường (U+0020)
            input = input.Replace('\u00A0', '\u0020');

            // Loại bỏ khoảng trắng ở đầu và cuối (bao gồm các ký tự đặc biệt như U+0020 và U+00A0)
            return input.Trim(' ', '\u00A0');
        }
        private T NormalizeRecord<T>(T record)
        {
            if (record == null)
                return record;

            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(string) && prop.CanWrite)
                {
                    var value = prop.GetValue(record) as string;
                    if (value != null)
                    {
                        // Chuẩn hóa chuỗi
                        var normalizedValue = NormalizeString(value);
                        prop.SetValue(record, normalizedValue);
                    }
                }
            }

            return record;
        }
        public async Task CopyFile(string file, string destinationPath, string step)
        {
            var fileName = Path.GetFileName(file);
            var copyFile = Path.Combine(destinationPath, fileName);

            int retries = 5;
            int delay = 1000; // 1 second
            string log = string.Empty;

            for (int i = 0; i < retries; i++)
            {
                try
                {
                    File.Copy(file, copyFile, true);
                    await _mail.SendMailResult(
                            $"已发送文件到客人指定路径 Đã gửi file vào ổ chung khách hàng" +
                            $"<br> {destinationPath}", step, file);
                    return;
                }
                catch (IOException ex) when (i < retries - 1)
                {
                    log += $"<br> Attempt {i + 1} failed: {ex.Message}. Retrying in {delay} ms...";
                    Task.Delay(delay).Wait();
                }
            }
            await _mail.SendMailResult(log);
            return;
        }
        public void RemoveFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
