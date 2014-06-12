Public Class ctlArtList
  Private WithEvents sckReader As New Net.WebClient
  Public Event NewArt(row As Generic.Dictionary(Of String, Object))
  Public Event Cancelled()

  Public Sub Display(Artist As String, Album As String, Rows() As Generic.Dictionary(Of String, Object))
    If String.IsNullOrEmpty(Artist) Then
      If String.IsNullOrEmpty(Album) Then
        grpArtwork.Text = "Select Artwork for Unknown Album"
        txtSearch.Text = "Enter Search Terms"
      Else
        grpArtwork.Text = "Select Album Artwork for " & Album.Replace("&", "&&")
        txtSearch.Text = Album
      End If
    ElseIf String.IsNullOrEmpty(Album) Then
      grpArtwork.Text = "Select Artwork for Unknown " & Artist.Replace("&", "&&") & " Album"
      txtSearch.Text = Artist
    Else
      grpArtwork.Text = "Select Album Artwork for " & Artist.Replace("&", "&&") & " - " & Album.Replace("&", "&&")
      txtSearch.Text = Artist & " " & Album
    End If
    pnlArtwork.Controls.Clear()
    If Rows.Count > 4 Then
      pnlProgress.Visible = True
      pbProgress.Value = 0
      pbProgress.Maximum = Rows.Count
      lblProgress.Text = "0%"
    End If
    For Each row In Rows
      AddRow(row)
      If pnlProgress.Visible Then
        pbProgress.Increment(1)
        lblProgress.Text = FormatPercent(pbProgress.Value / pbProgress.Maximum, 0, TriState.False, TriState.False, TriState.False)
        Application.DoEvents()
      End If
    Next
    If pnlProgress.Visible Then
      pnlProgress.Visible = False
    End If
    pnlArtwork.Focus()
    tmrArtwork.Enabled = True
  End Sub

  Public Sub AddRow(RowData As Generic.Dictionary(Of String, Object))
    If Not RowData("artworkUrl100") Is Nothing Then
      Dim pnlTmp As New TableLayoutPanel
      Dim pctTmp As New PictureBox
      Dim lblTmp As New Label
      pnlTmp.ColumnCount = 1
      pnlTmp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
      pnlTmp.RowCount = 2
      pnlTmp.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))
      pnlTmp.RowStyles.Add(New RowStyle(SizeType.AutoSize, 1.0!))
      pnlTmp.Size = New Drawing.Size(106, 139)
      pnlTmp.Margin = New Padding(0)
      pnlTmp.Name = "pnlArt" & Hex(RowData.GetHashCode)
      pnlTmp.Tag = RowData
      pnlTmp.BackColor = Drawing.Color.Transparent
      pctTmp.Tag = GetArtworkURI(RowData, String.Empty)
      pctTmp.Dock = DockStyle.Fill
      Dim imgTmp As Drawing.Image = New Drawing.Bitmap(100, 100)
      Using g As Drawing.Graphics = Drawing.Graphics.FromImage(imgTmp)
        g.Clear(Drawing.Color.White)
        Using limeBrush As New Drawing.Drawing2D.LinearGradientBrush(New Drawing.Rectangle(0, 0, 99, 99), Drawing.Color.White, Drawing.Color.Lime, Drawing.Drawing2D.LinearGradientMode.Vertical)
          g.FillRectangle(limeBrush, New Drawing.Rectangle(0, 0, 99, 99))
        End Using
        g.DrawRectangle(Drawing.Pens.Black, 0, 0, 99, 99)
        g.DrawString("Loading...", New Drawing.Font(Drawing.FontFamily.GenericSansSerif, 14), Drawing.Brushes.Black, New Drawing.RectangleF(1, 1, 98, 98), New Drawing.StringFormat With {.Alignment = Drawing.StringAlignment.Center, .LineAlignment = Drawing.StringAlignment.Center})
      End Using
      pctTmp.Image = imgTmp
      pctTmp.Name = "pctArt" & Hex(RowData.GetHashCode)
      Dim tArtist As String = GetArtist(RowData)
      Dim tAlbum As String = GetAlbum(RowData)
      If String.IsNullOrEmpty(tArtist) Then
        Stop
        tArtist = "Unknown Artist"
      End If
      If String.IsNullOrEmpty(tAlbum) Then
        Stop
        tAlbum = "Unknown Album"
      End If
      lblTmp.Text = tArtist & " - " & tAlbum
      lblTmp.Name = "lblArt" & Hex(RowData.GetHashCode)
      lblTmp.Anchor = (AnchorStyles.Left Or AnchorStyles.Right)
      lblTmp.AutoSize = True
      lblTmp.TextAlign = Drawing.ContentAlignment.TopCenter
      lblTmp.UseMnemonic = False
      pnlTmp.Controls.Add(pctTmp, 0, 0)
      pnlTmp.Controls.Add(lblTmp, 0, 1)
      pnlTmp.Size = New Drawing.Size(106, 109 + lblTmp.Height)
      AddHandler pnlTmp.Click, AddressOf ElementClick
      AddHandler pctTmp.Click, AddressOf ElementClick
      AddHandler lblTmp.Click, AddressOf ElementClick
      pnlArtwork.Controls.Add(pnlTmp)
    End If
  End Sub

  Private Sub GetArtwork(pctTmp As PictureBox)
    Dim rowURI As Uri = pctTmp.Tag
    If sckReader.IsBusy Then sckReader.CancelAsync()
    Do While sckReader.IsBusy
      Application.DoEvents()
      Threading.Thread.Sleep(1)
    Loop
    sckReader.DownloadDataAsync(rowURI, pctTmp.Name)
  End Sub

  Private Function GetArtworkURI(Row As Generic.Dictionary(Of String, Object), Optional ByVal SetSize As String = ".") As Uri
    Dim sURL As String = Row("artworkUrl100").ToString
    If String.IsNullOrEmpty(sURL) Then Return Nothing
    If Not String.IsNullOrEmpty(SetSize) Then If sURL.Contains(".100x100-75.") Then sURL = sURL.Replace(".100x100-75.", SetSize)
    Return New Uri(sURL)
  End Function

  Private Function GetArtist(Row As Generic.Dictionary(Of String, Object)) As String
    Dim sName As String = Row("artistName").ToString
    If String.IsNullOrEmpty(sName) Then Return Nothing
    Return UnEscape(sName)
  End Function

  Private Function GetAlbum(Row As Generic.Dictionary(Of String, Object)) As String
    Dim sName As String = Row("collectionName").ToString
    If String.IsNullOrEmpty(sName) Then Return Nothing
    Return UnEscape(sName)
  End Function

  Private Function UnEscape(Data As String) As String
    If Data.Contains("\u") Then
      For ID As Integer = 0 To &HFFFF
        Data = Data.Replace("\u" & PadHex(ID, 4).ToLower, ChrW(ID))
      Next
    End If
    Return Data
  End Function

  Private Function PadHex(Value As UInt32, Length As UInt16) As String
    Dim sVal As String = Hex(Value)
    Do While sVal.Length < Length : sVal = "0" & sVal : Loop
    Return sVal
  End Function

  Private Sub ElementClick(sender As Object, e As EventArgs)
    For Each box As TableLayoutPanel In pnlArtwork.Controls
      box.BackColor = Drawing.Color.Transparent
    Next
    If sender.GetType Is GetType(TableLayoutPanel) Then
      Dim sBox As TableLayoutPanel = sender
      sBox.BackColor = Drawing.SystemColors.Highlight
      cmdSelect.Tag = sender.tag
    Else
      Dim sBox As TableLayoutPanel = sender.parent
      sBox.BackColor = Drawing.SystemColors.Highlight
      cmdSelect.Tag = sender.parent.tag
    End If
    pnlArtwork.Focus()
  End Sub

  Private Sub tmrArtwork_Tick(sender As System.Object, e As System.EventArgs) Handles tmrArtwork.Tick
    tmrArtwork.Enabled = False
    For Each item As TableLayoutPanel In pnlArtwork.Controls
      If Not Me.Visible Then Exit For
      If item.Controls(0).Tag IsNot Nothing Then
        pnlProgress.Visible = True
        pbProgress.Value = 0
        pbProgress.Maximum = 100
        lblProgress.Text = "0%"
        Me.Cursor = Cursors.AppStarting
        Dim tX As New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf GetArtwork))
        tX.Start(item.Controls(0))
        Exit Sub
      End If
    Next
    tmrArtwork.Enabled = True
  End Sub

  Private Delegate Sub DownloadDataCompletedCallBack(sender As Object, e As System.Net.DownloadDataCompletedEventArgs)
  Private Sub sckReader_DownloadDataCompleted(sender As Object, e As System.Net.DownloadDataCompletedEventArgs) Handles sckReader.DownloadDataCompleted
    If Me.InvokeRequired Then
      Me.Invoke(New DownloadDataCompletedCallBack(AddressOf sckReader_DownloadDataCompleted), sender, e)
    Else
      Me.Cursor = Cursors.WaitCursor
      pbProgress.Value = 0
      pbProgress.Maximum = 100
      pbProgress.Value = 100

      Dim pnl = (From item As TableLayoutPanel In pnlArtwork.Controls Where item.Name = CType(e.UserState, String).Replace("pct", "pnl")).FirstOrDefault
      If pnl Is Nothing Then Exit Sub
      Dim pctTmp As PictureBox = pnl.Controls(0)
      If e.Error Is Nothing Then
        lblProgress.Text = "100%"
        pctTmp.Image = Drawing.Image.FromStream(New IO.MemoryStream(e.Result))
      Else
        lblProgress.Text = "Error"
        Dim imgTmp As Drawing.Image = New Drawing.Bitmap(100, 100)
        Using g As Drawing.Graphics = Drawing.Graphics.FromImage(imgTmp)
          g.Clear(Drawing.Color.White)
          Using redBrush As New Drawing.Drawing2D.LinearGradientBrush(New Drawing.Rectangle(0, 0, 99, 99), Drawing.Color.White, Drawing.Color.Red, Drawing.Drawing2D.LinearGradientMode.Vertical)
            g.FillRectangle(redBrush, New Drawing.Rectangle(0, 0, 99, 99))
          End Using
          g.DrawRectangle(Drawing.Pens.Black, 0, 0, 99, 99)
          g.DrawString("Error!", New Drawing.Font(Drawing.FontFamily.GenericSansSerif, 16, Drawing.FontStyle.Bold), Drawing.Brushes.Yellow, New Drawing.RectangleF(1, 1, 98, 98), New Drawing.StringFormat With {.Alignment = Drawing.StringAlignment.Center, .LineAlignment = Drawing.StringAlignment.Center})
        End Using
        pctTmp.Image = imgTmp
      End If
      pnlProgress.Visible = False
      pctTmp.Tag = Nothing
      tmrArtwork.Enabled = True
      Me.Cursor = Cursors.Default
    End If
  End Sub

  Private Delegate Sub DownloadProgressChangedCallBack(sender As Object, e As System.Net.DownloadProgressChangedEventArgs)
  Private Sub sckReader_DownloadProgressChanged(sender As Object, e As System.Net.DownloadProgressChangedEventArgs) Handles sckReader.DownloadProgressChanged
    If Me.InvokeRequired Then
      Me.BeginInvoke(New DownloadProgressChangedCallBack(AddressOf sckReader_DownloadProgressChanged), sender, e)
    Else
      pbProgress.Maximum = e.TotalBytesToReceive
      pbProgress.Value = e.BytesReceived
      lblProgress.Text = FormatPercent(e.BytesReceived / e.TotalBytesToReceive, 0, TriState.False, TriState.False, TriState.False)
    End If
  End Sub
  Private Sub cmdSearch_Click(sender As System.Object, e As System.EventArgs) Handles cmdSearch.Click
    cmdSearch.Enabled = False
    txtSearch.Enabled = False
    tmrArtwork.Enabled = False
    Dim sData As String = String.Empty
    Me.Cursor = Cursors.AppStarting
    Application.DoEvents()
    Using wsTmp As New Net.WebClient
      wsTmp.Encoding = System.Text.Encoding.UTF8
      sData = wsTmp.DownloadString(New Uri("http://itunes.apple.com/search?term=" & txtSearch.Text.Replace(" "c, "+"c) & "&media=music&entity=album"))
    End Using
    Dim Rows() As Generic.Dictionary(Of String, Object)
    Dim j As Object = New Web.Script.Serialization.JavaScriptSerializer().Deserialize(Of Object)(sData)
    Dim rowCount As Integer = j("resultCount")
    Dim rowData() As Object = j("results")
    ReDim Rows(rowCount - 1)
    Dim I As Integer = 0
    For Each dItem In rowData
      Rows(I) = dItem
      I += 1
    Next
    pnlArtwork.Controls.Clear()
    If Rows.Count > 4 Then
      pnlProgress.Visible = True
      pbProgress.Value = 0
      pbProgress.Maximum = Rows.Count
      lblProgress.Text = "0%"
    End If
    For Each row In Rows
      AddRow(row)
      If pnlProgress.Visible Then
        pbProgress.Increment(1)
        lblProgress.Text = FormatPercent(pbProgress.Value / pbProgress.Maximum, 0, TriState.False, TriState.False, TriState.False)
        Application.DoEvents()
      End If
    Next
    If pnlProgress.Visible Then
      pnlProgress.Visible = False
    End If
    Me.Cursor = Cursors.Default
    tmrArtwork.Enabled = True
    cmdSearch.Enabled = True
    txtSearch.Enabled = True
    pnlArtwork.Focus()
  End Sub

  Private Sub cmdSelect_Click(sender As System.Object, e As System.EventArgs) Handles cmdSelect.Click
    tmrArtwork.Enabled = False
    RaiseEvent NewArt(cmdSelect.Tag)
  End Sub

  Private Sub cmdCancel_Click(sender As System.Object, e As System.EventArgs) Handles cmdCancel.Click
    tmrArtwork.Enabled = False
    RaiseEvent Cancelled()
  End Sub

  Private Sub txtSearch_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtSearch.KeyDown
    If e.KeyCode = Keys.Enter Then
      e.Handled = True
      e.SuppressKeyPress = True
      cmdSearch.PerformClick()
    End If
  End Sub
End Class
