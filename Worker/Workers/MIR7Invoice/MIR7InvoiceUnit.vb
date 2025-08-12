Imports SAP.Utilities
Imports System.IO
Imports RPA.Core
Imports MIR7Invoice.Lib
Imports RPA.Core.Data
Imports System.Threading
Imports MIR7Invoice.Services
Imports MIR7InvoiceWorker.Model
Imports SharpCifs.Util.Sharpen
Imports Serilog
Imports iText.Kernel.XMP.Impl.XPath
Imports SharpCifs.Util.Transport
Imports System.Security.Cryptography



Public Class MIR7InvoiceUnit

#Region "Properties"
    Private ReadOnly files As List(Of FileInfo)
    Private ReadOnly inpSAPSessionID As String
    Private ReadOnly supplier As Supplier
    Private ReadOnly helper As MIR7InvoiceHelper
    Private ReadOnly closeInvoiceDate As Integer
    Private ReadOnly services As MIR7InvoiceServices
    Private ReadOnly processFolder As String
    Private ReadOnly successFolder As String
    Private ReadOnly errorFolder As String
    Private ReadOnly RPAFailFolder As String
    Private ReadOnly UnitLogger As ILogger
#End Region

#Region "Initialization and setup"
    Public Sub New(files As List(Of FileInfo), inpSAPSessionID As String, supplier As Supplier, closeInvoiceDate As Integer,
                processFolder As String, successFolder As String, errorFolder As String, RPAFailFolder As String)
        Me.files = files
        Me.inpSAPSessionID = inpSAPSessionID
        Me.supplier = supplier
        Me.closeInvoiceDate = closeInvoiceDate
        Me.processFolder = processFolder
        Me.successFolder = successFolder
        Me.errorFolder = errorFolder
        Me.RPAFailFolder = RPAFailFolder
        EnsureFoldersExist()
        helper = New MIR7InvoiceHelper()
        services = New MIR7InvoiceServices()
        UnitLogger = New LoggerConfiguration().
                WriteTo.File(
                    path:=AppDomain.CurrentDomain.BaseDirectory + $"UnitLog/{supplier.ID}-{supplier.Name}log.txt",
                    rollingInterval:=RollingInterval.Day,
                    rollOnFileSizeLimit:=True,
                    retainedFileCountLimit:=30,
                    [shared]:=True).
                CreateLogger()
    End Sub
    Private Sub EnsureFoldersExist()
        For Each folder In {processFolder, successFolder, errorFolder, RPAFailFolder}
            If Not String.IsNullOrEmpty(folder) AndAlso Not Directory.Exists(folder) Then
                Directory.CreateDirectory(folder)
            End If
        Next
    End Sub
#End Region

#Region "Logging and Event Handling"
    Public Event OnLog(message As String)
    Private Sub Log(message As Object)
        RaiseEvent OnLog(If(Not IsNothing(message), message?.ToString(), String.Empty))
        UnitLogger.Information(message?.ToString())
    End Sub
#End Region

#Region "File Handling"
    Public Function MoveFile(file As FileInfo, processResult As PID135_InvoiceResult, Optional copyInstead As Boolean = False) As (fileName As String, savePath As String)
        Dim savePath As String
        If processResult.ResultType = "RPAFail" Then
            savePath = Path.Combine(RPAFailFolder, processResult.InvoiceNo + "_" + processResult.ResultType + file.Extension)
        ElseIf processResult.ResultType = "OK" Then
            savePath = Path.Combine(successFolder, processResult.InvoiceNo + "_" + processResult.SAPInvoiceNo + file.Extension)
        Else

            savePath = Path.Combine(errorFolder, processResult.InvoiceNo + "_" + processResult.Result + file.Extension)
        End If
        Log(savePath)
        If System.IO.File.Exists(savePath) Then
            System.IO.File.Delete(savePath)
        End If
        If copyInstead Then
            file.CopyTo(savePath)
        Else
            file.MoveTo(savePath)
        End If
        Return (Path.GetFileName(savePath), savePath)
    End Function
    Public Function MoveToProcessFolder(file As FileInfo) As FileInfo
        Dim savePath As String = Path.Combine(processFolder, file.Name)
        Log(savePath)
        If System.IO.File.Exists(savePath) Then
            System.IO.File.Delete(savePath)
        End If
        file.MoveTo(savePath)
        Return file
    End Function
    Public Async Function SaveDB(file As FileInfo, invoice As Invoice, timeBegin As Date, result As String, typeResult As String, Optional imgList As String = Nothing) As Task
        Dim fileName = If(String.IsNullOrEmpty(invoice.InvoiceNo), Path.GetFileNameWithoutExtension(file.Name), invoice.InvoiceNo)
        Dim processResult As New PID135_InvoiceResult With {
                    .SupplierId = supplier.SAPID,
                    .SupplierName = supplier.Name,
                    .InvoiceDate = invoice.InvoiceDate,
                    .InvoiceNo = fileName,
                    .InvoiceAmount = invoice.TotalAmount,
                    .TimeBegin = timeBegin,
                    .ResultType = typeResult,
                    .Result = result,
                    .TimeEnd = Date.Now,
                    .ImageSrc = imgList
                }
        Dim filePathResult = MoveFile(file, processResult)
        processResult.FileName = filePathResult.fileName
        processResult.FolderName = filePathResult.savePath
        Log($"SaveDB: {invoice.InvoiceNo}")
        Try
            Dim response = Await services.MIR7RecordProcess(processResult)
            If Not response Then
                Log($"{processResult.InvoiceNo} cannot save to database")
            End If
        Catch ex As Exception
            Log($"Error in MIR7RecordProcess: {ex.Message}")
        End Try
    End Function
#End Region

#Region "Invoice"
    Public Function RegexInvoice(supplierID As Integer, file As FileInfo) As Invoice
        Select Case supplierID
            Case 1
                Return helper.ReadBestPacificInvoice(file.FullName)
            Case 2
                Return helper.ReadSMLInvoice(file.FullName)
            Case 3
                Return helper.ReadNilornInvoice(file.FullName)
            Case 4
                Return helper.ReadInternationalTrimmingsLabels(file.FullName)
            Case 5
                Return helper.ReadFineLineTechnologies(file.FullName)
            Case 6
                Return helper.ReadYVONNEINDUSTRIAL(file.FullName)
            Case 7
                Return helper.ReadSTRETCHLINE(file.FullName)
            Case 8
                Return helper.ReadPRYMINTIMATES(file.FullName)
            Case 9
                Return helper.ReadNewHorizonInvestment(file.FullName)
            Case 10
                Return helper.ReadPIONEERELASTIC(file.FullName)
            Case 11
                Return helper.ReadHINGYIPPRODUCTS1971LIMITED(file.FullName)
            Case 12
                Return helper.ReadLIJUNINDUSTRIAL(file.FullName)
            Case 13
                Return helper.ReadTIANHAILACE(file.FullName)
            Case 14
                Return helper.ReadSilverPrintingCompany(file.FullName)
            Case 15
                Return helper.ReadInspirePlasticsLimited(file.FullName)
            Case 16
                Return helper.ReadHungHon4KLimited(file.FullName)
            Case 17
                Return helper.ReadSunriseTextileAccessories(file.FullName)
            Case 18
                Return helper.ReadPacificTextilesLTD(file.FullName)
            Case 19
                Return helper.ReadBillionRiseKnittingLimited(file.FullName)
            Case 20
                Return helper.ReadFastechAsiaWorldwideLimited(file.FullName)
            Case 21
                Return helper.ReadJiangmenXinhuiCharmingIndustry(file.FullName)
            Case 22
                Return helper.ReadTaiHingPlasticMetal(file.FullName)
            Case Else
                Return Nothing
        End Select
    End Function

    Public Function ValidateInvoice(invoice As Invoice) As (isValid As Boolean, message As String)
        If IsNothing(invoice) Then
            Return (False, "NOTHING INVOICE")
        End If
        If invoice.Delivery.Count = 0 AndAlso supplier.ProcessingType = MIR7InvoiceProcessingType.DeliveryNote AndAlso supplier.ProcessingTypeBackup = MIR7InvoiceProcessingType.None Then
            Return (False, "发票没有交货单Hoá đơn không có thông tin đơn giao hàng")
        End If
        If invoice.PO.Count = 0 AndAlso supplier.ProcessingType = MIR7InvoiceProcessingType.PO AndAlso supplier.ProcessingTypeBackup = MIR7InvoiceProcessingType.None Then
            Return (False, "Hoá đơn không có thông tin PO")
        End If
        If supplier.Ignore517 AndAlso invoice.PO.Any(Function(po) po.StartsWith("517")) Then
            Return (False, "PO 517")
        End If
        If invoice.Currency = InvoiceResult.RMB Then
            Return (False, "货币为人民币 Loại tiền là nhân dân tệ")
        End If
        Return (True, Nothing)
    End Function
#End Region
    Public Async Function ZMMR0005(processResult As PID135_InvoiceResult, invoice As Invoice, supplierInvoice As SupplierInvoice) As Task(Of PID135_InvoiceResult)
        Using uZMMR0005 As New ZMMR0005(inpSAPSessionID)
            Dim reference As New List(Of String)
            Select Case If(supplierInvoice.UseProcessingBackup, supplierInvoice.ProcessingTypeBackup, supplierInvoice.ProcessingType)
                Case MIR7InvoiceProcessingType.None
                    Throw New MIR7Exception(InvoiceResult.Err, "ProcessTypeIsNull")
                Case MIR7InvoiceProcessingType.Invoice
                    reference = New List(Of String)({supplierInvoice.InvoiceNo})
                Case MIR7InvoiceProcessingType.DeliveryNote
                    reference = supplierInvoice.DeliveryNote
                Case MIR7InvoiceProcessingType.PO
                    reference = supplierInvoice.DeliveryNoteViaPO
            End Select
            If IsNothing(reference) Then
                Throw New MIR7Exception(InvoiceResult.Err, "Không có reference để tra cứu ZMMR0005")
            End If

            Dim zmmr0005Table = uZMMR0005.SearchResult(New ZMMR0005Input() With {
                .Factories = New List(Of String) From {"2502", "2504", "2505", "2506", "8103", "8104"},
               .Suppliers = New List(Of String) From {supplier.SAPID},
               .Reference = reference})
            If IsNothing(zmmr0005Table) Then
                processResult.Result = "ZMMR0005 没有找到数据Không tìm thấy dữ liệu"
            Else
                processResult.SAPQuantity = zmmr0005Table.Sum(Function(item) item.Quantity)
                processResult.SAPAmount = zmmr0005Table.Sum(Function(item) item.Amount)

                Dim invoiceAmount As Decimal
                Dim SAPAmount As Decimal

                If Decimal.TryParse(processResult.InvoiceAmount, invoiceAmount) AndAlso Decimal.TryParse(processResult.SAPAmount, SAPAmount) Then
                    If Math.Abs(invoiceAmount - SAPAmount) < 0.3D Then
                        processResult.Result = "此供应商发票已预制过或缩减Hóa đơn này đã tạo hoặc có đơn trừ tiền"
                    Else
                        processResult.Result = "金额不一致 Số tiền không khớp"
                    End If
                Else
                    Log($"Error in ZMMR0005RecordResult TryParse InvoiceAmount And SAPAmount")
                End If


                Try
                    Dim response = Await services.ZMMR0005RecordResult(zmmr0005Table)
                    If Not response Then
                        Log($"{processResult.InvoiceNo}_ZMMMR0005 cannot save to database")
                    End If
                Catch ex As Exception
                    Log($"Error in ZMMR0005RecordResult: {ex.Message}")
                End Try
            End If
        End Using
        Return processResult
    End Function


#Region "Main Process"
    Public Async Function Start(cancelToken As CancellationToken) As Task
        Dim count = 0
        For Each file In files
            count += 1
            Dim timeBegin = DateTime.Now
            'Dim captureImage As New List(Of String)

            Log($"Total {count}/{files.Count}. File: {file.FullName}")
            If IsNothing(cancelToken) Or cancelToken.IsCancellationRequested Then
                'Handle Operation Cancel
                Exit For
            End If
            file = MoveToProcessFolder(file)
            Dim invoice As New Invoice
            Dim regexResult As Boolean = True
            Dim regexCatchException As String = String.Empty
            Try
                invoice = RegexInvoice(supplier.ID, file)
            Catch ua As System.UnauthorizedAccessException
                Log($"RPA Robot could not access file.{ua.Message}")
                regexResult = False
                regexCatchException = "RPA Robot could not access file"
            Catch ex As Exception
                Log($"Regex: {ex.Message}")
                regexResult = False
                regexCatchException = "Regex"
            End Try
            If Not regexResult Then
                Await SaveDB(file, invoice, timeBegin, regexCatchException, InvoiceResult.RPAFail)
                Continue For
            End If

            ' Validate Invoice
            Dim validationResult = ValidateInvoice(invoice)
            If Not validationResult.isValid Then
                Log(validationResult.message)
                Await SaveDB(file, invoice, timeBegin, validationResult.message, InvoiceResult.Err)
                Continue For
            End If


            Dim results As New List(Of MIR7Result)
            Dim uMIR7 As New MIR7(inpSAPSessionID)
            AddHandler uMIR7.OnLogMessage, AddressOf Log
            Dim exception As Boolean = False
            Dim resultType As String = String.Empty
            Dim resultMessage As String = String.Empty
            Try
                uMIR7.DetectAndInputCompanyCodeUsingModalWindow("8100")
                uMIR7.SelectTransactionType(MIR7TransactionType.Invoice)
                Dim supplierInvoice = New SupplierInvoice() With {
                                  .SupplierID = supplier.SAPID,
                                  .SupplierName = supplier.Name,
                                  .InvoiceNo = invoice.InvoiceNo,
                                  .InvoiceDate = invoice.InvoiceDate,
                                  .DeliveryNote = invoice.Delivery,
                                  .PO = invoice.PO,
                                  .InvoiceAmount = invoice.TotalAmount,
                                  .ProcessingType = supplier.ProcessingType,
                                  .ProcessingTypeBackup = supplier.ProcessingTypeBackup
                                  }

                'ITL Supplier. Find DeliveryNote via PO
                If supplierInvoice.ProcessingType = MIR7InvoiceProcessingType.PO Then
                    Try
                        supplierInvoice.DeliveryNoteViaPO = uMIR7.SearchDeliveryNoteByPO(supplierInvoice)
                        invoice.Delivery = supplierInvoice.DeliveryNoteViaPO
                    Catch ex As MIR7Exception
                        If supplierInvoice.ProcessingTypeBackup = MIR7InvoiceProcessingType.None Then
                            Throw ex
                        End If
                    End Try
                End If

                results = uMIR7.MultipleProcessInvoice(supplierInvoice)
                'Get distinct result, if results is OK, get join SAPInvoice
                Dim result = New MIR7Result() With {
                    .Result = results.Select(Function(r) r.Result).Distinct().FirstOrDefault(),
                    .SAPInvoice = String.Join(",", results.Where(Function(r) r.Result = InvoiceResult.OK).Select(Function(r) r.SAPInvoice).ToList()),
                    .BalanceValue = results.Where(Function(r) r.Result = InvoiceResult.Balance).Select(Function(r) r.BalanceValue).FirstOrDefault()
                }

                Dim processResult As New PID135_InvoiceResult() With {
                        .SupplierId = supplier.SAPID,
                        .SupplierName = supplier.Name,
                        .InvoiceDate = invoice.InvoiceDate,
                        .InvoiceNo = invoice.InvoiceNo,
                        .Reference = String.Join("/", invoice.Delivery),
                        .InvoiceAmount = invoice.TotalAmount,
                        .TimeBegin = timeBegin
                    }
                If result.Result = InvoiceResult.OK Then
                    processResult.SAPInvoiceNo = String.Join(",", result.SAPInvoice)
                    processResult.ResultType = result.Result
                    processResult.Result = "成功 Thành công"
                    Dim filePathResult = MoveFile(file, processResult)
                    processResult.FileName = filePathResult.fileName
                    processResult.FolderName = filePathResult.savePath

                ElseIf result.Result = InvoiceResult.Balance Then
                    processResult.ResultType = result.Result
                    processResult.BalanceAmount = result.BalanceValue
                    processResult = Await ZMMR0005(processResult, invoice, supplierInvoice)
                    Dim filePathResult = MoveFile(file, processResult)
                    processResult.FileName = filePathResult.fileName
                    processResult.FolderName = filePathResult.savePath
                End If
                processResult.TimeEnd = Date.Now
                'Dim image = uMIR7.CaptureScreen()
                'processResult.ImageSrc = Image
                Try
                    Dim response = Await services.MIR7RecordProcess(processResult)
                    If Not response Then
                        Log($"{processResult.InvoiceNo} cannot save to database")
                    End If
                Catch ex As Exception
                    Log($"Error in MIR7RecordProcess: {ex.Message}")
                End Try
            Catch ex As Exception
                'Dim image = uMIR7.CaptureScreen()
                Log(ex.Message + ex.StackTrace)
                exception = True
                resultType = If(TypeOf ex Is MIR7Exception, CType(ex, MIR7Exception).Type, InvoiceResult.RPAFail)
                resultMessage = ex.Message
            Finally
                RemoveHandler uMIR7.OnLogMessage, AddressOf Log
            End Try
            If exception Then
                Await SaveDB(file, invoice, timeBegin, resultMessage, resultType, Nothing)
            End If
        Next
    End Function
#End Region

End Class
