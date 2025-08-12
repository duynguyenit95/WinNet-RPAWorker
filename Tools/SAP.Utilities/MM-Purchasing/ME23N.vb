Imports System.Data
Imports System.Globalization

Public Class ME23N
    Inherits SAPSessionBase
    Dim Log As String = String.Empty
    Public ReadOnly Property ExpandHeaderButtonID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB1:SAPLMEVIEWS:1100/subSUB1:SAPLMEVIEWS:4000/btnDYN_4000-BUTTON"
        End Get
    End Property
    Public ReadOnly Property ExpandItemOverviewID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB2:SAPLMEVIEWS:1100/subSUB1:SAPLMEVIEWS:4001/btnDYN_4000-BUTTON"
        End Get
    End Property
    Public ReadOnly Property ExpandItemDetailID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB3:SAPLMEVIEWS:1100/subSUB1:SAPLMEVIEWS:4002/btnDYN_4000-BUTTON"
        End Get
    End Property
    Public ReadOnly Property HeaderTitleID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB1:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEVIEWS:2000/txtDYN_2000-LABEL"
        End Get
    End Property
    Public ReadOnly Property ItemOverviewTitleID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB2:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEVIEWS:2000/txtDYN_2000-LABEL"
        End Get
    End Property
    Public ReadOnly Property ItemDetailTitleID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB3:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEVIEWS:2000/txtDYN_2000-LABEL"
        End Get
    End Property
    Public ReadOnly Property DeliveryScheduleTabID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB3:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1301/subSUB2:SAPLMEGUI:1303/tabsITEM_DETAIL/tabpTABIDT5"
        End Get
    End Property

    Public ReadOnly Property DeliveryScheduleTableID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB3:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1301/subSUB2:SAPLMEGUI:1303/tabsITEM_DETAIL/tabpTABIDT5/ssubTABSTRIPCONTROL1SUB:SAPLMEGUI:1320/tblSAPLMEGUITC_1320"
        End Get
    End Property

    Public ReadOnly Property ConditionsTabID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB3:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1301/subSUB2:SAPLMEGUI:1303/tabsITEM_DETAIL/tabpTABIDT9"
        End Get
    End Property

    Public ReadOnly Property ConditionsTableID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB3:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1301/subSUB2:SAPLMEGUI:1303/tabsITEM_DETAIL/tabpTABIDT9/ssubTABSTRIPCONTROL1SUB:SAPLMEGUI:1333/ssubSUB0:SAPLV69A:6201/tblSAPLV69ATCTRL_KONDITIONEN"
        End Get
    End Property

    Public ReadOnly Property POHistoryTabID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB3:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1301/subSUB2:SAPLMEGUI:1303/tabsITEM_DETAIL/tabpTABIDT14"
        End Get
    End Property

    Public ReadOnly Property POHistoryTableID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB3:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1301/subSUB2:SAPLMEGUI:1303/tabsITEM_DETAIL/tabpTABIDT14/ssubTABSTRIPCONTROL1SUB:SAPLMEGUI:1316/ssubPO_HISTORY:SAPLMMHIPO:0100/cntlMEALV_GRID_CONTROL_MMHIPO/shellcont/shell"
        End Get
    End Property

    Public ReadOnly Property POItemTableID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB2:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1211/tblSAPLMEGUITC_1211"
        End Get
    End Property

    Public ReadOnly Property ItemComboBoxID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB3:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1301/subSUB1:SAPLMEGUI:6000/cmbDYN_6000-LIST"
        End Get
    End Property


    Public ReadOnly Property HeaderTextsTabID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB1:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1102/tabsHEADER_DETAIL/tabpTABHDT3"
        End Get
    End Property

    Public ReadOnly Property HeaderTextsTabContentContainerID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB1:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1102/tabsHEADER_DETAIL/tabpTABHDT3/ssubTABSTRIPCONTROL2SUB:SAPLMEGUI:1230/subTEXTS:SAPLMMTE:0100/subEDITOR:SAPLMMTE:0101/cntlTEXT_EDITOR_0101/shellcont/shell"
        End Get
    End Property

    Public ReadOnly Property HeaderPartnerTabID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB1:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1102/tabsHEADER_DETAIL/tabpTABHDT6"
        End Get
    End Property

    Public ReadOnly Property HeaderOrgDataTabID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB1:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1102/tabsHEADER_DETAIL/tabpTABHDT9"
        End Get
    End Property

    Public ReadOnly Property POTextBoxID As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB0:SAPLMEGUI:0030/subSUB1:SAPLMEGUI:1105/txtMEPO_TOPLINE-EBELN"
        End Get
    End Property

    Public ReadOnly Property VendorTextField As String
        Get
            Return $"wnd[0]/usr/sub{UserAreaNumber}/subSUB0:SAPLMEGUI:0030/subSUB1:SAPLMEGUI:1105/ctxtMEPO_TOPLINE-SUPERFIELD"
        End Get
    End Property

    Function AppendPOColumnToTable(table As DataTable) As DataTable
        If Not IsNothing(table) Then
            table.Columns.Add("PO").SetOrdinal(0)
            If table.Rows.Count > 0 Then
                For i = 0 To table.Rows.Count - 1
                    table.Rows(i)("PO") = Session.FindById(POTextBoxID).Text
                Next
            End If
        End If
        Return table
    End Function

    Public Sub New(inpSessionStrID As String, Optional attachCurrentSession As Boolean = False)
        MyBase.New(inpSessionStrID, attachCurrentSession)
        LoadUserAreaNumber()
        If Not attachCurrentSession Then
            OpenTcode("ME23N")
        End If
    End Sub

    Sub TryExpandAllContainer()
        TryExpandHeaderContainer()
        TryExpandItemOverview()
        TryExpandItemDetails()
    End Sub
    Sub TryExpandHeaderContainer()
        'Expand Header
        If Not IsNothing(FindElementByID(HeaderTitleID)) Then
            FindElementByID(ExpandHeaderButtonID).Press
        End If
        WaitLoading()
        LoadUserAreaNumber()
    End Sub

    Sub TryExpandItemOverview()
        'Expand ItemOverview
        If Not IsNothing(FindElementByID(ItemOverviewTitleID)) Then
            FindElementByID(ExpandItemOverviewID).Press
        End If
        WaitLoading()

        LoadUserAreaNumber()
    End Sub
    Sub TryExpandItemDetails()
        'Expand ItemDetails
        If Not IsNothing(FindElementByID(ItemDetailTitleID)) Then
            FindElementByID(ExpandItemDetailID).Press
        End If
        WaitLoading()
        LoadUserAreaNumber()
    End Sub

    ''' <summary>
    ''' Try Input PO 
    ''' Return True if PO data is loaded successfully
    ''' Else return false
    ''' </summary>
    ''' <param name="PO"></param>
    ''' <returns></returns>
    Public Function InputPO(PO As String) As Boolean
        Session.FindById("wnd[0]/tbar[1]/btn[17]").Press()
        WaitLoading()
        Dim inpPOControl = Session.FindById("wnd[1]/usr/subSUB0:SAPLMEGUI:0003/ctxtMEPO_SELECT-EBELN")
        inpPOControl.Text = PO
        Session.FindById("wnd[1]").SendVKey(0)
        WaitLoading()
        LoadUserAreaNumber()
        Dim message = ReadSessionStatusPane()
        If message.MessageText.Contains("does not exist") Then
            Return False
        End If
        Return True
    End Function

    Public Function ReadPOHeaderText() As String
        TryExpandHeaderContainer()
        'Select HeaderText Tab
        Session.FindById(HeaderTextsTabID).Select()
        LoadUserAreaNumber()
        'Selecte Header Text TreeNode
        Session.FindById($"wnd[0]/usr/sub{UserAreaNumber}/subSUB1:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1102/tabsHEADER_DETAIL/tabpTABHDT3/ssubTABSTRIPCONTROL2SUB:SAPLMEGUI:1230/subTEXTS:SAPLMMTE:0100/cntlTEXT_TYPES_0100/shell").SelectedNode = "F01"
        Dim textEdit = Session.FindById(HeaderTextsTabContentContainerID)
        If Not IsNothing(textEdit) Then
            Return textEdit.Text
        Else
            Return String.Empty
        End If
    End Function

    Public Function ReadHeaderOrgData() As DataTable
        Dim table = New DataTable()
        table.Columns.Add("PurchOrg")
        table.Columns.Add("PurchGroup")
        table.Columns.Add("CompanyCode")
        Dim newRow = table.NewRow()
        TryExpandHeaderContainer()
        'Select OrgData Tab
        Session.FindById(HeaderOrgDataTabID).Select()
        LoadUserAreaNumber()
        'Selecte Header Text TreeNode
        newRow("PurchOrg") = Session.FindById($"wnd[0]/usr/sub{UserAreaNumber}/subSUB1:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1102/tabsHEADER_DETAIL/tabpTABHDT9/ssubTABSTRIPCONTROL2SUB:SAPLMEGUI:1221/ctxtMEPO1222-EKORG").Text
        newRow("PurchGroup") = Session.FindById($"wnd[0]/usr/sub{UserAreaNumber}/subSUB1:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1102/tabsHEADER_DETAIL/tabpTABHDT9/ssubTABSTRIPCONTROL2SUB:SAPLMEGUI:1221/ctxtMEPO1222-EKGRP").Text
        newRow("CompanyCode") = Session.FindById($"wnd[0]/usr/sub{UserAreaNumber}/subSUB1:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1102/tabsHEADER_DETAIL/tabpTABHDT9/ssubTABSTRIPCONTROL2SUB:SAPLMEGUI:1221/ctxtMEPO1222-BUKRS").Text
        table.Rows.Add(newRow)
        table = AppendPOColumnToTable(table)

        Return table
    End Function

    Public Function ReadHeaderPartner() As DataTable
        'Select Partner Tab
        Session.FindById(HeaderPartnerTabID).Select()
        LoadUserAreaNumber()
        Dim tableID = $"wnd[0]/usr/sub{UserAreaNumber}/subSUB1:SAPLMEVIEWS:1100/subSUB2:SAPLMEVIEWS:1200/subSUB1:SAPLMEGUI:1102/tabsHEADER_DETAIL/tabpTABHDT6/ssubTABSTRIPCONTROL2SUB:SAPLMEGUI:1224/subPARTNERS:SAPLEKPA:0111/tblSAPLEKPATC_0111"
        Dim table = SAPTableControlToDataTable(tableID, 0)
        table = AppendPOColumnToTable(table)
        Return table
    End Function

    Public Sub SelectDeliveryScheduleTab()
        Session.FindById(DeliveryScheduleTabID).Select()
        WaitLoading()
        LoadUserAreaNumber()
    End Sub
    Public Sub SelectConditionTab()
        Session.FindById(ConditionsTabID).Select()
        WaitLoading()
        LoadUserAreaNumber()
    End Sub

    Public Sub SelectPOHistoryTab()
        Session.FindById(POHistoryTabID).Select()
        WaitLoading()
        LoadUserAreaNumber()
    End Sub

    Public Function GetItemMapping(itemNos As List(Of String)) As Dictionary(Of String, String)
        Dim itemKeyMapping = New Dictionary(Of String, String)
        Dim itemComboBox = FindElementByID(ItemComboBoxID)
        Dim entries = itemComboBox.Entries
        For i = 0 To entries.Count - 1
            Dim value = entries.Item(i).Value.ToString()
            If value.IndexOf("[") > -1 Then
                Dim itemNo = value.Substring(value.IndexOf("[") + 1, value.IndexOf("]") - value.IndexOf("[") - 1).Trim()
                If IsNothing(itemNos) Then
                    itemKeyMapping.Add(entries(i).Key, itemNo)
                Else
                    If itemNos.Any(Function(x) x = itemNo) Then
                        itemKeyMapping.Add(entries(i).Key, itemNo)
                    End If
                End If
            End If
        Next
        Return itemKeyMapping
    End Function

    Public Sub SelectPOItem(poItems As String)
        Dim itemComboBox = FindElementByID(ItemComboBoxID)
        Dim entries = itemComboBox.Entries
        For i = 0 To entries.Count - 1
            Dim value = entries.Item(i).Value.ToString()
            If value.IndexOf("[") > -1 Then
                Dim itemNo = value.Substring(value.IndexOf("[") + 1, value.IndexOf("]") - value.IndexOf("[") - 1).Trim()
                If poItems = itemNo Then
                    itemComboBox.Key = entries(i).Key
                    Exit For
                End If
            End If
        Next
    End Sub

    Public Function GetLatestDeliveryDate(refDocs As List(Of RefDocItem)) As Date
        Dim table = FindElementByID(POHistoryTableID)
        Dim dataTable = GridViewToDataTable(table)
        Dim parseDate As Date
        Dim latestDate As New Date
        'Dim result As String   
        If dataTable.Rows.Count = 0 Then
            Return latestDate
        End If

        For Each item In refDocs
            For Each row As DataRow In dataTable.Rows
                ' Ignore blank row 
                If String.IsNullOrEmpty(row("BELNR").ToString()) Or String.IsNullOrEmpty(row("BUZEI").ToString()) Then
                    Continue For
                End If
                If row("BELNR").ToString() <> item.RefDoc Or row("BUZEI").ToString() <> item.RefDocItemNo Then
                    Continue For
                End If
                If Not Date.TryParseExact(row("BUDAT"), "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, parseDate) Then
                    Continue For
                End If
                If parseDate > latestDate Then
                    latestDate = parseDate
                End If
            Next
        Next


        Return latestDate
    End Function


    Public Function ReadItemConditionsTable(itemNos As List(Of String), Optional readSKUDescription As Boolean = False) As DataTable
        TryExpandItemDetails()
        SelectConditionTab()
        Dim itemKeyMapping = GetItemMapping(itemNos)
        Dim readSKU = New Dictionary(Of String, String)

        Dim resultTable = New DataTable()

        For i = 0 To itemKeyMapping.Count - 1
            Dim itemComboBox = FindElementByID(ItemComboBoxID)
            itemComboBox.Key = itemKeyMapping.ElementAt(i).Key.ToString()
            WaitLoading()
            Dim itemTable = SAPTableControlToDataTable(ConditionsTableID, 2)



            'ReadSKU Description
            If readSKUDescription Then
                itemTable.Columns.Add("SKUDescription")
                For rowIndex = 0 To itemTable.Rows.Count - 1
                    Dim sku = itemTable.Rows(rowIndex)("GrV_19").ToString()
                    If String.IsNullOrEmpty(sku) Then
                        Continue For
                    End If
                    If readSKU.Any(Function(x) x.Key = sku) Then
                        itemTable.Rows(rowIndex)("SKUDescription") = readSKU.First(Function(x) x.Key = sku).Value
                    Else
                        'Reload table after each operation
                        Dim conditionTable = Session.FindById(ConditionsTableID)
                        'Scroll to row then process row will be at index 0.
                        conditionTable = ScrollSAPTableVertically(ConditionsTableID, rowIndex)
                        conditionTable = ScrollSAPTableHorizontally(ConditionsTableID, 11)

                        'Use index = 0 to read first cell
                        Dim cell = conditionTable.GetCell(0, 19)
                        cell.SetFocus()
                        cell.CaretPosition = 6
                        ' Open Grid Value Description
                        Session.FindById("wnd[0]").SendVKey(4)
                        WaitLoading()
                        ' Click Search
                        Session.FindById("wnd[1]/tbar[0]/btn[71]").Press()
                        WaitLoading()

                        ' Input search value then press Enter
                        Session.FindById("wnd[2]/usr/txtRSYSF-STRING").Text = sku
                        Session.FindById("wnd[2]").SendVKey(0)
                        WaitLoading()

                        ' Extract SKUDescription
                        Dim resultWindow = Session.FindById("wnd[3]/usr")
                        Dim value = resultWindow.Children.ElementAt(resultWindow.Children.Count - 1).Text
                        itemTable.Rows(rowIndex)("SKUDescription") = value
                        readSKU.Add(sku, value)

                        ' Close all Modal
                        Session.FindById("wnd[3]/tbar[0]/btn[12]").Press()
                        WaitLoading()
                        Session.FindById("wnd[2]/tbar[0]/btn[12]").Press()
                        WaitLoading()
                        Session.FindById("wnd[1]/tbar[0]/btn[12]").Press()
                        WaitLoading()
                    End If
                Next
            End If

            'Get Blank Row
            Dim blankRows = itemTable.Rows.Cast(Of DataRow).Where(
            Function(x)
                Return IsDBNull(x("SLNo_1"))
            End Function).ToArray()

            'Delete Blank Row
            For Each row In blankRows
                itemTable.Rows.Remove(row)
            Next



            'Add PO Column
            itemTable.Columns.Add("POItemNo").SetOrdinal(0)
            For Each row In itemTable.Rows
                row("POItemNo") = itemKeyMapping.ElementAt(i).Value
            Next

            'Merge Table
            If i = 0 Then
                resultTable = itemTable
            Else
                resultTable.Merge(itemTable)
            End If
        Next
        Return resultTable
    End Function

    Public Function ReadItemDeliveryScheduleTable(itemNos As List(Of String)) As DataTable
        SelectDeliveryScheduleTab()
        Dim itemKeyMapping = GetItemMapping(itemNos)

        Dim resultTable = New DataTable()
        For i = 0 To itemKeyMapping.Count - 1
            FindElementByID(ItemComboBoxID).Key = itemKeyMapping.ElementAt(i).Key.ToString()
            WaitLoading()
            Dim itemTable = SAPTableControlToDataTable(DeliveryScheduleTableID, 4)
            itemTable.Columns.Add("POItemNo")
            For Each row In itemTable.Rows
                row("POItemNo") = itemKeyMapping.ElementAt(i).Value
            Next
            If i = 0 Then
                resultTable = itemTable
            Else
                resultTable.Merge(itemTable)
            End If
        Next

        'Clean Blank Row
        Dim blankRows = resultTable.Rows.Cast(Of DataRow).Where(
            Function(x)
                Return String.IsNullOrEmpty(x("Grid Value_3"))
            End Function).ToArray()

        For Each row In blankRows
            resultTable.Rows.Remove(row)
        Next

        Return resultTable
    End Function

    Public Function ReadPOItemTable() As DataTable
        TryExpandItemOverview()
        Dim table = SAPTableControlToDataTable(POItemTableID, 1)
        Return table
    End Function

    Public Function ReadVendorInfo() As String
        Return If(FindElementByID(VendorTextField)?.Text, String.Empty)
    End Function

End Class
