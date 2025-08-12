Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports MIR7Invoice.Lib
Imports OfficeOpenXml.FormulaParsing.Excel.Functions.Math
Imports OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup
Imports Org.BouncyCastle.Asn1.Ocsp
Imports RPA.Core
Imports RPA.Core.Data
Imports RPA.Tools
Imports RPA.Worker.Core
Imports RPA.Worker.Framework
Imports SAP.Utilities
Imports Serilog


Module Program
    Public Sub Main()

#If DEBUG Then
        TestRegex()
        Return
#End If
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Dim workerName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name
        Dim serverOption = New ServerOption() With {
            .WorkerName = workerName,
            .WorkerVersion = Application.ProductVersion
        }
#If Not DEBUG Then
        serverOption.ServerUrl = ConfigurationHelper.GetAppSettingsValue("ServerUrl")
#End If


        Dim worker = New MIR7InvoiceWorker(serverOption)

        Application.Run(New ControlPanel(Of WorkerOption)(worker))

    End Sub

    Public Sub TestTryCatch()
        Try
            Dim a = 1 / 0
            If True Then
                Throw New Exception("1 > 2")
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message + ex.StackTrace)
        End Try
    End Sub
    Public Async Sub TestCapture()
        Dim session As New MIR7("/app/con[0]/ses[0]")
        Dim image = session.CaptureScreen()
        Dim file = New FileInfo(image)
        Await session.HttpClientUploadFile(file)
        Launch(image)
    End Sub
    Private Sub Launch(path As String)
        Try
            If File.Exists(path) Then
                Process.Start(path)
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub
    Public Sub TestUnitLogger()
        'loop from 1-9
        For index As Integer = 1 To 9
            Dim supplier = New SupplierInfo().GetSupplierInfo(index)
            Dim UnitLogger = New LoggerConfiguration().
            WriteTo.File(
                path:=AppDomain.CurrentDomain.BaseDirectory + $"UnitLog/{supplier.ID}-{supplier.Name}log.txt",
                rollingInterval:=RollingInterval.Day,
                rollOnFileSizeLimit:=True,
                retainedFileCountLimit:=30,
                [shared]:=True).
            CreateLogger()

            UnitLogger.Information("Hello, {Name}!", supplier.Name)
        Next
    End Sub
    Public Async Sub TestMIR7()
        Dim SAPSesion = New MIR7("/app/con[0]/ses[0]")
        Dim cts = New CancellationTokenSource
        Dim dir = New IO.DirectoryInfo("Samples")
        Dim newFolder = dir.GetDirectories("01.New").FirstOrDefault()

        'Check folder 02.Processing, move all file to 01.New
        Dim processingFolder = dir.GetDirectories("02.Processing").FirstOrDefault()
        If Not IsNothing(processingFolder) Then
            Dim processFiles = processingFolder.GetFiles("*.pdf").ToList()
            If processFiles.Count > 0 Then
                For Each processFile In processFiles
                    processFile.MoveTo(Path.Combine(newFolder.FullName, processFile.Name))
                Next
            End If
        End If

        Dim files = newFolder.GetFiles("*.pdf").ToList()

        Dim _cts As CancellationToken
        Dim index As Integer = 4
        Dim supplier = New SupplierInfo().GetSupplierInfo(index)
        Dim unit = New MIR7InvoiceUnit(files, "/app/con[0]/ses[0]", supplier, 10,
                                                                       processFolder:=Path.Combine(dir.FullName, "02.Processing"),
                                                                       successFolder:=Path.Combine(dir.FullName, "03.Successed"),
                                                                       errorFolder:=Path.Combine(dir.FullName, "04.Errors"),
                                                                       RPAFailFolder:=Path.Combine(dir.FullName, "09.RPAFail"))
        Try
            Await unit.Start(_cts)
            Console.WriteLine("Done")
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try


    End Sub
    Public Sub TestChooseVendor()
        Dim SapGuiAuto = GetObject("SAPGUI")
        Dim SapApplication = SapGuiAuto.GetScriptingEngine
        Dim Session = SapApplication.FindById("/app/con[0]/ses[0]")
        Dim chooseVendor = Session.FindById("wnd[1]/usr/tblSAPLMR1MTC_MSEL_VEN")

        Dim index As Integer = 3
        Dim supplier = New SupplierInfo().GetSupplierInfo(index)

        Dim visibleRowCount = chooseVendor.VisibleRowCount
        Dim rowCount As Integer = chooseVendor.RowCount
        Dim currentIndex As Integer = 0
        Dim rowReaded As Integer = 0


        While rowReaded <= rowCount
            Dim row = chooseVendor.Rows.Item(currentIndex)
            Console.WriteLine(row.Item(0).Text)
            If row.Item(1).Text = supplier.SAPID Then
                row.Selected = True
                Session.FindById("wnd[1]/tbar[0]/btn[8]").Press()
                Exit While
            End If
            currentIndex += 1
            If currentIndex >= visibleRowCount Then
                If rowCount < 2 * visibleRowCount + chooseVendor.VerticalScrollBar.Position Then
                    chooseVendor.VerticalScrollBar.Position = rowCount - visibleRowCount
                Else
                    chooseVendor.VerticalScrollBar.Position = chooseVendor.VerticalScrollBar.Position + visibleRowCount
                End If
                chooseVendor = Session.FindById("wnd[1]/usr/tblSAPLMR1MTC_MSEL_VEN")
                currentIndex = 0
            End If
            rowReaded += 1
            Console.WriteLine(rowReaded)
        End While
        Console.WriteLine("End")
    End Sub
    Public Sub TestRegex()
        Dim shareDirPath = "\\172.19.18.69\fs01\QASRPA\QASRPA\01.Invoice - Yoyo - Howard\Test"
        Dim localPath = "E:\TestRegexMIR7"
        Dim localDir = New DirectoryInfo(localPath)
        'Dim supplierIDs = New List(Of String)({"500119", "500449", "500341", "500295", "502054", "500283", "500158", "500587", "500380"})
        Dim supplierIDs = New List(Of String)({"500855"})
        'Dim sIDs = New List(Of String)({"4"})
        For Each supplierID In supplierIDs

            Dim processResult As New List(Of String)
            Dim dir = localDir.GetDirectories($"*{supplierID}*").FirstOrDefault()
            Dim supplierName = dir.Name.Substring(dir.Name.IndexOf(supplierID) + supplierID.Length + 1)
            'Dim files = dir.GetDirectories("01.New").FirstOrDefault().GetFiles("*.pdf").ToList()
            Dim files = dir.GetFiles("*.pdf").ToList()
            Dim helper As New MIR7InvoiceHelper

            For Each file In files
                'Dim str = ITextSharpPDFOCR.ReadFile(file.FullName)
                Dim invoice As New Invoice
                Try
                    Select Case supplierID
#Region "Invoice"
                        Case 500855
                            invoice = helper.ReadBestPacificInvoice(file.FullName)
                        Case 500190
                            invoice = helper.ReadInternationalTrimmingsLabels(file.FullName)
                        Case 500119
                            invoice = helper.ReadFineLineTechnologies(file.FullName)
                        Case 500449
                            invoice = helper.ReadYVONNEINDUSTRIAL(file.FullName)
                        Case 500341
                            invoice = helper.ReadSTRETCHLINE(file.FullName)
                        Case 500295
                            invoice = helper.ReadPRYMINTIMATES(file.FullName)
                        Case 502054
                            invoice = helper.ReadNewHorizonInvestment(file.FullName)
                        Case 500283
                            invoice = helper.ReadPIONEERELASTIC(file.FullName)
                        Case 500158
                            invoice = helper.ReadHINGYIPPRODUCTS1971LIMITED(file.FullName)
                        Case 500587
                            invoice = helper.ReadLIJUNINDUSTRIAL(file.FullName)
                        Case 500380
                            invoice = helper.ReadTIANHAILACE(file.FullName)
#End Region
#Region "Delivery"
                        Case 500538
                            invoice = helper.ReadSilverPrintingCompany(file.FullName)
                        Case 502485
                            invoice = helper.ReadInspirePlasticsLimited(file.FullName)
                        Case 501091
                            invoice = helper.ReadHungHon4KLimited(file.FullName)
                        Case 500353
                            invoice = helper.ReadSunriseTextileAccessories(file.FullName)
                        Case 500266
                            invoice = helper.ReadPacificTextilesLTD(file.FullName)
                        Case 500029
                            invoice = helper.ReadBillionRiseKnittingLimited(file.FullName)
                        Case 501245
                            invoice = helper.ReadFastechAsiaWorldwideLimited(file.FullName)
                        Case 500523
                            invoice = helper.ReadJiangmenXinhuiCharmingIndustry(file.FullName)
                        Case 500364
                            invoice = helper.ReadTaiHingPlasticMetal(file.FullName)
#End Region
                        Case Else
                            Continue For
                    End Select
                    Console.WriteLine($"InvoiceNo: {invoice.InvoiceNo}")
                    Dim delivery = If(invoice.Delivery.Count = 0, "Nothing", String.Join(" / ", invoice.Delivery))
                    Dim PO = If(invoice.PO.Count = 0, "Nothing", String.Join(" / ", invoice.PO))
                    Console.WriteLine($"Delivery: {delivery}")
                    Console.WriteLine($"InvoiceDate: {invoice.InvoiceDate.ToString("dd.MM.yyyy")}")
                    Console.WriteLine($"TotalAmount: {invoice.TotalAmount}")
                    Console.WriteLine($"Currency: {invoice.Currency}")
                    Console.WriteLine($"PO: {PO}")
                    Console.WriteLine("-----------------------------------")
                    processResult.Add($"InvoiceNo: {invoice.InvoiceNo}")
                    processResult.Add($"Delivery: {delivery}")
                    processResult.Add($"InvoiceDate: {invoice.InvoiceDate.ToString("dd.MM.yyyy")}")
                    processResult.Add($"Amount: {invoice.TotalAmount}")
                    processResult.Add($"Currency: {invoice.Currency}")
                    processResult.Add($"PO: {PO}")
                    processResult.Add("-----------------------------------")
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    processResult.Add(ex.Message)
                End Try
            Next
            Dim logFile = Path.Combine(dir.FullName, $"Regex_{supplierID}_{supplierName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt")
            File.WriteAllLines(logFile, processResult)
        Next
    End Sub
    Public Async Sub TestTcodeZMMR0005()
        Dim SapGuiAuto = GetObject("SAPGUI")
        Dim SapApplication = SapGuiAuto.GetScriptingEngine
        Dim Session = SapApplication.FindById("/app/con[0]/ses[0]")
        Dim dialogWnd = Session.FindById("wnd[2]")
        Dim status = dialogWnd.PopupDialogText.ToString()
        If status.StartsWith("Delivery note") And status.EndsWith("not exist") Then
            Console.WriteLine(status)
        End If



        Dim cts = New CancellationTokenSource
        Dim files = New IO.DirectoryInfo("Samples").GetFiles("*.pdf").ToList()
        Dim dir = New IO.DirectoryInfo("Samples")
        Dim index As Integer = 1
        Dim supplier = New SupplierInfo().GetSupplierInfo(index)
        Dim unit = New MIR7InvoiceUnit(files, "/app/con[0]/ses[0]", supplier, 10,
                                                                       processFolder:=Path.Combine(dir.FullName, "02.Processing"),
                                                                       successFolder:=Path.Combine(dir.FullName, "03.Successed"),
                                                                       errorFolder:=Path.Combine(dir.FullName, "04.Errors"),
                                                                       RPAFailFolder:=Path.Combine(dir.FullName, "09.RPAFail"))
        Await unit.Start(cts.Token)
        Dim zmmr0005 As New ZMMR0005("/app/con[0]/ses[0]")
        Dim dt = zmmr0005.SearchAsDataTable(New ZMMR0005Input() With {
                       .Factories = New List(Of String) From {"2502", "2504", "2505", "2506", "8103"},
                       .Suppliers = New List(Of String) From {"500855"},
                       .Reference = New List(Of String) From {"H23386287-P10"}
                   })
        Dim zmmr0005Result As New List(Of ZMMR0005Result)
        For Each row As DataRow In dt.Rows
            If String.IsNullOrEmpty(row("XBLNR").ToString()) Then
                Continue For
            End If
            zmmr0005Result.Add(New ZMMR0005Result() With {
                               .Buyer = row("USERNAME").ToString(),
                               .RequestNo = row("BEDNR").ToString(),
                               .Reference = row("XBLNR").ToString(),
                               .PO = row("EBELN").ToString(),
                               .POItem = row("EBELP").ToString(),
                               .PlannedTrip = row("ETENR").ToString(),
                               .Material = row("MATNR").ToString(),
                               .MaterialDescription = row("MAKTX").ToString(),
                               .SO = row("VBELN").ToString(),
                               .POItemIndex = CInt(row("ZEILE").ToString()),
                               .GridDescription = row("J_3ASIZE").ToString(),
                               .Unit = row("ERFME").ToString(),
                               .Quantity = row("ERFMG").ToString(), 'RevertSAPNumber
                               .Currency = row("WAERS").ToString(),
                               .Price = row("NETPR").ToString(), 'RevertSAPNumber
                               .PriceUnit = CInt(row("PEINH").ToString()),
                               .Amount = row("NETWR").ToString() 'RevertSAPNumber
            })
        Next
        Dim zmmr0005TotalQty = zmmr0005Result.Sum(Function(item) item.Quantity)
    End Sub

    Private Property MIR7DataTableID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subITEM:SAPLMR1M:6310/tblSAPLMR1MTC_MR1M"
    Public Sub TestScrappingDataMIR7()
        Dim SapGuiAuto = GetObject("SAPGUI")
        Dim SapApplication = SapGuiAuto.GetScriptingEngine
        Dim mir7 As New MIR7("/app/con[0]/ses[0]", True)
        Dim MIR7DataTable As DataTable = mir7.SAPTableControlToDataTable(MIR7DataTableID, 0, True)
        Dim items = MIR7DataTable.Rows.Cast(Of DataRow).ToList()
        Dim totalAmount As Decimal = 0
        Dim count As Integer = 0
        For Each item In items
            If item.ItemArray(5) = "5510231227" Then
                Dim amount As Decimal = mir7.ReadDecimal(item.ItemArray(1))
                If amount > 0 Then
                    count += 1
                    Debug.WriteLine(item.ItemArray(1))
                    totalAmount += amount
                End If

            End If
        Next
        Debug.WriteLine("count: " + count.ToString())
        Dim inv = New With {
                                  .ItemAmount = items.Where(Function(x) x("Purchase Order_5") = "5510231227").Select(Function(x) mir7.ReadDecimal(x("Amount_1").ToString())).Sum()
        }
    End Sub
    Public Sub TestMultipleInput()
        Dim SapGuiAuto = GetObject("SAPGUI")
        Dim SapApplication = SapGuiAuto.GetScriptingEngine
        Dim Session = SapApplication.FindById("/app/con[2]/ses[0]")

        'Dim MultipleAssgmntID As String = "wnd[1]/usr/subMSEL:SAPLMR1M:6222/tblSAPLMR1MTC_MSEL_LIFS"
        Dim POMultipleAssgmntID As String = "wnd[1]/usr/subMSEL:SAPLMR1M:6221/tblSAPLMR1MTC_MSEL_BEST"
        Dim multipleAssgmnt = Session.FindById(POMultipleAssgmntID)
        Dim lists As New List(Of String)({"ABC", "CDE", "DEF"})
        Dim currentIndex As Integer = 0
        While currentIndex < lists.Count
            Dim visibleIndex = (currentIndex + 1) Mod multipleAssgmnt.VisibleRowCount
            Dim row = multipleAssgmnt.Rows.Item(currentIndex)
            row.Item(0).Text = lists(currentIndex)
            If (visibleIndex = 0) Then
                'Scroll input table - will make change to GUI
                multipleAssgmnt.VerticalScrollBar.Position = multipleAssgmnt.VerticalScrollBar.Position + multipleAssgmnt.VisibleRowCount
                'Reload table after scroll
                multipleAssgmnt = Session.FindById(POMultipleAssgmntID)
            End If
            currentIndex += 1
        End While
    End Sub
End Module
