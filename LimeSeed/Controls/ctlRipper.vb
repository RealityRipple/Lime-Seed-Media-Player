Public Class ctlRipper
  Private WithEvents cDrive As CDDrive
  Private WithEvents cInfo As CDDBNet
  Private WithEvents cArtwork As AppleNet
  Private WithEvents cCDText As New ReadCdTextWorker
  Private Delegate Sub CallBack(Obj As Object)
  Private mWriter As Mp3Writer
  Private lLastRev As Integer
  Private Const sRip As String = "Rip Tracks"
  Private Const sCancel As String = "Cancel"
  Private noPrompt As Boolean

  Private Sub ctlRipper_Disposed(sender As Object, e As System.EventArgs) Handles Me.Disposed
    cInfo = Nothing
    cDrive = Nothing
  End Sub

  Public Sub Init()
    noPrompt = True
    cDrive = New CDDrive
    RefreshDriveList()
    lLastRev = -1
    pctCover.AllowDrop = True
  End Sub

  Public Sub CancelActions()
    If cDrive IsNot Nothing Then cDrive.Close()
    cInfo = Nothing
    cArtwork = Nothing
    cCDText.Dispose()
  End Sub

  Public Sub RefreshDriveList()
    If cDrive Is Nothing Then
      Init()
    End If
    Dim sDriveLetter As Char
    Dim iActive As Integer = -1
    If Not String.IsNullOrEmpty(cmbDrive.SelectedItem) Then sDriveLetter = cmbDrive.SelectedItem.Substring(0, 1)
    Dim items As New Collections.ObjectModel.Collection(Of String)
    items.Clear()
    For Each drive In CDDrive.GetCDDriveLetters
      Dim dInfo = My.Computer.FileSystem.GetDriveInfo(drive & ":\")
      If dInfo.IsReady Then
        If dInfo.DriveFormat = "CDFS" Then
          items.Add(drive & ":\ [" & dInfo.VolumeLabel & "]")
          If iActive = -1 Then iActive = items.Count - 1
        Else
          items.Add(drive & ":\ [" & dInfo.DriveFormat & " Disc]")
        End If
      Else
        items.Add(drive & ":\ [No Disc]")
      End If
      If drive = sDriveLetter Then iActive = items.Count - 1
    Next
    Dim bChange As Boolean = False
    If cmbDrive.Items.Count > 0 Then
      For I As Integer = 0 To cmbDrive.Items.Count - 1
        If String.Compare(cmbDrive.Items(I), items(I)) <> 0 Then
          bChange = True
          Exit For
        End If
      Next
    Else
      bChange = True
    End If
    If bChange Then
      If cmbDrive.Items.Count <> items.Count Then
        cmbDrive.Items.Clear()
        For Each item In items
          cmbDrive.Items.Add(item)
        Next
        If cmbDrive.SelectedIndex = -1 Then cmbDrive.SelectedIndex = iActive
      Else
        For I As Integer = 0 To cmbDrive.Items.Count - 1
          If String.Compare(cmbDrive.Items(I), items(I)) <> 0 Then
            cmbDrive.Items(I) = items(I)
            If cmbDrive.SelectedIndex > -1 Then
              cmbDrive_SelectedIndexChanged(New Object, New EventArgs)
            Else
              cmbDrive.SelectedIndex = iActive
            End If
          End If
        Next
      End If
      lLastRev = -1
    End If
  End Sub

  Private Sub cDrive_CDInserted(sender As Object, e As System.EventArgs) Handles cDrive.CDInserted
    RefreshDriveList()
  End Sub

  Private Sub cDrive_CDRemoved(sender As Object, e As System.EventArgs) Handles cDrive.CDRemoved
    RefreshDriveList()
  End Sub

  Private Sub cmbDrive_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbDrive.SelectedIndexChanged
    If String.IsNullOrEmpty(cmbDrive.SelectedItem) Or cmbDrive.DroppedDown Then Return
    If cDrive Is Nothing Then Return
    If cDrive.Open(cmbDrive.SelectedItem.Substring(0, 1)) Then
      If cDrive.IsCDReady Then
        If cDrive.Refresh And cDrive.GetNumTracks > 0 Then
          If cDrive.GetNumAudioTracks > 0 Then
            If dgvTracks.Rows.Count = cDrive.GetNumTracks Then
              Dim Redo As Boolean = False
              Dim J As Integer = 0
              For I As Integer = 1 To cDrive.GetNumTracks
                If dgvTracks.Rows(J).Cells(5).Value <> CalcLength(cDrive.TrackSize(I)) Then
                  Redo = True
                  Exit For
                End If
                J += 1
              Next
              If Not Redo Then Return
            End If
            dgvTracks.Rows.Clear()
            txtAlbumAlbum.Text = "Unknown Album"
            txtAlbumArtist.Text = "Unknown Artist"
            txtAlbumGenre.Text = "Unknown Genre"
            txtAlbumYear.Text = "Unknown Year"
            pctCover.Image = Nothing
            pctCover.Tag = Nothing

            lblStatus.Text = "Drive ready with " & cDrive.GetNumTracks & " tracks."
            For I As Integer = 1 To cDrive.GetNumTracks
              If cDrive.IsAudioTrack(I) Then
                dgvTracks.Rows.Add({True, Format(I, "00"), "Track " & Format(I, "00"), "Unknown Artist", "Unknown Album", CalcLength(cDrive.TrackSize(I)), 0})
              Else
                dgvTracks.Rows.Add({False, Format(I, "00"), "Data Track " & Format(I, "00"), "Unknown Artist", "Unknown Disc", CalcLength(cDrive.TrackSize(I)), 0})
              End If
            Next
            If cDrive.GetNumAudioTracks > 0 Then
              lblStatus.Text = "Drive ready with " & cDrive.GetNumTracks & " tracks. Checking for CD Info..."
              Dim selDrive As IO.DriveInfo = New IO.DriveInfo(cmbDrive.SelectedItem)
              cCDText.RunWorkerAsync(selDrive)
            End If
          Else
            dgvTracks.Rows.Clear()
            lblStatus.Text = "Drive " & cmbDrive.SelectedItem & " has no audio tracks."
          End If
        Else
          dgvTracks.Rows.Clear()
          lblStatus.Text = "Unable to read Table of Contents for drive " & cmbDrive.SelectedItem & "."
        End If
      Else
        dgvTracks.Rows.Clear()
        lblStatus.Text = "Drive " & cmbDrive.SelectedItem & " is not ready."
      End If
    Else
      dgvTracks.Rows.Clear()
      lblStatus.Text = "Unable to open connection to drive " & cmbDrive.SelectedItem & "."
    End If
  End Sub

  Private Function CalcLength(TrackSize As UInteger) As String
    Dim seconds As Double = Math.Floor(TrackSize / 176400)
    If seconds < 60 Then
      Return "00:" & Format(seconds, "00")
    ElseIf seconds < 60 * 60 Then
      Dim M As UInteger = seconds \ 60
      seconds -= M * 60
      Dim S As UInteger = seconds
      Return Format(M, "00") & ":" & Format(S, "00")
    ElseIf seconds < 60 * 60 * 24 Then
      Dim H As UInteger = seconds \ (60 * 60)
      seconds -= H * 60 * 60
      Dim M As UInteger = seconds \ 60
      seconds -= M * 60
      Dim S As UInteger = seconds
      Return Format(H, "00") & ":" & Format(M, "00") & ":" & Format(S, "00")
    Else
      Dim D As UInteger = seconds \ (60 * 60 * 24)
      seconds -= D * 60 * 60 * 24
      Dim H As UInteger = seconds \ (60 * 60)
      seconds -= H * 60 * 60
      Dim M As UInteger = seconds \ 60
      seconds -= M * 60
      Dim S As UInteger = seconds
      Return D & ":" & Format(H, "00") & ":" & Format(M, "00") & ":" & Format(S, "00")
    End If
  End Function

  Private Function sumOfDigits(nVal As ULong) As ULong
    Dim sum As ULong = 0
    While nVal > 0
      sum += nVal Mod 10
      nVal \= 10
    End While
    Return sum
  End Function

  Private Function CalcDiscID(Disc As Char, Optional ByVal FullID As Boolean = True) As String
    Dim sTmp As String = String.Empty
    Dim StartSum As ULong = 0
    Dim LenSum As Double = 0
    For I As Integer = 1 To cDrive.GetNumTracks
      StartSum += sumOfDigits(Math.Floor((cDrive.GetStartSector(I) + 150) / 75))
      LenSum += cDrive.TrackSize(I) / 176400
    Next
    StartSum = StartSum Mod 255
    If StartSum > &HF Then
      sTmp = Hex(StartSum)
    Else
      sTmp = "0" & Hex(StartSum)
    End If
    Dim iLenSum As UInteger = Math.Floor(LenSum)
    sTmp &= IIf(Hex(iLenSum).Length = 4, Hex(iLenSum), StrDup(4 - Hex(iLenSum).Length, "0"c) & Hex(iLenSum))
    If cDrive.GetNumTracks > &HF Then
      sTmp &= Hex(cDrive.GetNumTracks)
    Else
      sTmp &= "0" & Hex(cDrive.GetNumTracks)
    End If
    If FullID Then
      sTmp &= " " & cDrive.GetNumTracks
      For I As Integer = 1 To cDrive.GetNumTracks
        sTmp &= " " & (cDrive.GetStartSector(I) + 150)
      Next
      sTmp &= " " & (iLenSum + 2)
    End If
    Return sTmp
  End Function

  Private Sub cInfo_FindError(Message As String) Handles cInfo.FindError
    SetStatus("Error: " & Message)
  End Sub

  Private Sub SetStatus(Obj As Object)
    If Me.InvokeRequired Then
      Me.Invoke(New CallBack(AddressOf SetStatus), Obj)
    Else
      lblStatus.Text = CType(Obj, String)
    End If
  End Sub

  Private Sub SetGlobalInfo(Obj As Object)
    If Me.InvokeRequired Then
      Me.BeginInvoke(New CallBack(AddressOf SetGlobalInfo), Obj)
    Else
      For I As Integer = 0 To UBound(Obj)
        Select Case Obj(I, 0)
          Case "Artist" : txtAlbumArtist.Text = Obj(I, 1)
          Case "Album" : txtAlbumAlbum.Text = Obj(I, 1)
          Case "Genre" : txtAlbumGenre.Text = Obj(I, 1)
          Case "Year" : txtAlbumYear.Text = Obj(I, 1)
        End Select
      Next I
    End If
  End Sub

  Private Sub cInfo_Finding(Artist As String, Album As String, Genre As String) Handles cInfo.Finding
    SetStatus("Finding album information for " & Album & " by " & Artist & "...")
    SetGlobalInfo({{"Artist", Artist}, {"Album", Album}, {"Genre", Genre}})
  End Sub

  Private Sub cInfo_Finished() Handles cInfo.Finished
    'cInfo = Nothing
  End Sub

  Private Sub cInfo_Found(Data As String) Handles cInfo.Found
    If Me.InvokeRequired Then
      Me.BeginInvoke(New CallBack(AddressOf cInfo_Found), Data)
    Else
      SetStatus("Drive ready with " & cDrive.GetNumTracks & " tracks.")
      Dim sSplits() As String = Split(Data, vbNewLine)
      Dim sArtist, sAlbum As String
      lLastRev = Val(GetLine(sSplits, "Revision"))
      If GetLine(sSplits, "DTITLE").Contains(" / ") Then
        sArtist = Split(GetLine(sSplits, "DTITLE"), " / ")(0)
        sAlbum = Split(GetLine(sSplits, "DTITLE"), " / ")(1)
      Else
        sArtist = GetLine(sSplits, "DTITLE")
        sAlbum = GetLine(sSplits, "DTITLE")
      End If
      txtAlbumAlbum.Text = sAlbum
      txtAlbumArtist.Text = sArtist
      'txtAlbumGenre.Text = 
      If Not sAlbum = sArtist Then
        cArtwork = New AppleNet(sAlbum, AppleNet.Term.Album)
      Else
        cArtwork = New AppleNet(sArtist, AppleNet.Term.Artist)
      End If
      Dim SpecialArtist As Boolean = True
      For I As Integer = 0 To dgvTracks.Rows.Count - 1
        If Split(GetLine(sSplits, "TTITLE" & I), " / ").Length <> 2 Then
          SpecialArtist = False
          Exit For
        End If
      Next
      For I As Integer = 0 To dgvTracks.Rows.Count - 1
        Dim sTitle As String = GetLine(sSplits, "TTITLE" & I)
        If SpecialArtist Then
          dgvTracks.Rows(I).Cells(2).Value = Split(sTitle, " / ", 2)(1)
          dgvTracks.Rows(I).Cells(3).Value = Split(sTitle, " / ", 2)(0)
        Else
          dgvTracks.Rows(I).Cells(2).Value = sTitle
          dgvTracks.Rows(I).Cells(3).Value = sArtist
        End If
        dgvTracks.Rows(I).Cells(4).Value = sAlbum
      Next

    End If
  End Sub

  Private Function GetLine(Lines() As String, ByVal ID As String) As String
    For Each Line As String In Lines
      If Line.StartsWith(ID & "=") Then Return Line.Substring(Line.IndexOf("="c) + 1)
      If Line.StartsWith("# " & ID & ": ") Then Return Line.Substring(Line.IndexOf(": ") + 2)
    Next
    Return String.Empty
  End Function

  Private Sub cmdRip_Click(sender As System.Object, e As System.EventArgs) Handles cmdRip.Click
    If dgvTracks.Rows.Count = 0 Then
      Beep()
      Return
    End If
    If cmdRip.Text = sRip Then
      cmdRip.Text = sCancel
      Dim sArtistDir As String = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) & "\" & SafeName(txtAlbumArtist.Text)
      Dim cArtistDir As String = CoolName(SafeName(txtAlbumArtist.Text))
      Dim sAlbumDir As String = sArtistDir & "\" & SafeName(txtAlbumAlbum.Text)
      Dim cAlbumDir As String = CoolName(SafeName(txtAlbumAlbum.Text))
      Dim fDir As String = sAlbumDir & "\"
      For I As Integer = 0 To dgvTracks.Rows.Count - 1
        If dgvTracks.Rows(I).Cells(0).Value = True Then
          Dim iTracks As Integer = (From row As DataGridViewRow In dgvTracks.Rows Where row.Cells(0).Value).Count
          SetStatus("Ripping track #" & I + 1 & " (" & iTracks & " track" & IIf(iTracks = 1, String.Empty, "s") & " remaining)")
          dgvTracks.Tag = I
          Dim sTitle As String = dgvTracks.Rows(dgvTracks.Tag).Cells(2).Value
          Dim sTrack As String = dgvTracks.Rows(dgvTracks.Tag).Cells(1).Value
          Dim fPath As String = fDir & sTrack & " " & SafeName(sTitle) & ".mp3"
          If Not My.Computer.FileSystem.DirectoryExists(fDir) Then My.Computer.FileSystem.CreateDirectory(fDir)
          mWriter = New Mp3Writer(New IO.FileStream(fPath, IO.FileMode.Create), New Mp3WriterConfig(New WaveFormat(44100, 16, 2), New BE_CONFIG(New WaveFormat(44100, 16, 2), 320)))
          Dim beVer As New BE_VERSION
          Lame_encDll.beVersion(beVer)
          mWriter.Path = fPath
          If cDrive.ReadTrack(I + 1, New CdDataReadEventHandler(AddressOf writeData), New CdReadProgressEventHandler(AddressOf showProgress)) > 0 Then
            dgvTracks.Rows(I).Cells(0).Value = False
            dgvTracks.Rows(I).Cells(6).Value = 100
          Else
            dgvTracks.Rows(I).Cells(6).Value = 0
          End If
          If cmdRip.Text = sRip Then Exit For
          If mWriter IsNot Nothing Then mWriter.Close()
          mWriter = Nothing
          Using ID3er As New Seed.clsID3v2(fPath)
            ID3er.ID3v2Ver = "2.3.0"
            ID3er.AddTextFrame("TP1", New Seed.clsID3v2.EncodedText(dgvTracks.Rows(dgvTracks.Tag).Cells(3).Value))
            ID3er.AddTextFrame("TP2", New Seed.clsID3v2.EncodedText(dgvTracks.Rows(dgvTracks.Tag).Cells(3).Value))
            ID3er.AddTextFrame("TAL", New Seed.clsID3v2.EncodedText(dgvTracks.Rows(dgvTracks.Tag).Cells(4).Value))
            ID3er.AddTextFrame("TT2", New Seed.clsID3v2.EncodedText(sTitle))
            ID3er.AddTextFrame("TRK", New Seed.clsID3v2.EncodedText(sTrack))
            If Not String.IsNullOrEmpty(txtAlbumGenre.Text) AndAlso Not txtAlbumGenre.Text = "Unknown Genre" Then ID3er.Genre = txtAlbumGenre.Text
            If Not String.IsNullOrEmpty(txtAlbumYear.Text) AndAlso Not txtAlbumYear.Text = "Unknown Year" Then
              ID3er.AddTextFrame("TRD", New Seed.clsID3v2.EncodedText(txtAlbumYear.Text))
              ID3er.AddTextFrame("TYE", New Seed.clsID3v2.EncodedText(txtAlbumYear.Text))
            End If
            If pctCover.Tag IsNot Nothing Then
              Dim bCover As Byte() = pctCover.Tag
              ID3er.AddImageFrame(bCover, Seed.clsID3v2.ID3_PIC_TYPE.FRONT_COVER, Seed.clsID3v2.ID3_PIC_MIME.JPG, Seed.clsID3v2.EncodedText.Empty)
            End If
            ID3er.AddTextFrame("TEN", New Seed.clsID3v2.EncodedText("LimeSeed [LAME v" & beVer.byMajorVersion & "." & beVer.byMinorVersion & " (" & beVer.byMonth & "/" & beVer.byDay & "/" & beVer.wYear & ")]"))
            ID3er.Save()
          End Using
          Using ID3v1 As New Seed.clsID3v1(fPath)
            ID3v1.Artist = dgvTracks.Rows(dgvTracks.Tag).Cells(3).Value
            ID3v1.Album = dgvTracks.Rows(dgvTracks.Tag).Cells(4).Value
            ID3v1.Title = sTitle
            ID3v1.Track = sTrack
            If Not String.IsNullOrEmpty(txtAlbumGenre.Text) AndAlso Not txtAlbumGenre.Text = "Unknown Genre" Then
              Dim bDid As Boolean = False
              For J As Integer = 0 To &H93
                If String.Compare(Seed.clsID3v1.GenreName(J), txtAlbumGenre.Text, True) = 0 Then
                  bDid = True
                  ID3v1.Genre = J
                  Exit For
                End If
              Next
              If Not bDid Then ID3v1.Genre = &HC
            End If
            If Not String.IsNullOrEmpty(txtAlbumYear.Text) AndAlso Not txtAlbumYear.Text = "Unknown Year" Then ID3v1.Year = txtAlbumYear.Text
            ID3v1.Comment = "Encoded with LAME v" & beVer.byMajorVersion & "." & beVer.byMinorVersion
            ID3v1.Save()
          End Using
          If IO.Path.GetFileName(fPath).Contains("{") Then My.Computer.FileSystem.RenameFile(fPath, CoolName(IO.Path.GetFileName(fPath)))
        End If
        If cmdRip.Text = sRip Then Exit For
      Next
      If Not My.Computer.FileSystem.FileExists(IO.Path.Combine(fDir, "Folder.jpg")) And pctCover.Tag IsNot Nothing Then My.Computer.FileSystem.WriteAllBytes(IO.Path.Combine(fDir, "Folder.jpg"), pctCover.Tag, False)

      If sArtistDir.Contains("{") Then My.Computer.FileSystem.RenameDirectory(sArtistDir, cArtistDir)
      If sAlbumDir.Contains("{") Then My.Computer.FileSystem.RenameDirectory(sAlbumDir, cAlbumDir)

      SetStatus("Rip Complete!")
      dgvTracks.Tag = Nothing
      cmdRip.Text = sRip
    Else
      cmdRip.Text = sRip
      cDrive.Close()
      cDrive = Nothing
      mWriter.Close()
      mWriter = Nothing
      cDrive = New CDDrive
      'RefreshDriveList()
    End If
  End Sub
  Private Sub writeData(sender As Object, ea As DataReadEventArgs)
    mWriter.Write(ea.Data)
  End Sub
  Private Sub showProgress(sender As Object, ea As ReadProgressEventArgs)
    If dgvTracks.Tag IsNot Nothing Then
      dgvTracks.Rows(dgvTracks.Tag).Cells(6).Value = Math.Round((ea.BytesRead / ea.Bytes2Read) * 100)
      Application.DoEvents()
    End If
  End Sub

  Private Sub cArtwork_Choices(sender As Object, e As AppleNet.ChoicesEventArgs) Handles cArtwork.Choices
    If Me.InvokeRequired Then
      Me.Invoke(New EventHandler(Of AppleNet.ChoicesEventArgs)(AddressOf cArtwork_Choices), sender, e)
    Else
      For Each ret In e.Rows
        If StrEquiv(ret("artistName"), txtAlbumArtist.Text) And StrEquiv(ret("collectionName"), txtAlbumAlbum.Text) Then
          If Not ret("artworkUrl100") Is Nothing Then
            cArtwork.ChooseRow(ret)
            Return
          End If
        End If
      Next

      If Not noPrompt Then
        Dim artList As New frmArtList
        artList.Display(txtAlbumArtist.Text, txtAlbumAlbum.Text, e.Rows)
        If artList.ShowDialog() = DialogResult.OK Then
          'Stop
          cArtwork.ChooseRow(artList.Tag)
        End If
      Else
        Dim imgTmp As Drawing.Image = New Drawing.Bitmap(150, 150)
        Using g As Drawing.Graphics = Drawing.Graphics.FromImage(imgTmp)
          g.Clear(Drawing.Color.White)
          Using redBrush As New Drawing.Drawing2D.LinearGradientBrush(New Drawing.Rectangle(0, 0, 149, 149), Drawing.Color.White, Drawing.Color.Red, Drawing.Drawing2D.LinearGradientMode.Vertical)
            g.FillRectangle(redBrush, New Drawing.Rectangle(0, 0, 149, 149))
          End Using
          g.DrawRectangle(Drawing.Pens.Black, 0, 0, 149, 149)
          g.DrawString("No Cover", New Drawing.Font(Drawing.FontFamily.GenericSansSerif, 24, Drawing.FontStyle.Bold), Drawing.Brushes.Yellow, New Drawing.RectangleF(1, 1, 148, 148), New Drawing.StringFormat With {.Alignment = Drawing.StringAlignment.Center, .LineAlignment = Drawing.StringAlignment.Center})
        End Using
        pctCover.Image = imgTmp.Clone
        pctCover.Tag = Nothing
        cArtwork = Nothing
      End If
    End If

  End Sub

  Private Sub cArtwork_Complete(sender As Object, e As AppleNet.CompleteEventArgs) Handles cArtwork.Complete
    If Me.InvokeRequired Then
      Me.Invoke(New EventHandler(Of AppleNet.CompleteEventArgs)(AddressOf cArtwork_Complete), sender, e)
    Else
      pctCover.Image = Drawing.Image.FromStream(New IO.MemoryStream(e.Cover))
      pctCover.Tag = e.Cover
      cArtwork = Nothing
    End If
  End Sub

  Private Sub cArtwork_Failed(sender As Object, e As AppleNet.FailEventArgs) Handles cArtwork.Failed
    If Me.InvokeRequired Then
      Me.Invoke(New EventHandler(Of AppleNet.FailEventArgs)(AddressOf cArtwork_Failed), sender, e)
    Else
      SetStatus("Failed to get artwork: " & e.Error.Message)
    End If
  End Sub

  Private Sub cArtwork_Infos(sender As Object, e As AppleNet.InfosEventArgs) Handles cArtwork.Infos
    If Me.InvokeRequired Then
      Me.Invoke(New EventHandler(Of AppleNet.InfosEventArgs)(AddressOf cArtwork_Infos), sender, e)
    Else
      SetGlobalInfo({{"Year", e.ReleaseDate}, {"Genre", e.Genre}})
    End If
  End Sub

  Private Sub cmdGetInfo_Click(sender As System.Object, e As System.EventArgs) Handles cmdGetInfo.Click
    noPrompt = False
    cArtwork = New AppleNet(txtAlbumAlbum.Text, AppleNet.Term.Any)
  End Sub

  Private Sub cmdSendInfo_Click(sender As System.Object, e As System.EventArgs) Handles cmdSendInfo.Click
    If lLastRev = -1 Then
      MsgBox("Revision entry is not set, you may be unable to submit this entry if a version already exists.", MsgBoxStyle.Exclamation, "No Revision Entry Set")
      lLastRev = 0
    End If
    If MsgBox("Are you sure you wish to submit this album to FreeDB?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo, "Submit Album?") = MsgBoxResult.Yes Then
      Dim sCat, sID, sEntry As String
      If String.Compare(txtAlbumGenre.Text, "blues", True) = 0 Or
         String.Compare(txtAlbumGenre.Text, "classical", True) = 0 Or
         String.Compare(txtAlbumGenre.Text, "country", True) = 0 Or
         String.Compare(txtAlbumGenre.Text, "data", True) = 0 Or
         String.Compare(txtAlbumGenre.Text, "folk", True) = 0 Or
         String.Compare(txtAlbumGenre.Text, "jazz", True) = 0 Or
         String.Compare(txtAlbumGenre.Text, "reggae", True) = 0 Or
         String.Compare(txtAlbumGenre.Text, "soundtrack", True) = 0 Then
        sCat = txtAlbumGenre.Text.ToLower
      ElseIf String.Compare(txtAlbumGenre.Text, "newage", True) = 0 Or
        String.Compare(txtAlbumGenre.Text, "new age", True) = 0 Then
        sCat = "newage"
      ElseIf txtAlbumGenre.Text.ToLower.Contains("rock") Or
        txtAlbumGenre.Text.ToLower.Contains("funk") Or
        txtAlbumGenre.Text.ToLower.Contains("soul") Or
        txtAlbumGenre.Text.ToLower.Contains("rap") Or
        txtAlbumGenre.Text.ToLower.Contains("pop") Or
        txtAlbumGenre.Text.ToLower.Contains("alternative") Or
        txtAlbumGenre.Text.ToLower.Contains("industrial") Or
        txtAlbumGenre.Text.ToLower.Contains("metal") Then
        sCat = "rock"
      Else
        sCat = "misc"
      End If
      Dim sFullID() As String = CalcDiscID(cmbDrive.SelectedItem.substring(0, 1)).Split(" "c)
      sID = sFullID(0)
      sEntry = "# xmcd" & vbCrLf
      sEntry &= "#" & vbCrLf
      sEntry &= "# Track frame offsets:" & vbCrLf
      For I As Integer = 1 To sFullID.Count - 2
        sEntry &= "#   " & sFullID(I) & vbCrLf
      Next
      sEntry &= "#" & vbCrLf
      Dim lLength As Long = 0
      For I = 1 To cDrive.GetNumTracks
        lLength += cDrive.TrackSize(I)
      Next I
      sEntry &= "# Disc length: " & Math.Round(lLength / 176400) & " seconds" & vbCrLf
      sEntry &= "#" & vbCrLf
      sEntry &= "# Revision: " & lLastRev + 1 & vbCrLf
      sEntry &= "# Submitted via: " & Application.ProductName.Replace(" "c, "_"c) & " " & Application.ProductVersion & vbCrLf
      sEntry &= "#" & vbCrLf
      sEntry &= "DISCID=" & sID & vbCrLf
      sEntry &= "DTITLE=" & txtAlbumArtist.Text & " / " & txtAlbumAlbum.Text & vbCrLf
      sEntry &= "DYEAR=" & txtAlbumYear.Text & vbCrLf
      sEntry &= "DGENRE=" & txtAlbumGenre.Text & vbCrLf
      For I As Integer = 0 To dgvTracks.Rows.Count - 1
        If String.Compare(dgvTracks.Rows(I).Cells(3).Value, txtAlbumArtist.Text, True) = 0 Then
          sEntry &= "TTITLE" & I & "=" & dgvTracks.Rows(I).Cells(2).Value & vbCrLf
        Else
          sEntry &= "TTITLE" & I & "=" & dgvTracks.Rows(I).Cells(3).Value & " / " & dgvTracks.Rows(I).Cells(2).Value & vbCrLf
        End If
      Next
      sEntry &= "EXTD=" & vbCrLf
      For I As Integer = 0 To dgvTracks.Rows.Count - 1
        sEntry &= "EXTT" & I & "=" & vbCrLf
      Next
      sEntry &= "PLAYORDER=" & vbNewLine
      If MsgBox("Ready to write entry:" & vbNewLine & "Category: " & sCat & vbNewLine & "Disc ID: " & sID & vbNewLine & "Entry Data:" & vbNewLine & sEntry & vbNewLine & "Submit this entry?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo, "Confirm Entry") = MsgBoxResult.Yes Then
        If cInfo.WriteEntry(sCat, sID, sEntry) Then
          SetStatus("Writing entry to FreeDB...")
        Else
          SetStatus("Prior entry pending! Please wait for previous task to complete...")
        End If
      End If
    End If
  End Sub

  Private Sub cInfo_ReadOnlyConnection(IsReadOnly As Boolean) Handles cInfo.ReadOnlyConnection
    If Me.InvokeRequired Then
      Me.BeginInvoke(New CallBack(AddressOf cInfo_ReadOnlyConnection), IsReadOnly)
    Else
      cmdSendInfo.Enabled = Not IsReadOnly
    End If
  End Sub

  Private Sub cInfo_WriteError(Message As String) Handles cInfo.WriteError
    SetStatus("Failed to write entry: " & Message)
  End Sub

  Private Sub cInfo_Written() Handles cInfo.Written
    SetStatus("Entry written!")
    lLastRev += 1
  End Sub

  Private Sub cmdApplyAlbum_Click(sender As System.Object, e As System.EventArgs) Handles cmdApplyAlbum.Click
    For Each row As DataGridViewRow In dgvTracks.Rows
      row.Cells(4).Value = txtAlbumAlbum.Text
    Next
  End Sub

  Private Sub cmdApplyArtist_Click(sender As System.Object, e As System.EventArgs) Handles cmdApplyArtist.Click
    For Each row As DataGridViewRow In dgvTracks.Rows
      row.Cells(3).Value = txtAlbumArtist.Text
    Next
  End Sub

  Private Sub cCDText_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles cCDText.RunWorkerCompleted
    Dim result As ReadCdTextWorkerResult = TryCast(e.Result, ReadCdTextWorkerResult)
    If result.Success AndAlso result.CDText.TrackData.Rows.Count > 0 Then
      Dim sArtist, sAlbum As String
      sAlbum = StrConv(result.CDText.TrackData.Rows(0).Item(2), VbStrConv.ProperCase)
      sArtist = StrConv(result.CDText.TrackData.Rows(0).Item(3), VbStrConv.ProperCase)
      txtAlbumAlbum.Text = sAlbum
      txtAlbumArtist.Text = sArtist
      cArtwork = New AppleNet(sAlbum, AppleNet.Term.Album)
      Dim SpecialArtist As Boolean = True
      For I As Integer = 1 To result.CDText.TrackData.Rows.Count - 1
        If Split(result.CDText.TrackData.Rows(I).Item(2), "-").Length <> 2 Then
          SpecialArtist = False
          Exit For
        End If
      Next
      For I As Integer = 1 To result.CDText.TrackData.Rows.Count - 1
        Dim sTitle As String = result.CDText.TrackData.Rows(I).Item(2)
        If SpecialArtist Then
          dgvTracks.Rows(I - 1).Cells(2).Value = StrConv(Split(sTitle, "-", 2)(0).Trim, VbStrConv.ProperCase)
          dgvTracks.Rows(I - 1).Cells(3).Value = StrConv(Split(sTitle, "-", 2)(1).Trim, VbStrConv.ProperCase)
        Else
          If Not String.IsNullOrWhiteSpace(result.CDText.TrackData.Rows(I).Item(3)) Then sArtist = StrConv(result.CDText.TrackData.Rows(I).Item(3), VbStrConv.ProperCase)
          dgvTracks.Rows(I - 1).Cells(2).Value = StrConv(sTitle, VbStrConv.ProperCase)
          dgvTracks.Rows(I - 1).Cells(3).Value = StrConv(sArtist, VbStrConv.ProperCase)
        End If
        dgvTracks.Rows(I - 1).Cells(4).Value = sAlbum
      Next

    Else
      Debug.Print(result.ErrorMessage)
      Try
        If cInfo Is Nothing Then cInfo = New CDDBNet
        Dim ID As String = CalcDiscID(cmbDrive.SelectedItem.substring(0, 1))
        lLastRev = 0
        cInfo.FindDisc(ID)
      Catch ex As Exception

      End Try
      'MessageBox.Show(result.ErrorMessage)
    End If
    'GoButton.Enabled = True
    Me.Cursor = Cursors.Default
  End Sub

  Private Sub pctCover_DoubleClick(sender As Object, e As System.EventArgs) Handles pctCover.DoubleClick
    Using cdlArt As New OpenFileDialog
      cdlArt.Filter = "Images|*.jpg;*.jpeg;*.gif;*.png;*.bmp|All Files|*.*"
      cdlArt.CheckFileExists = True
      'cdlArt.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyDocuments
      cdlArt.Title = "Select Album Cover..."
      If cdlArt.ShowDialog(Me) = DialogResult.OK Then
        If My.Computer.FileSystem.GetFileInfo(cdlArt.FileName).Length >= 1024L * 1024L * 1024L * 4L Then Return
        Dim bCover As Byte() = My.Computer.FileSystem.ReadAllBytes(cdlArt.FileName)
        pctCover.Image = Drawing.Image.FromStream(New IO.MemoryStream(bCover))
        pctCover.Tag = bCover
      End If
    End Using
  End Sub

  Private Sub pctCover_DragDrop(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles pctCover.DragDrop
    If e.Data.GetFormats(True).Contains("FileDrop") Then
      Dim Data = e.Data.GetData("FileDrop")
      If UBound(Data) = 0 Then
        Select Case IO.Path.GetExtension(Data(0)).ToLower
          Case ".jpg", ".jpeg", ".gif", ".png", ".bmp"
            If My.Computer.FileSystem.GetFileInfo(Data(0)).Length >= 1024L * 1024L * 1024L * 4L Then Return
            Dim bCover As Byte() = My.Computer.FileSystem.ReadAllBytes(Data(0))
            pctCover.Image = Drawing.Image.FromStream(New IO.MemoryStream(bCover))
            pctCover.Tag = bCover
          Case Else
            e.Effect = DragDropEffects.None
        End Select
      Else
        e.Effect = DragDropEffects.None
      End If
    Else
      e.Effect = DragDropEffects.None
    End If
  End Sub

  Private Sub pctCover_DragEnter(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles pctCover.DragEnter
    e.Effect = DragDropEffects.All
  End Sub

  Private Sub pctCover_DragOver(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles pctCover.DragOver
    If e.Data.GetFormats(True).Contains("FileDrop") Then
      Dim Data = e.Data.GetData("FileDrop")
      If UBound(Data) = 0 Then
        Select Case IO.Path.GetExtension(Data(0)).ToLower
          Case ".jpg", ".jpeg", ".gif", ".png", ".bmp"
            e.Effect = DragDropEffects.Link
          Case Else
            e.Effect = DragDropEffects.None
        End Select
      Else
        e.Effect = DragDropEffects.None
      End If
    Else
      e.Effect = DragDropEffects.None
    End If
  End Sub

  Private Sub cArtwork_Progress(sender As Object, e As System.Net.DownloadProgressChangedEventArgs) Handles cArtwork.Progress
    If Me.InvokeRequired Then
      Me.Invoke(New Net.DownloadProgressChangedEventHandler(AddressOf cArtwork_Progress), sender, e)
      Return
    End If
    lblStatus.Text = "Downloading Art: " & ByteSize(e.BytesReceived) & "/" & ByteSize(e.TotalBytesToReceive) & " - " & e.ProgressPercentage & "%"
  End Sub
End Class

Friend Class AppleNet
  Private WithEvents wsApple As Net.WebClient
  Public Class CompleteEventArgs
    Inherits EventArgs
    Public Cover As Byte()
    Public Sub New(bCover As Byte())
      Cover = bCover
    End Sub
  End Class
  Public Event Complete(sender As Object, e As CompleteEventArgs)
  Public Class InfosEventArgs
    Inherits EventArgs
    Public ReleaseDate As String
    Public Genre As String
    Public Sub New(sReleased As String, sGenre As String)
      ReleaseDate = sReleased
      Genre = sGenre
    End Sub
  End Class
  Public Event Infos(sender As Object, e As InfosEventArgs)
  Public Class FailEventArgs
    Inherits EventArgs
    Public [Error] As Exception
    Public Sub New(e As Exception)
      [Error] = e
    End Sub
  End Class
  Public Event Failed(sender As Object, e As FailEventArgs)
  Public Class ChoicesEventArgs
    Inherits EventArgs
    Public Rows() As Generic.Dictionary(Of String, Object)
    Public Sub New(sData As String)
      Dim j As Generic.Dictionary(Of String, Object) = New Web.Script.Serialization.JavaScriptSerializer().Deserialize(Of Object)(sData)
      Dim rowCount As Integer = j("resultCount")
      Dim rowData() As Object = j("results")
      ReDim Rows(rowCount - 1)
      Dim I As Integer = 0
      For Each dItem In rowData
        Rows(I) = dItem
        I += 1
      Next
    End Sub
  End Class
  Public Event Choices(sender As Object, e As ChoicesEventArgs)
  Public Event Progress(sender As Object, e As Net.DownloadProgressChangedEventArgs)
  Private cSearch, cArtURL As String
  Private cArtTry As Integer = 0
  Private cQualTry As Integer = 0
  Private cTerm As Term
  Private cMaxArtSize As Integer
  Public Enum Term
    Title
    Artist
    Album
    Any
  End Enum
  Public Sub New(SearchTerm As String, Optional TermType As Term = Term.Any, Optional maxArtSize As Integer = 2048)
    cSearch = SearchTerm.Trim
    cTerm = TermType
    cMaxArtSize = maxArtSize
    wsApple = New Net.WebClient
    wsApple.Encoding = System.Text.Encoding.UTF8
    Dim tX As New Threading.Thread(New Threading.ThreadStart(AddressOf DownloadAsync))
    tX.Start()
  End Sub

  Private Sub DownloadAsync()
    Dim appleURI As Uri
    Select Case cTerm
      Case Term.Title
        appleURI = New Uri("http://itunes.apple.com/search?term=" & cSearch.Replace(" "c, "+"c) & "&media=music&entity=album&attribute=songTerm&limit=200")
      Case Term.Artist
        appleURI = New Uri("http://itunes.apple.com/search?term=" & cSearch.Replace(" "c, "+"c) & "&media=music&entity=album&attribute=artistTerm&limit=200")
      Case Term.Album
        appleURI = New Uri("http://itunes.apple.com/search?term=" & cSearch.Replace(" "c, "+"c) & "&media=music&entity=album&attribute=albumTerm&limit=200")
      Case Else
        appleURI = New Uri("http://itunes.apple.com/search?term=" & cSearch.Replace(" "c, "+"c) & "&media=music&entity=album&limit=200")
    End Select
    wsApple.DownloadStringAsync(appleURI)
  End Sub

  Private Sub wsApple_DownloadDataCompleted(sender As Object, e As System.Net.DownloadDataCompletedEventArgs) Handles wsApple.DownloadDataCompleted
    If e.Error Is Nothing Then
      RaiseEvent Complete(Me, New CompleteEventArgs(e.Result))
    Else
      If cArtURL IsNot Nothing Then
        Dim AttemptSizes() As Integer = {1500, 900, 600, 450, 400, 225, 200, 150, 100} '{".600x600-75.", ".450x450-75.", ".400x400-75.", "225x225-75", ".200x200-75.", "150x150-65", ".100x100-75."}
        Dim AttemptQualities() As Integer = {100, 90, 85, 75, 65}
        If cQualTry < AttemptQualities.Length - 1 Then
          wsApple.DownloadDataAsync(GetArtworkURI(cArtURL, AttemptSizes(cArtTry), AttemptQualities(cQualTry)))
          cQualTry += 1
        Else
          cQualTry = 0
          cArtTry += 1
          If cArtTry < AttemptSizes.Length - 1 Then
            wsApple.DownloadDataAsync(GetArtworkURI(cArtURL, AttemptSizes(cArtTry), AttemptQualities(cQualTry)))
            cQualTry += 1
          Else
            RaiseEvent Failed(Me, New FailEventArgs(e.Error))
          End If
        End If
      Else
        RaiseEvent Failed(Me, New FailEventArgs(e.Error))
      End If
    End If
  End Sub

  Private Function GetArtworkURI(Row As Generic.Dictionary(Of String, Object), Optional ByVal SetSize As Integer = 1500, Optional ByVal SetQuality As Integer = 100) As Uri
    Dim sURL As String = Row("artworkUrl100").ToString
    If String.IsNullOrEmpty(sURL) Then Return Nothing
    If sURL.Contains(".100x100-75.") Then
      sURL = sURL.Replace(".100x100-75.", "." & SetSize & "x" & SetSize & "-" & SetQuality & ".")
    ElseIf sURL.Contains("/100x100-85.") Then
      sURL = sURL.Replace("/100x100-85.", "/" & SetSize & "x" & SetSize & "-" & SetQuality & ".")
    ElseIf sURL.Contains("/100x100bb.") Then
      sURL = sURL.Replace("/100x100bb.", "/" & SetSize & "x" & SetSize & "-" & SetQuality & ".")
    ElseIf sURL.Contains("/100x100bb-85.") Then
      sURL = sURL.Replace("/100x100bb-85.", "/" & SetSize & "x" & SetSize & "-" & SetQuality & ".")
    Else
      Debug.Print("Unknown Artwork URI Type: " & sURL)
      Stop
    End If
    Return New Uri(sURL)
  End Function

  Private Function GetArtworkURI(sURL As String, Optional ByVal SetSize As Integer = 1500, Optional ByVal SetQuality As Integer = 100) As Uri
    If String.IsNullOrEmpty(sURL) Then Return Nothing
    If sURL.Contains(".100x100-75.") Then
      sURL = sURL.Replace(".100x100-75.", "." & SetSize & "x" & SetSize & "-" & SetQuality & ".")
    ElseIf sURL.Contains("/100x100-85.") Then
      sURL = sURL.Replace("/100x100-85.", "/" & SetSize & "x" & SetSize & "-" & SetQuality & ".")
    ElseIf sURL.Contains("/100x100bb.") Then
      sURL = sURL.Replace("/100x100bb.", "/" & SetSize & "x" & SetSize & "-" & SetQuality & ".")
    ElseIf sURL.Contains("/100x100bb-85.") Then
      sURL = sURL.Replace("/100x100bb-85.", "/" & SetSize & "x" & SetSize & "-" & SetQuality & ".")
    Else
      Debug.Print("Unknown Artwork URI Type: " & sURL)
      Stop
    End If
    Return New Uri(sURL)
  End Function

  Private Function GetDate(Row As Generic.Dictionary(Of String, Object)) As String
    Dim sDate As String = Row("releaseDate").ToString
    If String.IsNullOrEmpty(sDate) Then Return Nothing
    sDate = sDate.Substring(0, sDate.IndexOf("-"))
    Return sDate
  End Function

  Private Function GetGenre(Row As Generic.Dictionary(Of String, Object)) As String
    Dim sGenre As String = Row("primaryGenreName").ToString
    If String.IsNullOrEmpty(sGenre) Then Return Nothing
    Return sGenre
  End Function

  Public Sub ChooseRow(InfoLine As Generic.Dictionary(Of String, Object))
    If Not InfoLine("artworkUrl100") Is Nothing Then
      cArtURL = InfoLine("artworkUrl100").ToString
      cArtTry = 0
      cQualTry = 0
      wsApple.DownloadDataAsync(GetArtworkURI(InfoLine))
      RaiseEvent Infos(Me, New InfosEventArgs(GetDate(InfoLine), GetGenre(InfoLine)))
    End If
  End Sub

  Private Sub wsApple_DownloadProgressChanged(sender As Object, e As System.Net.DownloadProgressChangedEventArgs) Handles wsApple.DownloadProgressChanged
    If e.TotalBytesToReceive > 0 Then
      RaiseEvent Progress(Me, e)
    End If
  End Sub

  Private Sub wsApple_DownloadStringCompleted(sender As Object, e As System.Net.DownloadStringCompletedEventArgs) Handles wsApple.DownloadStringCompleted
    If e.Error Is Nothing Then
      RaiseEvent Choices(Me, New ChoicesEventArgs(e.Result))
    Else
      RaiseEvent Failed(Me, New FailEventArgs(e.Error))
    End If
  End Sub
End Class

Friend Class CDDBNet
  Private WithEvents wsSocket As TCPWrapper
  Private mID As String
  Private lState As Integer
  Private mEntryData As String
  Private mEntryCategory As String
  Private mEntryID As String
  Event FindError(Message As String)
  Event Found(Data As String)
  Event Finding(Artist As String, ByVal Album As String, ByVal Genre As String)
  Event WriteError(Message As String)
  Event Written()
  Event Finished()
  Event ReadOnlyConnection(IsReadOnly As Boolean)

  Private Function FreeDBIP() As Net.IPAddress
    Dim freeDBAddrs() = Net.Dns.GetHostEntry("freedb.freedb.org").AddressList
    For Each addr In freeDBAddrs
      If addr.AddressFamily = Net.Sockets.AddressFamily.InterNetwork Then
        Return addr
      End If
    Next
    Return New Net.IPAddress(&H26D8D6C3)
  End Function

  Public Sub New()
    wsSocket = New TCPWrapper
    Try
      wsSocket.RemoteEndPoint = New System.Net.IPEndPoint(FreeDBIP, 8880)
      lState = 0
      wsSocket.Connect()
    Catch ex As Exception
      RaiseEvent FindError("Unable to connect")
    End Try
  End Sub

  Public Sub FindDisc(DiscID As String)
    mID = DiscID
    If wsSocket Is Nothing OrElse Not wsSocket.IsConnected Then
      wsSocket = New TCPWrapper
      wsSocket.RemoteEndPoint = New System.Net.IPEndPoint(FreeDBIP, 8880)
      lState = 0
      wsSocket.Connect()
    Else
      If lState = 0 Then
        Login()
      Else
        lState = 2
        SendData("cddb query " & mID)
      End If
    End If
  End Sub

  Public Function WriteEntry(Category As String, ByVal DiscID As String, ByVal EntryData As String) As Boolean
    If wsSocket Is Nothing OrElse Not wsSocket.IsConnected Then
      mEntryData = EntryData
      mEntryCategory = Category
      mEntryID = DiscID
      mID = Nothing
      wsSocket = New TCPWrapper
      wsSocket.RemoteEndPoint = New System.Net.IPEndPoint(FreeDBIP, 8880)
      lState = 0
      wsSocket.Connect()
      Return True
    Else
      If lState = 0 Then
        mEntryData = EntryData
        mEntryCategory = Category
        mEntryID = DiscID
        Login()
        Return True
      Else
        If String.IsNullOrEmpty(mEntryData) Then
          mEntryData = EntryData
          mEntryCategory = Category
          mEntryID = DiscID
          lState = 4
          SendData("cddb write " & Category & " " & DiscID)
          Return True
        Else
          Return False
        End If
      End If
    End If
  End Function

  Private Sub Login()
    If lState = 0 Then
      lState = 1
      SendData("cddb hello " & Environment.UserName.Replace(" "c, "_"c) & " " & Environment.MachineName.Replace(" "c, "_"c) & " " & Application.ProductName.Replace(" "c, "_"c) & " " & Application.ProductVersion)
    End If
  End Sub

  Private Sub HandleData(sData As String)
    Static sBuffer As String
    sBuffer &= sData
    If sBuffer.Contains(vbCrLf & "." & vbCrLf) Or sBuffer.EndsWith(vbCrLf) Then
      Select Case lState
        Case 0 'Login
          Select Case sBuffer.Substring(0, 3)
            Case "200"
              sBuffer = String.Empty
              RaiseEvent ReadOnlyConnection(False)
              Login()
            Case "201"
              sBuffer = String.Empty
              RaiseEvent ReadOnlyConnection(True)
              Login()
            Case "432"
              sBuffer = String.Empty
              RaiseEvent FindError("Permission denied")
            Case "433"
              sBuffer = String.Empty
              RaiseEvent FindError("Too many users")
            Case "434"
              sBuffer = String.Empty
              RaiseEvent FindError("Server busy")
            Case Else
              Debug.Print("Unknown Welcome Response: " & sBuffer)
              sBuffer = String.Empty
          End Select
        Case 1
          Select Case sBuffer.Substring(0, 3)
            Case "200"
              sBuffer = String.Empty
              lState = 2
              If Not String.IsNullOrEmpty(mID) Then
                SendData("cddb query " & mID)
              ElseIf Not String.IsNullOrEmpty(mEntryID) Then
                lState = 4
                SendData("cddb write " & mEntryCategory & " " & mEntryID)
              End If
            Case "431"
              sBuffer = String.Empty
              RaiseEvent FindError("Failed to login")
            Case Else
              Debug.Print("Unknown Handshake Response: " & sBuffer)
              sBuffer = String.Empty
          End Select
        Case 2
          Select Case sBuffer.Substring(0, 3)
            Case "200"
              Dim vals() As String = Split(sBuffer.Substring(0, sBuffer.IndexOf(vbCr)), " ", 4)
              sBuffer = String.Empty
              Dim Cat As String = vals(1)
              Dim dID As String = vals(2)
              Dim Title As String = Split(vals(3), " / ")(1)
              Dim Artist As String = Split(vals(3), " / ")(0)
              RaiseEvent Finding(Artist, Title, Cat)
              lState = 3
              SendData("cddb read " & Cat & " " & dID)
            Case "211"
              Dim sList As String = sBuffer.Substring(sBuffer.IndexOf(vbCr) + 2)
              sBuffer = String.Empty
              sList = sList.Substring(0, sList.Length - 5)
              If sList.Contains(vbNewLine) Then
                Dim matches() As String = Split(sList, vbNewLine)
                Dim chosen As Integer = 0
                Debug.Print(sBuffer)
                sList = matches(chosen)
              End If
              Dim vals() As String = Split(sList, " ", 3)
              Dim Cat As String = vals(0)
              Dim dID As String = vals(1)
              Dim Title As String = Split(vals(2), " / ")(1)
              Dim Artist As String = Split(vals(2), " / ")(0)
              RaiseEvent Finding(Artist, Title, Cat)
              lState = 3
              SendData("cddb read " & Cat & " " & dID)
            Case "202"
              sBuffer = String.Empty
              RaiseEvent FindError("No match found")
            Case "403"
              sBuffer = String.Empty
              RaiseEvent FindError("Database entry corrupt!")
            Case "409"
              sBuffer = String.Empty
              RaiseEvent FindError("No Handshake")
              lState = 0
              Login()
            Case Else
              Debug.Print("Unknown Query Response: " & sBuffer)
              sBuffer = String.Empty
          End Select
        Case 3
          Select Case sBuffer.Substring(0, 3)
            Case "210"
              'Debug.Print(sBuffer)
              Dim sContents As String = sBuffer.Substring(sBuffer.IndexOf(vbCr) + 2)
              sBuffer = String.Empty
              sContents = sContents.Substring(0, sContents.Length - 2)
              RaiseEvent Found(sContents)
              lState = 255
              SendData("quit")
            Case "401"
              sBuffer = String.Empty
              RaiseEvent FindError("Entry not found")
            Case "402"
              sBuffer = String.Empty
              RaiseEvent FindError("Server error")
            Case "403"
              sBuffer = String.Empty
              RaiseEvent FindError("Database entry corrupt!")
            Case "409"
              sBuffer = String.Empty
              RaiseEvent FindError("No Handshake")
              lState = 0
              Login()
            Case Else
              Debug.Print("Unknown Read Entry Response: " & sBuffer)
              sBuffer = String.Empty
          End Select
        Case 4
          Select Case sBuffer.Substring(0, 3)
            Case "320"
              sBuffer = String.Empty
              If String.IsNullOrEmpty(mEntryData) Then
                RaiseEvent WriteError("No Entry to Write")
              Else
                lState = 5
                SendData(mEntryData & IIf(mEntryData.EndsWith(vbCrLf), ".", vbCrLf & "."))
              End If
            Case "401"
              sBuffer = String.Empty
              'mEntry = String.Empty
              RaiseEvent WriteError("Permission Denied")
              lState = 10
              SendData("validate")
            Case "402"
              sBuffer = String.Empty
              mEntryData = String.Empty
              RaiseEvent WriteError("Server Full/File Access Denied")
            Case "409"
              sBuffer = String.Empty
              RaiseEvent WriteError("No Handshake")
              lState = 0
              Login()
            Case Else
              Debug.Print("Unknown Write Response: " & sBuffer)
              sBuffer = String.Empty
          End Select
        Case 5
          Select Case sBuffer.Substring(0, 3)
            Case "200"
              sBuffer = String.Empty
              mEntryData = String.Empty
              RaiseEvent Written()
            Case "501"
              RaiseEvent WriteError("Entry Rejected: " & sBuffer.Substring(4))
              mEntryData = String.Empty
              sBuffer = String.Empty
            Case Else
              Debug.Print("Unknown Write Entry Response: " & sBuffer)
              mEntryData = String.Empty
              sBuffer = String.Empty
          End Select
        Case 10
          Select Case sBuffer.Substring(0, 3)
            Case "503"
              sBuffer = String.Empty
              Debug.Print("Validation not required")
              If String.IsNullOrEmpty(mEntryData) Then
                RaiseEvent WriteError("No Entry to Write")
              Else
                lState = 4
                SendData("cddb write " & mEntryCategory & " " & mEntryID)
              End If
            Case "320"
              Debug.Print(sBuffer)
          End Select
        Case 255
          sBuffer = String.Empty
          RaiseEvent Finished()
        Case Else
          Debug.Print("Unknown Status: [" & lState & "] " & sBuffer)
          sBuffer = String.Empty
      End Select
    End If
  End Sub

  Private Sub wsSocket_SocketReceived(sender As Object, e As SocketReceivedEventArgs) Handles wsSocket.SocketReceived
    Dim sTmp As String = System.Text.Encoding.GetEncoding(latin_1).GetString(e.Data)
    sTmp = sTmp.Substring(0, sTmp.IndexOf(vbNullChar))
    'Debug.Print(sTmp)
    HandleData(sTmp)
  End Sub

  Private Sub SendData(Message As String)
    'Debug.Print(Message)
    Dim bSend As Byte() = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes(Message & vbCrLf)
    wsSocket.Send(bSend)
  End Sub
End Class
