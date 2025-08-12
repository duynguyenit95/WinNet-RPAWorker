using System.Text.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.IO;
using System.Linq;

namespace RPA.Tools
{
    public class ReadPurchaseXLS
    {
        public static DataTable ReadFile(string file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var extractPath = Path.Combine(Path.GetDirectoryName(file), fileName);
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }
            ZipFile.ExtractToDirectory(file, extractPath, Encoding.GetEncoding("GB2312"));

            var xlsFile = new DirectoryInfo(extractPath).GetFiles().First();
            var dataTable = new DataTable();
            var content = File.ReadAllText(xlsFile.FullName, Encoding.GetEncoding("GB2312"));
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                BadDataFound = null,
                NewLine = Environment.NewLine,
                DetectDelimiter = true,
                MissingFieldFound = null,
            };
            using (var csv = new CsvReader(new StringReader(content), config))
            {
                using (var dr = new CsvDataReader(csv))
                {
                    dataTable.Load(dr);
                }
            }
            return dataTable;
        }
    }

}
