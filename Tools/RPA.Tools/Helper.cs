using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using OfficeOpenXml;
using System.Drawing;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using RPA.Core;
using iText.Layout.Element;
using System.Net.Sockets;
using System.Net;

namespace RPA.Tools
{
    public static class Helper
    {

        #region Email Helper

        public static List<PropertyDefinition> listProperty = new List<PropertyDefinition>();

        public class PivotData
        {
            public string Name { get; set; }
            public dynamic Value { get; set; }
            //public bool IsPercent { get; set; }
        }

        public class ExcelPivotData
        {
            public List<PivotData> Pivots { get; set; }
            public string PivotDataNumberFormat { get; set; }
        }


        public class PropertyDefinition
        {
            public string Name { get; set; }
            public string Definition { get; set; }
        }

        public static string TableCreator<T>(List<T> listData)
        {
            string tablebody = "";
            string thead;
            var props = typeof(T).GetProperties();
            var rowcounter = 0;
            thead = "<tr>";
            thead += "<th>STT</th>";
            foreach (var item in props)
            {
                var propDefinition = Helper.listProperty.Where(x => x.Name == item.Name).FirstOrDefault();
                thead += "<th>" + (propDefinition != null ? propDefinition.Definition : item.Name) + "</th>";
            }
            thead += "</tr>";


            foreach (var obj in listData)
            {
                rowcounter++;
                string record = "<tr>";
                record += "<td>" + rowcounter + "</td>";
                foreach (var prop in props)
                {
                    record += "<td>" + prop.GetValue(obj, null) + "</td>";
                }
                record += "</tr>";
                tablebody += record;
            }
            return "<table>" + thead + tablebody + "</table>";
        }

        public static string TableCreatorVertical<T>(T data)
        {
            string tablebody = "";
            var props = typeof(T).GetProperties();
            foreach (var item in props)
            {
                var propDefinition = Helper.listProperty.Where(x => x.Name == item.Name).FirstOrDefault();
                tablebody += "<tr><th>" + (propDefinition != null ? propDefinition.Definition : item.Name) + "</th><td>" + item.GetValue(data, null) + "</td></tr>";
            }
            return "<table>" + tablebody + "</table>";
        }


        #endregion

        #region Epplus

        public static string DataTableToExcelSheet(DataTable dataTable, string saveFilePath)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                ExcelWorksheet sheet = package.Workbook.Worksheets.Add("Data");

                sheet.Cells.Style.Font.Name = "Times New Roman";
                sheet.Cells.Style.Font.Size = 12;

                sheet.Cells["A1"].LoadFromDataTable(dataTable, true);
                byte[] data = package.GetAsByteArray();
                System.IO.File.WriteAllBytes(saveFilePath, data);
            }
            return saveFilePath;
        }
        public static string ClassToExcelSheet<T>(IEnumerable<T> List, string saveFilePath
            , Dictionary<string, string> captions = null
            , List<string> hideProperties = null) where T : class
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                ExcelWorksheet sheet = package.Workbook.Worksheets.Add("Data");

                sheet.Cells.Style.Font.Name = "Times New Roman";
                sheet.Cells.Style.Font.Size = 12;

                var props = typeof(T).GetProperties();

                if (hideProperties != null && hideProperties.Count > 0)
                {
                    props = props.Where(x => !hideProperties.Contains(x.Name)).ToArray();
                }
                // Create Header
                var propertyCount = 1;
                foreach (var item in props)
                {
                    var caption = captions?.Where(x => x.Key == item.Name).FirstOrDefault().Value ?? item.Name;
                    sheet.Cells[1, propertyCount].Value = caption;
                    sheet.Cells[1, propertyCount].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    sheet.Cells[1, propertyCount].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(169, 208, 142));
                    sheet.Cells[1, propertyCount].Style.Font.Bold = true;
                    var type = item.PropertyType;
                    if (type == typeof(DateTime) || type == typeof(DateTime?))
                    {
                        sheet.Column(propertyCount).Style.Numberformat.Format = "yyyy-MM-dd hh:mm:ss";
                    }
                    else if (type == typeof(TimeSpan))
                    {
                        sheet.Column(propertyCount).Style.Numberformat.Format = "hh:mm:ss";
                    }

                    propertyCount++;
                }

                // Create Body
                int currentRow = 2;
                foreach (var item in List)
                {
                    var currentCol = 1;
                    foreach (var prop in props)
                    {
                        var value = prop.GetValue(item, null);
                        var type = prop.PropertyType.Name;
                        sheet.Cells[currentRow, currentCol].Value = value;
                        currentCol++;
                    }
                    currentRow++;
                }
                sheet.Cells.AutoFitColumns();
                byte[] data = package.GetAsByteArray();
                System.IO.File.WriteAllBytes(saveFilePath, data);
            }
            return saveFilePath;
        }

        public static string ClassToExcelSheet<T, T1>(T obj, string saveFilePath)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                ExcelWorksheet sheet = workbook.Worksheets.Add("MainWorkSheet");

                sheet.Cells.Style.Font.Name = "Times New Roman";
                sheet.Cells.Style.Font.Size = 12;
                sheet.Column(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                var props = typeof(T).GetProperties();
                var rowIndex = 1;
                foreach (var prop in props)
                {
                    if (typeof(IEnumerable<T1>).IsAssignableFrom(prop.PropertyType))
                    {
                        var list = (IEnumerable<T1>)prop.GetValue(obj);
                        if (typeof(T1) == typeof(string))
                        {
                            sheet.Cells[rowIndex, 1].Value = prop.Name;
                            foreach (var item in list)
                            {
                                sheet.Cells[rowIndex, 2].Value = item;
                                rowIndex++;
                            }

                        }
                        else
                        {
                            var propSheet = workbook.Worksheets.Add(prop.Name);
                            propSheet.Cells[1, 1].LoadFromCollection(list, true);
                            propSheet.Cells.AutoFitColumns();
                        }

                    }
                    else if (typeof(DataTable).IsAssignableFrom(prop.PropertyType))
                    {
                        var table = (DataTable)prop.GetValue(obj);
                        if (table != null)
                        {
                            var propSheet = workbook.Worksheets.Add(prop.Name);
                            propSheet.Cells[1, 1].LoadFromDataTable(table, true);
                            propSheet.Cells.AutoFitColumns();
                        }

                    }
                    else
                    {
                        sheet.Cells[rowIndex, 1].Value = prop.Name;
                        sheet.Cells[rowIndex, 2].Value = prop.GetValue(obj, null);
                        rowIndex++;
                    }
                }

                sheet.Cells.AutoFitColumns();
                byte[] data = package.GetAsByteArray();
                System.IO.File.WriteAllBytes(saveFilePath, data);
            }
            return saveFilePath;
        }

        public static string FlattenObjectToExcelSheet<T, T1>(T obj, string saveFilePath)
        {
            var mainObjType = typeof(T);
            var props = mainObjType.GetProperties();
            using (ExcelPackage package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                ExcelWorksheet sheet = workbook.Worksheets.Add(mainObjType.Name);

                sheet.Cells.Style.Font.Name = "Times New Roman";
                sheet.Cells.Style.Font.Size = 12;
                sheet.Column(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                var rowIndex = 1;
                foreach (var prop in props)
                {
                    if (typeof(IEnumerable<T1>).IsAssignableFrom(prop.PropertyType))
                    {
                        var list = (IEnumerable<T1>)prop.GetValue(obj);
                        if (typeof(T1) == typeof(string))
                        {
                            sheet.Cells[rowIndex, 1].Value = prop.Name;
                            foreach (var item in list)
                            {
                                sheet.Cells[rowIndex, 2].Value = item;
                                rowIndex++;
                            }

                        }
                        else
                        {
                            var propSheet = workbook.Worksheets.Add(prop.Name);
                            propSheet.Cells[1, 1].LoadFromCollection(list, true);
                            propSheet.Cells.AutoFitColumns();
                        }

                    }
                    else if (typeof(DataTable).IsAssignableFrom(prop.PropertyType))
                    {
                        var table = (DataTable)prop.GetValue(obj);
                        if (table != null)
                        {
                            var propSheet = workbook.Worksheets.Add(prop.Name);
                            propSheet.Cells[1, 1].LoadFromDataTable(table, true);
                            propSheet.Cells.AutoFitColumns();
                        }

                    }
                    else
                    {
                        sheet.Cells[rowIndex, 1].Value = prop.Name;
                        sheet.Cells[rowIndex, 2].Value = prop.GetValue(obj, null);
                        rowIndex++;
                    }
                }

                sheet.Cells.AutoFitColumns();
                byte[] data = package.GetAsByteArray();
                System.IO.File.WriteAllBytes(saveFilePath, data);
            }
            return saveFilePath;
        }

        public static (int lastRowIndex, ExcelWorksheet sheet) LoadCollectionAtSpecificPosition<T, T1, T2, T3, T4, T5>(ExcelWorksheet sheet, IEnumerable<T> enumerable, int rowIndex, int colIndex)
        {
            var props = typeof(T).GetProperties();

            return (rowIndex, sheet);
        }

        public static Dictionary<string, int> GetColumnHeader(ExcelWorksheet sheet, int headerRowIndex = 1, int startColIndex = 1)
        {
            Dictionary<string, int> propertyColIndexes = new Dictionary<string, int>();
            for (int i = startColIndex; i <= sheet.Dimension.End.Column; i++)
            {
                var cellValue = sheet.Cells[headerRowIndex, i].Value;
                if (cellValue != null)
                {
                    propertyColIndexes.Add(sheet.Cells[headerRowIndex, i].Value.ToString(), i);
                }
                else
                {
                    break;
                }

            }
            return propertyColIndexes;
        }

        public static void CopyRowAll(ref ExcelWorksheet ws, int Fform, int Fto, int Tform, int Tto)
        {
            var formatList = ws.ConditionalFormatting.ToList();
            ws.Cells[Fform, 1, Fto, ws.Dimension.End.Column].Copy(ws.Cells[Tform, 1, Tto, ws.Dimension.End.Column]);
            for (int i = 0; i <= (Fto - Fform); i++)
            {
                ws.Row(Tform + i).Height = ws.Row(Fform + i).Height;
                ws.Row(Tform + i).StyleID = ws.Row(Fform + i).StyleID;
            }
            var rowConditionFormat = formatList.FirstOrDefault(q => q.Address.Start.Row == Fform);
            if (rowConditionFormat != null)
            {
                var copyCF = ws.ConditionalFormatting.AddExpression(new ExcelAddress(Tform, 1, Tto, ws.Dimension.End.Column));
                copyCF.Style.Border = rowConditionFormat.Style.Border;
                copyCF.Style.Fill = rowConditionFormat.Style.Fill;
                copyCF.Style.Font = rowConditionFormat.Style.Font;
                copyCF.Style.NumberFormat = rowConditionFormat.Style.NumberFormat;
                copyCF.Formula = ((OfficeOpenXml.ConditionalFormatting.Contracts.IExcelConditionalFormattingExpression)rowConditionFormat).Formula.Replace(Fform.ToString(), Tform.ToString());
            }
        }
        public static bool CreateBasicExcelFromClass<T>(List<T> list, string saveFilePath)
        {
            FileInfo newfile = new FileInfo(saveFilePath);
            
            using (var package = new ExcelPackage(newfile))
            {
                var workbook = package.Workbook;
                ExcelWorksheet sheet = package.Workbook.Worksheets.Add("Data");

                sheet.Cells.Style.Font.Name = "Times New Roman";
                sheet.Cells.Style.Font.Size = 12;

                var props = typeof(T).GetProperties();
                var propertyCount = 1;
                foreach (var item in props)
                {
                    sheet.Cells[1, propertyCount].Style.Font.Bold = true;
                    sheet.Cells[1, propertyCount].Value = item.Name;
                    var type = item.PropertyType;
                    if (type == typeof(DateTime) || type == typeof(DateTime?))
                    {
                        sheet.Column(propertyCount).Style.Numberformat.Format = "yyyy-MM-dd hh:mm:ss";
                    }
                    else if (type == typeof(TimeSpan))
                    {
                        sheet.Column(propertyCount).Style.Numberformat.Format = "hh:mm:ss";
                    }

                    propertyCount++;
                }

                // Create Body
                int currentRow = 2;
                foreach (var item in list)
                {
                    var currentCol = 1;
                    foreach (var prop in props)
                    {
                        var value = prop.GetValue(item, null);
                        var type = prop.PropertyType.Name;
                        sheet.Cells[currentRow, currentCol].Value = value;
                        currentCol++;
                    }
                    currentRow++;
                }
                sheet.Cells.AutoFitColumns();
                byte[] data = package.GetAsByteArray();
                System.IO.File.WriteAllBytes(saveFilePath, data);
            }
            return true;
        }

        public static bool CreateExcelSheetFromTemplate<T>(List<T> list, string saveFilePath, string templateFilePath, int startDataRowIndex = 1, int headerRowIndex = 1, int startColIndex = 1, int sheetIndex = 0)
        {
            FileInfo newfile = new FileInfo(templateFilePath);
            using (ExcelPackage package = new ExcelPackage(newfile))
            {
                var workbook = package.Workbook;
                // Default sheetIndex is 0 if NetCore, else sheetIndex equal 0.
                int defaultSheetIndex = package.Workbook.Worksheets.FirstOrDefault().Index;
                int index = sheetIndex + defaultSheetIndex;
                ExcelWorksheet sheet = package.Workbook.Worksheets[index];
                var columnHeader = GetColumnHeader(sheet, headerRowIndex, startColIndex);
                var objProperties = list.First().GetType().GetProperties().ToList();
                int i = startDataRowIndex;
                int tem = 0;
                foreach (var item in list)
                {
                    tem++;
                    if (tem == list.Count)
                    {
                        ApplyData(ref sheet, item, i, columnHeader, objProperties);
                    }
                    else
                    {
                        sheet.InsertRow(i + 1, 1);
                        CopyRowAll(ref sheet, i, i, i + 1, i + 1);
                        ApplyData(ref sheet, item, i, columnHeader, objProperties);
                    }
                    i++;
                }
                sheet.DeleteRow(1);
                byte[] data = package.GetAsByteArray();
                System.IO.File.WriteAllBytes(saveFilePath, data);
            }
            return true;
        }
        public static bool CreateExcelMultiSheetFromTemplate<T>(Dictionary<string, List<T>> lists, string saveFilePath, string templateFilePath, int startDataRowIndex = 1, int headerRowIndex = 1, int startColIndex = 1)
        {
            FileInfo newfile = new FileInfo(templateFilePath);
            using (ExcelPackage package = new ExcelPackage(newfile))
            {
                var workbook = package.Workbook;
                ExcelWorksheet defaulSheet = package.Workbook.Worksheets.FirstOrDefault();
                var columnHeader = GetColumnHeader(defaulSheet, headerRowIndex, startColIndex);
                var objProperties = lists.First().Value.First().GetType().GetProperties().ToList();
                var defaultSheetName = defaulSheet.Name;
                foreach(var dt in lists)
                {
                    int i = startDataRowIndex;
                    var list = dt.Value;
                    var sheet = workbook.Worksheets.Copy(defaultSheetName, dt.Key);
                    int tem = 0;
                    
                    foreach (var item in list)
                    {
                        tem++;
                        if (tem == list.Count)
                        {
                            ApplyData(ref sheet, item, i, columnHeader, objProperties);
                        }
                        else
                        {
                            sheet.InsertRow(i + 1, 1);
                            CopyRowAll(ref sheet, i, i, i + 1, i + 1);
                            ApplyData(ref sheet, item, i, columnHeader, objProperties);
                        }
                        i++;
                    }
                }
                workbook.Worksheets.Delete(defaulSheet.Name);
                //ExcelWorksheet sheet = package.Workbook.Worksheets[sheetIndex];

                byte[] data = package.GetAsByteArray();
                System.IO.File.WriteAllBytes(saveFilePath, data);
            }
            return true;
        }
        public static void ApplyData<T>(ref ExcelWorksheet sheet, T item, int row, Dictionary<string, int> columnHeader, List<PropertyInfo> properties)
        {
            foreach (var prop in properties)
            {
                if (columnHeader.Any(x => x.Key == prop.Name))
                {
                    var colIndex = columnHeader.First(x => x.Key == prop.Name).Value;
                    sheet.Cells[row, colIndex].Value = prop.GetValue(item);
                }
            }
            if (typeof(ExcelPivotData).IsAssignableFrom(typeof(T)))
            {
                var pivots = (List<PivotData>)item.GetType().GetProperty("Pivots").GetValue(item, null);
                var format = (string)item.GetType().GetProperty("PivotDataNumberFormat").GetValue(item, null);
                foreach (var p in pivots)
                {
                    if (columnHeader.Any(x => x.Key == p.Name))
                    {
                        var colIndex = columnHeader.First(x => x.Key == p.Name).Value;
                        sheet.Cells[row, colIndex].Value = p.Value;
                        if (!string.IsNullOrEmpty(format))
                        {
                            sheet.Cells[row, colIndex].Style.Numberformat.Format = format;
                        }
                    }

                }
            }
        }



        public static string CreateExcelSheetFromTemplateWithCustomFill<T>(List<T> List
            , string saveFilePath, string templateFilePath
            , Func<ExcelWorksheet, T, int, ExcelWorksheet> rowDataFunc
            , int dataStartRow = 2)
        {
            FileInfo newfile = new FileInfo(templateFilePath);
            using (ExcelPackage package = new ExcelPackage(newfile))
            {
                var workbook = package.Workbook;
                ExcelWorksheet sheet = package.Workbook.Worksheets.FirstOrDefault();
                int i = dataStartRow;
                int count = 0;
                foreach (var item in List)
                {
                    count++;
                    if (count == List.Count)
                    {
                        sheet = rowDataFunc(sheet, item, i);
                    }
                    else
                    {
                        sheet.InsertRow(i + 1, 1);
                        sheet = CoppyRowAll(sheet, i, i, i + 1, i + 1);
                        sheet = rowDataFunc(sheet, item, i);
                    }
                    i++;
                }
                sheet.Cells.AutoFitColumns();
                byte[] data = package.GetAsByteArray();
                System.IO.File.WriteAllBytes(saveFilePath, data);
            }
            return saveFilePath;
        }
        public static ExcelWorksheet CoppyRowAll(ExcelWorksheet ws, int Fform, int Fto, int Tform, int Tto)
        {
            ws.Cells[Fform, 1, Fto, ws.Dimension.End.Column].Copy(ws.Cells[Tform, 1, Tto, ws.Dimension.End.Column]);
            for (int i = 0; i <= (Fto - Fform); i++)
            {
                ws.Row(Tform + i).Height = ws.Row(Fform + i).Height;
                ws.Row(Tform + i).StyleID = ws.Row(Fform + i).StyleID;
            }
            return ws;
        }
        public static ExcelWorksheet CopyColumnAll(ExcelWorksheet ws, int fromCol, int toCol, int fromRow, int toRow)
        {
            ws.Cells[fromRow, fromCol, toRow, fromCol].Copy(ws.Cells[fromRow, toCol, toRow, toCol]);
            ws.Column(toCol).StyleID = ws.Column(fromCol).StyleID;
            return ws;
        }
        public static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public static int ExcelColumnNameToNumber(string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException("columnName");

            columnName = columnName.ToUpperInvariant();

            int sum = 0;

            for (int i = 0; i < columnName.Length; i++)
            {
                sum *= 26;
                sum += (columnName[i] - 'A' + 1);
            }

            return sum;
        }
        public static List<T> CollectExcelData<T>(this string savepath, int startRow, List<ExcelColModel> colMap, string worksheetName = "")
        {
            try
            {
                var objT = Activator.CreateInstance<T>();
                var package = new ExcelPackage(new FileInfo(savepath));
                ExcelWorksheet worksheet = !string.IsNullOrEmpty(worksheetName) ? package.Workbook.Worksheets[worksheetName] : package.Workbook.Worksheets.FirstOrDefault();
                int rowCount = worksheet.Dimension.End.Row;
                int colCount = Math.Max(worksheet.Dimension.End.Column, objT.GetType().GetProperties().Length);

                var table = new DataTable();
                var indexMap = new Dictionary<string, int>();
                int tmp = 0;
                foreach (PropertyInfo info in objT.GetType().GetProperties())
                {
                    if (info.CanWrite)
                    {
                        table.Columns.Add(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType);
                    }
                    indexMap.Add(info.Name, tmp);
                    tmp++;
                }
                for (int i = startRow; i <= rowCount; i++)
                {
                    var row = table.NewRow();

                    foreach (var col in colMap)
                    {
                        var nameCol = col?.Key ?? string.Empty;
                        int z = col.Value;
                        int index = z - 1;
                        if (colMap != null && colMap.Count > 0 && !string.IsNullOrEmpty(nameCol))
                        {
                            index = indexMap.FirstOrDefault(x => x.Key == nameCol).Value;
                        }
                        if (objT.GetType().GetProperties().Count() - 1 < index) continue;

                        var type = objT.GetType().GetProperties()[index]?.PropertyType;


                        var value = worksheet.Cells[i, z]?.Text.Trim() ?? string.Empty;

                        try
                        {

                            if (col.Required == true && string.IsNullOrEmpty(value))
                            {
                                continue;
                            }

                            if (colMap == null || colMap.Count == 0)
                            {
                                row[objT.GetType().GetProperties()[z - 1].Name] = value;
                            }
                            else
                            {

                                if (!string.IsNullOrEmpty(nameCol))
                                {
                                    row[nameCol] = value;
                                }
                            }

                        }
                        catch
                        {
                        }
                    }
                    table.Rows.Add(row);
                }
                return table.ToDynamic<T>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static List<T> ToDynamic<T>(this DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>()
            .Select(c => c.ColumnName)
            .ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    var columns = pro.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "ColumnAttribute")?.ConstructorArguments?.FirstOrDefault().Value?.ToString() ?? pro.Name;
                    if (columnNames.Contains(columns))
                    {
                        object value = row[columns];
                        if (value == DBNull.Value)
                        {
                            value = null;
                        }
                        pro.SetValue(objT, value, null);
                    }
                }
                return objT;
            }).ToList();
        }

        #endregion

        #region DataTable To List 
        public static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        public static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName && dr[column.ColumnName].GetType() != typeof(DBNull))
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
        #endregion

        #region IPAddress
        public static string GetIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return null;
        }
        #endregion
    }
}
