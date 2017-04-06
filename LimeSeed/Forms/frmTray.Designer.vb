<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmTray
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
    Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmTray))
    Me.tmrUpdate = New System.Windows.Forms.Timer(Me.components)
    Me.pctRestore = New System.Windows.Forms.PictureBox()
    Me.pctFastForward = New System.Windows.Forms.PictureBox()
    Me.pctStop = New System.Windows.Forms.PictureBox()
    Me.pctPlayPause = New System.Windows.Forms.PictureBox()
    Me.pctRewind = New System.Windows.Forms.PictureBox()
    Me.ttInfo = New System.Windows.Forms.ToolTip(Me.components)
    Me.pbProgress = New LimeSeed.BetterProgress()
    CType(Me.pctRestore, System.ComponentModel.ISupportInitialize).BeginInit()
    CType(Me.pctFastForward, System.ComponentModel.ISupportInitialize).BeginInit()
    CType(Me.pctStop, System.ComponentModel.ISupportInitialize).BeginInit()
    CType(Me.pctPlayPause, System.ComponentModel.ISupportInitialize).BeginInit()
    CType(Me.pctRewind, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.SuspendLayout()
    '
    'tmrUpdate
    '
    Me.tmrUpdate.Enabled = True
    '
    'pctRestore
    '
    Me.pctRestore.Image = CType(resources.GetObject("pctRestore.Image"), System.Drawing.Image)
    Me.pctRestore.Location = New System.Drawing.Point(125, 12)
    Me.pctRestore.Name = "pctRestore"
    Me.pctRestore.Size = New System.Drawing.Size(20, 20)
    Me.pctRestore.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
    Me.pctRestore.TabIndex = 4
    Me.pctRestore.TabStop = False
    Me.ttInfo.SetToolTip(Me.pctRestore, "Restore Player")
    '
    'pctFastForward
    '
    Me.pctFastForward.Image = CType(resources.GetObject("pctFastForward.Image"), System.Drawing.Image)
    Me.pctFastForward.Location = New System.Drawing.Point(90, 12)
    Me.pctFastForward.Name = "pctFastForward"
    Me.pctFastForward.Size = New System.Drawing.Size(20, 20)
    Me.pctFastForward.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
    Me.pctFastForward.TabIndex = 3
    Me.pctFastForward.TabStop = False
    Me.ttInfo.SetToolTip(Me.pctFastForward, "Skip to Next Track")
    '
    'pctStop
    '
    Me.pctStop.Image = CType(resources.GetObject("pctStop.Image"), System.Drawing.Image)
    Me.pctStop.Location = New System.Drawing.Point(61, 12)
    Me.pctStop.Name = "pctStop"
    Me.pctStop.Size = New System.Drawing.Size(20, 20)
    Me.pctStop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
    Me.pctStop.TabIndex = 2
    Me.pctStop.TabStop = False
    Me.ttInfo.SetToolTip(Me.pctStop, "Stop")
    '
    'pctPlayPause
    '
    Me.pctPlayPause.Image = CType(resources.GetObject("pctPlayPause.Image"), System.Drawing.Image)
    Me.pctPlayPause.Location = New System.Drawing.Point(32, 12)
    Me.pctPlayPause.Name = "pctPlayPause"
    Me.pctPlayPause.Size = New System.Drawing.Size(20, 20)
    Me.pctPlayPause.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
    Me.pctPlayPause.TabIndex = 1
    Me.pctPlayPause.TabStop = False
    Me.ttInfo.SetToolTip(Me.pctPlayPause, "Play/Pause")
    '
    'pctRewind
    '
    Me.pctRewind.Image = CType(resources.GetObject("pctRewind.Image"), System.Drawing.Image)
    Me.pctRewind.Location = New System.Drawing.Point(3, 12)
    Me.pctRewind.Name = "pctRewind"
    Me.pctRewind.Size = New System.Drawing.Size(20, 20)
    Me.pctRewind.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
    Me.pctRewind.TabIndex = 0
    Me.pctRewind.TabStop = False
    Me.ttInfo.SetToolTip(Me.pctRewind, "Return to Previous Track")
    '
    'pbProgress
    '
    Me.pbProgress.BarBackground = System.Drawing.SystemColors.Control
    Me.pbProgress.BarBorder = System.Drawing.SystemColors.ActiveBorder
    Me.pbProgress.BarForeground = System.Drawing.SystemColors.Highlight
    Me.pbProgress.BarStyle = LimeSeed.BetterProgress.BetterProgressStyle.VisualStyleDrawn
    Me.pbProgress.Location = New System.Drawing.Point(0, 0)
    Me.pbProgress.Maximum = 100.0R
    Me.pbProgress.Minimum = 0.0R
    Me.pbProgress.Name = "pbProgress"
    Me.pbProgress.Size = New System.Drawing.Size(147, 9)
    Me.pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous
    Me.pbProgress.TabIndex = 5
    Me.pbProgress.Value = 0.0R
    '
    'frmTray
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.BackColor = System.Drawing.Color.Black
    Me.ClientSize = New System.Drawing.Size(147, 56)
    Me.ControlBox = False
    Me.Controls.Add(Me.pbProgress)
    Me.Controls.Add(Me.pctRestore)
    Me.Controls.Add(Me.pctFastForward)
    Me.Controls.Add(Me.pctStop)
    Me.Controls.Add(Me.pctPlayPause)
    Me.Controls.Add(Me.pctRewind)
    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
    Me.Location = New System.Drawing.Point(1700, 910)
    Me.MaximizeBox = False
    Me.MinimizeBox = False
    Me.Name = "frmTray"
    Me.ShowIcon = False
    Me.ShowInTaskbar = False
    Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
    Me.Text = "Lime Seed Media Player"
    Me.TopMost = True
    CType(Me.pctRestore, System.ComponentModel.ISupportInitialize).EndInit()
    CType(Me.pctFastForward, System.ComponentModel.ISupportInitialize).EndInit()
    CType(Me.pctStop, System.ComponentModel.ISupportInitialize).EndInit()
    CType(Me.pctPlayPause, System.ComponentModel.ISupportInitialize).EndInit()
    CType(Me.pctRewind, System.ComponentModel.ISupportInitialize).EndInit()
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents tmrUpdate As System.Windows.Forms.Timer
  Friend WithEvents pctRewind As System.Windows.Forms.PictureBox
  Friend WithEvents pctPlayPause As System.Windows.Forms.PictureBox
  Friend WithEvents pctStop As System.Windows.Forms.PictureBox
  Friend WithEvents pctFastForward As System.Windows.Forms.PictureBox
  Friend WithEvents pctRestore As System.Windows.Forms.PictureBox
  Friend WithEvents pbProgress As LimeSeed.BetterProgress
  Friend WithEvents ttInfo As System.Windows.Forms.ToolTip

End Class
