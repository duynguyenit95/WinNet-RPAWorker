
Imports System.Data
Imports RPA.Core

Public Class SAPLogon
    Inherits SAPSessionBase
    Public ReadOnly Property InpClientID As String
        Get
            Return $"wnd[0]/usr/txtRSYST-MANDT"
        End Get
    End Property

    Public ReadOnly Property InpUserID As String
        Get
            Return $"wnd[0]/usr/txtRSYST-BNAME"
        End Get
    End Property
    Public ReadOnly Property InpPasswordID As String
        Get
            Return $"wnd[0]/usr/pwdRSYST-BCODE"
        End Get
    End Property
    Public ReadOnly Property InpLanguageID As String
        Get
            Return $"wnd[0]/usr/txtRSYST-LANGU"
        End Get
    End Property
    Public ReadOnly Property ExtraModalID As String
        Get
            Return $"wnd[1]"
        End Get
    End Property

    Public Sub New(inpSessionID As String)
        MyBase.New(inpSessionID)
    End Sub

    Public Function Login(client As String, userName As String,
                          password As String, lang As String,
                          Optional multipleLoginAction As SAPMultipleLogonAction = SAPMultipleLogonAction.Terminate
                          ) As SAPLogonResult
        Dim loginResult = New SAPLogonResult() With {
                .IsSuccess = True
        }
        Session.FindById(InpClientID).Text = client
        Session.FindById(InpUserID).Text = userName
        Session.FindById(InpPasswordID).Text = password
        Session.FindById(InpLanguageID).Text = lang
        Session.FindById("wnd[0]").SendVKey(0)
        WaitLoading()

        'Check if login success based on status bar
        Dim statusBar = Session.FindById("wnd[0]/sbar")
        If Not String.IsNullOrEmpty(statusBar.Text) Then
            If (statusBar.MessageType = "W" Or statusBar.MessageType = "E") Then
                loginResult.IsSuccess = False
                loginResult.Message = statusBar.Text
            End If
        End If

        Dim extraModal = FindElementByID(ExtraModalID)
        'Detect and handle Multiple Logon Action
        If Not IsNothing(extraModal) Then
            If extraModal.Text.ToString().Contains("Multiple Logon") Then
                Select Case multipleLoginAction
                    Case SAPMultipleLogonAction.ContinueAndEndOther
                        Dim radioButton = extraModal.FindById("usr/radMULTI_LOGON_OPT1")
                        If Not IsNothing(radioButton) Then
                            radioButton.Select()
                        Else
                            loginResult.IsSuccess = False
                        End If
                    Case SAPMultipleLogonAction.ContinueWithoutEndOther
                        Dim radioButton = extraModal.FindById("usr/radMULTI_LOGON_OPT2")
                        If Not IsNothing(radioButton) Then
                            radioButton.Select()
                        Else
                            loginResult.IsSuccess = False
                        End If
                    Case SAPMultipleLogonAction.Terminate
                        Dim radioButton = extraModal.FindById("usr/radMULTI_LOGON_OPT3")
                        If Not IsNothing(radioButton) Then
                            radioButton.Select()
                        Else
                            loginResult.IsSuccess = False
                        End If
                End Select
                extraModal.SendVKey(0)
                WaitLoading()
            End If

        End If

        'Close any extra modal
        DetectAndCloseAllExtraModal()
        Return loginResult
    End Function
End Class
