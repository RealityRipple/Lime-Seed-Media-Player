<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmProps
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
    Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
    Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
    Me.tabProps = New System.Windows.Forms.TabControl()
    Me.tabClip = New System.Windows.Forms.TabPage()
    Me.pnlClip = New System.Windows.Forms.TableLayoutPanel()
    Me.pctClipIcon = New System.Windows.Forms.PictureBox()
    Me.grpMetadata = New System.Windows.Forms.GroupBox()
    Me.tbsTags = New System.Windows.Forms.TabControl()
    Me.tabID3v1 = New System.Windows.Forms.TabPage()
    Me.pnlID3v1 = New System.Windows.Forms.TableLayoutPanel()
    Me.dgvID3v1 = New System.Windows.Forms.DataGridView()
    Me.cmdID3v1Reset = New System.Windows.Forms.Button()
    Me.cmdID3v1Save = New System.Windows.Forms.Button()
    Me.tabID3v2 = New System.Windows.Forms.TabPage()
    Me.pnlID3v2 = New System.Windows.Forms.TableLayoutPanel()
    Me.dgvID3v2 = New System.Windows.Forms.DataGridView()
    Me.cmdID3v2Add = New System.Windows.Forms.Button()
    Me.cmdID3v2Remove = New System.Windows.Forms.Button()
    Me.cmdID3v2Save = New System.Windows.Forms.Button()
    Me.cmdID3v2Reset = New System.Windows.Forms.Button()
    Me.tabVorbis = New System.Windows.Forms.TabPage()
    Me.dgvVorbis = New System.Windows.Forms.DataGridView()
    Me.tabMPEG = New System.Windows.Forms.TabPage()
    Me.dgvMPEG = New System.Windows.Forms.DataGridView()
    Me.tabMKV = New System.Windows.Forms.TabPage()
    Me.tvMKV = New System.Windows.Forms.TreeView()
    Me.tabMIDI = New System.Windows.Forms.TabPage()
    Me.tvMIDI = New System.Windows.Forms.TreeView()
    Me.tabRIFF = New System.Windows.Forms.TabPage()
    Me.tvRIFF = New System.Windows.Forms.TreeView()
    Me.lblLocation = New System.Windows.Forms.Label()
    Me.txtClipFileName = New System.Windows.Forms.TextBox()
    Me.txtFilePath = New System.Windows.Forms.TextBox()
    Me.tabDetails = New System.Windows.Forms.TabPage()
    Me.pnlDetails = New System.Windows.Forms.TableLayoutPanel()
    Me.pctDetailsIcon = New System.Windows.Forms.PictureBox()
    Me.txtDetailsFileName = New System.Windows.Forms.TextBox()
    Me.pctPreview = New System.Windows.Forms.PictureBox()
    Me.lblType = New System.Windows.Forms.Label()
    Me.txtType = New System.Windows.Forms.TextBox()
    Me.lblSize = New System.Windows.Forms.Label()
    Me.txtSize = New System.Windows.Forms.TextBox()
    Me.lblLength = New System.Windows.Forms.Label()
    Me.txtLength = New System.Windows.Forms.TextBox()
    Me.lblVideoSize = New System.Windows.Forms.Label()
    Me.txtVideoSize = New System.Windows.Forms.TextBox()
    Me.lblCreated = New System.Windows.Forms.Label()
    Me.txtCreated = New System.Windows.Forms.TextBox()
    Me.tabAdvanced = New System.Windows.Forms.TabPage()
    Me.pnlAdvanced = New System.Windows.Forms.TableLayoutPanel()
    Me.pctAdvancedIcon = New System.Windows.Forms.PictureBox()
    Me.txtAdvancedFileName = New System.Windows.Forms.TextBox()
    Me.lblFilters = New System.Windows.Forms.Label()
    Me.pnlFilters = New System.Windows.Forms.TableLayoutPanel()
    Me.lstFilters = New System.Windows.Forms.ListBox()
    Me.cmdFilterProps = New System.Windows.Forms.Button()
    Me.cmdOK = New System.Windows.Forms.Button()
    Me.tmrGenPreview = New System.Windows.Forms.Timer(Me.components)
    Me.colID3v2Tag = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colID3v2Value = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colID3v1Tag = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colID3v1Value = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colVorbisTag = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colVorbisValue = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colMPEGTag = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.colMPEGValue = New System.Windows.Forms.DataGridViewTextBoxColumn()
    Me.tabProps.SuspendLayout()
    Me.tabClip.SuspendLayout()
    Me.pnlClip.SuspendLayout()
    CType(Me.pctClipIcon, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.grpMetadata.SuspendLayout()
    Me.tbsTags.SuspendLayout()
    Me.tabID3v1.SuspendLayout()
    Me.pnlID3v1.SuspendLayout()
    CType(Me.dgvID3v1, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.tabID3v2.SuspendLayout()
    Me.pnlID3v2.SuspendLayout()
    CType(Me.dgvID3v2, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.tabVorbis.SuspendLayout()
    CType(Me.dgvVorbis, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.tabMPEG.SuspendLayout()
    CType(Me.dgvMPEG, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.tabMKV.SuspendLayout()
    Me.tabMIDI.SuspendLayout()
    Me.tabRIFF.SuspendLayout()
    Me.tabDetails.SuspendLayout()
    Me.pnlDetails.SuspendLayout()
    CType(Me.pctDetailsIcon, System.ComponentModel.ISupportInitialize).BeginInit()
    CType(Me.pctPreview, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.tabAdvanced.SuspendLayout()
    Me.pnlAdvanced.SuspendLayout()
    CType(Me.pctAdvancedIcon, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.pnlFilters.SuspendLayout()
    Me.SuspendLayout()
    '
    'tabProps
    '
    Me.tabProps.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.tabProps.Controls.Add(Me.tabClip)
    Me.tabProps.Controls.Add(Me.tabDetails)
    Me.tabProps.Controls.Add(Me.tabAdvanced)
    Me.tabProps.Font = New System.Drawing.Font("Microsoft Sans Serif", 11.0!)
    Me.tabProps.Location = New System.Drawing.Point(12, 12)
    Me.tabProps.Name = "tabProps"
    Me.tabProps.SelectedIndex = 0
    Me.tabProps.Size = New System.Drawing.Size(330, 396)
    Me.tabProps.SizeMode = System.Windows.Forms.TabSizeMode.Fixed
    Me.tabProps.TabIndex = 0
    '
    'tabClip
    '
    Me.tabClip.Controls.Add(Me.pnlClip)
    Me.tabClip.Location = New System.Drawing.Point(4, 27)
    Me.tabClip.Name = "tabClip"
    Me.tabClip.Size = New System.Drawing.Size(322, 365)
    Me.tabClip.TabIndex = 0
    Me.tabClip.Text = "Clip"
    Me.tabClip.UseVisualStyleBackColor = True
    '
    'pnlClip
    '
    Me.pnlClip.ColumnCount = 2
    Me.pnlClip.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60.0!))
    Me.pnlClip.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlClip.Controls.Add(Me.pctClipIcon, 0, 0)
    Me.pnlClip.Controls.Add(Me.grpMetadata, 0, 1)
    Me.pnlClip.Controls.Add(Me.lblLocation, 0, 2)
    Me.pnlClip.Controls.Add(Me.txtClipFileName, 1, 0)
    Me.pnlClip.Controls.Add(Me.txtFilePath, 1, 2)
    Me.pnlClip.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlClip.Location = New System.Drawing.Point(0, 0)
    Me.pnlClip.Name = "pnlClip"
    Me.pnlClip.RowCount = 3
    Me.pnlClip.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50.0!))
    Me.pnlClip.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlClip.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlClip.Size = New System.Drawing.Size(322, 365)
    Me.pnlClip.TabIndex = 0
    '
    'pctClipIcon
    '
    Me.pctClipIcon.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.pctClipIcon.Location = New System.Drawing.Point(6, 1)
    Me.pctClipIcon.Margin = New System.Windows.Forms.Padding(0)
    Me.pctClipIcon.Name = "pctClipIcon"
    Me.pctClipIcon.Size = New System.Drawing.Size(48, 48)
    Me.pctClipIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
    Me.pctClipIcon.TabIndex = 0
    Me.pctClipIcon.TabStop = False
    '
    'grpMetadata
    '
    Me.pnlClip.SetColumnSpan(Me.grpMetadata, 2)
    Me.grpMetadata.Controls.Add(Me.tbsTags)
    Me.grpMetadata.Dock = System.Windows.Forms.DockStyle.Fill
    Me.grpMetadata.Location = New System.Drawing.Point(3, 53)
    Me.grpMetadata.Name = "grpMetadata"
    Me.grpMetadata.Padding = New System.Windows.Forms.Padding(8)
    Me.grpMetadata.Size = New System.Drawing.Size(316, 279)
    Me.grpMetadata.TabIndex = 2
    Me.grpMetadata.TabStop = False
    Me.grpMetadata.Text = "Clip Tags"
    '
    'tbsTags
    '
    Me.tbsTags.Controls.Add(Me.tabID3v1)
    Me.tbsTags.Controls.Add(Me.tabID3v2)
    Me.tbsTags.Controls.Add(Me.tabVorbis)
    Me.tbsTags.Controls.Add(Me.tabMPEG)
    Me.tbsTags.Controls.Add(Me.tabMKV)
    Me.tbsTags.Controls.Add(Me.tabMIDI)
    Me.tbsTags.Controls.Add(Me.tabRIFF)
    Me.tbsTags.Dock = System.Windows.Forms.DockStyle.Fill
    Me.tbsTags.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!)
    Me.tbsTags.Location = New System.Drawing.Point(8, 25)
    Me.tbsTags.Name = "tbsTags"
    Me.tbsTags.SelectedIndex = 0
    Me.tbsTags.Size = New System.Drawing.Size(300, 246)
    Me.tbsTags.TabIndex = 0
    '
    'tabID3v1
    '
    Me.tabID3v1.Controls.Add(Me.pnlID3v1)
    Me.tabID3v1.Location = New System.Drawing.Point(4, 24)
    Me.tabID3v1.Name = "tabID3v1"
    Me.tabID3v1.Size = New System.Drawing.Size(292, 218)
    Me.tabID3v1.TabIndex = 0
    Me.tabID3v1.Text = "ID3v1"
    Me.tabID3v1.UseVisualStyleBackColor = True
    '
    'pnlID3v1
    '
    Me.pnlID3v1.ColumnCount = 2
    Me.pnlID3v1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.pnlID3v1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.pnlID3v1.Controls.Add(Me.dgvID3v1, 0, 0)
    Me.pnlID3v1.Controls.Add(Me.cmdID3v1Reset, 1, 1)
    Me.pnlID3v1.Controls.Add(Me.cmdID3v1Save, 0, 1)
    Me.pnlID3v1.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlID3v1.Location = New System.Drawing.Point(0, 0)
    Me.pnlID3v1.Name = "pnlID3v1"
    Me.pnlID3v1.RowCount = 2
    Me.pnlID3v1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlID3v1.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlID3v1.Size = New System.Drawing.Size(292, 218)
    Me.pnlID3v1.TabIndex = 0
    '
    'dgvID3v1
    '
    Me.dgvID3v1.AllowUserToAddRows = False
    Me.dgvID3v1.AllowUserToDeleteRows = False
    Me.dgvID3v1.AllowUserToResizeColumns = False
    Me.dgvID3v1.AllowUserToResizeRows = False
    DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ButtonFace
    Me.dgvID3v1.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
    Me.dgvID3v1.BackgroundColor = System.Drawing.SystemColors.Window
    Me.dgvID3v1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
    Me.dgvID3v1.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText
    Me.dgvID3v1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
    Me.dgvID3v1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colID3v1Tag, Me.colID3v1Value})
    Me.pnlID3v1.SetColumnSpan(Me.dgvID3v1, 2)
    Me.dgvID3v1.Dock = System.Windows.Forms.DockStyle.Fill
    Me.dgvID3v1.Location = New System.Drawing.Point(3, 3)
    Me.dgvID3v1.Name = "dgvID3v1"
    Me.dgvID3v1.RowHeadersVisible = False
    Me.dgvID3v1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
    Me.dgvID3v1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
    Me.dgvID3v1.Size = New System.Drawing.Size(286, 181)
    Me.dgvID3v1.TabIndex = 0
    '
    'cmdID3v1Reset
    '
    Me.cmdID3v1Reset.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.cmdID3v1Reset.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdID3v1Reset.Location = New System.Drawing.Point(181, 190)
    Me.cmdID3v1Reset.Name = "cmdID3v1Reset"
    Me.cmdID3v1Reset.Size = New System.Drawing.Size(75, 25)
    Me.cmdID3v1Reset.TabIndex = 2
    Me.cmdID3v1Reset.Text = "Reset"
    Me.cmdID3v1Reset.UseVisualStyleBackColor = True
    '
    'cmdID3v1Save
    '
    Me.cmdID3v1Save.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.cmdID3v1Save.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdID3v1Save.Location = New System.Drawing.Point(35, 190)
    Me.cmdID3v1Save.Name = "cmdID3v1Save"
    Me.cmdID3v1Save.Size = New System.Drawing.Size(75, 25)
    Me.cmdID3v1Save.TabIndex = 1
    Me.cmdID3v1Save.Text = "Save"
    Me.cmdID3v1Save.UseVisualStyleBackColor = True
    '
    'tabID3v2
    '
    Me.tabID3v2.Controls.Add(Me.pnlID3v2)
    Me.tabID3v2.Location = New System.Drawing.Point(4, 24)
    Me.tabID3v2.Name = "tabID3v2"
    Me.tabID3v2.Size = New System.Drawing.Size(292, 218)
    Me.tabID3v2.TabIndex = 1
    Me.tabID3v2.Text = "ID3v2"
    Me.tabID3v2.UseVisualStyleBackColor = True
    '
    'pnlID3v2
    '
    Me.pnlID3v2.ColumnCount = 2
    Me.pnlID3v2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.00249!))
    Me.pnlID3v2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.99751!))
    Me.pnlID3v2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
    Me.pnlID3v2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
    Me.pnlID3v2.Controls.Add(Me.dgvID3v2, 0, 0)
    Me.pnlID3v2.Controls.Add(Me.cmdID3v2Add, 0, 1)
    Me.pnlID3v2.Controls.Add(Me.cmdID3v2Remove, 1, 1)
    Me.pnlID3v2.Controls.Add(Me.cmdID3v2Save, 0, 2)
    Me.pnlID3v2.Controls.Add(Me.cmdID3v2Reset, 1, 2)
    Me.pnlID3v2.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlID3v2.Location = New System.Drawing.Point(0, 0)
    Me.pnlID3v2.Name = "pnlID3v2"
    Me.pnlID3v2.RowCount = 3
    Me.pnlID3v2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlID3v2.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlID3v2.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlID3v2.Size = New System.Drawing.Size(292, 218)
    Me.pnlID3v2.TabIndex = 2
    '
    'dgvID3v2
    '
    Me.dgvID3v2.AllowUserToAddRows = False
    Me.dgvID3v2.AllowUserToDeleteRows = False
    Me.dgvID3v2.AllowUserToResizeColumns = False
    Me.dgvID3v2.AllowUserToResizeRows = False
    DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ButtonFace
    Me.dgvID3v2.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle2
    Me.dgvID3v2.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells
    Me.dgvID3v2.BackgroundColor = System.Drawing.SystemColors.Window
    Me.dgvID3v2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
    Me.dgvID3v2.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText
    Me.dgvID3v2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
    Me.dgvID3v2.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colID3v2Tag, Me.colID3v2Value})
    Me.pnlID3v2.SetColumnSpan(Me.dgvID3v2, 2)
    Me.dgvID3v2.Dock = System.Windows.Forms.DockStyle.Fill
    Me.dgvID3v2.Location = New System.Drawing.Point(3, 3)
    Me.dgvID3v2.Name = "dgvID3v2"
    Me.dgvID3v2.RowHeadersVisible = False
    Me.dgvID3v2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
    Me.dgvID3v2.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
    Me.dgvID3v2.Size = New System.Drawing.Size(286, 150)
    Me.dgvID3v2.TabIndex = 0
    '
    'cmdID3v2Add
    '
    Me.cmdID3v2Add.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.cmdID3v2Add.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdID3v2Add.Location = New System.Drawing.Point(28, 159)
    Me.cmdID3v2Add.Name = "cmdID3v2Add"
    Me.cmdID3v2Add.Size = New System.Drawing.Size(90, 25)
    Me.cmdID3v2Add.TabIndex = 1
    Me.cmdID3v2Add.Text = "Add Tag"
    Me.cmdID3v2Add.UseVisualStyleBackColor = True
    '
    'cmdID3v2Remove
    '
    Me.cmdID3v2Remove.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.cmdID3v2Remove.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdID3v2Remove.Location = New System.Drawing.Point(174, 159)
    Me.cmdID3v2Remove.Name = "cmdID3v2Remove"
    Me.cmdID3v2Remove.Size = New System.Drawing.Size(90, 25)
    Me.cmdID3v2Remove.TabIndex = 2
    Me.cmdID3v2Remove.Text = "Remove Tag"
    Me.cmdID3v2Remove.UseVisualStyleBackColor = True
    '
    'cmdID3v2Save
    '
    Me.cmdID3v2Save.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.cmdID3v2Save.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdID3v2Save.Location = New System.Drawing.Point(35, 190)
    Me.cmdID3v2Save.Name = "cmdID3v2Save"
    Me.cmdID3v2Save.Size = New System.Drawing.Size(75, 25)
    Me.cmdID3v2Save.TabIndex = 3
    Me.cmdID3v2Save.Text = "Save"
    Me.cmdID3v2Save.UseVisualStyleBackColor = True
    '
    'cmdID3v2Reset
    '
    Me.cmdID3v2Reset.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.cmdID3v2Reset.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdID3v2Reset.Location = New System.Drawing.Point(181, 190)
    Me.cmdID3v2Reset.Name = "cmdID3v2Reset"
    Me.cmdID3v2Reset.Size = New System.Drawing.Size(75, 25)
    Me.cmdID3v2Reset.TabIndex = 4
    Me.cmdID3v2Reset.Text = "Reset"
    Me.cmdID3v2Reset.UseVisualStyleBackColor = True
    '
    'tabVorbis
    '
    Me.tabVorbis.Controls.Add(Me.dgvVorbis)
    Me.tabVorbis.Location = New System.Drawing.Point(4, 24)
    Me.tabVorbis.Name = "tabVorbis"
    Me.tabVorbis.Size = New System.Drawing.Size(292, 218)
    Me.tabVorbis.TabIndex = 3
    Me.tabVorbis.Text = "Vorbis"
    Me.tabVorbis.UseVisualStyleBackColor = True
    '
    'dgvVorbis
    '
    Me.dgvVorbis.AllowUserToAddRows = False
    Me.dgvVorbis.AllowUserToDeleteRows = False
    Me.dgvVorbis.AllowUserToResizeColumns = False
    Me.dgvVorbis.AllowUserToResizeRows = False
    DataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.ButtonFace
    Me.dgvVorbis.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle3
    Me.dgvVorbis.BackgroundColor = System.Drawing.SystemColors.Window
    Me.dgvVorbis.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
    Me.dgvVorbis.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText
    Me.dgvVorbis.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
    Me.dgvVorbis.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colVorbisTag, Me.colVorbisValue})
    Me.dgvVorbis.Dock = System.Windows.Forms.DockStyle.Fill
    Me.dgvVorbis.Location = New System.Drawing.Point(0, 0)
    Me.dgvVorbis.Name = "dgvVorbis"
    Me.dgvVorbis.RowHeadersVisible = False
    Me.dgvVorbis.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
    Me.dgvVorbis.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
    Me.dgvVorbis.Size = New System.Drawing.Size(292, 218)
    Me.dgvVorbis.TabIndex = 1
    '
    'tabMPEG
    '
    Me.tabMPEG.Controls.Add(Me.dgvMPEG)
    Me.tabMPEG.Location = New System.Drawing.Point(4, 24)
    Me.tabMPEG.Name = "tabMPEG"
    Me.tabMPEG.Size = New System.Drawing.Size(292, 218)
    Me.tabMPEG.TabIndex = 2
    Me.tabMPEG.Text = "MPEG Header"
    Me.tabMPEG.UseVisualStyleBackColor = True
    '
    'dgvMPEG
    '
    Me.dgvMPEG.AllowUserToAddRows = False
    Me.dgvMPEG.AllowUserToDeleteRows = False
    Me.dgvMPEG.AllowUserToResizeColumns = False
    Me.dgvMPEG.AllowUserToResizeRows = False
    DataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.ButtonFace
    Me.dgvMPEG.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle4
    Me.dgvMPEG.BackgroundColor = System.Drawing.SystemColors.Window
    Me.dgvMPEG.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
    Me.dgvMPEG.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText
    Me.dgvMPEG.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
    Me.dgvMPEG.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colMPEGTag, Me.colMPEGValue})
    Me.dgvMPEG.Dock = System.Windows.Forms.DockStyle.Fill
    Me.dgvMPEG.Location = New System.Drawing.Point(0, 0)
    Me.dgvMPEG.Name = "dgvMPEG"
    Me.dgvMPEG.ReadOnly = True
    Me.dgvMPEG.RowHeadersVisible = False
    Me.dgvMPEG.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
    Me.dgvMPEG.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
    Me.dgvMPEG.Size = New System.Drawing.Size(292, 218)
    Me.dgvMPEG.TabIndex = 1
    '
    'tabMKV
    '
    Me.tabMKV.Controls.Add(Me.tvMKV)
    Me.tabMKV.Location = New System.Drawing.Point(4, 24)
    Me.tabMKV.Name = "tabMKV"
    Me.tabMKV.Size = New System.Drawing.Size(292, 218)
    Me.tabMKV.TabIndex = 4
    Me.tabMKV.Text = "MKV Header"
    Me.tabMKV.UseVisualStyleBackColor = True
    '
    'tvMKV
    '
    Me.tvMKV.Dock = System.Windows.Forms.DockStyle.Fill
    Me.tvMKV.Location = New System.Drawing.Point(0, 0)
    Me.tvMKV.Name = "tvMKV"
    Me.tvMKV.Size = New System.Drawing.Size(292, 218)
    Me.tvMKV.TabIndex = 0
    '
    'tabMIDI
    '
    Me.tabMIDI.Controls.Add(Me.tvMIDI)
    Me.tabMIDI.Location = New System.Drawing.Point(4, 24)
    Me.tabMIDI.Name = "tabMIDI"
    Me.tabMIDI.Padding = New System.Windows.Forms.Padding(3)
    Me.tabMIDI.Size = New System.Drawing.Size(292, 218)
    Me.tabMIDI.TabIndex = 5
    Me.tabMIDI.Text = "MIDI"
    Me.tabMIDI.UseVisualStyleBackColor = True
    '
    'tvMIDI
    '
    Me.tvMIDI.Dock = System.Windows.Forms.DockStyle.Fill
    Me.tvMIDI.Location = New System.Drawing.Point(3, 3)
    Me.tvMIDI.Name = "tvMIDI"
    Me.tvMIDI.Size = New System.Drawing.Size(286, 212)
    Me.tvMIDI.TabIndex = 1
    '
    'tabRIFF
    '
    Me.tabRIFF.Controls.Add(Me.tvRIFF)
    Me.tabRIFF.Location = New System.Drawing.Point(4, 24)
    Me.tabRIFF.Name = "tabRIFF"
    Me.tabRIFF.Padding = New System.Windows.Forms.Padding(3)
    Me.tabRIFF.Size = New System.Drawing.Size(292, 218)
    Me.tabRIFF.TabIndex = 6
    Me.tabRIFF.Text = "RIFF Header"
    Me.tabRIFF.UseVisualStyleBackColor = True
    '
    'tvRIFF
    '
    Me.tvRIFF.Dock = System.Windows.Forms.DockStyle.Fill
    Me.tvRIFF.Location = New System.Drawing.Point(3, 3)
    Me.tvRIFF.Name = "tvRIFF"
    Me.tvRIFF.Size = New System.Drawing.Size(286, 212)
    Me.tvRIFF.TabIndex = 1
    '
    'lblLocation
    '
    Me.lblLocation.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblLocation.AutoSize = True
    Me.lblLocation.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
    Me.lblLocation.Location = New System.Drawing.Point(3, 343)
    Me.lblLocation.Name = "lblLocation"
    Me.lblLocation.Size = New System.Drawing.Size(51, 13)
    Me.lblLocation.TabIndex = 3
    Me.lblLocation.Text = "Location:"
    '
    'txtClipFileName
    '
    Me.txtClipFileName.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtClipFileName.BackColor = System.Drawing.SystemColors.Window
    Me.txtClipFileName.BorderStyle = System.Windows.Forms.BorderStyle.None
    Me.txtClipFileName.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!)
    Me.txtClipFileName.Location = New System.Drawing.Point(63, 17)
    Me.txtClipFileName.Name = "txtClipFileName"
    Me.txtClipFileName.ReadOnly = True
    Me.txtClipFileName.Size = New System.Drawing.Size(256, 16)
    Me.txtClipFileName.TabIndex = 4
    Me.txtClipFileName.Text = "00 File Name.ext"
    '
    'txtFilePath
    '
    Me.txtFilePath.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtFilePath.BackColor = System.Drawing.SystemColors.Window
    Me.txtFilePath.BorderStyle = System.Windows.Forms.BorderStyle.None
    Me.txtFilePath.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.txtFilePath.Location = New System.Drawing.Point(63, 343)
    Me.txtFilePath.Name = "txtFilePath"
    Me.txtFilePath.ReadOnly = True
    Me.txtFilePath.Size = New System.Drawing.Size(256, 13)
    Me.txtFilePath.TabIndex = 5
    Me.txtFilePath.Text = "Drive:\Path\To\Directory\"
    '
    'tabDetails
    '
    Me.tabDetails.Controls.Add(Me.pnlDetails)
    Me.tabDetails.Location = New System.Drawing.Point(4, 27)
    Me.tabDetails.Name = "tabDetails"
    Me.tabDetails.Size = New System.Drawing.Size(322, 365)
    Me.tabDetails.TabIndex = 1
    Me.tabDetails.Text = "Details"
    Me.tabDetails.UseVisualStyleBackColor = True
    '
    'pnlDetails
    '
    Me.pnlDetails.ColumnCount = 2
    Me.pnlDetails.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60.0!))
    Me.pnlDetails.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlDetails.Controls.Add(Me.pctDetailsIcon, 0, 0)
    Me.pnlDetails.Controls.Add(Me.txtDetailsFileName, 1, 0)
    Me.pnlDetails.Controls.Add(Me.pctPreview, 0, 6)
    Me.pnlDetails.Controls.Add(Me.lblType, 0, 1)
    Me.pnlDetails.Controls.Add(Me.txtType, 1, 1)
    Me.pnlDetails.Controls.Add(Me.lblSize, 0, 2)
    Me.pnlDetails.Controls.Add(Me.txtSize, 1, 2)
    Me.pnlDetails.Controls.Add(Me.lblLength, 0, 3)
    Me.pnlDetails.Controls.Add(Me.txtLength, 1, 3)
    Me.pnlDetails.Controls.Add(Me.lblVideoSize, 0, 4)
    Me.pnlDetails.Controls.Add(Me.txtVideoSize, 1, 4)
    Me.pnlDetails.Controls.Add(Me.lblCreated, 0, 5)
    Me.pnlDetails.Controls.Add(Me.txtCreated, 1, 5)
    Me.pnlDetails.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlDetails.Location = New System.Drawing.Point(0, 0)
    Me.pnlDetails.Name = "pnlDetails"
    Me.pnlDetails.RowCount = 7
    Me.pnlDetails.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50.0!))
    Me.pnlDetails.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlDetails.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlDetails.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlDetails.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlDetails.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
    Me.pnlDetails.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlDetails.Size = New System.Drawing.Size(322, 365)
    Me.pnlDetails.TabIndex = 1
    '
    'pctDetailsIcon
    '
    Me.pctDetailsIcon.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.pctDetailsIcon.Location = New System.Drawing.Point(6, 1)
    Me.pctDetailsIcon.Margin = New System.Windows.Forms.Padding(0)
    Me.pctDetailsIcon.Name = "pctDetailsIcon"
    Me.pctDetailsIcon.Size = New System.Drawing.Size(48, 48)
    Me.pctDetailsIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
    Me.pctDetailsIcon.TabIndex = 0
    Me.pctDetailsIcon.TabStop = False
    '
    'txtDetailsFileName
    '
    Me.txtDetailsFileName.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtDetailsFileName.BackColor = System.Drawing.SystemColors.Window
    Me.txtDetailsFileName.BorderStyle = System.Windows.Forms.BorderStyle.None
    Me.txtDetailsFileName.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!)
    Me.txtDetailsFileName.Location = New System.Drawing.Point(63, 17)
    Me.txtDetailsFileName.Name = "txtDetailsFileName"
    Me.txtDetailsFileName.ReadOnly = True
    Me.txtDetailsFileName.Size = New System.Drawing.Size(256, 16)
    Me.txtDetailsFileName.TabIndex = 4
    Me.txtDetailsFileName.Text = "00 File Name.ext"
    '
    'pctPreview
    '
    Me.pctPreview.BackColor = System.Drawing.Color.Black
    Me.pctPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
    Me.pnlDetails.SetColumnSpan(Me.pctPreview, 2)
    Me.pctPreview.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pctPreview.Location = New System.Drawing.Point(12, 212)
    Me.pctPreview.Margin = New System.Windows.Forms.Padding(12)
    Me.pctPreview.Name = "pctPreview"
    Me.pctPreview.Size = New System.Drawing.Size(298, 141)
    Me.pctPreview.TabIndex = 5
    Me.pctPreview.TabStop = False
    '
    'lblType
    '
    Me.lblType.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.lblType.AutoSize = True
    Me.lblType.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
    Me.lblType.Location = New System.Drawing.Point(23, 58)
    Me.lblType.Name = "lblType"
    Me.lblType.Size = New System.Drawing.Size(34, 13)
    Me.lblType.TabIndex = 6
    Me.lblType.Text = "Type:"
    '
    'txtType
    '
    Me.txtType.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtType.BackColor = System.Drawing.SystemColors.Window
    Me.txtType.BorderStyle = System.Windows.Forms.BorderStyle.None
    Me.txtType.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
    Me.txtType.Location = New System.Drawing.Point(63, 58)
    Me.txtType.Name = "txtType"
    Me.txtType.ReadOnly = True
    Me.txtType.Size = New System.Drawing.Size(256, 13)
    Me.txtType.TabIndex = 7
    Me.txtType.Text = "File Type Here"
    '
    'lblSize
    '
    Me.lblSize.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.lblSize.AutoSize = True
    Me.lblSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
    Me.lblSize.Location = New System.Drawing.Point(27, 88)
    Me.lblSize.Name = "lblSize"
    Me.lblSize.Size = New System.Drawing.Size(30, 13)
    Me.lblSize.TabIndex = 8
    Me.lblSize.Text = "Size:"
    '
    'txtSize
    '
    Me.txtSize.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtSize.BackColor = System.Drawing.SystemColors.Window
    Me.txtSize.BorderStyle = System.Windows.Forms.BorderStyle.None
    Me.txtSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
    Me.txtSize.Location = New System.Drawing.Point(63, 88)
    Me.txtSize.Name = "txtSize"
    Me.txtSize.ReadOnly = True
    Me.txtSize.Size = New System.Drawing.Size(256, 13)
    Me.txtSize.TabIndex = 9
    Me.txtSize.Text = "##.## MB (#,###,### bytes)"
    '
    'lblLength
    '
    Me.lblLength.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.lblLength.AutoSize = True
    Me.lblLength.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
    Me.lblLength.Location = New System.Drawing.Point(14, 118)
    Me.lblLength.Name = "lblLength"
    Me.lblLength.Size = New System.Drawing.Size(43, 13)
    Me.lblLength.TabIndex = 10
    Me.lblLength.Text = "Length:"
    '
    'txtLength
    '
    Me.txtLength.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtLength.BackColor = System.Drawing.SystemColors.Window
    Me.txtLength.BorderStyle = System.Windows.Forms.BorderStyle.None
    Me.txtLength.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
    Me.txtLength.Location = New System.Drawing.Point(63, 118)
    Me.txtLength.Name = "txtLength"
    Me.txtLength.ReadOnly = True
    Me.txtLength.Size = New System.Drawing.Size(256, 13)
    Me.txtLength.TabIndex = 11
    Me.txtLength.Text = "## minutes and ## seconds"
    '
    'lblVideoSize
    '
    Me.lblVideoSize.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.lblVideoSize.AutoSize = True
    Me.lblVideoSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
    Me.lblVideoSize.Location = New System.Drawing.Point(9, 148)
    Me.lblVideoSize.Name = "lblVideoSize"
    Me.lblVideoSize.Size = New System.Drawing.Size(48, 13)
    Me.lblVideoSize.TabIndex = 12
    Me.lblVideoSize.Text = "Vid Size:"
    '
    'txtVideoSize
    '
    Me.txtVideoSize.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtVideoSize.BackColor = System.Drawing.SystemColors.Window
    Me.txtVideoSize.BorderStyle = System.Windows.Forms.BorderStyle.None
    Me.txtVideoSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
    Me.txtVideoSize.Location = New System.Drawing.Point(63, 148)
    Me.txtVideoSize.Name = "txtVideoSize"
    Me.txtVideoSize.ReadOnly = True
    Me.txtVideoSize.Size = New System.Drawing.Size(256, 13)
    Me.txtVideoSize.TabIndex = 13
    Me.txtVideoSize.Text = "No video"
    '
    'lblCreated
    '
    Me.lblCreated.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.lblCreated.AutoSize = True
    Me.lblCreated.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
    Me.lblCreated.Location = New System.Drawing.Point(10, 178)
    Me.lblCreated.Name = "lblCreated"
    Me.lblCreated.Size = New System.Drawing.Size(47, 13)
    Me.lblCreated.TabIndex = 14
    Me.lblCreated.Text = "Created:"
    '
    'txtCreated
    '
    Me.txtCreated.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtCreated.BackColor = System.Drawing.SystemColors.Window
    Me.txtCreated.BorderStyle = System.Windows.Forms.BorderStyle.None
    Me.txtCreated.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
    Me.txtCreated.Location = New System.Drawing.Point(63, 178)
    Me.txtCreated.Name = "txtCreated"
    Me.txtCreated.ReadOnly = True
    Me.txtCreated.Size = New System.Drawing.Size(256, 13)
    Me.txtCreated.TabIndex = 15
    Me.txtCreated.Text = "MM/DD/YY HH:MM:SS AP"
    '
    'tabAdvanced
    '
    Me.tabAdvanced.Controls.Add(Me.pnlAdvanced)
    Me.tabAdvanced.Location = New System.Drawing.Point(4, 27)
    Me.tabAdvanced.Name = "tabAdvanced"
    Me.tabAdvanced.Size = New System.Drawing.Size(322, 365)
    Me.tabAdvanced.TabIndex = 2
    Me.tabAdvanced.Text = "Advanced"
    Me.tabAdvanced.UseVisualStyleBackColor = True
    '
    'pnlAdvanced
    '
    Me.pnlAdvanced.ColumnCount = 2
    Me.pnlAdvanced.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60.0!))
    Me.pnlAdvanced.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlAdvanced.Controls.Add(Me.pctAdvancedIcon, 0, 0)
    Me.pnlAdvanced.Controls.Add(Me.txtAdvancedFileName, 1, 0)
    Me.pnlAdvanced.Controls.Add(Me.lblFilters, 0, 1)
    Me.pnlAdvanced.Controls.Add(Me.pnlFilters, 1, 1)
    Me.pnlAdvanced.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlAdvanced.Location = New System.Drawing.Point(0, 0)
    Me.pnlAdvanced.Name = "pnlAdvanced"
    Me.pnlAdvanced.RowCount = 3
    Me.pnlAdvanced.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50.0!))
    Me.pnlAdvanced.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.pnlAdvanced.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.pnlAdvanced.Size = New System.Drawing.Size(322, 365)
    Me.pnlAdvanced.TabIndex = 1
    '
    'pctAdvancedIcon
    '
    Me.pctAdvancedIcon.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.pctAdvancedIcon.Location = New System.Drawing.Point(6, 1)
    Me.pctAdvancedIcon.Margin = New System.Windows.Forms.Padding(0)
    Me.pctAdvancedIcon.Name = "pctAdvancedIcon"
    Me.pctAdvancedIcon.Size = New System.Drawing.Size(48, 48)
    Me.pctAdvancedIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
    Me.pctAdvancedIcon.TabIndex = 0
    Me.pctAdvancedIcon.TabStop = False
    '
    'txtAdvancedFileName
    '
    Me.txtAdvancedFileName.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtAdvancedFileName.BackColor = System.Drawing.SystemColors.Window
    Me.txtAdvancedFileName.BorderStyle = System.Windows.Forms.BorderStyle.None
    Me.txtAdvancedFileName.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!)
    Me.txtAdvancedFileName.Location = New System.Drawing.Point(63, 17)
    Me.txtAdvancedFileName.Name = "txtAdvancedFileName"
    Me.txtAdvancedFileName.ReadOnly = True
    Me.txtAdvancedFileName.Size = New System.Drawing.Size(256, 16)
    Me.txtAdvancedFileName.TabIndex = 4
    Me.txtAdvancedFileName.Text = "00 File Name.ext"
    '
    'lblFilters
    '
    Me.lblFilters.AutoSize = True
    Me.lblFilters.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
    Me.lblFilters.Location = New System.Drawing.Point(3, 66)
    Me.lblFilters.Margin = New System.Windows.Forms.Padding(3, 16, 3, 0)
    Me.lblFilters.Name = "lblFilters"
    Me.lblFilters.Size = New System.Drawing.Size(37, 13)
    Me.lblFilters.TabIndex = 5
    Me.lblFilters.Text = "Filters:"
    '
    'pnlFilters
    '
    Me.pnlFilters.ColumnCount = 1
    Me.pnlFilters.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlFilters.Controls.Add(Me.lstFilters, 0, 0)
    Me.pnlFilters.Controls.Add(Me.cmdFilterProps, 0, 1)
    Me.pnlFilters.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlFilters.Location = New System.Drawing.Point(63, 53)
    Me.pnlFilters.Name = "pnlFilters"
    Me.pnlFilters.RowCount = 2
    Me.pnlFilters.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.pnlFilters.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlFilters.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
    Me.pnlFilters.Size = New System.Drawing.Size(256, 151)
    Me.pnlFilters.TabIndex = 6
    '
    'lstFilters
    '
    Me.lstFilters.Dock = System.Windows.Forms.DockStyle.Fill
    Me.lstFilters.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!)
    Me.lstFilters.FormattingEnabled = True
    Me.lstFilters.ItemHeight = 15
    Me.lstFilters.Location = New System.Drawing.Point(8, 12)
    Me.lstFilters.Margin = New System.Windows.Forms.Padding(8, 12, 8, 8)
    Me.lstFilters.Name = "lstFilters"
    Me.lstFilters.Size = New System.Drawing.Size(240, 98)
    Me.lstFilters.TabIndex = 0
    '
    'cmdFilterProps
    '
    Me.cmdFilterProps.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdFilterProps.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.cmdFilterProps.Location = New System.Drawing.Point(3, 121)
    Me.cmdFilterProps.Name = "cmdFilterProps"
    Me.cmdFilterProps.Size = New System.Drawing.Size(100, 27)
    Me.cmdFilterProps.TabIndex = 1
    Me.cmdFilterProps.Text = "P&roperties"
    Me.cmdFilterProps.UseVisualStyleBackColor = True
    '
    'cmdOK
    '
    Me.cmdOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdOK.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.cmdOK.Location = New System.Drawing.Point(263, 416)
    Me.cmdOK.Name = "cmdOK"
    Me.cmdOK.Size = New System.Drawing.Size(75, 25)
    Me.cmdOK.TabIndex = 3
    Me.cmdOK.Text = "OK"
    Me.cmdOK.UseVisualStyleBackColor = True
    '
    'tmrGenPreview
    '
    Me.tmrGenPreview.Interval = 400
    '
    'colID3v2Tag
    '
    Me.colID3v2Tag.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
    Me.colID3v2Tag.FillWeight = 33.0!
    Me.colID3v2Tag.HeaderText = "Tag"
    Me.colID3v2Tag.Name = "colID3v2Tag"
    Me.colID3v2Tag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
    Me.colID3v2Tag.Width = 34
    '
    'colID3v2Value
    '
    Me.colID3v2Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
    Me.colID3v2Value.FillWeight = 66.0!
    Me.colID3v2Value.HeaderText = "Value"
    Me.colID3v2Value.Name = "colID3v2Value"
    Me.colID3v2Value.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
    '
    'colID3v1Tag
    '
    Me.colID3v1Tag.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
    Me.colID3v1Tag.FillWeight = 33.0!
    Me.colID3v1Tag.HeaderText = "Tag"
    Me.colID3v1Tag.Name = "colID3v1Tag"
    Me.colID3v1Tag.ReadOnly = True
    Me.colID3v1Tag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
    Me.colID3v1Tag.Width = 34
    '
    'colID3v1Value
    '
    Me.colID3v1Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
    Me.colID3v1Value.FillWeight = 66.0!
    Me.colID3v1Value.HeaderText = "Value"
    Me.colID3v1Value.Name = "colID3v1Value"
    Me.colID3v1Value.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
    '
    'colVorbisTag
    '
    Me.colVorbisTag.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
    Me.colVorbisTag.FillWeight = 33.0!
    Me.colVorbisTag.HeaderText = "Tag"
    Me.colVorbisTag.Name = "colVorbisTag"
    Me.colVorbisTag.ReadOnly = True
    Me.colVorbisTag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
    Me.colVorbisTag.Width = 34
    '
    'colVorbisValue
    '
    Me.colVorbisValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
    Me.colVorbisValue.FillWeight = 66.0!
    Me.colVorbisValue.HeaderText = "Value"
    Me.colVorbisValue.Name = "colVorbisValue"
    Me.colVorbisValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
    '
    'colMPEGTag
    '
    Me.colMPEGTag.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
    Me.colMPEGTag.FillWeight = 33.0!
    Me.colMPEGTag.HeaderText = "Tag"
    Me.colMPEGTag.Name = "colMPEGTag"
    Me.colMPEGTag.ReadOnly = True
    Me.colMPEGTag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
    Me.colMPEGTag.Width = 34
    '
    'colMPEGValue
    '
    Me.colMPEGValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
    Me.colMPEGValue.FillWeight = 66.0!
    Me.colMPEGValue.HeaderText = "Value"
    Me.colMPEGValue.Name = "colMPEGValue"
    Me.colMPEGValue.ReadOnly = True
    Me.colMPEGValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
    '
    'frmProps
    '
    Me.AcceptButton = Me.cmdOK
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(354, 452)
    Me.Controls.Add(Me.cmdOK)
    Me.Controls.Add(Me.tabProps)
    Me.MaximizeBox = False
    Me.MinimizeBox = False
    Me.MinimumSize = New System.Drawing.Size(370, 490)
    Me.Name = "frmProps"
    Me.ShowIcon = False
    Me.ShowInTaskbar = False
    Me.Text = "Properties"
    Me.tabProps.ResumeLayout(False)
    Me.tabClip.ResumeLayout(False)
    Me.pnlClip.ResumeLayout(False)
    Me.pnlClip.PerformLayout()
    CType(Me.pctClipIcon, System.ComponentModel.ISupportInitialize).EndInit()
    Me.grpMetadata.ResumeLayout(False)
    Me.tbsTags.ResumeLayout(False)
    Me.tabID3v1.ResumeLayout(False)
    Me.pnlID3v1.ResumeLayout(False)
    CType(Me.dgvID3v1, System.ComponentModel.ISupportInitialize).EndInit()
    Me.tabID3v2.ResumeLayout(False)
    Me.pnlID3v2.ResumeLayout(False)
    CType(Me.dgvID3v2, System.ComponentModel.ISupportInitialize).EndInit()
    Me.tabVorbis.ResumeLayout(False)
    CType(Me.dgvVorbis, System.ComponentModel.ISupportInitialize).EndInit()
    Me.tabMPEG.ResumeLayout(False)
    CType(Me.dgvMPEG, System.ComponentModel.ISupportInitialize).EndInit()
    Me.tabMKV.ResumeLayout(False)
    Me.tabMIDI.ResumeLayout(False)
    Me.tabRIFF.ResumeLayout(False)
    Me.tabDetails.ResumeLayout(False)
    Me.pnlDetails.ResumeLayout(False)
    Me.pnlDetails.PerformLayout()
    CType(Me.pctDetailsIcon, System.ComponentModel.ISupportInitialize).EndInit()
    CType(Me.pctPreview, System.ComponentModel.ISupportInitialize).EndInit()
    Me.tabAdvanced.ResumeLayout(False)
    Me.pnlAdvanced.ResumeLayout(False)
    Me.pnlAdvanced.PerformLayout()
    CType(Me.pctAdvancedIcon, System.ComponentModel.ISupportInitialize).EndInit()
    Me.pnlFilters.ResumeLayout(False)
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents tabProps As System.Windows.Forms.TabControl
  Friend WithEvents tabClip As System.Windows.Forms.TabPage
  Friend WithEvents tabDetails As System.Windows.Forms.TabPage
  Friend WithEvents tabAdvanced As System.Windows.Forms.TabPage
  Friend WithEvents cmdOK As System.Windows.Forms.Button
  Friend WithEvents pnlClip As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents pctClipIcon As System.Windows.Forms.PictureBox
  Friend WithEvents grpMetadata As System.Windows.Forms.GroupBox
  Friend WithEvents lblLocation As System.Windows.Forms.Label
  Friend WithEvents txtClipFileName As System.Windows.Forms.TextBox
  Friend WithEvents txtFilePath As System.Windows.Forms.TextBox
  Friend WithEvents tbsTags As System.Windows.Forms.TabControl
  Friend WithEvents tabID3v1 As System.Windows.Forms.TabPage
  Friend WithEvents tabID3v2 As System.Windows.Forms.TabPage
  Friend WithEvents tabVorbis As System.Windows.Forms.TabPage
  Friend WithEvents tabMPEG As System.Windows.Forms.TabPage
  Friend WithEvents pnlDetails As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents pctDetailsIcon As System.Windows.Forms.PictureBox
  Friend WithEvents txtDetailsFileName As System.Windows.Forms.TextBox
  Friend WithEvents pctPreview As System.Windows.Forms.PictureBox
  Friend WithEvents lblType As System.Windows.Forms.Label
  Friend WithEvents txtType As System.Windows.Forms.TextBox
  Friend WithEvents lblSize As System.Windows.Forms.Label
  Friend WithEvents txtSize As System.Windows.Forms.TextBox
  Friend WithEvents lblLength As System.Windows.Forms.Label
  Friend WithEvents txtLength As System.Windows.Forms.TextBox
  Friend WithEvents lblVideoSize As System.Windows.Forms.Label
  Friend WithEvents txtVideoSize As System.Windows.Forms.TextBox
  Friend WithEvents lblCreated As System.Windows.Forms.Label
  Friend WithEvents txtCreated As System.Windows.Forms.TextBox
  Friend WithEvents pnlAdvanced As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents pctAdvancedIcon As System.Windows.Forms.PictureBox
  Friend WithEvents txtAdvancedFileName As System.Windows.Forms.TextBox
  Friend WithEvents lblFilters As System.Windows.Forms.Label
  Friend WithEvents pnlFilters As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents lstFilters As System.Windows.Forms.ListBox
  Friend WithEvents cmdFilterProps As System.Windows.Forms.Button
  Friend WithEvents dgvID3v2 As System.Windows.Forms.DataGridView
  Friend WithEvents dgvVorbis As System.Windows.Forms.DataGridView
  Friend WithEvents dgvMPEG As System.Windows.Forms.DataGridView
  Friend WithEvents tabMKV As System.Windows.Forms.TabPage
  Friend WithEvents tvMKV As System.Windows.Forms.TreeView
  Friend WithEvents tabMIDI As System.Windows.Forms.TabPage
  Friend WithEvents tvMIDI As System.Windows.Forms.TreeView
  Friend WithEvents pnlID3v1 As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents dgvID3v1 As System.Windows.Forms.DataGridView
  Friend WithEvents cmdID3v1Reset As System.Windows.Forms.Button
  Friend WithEvents cmdID3v1Save As System.Windows.Forms.Button
  Friend WithEvents tabRIFF As System.Windows.Forms.TabPage
  Friend WithEvents tvRIFF As System.Windows.Forms.TreeView
  Friend WithEvents tmrGenPreview As System.Windows.Forms.Timer
  Friend WithEvents pnlID3v2 As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents cmdID3v2Reset As System.Windows.Forms.Button
  Friend WithEvents cmdID3v2Save As System.Windows.Forms.Button
  Friend WithEvents cmdID3v2Add As System.Windows.Forms.Button
  Friend WithEvents cmdID3v2Remove As System.Windows.Forms.Button
  Friend WithEvents colID3v2Tag As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colID3v2Value As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colID3v1Tag As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colID3v1Value As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colVorbisTag As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colVorbisValue As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colMPEGTag As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colMPEGValue As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
