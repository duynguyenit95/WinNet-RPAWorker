
Imports System.Data
Imports RPA.Core

Public Class XK03Input

    Public Property VendorID As String
    Public Property CompanyCode As String
    Public Property PurchOrg As String
    Public Overrides Function ToString() As String
        Return $"{VendorID}-{CompanyCode}-{PurchOrg}"
    End Function
End Class


Public Class XK03Data
    Public Property VendorID As String
    Public Property CompanyCode As String
    Public Property PurchOrg As String
End Class
Public Class XK03Address
    Public Property EmailTable As DataTable
End Class

Public Class XK03
    Inherits SAPSessionBase
    Public ReadOnly Property InpVendorID As String
        Get
            Return $"wnd[0]/usr/ctxtRF02K-LIFNR"
        End Get
    End Property

    Public ReadOnly Property InpCompanyCodeID As String
        Get
            Return $"wnd[0]/usr/ctxtRF02K-BUKRS"
        End Get
    End Property

    Public ReadOnly Property InpPurchOrgID As String
        Get
            Return $"wnd[0]/usr/ctxtRF02K-EKORG"
        End Get
    End Property

    Public ReadOnly Property AddressCheckBoxID As String
        Get
            Return $"wnd[0]/usr/chkRF02K-D0110"
        End Get
    End Property
    Public Sub New(inpSessionStrID As String)
        MyBase.New(inpSessionStrID)
        OpenTcode("XK03")
    End Sub


    Public Function ReadAddress(data As XK03Input) As XK03Address
        Dim result = New XK03Address()
        Dim addressCheckBox = Session.FindById(AddressCheckBoxID)
        If Not addressCheckBox.Selected Then
            addressCheckBox.Selected = True
        End If

        ' Input 
        Session.FindById(InpVendorID).Text = data.VendorID
        Session.FindById(InpCompanyCodeID).Text = data.CompanyCode
        Session.FindById(InpPurchOrgID).Text = data.PurchOrg
        Session.FindById("wnd[0]").SendVKey(0)
        WaitLoading()

        ' Check Status
        Dim a = ReadSessionStatusPane()
        If a.MessageType = "E" Then
            result.EmailTable = New DataTable()
            result.EmailTable.Columns.Add("Emails")
            result.EmailTable.Rows.Add(a.MessageText)
            Return result
        End If


        ' Read Data 
        ' Open Email Modal
        Session.FindById("wnd[0]/usr/subADDRESS:SAPLSZA1:0300/subCOUNTRY_SCREEN:SAPLSZA1:0301/btnG_ICON_SMTP").Press()
        WaitLoading()
        Dim modal = Session.FindById("wnd[1]")
        If Not IsNothing(modal) Then
            Dim EmailTableID = "wnd[1]/usr/tblSAPLSZA6T_CONTROL6"
            result.EmailTable = SAPTableControlToDataTable(EmailTableID, 0)
            ' Press Enter to close Modal
            Session.FindById("wnd[1]").SendVKey(0)
            WaitLoading()
            'Back to Input Screen
            Session.FindById("wnd[0]/tbar[0]/btn[3]").Press()
            WaitLoading()
        End If
        Return result
    End Function
End Class
