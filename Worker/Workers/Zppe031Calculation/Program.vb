Imports RPA.Core
Imports RPA.Tools
Imports RPA.Worker.Framework

Module Program
    Public Sub Main()
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
        Dim worker = New Zppe031CalculationWorker(serverOption)

        Application.Run(New ControlPanel(Of WorkerOption)(worker))
    End Sub
End Module

