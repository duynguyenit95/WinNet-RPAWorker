
Imports System.Data
Imports System.IO
Imports System.Threading
Imports System.Windows.Forms
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports FlaUI.UIA3
Imports FlaUI.Core.Conditions
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.AutomationElements
Imports System.Net.Http

Public Class SAPMessageType
    Public Const Warning As String = "W"
    Public Const ErrorType As String = "E"
    Public Const Information As String = "I"
    Public Const Success As String = "S"
    Public Const Abort As String = "A"
End Class

Public MustInherit Class SAPSessionBase
    Implements IDisposable

    Private ReadOnly _ExecuteButton As String = "wnd[0]/tbar[1]/btn[8]"
    Private ReadOnly _BackButton As String = "wnd[0]/tbar[0]/btn[3]"
    Private ReadOnly _PopupCancelButton As String = "wnd[1]/usr/btnSPOP-OPTION_CAN"

    Private ReadOnly _MultipleValueModalInputTable As String = "wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE"
    Private ReadOnly _MultipleValueModalDeleteButton As String = "wnd[1]/tbar[0]/btn[16]"
    Private ReadOnly _MultipleValueModalPasteButton As String = "wnd[1]/tbar[0]/btn[24]"
    Private ReadOnly _MultipleValueModalFinishButton As String = "wnd[1]/tbar[0]/btn[8]"
    Private ReadOnly _MultipleValueModalInsertLine As String = "wnd[1]/tbar[0]/btn[13]"

    Public ReadOnly Property Session As Object
    Public UserAreaNumber As String
    'Public LogStorage As List(Of String) = New List(Of String)
    Public Event OnLogMessage(message As String)

    Public Property DefaultWindow As String = "wnd[0]"


    Public Sub New(inpSessionStrID As String, Optional attach As Boolean = False)
        Dim SapGuiAuto = GetObject("SAPGUI")
        Dim SapApplication = SapGuiAuto.GetScriptingEngine
        Session = SapApplication.FindById(inpSessionStrID)
        'Try get back to default session Tcode IF not attach current session
        If Not attach Then
            ExitTCode()
        End If
    End Sub


    'Capture screenshot of SAP GUI window and launch 
    Public Function CaptureScreen() As String
        Try
            WaitLoading()
            Dim wnd = Session.FindById("wnd[0]")
            ' Ensure the SAP session window is not minimized
            If wnd.Iconic Then
                SetForeGround(wnd.Handle)
            End If

            Dim Image = wnd.HardCopyToMemory()
            Dim BinaryStream = CreateObject("ADODB.Stream")
            BinaryStream.Type = 1
            BinaryStream.Open
            BinaryStream.Write(Image)
            Dim tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScreenShot", Guid.NewGuid.ToString + ".png")
            BinaryStream.SaveToFile(tempPath, 2)
            ' Close the stream
            BinaryStream.Close()
            Dim file As New FileInfo(tempPath)
            'Send http client to upload file

            Return file.Name
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
    Public Async Function HttpClientUploadFile(file As FileInfo) As Task(Of String)
        Dim content = New MultipartFormDataContent()
        content.Add(New StreamContent(file.OpenRead()), "file", file.Name)
        Using httpClient As New HttpClient
            Dim response = Await httpClient.PostAsync("http://localhost:5000/pid135/uploadimage", content)
            Dim responseString = Await response.Content.ReadAsStringAsync()
            Return responseString
        End Using
    End Function

    Public Sub SetForeGround(handle As Integer)
        Try
            Using automation As New UIA3Automation
                Dim rootElement = automation.GetDesktop()
                Dim condition = New PropertyCondition(automation.PropertyLibrary.Element.NativeWindowHandle, handle)
                Dim sapSessions = rootElement.FindAll(TreeScope.Children, condition)

                Dim target = sapSessions(0).AsWindow()
                If target.Patterns.Window.Pattern.WindowVisualState.Value = WindowVisualState.Minimized Then
                    target.Patterns.Window.Pattern.SetWindowVisualState(WindowVisualState.Normal)
                End If
                'Set Foreground using FlaUI
                target.SetForeground()
            End Using
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Public Sub Back(Optional FakeBack As Boolean = False)
        FindElementByID(_BackButton).Press()
        If FakeBack Then
            FindElementByID(_PopupCancelButton).Press()
        End If
    End Sub

    Public Sub Execute()
        FindElementByID(_ExecuteButton).Press()
        WaitLoading()
    End Sub

    Public Sub HandleMultipleSingleValueInput(values As IEnumerable(Of String))
        Session.FindById(_MultipleValueModalDeleteButton).Press()
        WaitLoading()
        If values.Any() Then
            Dim array = values.ToArray()
            Dim processed = 0
            Dim multipleInputTable = FindElementByID(_MultipleValueModalInputTable)
            While True
                For Each row In multipleInputTable.Rows
                    If String.IsNullOrEmpty(row.Item(1).Text) Then
                        row.Item(1).Text = array(processed)
                        processed += 1
                        If processed >= array.Length Then
                            Exit While
                        End If
                    End If
                Next
                multipleInputTable.VerticalScrollBar.Position = multipleInputTable.VerticalScrollBar.Position + multipleInputTable.VerticalScrollBar.PageSize
                WaitLoading()
                multipleInputTable = FindElementByID(_MultipleValueModalInputTable)
            End While
        End If
        Session.FindById(_MultipleValueModalFinishButton).Press()
    End Sub

    Public Function GridViewToDataTable(grid As Object, Optional readFromStart As Boolean = True) As DataTable
        AddLog($"Read Grid: {grid.Id}")
        Dim table = New DataTable()
        Dim columns = grid.ColumnOrder
        For Each item In columns
            table.Columns.Add(item)
        Next
        If readFromStart Then
            grid.FirstVisibleRow = 0
            WaitLoading()
            grid = FindElementByID(grid.Id)
        End If
        Dim rowCount = CInt(grid.RowCount)
        AddLog($"Total Row:{rowCount}")
        If rowCount = 0 Then
            Return table
        End If

        Dim visibleRowCount = CInt(grid.VisibleRowCount)
        AddLog($"VisibleRowCount:{visibleRowCount}")
        Dim readCount = 0
        Dim reachEnd = False

        While Not reachEnd
            Dim startRow = CInt(grid.FirstVisibleRow)
            Dim rowReadyToRead = visibleRowCount
            ' Check if reach end 
            If rowCount - readCount <= visibleRowCount Then
                rowReadyToRead = rowCount - readCount
                startRow = readCount
                reachEnd = True
                AddLog($"ReachEnd:{reachEnd}")
            End If
            'Read data
            For i = 0 To rowReadyToRead - 1
                Dim rowIndex = startRow + i
                'AddLog($"Read Row Index:{rowIndex}")
                Dim newRow = table.NewRow()
                For colIndex = 0 To table.Columns.Count - 1
                    newRow(colIndex) = grid.GetCellValue(rowIndex, table.Columns(colIndex).ColumnName)
                Next
                table.Rows.Add(newRow)
            Next
            readCount += rowReadyToRead
            AddLog($"Progress {readCount}/{rowCount}")
            ' Set FirstVisibleRow to Scroll Grid 
            ' Cannot Read Non-VisibleRow
            If Not reachEnd Then
                grid.FirstVisibleRow = grid.FirstVisibleRow + visibleRowCount
                WaitLoading()
                grid = FindElementByID(grid.Id)
            End If
        End While
        Return table
    End Function

    Sub SetClipBoard(value As String)
        Dim staThread = New Thread(Sub() My.Computer.Clipboard.SetText(value))
        staThread.SetApartmentState(ApartmentState.STA)
        staThread.Start()
        staThread.Join()
    End Sub

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
        Try
            ExitTCode()
        Catch ex As Exception

        End Try

    End Sub


    Public Sub LoadUserAreaNumber()
        UserAreaNumber = FindMainAreaName()
    End Sub

    ''' <summary>
    ''' Read Status Pane
    ''' </summary>
    ''' <returns></returns>
    Public Function ReadSessionStatusPane(Optional ignoreNonErrorMessage As Boolean = True) As SAPSessionStatusMessage
        Dim statusBar = Session.FindById("wnd[0]/sbar")
        Dim statusMessage = New SAPSessionStatusMessage() With {
            .MessageText = statusBar.Text,
            .MessageType = statusBar.MessageType
}
        If ignoreNonErrorMessage Then
            If statusMessage.MessageType = SAPMessageType.Warning Or statusMessage.MessageType = SAPMessageType.Information Then
                ExecuteAction()
            End If
        End If
        Return statusMessage
    End Function

    Public Sub ReadInformationDialog()
        Dim infor = Session.FindById("wnd[1]/usr")

    End Sub

    ''' <summary>
    ''' Try get element by ID, return nothing if not fund
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Public Function FindElementByID(id As String)
        Try
            Return Session.FindById(id)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Wait Session transaction complete
    ''' </summary>
    Public Sub WaitLoading(Optional interval As Integer = 500)
        ' Wait loading
        Do While Session.Busy
            Threading.Thread.Sleep(interval)
        Loop
    End Sub
    Public Async Function WaitLoadingAsync(Optional interval As Integer = 500) As Task
        ' Wait loading
        Do While Session.Busy
            Await Task.Delay(interval)
        Loop
    End Function
    Public Sub DetectAndCloseAllExtraModal()
        Try
            While Session.Children.Count > 1
                ' Get last modal
                Dim wnd = Session.Children(Session.Children.Count - 1)
                wnd.Close()
                WaitLoading()
            End While
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Open New TCode
    ''' Make sure you're at SAP Home Page - SAP Easy access
    ''' </summary>
    ''' <param name="tcode"></param>
    Public Sub OpenTcode(tcode As String)
        If Session.Info.Transaction <> tcode Then
            'Input Code
            Session.FindById("wnd[0]/tbar[0]/okcd").Text = tcode
            'Press Enter 
            Session.FindById("wnd[0]").SendVKey(0)
        End If
        WaitLoading()
    End Sub

    Public Sub ExecuteAction()
        Session.FindById("wnd[0]").SendVKey(0)
        WaitLoading()
    End Sub

    ''' <summary>
    ''' Try close any modal before exit
    ''' Force Exit T-code, cancel all data
    ''' </summary>
    Public Sub ExitTCode()
        DetectAndCloseAllExtraModal()
        Dim complete = False
        Do While Not complete
            Dim backButton = Session.FindById("wnd[0]/tbar[0]/btn[3]")
            'Complete Exit when back button is disable
            If backButton.Changeable Then
                'Click Exit Button
                Session.FindById("wnd[0]/tbar[0]/btn[15]").Press()
                WaitLoading()
                Dim dialog = FindElementByID("wnd[1]")
                If Not IsNothing(dialog) Then
                    'Click No Button
                    Session.FindById("wnd[1]/usr/btnSPOP-OPTION2").Press()
                End If
                WaitLoading()
            Else
                complete = True
            End If
        Loop
    End Sub

    ''' <summary>
    ''' Find MainAreaName in T-code that UserArea ID change dynamically
    ''' </summary>
    ''' <returns></returns>
    Public Function FindMainAreaName() As String
        Dim MainArea As Object
        ' Main AreaName are different each time request
        ' Find MainAreaName
        Dim MainAreaName As String = ""
        Dim User = Session.findById("wnd[0]/usr")
        For i = 0 To User.Children.Count - 1
            MainAreaName = User.Children(CInt(i)).Name
            If Left(MainAreaName, 15) = "SUB0:SAPLMEGUI:" Then
                Exit For
            End If
        Next
        AddLog("Main Area Name :" + MainAreaName)

        ' Find MainArea
        MainArea = Session.findById("wnd[0]/usr/sub" + MainAreaName)
        AddLog(" Main Area Found :" + MainArea.Type)
        Return MainAreaName
    End Function

    ''' <summary>
    ''' Scroll SAP Table Control
    ''' </summary>
    ''' <param name="SAPTableID"></param>
    ''' <param name="NewScrollBarPosition"></param>
    Public Function ScrollSAPTableVertically(SAPTableID As String,
                                             NewScrollBarPosition As Long) As Object
        Dim SAPTable = FindElementByID(SAPTableID)
        Dim currentPosition = SAPTable.VerticalScrollbar.Position
        Dim retryCount = 0
        If NewScrollBarPosition > SAPTable.VerticalScrollbar.Maximum Then
            NewScrollBarPosition = SAPTable.VerticalScrollbar.Maximum
        End If

        If NewScrollBarPosition < SAPTable.VerticalScrollbar.Minimum Then
            NewScrollBarPosition = SAPTable.VerticalScrollbar.Minimum
        End If
        AddLog("NewScrollBarPosition :" + CStr(NewScrollBarPosition))

        ' When All visible row is processed . Scroll down 
        Do While currentPosition = SAPTable.VerticalScrollbar.Position And retryCount < 3
            'Scroll Table
            SAPTable.VerticalScrollbar.Position = NewScrollBarPosition
            WaitLoading()
            'Ignore popup notification
            DetectAndCloseAllExtraModal()
            'Reload After Scroll 
            SAPTable = FindElementByID(SAPTableID)
            ' Detect Error or Warning while scroll. 
            Dim statusBar = ReadSessionStatusPane()
            If Not String.IsNullOrEmpty(statusBar.MessageText) Then
                If (statusBar.MessageType = "W") Then
                    'Exit Do
                ElseIf statusBar.MessageType = "E" Then
                    Back(True)
                End If
                SAPTable = FindElementByID(SAPTableID)
                'Reset Position to scroll again
                SAPTable.VerticalScrollbar.Position = currentPosition
                SAPTable = FindElementByID(SAPTableID)
            End If
            retryCount += 1
        Loop

        AddLog("Vertical Scroll Bar Position :" + CStr(SAPTable.VerticalScrollbar.Position))
        Return SAPTable
    End Function

    Public Function ScrollSAPTableHorizontally(SAPTableID As String,
                                             NewScrollBarPosition As Long) As Object
        Dim SAPTable = FindElementByID(SAPTableID)
        Dim currentPosition = SAPTable.HorizontalScrollbar.Position
        Dim retryCount = 0
        If NewScrollBarPosition > SAPTable.HorizontalScrollbar.Maximum Then
            NewScrollBarPosition = SAPTable.HorizontalScrollbar.Maximum
        End If

        If NewScrollBarPosition < SAPTable.HorizontalScrollbar.Minimum Then
            NewScrollBarPosition = SAPTable.HorizontalScrollbar.Minimum
        End If
        AddLog("NewScrollBarPosition :" + CStr(NewScrollBarPosition))
        ' When All visible row is processed . Scroll down 
        Do While currentPosition = SAPTable.HorizontalScrollbar.Position And retryCount < 3
            'Scroll Table
            SAPTable.HorizontalScrollbar.Position = NewScrollBarPosition
            WaitLoading()
            'Reload After Scroll 
            SAPTable = Session.findById(SAPTableID)
            ' Detect Error or Warning while scroll. 
            Dim statusBar = ReadSessionStatusPane()
            If Not String.IsNullOrEmpty(statusBar.MessageText) Then
                If (statusBar.MessageType = "W" Or statusBar.MessageType = "E") Then
                    'Reset Position to scroll again
                    SAPTable.HorizontalScrollbar.Position = currentPosition
                    SAPTable = FindElementByID(SAPTableID)
                End If
            End If
            retryCount += 1
        Loop
        AddLog("Horizontal Scroll Bar Position :" + CStr(SAPTable.VerticalScrollbar.Position))
        Return SAPTable
    End Function

    Public Function SAPTableControlToDataTable(SAPTableID As String,
                                                KeyColumnIndex As Integer, Optional UniqueKey As Boolean = False) As DataTable
        Dim Table = New DataTable()

        Dim SAPTable As Object
        Dim SAPTableColsCollection As Object
        Dim SAPTableVisibleRowCount As Object
        Dim Keys = New List(Of String)

        ' Try Get Table
        SAPTable = FindElementByID(SAPTableID)
        SAPTableColsCollection = SAPTable.Columns
        SAPTableVisibleRowCount = SAPTable.VisibleRowCount
        Dim RowCount = SAPTable.RowCount
        Dim ColCount = SAPTableColsCollection.Count
        Dim CurrentScrollBarPosition = SAPTable.VerticalScrollbar.Position
        ' Try Parse Table
        Try
            ' Read SapTable Columns
            SAPTableColsCollection = SAPTable.Columns
            For index As Integer = 0 To ColCount - 1
                Dim Col = SAPTableColsCollection.ElementAt(index)
                Dim Title = Col.Title + "_" + CStr(index)
                Table.Columns.Add(Title)
            Next
            Table.Columns.Add("IsSelected")



            ' Parse SapTable 
            ' Set Vertical to Minimum to start parsing from start
            If SAPTable.VerticalScrollbar.Position <> SAPTable.VerticalScrollbar.Minimum Then
                SAPTable = ScrollSAPTableVertically(SAPTableID, SAPTable.VerticalScrollbar.Minimum)
            End If

            Dim NewScrollBarPosition As Long = 0
            Dim ProcessedRow As Integer = 0
            Dim BlankRowFlag As Boolean = False
            Dim DuplicationKeyFlag As Boolean = False

            ' While havent processed all rows and blank row is not found
            While (ProcessedRow < RowCount And Not BlankRowFlag And Not DuplicationKeyFlag)
                'Minus 1 since index start from 0
                Dim LastDisplayRow = SAPTableVisibleRowCount - 1 + NewScrollBarPosition

                ' Parse Row
                Dim rIndex = 0
                For Each row In SAPTable.Rows
                    If rIndex >= SAPTableVisibleRowCount Or row.Count = 0 Then
                        BlankRowFlag = True
                        Exit For
                    End If
                    ' Check if ID col Cell is blank then Stop Read 
                    Dim KeyCell = SAPTable.GetCell(rIndex, KeyColumnIndex)
                    If Len(KeyCell.Text) = 0 Then
                        BlankRowFlag = True
                        Exit For
                    End If
                    ' Check if current row key is exists
                    If (UniqueKey) Then
                        DuplicationKeyFlag = Keys.Any(Function(x As String)
                                                          Return x = KeyCell.Text
                                                      End Function)
                        If DuplicationKeyFlag Then
                            Exit For
                        End If
                    End If
                    ' Add Keys
                    Keys.Add(KeyCell.Text)

                    ' All Conditions are pass then start parse column
                    Dim newRow = Table.NewRow()
                    ' Parse Column
                    For cIndex As Integer = 0 To ColCount - 1
                        Try
                            Dim Cell = SAPTable.GetCell(rIndex, cIndex)
                            Dim cellText = Cell.Text
                            If String.IsNullOrEmpty(cellText) Then
                                newRow(cIndex) = Cell.Tooltip
                            Else
                                newRow(cIndex) = cellText
                            End If
                        Catch ex As Exception

                        End Try
                    Next
                    'Check row Is selected Or Not
                    newRow("IsSelected") = CBool(SAPTable.Rows.Item(rIndex).Selected)
                    ' Add new Row to Table
                    Table.Rows.Add(newRow)
                    ProcessedRow += 1
                    rIndex += 1
                Next


                If (Not BlankRowFlag And Not DuplicationKeyFlag) Then
                    NewScrollBarPosition = CLng(SAPTable.VerticalScrollbar.Position) + CLng(SAPTableVisibleRowCount)
                    SAPTable = ScrollSAPTableVertically(SAPTableID, NewScrollBarPosition)
                End If
            End While

        Catch e As Exception
            Dim a = 1
            a += 1
        Finally
            SAPTable = FindElementByID(SAPTableID)
            SAPTable = ScrollSAPTableVertically(SAPTableID, CurrentScrollBarPosition)
        End Try

        Return Table
    End Function

    Public Function ToSAPDate(pDate As DateTime) As String
        Return pDate.ToString("dd.MM.yyyy")
    End Function

    Public Function ToSAPNumber(pNum As String) As String
        Return pNum.Replace(".", ",")
    End Function
    Public Function RevertSAPNumber(pNum As String) As String
        Dim removeDot = pNum.Replace(".", "")
        Return removeDot.Replace(",", ".")
    End Function

    Public Function ReadDecimal(pNum As String) As Decimal
        Dim removeDot = pNum.Replace(".", "")
        Dim value As Decimal
        Return If(Decimal.TryParse(removeDot.Replace(",", "."), value), value, 0)
    End Function

    Public Function GetSafeValue(row As DataRow, columnName As String, Optional fallbackColumnName As String = "") As String
        If row.Table.Columns.Contains(columnName) AndAlso Not IsDBNull(row(columnName)) Then
            Return row(columnName).ToString()
        ElseIf Not String.IsNullOrEmpty(fallbackColumnName) AndAlso row.Table.Columns.Contains(fallbackColumnName) AndAlso Not IsDBNull(row(fallbackColumnName)) Then
            Return row(fallbackColumnName).ToString()
        End If
        Return String.Empty ' Return a default value if neither column exists
    End Function

    Public Sub AddLog(message As String)
        RaiseEvent OnLogMessage(message)
    End Sub
End Class

Public Class SAPSessionStatusMessage
    Public Property MessageType As String
    Public Property MessageText As String
End Class