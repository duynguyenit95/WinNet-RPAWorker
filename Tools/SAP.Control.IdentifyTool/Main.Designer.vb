<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Main
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.treeObject = New System.Windows.Forms.TreeView()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.btnLoad = New System.Windows.Forms.Button()
        Me.btnExpand = New System.Windows.Forms.Button()
        Me.btnCollapse = New System.Windows.Forms.Button()
        Me.WebBrowser1 = New System.Windows.Forms.WebBrowser()
        Me.gpControl = New System.Windows.Forms.GroupBox()
        Me.treeObjectDetails = New System.Windows.Forms.TreeView()
        Me.btnLoadObject = New System.Windows.Forms.Button()
        Me.gbDocument = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.txtSelectedNode = New System.Windows.Forms.TextBox()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.GroupBox6 = New System.Windows.Forms.GroupBox()
        Me.treeViewResult = New System.Windows.Forms.TreeView()
        Me.btnInvoke = New System.Windows.Forms.Button()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.dgvInput = New System.Windows.Forms.DataGridView()
        Me.cbbMethods = New System.Windows.Forms.ComboBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lbSelectedProperty = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtInpSetValue = New System.Windows.Forms.TextBox()
        Me.btnSetValue = New System.Windows.Forms.Button()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.lbSelectedNodeType = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.GroupBox1.SuspendLayout()
        Me.gpControl.SuspendLayout()
        Me.gbDocument.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.GroupBox6.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        CType(Me.dgvInput, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox3.SuspendLayout()
        Me.SuspendLayout()
        '
        'treeObject
        '
        Me.treeObject.Dock = System.Windows.Forms.DockStyle.Fill
        Me.treeObject.Location = New System.Drawing.Point(3, 16)
        Me.treeObject.Name = "treeObject"
        Me.treeObject.ShowNodeToolTips = True
        Me.treeObject.Size = New System.Drawing.Size(492, 823)
        Me.treeObject.TabIndex = 0
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.treeObject)
        Me.GroupBox1.Location = New System.Drawing.Point(13, 39)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(498, 842)
        Me.GroupBox1.TabIndex = 1
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Object Tree"
        '
        'btnLoad
        '
        Me.btnLoad.Location = New System.Drawing.Point(16, 13)
        Me.btnLoad.Name = "btnLoad"
        Me.btnLoad.Size = New System.Drawing.Size(73, 23)
        Me.btnLoad.TabIndex = 2
        Me.btnLoad.Text = "Load"
        Me.btnLoad.UseVisualStyleBackColor = True
        '
        'btnExpand
        '
        Me.btnExpand.Location = New System.Drawing.Point(135, 13)
        Me.btnExpand.Name = "btnExpand"
        Me.btnExpand.Size = New System.Drawing.Size(73, 23)
        Me.btnExpand.TabIndex = 2
        Me.btnExpand.Text = "Expand"
        Me.btnExpand.UseVisualStyleBackColor = True
        '
        'btnCollapse
        '
        Me.btnCollapse.Location = New System.Drawing.Point(214, 13)
        Me.btnCollapse.Name = "btnCollapse"
        Me.btnCollapse.Size = New System.Drawing.Size(73, 23)
        Me.btnCollapse.TabIndex = 2
        Me.btnCollapse.Text = "Collapse"
        Me.btnCollapse.UseVisualStyleBackColor = True
        '
        'WebBrowser1
        '
        Me.WebBrowser1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.WebBrowser1.Location = New System.Drawing.Point(3, 16)
        Me.WebBrowser1.MinimumSize = New System.Drawing.Size(20, 20)
        Me.WebBrowser1.Name = "WebBrowser1"
        Me.WebBrowser1.Size = New System.Drawing.Size(762, 549)
        Me.WebBrowser1.TabIndex = 5
        '
        'gpControl
        '
        Me.gpControl.Controls.Add(Me.treeObjectDetails)
        Me.gpControl.Location = New System.Drawing.Point(517, 39)
        Me.gpControl.Name = "gpControl"
        Me.gpControl.Size = New System.Drawing.Size(558, 571)
        Me.gpControl.TabIndex = 6
        Me.gpControl.TabStop = False
        Me.gpControl.Text = "Object Details"
        '
        'treeObjectDetails
        '
        Me.treeObjectDetails.Dock = System.Windows.Forms.DockStyle.Fill
        Me.treeObjectDetails.Location = New System.Drawing.Point(3, 16)
        Me.treeObjectDetails.Name = "treeObjectDetails"
        Me.treeObjectDetails.ShowNodeToolTips = True
        Me.treeObjectDetails.Size = New System.Drawing.Size(552, 552)
        Me.treeObjectDetails.TabIndex = 2
        '
        'btnLoadObject
        '
        Me.btnLoadObject.Location = New System.Drawing.Point(357, 13)
        Me.btnLoadObject.Name = "btnLoadObject"
        Me.btnLoadObject.Size = New System.Drawing.Size(151, 23)
        Me.btnLoadObject.TabIndex = 7
        Me.btnLoadObject.Text = "Load Object Details"
        Me.btnLoadObject.UseVisualStyleBackColor = True
        '
        'gbDocument
        '
        Me.gbDocument.Controls.Add(Me.WebBrowser1)
        Me.gbDocument.Location = New System.Drawing.Point(1082, 39)
        Me.gbDocument.Name = "gbDocument"
        Me.gbDocument.Size = New System.Drawing.Size(768, 568)
        Me.gbDocument.TabIndex = 9
        Me.gbDocument.TabStop = False
        Me.gbDocument.Text = "Document"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(613, 18)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(414, 13)
        Me.Label1.TabIndex = 10
        Me.Label1.Text = "Click node to load document or double-click node to expand child properties if po" &
    "ssible"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.txtSelectedNode)
        Me.GroupBox2.Controls.Add(Me.GroupBox4)
        Me.GroupBox2.Controls.Add(Me.GroupBox3)
        Me.GroupBox2.Controls.Add(Me.Label6)
        Me.GroupBox2.Controls.Add(Me.Label4)
        Me.GroupBox2.Controls.Add(Me.lbSelectedNodeType)
        Me.GroupBox2.Location = New System.Drawing.Point(520, 616)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(1327, 262)
        Me.GroupBox2.TabIndex = 11
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Intereaction"
        '
        'txtSelectedNode
        '
        Me.txtSelectedNode.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSelectedNode.Location = New System.Drawing.Point(162, 28)
        Me.txtSelectedNode.Name = "txtSelectedNode"
        Me.txtSelectedNode.ReadOnly = True
        Me.txtSelectedNode.Size = New System.Drawing.Size(260, 26)
        Me.txtSelectedNode.TabIndex = 11
        '
        'GroupBox4
        '
        Me.GroupBox4.Controls.Add(Me.GroupBox6)
        Me.GroupBox4.Controls.Add(Me.btnInvoke)
        Me.GroupBox4.Controls.Add(Me.GroupBox5)
        Me.GroupBox4.Controls.Add(Me.cbbMethods)
        Me.GroupBox4.Controls.Add(Me.Label5)
        Me.GroupBox4.Location = New System.Drawing.Point(428, 19)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(893, 230)
        Me.GroupBox4.TabIndex = 10
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "Method"
        '
        'GroupBox6
        '
        Me.GroupBox6.Controls.Add(Me.treeViewResult)
        Me.GroupBox6.Location = New System.Drawing.Point(503, 12)
        Me.GroupBox6.Name = "GroupBox6"
        Me.GroupBox6.Size = New System.Drawing.Size(384, 211)
        Me.GroupBox6.TabIndex = 4
        Me.GroupBox6.TabStop = False
        Me.GroupBox6.Text = "Result"
        '
        'treeViewResult
        '
        Me.treeViewResult.Dock = System.Windows.Forms.DockStyle.Fill
        Me.treeViewResult.Location = New System.Drawing.Point(3, 16)
        Me.treeViewResult.Name = "treeViewResult"
        Me.treeViewResult.Size = New System.Drawing.Size(378, 192)
        Me.treeViewResult.TabIndex = 0
        '
        'btnInvoke
        '
        Me.btnInvoke.Location = New System.Drawing.Point(223, 197)
        Me.btnInvoke.Name = "btnInvoke"
        Me.btnInvoke.Size = New System.Drawing.Size(86, 23)
        Me.btnInvoke.TabIndex = 3
        Me.btnInvoke.Text = "Invoke "
        Me.btnInvoke.UseVisualStyleBackColor = True
        '
        'GroupBox5
        '
        Me.GroupBox5.Controls.Add(Me.dgvInput)
        Me.GroupBox5.Location = New System.Drawing.Point(9, 49)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Size = New System.Drawing.Size(491, 145)
        Me.GroupBox5.TabIndex = 2
        Me.GroupBox5.TabStop = False
        Me.GroupBox5.Text = "Params"
        '
        'dgvInput
        '
        Me.dgvInput.AllowUserToAddRows = False
        Me.dgvInput.AllowUserToDeleteRows = False
        Me.dgvInput.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvInput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvInput.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvInput.Location = New System.Drawing.Point(3, 16)
        Me.dgvInput.Name = "dgvInput"
        Me.dgvInput.Size = New System.Drawing.Size(485, 126)
        Me.dgvInput.TabIndex = 0
        '
        'cbbMethods
        '
        Me.cbbMethods.FormattingEnabled = True
        Me.cbbMethods.Location = New System.Drawing.Point(91, 14)
        Me.cbbMethods.Name = "cbbMethods"
        Me.cbbMethods.Size = New System.Drawing.Size(406, 21)
        Me.cbbMethods.TabIndex = 1
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(6, 20)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(79, 13)
        Me.Label5.TabIndex = 0
        Me.Label5.Text = "Select Method:"
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.Button1)
        Me.GroupBox3.Controls.Add(Me.Label2)
        Me.GroupBox3.Controls.Add(Me.lbSelectedProperty)
        Me.GroupBox3.Controls.Add(Me.Label3)
        Me.GroupBox3.Controls.Add(Me.txtInpSetValue)
        Me.GroupBox3.Controls.Add(Me.btnSetValue)
        Me.GroupBox3.Location = New System.Drawing.Point(19, 113)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(247, 136)
        Me.GroupBox3.TabIndex = 9
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Property"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(24, 26)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(49, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Property:"
        '
        'lbSelectedProperty
        '
        Me.lbSelectedProperty.AutoSize = True
        Me.lbSelectedProperty.Location = New System.Drawing.Point(86, 26)
        Me.lbSelectedProperty.Name = "lbSelectedProperty"
        Me.lbSelectedProperty.Size = New System.Drawing.Size(39, 13)
        Me.lbSelectedProperty.TabIndex = 0
        Me.lbSelectedProperty.Text = "Label2"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(24, 57)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(37, 13)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Value:"
        '
        'txtInpSetValue
        '
        Me.txtInpSetValue.Location = New System.Drawing.Point(67, 54)
        Me.txtInpSetValue.Name = "txtInpSetValue"
        Me.txtInpSetValue.Size = New System.Drawing.Size(124, 20)
        Me.txtInpSetValue.TabIndex = 3
        '
        'btnSetValue
        '
        Me.btnSetValue.Location = New System.Drawing.Point(38, 89)
        Me.btnSetValue.Name = "btnSetValue"
        Me.btnSetValue.Size = New System.Drawing.Size(75, 23)
        Me.btnSetValue.TabIndex = 4
        Me.btnSetValue.Text = "Set Value"
        Me.btnSetValue.UseVisualStyleBackColor = True
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(15, 28)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(141, 24)
        Me.Label6.TabIndex = 7
        Me.Label6.Text = "Selected Node:"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(15, 68)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(58, 24)
        Me.Label4.TabIndex = 6
        Me.Label4.Text = "Type:"
        '
        'lbSelectedNodeType
        '
        Me.lbSelectedNodeType.AutoSize = True
        Me.lbSelectedNodeType.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbSelectedNodeType.Location = New System.Drawing.Point(144, 71)
        Me.lbSelectedNodeType.Name = "lbSelectedNodeType"
        Me.lbSelectedNodeType.Size = New System.Drawing.Size(57, 20)
        Me.lbSelectedNodeType.TabIndex = 5
        Me.lbSelectedNodeType.Text = "Label2"
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(129, 89)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 5
        Me.Button1.Text = "Get Value"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1862, 893)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.gbDocument)
        Me.Controls.Add(Me.btnLoadObject)
        Me.Controls.Add(Me.gpControl)
        Me.Controls.Add(Me.btnCollapse)
        Me.Controls.Add(Me.btnExpand)
        Me.Controls.Add(Me.btnLoad)
        Me.Controls.Add(Me.GroupBox1)
        Me.Name = "Main"
        Me.Text = "SAP Control Identify Tool"
        Me.GroupBox1.ResumeLayout(False)
        Me.gpControl.ResumeLayout(False)
        Me.gbDocument.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.GroupBox6.ResumeLayout(False)
        Me.GroupBox5.ResumeLayout(False)
        CType(Me.dgvInput, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents treeObject As TreeView
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents btnLoad As Button
    Friend WithEvents btnExpand As Button
    Friend WithEvents btnCollapse As Button
    Friend WithEvents WebBrowser1 As WebBrowser
    Friend WithEvents gpControl As GroupBox
    Friend WithEvents btnLoadObject As Button
    Friend WithEvents treeObjectDetails As TreeView
    Friend WithEvents gbDocument As GroupBox
    Friend WithEvents Label1 As Label
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents btnSetValue As Button
    Friend WithEvents txtInpSetValue As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents lbSelectedProperty As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents lbSelectedNodeType As Label
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents cbbMethods As ComboBox
    Friend WithEvents Label5 As Label
    Friend WithEvents GroupBox5 As GroupBox
    Friend WithEvents dgvInput As DataGridView
    Friend WithEvents GroupBox6 As GroupBox
    Friend WithEvents treeViewResult As TreeView
    Friend WithEvents btnInvoke As Button
    Friend WithEvents txtSelectedNode As TextBox
    Friend WithEvents Button1 As Button
End Class
