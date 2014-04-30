<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmNewID3v2Tag
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
    Me.pnlCreator = New System.Windows.Forms.TableLayoutPanel()
    Me.lblType = New System.Windows.Forms.Label()
    Me.cmbType = New System.Windows.Forms.ComboBox()
    Me.txtOtherType = New System.Windows.Forms.TextBox()
    Me.lblAttribute = New System.Windows.Forms.Label()
    Me.pnlText = New System.Windows.Forms.TableLayoutPanel()
    Me.chkReadOnly = New System.Windows.Forms.CheckBox()
    Me.chkTagAlter = New System.Windows.Forms.CheckBox()
    Me.chkGroup = New System.Windows.Forms.CheckBox()
    Me.txtGroup = New System.Windows.Forms.NumericUpDown()
    Me.chkFileAlter = New System.Windows.Forms.CheckBox()
    Me.chkCompress = New System.Windows.Forms.CheckBox()
    Me.chkEncrypt = New System.Windows.Forms.CheckBox()
    Me.pnlButtons = New System.Windows.Forms.TableLayoutPanel()
    Me.cmdCancel = New System.Windows.Forms.Button()
    Me.cmdAdd = New System.Windows.Forms.Button()
    Me.lblValue = New System.Windows.Forms.Label()
    Me.pnlEncryption = New System.Windows.Forms.TableLayoutPanel()
    Me.lblEncryptionOwner = New System.Windows.Forms.Label()
    Me.txtEncryptionOwner = New System.Windows.Forms.TextBox()
    Me.lblEncryptionStart = New System.Windows.Forms.Label()
    Me.txtEncryptionStart = New System.Windows.Forms.NumericUpDown()
    Me.lblEncryptionCount = New System.Windows.Forms.Label()
    Me.txtEncryptionCount = New System.Windows.Forms.NumericUpDown()
    Me.lblEncryptionInfo = New System.Windows.Forms.Label()
    Me.txtEncryptionInfo = New System.Windows.Forms.TextBox()
    Me.pnlHex = New System.Windows.Forms.TableLayoutPanel()
    Me.txtHexValue = New System.Windows.Forms.TextBox()
    Me.pnlDescribedValue = New System.Windows.Forms.TableLayoutPanel()
    Me.txtDescribedValue = New System.Windows.Forms.TextBox()
    Me.lblDescription = New System.Windows.Forms.Label()
    Me.txtDescription = New System.Windows.Forms.TextBox()
    Me.lblDescribedEncoding = New System.Windows.Forms.Label()
    Me.cmbDescribedEncoding = New System.Windows.Forms.ComboBox()
    Me.pnlTextValue = New System.Windows.Forms.TableLayoutPanel()
    Me.txtTextValue = New System.Windows.Forms.TextBox()
    Me.lblTextEncoding = New System.Windows.Forms.Label()
    Me.cmbTextEncoding = New System.Windows.Forms.ComboBox()
    Me.pnlCreator.SuspendLayout
    Me.pnlText.SuspendLayout
    CType(Me.txtGroup,System.ComponentModel.ISupportInitialize).BeginInit
    Me.pnlButtons.SuspendLayout
    Me.pnlEncryption.SuspendLayout
    CType(Me.txtEncryptionStart,System.ComponentModel.ISupportInitialize).BeginInit
    CType(Me.txtEncryptionCount,System.ComponentModel.ISupportInitialize).BeginInit
    Me.pnlHex.SuspendLayout
    Me.pnlDescribedValue.SuspendLayout
    Me.pnlTextValue.SuspendLayout
    Me.SuspendLayout
    '
    'pnlCreator
    '
    Me.pnlCreator.ColumnCount = 2
    Me.pnlCreator.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlCreator.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100!))
    Me.pnlCreator.Controls.Add(Me.lblType, 0, 0)
    Me.pnlCreator.Controls.Add(Me.cmbType, 1, 0)
    Me.pnlCreator.Controls.Add(Me.txtOtherType, 1, 1)
    Me.pnlCreator.Controls.Add(Me.lblAttribute, 0, 2)
    Me.pnlCreator.Controls.Add(Me.pnlText, 1, 2)
    Me.pnlCreator.Controls.Add(Me.pnlButtons, 1, 4)
    Me.pnlCreator.Controls.Add(Me.lblValue, 0, 3)
    Me.pnlCreator.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlCreator.Location = New System.Drawing.Point(0, 0)
    Me.pnlCreator.Name = "pnlCreator"
    Me.pnlCreator.RowCount = 5
    Me.pnlCreator.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlCreator.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlCreator.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75!))
    Me.pnlCreator.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100!))
    Me.pnlCreator.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlCreator.Size = New System.Drawing.Size(438, 287)
    Me.pnlCreator.TabIndex = 0
    '
    'lblType
    '
    Me.lblType.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblType.AutoSize = true
    Me.lblType.Location = New System.Drawing.Point(3, 7)
    Me.lblType.Name = "lblType"
    Me.lblType.Size = New System.Drawing.Size(56, 13)
    Me.lblType.TabIndex = 0
    Me.lblType.Text = "Tag Type:"
    '
    'cmbType
    '
    Me.cmbType.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbType.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmbType.FormattingEnabled = true
    Me.cmbType.Location = New System.Drawing.Point(68, 3)
    Me.cmbType.Name = "cmbType"
    Me.cmbType.Size = New System.Drawing.Size(367, 21)
    Me.cmbType.TabIndex = 1
    '
    'txtOtherType
    '
    Me.txtOtherType.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.txtOtherType.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
    Me.txtOtherType.Enabled = false
    Me.txtOtherType.Location = New System.Drawing.Point(68, 30)
    Me.txtOtherType.Name = "txtOtherType"
    Me.txtOtherType.Size = New System.Drawing.Size(367, 20)
    Me.txtOtherType.TabIndex = 2
    '
    'lblAttribute
    '
    Me.lblAttribute.AutoSize = true
    Me.lblAttribute.Location = New System.Drawing.Point(3, 53)
    Me.lblAttribute.Name = "lblAttribute"
    Me.lblAttribute.Size = New System.Drawing.Size(54, 13)
    Me.lblAttribute.TabIndex = 3
    Me.lblAttribute.Text = "Attributes:"
    '
    'pnlText
    '
    Me.pnlText.ColumnCount = 3
    Me.pnlText.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlText.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30!))
    Me.pnlText.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20!))
    Me.pnlText.Controls.Add(Me.chkReadOnly, 0, 2)
    Me.pnlText.Controls.Add(Me.chkTagAlter, 0, 0)
    Me.pnlText.Controls.Add(Me.chkGroup, 1, 2)
    Me.pnlText.Controls.Add(Me.txtGroup, 2, 2)
    Me.pnlText.Controls.Add(Me.chkFileAlter, 0, 1)
    Me.pnlText.Controls.Add(Me.chkCompress, 1, 0)
    Me.pnlText.Controls.Add(Me.chkEncrypt, 1, 1)
    Me.pnlText.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlText.Location = New System.Drawing.Point(65, 53)
    Me.pnlText.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlText.Name = "pnlText"
    Me.pnlText.RowCount = 3
    Me.pnlText.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333!))
    Me.pnlText.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333!))
    Me.pnlText.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333!))
    Me.pnlText.Size = New System.Drawing.Size(373, 75)
    Me.pnlText.TabIndex = 4
    '
    'chkReadOnly
    '
    Me.chkReadOnly.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.chkReadOnly.AutoSize = true
    Me.chkReadOnly.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.chkReadOnly.Location = New System.Drawing.Point(3, 53)
    Me.chkReadOnly.Name = "chkReadOnly"
    Me.chkReadOnly.Size = New System.Drawing.Size(82, 18)
    Me.chkReadOnly.TabIndex = 2
    Me.chkReadOnly.Text = "Read Only"
    Me.chkReadOnly.UseVisualStyleBackColor = true
    '
    'chkTagAlter
    '
    Me.chkTagAlter.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.chkTagAlter.AutoSize = true
    Me.chkTagAlter.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.chkTagAlter.Location = New System.Drawing.Point(3, 3)
    Me.chkTagAlter.Name = "chkTagAlter"
    Me.chkTagAlter.Size = New System.Drawing.Size(129, 18)
    Me.chkTagAlter.TabIndex = 0
    Me.chkTagAlter.Text = "Discard on Tag Alter"
    Me.chkTagAlter.UseVisualStyleBackColor = true
    '
    'chkGroup
    '
    Me.chkGroup.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.chkGroup.AutoSize = true
    Me.chkGroup.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.chkGroup.Location = New System.Drawing.Point(189, 53)
    Me.chkGroup.Name = "chkGroup"
    Me.chkGroup.Size = New System.Drawing.Size(101, 18)
    Me.chkGroup.TabIndex = 5
    Me.chkGroup.Text = "Group Identity:"
    Me.chkGroup.UseVisualStyleBackColor = true
    '
    'txtGroup
    '
    Me.txtGroup.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.txtGroup.Hexadecimal = true
    Me.txtGroup.Location = New System.Drawing.Point(319, 53)
    Me.txtGroup.Maximum = New Decimal(New Integer() {255, 0, 0, 0})
    Me.txtGroup.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
    Me.txtGroup.Name = "txtGroup"
    Me.txtGroup.Size = New System.Drawing.Size(51, 20)
    Me.txtGroup.TabIndex = 6
    Me.txtGroup.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
    Me.txtGroup.Value = New Decimal(New Integer() {1, 0, 0, 0})
    '
    'chkFileAlter
    '
    Me.chkFileAlter.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.chkFileAlter.AutoSize = true
    Me.chkFileAlter.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.chkFileAlter.Location = New System.Drawing.Point(3, 28)
    Me.chkFileAlter.Name = "chkFileAlter"
    Me.chkFileAlter.Size = New System.Drawing.Size(126, 18)
    Me.chkFileAlter.TabIndex = 1
    Me.chkFileAlter.Text = "Discard on File Alter"
    Me.chkFileAlter.UseVisualStyleBackColor = true
    '
    'chkCompress
    '
    Me.chkCompress.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.chkCompress.AutoSize = true
    Me.pnlText.SetColumnSpan(Me.chkCompress, 2)
    Me.chkCompress.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.chkCompress.Location = New System.Drawing.Point(189, 3)
    Me.chkCompress.Name = "chkCompress"
    Me.chkCompress.Size = New System.Drawing.Size(90, 18)
    Me.chkCompress.TabIndex = 3
    Me.chkCompress.Text = "Compressed"
    Me.chkCompress.UseVisualStyleBackColor = true
    '
    'chkEncrypt
    '
    Me.chkEncrypt.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.chkEncrypt.AutoSize = true
    Me.pnlText.SetColumnSpan(Me.chkEncrypt, 2)
    Me.chkEncrypt.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.chkEncrypt.Location = New System.Drawing.Point(189, 28)
    Me.chkEncrypt.Name = "chkEncrypt"
    Me.chkEncrypt.Size = New System.Drawing.Size(80, 18)
    Me.chkEncrypt.TabIndex = 4
    Me.chkEncrypt.Text = "Encrypted"
    Me.chkEncrypt.UseVisualStyleBackColor = true
    '
    'pnlButtons
    '
    Me.pnlButtons.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.pnlButtons.AutoSize = true
    Me.pnlButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.pnlButtons.ColumnCount = 2
    Me.pnlButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlButtons.Controls.Add(Me.cmdCancel, 1, 0)
    Me.pnlButtons.Controls.Add(Me.cmdAdd, 0, 0)
    Me.pnlButtons.Location = New System.Drawing.Point(276, 256)
    Me.pnlButtons.Margin = New System.Windows.Forms.Padding(0)
    Me.pnlButtons.Name = "pnlButtons"
    Me.pnlButtons.RowCount = 1
    Me.pnlButtons.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlButtons.Size = New System.Drawing.Size(162, 31)
    Me.pnlButtons.TabIndex = 5
    '
    'cmdCancel
    '
    Me.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdCancel.Location = New System.Drawing.Point(84, 3)
    Me.cmdCancel.Name = "cmdCancel"
    Me.cmdCancel.Size = New System.Drawing.Size(75, 25)
    Me.cmdCancel.TabIndex = 1
    Me.cmdCancel.Text = "Cancel"
    Me.cmdCancel.UseVisualStyleBackColor = true
    '
    'cmdAdd
    '
    Me.cmdAdd.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.cmdAdd.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmdAdd.Location = New System.Drawing.Point(3, 3)
    Me.cmdAdd.Name = "cmdAdd"
    Me.cmdAdd.Size = New System.Drawing.Size(75, 25)
    Me.cmdAdd.TabIndex = 0
    Me.cmdAdd.Text = "Add Tag"
    Me.cmdAdd.UseVisualStyleBackColor = true
    '
    'lblValue
    '
    Me.lblValue.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblValue.AutoSize = true
    Me.lblValue.Location = New System.Drawing.Point(3, 185)
    Me.lblValue.Name = "lblValue"
    Me.lblValue.Size = New System.Drawing.Size(59, 13)
    Me.lblValue.TabIndex = 6
    Me.lblValue.Text = "Tag Value:"
    '
    'pnlEncryption
    '
    Me.pnlEncryption.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.pnlEncryption.AutoSize = true
    Me.pnlEncryption.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.pnlEncryption.ColumnCount = 4
    Me.pnlEncryption.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlEncryption.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlEncryption.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlEncryption.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlEncryption.Controls.Add(Me.lblEncryptionOwner, 0, 0)
    Me.pnlEncryption.Controls.Add(Me.txtEncryptionOwner, 1, 0)
    Me.pnlEncryption.Controls.Add(Me.lblEncryptionStart, 0, 1)
    Me.pnlEncryption.Controls.Add(Me.txtEncryptionStart, 1, 1)
    Me.pnlEncryption.Controls.Add(Me.lblEncryptionCount, 2, 1)
    Me.pnlEncryption.Controls.Add(Me.txtEncryptionCount, 3, 1)
    Me.pnlEncryption.Controls.Add(Me.lblEncryptionInfo, 0, 2)
    Me.pnlEncryption.Controls.Add(Me.txtEncryptionInfo, 1, 2)
    Me.pnlEncryption.Location = New System.Drawing.Point(68, 138)
    Me.pnlEncryption.Name = "pnlEncryption"
    Me.pnlEncryption.RowCount = 3
    Me.pnlEncryption.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlEncryption.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlEncryption.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.pnlEncryption.Size = New System.Drawing.Size(367, 108)
    Me.pnlEncryption.TabIndex = 7
    '
    'lblEncryptionOwner
    '
    Me.lblEncryptionOwner.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblEncryptionOwner.AutoSize = true
    Me.lblEncryptionOwner.Location = New System.Drawing.Point(3, 6)
    Me.lblEncryptionOwner.Name = "lblEncryptionOwner"
    Me.lblEncryptionOwner.Size = New System.Drawing.Size(84, 13)
    Me.lblEncryptionOwner.TabIndex = 0
    Me.lblEncryptionOwner.Text = "Owner Identifier:"
    '
    'txtEncryptionOwner
    '
    Me.txtEncryptionOwner.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.pnlEncryption.SetColumnSpan(Me.txtEncryptionOwner, 3)
    Me.txtEncryptionOwner.Location = New System.Drawing.Point(114, 3)
    Me.txtEncryptionOwner.Name = "txtEncryptionOwner"
    Me.txtEncryptionOwner.Size = New System.Drawing.Size(250, 20)
    Me.txtEncryptionOwner.TabIndex = 1
    '
    'lblEncryptionStart
    '
    Me.lblEncryptionStart.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblEncryptionStart.AutoSize = true
    Me.lblEncryptionStart.Location = New System.Drawing.Point(3, 32)
    Me.lblEncryptionStart.Name = "lblEncryptionStart"
    Me.lblEncryptionStart.Size = New System.Drawing.Size(105, 13)
    Me.lblEncryptionStart.TabIndex = 2
    Me.lblEncryptionStart.Text = "Preview Start Frame:"
    '
    'txtEncryptionStart
    '
    Me.txtEncryptionStart.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.txtEncryptionStart.Location = New System.Drawing.Point(114, 29)
    Me.txtEncryptionStart.Maximum = New Decimal(New Integer() {65535, 0, 0, 0})
    Me.txtEncryptionStart.Name = "txtEncryptionStart"
    Me.txtEncryptionStart.Size = New System.Drawing.Size(60, 20)
    Me.txtEncryptionStart.TabIndex = 3
    Me.txtEncryptionStart.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
    '
    'lblEncryptionCount
    '
    Me.lblEncryptionCount.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblEncryptionCount.AutoSize = true
    Me.lblEncryptionCount.Location = New System.Drawing.Point(183, 32)
    Me.lblEncryptionCount.Name = "lblEncryptionCount"
    Me.lblEncryptionCount.Size = New System.Drawing.Size(111, 13)
    Me.lblEncryptionCount.TabIndex = 4
    Me.lblEncryptionCount.Text = "Preview Frame Count:"
    '
    'txtEncryptionCount
    '
    Me.txtEncryptionCount.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.txtEncryptionCount.Location = New System.Drawing.Point(300, 29)
    Me.txtEncryptionCount.Maximum = New Decimal(New Integer() {65535, 0, 0, 0})
    Me.txtEncryptionCount.Name = "txtEncryptionCount"
    Me.txtEncryptionCount.Size = New System.Drawing.Size(60, 20)
    Me.txtEncryptionCount.TabIndex = 5
    Me.txtEncryptionCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
    '
    'lblEncryptionInfo
    '
    Me.lblEncryptionInfo.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblEncryptionInfo.AutoSize = true
    Me.lblEncryptionInfo.Location = New System.Drawing.Point(3, 73)
    Me.lblEncryptionInfo.Name = "lblEncryptionInfo"
    Me.lblEncryptionInfo.Size = New System.Drawing.Size(81, 13)
    Me.lblEncryptionInfo.TabIndex = 6
    Me.lblEncryptionInfo.Text = "Encryption Info:"
    '
    'txtEncryptionInfo
    '
    Me.txtEncryptionInfo.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.txtEncryptionInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
    Me.pnlEncryption.SetColumnSpan(Me.txtEncryptionInfo, 3)
    Me.txtEncryptionInfo.Font = New System.Drawing.Font("Lucida Console", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
    Me.txtEncryptionInfo.Location = New System.Drawing.Point(114, 55)
    Me.txtEncryptionInfo.Multiline = true
    Me.txtEncryptionInfo.Name = "txtEncryptionInfo"
    Me.txtEncryptionInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
    Me.txtEncryptionInfo.ShortcutsEnabled = false
    Me.txtEncryptionInfo.Size = New System.Drawing.Size(250, 50)
    Me.txtEncryptionInfo.TabIndex = 7
    '
    'pnlHex
    '
    Me.pnlHex.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.pnlHex.AutoSize = true
    Me.pnlHex.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.pnlHex.ColumnCount = 1
    Me.pnlHex.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlHex.Controls.Add(Me.txtHexValue, 0, 0)
    Me.pnlHex.Location = New System.Drawing.Point(68, 169)
    Me.pnlHex.Name = "pnlHex"
    Me.pnlHex.RowCount = 1
    Me.pnlHex.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlHex.Size = New System.Drawing.Size(367, 46)
    Me.pnlHex.TabIndex = 7
    '
    'txtHexValue
    '
    Me.txtHexValue.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.txtHexValue.Font = New System.Drawing.Font("Lucida Console", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
    Me.txtHexValue.Location = New System.Drawing.Point(3, 3)
    Me.txtHexValue.Multiline = true
    Me.txtHexValue.Name = "txtHexValue"
    Me.txtHexValue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
    Me.txtHexValue.ShortcutsEnabled = false
    Me.txtHexValue.Size = New System.Drawing.Size(361, 40)
    Me.txtHexValue.TabIndex = 0
    '
    'pnlDescribedValue
    '
    Me.pnlDescribedValue.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.pnlDescribedValue.AutoSize = true
    Me.pnlDescribedValue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.pnlDescribedValue.ColumnCount = 4
    Me.pnlDescribedValue.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlDescribedValue.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlDescribedValue.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlDescribedValue.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlDescribedValue.Controls.Add(Me.txtDescribedValue, 0, 0)
    Me.pnlDescribedValue.Controls.Add(Me.lblDescription, 0, 1)
    Me.pnlDescribedValue.Controls.Add(Me.txtDescription, 1, 1)
    Me.pnlDescribedValue.Controls.Add(Me.lblDescribedEncoding, 2, 1)
    Me.pnlDescribedValue.Controls.Add(Me.cmbDescribedEncoding, 3, 1)
    Me.pnlDescribedValue.Location = New System.Drawing.Point(68, 165)
    Me.pnlDescribedValue.Name = "pnlDescribedValue"
    Me.pnlDescribedValue.RowCount = 2
    Me.pnlDescribedValue.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlDescribedValue.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlDescribedValue.Size = New System.Drawing.Size(367, 54)
    Me.pnlDescribedValue.TabIndex = 7
    '
    'txtDescribedValue
    '
    Me.txtDescribedValue.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.pnlDescribedValue.SetColumnSpan(Me.txtDescribedValue, 4)
    Me.txtDescribedValue.Location = New System.Drawing.Point(3, 3)
    Me.txtDescribedValue.Name = "txtDescribedValue"
    Me.txtDescribedValue.Size = New System.Drawing.Size(361, 20)
    Me.txtDescribedValue.TabIndex = 0
    '
    'lblDescription
    '
    Me.lblDescription.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblDescription.AutoSize = true
    Me.lblDescription.Location = New System.Drawing.Point(3, 34)
    Me.lblDescription.Name = "lblDescription"
    Me.lblDescription.Size = New System.Drawing.Size(63, 13)
    Me.lblDescription.TabIndex = 1
    Me.lblDescription.Text = "Description:"
    '
    'txtDescription
    '
    Me.txtDescription.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.txtDescription.Location = New System.Drawing.Point(72, 30)
    Me.txtDescription.Name = "txtDescription"
    Me.txtDescription.Size = New System.Drawing.Size(112, 20)
    Me.txtDescription.TabIndex = 2
    '
    'lblDescribedEncoding
    '
    Me.lblDescribedEncoding.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblDescribedEncoding.AutoSize = true
    Me.lblDescribedEncoding.Location = New System.Drawing.Point(190, 34)
    Me.lblDescribedEncoding.Name = "lblDescribedEncoding"
    Me.lblDescribedEncoding.Size = New System.Drawing.Size(55, 13)
    Me.lblDescribedEncoding.TabIndex = 3
    Me.lblDescribedEncoding.Text = "Encoding:"
    '
    'cmbDescribedEncoding
    '
    Me.cmbDescribedEncoding.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.cmbDescribedEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbDescribedEncoding.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmbDescribedEncoding.FormattingEnabled = true
    Me.cmbDescribedEncoding.Items.AddRange(New Object() {"ISO-8859-1", "UTF-16", "UTF-16 (Big Endian)", "UTF-8"})
    Me.cmbDescribedEncoding.Location = New System.Drawing.Point(251, 30)
    Me.cmbDescribedEncoding.Name = "cmbDescribedEncoding"
    Me.cmbDescribedEncoding.Size = New System.Drawing.Size(113, 21)
    Me.cmbDescribedEncoding.TabIndex = 4
    '
    'pnlTextValue
    '
    Me.pnlTextValue.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.pnlTextValue.AutoSize = true
    Me.pnlTextValue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.pnlTextValue.ColumnCount = 3
    Me.pnlTextValue.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlTextValue.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.pnlTextValue.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50!))
    Me.pnlTextValue.Controls.Add(Me.txtTextValue, 0, 0)
    Me.pnlTextValue.Controls.Add(Me.lblTextEncoding, 1, 0)
    Me.pnlTextValue.Controls.Add(Me.cmbTextEncoding, 2, 0)
    Me.pnlTextValue.Location = New System.Drawing.Point(68, 178)
    Me.pnlTextValue.Name = "pnlTextValue"
    Me.pnlTextValue.RowCount = 1
    Me.pnlTextValue.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100!))
    Me.pnlTextValue.Size = New System.Drawing.Size(367, 27)
    Me.pnlTextValue.TabIndex = 8
    '
    'txtTextValue
    '
    Me.txtTextValue.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.txtTextValue.Location = New System.Drawing.Point(3, 3)
    Me.txtTextValue.Name = "txtTextValue"
    Me.txtTextValue.Size = New System.Drawing.Size(147, 20)
    Me.txtTextValue.TabIndex = 7
    '
    'lblTextEncoding
    '
    Me.lblTextEncoding.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.lblTextEncoding.AutoSize = true
    Me.lblTextEncoding.Location = New System.Drawing.Point(156, 7)
    Me.lblTextEncoding.Name = "lblTextEncoding"
    Me.lblTextEncoding.Size = New System.Drawing.Size(55, 13)
    Me.lblTextEncoding.TabIndex = 8
    Me.lblTextEncoding.Text = "Encoding:"
    '
    'cmbTextEncoding
    '
    Me.cmbTextEncoding.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.cmbTextEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbTextEncoding.FlatStyle = System.Windows.Forms.FlatStyle.System
    Me.cmbTextEncoding.FormattingEnabled = true
    Me.cmbTextEncoding.Items.AddRange(New Object() {"ISO-8859-1", "UTF-16", "UTF-16 (Big Endian)", "UTF-8"})
    Me.cmbTextEncoding.Location = New System.Drawing.Point(217, 3)
    Me.cmbTextEncoding.Name = "cmbTextEncoding"
    Me.cmbTextEncoding.Size = New System.Drawing.Size(147, 21)
    Me.cmbTextEncoding.TabIndex = 9
    '
    'frmNewID3v2Tag
    '
    Me.AcceptButton = Me.cmdAdd
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.CancelButton = Me.cmdCancel
    Me.ClientSize = New System.Drawing.Size(438, 287)
    Me.Controls.Add(Me.pnlCreator)
    Me.MaximizeBox = false
    Me.MinimizeBox = false
    Me.Name = "frmNewID3v2Tag"
    Me.ShowIcon = false
    Me.ShowInTaskbar = false
    Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
    Me.Text = "Create New ID3v2 Tag"
    Me.pnlCreator.ResumeLayout(false)
    Me.pnlCreator.PerformLayout
    Me.pnlText.ResumeLayout(false)
    Me.pnlText.PerformLayout
    CType(Me.txtGroup,System.ComponentModel.ISupportInitialize).EndInit
    Me.pnlButtons.ResumeLayout(false)
    Me.pnlEncryption.ResumeLayout(false)
    Me.pnlEncryption.PerformLayout
    CType(Me.txtEncryptionStart,System.ComponentModel.ISupportInitialize).EndInit
    CType(Me.txtEncryptionCount,System.ComponentModel.ISupportInitialize).EndInit
    Me.pnlHex.ResumeLayout(false)
    Me.pnlHex.PerformLayout
    Me.pnlDescribedValue.ResumeLayout(false)
    Me.pnlDescribedValue.PerformLayout
    Me.pnlTextValue.ResumeLayout(false)
    Me.pnlTextValue.PerformLayout
    Me.ResumeLayout(false)

End Sub
  Friend WithEvents pnlCreator As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents cmdAdd As System.Windows.Forms.Button
  Friend WithEvents cmdCancel As System.Windows.Forms.Button
  Friend WithEvents lblType As System.Windows.Forms.Label
  Friend WithEvents cmbType As System.Windows.Forms.ComboBox
  Friend WithEvents txtOtherType As System.Windows.Forms.TextBox
  Friend WithEvents lblAttribute As System.Windows.Forms.Label
  Friend WithEvents pnlText As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents pnlButtons As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents chkTagAlter As System.Windows.Forms.CheckBox
  Friend WithEvents chkFileAlter As System.Windows.Forms.CheckBox
  Friend WithEvents chkCompress As System.Windows.Forms.CheckBox
  Friend WithEvents chkEncrypt As System.Windows.Forms.CheckBox
  Friend WithEvents chkGroup As System.Windows.Forms.CheckBox
  Friend WithEvents txtGroup As System.Windows.Forms.NumericUpDown
  Friend WithEvents chkReadOnly As System.Windows.Forms.CheckBox
  Friend WithEvents lblValue As System.Windows.Forms.Label
  Friend WithEvents txtTextValue As System.Windows.Forms.TextBox
  Friend WithEvents pnlTextValue As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents lblTextEncoding As System.Windows.Forms.Label
  Friend WithEvents cmbTextEncoding As System.Windows.Forms.ComboBox
  Friend WithEvents pnlDescribedValue As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents txtDescribedValue As System.Windows.Forms.TextBox
  Friend WithEvents lblDescription As System.Windows.Forms.Label
  Friend WithEvents txtDescription As System.Windows.Forms.TextBox
  Friend WithEvents lblDescribedEncoding As System.Windows.Forms.Label
  Friend WithEvents cmbDescribedEncoding As System.Windows.Forms.ComboBox
  Friend WithEvents pnlEncryption As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents lblEncryptionOwner As System.Windows.Forms.Label
  Friend WithEvents txtEncryptionOwner As System.Windows.Forms.TextBox
  Friend WithEvents lblEncryptionStart As System.Windows.Forms.Label
  Friend WithEvents txtEncryptionStart As System.Windows.Forms.NumericUpDown
  Friend WithEvents lblEncryptionCount As System.Windows.Forms.Label
  Friend WithEvents txtEncryptionCount As System.Windows.Forms.NumericUpDown
  Friend WithEvents lblEncryptionInfo As System.Windows.Forms.Label
  Friend WithEvents txtEncryptionInfo As System.Windows.Forms.TextBox
  Friend WithEvents pnlHex As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents txtHexValue As System.Windows.Forms.TextBox
End Class
