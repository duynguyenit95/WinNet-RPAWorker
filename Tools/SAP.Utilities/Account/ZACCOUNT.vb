Imports RPA.Core.Models.SAP.ZACCOUNT

Public Class ZACCOUNTInput
    Public Property Account As IEnumerable(Of String) = Nothing '账号
    Public Property EmpId As IEnumerable(Of String) = Nothing '工号
    Public Property ProcessManager As IEnumerable(Of String) = Nothing '流程管理者
    Public Property Company As IEnumerable(Of String) = Nothing '公司
    Public Property Field As IEnumerable(Of String) = Nothing '模块
    Public Property ValidFrom As IEnumerable(Of String) = Nothing '有效从
    Public Property ValidTo As IEnumerable(Of String) = Nothing '有效至
End Class


Public Class ZACCOUNT
    Inherits SAPSessionBase

    Private ReadOnly _CompanyField As String = "wnd[0]/usr/txtS_SORT1-LOW"
    Private ReadOnly _ZaccountGrid As String = "wnd[0]/usr/cntlGRID1/shellcont/shell"
    Public Sub New(inpSessionStrID As String)
        MyBase.New(inpSessionStrID)
        OpenTcode("ZACCOUNT")
    End Sub

    Public Sub SearchAll()
        Dim inputCompany = FindElementByID(_CompanyField)
        inputCompany.Text = "*"
        Execute()
    End Sub
    Public Function ReadAccountData() As DataTable
        Dim grid = FindElementByID(_ZaccountGrid)
        If IsNothing(grid) Then
            Return Nothing
        End If
        Return GridViewToDataTable(grid)
    End Function
    Public Function SearchResult() As List(Of ZAccountResult)
        SearchAll()
        Dim dataTable = ReadAccountData()
        Dim zaccountResult As New List(Of ZAccountResult)
        For Each row As DataRow In dataTable.Rows

            zaccountResult.Add(New ZAccountResult() With {
                .Account = GetSafeValue(row, "BNAME"), '账号
                .SystemName = GetSafeValue(row, "NAME_LAST"), '系统名称
                .EmpId = GetSafeValue(row, "NICKNAME"), '工号
                .ValidFrom = GetSafeValue(row, "GLTGV"), '有效从
                .ValidTo = GetSafeValue(row, "GLTGB"), '有效至
                .Department1 = GetSafeValue(row, "ROOMNUMBER"), '部门1
                .Department2 = GetSafeValue(row, "FLOOR"), '部门2
                .Department3 = GetSafeValue(row, "DEPARTMENT"), '部门3
                .Department4 = GetSafeValue(row, "Function"), '部门4
                .Email = GetSafeValue(row, "EMAIL"), 'Email
                .Phone = GetSafeValue(row, "TEL_NUMBER"), '电话
                .ShortPhone = GetSafeValue(row, "TEL_EXTENS"), '短号
                .MobilePhone = GetSafeValue(row, "MOBLIE"), '手机
                .Company = GetSafeValue(row, "SORT1"), '公司
                .Field = GetSafeValue(row, "SORT2"), '模块
                .ProcessManager = GetSafeValue(row, "NAMEMIDDLE"), '流程管理者
                .LastLogin = GetSafeValue(row, "TRDAT") '最后登录
            })
        Next
        Return zaccountResult

    End Function

End Class

'Public Class ZAccountResult
'    {
'        Public String Account { Get; Set; } //BNAME
'        Public String SystemName { Get; Set; } //NAME_LAST
'        Public String EmpId { Get; Set; } //NICKNAME
'        Public String ValidFrom { Get; Set; } //GLTGV
'        Public String ValidTo { Get; Set; } //GLTGB
'        Public String Department1 { Get; Set; } //ROOMNUMBER
'        Public String Department2 { Get; Set; } //FLOOR
'        Public String Department3 { Get; Set; } //DEPARTMENT
'        Public String Department4 { Get; Set; } //Function
'        Public String Email { Get; Set; } //EMAIL
'        Public String Phone { Get; Set; } //TEL_NUMBER
'        Public String ShortPhone { Get; Set; } //TEL_EXTENS
'        Public String MobilePhone { Get; Set; } //MOBLIE
'        Public String Company { Get; Set; } //SORT1
'        Public String Field { Get; Set; } //SORT2
'        Public String ProcessManager { Get; Set; } //NAMEMIDDLE
'        Public String LastLogin { Get; Set; } //TRDAT
'    }
