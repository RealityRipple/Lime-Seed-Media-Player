<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmOpen
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
    Me.pnlOpen = New System.Windows.Forms.TableLayoutPanel()
    Me.pctOpenIcon = New System.Windows.Forms.PictureBox()
    Me.lblOpen = New System.Windows.Forms.Label()
    Me.tbsOpen = New System.Windows.Forms.TabControl()
    Me.tabFile = New System.Windows.Forms.TabPage()
    Me.pnlFile = New System.Windows.Forms.TableLayoutPanel()
    Me.cmdBrowse = New System.Windows.Forms.Button()
    Me.txtOpenFile = New System.Windows.Forms.TextBox()
    Me.tabDisc = New System.Windows.Forms.TabPage()
    Me.pnlDisc = New System.Windows.Forms.TableLayoutPanel()
    Me.cmbDisc = New System.Windows.Forms.ComboBox()
    Me.cmdEject = New System.Windows.Forms.Button()
    Me.cmdOpen = New System.Windows.Forms.Button()
    Me.cmdCancel = New System.Windows.Forms.Button()
    Me.pnlOpen.SuspendLayout()
    CType(Me.pctOpenIcon, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.tbsOpen.SuspendLayout()
    Me.tabFile.SuspendLayout()
    Me.pnlFile.SuspendLayout()
    Me.tabDisc.SuspendLayout()
    Me.pnlDisc.SuspendLayout()
    Me.SuspendLayout()
    '
    'pnlOpen
    '
    Me.pnlOpen.ColumnCount = 3
    Me.pnlOpen.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlOpen.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlOpen.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlOpen.Controls.Add(Me.pctOpenIcon, 0, 0)
    Me.pnlOpen.Controls.Add(Me.lblOpen, 1, 0)
    Me.pnlOpen.Controls.Add(Me.tbsOpen, 1, 1)
    Me.pnlOpen.Controls.Add(Me.cmdOpen, 1, 2)
    Me.pnlOpen.Controls.Add(Me.cmdCancel, 2, 2)
    Me.pnlOpen.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlOpen.Location = New System.Drawing.Point(0, 0)
    Me.pnlOpen.Name = "pnlOpen"
    Me.pnlOpen.RowCount = 3
    Me.pnlOpen.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlOpen.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlOpen.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlOpen.Size = New System.Drawing.Size(379, 124)
    Me.pnlOpen.TabIndex = 0
    '
    'pctOpenIcon
    '
    Me.pctOpenIcon.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.pctOpenIcon.Image = Global.LimeSeed.My.Resources.Resources.open
    Me.pctOpenIcon.Location = New System.Drawing.Point(9, 31)
    Me.pctOpenIcon.Margin = New System.Windows.Forms.Padding(9, 3, 9, 3)
    Me.pctOpenIcon.Name = "pctOpenIcon"
    Me.pnlOpen.SetRowSpan(Me.pctOpenIcon, 2)
    Me.pctOpenIcon.Size = New System.Drawing.Size(32, 32)
    Me.pctOpenIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
    Me.pctOpenIcon.TabIndex = 1
    Me.pctOpenIcon.TabStop = False
    '
    'lblOpen
    '
    Me.lblOpen.AutoSize = True
    Me.pnlOpen.SetColumnSpan(Me.lblOpen, 2)
    Me.lblOpen.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.lblOpen.Location = New System.Drawing.Point(53, 8)
    Me.lblOpen.Margin = New System.Windows.Forms.Padding(3, 8, 3, 8)
    Me.lblOpen.Name = "lblOpen"
    Me.lblOpen.Size = New System.Drawing.Size(321, 13)
    Me.lblOpen.TabIndex = 0
    Me.lblOpen.Text = "Choose a File or Disc below to Open with Lime Seed Media Player."
    '
    'tbsOpen
    '
    Me.pnlOpen.SetColumnSpan(Me.tbsOpen, 2)
    Me.tbsOpen.Controls.Add(Me.tabFile)
    Me.tbsOpen.Controls.Add(Me.tabDisc)
    Me.tbsOpen.Dock = System.Windows.Forms.DockStyle.Fill
    Me.tbsOpen.Location = New System.Drawing.Point(53, 32)
    Me.tbsOpen.Name = "tbsOpen"
    Me.tbsOpen.SelectedIndex = 0
    Me.tbsOpen.Size = New System.Drawing.Size(323, 60)
    Me.tbsOpen.TabIndex = 1
    '
    'tabFile
    '
    Me.tabFile.Controls.Add(Me.pnlFile)
    Me.tabFile.Location = New System.Drawing.Point(4, 22)
    Me.tabFile.Name = "tabFile"
    Me.tabFile.Size = New System.Drawing.Size(315, 34)
    Me.tabFile.TabIndex = 0
    Me.tabFile.Text = "File"
    Me.tabFile.UseVisualStyleBackColor = True
    '
    'pnlFile
    '
    Me.pnlFile.ColumnCount = 2
    Me.pnlFile.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlFile.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlFile.Controls.Add(Me.cmdBrowse, 1, 0)
    Me.pnlFile.Controls.Add(Me.txtOpenFile, 0, 0)
    Me.pnlFile.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlFile.Location = New System.Drawing.Point(0, 0)
    Me.pnlFile.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlFile.Name = "pnlFile"
    Me.pnlFile.RowCount = 1
    Me.pnlFile.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlFile.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
    Me.pnlFile.Size = New System.Drawing.Size(315, 34)
    Me.pnlFile.TabIndex = 0
    '
    'cmdBrowse
    '
    Me.cmdBrowse.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.cmdBrowse.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdBrowse.Location = New System.Drawing.Point(237, 5)
    Me.cmdBrowse.Name = "cmdBrowse"
    Me.cmdBrowse.Size = New System.Drawing.Size(75, 23)
    Me.cmdBrowse.TabIndex = 1
    Me.cmdBrowse.Text = "Browse..."
    Me.cmdBrowse.UseVisualStyleBackColor = True
    '
    'txtOpenFile
    '
    Me.txtOpenFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtOpenFile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
    Me.txtOpenFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem
    Me.txtOpenFile.Location = New System.Drawing.Point(3, 7)
    Me.txtOpenFile.Name = "txtOpenFile"
    Me.txtOpenFile.Size = New System.Drawing.Size(228, 20)
    Me.txtOpenFile.TabIndex = 0
    '
    'tabDisc
    '
    Me.tabDisc.Controls.Add(Me.pnlDisc)
    Me.tabDisc.Location = New System.Drawing.Point(4, 22)
    Me.tabDisc.Name = "tabDisc"
    Me.tabDisc.Size = New System.Drawing.Size(315, 34)
    Me.tabDisc.TabIndex = 1
    Me.tabDisc.Text = "Disc"
    Me.tabDisc.UseVisualStyleBackColor = True
    '
    'pnlDisc
    '
    Me.pnlDisc.ColumnCount = 2
    Me.pnlDisc.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlDisc.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlDisc.Controls.Add(Me.cmbDisc, 0, 0)
    Me.pnlDisc.Controls.Add(Me.cmdEject, 1, 0)
    Me.pnlDisc.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlDisc.Location = New System.Drawing.Point(0, 0)
    Me.pnlDisc.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlDisc.Name = "pnlDisc"
    Me.pnlDisc.RowCount = 1
    Me.pnlDisc.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlDisc.Size = New System.Drawing.Size(315, 34)
    Me.pnlDisc.TabIndex = 0
    '
    'cmbDisc
    '
    Me.cmbDisc.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.cmbDisc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbDisc.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmbDisc.FormattingEnabled = True
    Me.cmbDisc.Location = New System.Drawing.Point(3, 6)
    Me.cmbDisc.Name = "cmbDisc"
    Me.cmbDisc.Size = New System.Drawing.Size(277, 21)
    Me.cmbDisc.TabIndex = 0
    '
    'cmdEject
    '
    Me.cmdEject.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.cmdEject.Image = Global.LimeSeed.My.Resources.Resources.button_eject
    Me.cmdEject.Location = New System.Drawing.Point(286, 4)
    Me.cmdEject.Name = "cmdEject"
    Me.cmdEject.Size = New System.Drawing.Size(26, 26)
    Me.cmdEject.TabIndex = 1
    Me.cmdEject.UseVisualStyleBackColor = True
    '
    'cmdOpen
    '
    Me.cmdOpen.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.cmdOpen.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdOpen.Location = New System.Drawing.Point(220, 98)
    Me.cmdOpen.Name = "cmdOpen"
    Me.cmdOpen.Size = New System.Drawing.Size(75, 23)
    Me.cmdOpen.TabIndex = 2
    Me.cmdOpen.Text = "Open"
    Me.cmdOpen.UseVisualStyleBackColor = True
    '
    'cmdCancel
    '
    Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdCancel.Location = New System.Drawing.Point(301, 98)
    Me.cmdCancel.Name = "cmdCancel"
    Me.cmdCancel.Size = New System.Drawing.Size(75, 23)
    Me.cmdCancel.TabIndex = 3
    Me.cmdCancel.Text = "Cancel"
    Me.cmdCancel.UseVisualStyleBackColor = True
    '
    'frmOpen
    '
    Me.AcceptButton = Me.cmdOpen
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.CancelButton = Me.cmdCancel
    Me.ClientSize = New System.Drawing.Size(379, 124)
    Me.Controls.Add(Me.pnlOpen)
    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
    Me.MaximizeBox = False
    Me.MinimizeBox = False
    Me.Name = "frmOpen"
    Me.ShowInTaskbar = False
    Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
    Me.Text = "Open..."
    Me.pnlOpen.ResumeLayout(False)
    Me.pnlOpen.PerformLayout()
    CType(Me.pctOpenIcon, System.ComponentModel.ISupportInitialize).EndInit()
    Me.tbsOpen.ResumeLayout(False)
    Me.tabFile.ResumeLayout(False)
    Me.pnlFile.ResumeLayout(False)
    Me.pnlFile.PerformLayout()
    Me.tabDisc.ResumeLayout(False)
    Me.pnlDisc.ResumeLayout(False)
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents pnlOpen As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents pctOpenIcon As System.Windows.Forms.PictureBox
  Friend WithEvents lblOpen As System.Windows.Forms.Label
  Friend WithEvents tbsOpen As System.Windows.Forms.TabControl
  Friend WithEvents tabFile As System.Windows.Forms.TabPage
  Friend WithEvents txtOpenFile As System.Windows.Forms.TextBox
  Friend WithEvents tabDisc As System.Windows.Forms.TabPage
  Friend WithEvents cmbDisc As System.Windows.Forms.ComboBox
  Friend WithEvents cmdOpen As System.Windows.Forms.Button
  Friend WithEvents cmdCancel As System.Windows.Forms.Button
  Friend WithEvents pnlFile As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents cmdBrowse As System.Windows.Forms.Button
  Friend WithEvents pnlDisc As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents cmdEject As System.Windows.Forms.Button
End Class
