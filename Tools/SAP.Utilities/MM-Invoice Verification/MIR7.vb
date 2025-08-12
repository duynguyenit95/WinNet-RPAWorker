Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Window
Imports RPA.Core
Imports RPA.Core.Data

Public Enum MIR7TransactionType As Integer
    Invoice = 1
    CreditMemo = 2
    SubsequentDebit = 3
    SubsequentCredit = 4
End Enum

Public Enum MIR7ReferenceType As Integer
    PuchaseOrder_SchedulingAgreement = 1
    DeliveryNote = 2
    BillOfLading = 3
    ServiceEntrySheet = 4
    Vendor = 5
    TransportationServiceAgent = 6
End Enum

Public Enum MIR7SubLayout As Integer
    ServiceItems = 1
    PlannedDelivery = 2
    ServiceItemsAndPlannedDelivery = 3
End Enum

Public Class MIR7Layout
    Public Const AllInformation = "7_6310"
    Public Const AggregationDeliveryNote = "4_6350"
End Class

Public Class InvoiceResult
    Public Const OK As String = "OK"
    Public Const RMB As String = "RMB"
    Public Const Err As String = "Error"
    Public Const POPosting As String = "POPosting"
    Public Const NotExisted As String = "NotExisted"
    Public Const RPAFail As String = "RPAFail"
    Public Const Balance As String = "Balance"
    Public Const Existed As String = "Existed"
End Class

Public Class SupplierInvoice
    Public Property SupplierID As String
    Public Property SupplierName As String
    Public Property InvoiceNo As String
    Public Property InvoiceAmount As Decimal
    Public Property InvoiceDate As DateTime
    Public Property PostingDate As DateTime
    Public Property DeliveryNote As List(Of String)
    Public Property DeliveryNoteViaPO As List(Of String)
    Public Property PO As List(Of String)
    Public Property Items As List(Of (ItemIndex As Integer, ReferenceDoc As String, ReferenceDocItemNo As String))
    Public Property ItemAmount As Decimal
    Public Property ProcessingType As MIR7InvoiceProcessingType = MIR7InvoiceProcessingType.DeliveryNote
    Public Property ProcessingTypeBackup As MIR7InvoiceProcessingType
    Public Property UseProcessingBackup As Boolean = False
End Class
Public Class MIR7Result
    Public Property Result As String
    Public Property BalanceValue As Decimal
    Public Property SAPInvoice As String
End Class
Public Class MIR7Invoice
    Public Property InvoiceNo As String
    Public Property Amount As Decimal
    Public Property Items As String
End Class
Public Class ME23NReference
    Public Property PO As String
    Public Property POItemNo As String
    Public Property RefDocs As List(Of RefDocItem)
End Class
Public Class RefDocItem
    Public Property RefDoc As String
    Public Property RefDocItemNo As String
End Class


Public Class MIR7Exception
    Inherits Exception
    Public Property Type As String

    Public Sub New(pType As String, pMessage As String)
        MyBase.New(pMessage)
        Type = pType
    End Sub
End Class

Public Class MIR7
    Inherits SAPSessionBase
    Dim Log As String = String.Empty
    Private Property BalanceDifferentID As String = "wnd[0]/usr/txtRM08M-DIFFERENZ"
    Private Property MIR7DataTableID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subITEM:SAPLMR1M:6310/tblSAPLMR1MTC_MR1M"
    Private Property ButtonMIR7DataTableDeSelectAll As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subITEM:SAPLMR1M:6310/btnDESELECT_ALL"
    Private Property InputTableSearch As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subITEM:SAPLMR1M:6310/txtRM08M-SEARCH_STRING"
    Private Property ButtonTableSearch As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subITEM:SAPLMR1M:6310/btnSEARCH"
    Private Property ItemSelected As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subITEM:SAPLMR1M:6310/txtRM08M-ANZPS"
    Private Property TextTotalItem As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subITEM:SAPLMR1M:6310/txtRM08M-ANZPS_TOTAL"

    Private Property InpAmountID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/tabsHEADER/tabpHEADER_TOTAL/ssubHEADER_SCREEN:SAPLFDCB:0010/txtINVFO-WRBTR"
    Private Property InpTextID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/tabsHEADER/tabpHEADER_TOTAL/ssubHEADER_SCREEN:SAPLFDCB:0010/ctxtINVFO-SGTXT"
    Private Property InpReferenceID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/tabsHEADER/tabpHEADER_TOTAL/ssubHEADER_SCREEN:SAPLFDCB:0010/txtINVFO-XBLNR"
    Private Property InpInvoiceDateID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/tabsHEADER/tabpHEADER_TOTAL/ssubHEADER_SCREEN:SAPLFDCB:0010/ctxtINVFO-BLDAT"
    Private Property InpPostingDateID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/tabsHEADER/tabpHEADER_TOTAL/ssubHEADER_SCREEN:SAPLFDCB:0010/ctxtINVFO-BUDAT"
    Private Property ItemTabID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB"
    Private Property POReferenceTabID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO"
    Private Property ConditionComboBoxID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/cmbRM08M-REFERENZBELEGTYP"
    Private Property SearchFieldID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subREFERENZBELEG:SAPLMR1M:6212/ctxtRM08M-LFSNR"
    Private Property SearchPOFieldID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subREFERENZBELEG:SAPLMR1M:6211/ctxtRM08M-EBELN"
    Private Property SearchButtonID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subREFERENZBELEG:SAPLMR1M:6212/btnRM08M-XMSEL"
    Private Property SearchPOButtonID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subREFERENZBELEG:SAPLMR1M:6211/btnRM08M-XMSEL"
    Private Property SaveBtnID As String = "wnd[0]/tbar[0]/btn[11]"
    Private Property SaveAsCompletedBtnID As String = "wnd[0]/tbar[1]/btn[32]"
    Private Property ComboBoxLayout_ITEM_LIST As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subITEM:SAPLMR1M:6310/cmbRM08M-ITEM_LIST_VERSION"
    Private Property ComboBoxLayout_AGGR_LIST As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subITEM:SAPLMR1M:6350/cmbRM08M-AGGR_LIST_VERSION"
    Private Property ComboBoxSubLayout As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subREFERENZBELEG:SAPLMR1M:6211/cmbRM08M-XWARE_BNK"
    Private Property PurchareOrderTableID As String = "wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subITEMS:SAPLMR1M:6010/tabsITEMTAB/tabpITEMS_PO/ssubTABS:SAPLMR1M:6020/subITEM:SAPLMR1M:6350/tblSAPLMR1MTC_AGGREGATION"
    'MultipleAssgmnt: Search multi reference
    Private Property MultipleAssgmntID As String = "wnd[1]/usr/subMSEL:SAPLMR1M:6222/tblSAPLMR1MTC_MSEL_LIFS"
    Private Property POMultipleAssgmntID As String = "wnd[1]/usr/subMSEL:SAPLMR1M:6221/tblSAPLMR1MTC_MSEL_BEST"
    Private Property MIR7BalanceValue As Decimal = Decimal.MinValue
    Private Property MIR7DataTable As DataTable
    Private Property PurchaseOrderTable As DataTable
    Private Property ChooseVendorTableID As String = "wnd[1]/usr/tblSAPLMR1MTC_MSEL_VEN"



    Public Sub New(inpSessionStrID As String, Optional attach As Boolean = False)
        MyBase.New(inpSessionStrID, attach)
        If Not attach Then
            OpenTcode("MIR7")
        End If
    End Sub
#Region "Main Process"
    Public Function MultipleProcessInvoice(inpInvoice As SupplierInvoice, Optional itemPerInvoice As Integer = 300) As List(Of MIR7Result)
        AddLog("MultipleProcessInvoice")
        Dim result = New List(Of MIR7Result)
        InputDate(ToSAPDate(inpInvoice.InvoiceDate))
        SelectReferenceType(MIR7ReferenceType.DeliveryNote)
        DetectAndAcceptAllInformationModalWindow()
        SelectLayout(MIR7Layout.AllInformation)
        If inpInvoice.PostingDate > inpInvoice.InvoiceDate Then
            InputPostingDate(ToSAPDate(inpInvoice.PostingDate))
        Else
            InputPostingDate(ToSAPDate(CalculatePostingDate(inpInvoice.InvoiceDate)))
        End If
        InputReference(inpInvoice.InvoiceNo)
        InputAmount(ToSAPNumber(inpInvoice.InvoiceAmount))

        SearchInfo(inpInvoice)

        If Not IsNothing(inpInvoice.Items) Then
            result.Add(CreateSubInvoice(inpInvoice))
            Return result
        End If


        If Math.Abs(MIR7BalanceValue) > 0.3 Then
            result.Add(New MIR7Result() With {
                    .Result = InvoiceResult.Balance,
                    .BalanceValue = MIR7BalanceValue
                })
            Return result
        End If


        Dim itemCount = CInt(FindElementByID(ItemSelected).Text.ToString())
        If itemCount <= itemPerInvoice Then
            'Create basic invoice 
            result.Add(SaveInvoice(inpInvoice))
            Return result
        End If

        ' Create multiple invoices
        MIR7DataTable = SAPTableControlToDataTable(MIR7DataTableID, 0, True)
        ClickPOToMoveME23N()
        Dim me23nLastedDate = ME23NGetLastedDate()
        If me23nLastedDate > CalculatePostingDate(inpInvoice.InvoiceDate) Then
            InputPostingDate(ToSAPDate(me23nLastedDate))
            inpInvoice.PostingDate = me23nLastedDate
        End If
        Dim MIR7SelectedRows = MIR7DataTable.Rows.Cast(Of DataRow).Where(Function(x) x("IsSelected")).ToList()
        Dim skip = 0
        While skip < MIR7SelectedRows.Count
            Dim items = MIR7SelectedRows.Skip(skip).Take(itemPerInvoice).ToList()

            Dim inv = New SupplierInvoice() With {
                                  .SupplierID = inpInvoice.SupplierID,
                                  .SupplierName = inpInvoice.SupplierName,
                                  .DeliveryNote = inpInvoice.DeliveryNote,
                                  .InvoiceDate = inpInvoice.InvoiceDate,
                                  .PostingDate = inpInvoice.PostingDate,
                                  .InvoiceNo = inpInvoice.InvoiceNo,
                                  .ProcessingType = inpInvoice.ProcessingType,
                                  .PO = inpInvoice.PO,
                                  .DeliveryNoteViaPO = inpInvoice.DeliveryNoteViaPO,
                                  .Items = items.Select(Function(x, index) (index, x("Reference Doc._61").ToString(), x("Ref. Doc. Item_60").ToString())).ToList(),
                                  .ItemAmount = items.Select(Function(x) ReadDecimal(x("Amount_1").ToString())).Sum()
            }
            If skip = 0 Then
                result.Add(CreateSubInvoice(inv))
            Else
                result.AddRange(MultipleProcessInvoice(inv, itemPerInvoice))
            End If

            skip += itemPerInvoice
        End While

        Return result
    End Function
    Public Function CreateSubInvoice(invoice As SupplierInvoice) As MIR7Result
        AddLog("CreateSubInvoice")
        FindElementByID(ButtonMIR7DataTableDeSelectAll).Press()
        HandleNetDueWarning()
        Dim inputSearch = FindElementByID(InputTableSearch)
        FindElementByID(ButtonTableSearch)
        Dim MIR7table = FindElementByID(MIR7DataTableID)
        Dim selected = 0
        Dim pageSize = MIR7table.VisibleRowCount
        Dim processingRowIndex = 0
        Dim rowRead = 0
        While selected < invoice.Items.Count
            Dim refDocItemNo = MIR7table.GetCell(processingRowIndex, 60).Text.ToString()
            Dim refDoc = MIR7table.GetCell(processingRowIndex, 61).Text.ToString()
            If invoice.Items.Any(Function(x) x.ReferenceDoc = refDoc And x.ReferenceDocItemNo = refDocItemNo) Then
                MIR7table.Rows.Item(processingRowIndex).Selected = True
                selected += 1
                If (selected = invoice.Items.Count) Then
                    Exit While
                End If
            End If

            processingRowIndex += 1
            rowRead += 1
            ' Should find all item to select before reach end 
            If String.IsNullOrEmpty(refDocItemNo) Then
                Throw New MIR7Exception(InvoiceResult.Err, "Could not find all item for subinvoices")
            End If

            ' Vertical Scroll table when reach end
            If rowRead Mod pageSize = 0 Then
                MIR7table.VerticalScrollBar.Position = MIR7table.VerticalScrollBar.Position + pageSize
                WaitLoading()
                HandlePostingDateWarning()
                'Must reload after scroll
                MIR7table = FindElementByID(MIR7DataTableID)
                'Reset processing index
                processingRowIndex = 0
            End If
        End While
        InputAmount(ToSAPNumber(invoice.ItemAmount))
        If invoice.PostingDate > invoice.InvoiceDate Then
            InputPostingDate(ToSAPDate(invoice.PostingDate))
        Else
            InputPostingDate(ToSAPDate(CalculatePostingDate(invoice.InvoiceDate)))
        End If
        ExecuteAction()
        Return SaveInvoice(invoice, True)
    End Function
    Public Sub ReadBalance()
        AddLog("ReadBalance")
        Dim balance = FindElementByID(BalanceDifferentID).Text
        Dim balanceValue As Decimal
        If Decimal.TryParse(RevertSAPNumber(balance), balanceValue) Then
            MIR7BalanceValue = balanceValue
        Else
            Throw New MIR7Exception(InvoiceResult.RPAFail, "Read Balance Number Fail: " + balance)
        End If
    End Sub

    Public Function SaveInvoice(inpInvoice As SupplierInvoice, Optional me23nChecked As Boolean = False, Optional isBackedProcess As Boolean = False) As MIR7Result
        AddLog("SaveInvoice")
        ReadBalance()
        If Math.Abs(MIR7BalanceValue) > 0.3 Then
            Return New MIR7Result() With {
                    .Result = InvoiceResult.Balance,
                    .BalanceValue = MIR7BalanceValue
                }
        End If

        FindElementByID(SaveBtnID).Press()
        WaitLoading()
        DetectAndAcceptAllInformationModalWindow()
        HandleNetDueWarning()
        HandlePeroidAdjutedInLineWithPostingDate()
        Dim saveInvoiceStatus = ReadSessionStatusPane()
        If (saveInvoiceStatus.MessageText.Contains("的确定偿还日期已经过去了")) Then
            FindElementByID(SaveBtnID).Press()
            WaitLoading()
        End If
        If (saveInvoiceStatus.MessageText.StartsWith("Document number") And saveInvoiceStatus.MessageText.EndsWith("has been parked")) _
            Or (saveInvoiceStatus.MessageText.StartsWith("Invoice document") And saveInvoiceStatus.MessageText.EndsWith("complete")) Then
            Dim matches As MatchCollection = Regex.Matches(saveInvoiceStatus.MessageText, "\b\d{10}\b")
            If matches.Count > 0 Then
                Dim SAPInvoice = matches(0).Value
                'MIR7SaveInvoiceNo = SAPInvoice
                Return New MIR7Result() With {
                    .Result = InvoiceResult.OK,
                    .SAPInvoice = SAPInvoice
                }
            Else
                Throw New MIR7Exception(InvoiceResult.RPAFail, "Cannot Detect SAP Invoice")
            End If
        End If

        If saveInvoiceStatus.MessageType = SAPMessageType.ErrorType And saveInvoiceStatus.MessageText.Contains("发票过账日期比PO收货日期早") Then
            If me23nChecked Then
                If isBackedProcess Then
                    Throw New MIR7Exception(InvoiceResult.Err, "ME23NChecked_Backed")
                End If
                HandleFakeBackProcess(inpInvoice.InvoiceDate)
                Return SaveInvoice(inpInvoice, True, True)
                'Throw New MIR7Exception(InvoiceResult.Err, saveInvoiceStatus.MessageText + "_ME23NChecked")
            End If

            'Clear error state by pressing F2
            FindElementByID("wnd[0]").SendVKey(2)
            MIR7DataTable = SAPTableControlToDataTable(MIR7DataTableID, 0, True)
            ClickPOToMoveME23N()
            Dim me23nLastedDate = ME23NGetLastedDate()

            If me23nLastedDate > CalculatePostingDate(inpInvoice.InvoiceDate) Then
                InputPostingDate(ToSAPDate(me23nLastedDate))
                Return SaveInvoice(inpInvoice, True)
            Else
                HandleFakeBackProcess(inpInvoice.InvoiceDate)
                Return SaveInvoice(inpInvoice, True, True)
            End If

            Throw New MIR7Exception(InvoiceResult.Err, saveInvoiceStatus.MessageText + "_ME23NChecked")
        End If

        Throw New MIR7Exception(InvoiceResult.RPAFail, $"Fail to Save Invoice: {saveInvoiceStatus.MessageType}")
    End Function

    Public Sub ClickPOToMoveME23N()
        AddLog("ClickPOToMoveME23N")
        While FindElementByID(MIR7DataTableID) IsNot Nothing
            Dim clickPO = FindElementByID(MIR7DataTableID + "/txtDRSEG-EBELN[5,0]")
            clickPO.SetFocus()
            clickPO.CaretPosition = 9
            FindElementByID("wnd[0]").SendVKey(2)
            WaitLoading()
            DetectAndAcceptAllInformationModalWindow()
        End While
    End Sub

    Public Function ME23NGetLastedDate() As DateTime
        AddLog("ME23NGetLastedDate")
        Dim uME23N As New ME23N(Session.Id, True)
        Dim status = ReadSessionStatusPane()
        If status.MessageType = "E" And status.MessageText.ToUpper().Contains("MISSING AUTHORIZATION") Then
            Throw New MIR7Exception(InvoiceResult.RPAFail, status.MessageText)
        End If
        Dim lastedPOPostingDate = Date.MinValue
        Dim inputPO = String.Empty
        Dim inputPOItem = String.Empty
        Dim poAndItems = MIR7DataTable.Rows.Cast(Of DataRow).
                                        Where(Function(x) x("IsSelected")).
                                        GroupBy(Function(x) New With {
                                            Key .PO = x("Purchase Order_5").ToString(),
                                            Key .POItemNo = x("PO Item_7").ToString()
                                        }).
                                        Select(Function(g) New ME23NReference With {
                                            .PO = g.Key.PO,
                                            .POItemNo = g.Key.POItemNo,
                                            .RefDocs = g.Select(Function(x) New RefDocItem With {
                                                .RefDoc = x("Reference Doc._61").ToString(),
                                                .RefDocItemNo = x("Ref. Doc. Item_60").ToString()
                                            }).Distinct().ToList()
                                        }).ToList()

        For Each item In poAndItems
            If inputPO <> item.PO Then
                inputPO = item.PO
                uME23N.InputPO(inputPO)
            End If
            If inputPOItem <> item.POItemNo Then
                inputPOItem = item.POItemNo
                uME23N.SelectPOItem(inputPOItem)
                uME23N.SelectPOHistoryTab()
            End If

            Dim me23nLastedDate = uME23N.GetLatestDeliveryDate(item.RefDocs)
            If me23nLastedDate > lastedPOPostingDate Then
                lastedPOPostingDate = me23nLastedDate
            End If
        Next
        uME23N.Back()
        uME23N.WaitLoading()
        Return lastedPOPostingDate
    End Function

    Public Sub SearchInfo(inpInvoice As SupplierInvoice)
        Dim searchValue As New List(Of String)
        Dim secondChange As Boolean
        Dim secondSearchValue As New List(Of String)

        Select Case inpInvoice.ProcessingType
            Case MIR7InvoiceProcessingType.None
                Throw New MIR7Exception(InvoiceResult.Err, "ProcessTypeIsNull")
            Case MIR7InvoiceProcessingType.Invoice
                searchValue = New List(Of String)({inpInvoice.InvoiceNo})
            Case MIR7InvoiceProcessingType.DeliveryNote
                searchValue = inpInvoice.DeliveryNote
            Case MIR7InvoiceProcessingType.PO
                searchValue = inpInvoice.DeliveryNoteViaPO
        End Select

        Select Case inpInvoice.ProcessingTypeBackup
            Case MIR7InvoiceProcessingType.None
                secondChange = False
            Case MIR7InvoiceProcessingType.Invoice
                secondChange = True
                secondSearchValue = New List(Of String)({inpInvoice.InvoiceNo})
            Case MIR7InvoiceProcessingType.DeliveryNote
                secondChange = True
                secondSearchValue = inpInvoice.DeliveryNote
        End Select

        SearchByDeliveryNote(searchValue, inpInvoice, secondChange, secondSearchValue)
    End Sub

    Public Sub SearchByDeliveryNote(searchValue As List(Of String), inpInvoice As SupplierInvoice, secondChange As Boolean, secondSearchValue As List(Of String))
        If IsNothing(searchValue) OrElse searchValue.Count = 0 Then
            If secondChange Then
                inpInvoice.UseProcessingBackup = True
                SearchByDeliveryNote(secondSearchValue, inpInvoice, False, secondSearchValue)
            Else
                Throw New MIR7Exception(InvoiceResult.Err, "Không tìm thấy thông tin đơn giao hàng (2)")
            End If
            Return
        End If
        AddLog("SearchByDeliveryNote")
        Dim itemTab = FindElementByID(ItemTabID)
        If itemTab.SelectedTab.Name <> "ITEMS_PO" Then
            Dim poRefTab = FindElementByID(POReferenceTabID)
            poRefTab.Select()
        End If
        'HandlePostingDateWarning()
        If searchValue.Count = 1 Then
            Dim searchField = FindElementByID(SearchFieldID)
            searchField.Text = searchValue(0)
            searchField.SetFocus()
            ExecuteAction()
            ValidateInvoicingParty()
            WaitLoading()
            HandleVendorChossing(inpInvoice.SupplierID)
            DetectAndAcceptAllInformationModalWindow()
        Else
            FindElementByID(SearchFieldID).Text = ""
            WaitLoading()
            FindElementByID(SearchButtonID).Press()
            WaitLoading()
            HandleNetDueWarning()
            SearchMultiField(searchValue)
        End If
        ValidateExistedInvoice()
        If inpInvoice.SupplierID = "0000500341" Then
            HandlePostingDateWarning()
        End If
        ValidateExistedVendor()
        'Kiểm tra đơn giao hàng có tồn tại hay không
        Dim validateExisted = ValidateExistedDeliveryNote()
        'Nếu tồn tại thì bỏ qua bước này
        If validateExisted Then
        Else
            'Nếu không tồn tại kiểm tra phương án dự phòng
            If secondChange Then
                'Nếu có phương and dự phòng, tìm kiếm đơn giao hàng theo phương án dự phòng
                SearchByDeliveryNote(secondSearchValue, inpInvoice, False, secondSearchValue)
            Else
                'Nếu không có phương án dự phòng (hoặc đã sử dụng phương án dự phòng mà vẫn fail validate)=> báo lỗi
                Throw New MIR7Exception(InvoiceResult.Existed, "供应商发票不存在Hóa đơn Nhà cung cấp không tồn tại")
            End If
        End If
        ValidateNoData()
        ValidateSupplierID(inpInvoice.SupplierID)
        If inpInvoice.ProcessingType = MIR7InvoiceProcessingType.PO Then
            DeselectNonRelatedPO(inpInvoice.PO)
            FindElementByID(InpAmountID).SetFocus()
            FindElementByID("wnd[0]").SendVKey(0)
            WaitLoading()
            HandlePostingDateWarning()
            HandleNetDueWarning()
        End If
        ReadBalance()
    End Sub
    Public Sub HandleVendorChossing(supplierID As String)
        Dim vendorTable = FindElementByID(ChooseVendorTableID)
        If vendorTable Is Nothing Then
            Return
        End If
        AddLog("HandleVendorChossing")
        Dim visibleRowCount = vendorTable.VisibleRowCount
        Dim rowCount As Integer = vendorTable.RowCount
        Dim currentIndex As Integer = 0
        Dim rowReaded As Integer = 0
        While rowReaded <= rowCount
            Dim row = vendorTable.Rows.Item(currentIndex)
            Console.WriteLine(row.Item(0).Text)
            If row.Item(1).Text = supplierID Then
                AddLog($"Chossing {row.Item(0).Text}")
                row.Selected = True
                FindElementByID("wnd[1]/tbar[0]/btn[8]").Press()
                Exit While
            End If
            currentIndex += 1
            If currentIndex >= visibleRowCount Then
                If rowCount < 2 * visibleRowCount + vendorTable.VerticalScrollBar.Position Then
                    vendorTable.VerticalScrollBar.Position = rowCount - visibleRowCount
                Else
                    vendorTable.VerticalScrollBar.Position = vendorTable.VerticalScrollBar.Position + visibleRowCount
                End If
                vendorTable = Session.FindById(ChooseVendorTableID)
                currentIndex = 0
            End If
            rowReaded += 1
        End While
    End Sub
    Public Sub DeselectNonRelatedPO(POs As List(Of String))
        Dim MIR7table = FindElementByID(MIR7DataTableID)
        Dim pageSize = MIR7table.VisibleRowCount
        Dim processingRowIndex = 0
        Dim rowRead = 0
        Dim firstRowPOcorrect = -1
        Dim totalRow = CInt(FindElementByID(TextTotalItem).Text.ToString())
        MIR7table.VerticalScrollBar.Position = 0
        While True
            ' Check PO and deselect
            Dim rowPO = MIR7table.GetCell(processingRowIndex, 5).Text.ToString()
            If Not POs.Contains(rowPO) Then
                MIR7table.Rows.Item(processingRowIndex).Selected = False
            ElseIf firstRowPOcorrect = -1 Then
                firstRowPOcorrect = rowRead
            End If
            ' Check blank row and exit
            Dim item = MIR7table.GetCell(processingRowIndex, 0).Text.ToString()
            If String.IsNullOrEmpty(item) Then
                Exit While
            End If
            processingRowIndex += 1
            rowRead += 1
            ' Vertical Scroll table when reach end
            If rowRead Mod pageSize = 0 Then
                MIR7table.VerticalScrollBar.Position = MIR7table.VerticalScrollBar.Position + pageSize
                WaitLoading()
                HandlePostingDateWarning()
                'Must reload after scroll
                MIR7table = FindElementByID(MIR7DataTableID)
                'Reset processing index
                processingRowIndex = 0
            End If
            If rowRead = totalRow Then
                'Scroll up to first page
                MIR7table.VerticalScrollBar.Position = 0
                WaitLoading()
                HandlePostingDateWarning()
                Exit While
            End If
        End While
    End Sub

    Private Sub ValidateExistedInvoice()
        AddLog("ValidateExistedInvoice")
        ' Focus on input amount then press Enter to do SAP Validation
        FindElementByID(InpAmountID).SetFocus()
        Session.FindById("wnd[0]").SendVKey(0)
        Dim status = ReadSessionStatusPane()
        If status.MessageText.StartsWith("Check if invoice already entered as logistics") Then
            Throw New MIR7Exception(InvoiceResult.Err, status.MessageText)
        End If
        DetectAndAcceptAllInformationModalWindow()
    End Sub

    Public Sub SearchMultiField(lists As List(Of String))
        AddLog("SearchMultiField")
        'Dim array = lists.ToArray()
        Dim multipleAssgmnt = FindElementByID(MultipleAssgmntID)
        WaitLoading()
        Dim currentIndex As Integer = 0
        While currentIndex < lists.Count
            Dim visibleIndex = (currentIndex + 1) Mod multipleAssgmnt.VisibleRowCount
            Dim row = multipleAssgmnt.Rows.Item(currentIndex)
            row.Item(0).Text = lists(currentIndex)
            If (visibleIndex = 0) Then
                'Scroll input table - will make change to GUI
                multipleAssgmnt.VerticalScrollBar.Position = multipleAssgmnt.VerticalScrollBar.Position + multipleAssgmnt.VisibleRowCount
                WaitLoading()
                ValidateExistedDeliveryNoteDialog()
                'Reload table after scroll
                multipleAssgmnt = FindElementByID(MultipleAssgmntID)
            End If
            currentIndex += 1
        End While
        FindElementByID("wnd[1]/tbar[0]/btn[8]").Press()
        WaitLoading()
        ValidateExistedDeliveryNoteDialog()
        DetectAndAcceptAllInformationModalWindow()
    End Sub
    Public Sub SearchMultiFieldPO(lists As List(Of String))
        AddLog("SearchMultiFieldPO")
        'Dim array = lists.ToArray()
        Dim multipleAssgmnt = FindElementByID(POMultipleAssgmntID)
        WaitLoading()
        Dim currentIndex As Integer = 0
        While currentIndex < lists.Count
            Dim visibleIndex = (currentIndex + 1) Mod multipleAssgmnt.VisibleRowCount
            Dim row = multipleAssgmnt.Rows.Item(currentIndex)
            row.Item(0).Text = lists(currentIndex)
            If (visibleIndex = 0) Then
                'Scroll input table - will make change to GUI
                multipleAssgmnt.VerticalScrollBar.Position = multipleAssgmnt.VerticalScrollBar.Position + multipleAssgmnt.VisibleRowCount
                WaitLoading()
                ValidateExistedDeliveryNoteDialog()
                'Reload table after scroll
                multipleAssgmnt = FindElementByID(POMultipleAssgmntID)
            End If
            currentIndex += 1
        End While
        FindElementByID("wnd[1]/tbar[0]/btn[8]").Press()
        WaitLoading()
        ValidateExistedPODialog()
        DetectAndAcceptAllInformationModalWindow()
    End Sub
#End Region

#Region "Specialized Process"
    'Supplier: International Trimming & Label
    Public Function SearchDeliveryNoteByPO(inpInvoice As SupplierInvoice) As List(Of String)
        AddLog("SearchDeliveryNoteByPO")
        InputDate(ToSAPDate(inpInvoice.InvoiceDate))
        InputPostingDate(ToSAPDate(CalculatePostingDate(inpInvoice.InvoiceDate)))
        InputReference(inpInvoice.InvoiceNo)

        SelectReferenceType(MIR7ReferenceType.PuchaseOrder_SchedulingAgreement)
        SelectSubLayout(MIR7SubLayout.ServiceItemsAndPlannedDelivery)
        If inpInvoice.PO.Count = 1 Then
            Session.FindById(SearchPOFieldID).Text = inpInvoice.PO(0)
        Else
            FindElementByID(SearchPOFieldID).Text = ""
            WaitLoading()
            FindElementByID(SearchPOButtonID).Press()
            WaitLoading()
            HandleNetDueWarning()
            SearchMultiFieldPO(inpInvoice.PO)
        End If
        ExecuteAction()
        WaitLoading()
        HandleNetDueWarning()
        HandlePeroidAdjutedInLineWithPostingDate()
        DetectAndAcceptAllInformationModalWindow()
        SelectLayout(MIR7Layout.AggregationDeliveryNote)
        WaitLoading()
        DetectAndAcceptAllInformationModalWindow()
        HandleNetDueWarning()
        Dim POTable = GetPurchaseOrderTable()
        'Return POTable.Rows.Cast(Of DataRow).Select(Function(x) x("Delivery note_5").ToString()).ToList()
        Return FindTheSubsetOfDeliveryNotesWithSum(POTable, inpInvoice.InvoiceAmount)
    End Function
    Public Function GetPurchaseOrderTable() As DataTable
        AddLog("SearchDeliveryNoteByPO")
        Dim purchaseOrderTable = SAPTableControlToDataTable(PurchareOrderTableID, 5, True)
        If purchaseOrderTable.Rows.Count = 0 Then
            Throw New MIR7Exception(InvoiceResult.Err, "没有带出数据Không tải ra dữ liệu")
        End If
        If purchaseOrderTable.Rows.Cast(Of DataRow).FirstOrDefault().ItemArray(5) = "*" Then
            Dim symbol = FindElementByID(PurchareOrderTableID + "/txtRM08V-LSNR[5,0]")
            symbol.SetFocus()
            symbol.CaretPosition = 1
            FindElementByID("wnd[0]").SendVKey(2)
            WaitLoading()
            HandleNetDueWarning()
            GetPurchaseOrderTable()
        End If
        Return SAPTableControlToDataTable(PurchareOrderTableID, 5, True)
    End Function


#End Region

#Region "Calculator"
    Public Function FindTheSubsetOfDeliveryNotesWithSum(poTable As DataTable, targetSum As Decimal) As List(Of String)
        AddLog("FindTheSubsetOfDeliveryNotesWithSum")
        Dim rows = poTable.Rows.Cast(Of DataRow).Select(Function(x)
                                                            Return (DeliveryNote:=x("Delivery note_5").ToString(),
                                                                                NetAmount:=ReadDecimal(x("Net amount in doc. crcy_7").ToString()))
                                                        End Function).Where(Function(x) x.NetAmount > 0).OrderBy(Function(x) x.NetAmount).ToList()
        If rows.Count = 0 Then
            Throw New MIR7Exception(InvoiceResult.Err, $"没有对应金额的送货单Không có đơn giao hàng nào khớp số tiền")
        End If
        Dim result As New List(Of List(Of (String, Decimal)))
        Dim numSubsets As Integer = 1 << rows.Count

        For mask As Integer = 0 To numSubsets - 1
            Dim currentSubset As New List(Of (String, Decimal))
            Dim currentSum As Decimal = 0
            Dim bitmask As Integer = mask

            For i As Integer = 0 To rows.Count - 1
                If (bitmask And 1) <> 0 Then
                    currentSubset.Add(rows(i))
                    currentSum += rows(i).NetAmount
                End If
                bitmask >>= 1
            Next

            If Math.Abs(currentSum - targetSum) <= 0.3 Then
                result.Add(New List(Of (String, Decimal))(currentSubset))
            End If
        Next
        If result.Count = 0 Then
            Throw New MIR7Exception(InvoiceResult.Err, $"没有对应金额的送货单Không có đơn giao hàng nào khớp số tiền")
        ElseIf result.Count > 1 Then
            Throw New MIR7Exception(InvoiceResult.Err, $"有多于符合金额的送货单Có nhiều đơn giao hàng khớp số tiền")
        Else
            Return result(0).Select(Function(x) x.Item1.ToString()).ToList()
        End If

    End Function
    Public Function CalculatePostingDate(inpDate As DateTime, Optional closeInvoiceDate As Integer = 10) As Date
        AddLog("CalculatePostingDate")
        Dim monthDiff = Today.Month - inpDate.Month + (Today.Year - inpDate.Year) * 12
        Dim firstDayOfThisMonth = New DateTime(Today.Year, Today.Month, 1)
        Dim firstDayOfPreviousMonth = firstDayOfThisMonth.AddMonths(-1)

        If monthDiff = 0 Then
            'Return ToSAPDate(inpDate) ' Case 1 
            Return inpDate
        End If

        If monthDiff = 1 And Today.Day <= closeInvoiceDate Then
            'Return ToSAPDate(inpDate) ' Case 2
            Return inpDate
        End If

        If monthDiff = 1 And Today.Day > closeInvoiceDate Then
            'Return ToSAPDate(firstDayOfThisMonth) ' Case 3
            Return firstDayOfThisMonth
        End If

        If monthDiff > 1 And Today.Day <= closeInvoiceDate Then
            'Return ToSAPDate(firstDayOfPreviousMonth) ' Case 4
            Return firstDayOfPreviousMonth
        End If

        If monthDiff > 1 And Today.Day > closeInvoiceDate Then
            'Return ToSAPDate(firstDayOfThisMonth) ' Case 5
            Return firstDayOfThisMonth
        End If

        'Return ToSAPDate(inpDate)
        Return inpDate
    End Function


#End Region

#Region "ComboBox"
    Public Sub SelectTransactionType(transactionTypeCode As MIR7TransactionType)
        AddLog("Try Select Transaction Type")
        Dim transactionType = FindElementByID("wnd[0]/usr/cmbRM08M-VORGANG")
        If IsNothing(transactionType) Then
            Throw New MIR7Exception(InvoiceResult.RPAFail, "Not found MIR7TransactionType Id")
        End If
        If transactionType.Key <> transactionTypeCode Then
            Session.FindById("wnd[0]/usr/cmbRM08M-VORGANG").Key = transactionTypeCode
        End If
        WaitLoading()
        AddLog("Try Select Transaction Type Completed")
    End Sub
    Public Sub SelectReferenceType(refTypeCode As MIR7ReferenceType)
        AddLog("SelectReferenceType")
        Dim referenceType = FindElementByID(ConditionComboBoxID)
        If IsNothing(referenceType) Then
            Throw New MIR7Exception(InvoiceResult.RPAFail, "Not found MIR7ReferenceType Id")
        End If
        If referenceType.Key <> refTypeCode Then
            referenceType.Key = refTypeCode
        End If
        WaitLoading()
    End Sub
    Public Sub SelectSubLayout(subLayoutCode As MIR7SubLayout)
        AddLog("SelectSubLayout")
        Dim subLayout = FindElementByID(ComboBoxSubLayout)
        If IsNothing(subLayout) Then
            Throw New MIR7Exception(InvoiceResult.RPAFail, "Not found MIR7SubLayout Id")
        End If
        If subLayout.Key <> subLayoutCode Then
            subLayout.Key = subLayoutCode
        End If
        WaitLoading()
    End Sub
    Public Sub SelectLayout(layoutCode As String)
        AddLog("SelectLayout")
        Dim layout = FindElementByID(ComboBoxLayout_ITEM_LIST)
        If IsNothing(layout) Then
            layout = FindElementByID(ComboBoxLayout_AGGR_LIST)
            If IsNothing(layout) Then
                Throw New MIR7Exception(InvoiceResult.RPAFail, "Not found MIR7Layout Id")
            End If
        End If
        If layout.Key <> layoutCode Then
            layout.Key = layoutCode
        End If
        WaitLoading()
    End Sub
#End Region

#Region "Input"
    Public Sub InputReference(inputValue As String)
        AddLog("InputReference")
        Dim element = FindElementByID(InpReferenceID)
        element.Text = inputValue
        AddLog("InputReference Completed")
    End Sub

    Public Sub InputAmount(inputValue As String)
        AddLog("InputAmount")
        Dim element = FindElementByID(InpAmountID)
        element.Text = inputValue.Replace(".", ",")
        AddLog("InputAmountCompleted")
    End Sub


    Public Sub InputDate(inputValue As String)
        AddLog("Input Date")
        Dim element = FindElementByID(InpInvoiceDateID)
        element.Text = inputValue
        'Focus and hit Enter to load
        element.SetFocus()

        WaitLoading()
        DetectAndCloseAllExtraModal()
        AddLog("Input Date Completed")
    End Sub



    Public Sub InputPostingDate(inputValue As String, Optional tryCount As Integer = 0)
        AddLog($"Input Posting Date {tryCount}")
        Dim inpDate = FindElementByID(InpPostingDateID)
        inpDate.Text = inputValue
        'Focus and hit Enter to load
        inpDate.SetFocus()
        ExecuteAction()
        WaitLoading()
        HandlePostingDateWarning(tryCount)
        HandlePeroidAdjutedInLineWithPostingDate()
        HandlePostingTakesPlaceInPreviousFiscalYear()
        Dim status = ReadSessionStatusPane()
        If status.MessageType = SAPMessageType.ErrorType Then
            Throw New MIR7Exception(InvoiceResult.Err, status.MessageText)
        End If
        AddLog("Input Date Completed")
    End Sub
#End Region

#Region "Validate"
    Public Sub ValidateSupplierID(supplierID As String)
        AddLog("ValidateSupplierID")
        Dim mir7SupplierInfo = Session.FindById("wnd[0]/usr/subHEADER_AND_ITEMS:SAPLMR1M:6005/subVENDOR_DATA:SAPLMR1M:6510/boxRM08M-LIFNR_TEXT").Text.ToString()
        If Not mir7SupplierInfo.EndsWith(supplierID) Then
            Throw New MIR7Exception(InvoiceResult.Err, "供应商代码不对Mã nhà cung cấp trên PO không đúng")
        End If
    End Sub

    Public Sub ValidateExistedVendor()
        AddLog("ValidateExistedVendor")
        Dim status = ReadSessionStatusPane()
        If status.MessageText.StartsWith("Vendor is not defined in company code") Then
            Throw New MIR7Exception(InvoiceResult.Err, status.MessageText)
        End If
    End Sub

    Public Function ValidateExistedDeliveryNote() As Boolean
        AddLog("ValidateExistedDeliveryNote")
        Dim status = ReadSessionStatusPane()
        If status.MessageText.StartsWith("Delivery note") And status.MessageText.EndsWith("does not exist") Then
            Return False
            'Throw New MIR7Exception(InvoiceResult.Existed, "供应商发票不存在Hóa đơn Nhà cung cấp không tồn tại")
        End If
        Return True
    End Function

    Public Sub ValidateNoData()
        AddLog("ValidateNoData")
        Dim item = CInt(FindElementByID(TextTotalItem).Text.ToString())
        If item < 1 Then
            Throw New MIR7Exception(InvoiceResult.Err, "没有带出数据Không tải ra dữ liệu")
        End If
    End Sub

    Public Sub ValidateExistedDeliveryNoteDialog()
        AddLog("ValidateExistedDeliveryNoteDialog")
        Dim dialogWnd = FindElementByID("wnd[2]")
        If IsNothing(dialogWnd) Then
            Return
        End If
        Dim status = dialogWnd.PopupDialogText.ToString()
        If status.StartsWith("Delivery note") And status.EndsWith("does not exist") Then
            Throw New MIR7Exception(InvoiceResult.Existed, "供应商发票不存在Hóa đơn Nhà cung cấp không tồn tại")
        Else
            FindElementByID("wnd[2]/tbar[0]/btn[0]").Press()
        End If
    End Sub
    Public Sub ValidateExistedPODialog()
        AddLog("ValidateExistedDeliveryNoteDialog")
        Dim dialogWnd = FindElementByID("wnd[2]")
        If IsNothing(dialogWnd) Then
            Return
        End If
        Dim status = dialogWnd.PopupDialogText.ToString()
        If status.StartsWith("Entry") And status.Contains("does not exist") Then
            Throw New MIR7Exception(InvoiceResult.Existed, "发票无PO号Số PO không tồn tại")
        Else
            FindElementByID("wnd[2]/tbar[0]/btn[0]").Press()
        End If
    End Sub
    Public Sub ValidateInvoicingParty()
        AddLog("ValidateInvoicingParty")
        Dim sessionWindows = Session.Children()
        If sessionWindows.Count > 1 Then
            Dim label = FindElementByID("wnd[1]/usr/lbl%#AUTOTEXT001")
            If label Is Nothing Then
                AddLog("Not found Invoicing Party")
                Return
            End If
            Return
            If label.Text = "PO vendor is not an invoicing party" Then
                Throw New MIR7Exception(InvoiceResult.Err, "Vendor is not defined in company code 8100")
            End If
        End If
    End Sub
#End Region

#Region "Handle"
    Public Sub HandlePostingDateWarning(Optional tryCount As Integer = 0)
        AddLog($"HandlePostingDateWarning {tryCount}")
        If tryCount > 10 Then
            AddLog("Handle Posting Date Loop more than 10 times. Posting date might not be opend")
            Throw New MIR7Exception(InvoiceResult.Err, "Handle Posting Date Loop more than 10 times. Posting date might not be opened")
        End If
        Dim status = ReadSessionStatusPane(False)
        If status.MessageType = SAPMessageType.Warning Then
            If status.MessageText.Contains("posting date") Then
                FindElementByID(InpPostingDateID).SetFocus()
                ExecuteAction()
            End If
        End If
        Dim popup = FindElementByID("wnd[1]")
        If Not IsNothing(popup) Then
            If popup.PopupDialogText.ToString().Contains("posting periods") Then
                Dim currentInvoiceDate = DateTime.ParseExact(FindElementByID(InpInvoiceDateID).Text, "dd.MM.yyyy", Nothing)
                popup.Close()
                tryCount += 1
                InputPostingDate(ToSAPDate(CalculatePostingDate(currentInvoiceDate)), tryCount)
                ExecuteAction()
            End If

        End If
    End Sub

    Public Sub HandleNetDueWarning()
        AddLog("HandleNetDueWarning")
        Dim status = ReadSessionStatusPane(False)
        If status.MessageType = SAPMessageType.Warning Then
            If status.MessageText.Contains("Net due") Then
                ExecuteAction()
                WaitLoading()
                DetectAndAcceptAllInformationModalWindow()
            End If
        End If
    End Sub

    Public Sub HandlePeroidAdjutedInLineWithPostingDate()
        Dim status = ReadSessionStatusPane(False)
        While status.MessageType = SAPMessageType.Warning
            If status.MessageText.StartsWith("Period") And status.MessageText.Contains("adjusted in line with posting date") Then
                Dim inpDate = FindElementByID(InpPostingDateID)
                'Focus and hit Enter to load
                inpDate.SetFocus()
                ExecuteAction()
            End If
            ExecuteAction()
            DetectAndAcceptAllInformationModalWindow()
            status = ReadSessionStatusPane(False)
        End While
    End Sub

    Public Sub HandlePostingTakesPlaceInPreviousFiscalYear()
        Dim status = ReadSessionStatusPane(False)
        While status.MessageType = SAPMessageType.Warning
            If status.MessageText = "Posting takes place in previous fiscal year" Then
                Dim inpDate = FindElementByID(InpPostingDateID)
                'Focus and hit Enter to load
                inpDate.SetFocus()
                ExecuteAction()
            End If
            ExecuteAction()
            DetectAndAcceptAllInformationModalWindow()
            status = ReadSessionStatusPane(False)
        End While
    End Sub

    Public Sub HandleFakeBackProcess(invoiceDate As Date)
        AddLog("HandleME23NChecked")
        Back(True)
        HandlePostingDateWarning()
        InputPostingDate(ToSAPDate(CalculatePostingDate(invoiceDate)))
    End Sub

    Public Sub DetectAndInputCompanyCodeUsingModalWindow(companyCode As String)
        AddLog("DetectAndInputCompanyCodeUsingModalWindow")
        'Detect and Input Company Code - Method 2 : Using Modal Window. 
        Dim sessionWindows = Session.Children()
        If sessionWindows.Count > 1 Then
            Session.FindById("wnd[1]/usr/ctxtBKPF-BUKRS").Text = companyCode
            Session.FindById("wnd[1]").SendVKey(0)
            WaitLoading()
        End If
    End Sub
    Public Sub DetectAndAcceptAllInformationModalWindow(Optional counter As Integer = 0)
        If counter > 100 Then
            AddLog("Cannot close all information modal window")
            Throw New MIR7Exception(InvoiceResult.Err, "Cannot close all information modal window")
        End If
        AddLog($"DetectAndAcceptAllInformationModalWindow {counter.ToString()}")
        Dim sessionWindows = Session.Children()
        If sessionWindows.Count > 1 Then
            Session.FindById("wnd[1]").SendVKey(0)
            WaitLoading()
            DetectAndAcceptAllInformationModalWindow(counter + 1)
        End If
    End Sub

#End Region


End Class
