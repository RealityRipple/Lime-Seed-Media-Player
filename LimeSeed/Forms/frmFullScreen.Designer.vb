<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmFullScreen
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
    Me.pctVideo = New System.Windows.Forms.PictureBox()
    Me.cmdPlayPause = New System.Windows.Forms.Button()
    Me.cmdStop = New System.Windows.Forms.Button()
    Me.cmdMute = New System.Windows.Forms.Button()
    Me.cmdFullScreen = New System.Windows.Forms.Button()
    Me.tmrHide = New System.Windows.Forms.Timer(Me.components)
    Me.ttDisp = New System.Windows.Forms.ToolTip(Me.components)
    Me.bpgVolume = New LimeSeed.BetterProgress()
    Me.pbProgress = New LimeSeed.BetterProgress()
    Me.tmrUpdate = New System.Windows.Forms.Timer(Me.components)
    Me.tmrScroll = New System.Windows.Forms.Timer(Me.components)
    CType(Me.pctVideo, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.SuspendLayout()
    '
    'pctVideo
    '
    Me.pctVideo.BackColor = System.Drawing.Color.Black
    Me.pctVideo.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pctVideo.Location = New System.Drawing.Point(0, 0)
    Me.pctVideo.Margin = New System.Windows.Forms.Padding(0)
    Me.pctVideo.Name = "pctVideo"
    Me.pctVideo.Size = New System.Drawing.Size(569, 359)
    Me.pctVideo.TabIndex = 1
    Me.pctVideo.TabStop = False
    '
    'cmdPlayPause
    '
    Me.cmdPlayPause.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
    Me.cmdPlayPause.AutoSize = True
    Me.cmdPlayPause.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.cmdPlayPause.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
    Me.cmdPlayPause.FlatAppearance.BorderColor = System.Drawing.Color.Gray
    Me.cmdPlayPause.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver
    Me.cmdPlayPause.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver
    Me.cmdPlayPause.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray
    Me.cmdPlayPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat
    Me.cmdPlayPause.Image = Global.LimeSeed.My.Resources.Resources.button_play
    Me.cmdPlayPause.Location = New System.Drawing.Point(2, 333)
    Me.cmdPlayPause.Margin = New System.Windows.Forms.Padding(0)
    Me.cmdPlayPause.Name = "cmdPlayPause"
    Me.cmdPlayPause.Size = New System.Drawing.Size(24, 24)
    Me.cmdPlayPause.TabIndex = 11
    Me.cmdPlayPause.UseVisualStyleBackColor = False
    '
    'cmdStop
    '
    Me.cmdStop.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
    Me.cmdStop.AutoSize = True
    Me.cmdStop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.cmdStop.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
    Me.cmdStop.FlatAppearance.BorderColor = System.Drawing.Color.Gray
    Me.cmdStop.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver
    Me.cmdStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver
    Me.cmdStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray
    Me.cmdStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat
    Me.cmdStop.Image = Global.LimeSeed.My.Resources.Resources.button_stop
    Me.cmdStop.Location = New System.Drawing.Point(28, 333)
    Me.cmdStop.Margin = New System.Windows.Forms.Padding(0)
    Me.cmdStop.Name = "cmdStop"
    Me.cmdStop.Size = New System.Drawing.Size(24, 24)
    Me.cmdStop.TabIndex = 12
    Me.cmdStop.UseVisualStyleBackColor = False
    '
    'cmdMute
    '
    Me.cmdMute.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
    Me.cmdMute.AutoSize = True
    Me.cmdMute.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.cmdMute.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
    Me.cmdMute.FlatAppearance.BorderColor = System.Drawing.Color.Gray
    Me.cmdMute.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver
    Me.cmdMute.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver
    Me.cmdMute.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray
    Me.cmdMute.FlatStyle = System.Windows.Forms.FlatStyle.Flat
    Me.cmdMute.Image = Global.LimeSeed.My.Resources.Resources.button_nomute
    Me.cmdMute.Location = New System.Drawing.Point(80, 333)
    Me.cmdMute.Margin = New System.Windows.Forms.Padding(0)
    Me.cmdMute.Name = "cmdMute"
    Me.cmdMute.Size = New System.Drawing.Size(24, 24)
    Me.cmdMute.TabIndex = 14
    Me.cmdMute.UseVisualStyleBackColor = False
    '
    'cmdFullScreen
    '
    Me.cmdFullScreen.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
    Me.cmdFullScreen.AutoSize = True
    Me.cmdFullScreen.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.cmdFullScreen.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
    Me.cmdFullScreen.FlatAppearance.BorderColor = System.Drawing.Color.Gray
    Me.cmdFullScreen.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver
    Me.cmdFullScreen.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver
    Me.cmdFullScreen.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray
    Me.cmdFullScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat
    Me.cmdFullScreen.Image = Global.LimeSeed.My.Resources.Resources.button_fs
    Me.cmdFullScreen.Location = New System.Drawing.Point(54, 333)
    Me.cmdFullScreen.Margin = New System.Windows.Forms.Padding(0)
    Me.cmdFullScreen.Name = "cmdFullScreen"
    Me.cmdFullScreen.Size = New System.Drawing.Size(24, 24)
    Me.cmdFullScreen.TabIndex = 13
    Me.cmdFullScreen.UseVisualStyleBackColor = False
    '
    'tmrHide
    '
    Me.tmrHide.Interval = 150
    '
    'ttDisp
    '
    Me.ttDisp.ShowAlways = True
    '
    'bpgVolume
    '
    Me.bpgVolume.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.bpgVolume.BackColor = System.Drawing.Color.Black
    Me.bpgVolume.BarBackground = System.Drawing.Color.Black
    Me.bpgVolume.BarBorder = System.Drawing.Color.Gray
    Me.bpgVolume.BarForeground = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
    Me.bpgVolume.BarStyle = LimeSeed.BetterProgress.BetterProgressStyle.CustomColorsSlanted
    Me.bpgVolume.ForeColor = System.Drawing.Color.Blue
    Me.bpgVolume.Location = New System.Drawing.Point(475, 332)
    Me.bpgVolume.Margin = New System.Windows.Forms.Padding(0)
    Me.bpgVolume.Maximum = 100.0R
    Me.bpgVolume.Minimum = 0.0R
    Me.bpgVolume.Name = "bpgVolume"
    Me.bpgVolume.Size = New System.Drawing.Size(90, 24)
    Me.bpgVolume.TabIndex = 17
    Me.bpgVolume.Value = 100.0R
    '
    'pbProgress
    '
    Me.pbProgress.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.pbProgress.BackColor = System.Drawing.Color.Black
    Me.pbProgress.BarBackground = System.Drawing.Color.Black
    Me.pbProgress.BarBorder = System.Drawing.Color.Gray
    Me.pbProgress.BarForeground = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
    Me.pbProgress.BarStyle = LimeSeed.BetterProgress.BetterProgressStyle.CustomColors
    Me.pbProgress.Cursor = System.Windows.Forms.Cursors.VSplit
    Me.pbProgress.Location = New System.Drawing.Point(106, 337)
    Me.pbProgress.Margin = New System.Windows.Forms.Padding(3, 0, 3, 0)
    Me.pbProgress.Maximum = 100.0R
    Me.pbProgress.Minimum = 0.0R
    Me.pbProgress.Name = "pbProgress"
    Me.pbProgress.Size = New System.Drawing.Size(365, 16)
    Me.pbProgress.TabIndex = 16
    Me.pbProgress.Value = 0.0R
    '
    'tmrUpdate
    '
    '
    'tmrScroll
    '
    '
    'frmFullScreen
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.BackColor = System.Drawing.Color.Red
    Me.ClientSize = New System.Drawing.Size(569, 359)
    Me.ControlBox = False
    Me.Controls.Add(Me.bpgVolume)
    Me.Controls.Add(Me.cmdPlayPause)
    Me.Controls.Add(Me.cmdStop)
    Me.Controls.Add(Me.cmdMute)
    Me.Controls.Add(Me.cmdFullScreen)
    Me.Controls.Add(Me.pbProgress)
    Me.Controls.Add(Me.pctVideo)
    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
    Me.Icon = Global.LimeSeed.My.Resources.Resources.norm
    Me.MaximizeBox = False
    Me.MinimizeBox = False
    Me.MinimumSize = New System.Drawing.Size(320, 240)
    Me.Name = "frmFullScreen"
    Me.ShowIcon = False
    Me.ShowInTaskbar = False
    Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
    CType(Me.pctVideo, System.ComponentModel.ISupportInitialize).EndInit()
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents pctVideo As System.Windows.Forms.PictureBox
  Friend WithEvents cmdPlayPause As System.Windows.Forms.Button
  Friend WithEvents cmdStop As System.Windows.Forms.Button
  Friend WithEvents cmdMute As System.Windows.Forms.Button
  Friend WithEvents cmdFullScreen As System.Windows.Forms.Button
  Friend WithEvents pbProgress As BetterProgress
  Friend WithEvents tmrHide As System.Windows.Forms.Timer
  Friend WithEvents bpgVolume As LimeSeed.BetterProgress
  Friend WithEvents ttDisp As System.Windows.Forms.ToolTip
  Friend WithEvents tmrUpdate As System.Windows.Forms.Timer
  Friend WithEvents tmrScroll As System.Windows.Forms.Timer
End Class
