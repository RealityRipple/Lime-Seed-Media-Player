<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlRipper
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
    Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
    Me.dgvTracks = New System.Windows.Forms.DataGridView()
    Me.colCheck = New System.Windows.Forms.DataGridViewCheckBoxColumn()
    Me.colTrack = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colTitle = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colArtist = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colAlbum = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colLength = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colProgress = New LimeSeed.DataGridViewProgressColumn()
    Me.pnlRipper = New System.Windows.Forms.TableLayoutPanel()
    Me.lblDrive = New System.Windows.Forms.Label()
    Me.cmbDrive = New System.Windows.Forms.ComboBox()
    Me.lblStatus = New System.Windows.Forms.Label()
    Me.pnlAlbum = New System.Windows.Forms.TableLayoutPanel()
    Me.pctCover = New System.Windows.Forms.PictureBox()
    Me.txtAlbumAlbum = New System.Windows.Forms.TextBox()
    Me.txtAlbumArtist = New System.Windows.Forms.TextBox()
    Me.txtAlbumGenre = New System.Windows.Forms.TextBox()
    Me.txtAlbumYear = New System.Windows.Forms.TextBox()
    Me.cmdGetInfo = New System.Windows.Forms.Button()
    Me.lblAlbum = New System.Windows.Forms.Label()
    Me.lblArtist = New System.Windows.Forms.Label()
    Me.lblGenre = New System.Windows.Forms.Label()
    Me.lblYear = New System.Windows.Forms.Label()
    Me.cmdSendInfo = New System.Windows.Forms.Button()
    Me.cmdApplyAlbum = New System.Windows.Forms.Button()
    Me.cmdApplyArtist = New System.Windows.Forms.Button()
    Me.cmdRip = New System.Windows.Forms.Button()
    Me.DataGridViewProgressColumn1 = New LimeSeed.DataGridViewProgressColumn()
    CType(Me.dgvTracks, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.pnlRipper.SuspendLayout()
    Me.pnlAlbum.SuspendLayout()
    CType(Me.pctCover, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.SuspendLayout()
    '
    'dgvTracks
    '
    Me.dgvTracks.AllowUserToAddRows = False
    Me.dgvTracks.AllowUserToDeleteRows = False
    Me.dgvTracks.AllowUserToResizeRows = False
    DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight
    Me.dgvTracks.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
    Me.dgvTracks.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
    Me.dgvTracks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
    Me.dgvTracks.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colCheck, Me.colTrack, Me.colTitle, Me.colArtist, Me.colAlbum, Me.colLength, Me.colProgress})
    Me.pnlRipper.SetColumnSpan(Me.dgvTracks, 2)
    Me.dgvTracks.Dock = System.Windows.Forms.DockStyle.Fill
    Me.dgvTracks.Location = New System.Drawing.Point(178, 33)
    Me.dgvTracks.MultiSelect = False
    Me.dgvTracks.Name = "dgvTracks"
    Me.dgvTracks.RowHeadersVisible = False
    Me.dgvTracks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
    Me.dgvTracks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
    Me.dgvTracks.Size = New System.Drawing.Size(432, 298)
    Me.dgvTracks.TabIndex = 0
    '
    'colCheck
    '
    Me.colCheck.FillWeight = 5.0!
    Me.colCheck.HeaderText = "Rip"
    Me.colCheck.MinimumWidth = 32
    Me.colCheck.Name = "colCheck"
    '
    'colTrack
    '
    Me.colTrack.FillWeight = 10.0!
    Me.colTrack.HeaderText = "Track"
    Me.colTrack.MinimumWidth = 35
    Me.colTrack.Name = "colTrack"
    Me.colTrack.ReadOnly = True
    Me.colTrack.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
    '
    'colTitle
    '
    Me.colTitle.FillWeight = 30.0!
    Me.colTitle.HeaderText = "Title"
    Me.colTitle.Name = "colTitle"
    '
    'colArtist
    '
    Me.colArtist.FillWeight = 30.0!
    Me.colArtist.HeaderText = "Artist"
    Me.colArtist.Name = "colArtist"
    '
    'colAlbum
    '
    Me.colAlbum.FillWeight = 30.0!
    Me.colAlbum.HeaderText = "Album"
    Me.colAlbum.Name = "colAlbum"
    '
    'colLength
    '
    Me.colLength.FillWeight = 15.0!
    Me.colLength.HeaderText = "Length"
    Me.colLength.Name = "colLength"
    Me.colLength.ReadOnly = True
    '
    'colProgress
    '
    Me.colProgress.FillWeight = 20.0!
    Me.colProgress.HeaderText = "Progress"
    Me.colProgress.MinimumWidth = 150
    Me.colProgress.Name = "colProgress"
    '
    'pnlRipper
    '
    Me.pnlRipper.ColumnCount = 3
    Me.pnlRipper.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175.0!))
    Me.pnlRipper.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlRipper.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlRipper.Controls.Add(Me.dgvTracks, 1, 1)
    Me.pnlRipper.Controls.Add(Me.lblDrive, 0, 0)
    Me.pnlRipper.Controls.Add(Me.cmbDrive, 1, 0)
    Me.pnlRipper.Controls.Add(Me.lblStatus, 1, 2)
    Me.pnlRipper.Controls.Add(Me.pnlAlbum, 0, 1)
    Me.pnlRipper.Controls.Add(Me.cmdRip, 2, 0)
    Me.pnlRipper.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlRipper.Location = New System.Drawing.Point(0, 0)
    Me.pnlRipper.Name = "pnlRipper"
    Me.pnlRipper.RowCount = 2
    Me.pnlRipper.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlRipper.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlRipper.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlRipper.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
    Me.pnlRipper.Size = New System.Drawing.Size(613, 364)
    Me.pnlRipper.TabIndex = 1
    '
    'lblDrive
    '
    Me.lblDrive.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.lblDrive.AutoSize = True
    Me.lblDrive.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.lblDrive.Location = New System.Drawing.Point(137, 8)
    Me.lblDrive.Name = "lblDrive"
    Me.lblDrive.Size = New System.Drawing.Size(35, 13)
    Me.lblDrive.TabIndex = 1
    Me.lblDrive.Text = "Drive:"
    '
    'cmbDrive
    '
    Me.cmbDrive.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.cmbDrive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbDrive.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmbDrive.FormattingEnabled = True
    Me.cmbDrive.Location = New System.Drawing.Point(178, 4)
    Me.cmbDrive.Name = "cmbDrive"
    Me.cmbDrive.Size = New System.Drawing.Size(347, 21)
    Me.cmbDrive.TabIndex = 2
    '
    'lblStatus
    '
    Me.lblStatus.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.lblStatus.AutoEllipsis = True
    Me.pnlRipper.SetColumnSpan(Me.lblStatus, 2)
    Me.lblStatus.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.lblStatus.Location = New System.Drawing.Point(190, 337)
    Me.lblStatus.Margin = New System.Windows.Forms.Padding(15, 0, 3, 0)
    Me.lblStatus.Name = "lblStatus"
    Me.lblStatus.Size = New System.Drawing.Size(420, 23)
    Me.lblStatus.TabIndex = 4
    Me.lblStatus.Text = "Idle."
    Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
    '
    'pnlAlbum
    '
    Me.pnlAlbum.ColumnCount = 3
    Me.pnlAlbum.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45.0!))
    Me.pnlAlbum.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlAlbum.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25.0!))
    Me.pnlAlbum.Controls.Add(Me.pctCover, 0, 0)
    Me.pnlAlbum.Controls.Add(Me.txtAlbumAlbum, 1, 1)
    Me.pnlAlbum.Controls.Add(Me.txtAlbumArtist, 1, 2)
    Me.pnlAlbum.Controls.Add(Me.txtAlbumGenre, 1, 3)
    Me.pnlAlbum.Controls.Add(Me.txtAlbumYear, 1, 4)
    Me.pnlAlbum.Controls.Add(Me.cmdGetInfo, 1, 5)
    Me.pnlAlbum.Controls.Add(Me.lblAlbum, 0, 1)
    Me.pnlAlbum.Controls.Add(Me.lblArtist, 0, 2)
    Me.pnlAlbum.Controls.Add(Me.lblGenre, 0, 3)
    Me.pnlAlbum.Controls.Add(Me.lblYear, 0, 4)
    Me.pnlAlbum.Controls.Add(Me.cmdSendInfo, 1, 6)
    Me.pnlAlbum.Controls.Add(Me.cmdApplyAlbum, 2, 1)
    Me.pnlAlbum.Controls.Add(Me.cmdApplyArtist, 2, 2)
    Me.pnlAlbum.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlAlbum.Location = New System.Drawing.Point(0, 30)
    Me.pnlAlbum.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlAlbum.Name = "pnlAlbum"
    Me.pnlAlbum.RowCount = 7
    Me.pnlRipper.SetRowSpan(Me.pnlAlbum, 2)
    Me.pnlAlbum.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 175.0!))
    Me.pnlAlbum.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25.0!))
    Me.pnlAlbum.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25.0!))
    Me.pnlAlbum.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25.0!))
    Me.pnlAlbum.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25.0!))
    Me.pnlAlbum.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlAlbum.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlAlbum.Size = New System.Drawing.Size(175, 334)
    Me.pnlAlbum.TabIndex = 5
    '
    'pctCover
    '
    Me.pnlAlbum.SetColumnSpan(Me.pctCover, 3)
    Me.pctCover.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pctCover.Location = New System.Drawing.Point(0, 0)
    Me.pctCover.Margin = New System.Windows.Forms.Padding(0)
    Me.pctCover.Name = "pctCover"
    Me.pctCover.Size = New System.Drawing.Size(175, 175)
    Me.pctCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
    Me.pctCover.TabIndex = 0
    Me.pctCover.TabStop = False
    '
    'txtAlbumAlbum
    '
    Me.txtAlbumAlbum.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtAlbumAlbum.Location = New System.Drawing.Point(48, 178)
    Me.txtAlbumAlbum.Name = "txtAlbumAlbum"
    Me.txtAlbumAlbum.Size = New System.Drawing.Size(99, 20)
    Me.txtAlbumAlbum.TabIndex = 1
    Me.txtAlbumAlbum.Text = "Unknown Album"
    '
    'txtAlbumArtist
    '
    Me.txtAlbumArtist.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtAlbumArtist.Location = New System.Drawing.Point(48, 203)
    Me.txtAlbumArtist.Name = "txtAlbumArtist"
    Me.txtAlbumArtist.Size = New System.Drawing.Size(99, 20)
    Me.txtAlbumArtist.TabIndex = 2
    Me.txtAlbumArtist.Text = "Unknown Artist"
    '
    'txtAlbumGenre
    '
    Me.txtAlbumGenre.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtAlbumGenre.Location = New System.Drawing.Point(48, 228)
    Me.txtAlbumGenre.Name = "txtAlbumGenre"
    Me.txtAlbumGenre.Size = New System.Drawing.Size(99, 20)
    Me.txtAlbumGenre.TabIndex = 3
    Me.txtAlbumGenre.Text = "Unknown Genre"
    '
    'txtAlbumYear
    '
    Me.txtAlbumYear.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtAlbumYear.Location = New System.Drawing.Point(48, 253)
    Me.txtAlbumYear.Name = "txtAlbumYear"
    Me.txtAlbumYear.Size = New System.Drawing.Size(99, 20)
    Me.txtAlbumYear.TabIndex = 4
    Me.txtAlbumYear.Text = "Unknown Year"
    '
    'cmdGetInfo
    '
    Me.cmdGetInfo.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.pnlAlbum.SetColumnSpan(Me.cmdGetInfo, 2)
    Me.cmdGetInfo.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdGetInfo.Location = New System.Drawing.Point(99, 278)
    Me.cmdGetInfo.Name = "cmdGetInfo"
    Me.cmdGetInfo.Size = New System.Drawing.Size(73, 23)
    Me.cmdGetInfo.TabIndex = 5
    Me.cmdGetInfo.Text = "Get Info"
    Me.cmdGetInfo.UseVisualStyleBackColor = True
    '
    'lblAlbum
    '
    Me.lblAlbum.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblAlbum.AutoSize = True
    Me.lblAlbum.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.lblAlbum.Location = New System.Drawing.Point(3, 181)
    Me.lblAlbum.Name = "lblAlbum"
    Me.lblAlbum.Size = New System.Drawing.Size(39, 13)
    Me.lblAlbum.TabIndex = 6
    Me.lblAlbum.Text = "Album:"
    '
    'lblArtist
    '
    Me.lblArtist.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblArtist.AutoSize = True
    Me.lblArtist.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.lblArtist.Location = New System.Drawing.Point(3, 206)
    Me.lblArtist.Name = "lblArtist"
    Me.lblArtist.Size = New System.Drawing.Size(33, 13)
    Me.lblArtist.TabIndex = 7
    Me.lblArtist.Text = "Artist:"
    '
    'lblGenre
    '
    Me.lblGenre.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblGenre.AutoSize = True
    Me.lblGenre.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.lblGenre.Location = New System.Drawing.Point(3, 231)
    Me.lblGenre.Name = "lblGenre"
    Me.lblGenre.Size = New System.Drawing.Size(39, 13)
    Me.lblGenre.TabIndex = 8
    Me.lblGenre.Text = "Genre:"
    '
    'lblYear
    '
    Me.lblYear.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblYear.AutoSize = True
    Me.lblYear.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.lblYear.Location = New System.Drawing.Point(3, 256)
    Me.lblYear.Name = "lblYear"
    Me.lblYear.Size = New System.Drawing.Size(32, 13)
    Me.lblYear.TabIndex = 9
    Me.lblYear.Text = "Year:"
    '
    'cmdSendInfo
    '
    Me.cmdSendInfo.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.pnlAlbum.SetColumnSpan(Me.cmdSendInfo, 2)
    Me.cmdSendInfo.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdSendInfo.Location = New System.Drawing.Point(99, 307)
    Me.cmdSendInfo.Name = "cmdSendInfo"
    Me.cmdSendInfo.Size = New System.Drawing.Size(73, 23)
    Me.cmdSendInfo.TabIndex = 10
    Me.cmdSendInfo.Text = "Send Info"
    Me.cmdSendInfo.UseVisualStyleBackColor = True
    '
    'cmdApplyAlbum
    '
    Me.cmdApplyAlbum.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdApplyAlbum.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdApplyAlbum.Location = New System.Drawing.Point(150, 175)
    Me.cmdApplyAlbum.Margin = New System.Windows.Forms.Padding(0)
    Me.cmdApplyAlbum.Name = "cmdApplyAlbum"
    Me.cmdApplyAlbum.Size = New System.Drawing.Size(25, 25)
    Me.cmdApplyAlbum.TabIndex = 11
    Me.cmdApplyAlbum.Text = ">"
    Me.cmdApplyAlbum.UseVisualStyleBackColor = True
    '
    'cmdApplyArtist
    '
    Me.cmdApplyArtist.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdApplyArtist.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdApplyArtist.Location = New System.Drawing.Point(150, 200)
    Me.cmdApplyArtist.Margin = New System.Windows.Forms.Padding(0)
    Me.cmdApplyArtist.Name = "cmdApplyArtist"
    Me.cmdApplyArtist.Size = New System.Drawing.Size(25, 25)
    Me.cmdApplyArtist.TabIndex = 12
    Me.cmdApplyArtist.Text = ">"
    Me.cmdApplyArtist.UseVisualStyleBackColor = True
    '
    'cmdRip
    '
    Me.cmdRip.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.cmdRip.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdRip.Location = New System.Drawing.Point(531, 3)
    Me.cmdRip.Name = "cmdRip"
    Me.cmdRip.Size = New System.Drawing.Size(79, 24)
    Me.cmdRip.TabIndex = 3
    Me.cmdRip.Text = "Rip Tracks"
    Me.cmdRip.UseVisualStyleBackColor = True
    '
    'DataGridViewProgressColumn1
    '
    Me.DataGridViewProgressColumn1.FillWeight = 20.0!
    Me.DataGridViewProgressColumn1.HeaderText = "Progress"
    Me.DataGridViewProgressColumn1.MinimumWidth = 150
    Me.DataGridViewProgressColumn1.Name = "DataGridViewProgressColumn1"
    Me.DataGridViewProgressColumn1.Width = 150
    '
    'ctlRipper
    '
    Me.AllowDrop = True
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.Controls.Add(Me.pnlRipper)
    Me.Name = "ctlRipper"
    Me.Size = New System.Drawing.Size(613, 364)
    CType(Me.dgvTracks, System.ComponentModel.ISupportInitialize).EndInit()
    Me.pnlRipper.ResumeLayout(False)
    Me.pnlRipper.PerformLayout()
    Me.pnlAlbum.ResumeLayout(False)
    Me.pnlAlbum.PerformLayout()
    CType(Me.pctCover, System.ComponentModel.ISupportInitialize).EndInit()
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents dgvTracks As System.Windows.Forms.DataGridView
  Friend WithEvents pnlRipper As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents lblDrive As System.Windows.Forms.Label
  Friend WithEvents cmbDrive As System.Windows.Forms.ComboBox
  Friend WithEvents cmdRip As System.Windows.Forms.Button
  Friend WithEvents lblStatus As System.Windows.Forms.Label
  Friend WithEvents DataGridViewProgressColumn1 As LimeSeed.DataGridViewProgressColumn
  Friend WithEvents colCheck As System.Windows.Forms.DataGridViewCheckBoxColumn
  Friend WithEvents colTrack As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colTitle As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colArtist As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colAlbum As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colLength As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colProgress As LimeSeed.DataGridViewProgressColumn
  Friend WithEvents pnlAlbum As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents pctCover As System.Windows.Forms.PictureBox
  Friend WithEvents txtAlbumAlbum As System.Windows.Forms.TextBox
  Friend WithEvents txtAlbumArtist As System.Windows.Forms.TextBox
  Friend WithEvents txtAlbumGenre As System.Windows.Forms.TextBox
  Friend WithEvents txtAlbumYear As System.Windows.Forms.TextBox
  Friend WithEvents cmdGetInfo As System.Windows.Forms.Button
  Friend WithEvents lblAlbum As System.Windows.Forms.Label
  Friend WithEvents lblArtist As System.Windows.Forms.Label
  Friend WithEvents lblGenre As System.Windows.Forms.Label
  Friend WithEvents lblYear As System.Windows.Forms.Label
  Friend WithEvents cmdSendInfo As System.Windows.Forms.Button
  Friend WithEvents cmdApplyAlbum As System.Windows.Forms.Button
  Friend WithEvents cmdApplyArtist As System.Windows.Forms.Button

End Class
