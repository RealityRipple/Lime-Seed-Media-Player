<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlArtList
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
  <System.Diagnostics.DebuggerNonUserCode()> _
  Protected Overrides Sub Dispose(disposing As Boolean)
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
    Me.pnlSearch = New System.Windows.Forms.TableLayoutPanel()
    Me.lblSearch = New System.Windows.Forms.Label()
    Me.txtSearch = New System.Windows.Forms.TextBox()
    Me.cmdSearch = New System.Windows.Forms.Button()
    Me.cmdCancel = New System.Windows.Forms.Button()
    Me.tmrArtwork = New System.Windows.Forms.Timer(Me.components)
    Me.pnlArtwork = New System.Windows.Forms.FlowLayoutPanel()
    Me.cmdSelect = New System.Windows.Forms.Button()
    Me.grpArtwork = New System.Windows.Forms.GroupBox()
    Me.pnlArtList = New System.Windows.Forms.TableLayoutPanel()
    Me.pnlButtons = New System.Windows.Forms.TableLayoutPanel()
    Me.pnlProgress = New System.Windows.Forms.TableLayoutPanel()
    Me.pbProgress = New System.Windows.Forms.ProgressBar()
    Me.lblProgress = New System.Windows.Forms.Label()
    Me.ttArtwork = New System.Windows.Forms.ToolTip(Me.components)
    Me.pnlSearch.SuspendLayout()
    Me.grpArtwork.SuspendLayout()
    Me.pnlArtList.SuspendLayout()
    Me.pnlButtons.SuspendLayout()
    Me.pnlProgress.SuspendLayout()
    Me.SuspendLayout()
    '
    'pnlSearch
    '
    Me.pnlSearch.AutoSize = True
    Me.pnlSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.pnlSearch.ColumnCount = 3
    Me.pnlSearch.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlSearch.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlSearch.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlSearch.Controls.Add(Me.lblSearch, 0, 0)
    Me.pnlSearch.Controls.Add(Me.txtSearch, 1, 0)
    Me.pnlSearch.Controls.Add(Me.cmdSearch, 2, 0)
    Me.pnlSearch.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlSearch.Location = New System.Drawing.Point(0, 0)
    Me.pnlSearch.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlSearch.Name = "pnlSearch"
    Me.pnlSearch.RowCount = 1
    Me.pnlSearch.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlSearch.Size = New System.Drawing.Size(562, 29)
    Me.pnlSearch.TabIndex = 7
    '
    'lblSearch
    '
    Me.lblSearch.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblSearch.AutoSize = True
    Me.lblSearch.Location = New System.Drawing.Point(3, 8)
    Me.lblSearch.Name = "lblSearch"
    Me.lblSearch.Size = New System.Drawing.Size(44, 13)
    Me.lblSearch.TabIndex = 0
    Me.lblSearch.Text = "Search:"
    '
    'txtSearch
    '
    Me.txtSearch.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtSearch.Location = New System.Drawing.Point(53, 4)
    Me.txtSearch.Name = "txtSearch"
    Me.txtSearch.Size = New System.Drawing.Size(425, 20)
    Me.txtSearch.TabIndex = 1
    '
    'cmdSearch
    '
    Me.cmdSearch.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.cmdSearch.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdSearch.Location = New System.Drawing.Point(484, 3)
    Me.cmdSearch.Name = "cmdSearch"
    Me.cmdSearch.Size = New System.Drawing.Size(75, 23)
    Me.cmdSearch.TabIndex = 2
    Me.cmdSearch.Text = "Find Albums"
    Me.cmdSearch.UseVisualStyleBackColor = True
    '
    'cmdCancel
    '
    Me.cmdCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdCancel.Location = New System.Drawing.Point(484, 3)
    Me.cmdCancel.Name = "cmdCancel"
    Me.cmdCancel.Size = New System.Drawing.Size(75, 25)
    Me.cmdCancel.TabIndex = 6
    Me.cmdCancel.Text = "Cancel"
    Me.cmdCancel.UseVisualStyleBackColor = True
    '
    'tmrArtwork
    '
    Me.tmrArtwork.Interval = 500
    '
    'pnlArtwork
    '
    Me.pnlArtwork.AutoScroll = True
    Me.pnlArtwork.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlArtwork.Location = New System.Drawing.Point(3, 16)
    Me.pnlArtwork.Name = "pnlArtwork"
    Me.pnlArtwork.Size = New System.Drawing.Size(550, 266)
    Me.pnlArtwork.TabIndex = 0
    '
    'cmdSelect
    '
    Me.cmdSelect.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.cmdSelect.DialogResult = System.Windows.Forms.DialogResult.OK
    Me.cmdSelect.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdSelect.Location = New System.Drawing.Point(403, 3)
    Me.cmdSelect.Name = "cmdSelect"
    Me.cmdSelect.Size = New System.Drawing.Size(75, 25)
    Me.cmdSelect.TabIndex = 5
    Me.cmdSelect.Text = "Select"
    Me.cmdSelect.UseVisualStyleBackColor = True
    '
    'grpArtwork
    '
    Me.grpArtwork.Controls.Add(Me.pnlArtwork)
    Me.grpArtwork.Dock = System.Windows.Forms.DockStyle.Fill
    Me.grpArtwork.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.grpArtwork.Location = New System.Drawing.Point(3, 32)
    Me.grpArtwork.Name = "grpArtwork"
    Me.grpArtwork.Size = New System.Drawing.Size(556, 285)
    Me.grpArtwork.TabIndex = 4
    Me.grpArtwork.TabStop = False
    Me.grpArtwork.Text = "Select Album Artwork for Artist - Album"
    '
    'pnlArtList
    '
    Me.pnlArtList.ColumnCount = 1
    Me.pnlArtList.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlArtList.Controls.Add(Me.pnlSearch, 0, 0)
    Me.pnlArtList.Controls.Add(Me.grpArtwork, 0, 1)
    Me.pnlArtList.Controls.Add(Me.pnlButtons, 0, 2)
    Me.pnlArtList.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlArtList.Location = New System.Drawing.Point(0, 0)
    Me.pnlArtList.Name = "pnlArtList"
    Me.pnlArtList.RowCount = 3
    Me.pnlArtList.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlArtList.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlArtList.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlArtList.Size = New System.Drawing.Size(562, 351)
    Me.pnlArtList.TabIndex = 8
    '
    'pnlButtons
    '
    Me.pnlButtons.AutoSize = True
    Me.pnlButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.pnlButtons.ColumnCount = 3
    Me.pnlButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlButtons.Controls.Add(Me.cmdCancel, 2, 0)
    Me.pnlButtons.Controls.Add(Me.cmdSelect, 1, 0)
    Me.pnlButtons.Controls.Add(Me.pnlProgress, 0, 0)
    Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlButtons.Location = New System.Drawing.Point(0, 320)
    Me.pnlButtons.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlButtons.Name = "pnlButtons"
    Me.pnlButtons.RowCount = 1
    Me.pnlButtons.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlButtons.Size = New System.Drawing.Size(562, 31)
    Me.pnlButtons.TabIndex = 8
    '
    'pnlProgress
    '
    Me.pnlProgress.ColumnCount = 2
    Me.pnlProgress.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlProgress.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlProgress.Controls.Add(Me.pbProgress, 0, 0)
    Me.pnlProgress.Controls.Add(Me.lblProgress, 1, 0)
    Me.pnlProgress.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlProgress.Location = New System.Drawing.Point(3, 3)
    Me.pnlProgress.Name = "pnlProgress"
    Me.pnlProgress.RowCount = 1
    Me.pnlProgress.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlProgress.Size = New System.Drawing.Size(394, 25)
    Me.pnlProgress.TabIndex = 7
    Me.pnlProgress.Visible = False
    '
    'pbProgress
    '
    Me.pbProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.pbProgress.Location = New System.Drawing.Point(3, 3)
    Me.pbProgress.Name = "pbProgress"
    Me.pbProgress.Size = New System.Drawing.Size(361, 19)
    Me.pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous
    Me.pbProgress.TabIndex = 0
    '
    'lblProgress
    '
    Me.lblProgress.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.lblProgress.AutoSize = True
    Me.lblProgress.Location = New System.Drawing.Point(370, 6)
    Me.lblProgress.Name = "lblProgress"
    Me.lblProgress.Size = New System.Drawing.Size(21, 13)
    Me.lblProgress.TabIndex = 1
    Me.lblProgress.Text = "0%"
    Me.lblProgress.UseMnemonic = False
    '
    'ttArtwork
    '
    Me.ttArtwork.AutomaticDelay = 6000
    '
    'ctlArtList
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.Controls.Add(Me.pnlArtList)
    Me.Name = "ctlArtList"
    Me.Size = New System.Drawing.Size(562, 351)
    Me.pnlSearch.ResumeLayout(False)
    Me.pnlSearch.PerformLayout()
    Me.grpArtwork.ResumeLayout(False)
    Me.pnlArtList.ResumeLayout(False)
    Me.pnlArtList.PerformLayout()
    Me.pnlButtons.ResumeLayout(False)
    Me.pnlProgress.ResumeLayout(False)
    Me.pnlProgress.PerformLayout()
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents pnlSearch As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents lblSearch As System.Windows.Forms.Label
  Friend WithEvents txtSearch As System.Windows.Forms.TextBox
  Friend WithEvents cmdSearch As System.Windows.Forms.Button
  Friend WithEvents cmdCancel As System.Windows.Forms.Button
  Friend WithEvents tmrArtwork As System.Windows.Forms.Timer
  Friend WithEvents pnlArtwork As System.Windows.Forms.FlowLayoutPanel
  Friend WithEvents cmdSelect As System.Windows.Forms.Button
  Friend WithEvents grpArtwork As System.Windows.Forms.GroupBox
  Friend WithEvents pnlArtList As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents pnlButtons As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents pnlProgress As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents pbProgress As System.Windows.Forms.ProgressBar
  Friend WithEvents lblProgress As System.Windows.Forms.Label
  Friend WithEvents ttArtwork As System.Windows.Forms.ToolTip

End Class
