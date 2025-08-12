Imports System.Threading
Imports RPA.Core
Imports RPA.Worker.Core
Imports Microsoft.AspNetCore.SignalR.Client
Imports SAP.Utilities
Imports FlaUI.Core.AutomationElements
Imports FlaUI.UIA3
Imports RPA.Tools
Imports OfficeOpenXml.FormulaParsing.Excel.Functions.Math

Public Class SAPManager
    Inherits QueueWorker(Of SAPManagerOptions)
    'sapgui/user_scripting
    Private ReadOnly Property ExePath As String
        Get
            Return WorkerOption.SAPExeLocation
        End Get
    End Property

    Private ReadOnly Property ConnectionName As String
        Get
            Return WorkerOption.SAPCredential.ConnectionName
        End Get
    End Property
    Private ReadOnly Property TestConnectionName As String
        Get
            Return WorkerOption.SAPCredential.TestConnectionName
        End Get
    End Property

    Private CurrentSessionCount As Integer = 0
    Private SettingUp As Boolean = False
    Private _optionLoaded As Boolean = False
    Public Sub New(options As ServerOption)
        MyBase.New(options)
        AddHandler WorkerOptionLoaded, AddressOf OptionLoadedHandler
        Task.Run(Async Function()
                     Await CheckSAPError()
                 End Function)
    End Sub

    Private Async Function CheckSAPError() As Task
        While True
            Try
                Using automation As New UIA3Automation
                    Dim rootElement = automation.GetDesktop()
                    Dim popup As Window = Nothing
                    For Each childWindow In rootElement.FindAllChildren()
                        If childWindow.Name = "SAP GUI for Windows 750" Then
                            popup = childWindow.AsWindow()
                            Exit For
                        End If
                    Next
                    If Not IsNothing(popup) Then
                        Dim element = popup.FindFirstChild(Function(x) x.ByControlType(FlaUI.Core.Definitions.ControlType.Text))
                        Await HubConnection.SendAsync(WorkerActions.SAPError, element?.Name)
                        NotifySAPError(element?.Name)
                        popup.Close()
                        StartUp(True)
                    End If

                End Using
            Catch ex As Exception

            End Try
            Await Task.Delay(1000 * 10)
        End While
    End Function

    Public Overrides Sub HubClientSetup()
        MyBase.HubClientSetup()
        HubConnection.[On](Of Boolean)(WorkerActions.RestartSAP, AddressOf StartUp)
    End Sub

    Public Overrides Async Function ProcessQueue(inputData As SimpleQueueItem) As Task
        While SettingUp
            Await Task.Delay(2000)
        End While
        Select Case inputData.Name
            Case WorkerActions.CloseSAPSession
                Await CloseSAPSession(inputData.Data)
            Case WorkerActions.RequestNewSAPSession
                While True
                    Try
                        Await NewSAPSession(inputData)
                        Exit While
                    Catch ex As Exception
                        StartUp(True)
                    End Try
                    Await Task.Delay(10000)
                End While

        End Select
    End Function

    Sub StartUp(forceRestart As Boolean)
        SettingUp = True
        StartSAP(forceRestart)
        SettingUp = False
    End Sub

    Function GetSAPConnection(connectionName As String) As Object
        If Not SAPIsRunning() Then
            StartUp(False)
        End If

        Dim cts As New CancellationTokenSource
        Try
            cts.CancelAfter(60 * 1000)
            Dim SapConnection = Nothing
            Dim SapGuiAuto = GetObject("SAPGUI")
            Dim SapApplication = SapGuiAuto.GetScriptingEngine
            For i = 0 To SapApplication.Connections.Count - 1
                If SapApplication.Connections(CInt(i)).Description = connectionName Then
                    SapConnection = SapApplication.Connections(CInt(i))
                End If
            Next
            ' Connection is not opened yet
            If IsNothing(SapConnection) Then
                ' Open New connection
                SapConnection = SapApplication.OpenConnection(connectionName, True)
            End If
            ' Check RZ11 is configured
            If SapConnection.DisabledByServer Then
                State = WorkerState.Error
                NotifyDisabledByServer()
                Return Nothing
            End If
            Return SapConnection
        Catch ex As Exception
            NotifyOpenConnectionError(ex.Message)
            State = WorkerState.Error
        Finally
            cts.Dispose()
        End Try
        Return Nothing
    End Function

    Function IsLogon(connection As Object)
        Dim baseSession = connection.Children(0)
        Return Not String.IsNullOrEmpty(baseSession.Info.User)
    End Function

    Function StartSAP(forceRestart As Boolean) As Boolean
        Dim startExeTry = 0
        If (SAPIsRunning() And forceRestart) Then
            Dim sapProcess = Process.GetProcessesByName("saplogon")
            If (sapProcess.Length > 0) Then
                sapProcess.First().Kill()
            End If
        End If

        Do While Not SAPIsRunning() And startExeTry < 3
            HubLog("Try Start SAP " + CStr(startExeTry + 1))
            Dim startinf = New ProcessStartInfo(ExePath)
            Dim process = New Process()
            process.StartInfo = startinf
            process.Start()
            Thread.Sleep(2000)
            startExeTry += 1
        Loop
        If startExeTry = 3 Then
            HubLog("Failed To start SAP ")
            NotifyCouldNotStartSAP()
            Return False
        End If
        HubLog("Start SAP Successfully ")
        Return True
    End Function

    Function OptionLoadedHandler() As Task
        If Not _optionLoaded Then
            _optionLoaded = True
            Task.Run(Sub()
                         StartUp(False)
                     End Sub)
        End If
        Return Task.CompletedTask
    End Function

    Private Function Login(sapConnection As Object, loginCredential As SAPLogonOptions, multipleLoginOption As SAPMultipleLogonAction) As SAPLogonResult
        Dim sapSession = sapConnection.Children(0)
        Dim sapLogon = New SAPLogon(sapSession.Id)
        Return sapLogon.Login(loginCredential.Client, loginCredential.UserName, loginCredential.Password, "EN", multipleLoginOption)
    End Function

    Private Function TestLogin(loginCredential As SAPLogonOptions) As SAPLogonResult
        Try
            Dim testConnection = GetSAPConnection(loginCredential.TestConnectionName)
            If Not IsNothing(testConnection) Then
                Return Login(testConnection, loginCredential, SAPMultipleLogonAction.Terminate)
            Else
                Return New SAPLogonResult() With {
                .IsSuccess = False,
                .Message = "Could not get connection"
                }
            End If
        Catch ex As Exception
            Return New SAPLogonResult() With {
            .IsSuccess = False,
            .Message = ex.Message
            }
        End Try
    End Function

    Private Sub NotifyCouldNotStartSAP()
        SendMail("SAP Session: Cound Not Start SAP")
        Throw New NotImplementedException()
    End Sub

    Private Sub NotifyDisabledByServer()
        SendMail("SAP Session: Disabled By Server")
        Throw New NotImplementedException()
    End Sub

    Private Sub NotifyOpenConnectionError(msg As String)
        SendMail("SAP Session: Open Connection Error", msg)
    End Sub

    Private Sub NotifySAPError(msg As String)
        SendMail("SAP Session: SAP Error", msg)
    End Sub

    Function SAPIsRunning() As Boolean
        Dim processes = Process.GetProcesses()
        For Each process In processes
            If process.ProcessName = "saplogon" Then
                If (process.Responding) Then
                    Return True
                Else

                End If
            End If
        Next
        HubLog("SAP is not running")
        Return False
    End Function

    Async Function NewSAPSession(input As SimpleQueueItem) As Task
        Dim requestConnectionId = input.Data
        HubLog("Request Create New SAP Session ID. RequestConnectionID : " + requestConnectionId)

        'Open Connection
        Dim SapConnection = GetSAPConnection(ConnectionName)

        'Check session limitation
        While SapConnection.Children.Count = WorkerOption.SAPSessionsLimitedNumber
            HubLog("SAP Session Limitation reached!!!!")
            Await Task.Delay(5000)
            SapConnection = GetSAPConnection(ConnectionName)
        End While
        Dim newSessionID
        If Not IsLogon(SapConnection) Then
            Dim loginStatus = Login(SapConnection, WorkerOption.SAPCredential, SAPMultipleLogonAction.ContinueWithoutEndOther)
            If Not loginStatus.IsSuccess Then
                SendMail("SAPLogon", $"ConnectionName: {WorkerOption.SAPCredential.ConnectionName}
                                            <br/>Client: {WorkerOption.SAPCredential.Client}
                                            <br/>Language: {WorkerOption.SAPCredential.Language}
                                            <br/>UserName: {WorkerOption.SAPCredential.UserName}
                                            <br/>{loginStatus.Message}")
                Await Task.Delay(1000 * 60 * 30)
                Await NewSAPSession(input)
                Return
            End If
            'Return first session
            'Create New Session ID and keep first session idle, this will be used to create all further session
            SapConnection.Children(0).CreateSession()
            newSessionID = "/app/con[0]/ses[1]"
        Else
            Dim currentSessionCount = SapConnection.Children.Count
            ' Check current Sessions
            Dim currentSessionIDs = New List(Of String)
            For i = 0 To currentSessionCount - 1
                currentSessionIDs.Add(SapConnection.Children(CInt(i)).Id)
            Next

            Dim firstSession = SapConnection.FindById("/app/con[0]/ses[0]")
            firstSession.CreateSession()
            '' Open new sessions
            'Dim newSessionCreated = False
            'While Not newSessionCreated
            '    ' Check all available session 
            '    For i = 0 To currentSessionCount - 1
            '        Dim session = SapConnection.Children(CInt(i))
            '        ' Try open new session if the session is not busy
            '        If Not session.Busy Then
            '            session.CreateSession()
            '            
            '            newSessionCreated = True
            '        End If
            '    Next
            '    ' Continue wait
            '    Await Task.Delay(30000)
            'End While
            Await Task.Delay(5000)



            ' Check new session
            Dim afterSessionIDs = New List(Of String)
            For i = 0 To SapConnection.Children.Count - 1
                afterSessionIDs.Add(SapConnection.Children(CInt(i)).Id)
            Next
            ' Set session count
            currentSessionCount = SapConnection.Children.Count

            ' filter new session ID
            newSessionID = afterSessionIDs.Where(Function(x) Not currentSessionIDs.Contains(x)).First()
            HubLog($"Session Created: {newSessionID}")
        End If
        ' Send result
        Await HubConnection.SendAsync(WorkerActions.SAPSessionCreated, requestConnectionId, newSessionID, input.ContextId)

        HubLog($"Finish Create New SAP Session ID: {newSessionID}. RequestConnectionID : " + requestConnectionId + ". Context:" + input.ContextId)
    End Function

    Private Sub SendMail(subject As String, Optional content As String = "")
        Dim sendMail As New SendMail("rpa05@reginamiracle.com", "Daj16028")
        Dim listMail As New List(Of String)({"darius.nguyen@reginamiracle.com", "lyrio.lai@reginamiracle.com", "mary.ruan@reginamiracle.com"})
        sendMail.BasicEmail(listMail, subject, "", content)
    End Sub

    Private Function CloseSAPSession(sessionID As String) As Task
        HubLog("Request Close SAP Session ID:" + CStr(sessionID))
        'Open Connection
        Dim SapConnection = GetSAPConnection(ConnectionName)
        If Not IsNothing(SapConnection) Then
            'Close Session
            SapConnection.CloseSession(sessionID)
            ' Set session count
            CurrentSessionCount = SapConnection.Children.Count
            HubLog("Close SAP Session ID:" + CStr(sessionID) + " completed")
        Else
            HubLog("Could not get connection")
        End If

        Return Task.CompletedTask
        'End If
    End Function

    Public Overrides Function ValidateOptions([option] As SAPManagerOptions) As WorkerOptionValidateResult
        Dim baseResult = MyBase.ValidateOptions([option])
        'Dim tryLogin = TestLogin([option].SAPCredential)
        'If Not tryLogin.IsSuccess Then
        '    baseResult.IsValid = False
        '    baseResult.Errors.Add("Wrong Credential !!!!!")
        '    baseResult.Errors.Add(tryLogin.Message)
        'End If

        'If Not IO.File.Exists([option].SAPExeLocation) Then
        '    baseResult.IsValid = False
        '    baseResult.Errors.Add("Could not find execution file !!!!!")
        'End If

        Return baseResult
    End Function

    Public Overrides Function LoadWorkerDefaultOption() As SAPManagerOptions
        Return New SAPManagerOptions() With {
            .SAPCredential = New SAPLogonOptions() With {
                .Client = "",
                .Language = "",
                .Password = "",
                .UserName = "",
                .ConnectionName = "",
                .TestConnectionName = "98Test"
            },
            .SAPExeLocation = "C:\Program Files (x86)\SAP\FrontEnd\SAPgui\saplogon.exe"
        }
    End Function


End Class


