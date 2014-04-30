<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmTransfer
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
    Me.components = New System.ComponentModel.Container()
    Me.pnlTransfer = New System.Windows.Forms.TableLayoutPanel()
    Me.lvServers = New System.Windows.Forms.ListView()
    Me.colStatus = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
    Me.colID = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
    Me.colIP = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
    Me.cmdTransfer = New System.Windows.Forms.Button()
    Me.cmdCancel = New System.Windows.Forms.Button()
    Me.cmdRefresh = New System.Windows.Forms.Button()
    Me.cmbIPs = New System.Windows.Forms.ComboBox()
    Me.lblIP = New System.Windows.Forms.Label()
    Me.tmrTry = New System.Windows.Forms.Timer(Me.components)
    Me.pnlTransfer.SuspendLayout()
    Me.SuspendLayout()
    '
    'pnlTransfer
    '
    Me.pnlTransfer.ColumnCount = 5
    Me.pnlTransfer.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlTransfer.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlTransfer.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlTransfer.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlTransfer.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlTransfer.Controls.Add(Me.lvServers, 0, 0)
    Me.pnlTransfer.Controls.Add(Me.cmdTransfer, 3, 1)
    Me.pnlTransfer.Controls.Add(Me.cmdCancel, 4, 1)
    Me.pnlTransfer.Controls.Add(Me.cmdRefresh, 0, 1)
    Me.pnlTransfer.Controls.Add(Me.cmbIPs, 2, 1)
    Me.pnlTransfer.Controls.Add(Me.lblIP, 1, 1)
    Me.pnlTransfer.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlTransfer.Location = New System.Drawing.Point(0, 0)
    Me.pnlTransfer.Name = "pnlTransfer"
    Me.pnlTransfer.RowCount = 2
    Me.pnlTransfer.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlTransfer.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlTransfer.Size = New System.Drawing.Size(463, 225)
    Me.pnlTransfer.TabIndex = 0
    '
    'lvServers
    '
    Me.lvServers.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colStatus, Me.colID, Me.colIP})
    Me.pnlTransfer.SetColumnSpan(Me.lvServers, 5)
    Me.lvServers.Dock = System.Windows.Forms.DockStyle.Fill
    Me.lvServers.FullRowSelect = True
    Me.lvServers.Location = New System.Drawing.Point(3, 3)
    Me.lvServers.MultiSelect = False
    Me.lvServers.Name = "lvServers"
    Me.lvServers.Size = New System.Drawing.Size(457, 188)
    Me.lvServers.TabIndex = 0
    Me.lvServers.UseCompatibleStateImageBehavior = False
    Me.lvServers.View = System.Windows.Forms.View.Details
    '
    'colStatus
    '
    Me.colStatus.Text = "Status"
    Me.colStatus.Width = 50
    '
    'colID
    '
    Me.colID.Text = "Device ID"
    Me.colID.Width = 250
    '
    'colIP
    '
    Me.colIP.Text = "IP Address"
    Me.colIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
    Me.colIP.Width = 120
    '
    'cmdTransfer
    '
    Me.cmdTransfer.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.cmdTransfer.AutoSize = True
    Me.cmdTransfer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.cmdTransfer.Enabled = False
    Me.cmdTransfer.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdTransfer.Location = New System.Drawing.Point(283, 197)
    Me.cmdTransfer.Name = "cmdTransfer"
    Me.cmdTransfer.Size = New System.Drawing.Size(96, 25)
    Me.cmdTransfer.TabIndex = 1
    Me.cmdTransfer.Text = "Transfer to ? >>"
    Me.cmdTransfer.UseVisualStyleBackColor = True
    '
    'cmdCancel
    '
    Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdCancel.Location = New System.Drawing.Point(385, 197)
    Me.cmdCancel.Name = "cmdCancel"
    Me.cmdCancel.Size = New System.Drawing.Size(75, 25)
    Me.cmdCancel.TabIndex = 2
    Me.cmdCancel.Text = "Cancel"
    Me.cmdCancel.UseVisualStyleBackColor = True
    '
    'cmdRefresh
    '
    Me.cmdRefresh.Image = Global.LimeSeed.My.Resources.Resources.button_loop_on
    Me.cmdRefresh.Location = New System.Drawing.Point(3, 197)
    Me.cmdRefresh.Name = "cmdRefresh"
    Me.cmdRefresh.Size = New System.Drawing.Size(25, 25)
    Me.cmdRefresh.TabIndex = 3
    Me.cmdRefresh.UseVisualStyleBackColor = True
    '
    'cmbIPs
    '
    Me.cmbIPs.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.cmbIPs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbIPs.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmbIPs.FormattingEnabled = True
    Me.cmbIPs.Location = New System.Drawing.Point(89, 199)
    Me.cmbIPs.Name = "cmbIPs"
    Me.cmbIPs.Size = New System.Drawing.Size(100, 21)
    Me.cmbIPs.TabIndex = 5
    '
    'lblIP
    '
    Me.lblIP.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblIP.AutoSize = True
    Me.lblIP.Location = New System.Drawing.Point(34, 203)
    Me.lblIP.Name = "lblIP"
    Me.lblIP.Size = New System.Drawing.Size(49, 13)
    Me.lblIP.TabIndex = 4
    Me.lblIP.Text = "Local IP:"
    '
    'tmrTry
    '
    Me.tmrTry.Interval = 5000
    '
    'frmTransfer
    '
    Me.AcceptButton = Me.cmdTransfer
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.CancelButton = Me.cmdCancel
    Me.ClientSize = New System.Drawing.Size(463, 225)
    Me.Controls.Add(Me.pnlTransfer)
    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
    Me.Name = "frmTransfer"
    Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
    Me.Text = "Transfer Playback"
    Me.pnlTransfer.ResumeLayout(False)
    Me.pnlTransfer.PerformLayout()
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents pnlTransfer As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents lvServers As System.Windows.Forms.ListView
  Friend WithEvents cmdTransfer As System.Windows.Forms.Button
  Friend WithEvents cmdCancel As System.Windows.Forms.Button
  Friend WithEvents colStatus As System.Windows.Forms.ColumnHeader
  Friend WithEvents colID As System.Windows.Forms.ColumnHeader
  Friend WithEvents colIP As System.Windows.Forms.ColumnHeader
  Friend WithEvents cmdRefresh As System.Windows.Forms.Button
  Friend WithEvents cmbIPs As System.Windows.Forms.ComboBox
  Friend WithEvents tmrTry As System.Windows.Forms.Timer
  Friend WithEvents lblIP As System.Windows.Forms.Label
End Class
