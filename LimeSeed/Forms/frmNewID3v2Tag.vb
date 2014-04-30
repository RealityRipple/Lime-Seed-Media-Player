Public Class frmNewID3v2Tag
  Friend Structure ID3FrameList
    Public Name As String
    Public ID As String
    Public Sub New(sName, sID)
      Name = sName
      ID = sID
    End Sub
    Public Sub New(sID)
      Name = Seed.clsID3v2.GetFrameName(sID)
      ID = sID
    End Sub
    Public Overrides Function ToString() As String
      Return Name
    End Function
  End Structure

  Public Sub New()
    InitializeComponent()
    cmbType.BeginUpdate()
    cmbType.Items.Add(New ID3FrameList("BUF"))
    cmbType.Items.Add(New ID3FrameList("CNT"))
    cmbType.Items.Add(New ID3FrameList("COM"))
    cmbType.Items.Add(New ID3FrameList("CRA"))
    cmbType.Items.Add(New ID3FrameList("CRM"))
    cmbType.Items.Add(New ID3FrameList("ETC"))
    cmbType.Items.Add(New ID3FrameList("EQU"))
    cmbType.Items.Add(New ID3FrameList("GEO"))
    cmbType.Items.Add(New ID3FrameList("IPL"))
    cmbType.Items.Add(New ID3FrameList("LNK"))
    cmbType.Items.Add(New ID3FrameList("MCI"))
    cmbType.Items.Add(New ID3FrameList("MLL"))
    cmbType.Items.Add(New ID3FrameList("PIC"))
    cmbType.Items.Add(New ID3FrameList("POP"))
    cmbType.Items.Add(New ID3FrameList("REV"))
    cmbType.Items.Add(New ID3FrameList("RVA"))
    cmbType.Items.Add(New ID3FrameList("SLT"))
    cmbType.Items.Add(New ID3FrameList("STC"))
    cmbType.Items.Add(New ID3FrameList("TAL"))
    cmbType.Items.Add(New ID3FrameList("TBP"))
    cmbType.Items.Add(New ID3FrameList("TCM"))
    cmbType.Items.Add(New ID3FrameList("TCO"))
    cmbType.Items.Add(New ID3FrameList("TCR"))
    cmbType.Items.Add(New ID3FrameList("TDA"))
    cmbType.Items.Add(New ID3FrameList("TDY"))
    cmbType.Items.Add(New ID3FrameList("TEN"))
    cmbType.Items.Add(New ID3FrameList("TFT"))
    cmbType.Items.Add(New ID3FrameList("TIM"))
    cmbType.Items.Add(New ID3FrameList("TKE"))
    cmbType.Items.Add(New ID3FrameList("TLA"))
    cmbType.Items.Add(New ID3FrameList("TLE"))
    cmbType.Items.Add(New ID3FrameList("TMT"))
    cmbType.Items.Add(New ID3FrameList("TOA"))
    cmbType.Items.Add(New ID3FrameList("TOF"))
    cmbType.Items.Add(New ID3FrameList("TOL"))
    cmbType.Items.Add(New ID3FrameList("TOR"))
    cmbType.Items.Add(New ID3FrameList("TOT"))
    cmbType.Items.Add(New ID3FrameList("TP1"))
    cmbType.Items.Add(New ID3FrameList("TP2"))
    cmbType.Items.Add(New ID3FrameList("TP3"))
    cmbType.Items.Add(New ID3FrameList("TP4"))
    cmbType.Items.Add(New ID3FrameList("TPA"))
    cmbType.Items.Add(New ID3FrameList("TPB"))
    cmbType.Items.Add(New ID3FrameList("TRC"))
    cmbType.Items.Add(New ID3FrameList("TRD"))
    cmbType.Items.Add(New ID3FrameList("TRK"))
    cmbType.Items.Add(New ID3FrameList("TSI"))
    cmbType.Items.Add(New ID3FrameList("TSS"))
    cmbType.Items.Add(New ID3FrameList("TT1"))
    cmbType.Items.Add(New ID3FrameList("TT2"))
    cmbType.Items.Add(New ID3FrameList("TT3"))
    cmbType.Items.Add(New ID3FrameList("TXT"))
    cmbType.Items.Add(New ID3FrameList("TXX"))
    cmbType.Items.Add(New ID3FrameList("TYE"))
    cmbType.Items.Add(New ID3FrameList("UFI"))
    cmbType.Items.Add(New ID3FrameList("ULT"))
    cmbType.Items.Add(New ID3FrameList("WAF"))
    cmbType.Items.Add(New ID3FrameList("WAR"))
    cmbType.Items.Add(New ID3FrameList("WAS"))
    cmbType.Items.Add(New ID3FrameList("WCM"))
    cmbType.Items.Add(New ID3FrameList("WCP"))
    cmbType.Items.Add(New ID3FrameList("WPB"))
    cmbType.Items.Add(New ID3FrameList("WXX"))
    cmbType.Items.Add("Custom...")
    cmbType.EndUpdate()
    txtOtherType.Enabled = False
    Me.Text = "Create New ID3v2 Tag"
    cmdAdd.Text = "Add Tag"
  End Sub

  Public Sub New(Tag As String, Flags As UInt16, Optional Group As Byte = 0)
    InitializeComponent()
    cmbType.BeginUpdate()
    cmbType.Items.Add(New ID3FrameList("BUF"))
    cmbType.Items.Add(New ID3FrameList("CNT"))
    cmbType.Items.Add(New ID3FrameList("COM"))
    cmbType.Items.Add(New ID3FrameList("CRA"))
    cmbType.Items.Add(New ID3FrameList("CRM"))
    cmbType.Items.Add(New ID3FrameList("ETC"))
    cmbType.Items.Add(New ID3FrameList("EQU"))
    cmbType.Items.Add(New ID3FrameList("GEO"))
    cmbType.Items.Add(New ID3FrameList("IPL"))
    cmbType.Items.Add(New ID3FrameList("LNK"))
    cmbType.Items.Add(New ID3FrameList("MCI"))
    cmbType.Items.Add(New ID3FrameList("MLL"))
    cmbType.Items.Add(New ID3FrameList("PIC"))
    cmbType.Items.Add(New ID3FrameList("POP"))
    cmbType.Items.Add(New ID3FrameList("REV"))
    cmbType.Items.Add(New ID3FrameList("RVA"))
    cmbType.Items.Add(New ID3FrameList("SLT"))
    cmbType.Items.Add(New ID3FrameList("STC"))
    cmbType.Items.Add(New ID3FrameList("TAL"))
    cmbType.Items.Add(New ID3FrameList("TBP"))
    cmbType.Items.Add(New ID3FrameList("TCM"))
    cmbType.Items.Add(New ID3FrameList("TCO"))
    cmbType.Items.Add(New ID3FrameList("TCR"))
    cmbType.Items.Add(New ID3FrameList("TDA"))
    cmbType.Items.Add(New ID3FrameList("TDY"))
    cmbType.Items.Add(New ID3FrameList("TEN"))
    cmbType.Items.Add(New ID3FrameList("TFT"))
    cmbType.Items.Add(New ID3FrameList("TIM"))
    cmbType.Items.Add(New ID3FrameList("TKE"))
    cmbType.Items.Add(New ID3FrameList("TLA"))
    cmbType.Items.Add(New ID3FrameList("TLE"))
    cmbType.Items.Add(New ID3FrameList("TMT"))
    cmbType.Items.Add(New ID3FrameList("TOA"))
    cmbType.Items.Add(New ID3FrameList("TOF"))
    cmbType.Items.Add(New ID3FrameList("TOL"))
    cmbType.Items.Add(New ID3FrameList("TOR"))
    cmbType.Items.Add(New ID3FrameList("TOT"))
    cmbType.Items.Add(New ID3FrameList("TP1"))
    cmbType.Items.Add(New ID3FrameList("TP2"))
    cmbType.Items.Add(New ID3FrameList("TP3"))
    cmbType.Items.Add(New ID3FrameList("TP4"))
    cmbType.Items.Add(New ID3FrameList("TPA"))
    cmbType.Items.Add(New ID3FrameList("TPB"))
    cmbType.Items.Add(New ID3FrameList("TRC"))
    cmbType.Items.Add(New ID3FrameList("TRD"))
    cmbType.Items.Add(New ID3FrameList("TRK"))
    cmbType.Items.Add(New ID3FrameList("TSI"))
    cmbType.Items.Add(New ID3FrameList("TSS"))
    cmbType.Items.Add(New ID3FrameList("TT1"))
    cmbType.Items.Add(New ID3FrameList("TT2"))
    cmbType.Items.Add(New ID3FrameList("TT3"))
    cmbType.Items.Add(New ID3FrameList("TXT"))
    cmbType.Items.Add(New ID3FrameList("TXX"))
    cmbType.Items.Add(New ID3FrameList("TYE"))
    cmbType.Items.Add(New ID3FrameList("UFI"))
    cmbType.Items.Add(New ID3FrameList("ULT"))
    cmbType.Items.Add(New ID3FrameList("WAF"))
    cmbType.Items.Add(New ID3FrameList("WAR"))
    cmbType.Items.Add(New ID3FrameList("WAS"))
    cmbType.Items.Add(New ID3FrameList("WCM"))
    cmbType.Items.Add(New ID3FrameList("WCP"))
    cmbType.Items.Add(New ID3FrameList("WPB"))
    cmbType.Items.Add(New ID3FrameList("WXX"))
    cmbType.Items.Add("Custom...")
    For I As Integer = 0 To cmbType.Items.Count - 2
      If CType(cmbType.Items(I), ID3FrameList).ID = Tag Then
        cmbType.SelectedIndex = I
        txtOtherType.Enabled = False
      End If
    Next
    If cmbType.SelectedIndex = -1 Then
      cmbType.SelectedIndex = cmbType.Items.Count - 1
      txtOtherType.Enabled = True
      txtOtherType.Text = Tag
    End If
    cmbType.EndUpdate()
    Me.Text = "Modify ID3v2 Tag"
    cmdAdd.Text = "Update"
    chkTagAlter.Checked = (Flags And Seed.clsID3v2.CONSTS.FFLG_TAGALTER)
    chkFileAlter.Checked = (Flags And Seed.clsID3v2.CONSTS.FFLG_FILEALTER)
    chkReadOnly.Checked = (Flags And Seed.clsID3v2.CONSTS.FFLG_READONLY)
    chkCompress.Checked = (Flags And Seed.clsID3v2.CONSTS.FFLG_COMPRESS)
    chkEncrypt.Checked = (Flags And Seed.clsID3v2.CONSTS.FFLG_ENCRYPT)
    chkGroup.Checked = (Flags And Seed.clsID3v2.CONSTS.FFLG_GROUP)
    If Group > 0 Then
      txtGroup.Value = Group
    Else
      txtGroup.Value = 1
    End If
    txtGroup.Enabled = chkGroup.Checked
  End Sub

  Private Sub cmbType_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbType.SelectedIndexChanged
    chkTagAlter.Checked = False
    chkTagAlter.Checked = False
    Dim TypeInfo As ID3FrameList
    If cmbType.SelectedIndex = cmbType.Items.Count - 1 Then
      TypeInfo = New ID3FrameList(txtOtherType.Text)
    Else
      TypeInfo = cmbType.SelectedItem
    End If
    Select Case TypeInfo.ID
      Case "AENC", "CRA", "ETCO", "ETC", "EQUA", "EQU", "EQU2", "MLLT", "MLL", "POSS", "SYLT", "SLT", "SYTC", "STC", "RVAD", "RVA", "RVA2", "TENC", "TEN", "TLEN", "TLE", "TSIZ", "TSI"
        chkFileAlter.Checked = True
    End Select
    Select Case TypeInfo.ID.Substring(0, 1)
      Case "T", "W"
        If TypeInfo.ID.Substring(1, 2) = "XX" Then
          If pnlCreator.Controls.Contains(pnlTextValue) Then pnlCreator.Controls.Remove(pnlTextValue)
          'If pnlCreator.Controls.Contains(pnlFilesValue) Then pnlCreator.Controls.Remove(pnlFilesValue)
          If pnlCreator.Controls.Contains(pnlHex) Then pnlCreator.Controls.Remove(pnlHex)
          pnlCreator.Controls.Add(pnlDescribedValue, 1, 3)
        Else
          If pnlCreator.Controls.Contains(pnlDescribedValue) Then pnlCreator.Controls.Remove(pnlDescribedValue)
          'If pnlCreator.Controls.Contains(pnlFilesValue) Then pnlCreator.Controls.Remove(pnlFilesValue)
          If pnlCreator.Controls.Contains(pnlHex) Then pnlCreator.Controls.Remove(pnlHex)
          pnlCreator.Controls.Add(pnlTextValue, 1, 3)
        End If

    End Select
  End Sub

  Private Sub txtOtherType_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtOtherType.TextChanged
    For I As Integer = 0 To cmbType.Items.Count - 2
      If CType(cmbType.Items(I), ID3FrameList).ID = txtOtherType.Text Then
        cmbType.SelectedIndex = I
        'txtOtherType.Enabled = False
      End If
    Next
  End Sub

  Private Sub UpdateEncryptionInfo()
    Dim iIndex As Integer = txtEncryptionInfo.SelectionStart
    Dim iLen As Integer = txtEncryptionInfo.TextLength
    txtEncryptionInfo.Text = ReformatBytes(txtEncryptionInfo.Text)
    txtEncryptionInfo.SelectionStart = iIndex + (txtEncryptionInfo.TextLength - iLen)
  End Sub

  Private Sub txtEncryptionInfo_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtEncryptionInfo.KeyDown, txtEncryptionInfo.KeyUp
    Select Case e.KeyCode
      Case Keys.D0, Keys.D1, Keys.D1, Keys.NumPad1, Keys.D2, Keys.NumPad2, Keys.D3, Keys.NumPad3, Keys.D4, Keys.NumPad4, Keys.D5, Keys.NumPad5, Keys.D6, Keys.NumPad6, Keys.D7, Keys.NumPad7, Keys.D8, Keys.NumPad8, Keys.D9, Keys.NumPad9, Keys.A, Keys.B, Keys.D, Keys.E, Keys.F
        UpdateEncryptionInfo()
      Case Keys.C
        If e.Modifiers And Keys.Control Then
          'Copy
          If txtEncryptionInfo.SelectionStart Mod 3 = 2 Then
            txtEncryptionInfo.SelectionStart -= 2
            txtEncryptionInfo.SelectionLength += 2
          ElseIf txtEncryptionInfo.SelectionStart Mod 3 = 1 Then
            txtEncryptionInfo.SelectionStart -= 1
            txtEncryptionInfo.SelectionLength += 1
          End If
          If txtEncryptionInfo.SelectionLength Mod 3 = 1 Then
            txtEncryptionInfo.SelectionLength += 1
          ElseIf txtEncryptionInfo.SelectionLength Mod 3 = 0 Then
            txtEncryptionInfo.SelectionLength -= 1
          End If
          Clipboard.SetText(txtEncryptionInfo.SelectedText)
        Else
          'c
          UpdateEncryptionInfo()
        End If
      Case Keys.Back
        UpdateEncryptionInfo()
      Case Keys.Delete
        UpdateEncryptionInfo()
      Case Keys.X
        If e.Modifiers And Keys.Control Then
          'cut

        Else
          e.Handled = True
          e.SuppressKeyPress = True
        End If
      Case Keys.V
        If e.Modifiers And Keys.Control Then
          'paste

        Else
          e.Handled = True
          e.SuppressKeyPress = True
        End If
      Case Keys.Left
        If txtEncryptionInfo.SelectedText = " " Then
          txtEncryptionInfo.Select((txtEncryptionInfo.SelectionStart + txtEncryptionInfo.SelectionLength) - 1, -(txtEncryptionInfo.SelectionLength))
        ElseIf txtEncryptionInfo.SelectedText.StartsWith(" ") Then
          txtEncryptionInfo.Select((txtEncryptionInfo.SelectionStart + txtEncryptionInfo.SelectionLength), -(txtEncryptionInfo.SelectionLength + 1))
        ElseIf txtEncryptionInfo.SelectedText.EndsWith(" ") Then
          txtEncryptionInfo.Select(txtEncryptionInfo.SelectionStart, txtEncryptionInfo.SelectionLength - 1)
        End If
      Case Keys.Right
        If txtEncryptionInfo.SelectedText = " " Then
          txtEncryptionInfo.Select(txtEncryptionInfo.SelectionStart + 1, txtEncryptionInfo.SelectionLength)
        ElseIf txtEncryptionInfo.SelectedText.EndsWith(" ") Then
          txtEncryptionInfo.Select(txtEncryptionInfo.SelectionStart, txtEncryptionInfo.SelectionLength + 1)
        ElseIf txtEncryptionInfo.SelectedText.StartsWith(" ") Then
          txtEncryptionInfo.Select((txtEncryptionInfo.SelectionStart + txtEncryptionInfo.SelectionLength), -(txtEncryptionInfo.SelectionLength - 1))
        End If
      Case Keys.Up, Keys.Down, Keys.Home, Keys.End, Keys.PageUp, Keys.PageDown

      Case Else
        e.Handled = True
        e.SuppressKeyPress = True
    End Select
  End Sub

  Public Function ReformatBytes(sIn As String) As String
    sIn = sIn.Replace(" ", "")
    If sIn.Length Mod 2 = 1 Then
      For I As Integer = sIn.Length - 1 To 2 Step -2
        sIn = sIn.Substring(0, I) & " " & sIn.Substring(I)
      Next
    Else
      For I As Integer = sIn.Length - 2 To 2 Step -2
        sIn = sIn.Substring(0, I) & " " & sIn.Substring(I)
      Next
    End If
    Return sIn
  End Function
End Class