<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
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
    Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
    Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
    Me.tmrUpdate = New System.Windows.Forms.Timer(Me.components)
    Me.pnlMain = New System.Windows.Forms.TableLayoutPanel()
    Me.pnlControls = New System.Windows.Forms.TableLayoutPanel()
    Me.cmdPlayPause = New System.Windows.Forms.Button()
    Me.cmdStop = New System.Windows.Forms.Button()
    Me.cmdFullScreen = New System.Windows.Forms.Button()
    Me.bpgVolume = New LimeSeed.BetterProgress()
    Me.pnlVidOpts = New System.Windows.Forms.TableLayoutPanel()
    Me.cmbVidTrack = New System.Windows.Forms.ComboBox()
    Me.cmbAudTrack = New System.Windows.Forms.ComboBox()
    Me.cmbChapters = New System.Windows.Forms.ComboBox()
    Me.cmbSubtitles = New System.Windows.Forms.ComboBox()
    Me.pctChannels = New System.Windows.Forms.PictureBox()
    Me.pctBitrate = New System.Windows.Forms.PictureBox()
    Me.cmdLoop = New System.Windows.Forms.Button()
    Me.cmdMute = New System.Windows.Forms.Button()
    Me.pctQuality = New System.Windows.Forms.PictureBox()
    Me.cmdMenu = New System.Windows.Forms.Button()
    Me.pctBeat = New System.Windows.Forms.PictureBox()
    Me.tbsView = New Dotnetrix.Controls.TabControl()
    Me.tabArt = New System.Windows.Forms.TabPage()
    Me.pbArt = New System.Windows.Forms.ProgressBar()
    Me.pctAlbumArt = New System.Windows.Forms.PictureBox()
    Me.mnuAlbumArt = New System.Windows.Forms.ContextMenuStrip(Me.components)
    Me.mnuArtShow = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuArtFind = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuArtSpace = New System.Windows.Forms.ToolStripSeparator()
    Me.mnuArtVisShow = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuArtVisSelect = New System.Windows.Forms.ToolStripMenuItem()
    Me.artList = New LimeSeed.ctlArtList()
    Me.tabVideo = New System.Windows.Forms.TabPage()
    Me.mpPlayer = New Seed.ctlSeed()
    Me.tabRipper = New System.Windows.Forms.TabPage()
    Me.ripBox = New LimeSeed.ctlRipper()
    Me.tabPlayList = New System.Windows.Forms.TabPage()
    Me.pnlPlayList = New System.Windows.Forms.TableLayoutPanel()
    Me.pnlPlayListControls = New System.Windows.Forms.TableLayoutPanel()
    Me.cmdAddToPL = New System.Windows.Forms.Button()
    Me.cmdClearPL = New System.Windows.Forms.Button()
    Me.cmdRemoveFromPL = New System.Windows.Forms.Button()
    Me.cmdSavePL = New System.Windows.Forms.Button()
    Me.cmdOpenPL = New System.Windows.Forms.Button()
    Me.cmdLoopPL = New System.Windows.Forms.Button()
    Me.cmdShufflePL = New System.Windows.Forms.Button()
    Me.dgvPlayList = New System.Windows.Forms.DataGridView()
    Me.colTitle = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colLength = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.txtPlayListTitle = New System.Windows.Forms.TextBox()
    Me.lblPLAlert = New System.Windows.Forms.Label()
    Me.pnlProgress = New System.Windows.Forms.TableLayoutPanel()
    Me.cmdBackPL = New System.Windows.Forms.Button()
    Me.pbProgress = New LimeSeed.BetterProgress()
    Me.cmdNextPL = New System.Windows.Forms.Button()
    Me.mnuMain = New System.Windows.Forms.MenuStrip()
    Me.mnuFile = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuOpenFile = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuCloseFile = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuSpace1 = New System.Windows.Forms.ToolStripSeparator()
    Me.mnuTransferFile = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuProperties = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuSpace2 = New System.Windows.Forms.ToolStripSeparator()
    Me.mnuExit = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuOptions = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuOutDev = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuOutDefault = New LimeSeed.ToolStripRadioButtonMenuItem()
    Me.ToolStripMenuItem4 = New System.Windows.Forms.ToolStripSeparator()
    Me.mnuSettings = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuHelp = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuWebpage = New System.Windows.Forms.ToolStripMenuItem()
    Me.ToolStripMenuItem3 = New System.Windows.Forms.ToolStripSeparator()
    Me.mnuAbout = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuVideo = New System.Windows.Forms.ContextMenuStrip(Me.components)
    Me.mnuScale = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuScaleHalf = New LimeSeed.ToolStripRadioButtonMenuItem()
    Me.mnuScaleNorm = New LimeSeed.ToolStripRadioButtonMenuItem()
    Me.mnuScaleTwice = New LimeSeed.ToolStripRadioButtonMenuItem()
    Me.mnuRatio = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuRatioForce = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuRatioSpace1 = New System.Windows.Forms.ToolStripSeparator()
    Me.mnuRatioAutomatic = New LimeSeed.ToolStripRadioButtonMenuItem()
    Me.mnuRatioStandard = New LimeSeed.ToolStripRadioButtonMenuItem()
    Me.mnuRatioWide = New LimeSeed.ToolStripRadioButtonMenuItem()
    Me.mnuFullScreen = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuVideoTrack = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuAudioTrack = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuSubtitleTrack = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuChapterTrack = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuCopyScreenshot = New System.Windows.Forms.ToolStripMenuItem()
    Me.BottomToolStripPanel = New System.Windows.Forms.ToolStripPanel()
    Me.TopToolStripPanel = New System.Windows.Forms.ToolStripPanel()
    Me.RightToolStripPanel = New System.Windows.Forms.ToolStripPanel()
    Me.LeftToolStripPanel = New System.Windows.Forms.ToolStripPanel()
    Me.ContentPanel = New System.Windows.Forms.ToolStripContentPanel()
    Me.tmrCommandCycle = New System.Windows.Forms.Timer(Me.components)
    Me.ttDisp = New System.Windows.Forms.ToolTip(Me.components)
    Me.mnuPL = New System.Windows.Forms.ContextMenuStrip(Me.components)
    Me.mnuPLPlay = New System.Windows.Forms.ToolStripMenuItem()
    Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripSeparator()
    Me.mnuPLProps = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuPLOpenFile = New System.Windows.Forms.ToolStripMenuItem()
    Me.ToolStripMenuItem2 = New System.Windows.Forms.ToolStripSeparator()
    Me.mnuPLDelete = New System.Windows.Forms.ToolStripMenuItem()
    Me.tmrVis = New System.Windows.Forms.Timer(Me.components)
    Me.svcSqueezer = New System.ServiceProcess.ServiceController()
    Me.tmrHideAlert = New System.Windows.Forms.Timer(Me.components)
    Me.pnlMain.SuspendLayout()
    Me.pnlControls.SuspendLayout()
    Me.pnlVidOpts.SuspendLayout()
    CType(Me.pctChannels, System.ComponentModel.ISupportInitialize).BeginInit()
    CType(Me.pctBitrate, System.ComponentModel.ISupportInitialize).BeginInit()
    CType(Me.pctQuality, System.ComponentModel.ISupportInitialize).BeginInit()
    CType(Me.pctBeat, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.tbsView.SuspendLayout()
    Me.tabArt.SuspendLayout()
    CType(Me.pctAlbumArt, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.mnuAlbumArt.SuspendLayout()
    Me.tabVideo.SuspendLayout()
    Me.tabRipper.SuspendLayout()
    Me.tabPlayList.SuspendLayout()
    Me.pnlPlayList.SuspendLayout()
    Me.pnlPlayListControls.SuspendLayout()
    CType(Me.dgvPlayList, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.pnlProgress.SuspendLayout()
    Me.mnuMain.SuspendLayout()
    Me.mnuVideo.SuspendLayout()
    Me.mnuPL.SuspendLayout()
    Me.SuspendLayout()
    '
    'tmrUpdate
    '
    '
    'pnlMain
    '
    Me.pnlMain.BackColor = System.Drawing.SystemColors.Control
    Me.pnlMain.ColumnCount = 1
    Me.pnlMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlMain.Controls.Add(Me.pnlControls, 0, 3)
    Me.pnlMain.Controls.Add(Me.tbsView, 0, 2)
    Me.pnlMain.Controls.Add(Me.pnlProgress, 0, 1)
    Me.pnlMain.Controls.Add(Me.mnuMain, 0, 0)
    Me.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlMain.Location = New System.Drawing.Point(0, 0)
    Me.pnlMain.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlMain.Name = "pnlMain"
    Me.pnlMain.RowCount = 4
    Me.pnlMain.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlMain.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlMain.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
    Me.pnlMain.Size = New System.Drawing.Size(940, 377)
    Me.pnlMain.TabIndex = 0
    '
    'pnlControls
    '
    Me.pnlControls.ColumnCount = 12
    Me.pnlControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75.0!))
    Me.pnlControls.Controls.Add(Me.cmdPlayPause, 0, 0)
    Me.pnlControls.Controls.Add(Me.cmdStop, 1, 0)
    Me.pnlControls.Controls.Add(Me.cmdFullScreen, 2, 0)
    Me.pnlControls.Controls.Add(Me.bpgVolume, 11, 0)
    Me.pnlControls.Controls.Add(Me.pnlVidOpts, 5, 0)
    Me.pnlControls.Controls.Add(Me.pctChannels, 8, 0)
    Me.pnlControls.Controls.Add(Me.pctBitrate, 7, 0)
    Me.pnlControls.Controls.Add(Me.cmdLoop, 3, 0)
    Me.pnlControls.Controls.Add(Me.cmdMute, 10, 0)
    Me.pnlControls.Controls.Add(Me.pctQuality, 6, 0)
    Me.pnlControls.Controls.Add(Me.cmdMenu, 4, 0)
    Me.pnlControls.Controls.Add(Me.pctBeat, 9, 0)
    Me.pnlControls.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlControls.Location = New System.Drawing.Point(0, 347)
    Me.pnlControls.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlControls.Name = "pnlControls"
    Me.pnlControls.RowCount = 1
    Me.pnlControls.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlControls.Size = New System.Drawing.Size(940, 30)
    Me.pnlControls.TabIndex = 0
    '
    'cmdPlayPause
    '
    Me.cmdPlayPause.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdPlayPause.Image = Global.LimeSeed.My.Resources.Resources.button_play
    Me.cmdPlayPause.Location = New System.Drawing.Point(2, 2)
    Me.cmdPlayPause.Margin = New System.Windows.Forms.Padding(2, 2, 2, 5)
    Me.cmdPlayPause.Name = "cmdPlayPause"
    Me.cmdPlayPause.Size = New System.Drawing.Size(26, 26)
    Me.cmdPlayPause.TabIndex = 0
    Me.ttDisp.SetToolTip(Me.cmdPlayPause, "Play/Pause")
    Me.cmdPlayPause.UseVisualStyleBackColor = True
    '
    'cmdStop
    '
    Me.cmdStop.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdStop.Image = Global.LimeSeed.My.Resources.Resources.button_stop
    Me.cmdStop.Location = New System.Drawing.Point(32, 2)
    Me.cmdStop.Margin = New System.Windows.Forms.Padding(2, 2, 2, 5)
    Me.cmdStop.Name = "cmdStop"
    Me.cmdStop.Size = New System.Drawing.Size(26, 26)
    Me.cmdStop.TabIndex = 1
    Me.ttDisp.SetToolTip(Me.cmdStop, "Stop")
    Me.cmdStop.UseVisualStyleBackColor = True
    '
    'cmdFullScreen
    '
    Me.cmdFullScreen.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdFullScreen.Image = Global.LimeSeed.My.Resources.Resources.button_fs
    Me.cmdFullScreen.Location = New System.Drawing.Point(62, 2)
    Me.cmdFullScreen.Margin = New System.Windows.Forms.Padding(2, 2, 2, 5)
    Me.cmdFullScreen.Name = "cmdFullScreen"
    Me.cmdFullScreen.Size = New System.Drawing.Size(26, 26)
    Me.cmdFullScreen.TabIndex = 2
    Me.ttDisp.SetToolTip(Me.cmdFullScreen, "Full Screen")
    Me.cmdFullScreen.UseVisualStyleBackColor = True
    '
    'bpgVolume
    '
    Me.bpgVolume.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.bpgVolume.BarBackground = System.Drawing.Color.Black
    Me.bpgVolume.BarBorder = System.Drawing.SystemColors.ActiveBorder
    Me.bpgVolume.BarForeground = System.Drawing.Color.Gray
    Me.bpgVolume.BarStyle = LimeSeed.BetterProgress.BetterProgressStyle.CustomColorsSlanted
    Me.bpgVolume.Location = New System.Drawing.Point(866, 4)
    Me.bpgVolume.Margin = New System.Windows.Forms.Padding(1, 0, 2, 0)
    Me.bpgVolume.Maximum = 100.0R
    Me.bpgVolume.Minimum = 0.0R
    Me.bpgVolume.Name = "bpgVolume"
    Me.bpgVolume.Size = New System.Drawing.Size(72, 24)
    Me.bpgVolume.Style = System.Windows.Forms.ProgressBarStyle.Continuous
    Me.bpgVolume.TabIndex = 7
    Me.bpgVolume.TabStop = False
    Me.bpgVolume.Value = 100.0R
    '
    'pnlVidOpts
    '
    Me.pnlVidOpts.ColumnCount = 4
    Me.pnlVidOpts.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlVidOpts.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlVidOpts.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlVidOpts.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlVidOpts.Controls.Add(Me.cmbVidTrack, 0, 0)
    Me.pnlVidOpts.Controls.Add(Me.cmbAudTrack, 1, 0)
    Me.pnlVidOpts.Controls.Add(Me.cmbChapters, 2, 0)
    Me.pnlVidOpts.Controls.Add(Me.cmbSubtitles, 3, 0)
    Me.pnlVidOpts.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlVidOpts.Location = New System.Drawing.Point(150, 0)
    Me.pnlVidOpts.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlVidOpts.Name = "pnlVidOpts"
    Me.pnlVidOpts.RowCount = 1
    Me.pnlVidOpts.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlVidOpts.Size = New System.Drawing.Size(601, 33)
    Me.pnlVidOpts.TabIndex = 5
    '
    'cmbVidTrack
    '
    Me.cmbVidTrack.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmbVidTrack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbVidTrack.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmbVidTrack.IntegralHeight = False
    Me.cmbVidTrack.Location = New System.Drawing.Point(2, 5)
    Me.cmbVidTrack.Margin = New System.Windows.Forms.Padding(2, 5, 2, 4)
    Me.cmbVidTrack.Name = "cmbVidTrack"
    Me.cmbVidTrack.Size = New System.Drawing.Size(98, 21)
    Me.cmbVidTrack.TabIndex = 0
    '
    'cmbAudTrack
    '
    Me.cmbAudTrack.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmbAudTrack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbAudTrack.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmbAudTrack.IntegralHeight = False
    Me.cmbAudTrack.Location = New System.Drawing.Point(104, 5)
    Me.cmbAudTrack.Margin = New System.Windows.Forms.Padding(2, 5, 2, 4)
    Me.cmbAudTrack.Name = "cmbAudTrack"
    Me.cmbAudTrack.Size = New System.Drawing.Size(98, 21)
    Me.cmbAudTrack.TabIndex = 1
    '
    'cmbChapters
    '
    Me.cmbChapters.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmbChapters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbChapters.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmbChapters.IntegralHeight = False
    Me.cmbChapters.Location = New System.Drawing.Point(206, 5)
    Me.cmbChapters.Margin = New System.Windows.Forms.Padding(2, 5, 2, 4)
    Me.cmbChapters.Name = "cmbChapters"
    Me.cmbChapters.Size = New System.Drawing.Size(98, 21)
    Me.cmbChapters.TabIndex = 2
    '
    'cmbSubtitles
    '
    Me.cmbSubtitles.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmbSubtitles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbSubtitles.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmbSubtitles.IntegralHeight = False
    Me.cmbSubtitles.Location = New System.Drawing.Point(308, 5)
    Me.cmbSubtitles.Margin = New System.Windows.Forms.Padding(2, 5, 2, 4)
    Me.cmbSubtitles.Name = "cmbSubtitles"
    Me.cmbSubtitles.Size = New System.Drawing.Size(291, 21)
    Me.cmbSubtitles.TabIndex = 3
    '
    'pctChannels
    '
    Me.pctChannels.Location = New System.Drawing.Point(793, 7)
    Me.pctChannels.Margin = New System.Windows.Forms.Padding(2, 7, 2, 7)
    Me.pctChannels.Name = "pctChannels"
    Me.pctChannels.Size = New System.Drawing.Size(16, 16)
    Me.pctChannels.TabIndex = 10
    Me.pctChannels.TabStop = False
    '
    'pctBitrate
    '
    Me.pctBitrate.Location = New System.Drawing.Point(773, 7)
    Me.pctBitrate.Margin = New System.Windows.Forms.Padding(2, 7, 2, 7)
    Me.pctBitrate.Name = "pctBitrate"
    Me.pctBitrate.Size = New System.Drawing.Size(16, 16)
    Me.pctBitrate.TabIndex = 9
    Me.pctBitrate.TabStop = False
    '
    'cmdLoop
    '
    Me.cmdLoop.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdLoop.Image = Global.LimeSeed.My.Resources.Resources.button_loop_off
    Me.cmdLoop.Location = New System.Drawing.Point(92, 2)
    Me.cmdLoop.Margin = New System.Windows.Forms.Padding(2, 2, 2, 5)
    Me.cmdLoop.Name = "cmdLoop"
    Me.cmdLoop.Size = New System.Drawing.Size(26, 26)
    Me.cmdLoop.TabIndex = 3
    Me.ttDisp.SetToolTip(Me.cmdLoop, "Loop Track")
    Me.cmdLoop.UseVisualStyleBackColor = True
    '
    'cmdMute
    '
    Me.cmdMute.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdMute.Image = Global.LimeSeed.My.Resources.Resources.button_nomute
    Me.cmdMute.Location = New System.Drawing.Point(837, 2)
    Me.cmdMute.Margin = New System.Windows.Forms.Padding(2, 2, 2, 5)
    Me.cmdMute.Name = "cmdMute"
    Me.cmdMute.Size = New System.Drawing.Size(26, 26)
    Me.cmdMute.TabIndex = 6
    Me.ttDisp.SetToolTip(Me.cmdMute, "Mute")
    Me.cmdMute.UseVisualStyleBackColor = True
    '
    'pctQuality
    '
    Me.pctQuality.Location = New System.Drawing.Point(753, 7)
    Me.pctQuality.Margin = New System.Windows.Forms.Padding(2, 7, 2, 7)
    Me.pctQuality.Name = "pctQuality"
    Me.pctQuality.Size = New System.Drawing.Size(16, 16)
    Me.pctQuality.TabIndex = 10
    Me.pctQuality.TabStop = False
    '
    'cmdMenu
    '
    Me.cmdMenu.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdMenu.Image = Global.LimeSeed.My.Resources.Resources.properties
    Me.cmdMenu.Location = New System.Drawing.Point(122, 2)
    Me.cmdMenu.Margin = New System.Windows.Forms.Padding(2, 2, 2, 5)
    Me.cmdMenu.Name = "cmdMenu"
    Me.cmdMenu.Size = New System.Drawing.Size(26, 26)
    Me.cmdMenu.TabIndex = 4
    Me.ttDisp.SetToolTip(Me.cmdMenu, "Root Menu")
    Me.cmdMenu.UseVisualStyleBackColor = True
    Me.cmdMenu.Visible = False
    '
    'pctBeat
    '
    Me.pctBeat.Location = New System.Drawing.Point(813, 5)
    Me.pctBeat.Margin = New System.Windows.Forms.Padding(2, 5, 2, 5)
    Me.pctBeat.Name = "pctBeat"
    Me.pctBeat.Size = New System.Drawing.Size(20, 20)
    Me.pctBeat.TabIndex = 10
    Me.pctBeat.TabStop = False
    '
    'tbsView
    '
    Me.tbsView.Alignment = System.Windows.Forms.TabAlignment.Bottom
    Me.tbsView.Appearance = System.Windows.Forms.TabAppearance.Buttons
    Me.tbsView.Controls.Add(Me.tabArt)
    Me.tbsView.Controls.Add(Me.tabVideo)
    Me.tbsView.Controls.Add(Me.tabRipper)
    Me.tbsView.Controls.Add(Me.tabPlayList)
    Me.tbsView.Dock = System.Windows.Forms.DockStyle.Fill
    Me.tbsView.ItemSize = New System.Drawing.Size(65, 20)
    Me.tbsView.Location = New System.Drawing.Point(0, 41)
    Me.tbsView.Margin = New System.Windows.Forms.Padding(0)
    Me.tbsView.Multiline = True
    Me.tbsView.Name = "tbsView"
    Me.tbsView.SelectedIndex = 0
    Me.tbsView.Size = New System.Drawing.Size(940, 306)
    Me.tbsView.SizeMode = System.Windows.Forms.TabSizeMode.Fixed
    Me.tbsView.TabIndex = 1
    '
    'tabArt
    '
    Me.tabArt.Controls.Add(Me.pbArt)
    Me.tabArt.Controls.Add(Me.pctAlbumArt)
    Me.tabArt.Controls.Add(Me.artList)
    Me.tabArt.Location = New System.Drawing.Point(0, 0)
    Me.tabArt.Margin = New System.Windows.Forms.Padding(0)
    Me.tabArt.Name = "tabArt"
    Me.tabArt.Size = New System.Drawing.Size(940, 283)
    Me.tabArt.TabIndex = 0
    Me.tabArt.Text = "Album Art"
    '
    'pbArt
    '
    Me.pbArt.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.pbArt.Location = New System.Drawing.Point(3, 268)
    Me.pbArt.Name = "pbArt"
    Me.pbArt.Size = New System.Drawing.Size(934, 12)
    Me.pbArt.TabIndex = 2
    Me.pbArt.Visible = False
    '
    'pctAlbumArt
    '
    Me.pctAlbumArt.BackColor = System.Drawing.Color.Black
    Me.pctAlbumArt.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
    Me.pctAlbumArt.ContextMenuStrip = Me.mnuAlbumArt
    Me.pctAlbumArt.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pctAlbumArt.Location = New System.Drawing.Point(0, 0)
    Me.pctAlbumArt.Name = "pctAlbumArt"
    Me.pctAlbumArt.Size = New System.Drawing.Size(940, 283)
    Me.pctAlbumArt.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
    Me.pctAlbumArt.TabIndex = 0
    Me.pctAlbumArt.TabStop = False
    '
    'mnuAlbumArt
    '
    Me.mnuAlbumArt.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuArtShow, Me.mnuArtFind, Me.mnuArtSpace, Me.mnuArtVisShow, Me.mnuArtVisSelect})
    Me.mnuAlbumArt.Name = "munAlbumArt"
    Me.mnuAlbumArt.Size = New System.Drawing.Size(178, 98)
    Me.mnuAlbumArt.Text = "Album Art"
    '
    'mnuArtShow
    '
    Me.mnuArtShow.Checked = True
    Me.mnuArtShow.CheckOnClick = True
    Me.mnuArtShow.CheckState = System.Windows.Forms.CheckState.Checked
    Me.mnuArtShow.Name = "mnuArtShow"
    Me.mnuArtShow.Size = New System.Drawing.Size(177, 22)
    Me.mnuArtShow.Text = "Show Album Art"
    '
    'mnuArtFind
    '
    Me.mnuArtFind.Image = Global.LimeSeed.My.Resources.Resources.find
    Me.mnuArtFind.Name = "mnuArtFind"
    Me.mnuArtFind.Size = New System.Drawing.Size(177, 22)
    Me.mnuArtFind.Text = "Find Album Art"
    '
    'mnuArtSpace
    '
    Me.mnuArtSpace.Name = "mnuArtSpace"
    Me.mnuArtSpace.Size = New System.Drawing.Size(174, 6)
    '
    'mnuArtVisShow
    '
    Me.mnuArtVisShow.Checked = True
    Me.mnuArtVisShow.CheckOnClick = True
    Me.mnuArtVisShow.CheckState = System.Windows.Forms.CheckState.Checked
    Me.mnuArtVisShow.Name = "mnuArtVisShow"
    Me.mnuArtVisShow.Size = New System.Drawing.Size(177, 22)
    Me.mnuArtVisShow.Text = "Show Visualizations"
    '
    'mnuArtVisSelect
    '
    Me.mnuArtVisSelect.Image = Global.LimeSeed.My.Resources.Resources.visualization
    Me.mnuArtVisSelect.Name = "mnuArtVisSelect"
    Me.mnuArtVisSelect.Size = New System.Drawing.Size(177, 22)
    Me.mnuArtVisSelect.Text = "Select Visualization"
    '
    'artList
    '
    Me.artList.Dock = System.Windows.Forms.DockStyle.Fill
    Me.artList.Location = New System.Drawing.Point(0, 0)
    Me.artList.Name = "artList"
    Me.artList.Size = New System.Drawing.Size(940, 283)
    Me.artList.TabIndex = 1
    Me.artList.Visible = False
    '
    'tabVideo
    '
    Me.tabVideo.Controls.Add(Me.mpPlayer)
    Me.tabVideo.Location = New System.Drawing.Point(0, 0)
    Me.tabVideo.Margin = New System.Windows.Forms.Padding(0)
    Me.tabVideo.Name = "tabVideo"
    Me.tabVideo.Size = New System.Drawing.Size(940, 283)
    Me.tabVideo.TabIndex = 1
    Me.tabVideo.Text = "Video"
    '
    'mpPlayer
    '
    Me.mpPlayer.AllowDrop = True
    Me.mpPlayer.AudioDevice = Nothing
    Me.mpPlayer.BackColor = System.Drawing.Color.Black
    Me.mpPlayer.Balance = 0
    Me.mpPlayer.Dock = System.Windows.Forms.DockStyle.Fill
    Me.mpPlayer.FileName = Nothing
    Me.mpPlayer.FullScreen = False
    Me.mpPlayer.FullScreenObj = Nothing
    Me.mpPlayer.LinearVolume = CType(0, Long)
    Me.mpPlayer.Location = New System.Drawing.Point(0, 0)
    Me.mpPlayer.LogVolume = CType(1, Long)
    Me.mpPlayer.Margin = New System.Windows.Forms.Padding(0)
    Me.mpPlayer.Mute = False
    Me.mpPlayer.Name = "mpPlayer"
    Me.mpPlayer.Position = 0.0R
    Me.mpPlayer.QueueTime = 0.17R
    Me.mpPlayer.Rate = 1.0R
    Me.mpPlayer.Repeat = False
    Me.mpPlayer.Size = New System.Drawing.Size(940, 283)
    Me.mpPlayer.StateFade = False
    Me.mpPlayer.TabIndex = 10
    Me.mpPlayer.VideoHeight = CType(240, Long)
    Me.mpPlayer.VideoWidth = CType(320, Long)
    '
    'tabRipper
    '
    Me.tabRipper.Controls.Add(Me.ripBox)
    Me.tabRipper.Location = New System.Drawing.Point(0, 0)
    Me.tabRipper.Margin = New System.Windows.Forms.Padding(0)
    Me.tabRipper.Name = "tabRipper"
    Me.tabRipper.Size = New System.Drawing.Size(940, 283)
    Me.tabRipper.TabIndex = 2
    Me.tabRipper.Text = "Rip CD"
    '
    'ripBox
    '
    Me.ripBox.AllowDrop = True
    Me.ripBox.Cursor = System.Windows.Forms.Cursors.Default
    Me.ripBox.Dock = System.Windows.Forms.DockStyle.Fill
    Me.ripBox.Location = New System.Drawing.Point(0, 0)
    Me.ripBox.Margin = New System.Windows.Forms.Padding(0)
    Me.ripBox.Name = "ripBox"
    Me.ripBox.Size = New System.Drawing.Size(940, 283)
    Me.ripBox.TabIndex = 0
    '
    'tabPlayList
    '
    Me.tabPlayList.Controls.Add(Me.pnlPlayList)
    Me.tabPlayList.Location = New System.Drawing.Point(0, 0)
    Me.tabPlayList.Margin = New System.Windows.Forms.Padding(0)
    Me.tabPlayList.Name = "tabPlayList"
    Me.tabPlayList.Size = New System.Drawing.Size(940, 283)
    Me.tabPlayList.TabIndex = 3
    Me.tabPlayList.Text = "PlayList"
    '
    'pnlPlayList
    '
    Me.pnlPlayList.ColumnCount = 1
    Me.pnlPlayList.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlPlayList.Controls.Add(Me.pnlPlayListControls, 0, 3)
    Me.pnlPlayList.Controls.Add(Me.dgvPlayList, 0, 2)
    Me.pnlPlayList.Controls.Add(Me.txtPlayListTitle, 0, 0)
    Me.pnlPlayList.Controls.Add(Me.lblPLAlert, 0, 1)
    Me.pnlPlayList.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlPlayList.Location = New System.Drawing.Point(0, 0)
    Me.pnlPlayList.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlPlayList.Name = "pnlPlayList"
    Me.pnlPlayList.RowCount = 4
    Me.pnlPlayList.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlPlayList.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0.0!))
    Me.pnlPlayList.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlPlayList.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlPlayList.Size = New System.Drawing.Size(940, 283)
    Me.pnlPlayList.TabIndex = 0
    '
    'pnlPlayListControls
    '
    Me.pnlPlayListControls.ColumnCount = 13
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10.0!))
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10.0!))
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10.0!))
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10.0!))
    Me.pnlPlayListControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
    Me.pnlPlayListControls.Controls.Add(Me.cmdAddToPL, 4, 0)
    Me.pnlPlayListControls.Controls.Add(Me.cmdClearPL, 8, 0)
    Me.pnlPlayListControls.Controls.Add(Me.cmdRemoveFromPL, 6, 0)
    Me.pnlPlayListControls.Controls.Add(Me.cmdSavePL, 12, 0)
    Me.pnlPlayListControls.Controls.Add(Me.cmdOpenPL, 10, 0)
    Me.pnlPlayListControls.Controls.Add(Me.cmdLoopPL, 2, 0)
    Me.pnlPlayListControls.Controls.Add(Me.cmdShufflePL, 0, 0)
    Me.pnlPlayListControls.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlPlayListControls.Location = New System.Drawing.Point(1, 255)
    Me.pnlPlayListControls.Margin = New System.Windows.Forms.Padding(1, 0, 1, 0)
    Me.pnlPlayListControls.Name = "pnlPlayListControls"
    Me.pnlPlayListControls.RowCount = 1
    Me.pnlPlayListControls.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlPlayListControls.Size = New System.Drawing.Size(938, 28)
    Me.pnlPlayListControls.TabIndex = 1
    '
    'cmdAddToPL
    '
    Me.cmdAddToPL.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdAddToPL.Image = Global.LimeSeed.My.Resources.Resources.pl_button_add
    Me.cmdAddToPL.Location = New System.Drawing.Point(414, 1)
    Me.cmdAddToPL.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
    Me.cmdAddToPL.Name = "cmdAddToPL"
    Me.cmdAddToPL.Size = New System.Drawing.Size(26, 26)
    Me.cmdAddToPL.TabIndex = 0
    Me.ttDisp.SetToolTip(Me.cmdAddToPL, "Add to PlayList")
    Me.cmdAddToPL.UseVisualStyleBackColor = True
    '
    'cmdClearPL
    '
    Me.cmdClearPL.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdClearPL.Image = Global.LimeSeed.My.Resources.Resources.pl_button_clr
    Me.cmdClearPL.Location = New System.Drawing.Point(494, 1)
    Me.cmdClearPL.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
    Me.cmdClearPL.Name = "cmdClearPL"
    Me.cmdClearPL.Padding = New System.Windows.Forms.Padding(2)
    Me.cmdClearPL.Size = New System.Drawing.Size(26, 26)
    Me.cmdClearPL.TabIndex = 2
    Me.ttDisp.SetToolTip(Me.cmdClearPL, "Clear PlayList")
    Me.cmdClearPL.UseVisualStyleBackColor = True
    '
    'cmdRemoveFromPL
    '
    Me.cmdRemoveFromPL.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdRemoveFromPL.Image = Global.LimeSeed.My.Resources.Resources.pl_button_del
    Me.cmdRemoveFromPL.Location = New System.Drawing.Point(454, 1)
    Me.cmdRemoveFromPL.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
    Me.cmdRemoveFromPL.Name = "cmdRemoveFromPL"
    Me.cmdRemoveFromPL.Padding = New System.Windows.Forms.Padding(2)
    Me.cmdRemoveFromPL.Size = New System.Drawing.Size(26, 26)
    Me.cmdRemoveFromPL.TabIndex = 1
    Me.ttDisp.SetToolTip(Me.cmdRemoveFromPL, "Remove Selected Track")
    Me.cmdRemoveFromPL.UseVisualStyleBackColor = True
    '
    'cmdSavePL
    '
    Me.cmdSavePL.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdSavePL.Image = Global.LimeSeed.My.Resources.Resources.pl_button_save
    Me.cmdSavePL.Location = New System.Drawing.Point(906, 1)
    Me.cmdSavePL.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
    Me.cmdSavePL.Name = "cmdSavePL"
    Me.cmdSavePL.Padding = New System.Windows.Forms.Padding(2)
    Me.cmdSavePL.Size = New System.Drawing.Size(30, 26)
    Me.cmdSavePL.TabIndex = 6
    Me.ttDisp.SetToolTip(Me.cmdSavePL, "Save PlayList")
    Me.cmdSavePL.UseVisualStyleBackColor = True
    '
    'cmdOpenPL
    '
    Me.cmdOpenPL.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdOpenPL.Image = Global.LimeSeed.My.Resources.Resources.pl_button_open
    Me.cmdOpenPL.Location = New System.Drawing.Point(866, 1)
    Me.cmdOpenPL.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
    Me.cmdOpenPL.Name = "cmdOpenPL"
    Me.cmdOpenPL.Padding = New System.Windows.Forms.Padding(2)
    Me.cmdOpenPL.Size = New System.Drawing.Size(26, 26)
    Me.cmdOpenPL.TabIndex = 5
    Me.ttDisp.SetToolTip(Me.cmdOpenPL, "Open PlayList")
    Me.cmdOpenPL.UseVisualStyleBackColor = True
    '
    'cmdLoopPL
    '
    Me.cmdLoopPL.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdLoopPL.Image = Global.LimeSeed.My.Resources.Resources.button_loop_off
    Me.cmdLoopPL.Location = New System.Drawing.Point(42, 1)
    Me.cmdLoopPL.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
    Me.cmdLoopPL.Name = "cmdLoopPL"
    Me.cmdLoopPL.Size = New System.Drawing.Size(26, 26)
    Me.cmdLoopPL.TabIndex = 4
    Me.ttDisp.SetToolTip(Me.cmdLoopPL, "Loop PlayList")
    Me.cmdLoopPL.UseVisualStyleBackColor = True
    '
    'cmdShufflePL
    '
    Me.cmdShufflePL.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmdShufflePL.Image = Global.LimeSeed.My.Resources.Resources.pl_button_order
    Me.cmdShufflePL.Location = New System.Drawing.Point(2, 1)
    Me.cmdShufflePL.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
    Me.cmdShufflePL.Name = "cmdShufflePL"
    Me.cmdShufflePL.Size = New System.Drawing.Size(26, 26)
    Me.cmdShufflePL.TabIndex = 3
    Me.ttDisp.SetToolTip(Me.cmdShufflePL, "Shuffle")
    Me.cmdShufflePL.UseVisualStyleBackColor = True
    '
    'dgvPlayList
    '
    Me.dgvPlayList.AllowDrop = True
    Me.dgvPlayList.AllowUserToAddRows = False
    Me.dgvPlayList.AllowUserToDeleteRows = False
    DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight
    DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText
    DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight
    DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText
    Me.dgvPlayList.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
    Me.dgvPlayList.BackgroundColor = System.Drawing.SystemColors.Window
    Me.dgvPlayList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
    Me.dgvPlayList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None
    Me.dgvPlayList.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
    Me.dgvPlayList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
    Me.dgvPlayList.ColumnHeadersVisible = False
    Me.dgvPlayList.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colTitle, Me.colLength})
    Me.dgvPlayList.Dock = System.Windows.Forms.DockStyle.Fill
    Me.dgvPlayList.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
    Me.dgvPlayList.Location = New System.Drawing.Point(3, 33)
    Me.dgvPlayList.MultiSelect = False
    Me.dgvPlayList.Name = "dgvPlayList"
    Me.dgvPlayList.ReadOnly = True
    Me.dgvPlayList.RowHeadersVisible = False
    Me.dgvPlayList.RowTemplate.ReadOnly = True
    Me.dgvPlayList.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
    Me.dgvPlayList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
    Me.dgvPlayList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
    Me.dgvPlayList.ShowCellErrors = False
    Me.dgvPlayList.ShowEditingIcon = False
    Me.dgvPlayList.ShowRowErrors = False
    Me.dgvPlayList.Size = New System.Drawing.Size(934, 219)
    Me.dgvPlayList.StandardTab = True
    Me.dgvPlayList.TabIndex = 2
    '
    'colTitle
    '
    Me.colTitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
    Me.colTitle.FillWeight = 90.0!
    Me.colTitle.HeaderText = "Title"
    Me.colTitle.Name = "colTitle"
    Me.colTitle.ReadOnly = True
    Me.colTitle.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
    Me.colTitle.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
    '
    'colLength
    '
    Me.colLength.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader
    DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
    Me.colLength.DefaultCellStyle = DataGridViewCellStyle2
    Me.colLength.FillWeight = 10.0!
    Me.colLength.HeaderText = "Length"
    Me.colLength.MinimumWidth = 15
    Me.colLength.Name = "colLength"
    Me.colLength.ReadOnly = True
    Me.colLength.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
    Me.colLength.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
    Me.colLength.Width = 15
    '
    'txtPlayListTitle
    '
    Me.txtPlayListTitle.BackColor = System.Drawing.SystemColors.Control
    Me.txtPlayListTitle.BorderStyle = System.Windows.Forms.BorderStyle.None
    Me.txtPlayListTitle.Dock = System.Windows.Forms.DockStyle.Fill
    Me.txtPlayListTitle.Font = New System.Drawing.Font("Microsoft Sans Serif", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.txtPlayListTitle.Location = New System.Drawing.Point(3, 3)
    Me.txtPlayListTitle.Multiline = True
    Me.txtPlayListTitle.Name = "txtPlayListTitle"
    Me.txtPlayListTitle.ReadOnly = True
    Me.txtPlayListTitle.Size = New System.Drawing.Size(934, 24)
    Me.txtPlayListTitle.TabIndex = 0
    Me.txtPlayListTitle.TabStop = False
    Me.txtPlayListTitle.Tag = "Unknown Album"
    Me.txtPlayListTitle.Text = "Unknown Album (0:00/0:00)"
    Me.txtPlayListTitle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
    Me.ttDisp.SetToolTip(Me.txtPlayListTitle, "Rename PlayList")
    Me.txtPlayListTitle.WordWrap = False
    '
    'lblPLAlert
    '
    Me.lblPLAlert.AutoSize = True
    Me.lblPLAlert.BackColor = System.Drawing.SystemColors.Info
    Me.lblPLAlert.Dock = System.Windows.Forms.DockStyle.Top
    Me.lblPLAlert.Location = New System.Drawing.Point(0, 30)
    Me.lblPLAlert.Margin = New System.Windows.Forms.Padding(0)
    Me.lblPLAlert.Name = "lblPLAlert"
    Me.lblPLAlert.Padding = New System.Windows.Forms.Padding(3)
    Me.lblPLAlert.Size = New System.Drawing.Size(940, 1)
    Me.lblPLAlert.TabIndex = 3
    '
    'pnlProgress
    '
    Me.pnlProgress.ColumnCount = 3
    Me.pnlProgress.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16.0!))
    Me.pnlProgress.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlProgress.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16.0!))
    Me.pnlProgress.Controls.Add(Me.cmdBackPL, 0, 0)
    Me.pnlProgress.Controls.Add(Me.pbProgress, 1, 0)
    Me.pnlProgress.Controls.Add(Me.cmdNextPL, 2, 0)
    Me.pnlProgress.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlProgress.Location = New System.Drawing.Point(0, 24)
    Me.pnlProgress.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlProgress.Name = "pnlProgress"
    Me.pnlProgress.RowCount = 1
    Me.pnlProgress.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlProgress.Size = New System.Drawing.Size(940, 17)
    Me.pnlProgress.TabIndex = 2
    '
    'cmdBackPL
    '
    Me.cmdBackPL.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.cmdBackPL.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.cmdBackPL.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonShadow
    Me.cmdBackPL.FlatAppearance.BorderSize = 0
    Me.cmdBackPL.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonFace
    Me.cmdBackPL.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonHighlight
    Me.cmdBackPL.Image = Global.LimeSeed.My.Resources.Resources.pl_button_jump_previous
    Me.cmdBackPL.Location = New System.Drawing.Point(0, 0)
    Me.cmdBackPL.Margin = New System.Windows.Forms.Padding(0)
    Me.cmdBackPL.Name = "cmdBackPL"
    Me.cmdBackPL.Padding = New System.Windows.Forms.Padding(0, 0, 0, 1)
    Me.cmdBackPL.Size = New System.Drawing.Size(16, 16)
    Me.cmdBackPL.TabIndex = 0
    Me.ttDisp.SetToolTip(Me.cmdBackPL, "Return to Previous Track")
    '
    'pbProgress
    '
    Me.pbProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.pbProgress.BarBackground = System.Drawing.SystemColors.Control
    Me.pbProgress.BarBorder = System.Drawing.SystemColors.ActiveBorder
    Me.pbProgress.BarForeground = System.Drawing.SystemColors.Highlight
    Me.pbProgress.BarStyle = LimeSeed.BetterProgress.BetterProgressStyle.VisualStyleDrawn
    Me.pbProgress.Cursor = System.Windows.Forms.Cursors.VSplit
    Me.pbProgress.Location = New System.Drawing.Point(17, 1)
    Me.pbProgress.Margin = New System.Windows.Forms.Padding(1, 1, 1, 2)
    Me.pbProgress.Maximum = 100.0R
    Me.pbProgress.Minimum = 0.0R
    Me.pbProgress.Name = "pbProgress"
    Me.pbProgress.Size = New System.Drawing.Size(906, 14)
    Me.pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous
    Me.pbProgress.TabIndex = 2
    Me.pbProgress.TabStop = False
    Me.pbProgress.Value = 0.0R
    '
    'cmdNextPL
    '
    Me.cmdNextPL.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.cmdNextPL.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.cmdNextPL.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonShadow
    Me.cmdNextPL.FlatAppearance.BorderSize = 0
    Me.cmdNextPL.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonFace
    Me.cmdNextPL.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonHighlight
    Me.cmdNextPL.Image = Global.LimeSeed.My.Resources.Resources.pl_button_jump_next
    Me.cmdNextPL.Location = New System.Drawing.Point(924, 0)
    Me.cmdNextPL.Margin = New System.Windows.Forms.Padding(0)
    Me.cmdNextPL.Name = "cmdNextPL"
    Me.cmdNextPL.Padding = New System.Windows.Forms.Padding(0, 0, 0, 1)
    Me.cmdNextPL.Size = New System.Drawing.Size(16, 16)
    Me.cmdNextPL.TabIndex = 1
    Me.ttDisp.SetToolTip(Me.cmdNextPL, "Skip to Next Track")
    '
    'mnuMain
    '
    Me.mnuMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuFile, Me.mnuOptions, Me.mnuHelp})
    Me.mnuMain.Location = New System.Drawing.Point(0, 0)
    Me.mnuMain.Name = "mnuMain"
    Me.mnuMain.Size = New System.Drawing.Size(940, 24)
    Me.mnuMain.TabIndex = 9
    Me.mnuMain.Text = "Lime Seed"
    '
    'mnuFile
    '
    Me.mnuFile.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuOpenFile, Me.mnuCloseFile, Me.mnuSpace1, Me.mnuTransferFile, Me.mnuProperties, Me.mnuSpace2, Me.mnuExit})
    Me.mnuFile.Name = "mnuFile"
    Me.mnuFile.Size = New System.Drawing.Size(37, 20)
    Me.mnuFile.Text = "&File"
    '
    'mnuOpenFile
    '
    Me.mnuOpenFile.Image = Global.LimeSeed.My.Resources.Resources.pl_button_open
    Me.mnuOpenFile.Name = "mnuOpenFile"
    Me.mnuOpenFile.Size = New System.Drawing.Size(136, 22)
    Me.mnuOpenFile.Text = "&Open"
    '
    'mnuCloseFile
    '
    Me.mnuCloseFile.Image = Global.LimeSeed.My.Resources.Resources.pl_button_clr
    Me.mnuCloseFile.Name = "mnuCloseFile"
    Me.mnuCloseFile.Size = New System.Drawing.Size(136, 22)
    Me.mnuCloseFile.Text = "&Close"
    '
    'mnuSpace1
    '
    Me.mnuSpace1.ForeColor = System.Drawing.SystemColors.ControlText
    Me.mnuSpace1.Name = "mnuSpace1"
    Me.mnuSpace1.Size = New System.Drawing.Size(133, 6)
    '
    'mnuTransferFile
    '
    Me.mnuTransferFile.Image = Global.LimeSeed.My.Resources.Resources.button_loop_on
    Me.mnuTransferFile.Name = "mnuTransferFile"
    Me.mnuTransferFile.Size = New System.Drawing.Size(136, 22)
    Me.mnuTransferFile.Text = "&Transfer..."
    '
    'mnuProperties
    '
    Me.mnuProperties.Image = Global.LimeSeed.My.Resources.Resources.properties
    Me.mnuProperties.Name = "mnuProperties"
    Me.mnuProperties.Size = New System.Drawing.Size(136, 22)
    Me.mnuProperties.Text = "&Properties..."
    '
    'mnuSpace2
    '
    Me.mnuSpace2.ForeColor = System.Drawing.SystemColors.ControlText
    Me.mnuSpace2.Name = "mnuSpace2"
    Me.mnuSpace2.Size = New System.Drawing.Size(133, 6)
    '
    'mnuExit
    '
    Me.mnuExit.Image = Global.LimeSeed.My.Resources.Resources.close
    Me.mnuExit.Name = "mnuExit"
    Me.mnuExit.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.F4), System.Windows.Forms.Keys)
    Me.mnuExit.Size = New System.Drawing.Size(136, 22)
    Me.mnuExit.Text = "E&xit"
    '
    'mnuOptions
    '
    Me.mnuOptions.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuOutDev, Me.ToolStripMenuItem4, Me.mnuSettings})
    Me.mnuOptions.Name = "mnuOptions"
    Me.mnuOptions.Size = New System.Drawing.Size(61, 20)
    Me.mnuOptions.Text = "&Options"
    '
    'mnuOutDev
    '
    Me.mnuOutDev.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuOutDefault})
    Me.mnuOutDev.Image = Global.LimeSeed.My.Resources.Resources.button_nomute
    Me.mnuOutDev.Name = "mnuOutDev"
    Me.mnuOutDev.Size = New System.Drawing.Size(167, 22)
    Me.mnuOutDev.Text = "&Audio Out Device"
    '
    'mnuOutDefault
    '
    Me.mnuOutDefault.CheckOnClick = True
    Me.mnuOutDefault.Name = "mnuOutDefault"
    Me.mnuOutDefault.ShortcutKeys = System.Windows.Forms.Keys.F1
    Me.mnuOutDefault.Size = New System.Drawing.Size(149, 22)
    Me.mnuOutDefault.Text = "OS Default"
    '
    'ToolStripMenuItem4
    '
    Me.ToolStripMenuItem4.Name = "ToolStripMenuItem4"
    Me.ToolStripMenuItem4.Size = New System.Drawing.Size(164, 6)
    '
    'mnuSettings
    '
    Me.mnuSettings.Image = Global.LimeSeed.My.Resources.Resources.pl_button_save
    Me.mnuSettings.Name = "mnuSettings"
    Me.mnuSettings.Size = New System.Drawing.Size(167, 22)
    Me.mnuSettings.Text = "&Settings"
    '
    'mnuHelp
    '
    Me.mnuHelp.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuWebpage, Me.ToolStripMenuItem3, Me.mnuAbout})
    Me.mnuHelp.Name = "mnuHelp"
    Me.mnuHelp.Size = New System.Drawing.Size(44, 20)
    Me.mnuHelp.Text = "&Help"
    '
    'mnuWebpage
    '
    Me.mnuWebpage.Image = Global.LimeSeed.My.Resources.Resources.web
    Me.mnuWebpage.Name = "mnuWebpage"
    Me.mnuWebpage.Size = New System.Drawing.Size(124, 22)
    Me.mnuWebpage.Text = "&Webpage"
    '
    'ToolStripMenuItem3
    '
    Me.ToolStripMenuItem3.Name = "ToolStripMenuItem3"
    Me.ToolStripMenuItem3.Size = New System.Drawing.Size(121, 6)
    '
    'mnuAbout
    '
    Me.mnuAbout.Image = Global.LimeSeed.My.Resources.Resources.pl_button_shuffle
    Me.mnuAbout.Name = "mnuAbout"
    Me.mnuAbout.Size = New System.Drawing.Size(124, 22)
    Me.mnuAbout.Text = "&About"
    '
    'mnuVideo
    '
    Me.mnuVideo.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuScale, Me.mnuRatio, Me.mnuFullScreen, Me.mnuVideoTrack, Me.mnuAudioTrack, Me.mnuSubtitleTrack, Me.mnuChapterTrack, Me.mnuCopyScreenshot})
    Me.mnuVideo.Name = "mnuVideo"
    Me.mnuVideo.Size = New System.Drawing.Size(164, 180)
    '
    'mnuScale
    '
    Me.mnuScale.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuScaleHalf, Me.mnuScaleNorm, Me.mnuScaleTwice})
    Me.mnuScale.Image = Global.LimeSeed.My.Resources.Resources.scale
    Me.mnuScale.Name = "mnuScale"
    Me.mnuScale.Size = New System.Drawing.Size(163, 22)
    Me.mnuScale.Text = "Scale"
    '
    'mnuScaleHalf
    '
    Me.mnuScaleHalf.CheckOnClick = True
    Me.mnuScaleHalf.Name = "mnuScaleHalf"
    Me.mnuScaleHalf.Size = New System.Drawing.Size(90, 22)
    Me.mnuScaleHalf.Text = "½x"
    '
    'mnuScaleNorm
    '
    Me.mnuScaleNorm.CheckOnClick = True
    Me.mnuScaleNorm.Name = "mnuScaleNorm"
    Me.mnuScaleNorm.Size = New System.Drawing.Size(90, 22)
    Me.mnuScaleNorm.Text = "1x"
    '
    'mnuScaleTwice
    '
    Me.mnuScaleTwice.CheckOnClick = True
    Me.mnuScaleTwice.Name = "mnuScaleTwice"
    Me.mnuScaleTwice.Size = New System.Drawing.Size(90, 22)
    Me.mnuScaleTwice.Text = "2x"
    '
    'mnuRatio
    '
    Me.mnuRatio.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuRatioForce, Me.mnuRatioSpace1, Me.mnuRatioAutomatic, Me.mnuRatioStandard, Me.mnuRatioWide})
    Me.mnuRatio.Image = Global.LimeSeed.My.Resources.Resources.ratio
    Me.mnuRatio.Name = "mnuRatio"
    Me.mnuRatio.Size = New System.Drawing.Size(163, 22)
    Me.mnuRatio.Text = "Aspect Ratio"
    '
    'mnuRatioForce
    '
    Me.mnuRatioForce.CheckOnClick = True
    Me.mnuRatioForce.Name = "mnuRatioForce"
    Me.mnuRatioForce.Size = New System.Drawing.Size(172, 22)
    Me.mnuRatioForce.Text = "Force Aspect Ratio"
    '
    'mnuRatioSpace1
    '
    Me.mnuRatioSpace1.Name = "mnuRatioSpace1"
    Me.mnuRatioSpace1.Size = New System.Drawing.Size(169, 6)
    '
    'mnuRatioAutomatic
    '
    Me.mnuRatioAutomatic.CheckOnClick = True
    Me.mnuRatioAutomatic.Name = "mnuRatioAutomatic"
    Me.mnuRatioAutomatic.Size = New System.Drawing.Size(172, 22)
    Me.mnuRatioAutomatic.Text = "Automatic"
    '
    'mnuRatioStandard
    '
    Me.mnuRatioStandard.CheckOnClick = True
    Me.mnuRatioStandard.Name = "mnuRatioStandard"
    Me.mnuRatioStandard.Size = New System.Drawing.Size(172, 22)
    Me.mnuRatioStandard.Text = "4:3"
    '
    'mnuRatioWide
    '
    Me.mnuRatioWide.CheckOnClick = True
    Me.mnuRatioWide.Name = "mnuRatioWide"
    Me.mnuRatioWide.Size = New System.Drawing.Size(172, 22)
    Me.mnuRatioWide.Text = "16:9"
    '
    'mnuFullScreen
    '
    Me.mnuFullScreen.Image = Global.LimeSeed.My.Resources.Resources.button_fs
    Me.mnuFullScreen.Name = "mnuFullScreen"
    Me.mnuFullScreen.Size = New System.Drawing.Size(163, 22)
    Me.mnuFullScreen.Text = "Full Screen"
    '
    'mnuVideoTrack
    '
    Me.mnuVideoTrack.Name = "mnuVideoTrack"
    Me.mnuVideoTrack.Size = New System.Drawing.Size(163, 22)
    Me.mnuVideoTrack.Text = "Video Track"
    '
    'mnuAudioTrack
    '
    Me.mnuAudioTrack.Image = Global.LimeSeed.My.Resources.Resources.button_mute
    Me.mnuAudioTrack.Name = "mnuAudioTrack"
    Me.mnuAudioTrack.Size = New System.Drawing.Size(163, 22)
    Me.mnuAudioTrack.Text = "Audio Track"
    '
    'mnuSubtitleTrack
    '
    Me.mnuSubtitleTrack.Image = Global.LimeSeed.My.Resources.Resources.subtitles
    Me.mnuSubtitleTrack.Name = "mnuSubtitleTrack"
    Me.mnuSubtitleTrack.Size = New System.Drawing.Size(163, 22)
    Me.mnuSubtitleTrack.Text = "Subtitle Track"
    '
    'mnuChapterTrack
    '
    Me.mnuChapterTrack.Image = Global.LimeSeed.My.Resources.Resources.chapters
    Me.mnuChapterTrack.Name = "mnuChapterTrack"
    Me.mnuChapterTrack.Size = New System.Drawing.Size(163, 22)
    Me.mnuChapterTrack.Text = "Chapters"
    '
    'mnuCopyScreenshot
    '
    Me.mnuCopyScreenshot.Image = Global.LimeSeed.My.Resources.Resources.copy
    Me.mnuCopyScreenshot.Name = "mnuCopyScreenshot"
    Me.mnuCopyScreenshot.Size = New System.Drawing.Size(163, 22)
    Me.mnuCopyScreenshot.Text = "Copy Screenshot"
    '
    'BottomToolStripPanel
    '
    Me.BottomToolStripPanel.Location = New System.Drawing.Point(0, 0)
    Me.BottomToolStripPanel.Name = "BottomToolStripPanel"
    Me.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
    Me.BottomToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
    Me.BottomToolStripPanel.Size = New System.Drawing.Size(0, 0)
    '
    'TopToolStripPanel
    '
    Me.TopToolStripPanel.Location = New System.Drawing.Point(0, 0)
    Me.TopToolStripPanel.Name = "TopToolStripPanel"
    Me.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
    Me.TopToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
    Me.TopToolStripPanel.Size = New System.Drawing.Size(0, 0)
    '
    'RightToolStripPanel
    '
    Me.RightToolStripPanel.Location = New System.Drawing.Point(0, 0)
    Me.RightToolStripPanel.Name = "RightToolStripPanel"
    Me.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
    Me.RightToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
    Me.RightToolStripPanel.Size = New System.Drawing.Size(0, 0)
    '
    'LeftToolStripPanel
    '
    Me.LeftToolStripPanel.Location = New System.Drawing.Point(0, 0)
    Me.LeftToolStripPanel.Name = "LeftToolStripPanel"
    Me.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
    Me.LeftToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
    Me.LeftToolStripPanel.Size = New System.Drawing.Size(0, 0)
    '
    'ContentPanel
    '
    Me.ContentPanel.Size = New System.Drawing.Size(255, 203)
    '
    'tmrCommandCycle
    '
    Me.tmrCommandCycle.Interval = 1000
    '
    'mnuPL
    '
    Me.mnuPL.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuPLPlay, Me.ToolStripMenuItem1, Me.mnuPLProps, Me.mnuPLOpenFile, Me.ToolStripMenuItem2, Me.mnuPLDelete})
    Me.mnuPL.Name = "mnuPL"
    Me.mnuPL.Size = New System.Drawing.Size(174, 104)
    '
    'mnuPLPlay
    '
    Me.mnuPLPlay.Image = Global.LimeSeed.My.Resources.Resources.button_play
    Me.mnuPLPlay.Name = "mnuPLPlay"
    Me.mnuPLPlay.Size = New System.Drawing.Size(173, 22)
    Me.mnuPLPlay.Text = "&Play Item"
    '
    'ToolStripMenuItem1
    '
    Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
    Me.ToolStripMenuItem1.Size = New System.Drawing.Size(170, 6)
    '
    'mnuPLProps
    '
    Me.mnuPLProps.Image = Global.LimeSeed.My.Resources.Resources.properties
    Me.mnuPLProps.Name = "mnuPLProps"
    Me.mnuPLProps.Size = New System.Drawing.Size(173, 22)
    Me.mnuPLProps.Text = "P&roperties..."
    '
    'mnuPLOpenFile
    '
    Me.mnuPLOpenFile.Image = Global.LimeSeed.My.Resources.Resources.pl_button_open
    Me.mnuPLOpenFile.Name = "mnuPLOpenFile"
    Me.mnuPLOpenFile.Size = New System.Drawing.Size(173, 22)
    Me.mnuPLOpenFile.Text = "&Open File Location"
    '
    'ToolStripMenuItem2
    '
    Me.ToolStripMenuItem2.Name = "ToolStripMenuItem2"
    Me.ToolStripMenuItem2.Size = New System.Drawing.Size(170, 6)
    '
    'mnuPLDelete
    '
    Me.mnuPLDelete.Image = Global.LimeSeed.My.Resources.Resources.pl_button_del
    Me.mnuPLDelete.Name = "mnuPLDelete"
    Me.mnuPLDelete.Size = New System.Drawing.Size(173, 22)
    Me.mnuPLDelete.Text = "De&lete"
    '
    'tmrVis
    '
    Me.tmrVis.Interval = 66
    '
    'svcSqueezer
    '
    Me.svcSqueezer.ServiceName = "LimeSqueezer"
    '
    'tmrHideAlert
    '
    Me.tmrHideAlert.Interval = 4000
    '
    'frmMain
    '
    Me.AllowDrop = True
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.BackColor = System.Drawing.Color.Black
    Me.ClientSize = New System.Drawing.Size(940, 377)
    Me.Controls.Add(Me.pnlMain)
    Me.Icon = Global.LimeSeed.My.Resources.Resources.norm
    Me.MainMenuStrip = Me.mnuMain
    Me.MinimumSize = New System.Drawing.Size(320, 108)
    Me.Name = "frmMain"
    Me.Text = "Lime Seed Media Player"
    Me.pnlMain.ResumeLayout(False)
    Me.pnlMain.PerformLayout()
    Me.pnlControls.ResumeLayout(False)
    Me.pnlVidOpts.ResumeLayout(False)
    CType(Me.pctChannels, System.ComponentModel.ISupportInitialize).EndInit()
    CType(Me.pctBitrate, System.ComponentModel.ISupportInitialize).EndInit()
    CType(Me.pctQuality, System.ComponentModel.ISupportInitialize).EndInit()
    CType(Me.pctBeat, System.ComponentModel.ISupportInitialize).EndInit()
    Me.tbsView.ResumeLayout(False)
    Me.tabArt.ResumeLayout(False)
    CType(Me.pctAlbumArt, System.ComponentModel.ISupportInitialize).EndInit()
    Me.mnuAlbumArt.ResumeLayout(False)
    Me.tabVideo.ResumeLayout(False)
    Me.tabRipper.ResumeLayout(False)
    Me.tabPlayList.ResumeLayout(False)
    Me.pnlPlayList.ResumeLayout(False)
    Me.pnlPlayList.PerformLayout()
    Me.pnlPlayListControls.ResumeLayout(False)
    CType(Me.dgvPlayList, System.ComponentModel.ISupportInitialize).EndInit()
    Me.pnlProgress.ResumeLayout(False)
    Me.mnuMain.ResumeLayout(False)
    Me.mnuMain.PerformLayout()
    Me.mnuVideo.ResumeLayout(False)
    Me.mnuPL.ResumeLayout(False)
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents tmrUpdate As System.Windows.Forms.Timer
  Friend WithEvents pnlMain As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents mnuMain As System.Windows.Forms.MenuStrip
  Friend WithEvents mnuFile As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuOpenFile As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuCloseFile As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuSpace2 As System.Windows.Forms.ToolStripSeparator
  Friend WithEvents mnuExit As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuSpace1 As System.Windows.Forms.ToolStripSeparator
  Friend WithEvents mnuProperties As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents pbProgress As BetterProgress 'System.Windows.Forms.ProgressBar
  Friend WithEvents pnlControls As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents cmdPlayPause As System.Windows.Forms.Button
  Friend WithEvents cmdStop As System.Windows.Forms.Button
  Friend WithEvents cmdFullScreen As System.Windows.Forms.Button
  Friend WithEvents cmdMute As System.Windows.Forms.Button
  Friend WithEvents tbsView As Dotnetrix.Controls.TabControl
  Friend WithEvents tabArt As System.Windows.Forms.TabPage
  Friend WithEvents pctAlbumArt As System.Windows.Forms.PictureBox
  Friend WithEvents tabVideo As System.Windows.Forms.TabPage
  Friend WithEvents tabRipper As System.Windows.Forms.TabPage
  Friend WithEvents mpPlayer As Seed.ctlSeed
  Friend WithEvents ripBox As LimeSeed.ctlRipper
  Friend WithEvents tabPlayList As System.Windows.Forms.TabPage
  Friend WithEvents pnlPlayList As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents pnlPlayListControls As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents cmdAddToPL As System.Windows.Forms.Button
  Friend WithEvents cmdClearPL As System.Windows.Forms.Button
  Friend WithEvents cmdRemoveFromPL As System.Windows.Forms.Button
  Friend WithEvents BottomToolStripPanel As System.Windows.Forms.ToolStripPanel
  Friend WithEvents TopToolStripPanel As System.Windows.Forms.ToolStripPanel
  Friend WithEvents RightToolStripPanel As System.Windows.Forms.ToolStripPanel
  Friend WithEvents LeftToolStripPanel As System.Windows.Forms.ToolStripPanel
  Friend WithEvents ContentPanel As System.Windows.Forms.ToolStripContentPanel
  Friend WithEvents cmdBackPL As System.Windows.Forms.Button
  Friend WithEvents cmdNextPL As System.Windows.Forms.Button
  Friend WithEvents mnuOptions As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuSettings As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents dgvPlayList As System.Windows.Forms.DataGridView
  Friend WithEvents cmbChapters As System.Windows.Forms.ComboBox
  Friend WithEvents colTitle As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colLength As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents cmbAudTrack As System.Windows.Forms.ComboBox
  Friend WithEvents cmbVidTrack As System.Windows.Forms.ComboBox
  Friend WithEvents cmbSubtitles As System.Windows.Forms.ComboBox
  Friend WithEvents pnlVidOpts As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents bpgVolume As LimeSeed.BetterProgress
  Friend WithEvents tmrCommandCycle As System.Windows.Forms.Timer
  Friend WithEvents ttDisp As System.Windows.Forms.ToolTip
  Friend WithEvents mnuVideo As System.Windows.Forms.ContextMenuStrip
  Friend WithEvents mnuRatio As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuRatioForce As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuRatioSpace1 As System.Windows.Forms.ToolStripSeparator
  Friend WithEvents mnuRatioAutomatic As ToolStripRadioButtonMenuItem
  Friend WithEvents mnuRatioStandard As ToolStripRadioButtonMenuItem
  Friend WithEvents mnuRatioWide As ToolStripRadioButtonMenuItem
  Friend WithEvents mnuFullScreen As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuPL As System.Windows.Forms.ContextMenuStrip
  Friend WithEvents mnuPLPlay As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents ToolStripMenuItem1 As System.Windows.Forms.ToolStripSeparator
  Friend WithEvents mnuPLProps As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuPLOpenFile As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents ToolStripMenuItem2 As System.Windows.Forms.ToolStripSeparator
  Friend WithEvents mnuPLDelete As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents artList As LimeSeed.ctlArtList
  Friend WithEvents pnlProgress As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents cmdOpenPL As System.Windows.Forms.Button
  Friend WithEvents cmdSavePL As System.Windows.Forms.Button
  Friend WithEvents txtPlayListTitle As System.Windows.Forms.TextBox
  Friend WithEvents mnuHelp As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuWebpage As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents ToolStripMenuItem3 As System.Windows.Forms.ToolStripSeparator
  Friend WithEvents mnuAbout As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuOutDev As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuOutDefault As ToolStripRadioButtonMenuItem
  Friend WithEvents ToolStripMenuItem4 As System.Windows.Forms.ToolStripSeparator
  Friend WithEvents pctChannels As System.Windows.Forms.PictureBox
  Friend WithEvents pctBitrate As System.Windows.Forms.PictureBox
  Friend WithEvents cmdLoop As System.Windows.Forms.Button
  Friend WithEvents cmdLoopPL As System.Windows.Forms.Button
  Friend WithEvents cmdShufflePL As System.Windows.Forms.Button
  Friend WithEvents pctQuality As System.Windows.Forms.PictureBox
  Friend WithEvents cmdMenu As System.Windows.Forms.Button
  Friend WithEvents pctBeat As System.Windows.Forms.PictureBox
  Friend WithEvents tmrVis As System.Windows.Forms.Timer
  Friend WithEvents mnuAlbumArt As System.Windows.Forms.ContextMenuStrip
  Friend WithEvents mnuArtShow As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuArtFind As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuArtSpace As System.Windows.Forms.ToolStripSeparator
  Friend WithEvents mnuArtVisShow As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuArtVisSelect As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuScale As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuScaleHalf As ToolStripRadioButtonMenuItem
  Friend WithEvents mnuScaleNorm As ToolStripRadioButtonMenuItem
  Friend WithEvents mnuScaleTwice As ToolStripRadioButtonMenuItem
  Friend WithEvents mnuCopyScreenshot As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuAudioTrack As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuSubtitleTrack As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuChapterTrack As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuVideoTrack As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuTransferFile As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents svcSqueezer As System.ServiceProcess.ServiceController
  Friend WithEvents pbArt As System.Windows.Forms.ProgressBar
  Friend WithEvents lblPLAlert As System.Windows.Forms.Label
  Friend WithEvents tmrHideAlert As System.Windows.Forms.Timer

End Class
