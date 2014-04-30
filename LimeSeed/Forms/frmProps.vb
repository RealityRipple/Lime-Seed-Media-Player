﻿Public Class frmProps
  Private mpPreview As Seed.ctlSeed
  Private sFile As String

  Private Sub lstFilters_DoubleClick(sender As Object, e As System.EventArgs) Handles lstFilters.DoubleClick
    cmdFilterProps.PerformClick()
  End Sub

  Private Sub cmdFilterProps_Click(sender As System.Object, e As System.EventArgs) Handles cmdFilterProps.Click
    If String.Compare(CType(Me.Tag, frmMain).mpPlayer.FileName, mpPreview.FileName, True) = 0 Then
      If Not CType(Me.Tag, frmMain).mpPlayer.ShowProperties(lstFilters.Text, Me.Handle) Then MsgBox("This filter has no properties.", vbInformation, "No Properties")
    Else
      If Not mpPreview.ShowProperties(lstFilters.Text, Me.Handle) Then MsgBox("This filter has no properties.", vbInformation, "No Properties")
    End If
  End Sub

  Private Function GenerateRow(Label As String, ByVal Value As String, ByVal Tag As String, Optional MaxInputLength As Integer = 32767) As DataGridViewRow
    Dim newRow As New DataGridViewRow
    newRow.Cells.Add(New DataGridViewTextBoxCell With {.Value = Label & ":"})
    newRow.Cells.Add(New DataGridViewTextBoxCell With {.Value = Value, .MaxInputLength = MaxInputLength})
    newRow.Tag = Tag
    Return newRow
  End Function

  Private Function GenerateLinkRow(Label As String, ByVal Value As String, URL As String, Tag As String) As DataGridViewRow
    Dim newRow As New DataGridViewRow
    newRow.Cells.Add(New DataGridViewTextBoxCell With {.Value = Label & ":"})
    Dim newCell As New DataGridViewLinkCell With {.Value = Value, .Tag = URL, .LinkBehavior = LinkBehavior.HoverUnderline, .TrackVisitedState = False, .ToolTipText = URL}
    newRow.Cells.Add(newCell)
    newRow.Tag = Tag
    Return newRow
  End Function

  Private Function GenerateComboRow(Label As String, ByVal Value As String, ByVal Items() As String, Tag As String) As DataGridViewRow
    Dim newRow As New DataGridViewRow
    newRow.Cells.Add(New DataGridViewTextBoxCell With {.Value = Label & ":"})
    Dim newCell As New DataGridViewComboBoxCell
    newCell.Items.AddRange(Items)
    If Not newCell.Items.Contains(Value) Then newCell.Items.Add(Value)
    newCell.Value = Value
    newRow.Cells.Add(newCell)
    newRow.Tag = Tag
    Return newRow
  End Function

  Private Function GenerateGenres() As String()
    Static sGenres(&H93) As String
    If String.IsNullOrEmpty(sGenres(0)) Then
      For I As Byte = 0 To &H93
        sGenres(I) = Seed.clsID3v1.GenreName(I)
      Next
    End If
    Return sGenres
  End Function

  Private Function LoadID3v1() As Boolean
    dgvID3v1.Rows.Clear()
    Using ID3v1Tags As New Seed.clsID3v1(sFile)
      If ID3v1Tags.HasID3v1Tag Then
        dgvID3v1.Rows.Add(GenerateRow("Track", ID3v1Tags.Track, Nothing, 3))
        dgvID3v1.Rows.Add(GenerateRow("Title", ID3v1Tags.Title, Nothing, 30))
        dgvID3v1.Rows.Add(GenerateRow("Artist", ID3v1Tags.Artist, Nothing, 30))
        dgvID3v1.Rows.Add(GenerateRow("Album", ID3v1Tags.Album, Nothing, 30))
        dgvID3v1.Rows.Add(GenerateComboRow("Genre", Seed.clsID3v1.GenreName(ID3v1Tags.Genre), GenerateGenres, ID3v1Tags.Genre))
        dgvID3v1.Rows.Add(GenerateRow("Year", ID3v1Tags.Year, Nothing, 4))
        dgvID3v1.Rows.Add(GenerateRow("Comment", ID3v1Tags.Comment, Nothing, IIf(ID3v1Tags.Track = 0, 30, 28)))
        Return True
      Else
        dgvID3v1.Rows.Add(GenerateRow("Track", Nothing, Nothing, 3))
        dgvID3v1.Rows.Add(GenerateRow("Title", Nothing, Nothing, 30))
        dgvID3v1.Rows.Add(GenerateRow("Artist", Nothing, Nothing, 30))
        dgvID3v1.Rows.Add(GenerateRow("Album", Nothing, Nothing, 30))
        dgvID3v1.Rows.Add(GenerateComboRow("Genre", "Other", GenerateGenres, &HC))
        dgvID3v1.Rows.Add(GenerateRow("Year", Nothing, Nothing, 30))
        dgvID3v1.Rows.Add(GenerateRow("Comment", Nothing, Nothing, 30))
        Return False
      End If
    End Using
  End Function

  Private Function LoadID3v2() As Boolean
    dgvID3v2.Rows.Clear()
    Using ID3v2Tags As New Seed.clsID3v2(sFile)
      If ID3v2Tags.HasID3v2Tag Then
        dgvID3v2.Rows.Add(GenerateComboRow("Version", ID3v2Tags.ID3v2Ver, {"2.2.0", "2.3.0", "2.4.0"}, Nothing))
        Dim bPic As Boolean = False
        For I As Integer = 0 To ID3v2Tags.FrameCount - 1
          Dim sFName As String = ID3v2Tags.FrameName(I)
          Dim sFrame As String = Seed.clsID3v2.GetFrameName(sFName)
          Dim fData() As Object = ID3v2Tags.ParseFrame(sFName, ID3v2Tags.FrameData(I))
          If sFName.StartsWith("T") Or sFName.StartsWith("W") Then
            For J As Integer = 0 To fData.Length - 1
              fData(J) = fData(J).Replace(vbCr, vbNewLine)
            Next
          End If
          Select Case sFName
            Case "TXX", "TXXX"
              If fData.Length = 2 Then
                dgvID3v2.Rows.Add(GenerateRow(fData(0), fData(1), sFName))
              Else
                dgvID3v2.Rows.Add(GenerateRow(sFrame, fData(0), sFName))
              End If
            Case "WXX", "WXXX"
              If fData.Length = 2 Then
                dgvID3v2.Rows.Add(GenerateLinkRow(fData(0), fData(1), fData(1), sFName))
              Else
                dgvID3v2.Rows.Add(GenerateLinkRow(sFrame, fData(0), fData(0), sFName))
              End If
            Case "GEO", "GEOB"
              dgvID3v2.Rows.Add(GenerateRow(sFrame, "MIME Type: " & fData(0) & vbNewLine & "File Name: " & fData(1) & vbNewLine & "Description: " & fData(2) & vbNewLine & "Content: " & fData(3), sFName))
            Case "PIC", "APIC"
              Dim picRow As New DataGridViewRow
              picRow.Cells.Add(New DataGridViewTextBoxCell With {.Value = "Pictures:"})
              Dim picButton As New DataGridViewButtonCell With {.Value = "View Pictures"}
              picRow.Cells.Add(picButton)
              dgvID3v2.Rows.Add(picRow)
              Dim sTmpPath As String = IO.Path.GetTempFileName
              My.Computer.FileSystem.WriteAllText(sTmpPath, fData(3), False, System.Text.Encoding.GetEncoding("latin1"))
              Try
                Using imgTmp As Drawing.Image = PathToImg(sTmpPath)
                  pctPreview.Image = imgTmp.Clone
                  picButton.Tag = {fData(0), fData(1), fData(2), imgTmp.Clone} 'MIME / Description / 0xID / Image
                End Using
                pctPreview.SizeMode = PictureBoxSizeMode.Zoom
                txtVideoSize.Text = "No video"
                bPic = True
              Catch ex As Exception
                Debug.Print("Image Error: " & ex.Message)
                bPic = False
              Finally
                My.Computer.FileSystem.DeleteFile(sTmpPath)
              End Try
            Case "PRIV"
              If fData(0).Contains(vbNullChar) Then
                Dim sSplit() As String = Split(fData(0), vbNullChar, 2)
                If String.IsNullOrEmpty(sSplit(0)) Then
                  dgvID3v2.Rows.Add(GenerateRow(sFrame, sSplit(1), sFName))
                Else
                  If sSplit(0).StartsWith("WM/") Then
                    If sSplit(0).EndsWith("ID") Then
                      dgvID3v2.Rows.Add(GenerateRow(sFrame & " [" & sSplit(0) & "]", "{" & New Guid(System.Text.Encoding.GetEncoding("latin1").GetBytes(sSplit(1))).ToString & "}", sFName))
                    Else
                      Dim sUni As String = System.Text.Encoding.Unicode.GetString((System.Text.Encoding.GetEncoding("latin1").GetBytes(sSplit(1))))
                      Do While sUni.EndsWith(vbNullChar)
                        sUni = sUni.Substring(0, sUni.Length - 1)
                      Loop
                      dgvID3v2.Rows.Add(GenerateRow(sFrame & " [" & sSplit(0) & "]", sUni, sFName))
                    End If
                  Else
                    dgvID3v2.Rows.Add(GenerateRow(sFrame & " [" & sSplit(0) & "]", BitConverter.ToString(System.Text.Encoding.GetEncoding("latin1").GetBytes(sSplit(1))), sFName))
                  End If
                End If
              Else
                dgvID3v2.Rows.Add(GenerateRow(sFrame, fData(0), sFName))
              End If
            Case "ULT", "USLT"
              If fData.Length = 2 Then
                dgvID3v2.Rows.Add(GenerateRow(sFrame & " [" & fData(0) & "]", fData(1).ToString.Replace(vbCr, vbNewLine), sFName))
              Else
                dgvID3v2.Rows.Add(GenerateRow(sFrame, fData(0).ToString.Replace(vbCr, vbNewLine), sFName))
              End If
            Case Else
              If sFName.StartsWith("T") Then
                dgvID3v2.Rows.Add(GenerateRow(sFrame, fData(0), sFName))
              ElseIf sFName.StartsWith("W") Then
                dgvID3v2.Rows.Add(GenerateLinkRow(sFrame, fData(0), fData(0), sFName))
              Else
                dgvID3v2.Rows.Add(GenerateRow(sFrame, fData(0), sFName))
              End If
          End Select
        Next
        Dim iIndex As Integer = 1
        For Each sItem As String In ({"Track:", "Title:", "Artist:", "Album:", "Genre:", "Year:"})
          For I As Integer = 0 To dgvID3v2.Rows.Count - 1
            Dim cellI As DataGridViewTextBoxCell = dgvID3v2.Rows(I).Cells(0)
            If CType(cellI.Value, String) = sItem Then
              Dim newRow As DataGridViewRow = dgvID3v2.Rows(I)
              dgvID3v2.Rows.Remove(newRow)
              dgvID3v2.Rows.Insert(iIndex, newRow)
              iIndex += 1
              Exit For
            End If
          Next
        Next

        If Not bPic Then
          If My.Computer.FileSystem.FileExists(txtFilePath.Text & "\Folder.jpg") Then
            Using bmpTmp As Drawing.Bitmap = PathToImg(txtFilePath.Text & "\Folder.jpg")
              pctPreview.Image = bmpTmp.Clone
              pctPreview.SizeMode = PictureBoxSizeMode.Zoom
              txtVideoSize.Text = "No video"
            End Using
          End If
        End If
        Return True
      Else
        If My.Computer.FileSystem.FileExists(txtFilePath.Text & "\Folder.jpg") Then
          Using bmpTmp As Drawing.Bitmap = PathToImg(txtFilePath.Text & "\Folder.jpg")
            pctPreview.Image = bmpTmp.Clone
            pctPreview.SizeMode = PictureBoxSizeMode.Zoom
            txtVideoSize.Text = "No video"
          End Using
        End If
        Return False
      End If
    End Using
  End Function

  Private Function LoadMPEG() As Boolean
    dgvMPEG.Rows.Clear()
    Using cHeader As New Seed.clsHeaderLoader(sFile)
      If cHeader.cMPEG IsNot Nothing AndAlso cHeader.cMPEG.CheckValidity Then
        dgvMPEG.Rows.Add({"Version:", "MPEG-" & cHeader.cMPEG.GetMPEGVer & " Layer-" & StrDup(cHeader.cMPEG.GetMPEGLayer, "I")})
        dgvMPEG.Rows.Add({"Bitrate:", KRater(cHeader.cMPEG.GetBitrate, "bps") & " " & cHeader.RateFormat})
        If String.IsNullOrEmpty(cHeader.cXING.HeaderID) And String.IsNullOrEmpty(cHeader.cVBRI.HeaderID) Then
          'default
          Dim lLen As Long = My.Computer.FileSystem.GetFileInfo(sFile).Length - (IIf(cHeader.cID3v1.HasID3v1Tag, &H80, 0) + IIf(cHeader.cID3v2.HasID3v2Tag, cHeader.cID3v2.ID3v2Len, 0))
          dgvMPEG.Rows.Add({" Actual: ", KRater(lLen * 8 / cHeader.Duration, "bps")})
        ElseIf String.IsNullOrEmpty(cHeader.cXING.HeaderID) Then
          'vbri
          dgvMPEG.Rows.Add({" Actual: ", KRater(cHeader.cVBRI.Bytes * 8 / cHeader.Duration, "bps")})
        ElseIf String.IsNullOrEmpty(cHeader.cVBRI.HeaderID) Then
          'xing
          dgvMPEG.Rows.Add({" Actual: ", KRater(cHeader.cXING.MusicLength * 8 / cHeader.Duration, "bps")})
        Else
          'both?
          Dim dXING As Double = cHeader.cXING.MusicLength * 8 / cHeader.Duration
          Dim dVBRI As Double = cHeader.cVBRI.Bytes * 8 / cHeader.Duration
          dgvMPEG.Rows.Add({" Actual: ", KRater(dXING * dVBRI / 2, "bps")})
        End If
        dgvMPEG.Rows.Add({"Frames:", cHeader.Frames})
        dgvMPEG.Rows.Add({"Sample Rate:", KRater(cHeader.cMPEG.GetSampleRate, "Hz")})
        dgvMPEG.Rows.Add({"Channels:", cHeader.cMPEG.GetChannels})
        dgvMPEG.Rows.Add({"Duration:", ConvertTimeVal(cHeader.Duration)})
        If cHeader.cMPEG.GetChannels = "Joint Stereo" And Not String.IsNullOrEmpty(cHeader.cMPEG.GetModeExtension) Then dgvMPEG.Rows.Add({"Extended Data:", cHeader.cMPEG.GetModeExtension})



        If Not String.IsNullOrEmpty(cHeader.cXING.HeaderID) Then
          dgvMPEG.Rows.Add({"- Xing Tag -", ""})
          dgvMPEG.Rows.Add({"   Header ID:", cHeader.cXING.HeaderID})
          dgvMPEG.Rows.Add({"   Frames:", cHeader.cXING.FrameCount})
          dgvMPEG.Rows.Add({"   Size:", ByteSize(cHeader.cXING.ByteCount)})
          dgvMPEG.Rows.Add({"   Quality:", cHeader.cXING.Quality})
          dgvMPEG.Rows.Add({"   Encoder:", cHeader.cXING.EncoderVersion})
          dgvMPEG.Rows.Add({"   Revision:", cHeader.cXING.InfoTagRevision})
          dgvMPEG.Rows.Add({"   Method: ", cHeader.cXING.VBRMethod})
          dgvMPEG.Rows.Add({"   Lowpass Filter:", KRater(cHeader.cXING.LowpassFilter, "Hz")})
          dgvMPEG.Rows.Add({"   Peak Amplitude:", cHeader.cXING.PeakSignalAmplitude})
          dgvMPEG.Rows.Add({"   Replay Gain 1:", cHeader.cXING.RadioReplayGain})
          dgvMPEG.Rows.Add({"   Replay Gain 2:", cHeader.cXING.AudiophileReplayGain})
          dgvMPEG.Rows.Add({"   Encoding Flags:", cHeader.cXING.EncodingFlags})
          dgvMPEG.Rows.Add({"   ATH Type:", cHeader.cXING.ATHType})
          dgvMPEG.Rows.Add({"   Minimal Bitrate:", KRater(cHeader.cXING.MinimalBitrate * 1000, "bps")})
          dgvMPEG.Rows.Add({"   Start Delay:", Math.Round(cHeader.cXING.StartEncoderDelay / 1152 * 0.026, 3) & " seconds"})
          dgvMPEG.Rows.Add({"   End Delay:", Math.Round(cHeader.cXING.EndEncoderDelay / 1152 * 0.026, 3) & " seconds"})
          dgvMPEG.Rows.Add({"   Sharpening:", cHeader.cXING.NoiseSharpening})
          dgvMPEG.Rows.Add({"   Stereo Mode:", cHeader.cXING.StereoMode})
          dgvMPEG.Rows.Add({"   Unwise Setting:", cHeader.cXING.UnwiseSetting})
          Select Case cHeader.cXING.SourceFrequency
            Case "<=32000" : dgvMPEG.Rows.Add({"   Source Freq:", "<= 32 KHz"})
            Case "44100" : dgvMPEG.Rows.Add({"   Source Freq:", "44.1 KHz"})
            Case "48000" : dgvMPEG.Rows.Add({"   Source Freq:", "48 KHz"})
            Case ">48000" : dgvMPEG.Rows.Add({"   Source Freq:", ">48 KHz"})
          End Select
          dgvMPEG.Rows.Add({"   MP3 Gain:", cHeader.cXING.MP3Gain & "dB"})
          dgvMPEG.Rows.Add({"   Surround Info:", cHeader.cXING.SurroundInfo})
          dgvMPEG.Rows.Add({"   Preset:", cHeader.cXING.PresetUsed})
          dgvMPEG.Rows.Add({"   Music Size:", ByteSize(cHeader.cXING.MusicLength)})
          dgvMPEG.Rows.Add({"   Music CRC:", Hex$(cHeader.cXING.MusicCRC)})
          dgvMPEG.Rows.Add({"   Info Tag CRC:", Hex$(cHeader.cXING.InfoTagCRC)})
        End If
        If Not String.IsNullOrEmpty(cHeader.cVBRI.HeaderID) Then
          dgvMPEG.Rows.Add({"- Fraunhofer Tag -", ""})
          dgvMPEG.Rows.Add({"   Header ID:", cHeader.cVBRI.HeaderID})
          dgvMPEG.Rows.Add({"   Version:", cHeader.cVBRI.Version})
          dgvMPEG.Rows.Add({"   Delay:", cHeader.cVBRI.Delay})
          dgvMPEG.Rows.Add({"   Quality:", cHeader.cVBRI.Quality})
          dgvMPEG.Rows.Add({"   Size:", ByteSize(cHeader.cVBRI.Bytes)})
          dgvMPEG.Rows.Add({"   Frames:", cHeader.cVBRI.Frames})
        End If
        Return True
      Else
        Return False
      End If
    End Using
  End Function

  Private Function LoadVorbis() As Boolean
    dgvVorbis.Rows.Clear()
    Using cVorbis As New Seed.clsVorbis(sFile)
      If cVorbis.HasVorbis Then
        If Not String.IsNullOrEmpty(cVorbis.Track) Then dgvVorbis.Rows.Add({"Track:", cVorbis.Track})
        If Not String.IsNullOrEmpty(cVorbis.Title) Then dgvVorbis.Rows.Add({"Title:", cVorbis.Title})
        If Not String.IsNullOrEmpty(cVorbis.Artist) Then dgvVorbis.Rows.Add({"Artist:", cVorbis.Artist})
        If Not String.IsNullOrEmpty(cVorbis.Album) Then dgvVorbis.Rows.Add({"Album:", cVorbis.Album})
        If Not String.IsNullOrEmpty(cVorbis.Genre) Then dgvVorbis.Rows.Add({"Genre:", cVorbis.Genre})
        If Not String.IsNullOrEmpty(cVorbis.RecordDate) Then dgvVorbis.Rows.Add({"Date:", cVorbis.RecordDate})
        If Not String.IsNullOrEmpty(cVorbis.Version) Then dgvVorbis.Rows.Add({"Version:", cVorbis.Version})
        If Not String.IsNullOrEmpty(cVorbis.Performer) Then dgvVorbis.Rows.Add({"Performer:", cVorbis.Performer})
        If Not String.IsNullOrEmpty(cVorbis.Copyright) Then dgvVorbis.Rows.Add({"Copyright:", cVorbis.Copyright})
        If Not String.IsNullOrEmpty(cVorbis.License) Then dgvVorbis.Rows.Add({"License:", cVorbis.License})
        If Not String.IsNullOrEmpty(cVorbis.Label) Then dgvVorbis.Rows.Add({"Record Label:", cVorbis.Label})
        If Not String.IsNullOrEmpty(cVorbis.Description) Then dgvVorbis.Rows.Add({"Description:", cVorbis.Description})
        If Not String.IsNullOrEmpty(cVorbis.Location) Then dgvVorbis.Rows.Add({"Location:", cVorbis.Location})
        If Not String.IsNullOrEmpty(cVorbis.Contact) Then dgvVorbis.Rows.Add({"Contact:", cVorbis.Contact})
        If Not String.IsNullOrEmpty(cVorbis.ISRC) Then dgvVorbis.Rows.Add({"ISRC:", cVorbis.ISRC})
        If Not String.IsNullOrEmpty(cVorbis.Other) Then
          Dim sOther() As String = Split(cVorbis.Other, vbNewLine)
          For Each sItem In sOther
            dgvVorbis.Rows.Add({StrConv(Split(sItem, ": ", 2)(0), VbStrConv.ProperCase) & ":", Split(sItem, ": ", 2)(1)})
          Next
        End If
        dgvVorbis.Rows.Add({"- File Info -", ""})
        If cVorbis.File_Version > 0 Then dgvVorbis.Rows.Add({"   Version:", cVorbis.File_Version})
        If cVorbis.File_Channels > 0 Then
          Dim sChannel As String = cVorbis.File_Channels
          Select Case cVorbis.File_Channels
            Case 1 : sChannel = "Mono"
            Case 2 : sChannel = "Stereo"
            Case 3 : sChannel = "2.1 Stereo"
            Case 4 : sChannel = "Quadraphonic"
            Case 5 : sChannel = "Surround"
            Case 6 : sChannel = "5.1 Surround"
            Case 7 : sChannel = "6.1 Surround"
            Case 8 : sChannel = "7.1 Surround"
          End Select
          dgvVorbis.Rows.Add({"   Channels:", sChannel})
        End If
        dgvVorbis.Rows.Add({"   Minimum Bitrate:", KRater(cVorbis.File_MinQuality, "bps")})
        dgvVorbis.Rows.Add({"   Nominal Bitrate:", KRater(cVorbis.File_Quality, "bps")})
        dgvVorbis.Rows.Add({"   Maximum Bitrate:", KRater(cVorbis.File_MaxQuality, "bps")})
        Dim lLen As Long = My.Computer.FileSystem.GetFileInfo(sFile).Length - (IIf(cVorbis.HasVorbis, cVorbis.HeaderLength, 0))
        dgvVorbis.Rows.Add({"   Actual Bitrate: ", KRater(lLen * 8 / mpPreview.GetFileDuration(sFile), "bps")})

        dgvVorbis.Rows.Add({"   Sample Rate:", KRater(cVorbis.File_Rate, "Hz")})
        If Not String.IsNullOrEmpty(cVorbis.File_Vendor) Then dgvVorbis.Rows.Add({"   Vendor:", cVorbis.File_Vendor})
        If cVorbis.Pictures IsNot Nothing Then
          pctPreview.Image = cVorbis.Pictures(0).Image
          For Each Pic In cVorbis.Pictures
            Dim picRow As New DataGridViewRow
            picRow.Cells.Add(New DataGridViewTextBoxCell With {.Value = "Pictures:"})
            Dim picButton As New DataGridViewButtonCell With {.Value = "View Pictures"}
            picRow.Cells.Add(picButton)
            dgvVorbis.Rows.Add(picRow)
            picButton.Tag = {Pic.MIME, Pic.Descr, Pic.PicType, Pic.Image} 'MIME / Description / 0xID / Image
          Next
        End If
        Return True
      End If
    End Using
    Return False
  End Function

  Private Function LoadMKV() As Boolean
    tvMKV.Nodes.Clear()
    Try
      Using mkvInfo As New Seed.clsMKVHeaders(sFile)
        If mkvInfo.HasMKV Then
          Dim nodeEBML = tvMKV.Nodes.Add("EBML Header")

          If mkvInfo.EBMLHead.Version > 0 Then nodeEBML.Nodes.Add("Version: " & mkvInfo.EBMLHead.Version)
          If mkvInfo.EBMLHead.ReadVersion > 0 Then nodeEBML.Nodes.Add("Read Version: " & mkvInfo.EBMLHead.ReadVersion)
          If mkvInfo.EBMLHead.MaxIDLength > 0 Then nodeEBML.Nodes.Add("Max ID Length: " & mkvInfo.EBMLHead.MaxIDLength)
          If mkvInfo.EBMLHead.MaxSizeLength > 0 Then nodeEBML.Nodes.Add("Max Size Length: " & mkvInfo.EBMLHead.MaxSizeLength)
          If Not String.IsNullOrEmpty(mkvInfo.EBMLHead.DocType) Then nodeEBML.Nodes.Add("Doc Type: " & mkvInfo.EBMLHead.DocType)
          If mkvInfo.EBMLHead.DocTypeVersion > 0 Then nodeEBML.Nodes.Add("Doc Type Version: " & mkvInfo.EBMLHead.DocTypeVersion)
          If mkvInfo.EBMLHead.DocTypeReadVersion > 0 Then nodeEBML.Nodes.Add("Doc Type Read Version: " & mkvInfo.EBMLHead.DocTypeReadVersion)

          Dim nodeSeek = tvMKV.Nodes.Add("Seek")
          Dim nodeChunk As New TreeNode("Data Chunks")
          For I As Integer = 0 To mkvInfo.SeekHead.Contents.Length - 1
            If mkvInfo.SeekHead.Contents(I).SeekID.SequenceEqual({&H1F, &H43, &HB6, &H75}) Then
              nodeChunk.Nodes.Add("Position: " & mkvInfo.SeekHead.Contents(I).SeekPosition)
            Else
              nodeSeek.Nodes.Add(Seed.clsMKVHeaders.MKVReadable.SeekID(mkvInfo.SeekHead.Contents(I).SeekID) & " @ " & mkvInfo.SeekHead.Contents(I).SeekPosition)
            End If
          Next
          nodeSeek.Nodes.Add(nodeChunk)

          Dim nodeSegment = tvMKV.Nodes.Add("Segment")
          If mkvInfo.SegmentInfo.SegmentUID IsNot Nothing Then nodeSegment.Nodes.Add("Segment UID: " & BitConverter.ToString(mkvInfo.SegmentInfo.SegmentUID))
          If Not String.IsNullOrEmpty(mkvInfo.SegmentInfo.SegmentFilename) Then nodeSegment.Nodes.Add("Segment Filename: " & mkvInfo.SegmentInfo.SegmentFilename)
          If mkvInfo.SegmentInfo.PrevUID IsNot Nothing Then nodeSegment.Nodes.Add("Prev UID: " & BitConverter.ToString(mkvInfo.SegmentInfo.PrevUID))
          If Not String.IsNullOrEmpty(mkvInfo.SegmentInfo.PrevFilename) Then nodeSegment.Nodes.Add("Prev Filename: " & mkvInfo.SegmentInfo.PrevFilename)
          If mkvInfo.SegmentInfo.NextUID IsNot Nothing Then nodeSegment.Nodes.Add("Next UID: " & BitConverter.ToString(mkvInfo.SegmentInfo.NextUID))
          If Not String.IsNullOrEmpty(mkvInfo.SegmentInfo.NextFilename) Then nodeSegment.Nodes.Add("Next Filename: " & mkvInfo.SegmentInfo.NextFilename)
          If mkvInfo.SegmentInfo.SegmentFamily IsNot Nothing Then
            For Each Family As Byte() In mkvInfo.SegmentInfo.SegmentFamily
              nodeSegment.Nodes.Add("Segment Family: " & BitConverter.ToString(Family))
            Next
          End If

          If mkvInfo.SegmentInfo.ChapterTranslate IsNot Nothing Then
            Dim nodeTranslate = nodeSegment.Nodes.Add("Chapter Translations")
            For Each Translate As Seed.clsMKVHeaders.TranslateInfo In mkvInfo.SegmentInfo.ChapterTranslate
              Dim nodeInfo = nodeTranslate.Nodes.Add("Translation Info")

              If Translate.EditionUID IsNot Nothing Then
                Dim aUIDs(Translate.EditionUID.Length - 1) As String
                Array.Copy(Translate.EditionUID, aUIDs, Translate.EditionUID.Length)
                nodeInfo.Nodes.Add("Edition UID: " & Join(aUIDs, ", "))
              End If
              If Translate.Codec > 0 Then nodeInfo.Nodes.Add("Codec: " & Translate.Codec)
              If Translate.TrackID IsNot Nothing Then nodeInfo.Nodes.Add("ID: " & BitConverter.ToString(Translate.TrackID))
            Next
          End If
          If mkvInfo.SegmentInfo.TimecodeScale > 0 Then nodeSegment.Nodes.Add("Timecode Scale:" & mkvInfo.SegmentInfo.TimecodeScale)
          If mkvInfo.SegmentInfo.Duration <> 0 Then nodeSegment.Nodes.Add("Duration: " & ConvertTimeVal(mkvInfo.SegmentInfo.Duration / 1000))
          If mkvInfo.SegmentInfo.DateUTC.CompareTo(New Date(2001, 1, 1)) <> 0 Then nodeSegment.Nodes.Add("Date (UTC): " & mkvInfo.SegmentInfo.DateUTC.ToString("F"))
          If Not String.IsNullOrEmpty(mkvInfo.SegmentInfo.Title) Then nodeSegment.Nodes.Add("Title: " & mkvInfo.SegmentInfo.Title)
          If Not String.IsNullOrEmpty(mkvInfo.SegmentInfo.MuxingApp) Then nodeSegment.Nodes.Add("Muxing App: " & mkvInfo.SegmentInfo.MuxingApp)
          If Not String.IsNullOrEmpty(mkvInfo.SegmentInfo.WritingApp) Then nodeSegment.Nodes.Add("Writing App: " & mkvInfo.SegmentInfo.WritingApp)

          Dim nodeTracks = tvMKV.Nodes.Add("Tracks")
          For I As Integer = 0 To mkvInfo.TrackEntries.Length - 1
            Dim nodeTrack = nodeTracks.Nodes.Add(Seed.clsMKVHeaders.MKVReadable.TrackType(mkvInfo.TrackEntries(I).TrackType) & " Track " & mkvInfo.TrackEntries(I).TrackNumber)
            If mkvInfo.TrackEntries(I).TrackNumber > 0 Then nodeTrack.Nodes.Add("Track Number: " & mkvInfo.TrackEntries(I).TrackNumber)
            If mkvInfo.TrackEntries(I).TrackUID > 0 Then nodeTrack.Nodes.Add("Track UID: " & mkvInfo.TrackEntries(I).TrackUID)
            If mkvInfo.TrackEntries(I).TrackType > 0 Then nodeTrack.Nodes.Add("Track Type: " & Seed.clsMKVHeaders.MKVReadable.TrackType(mkvInfo.TrackEntries(I).TrackType))
            nodeTrack.Nodes.Add("Enabled: " & mkvInfo.TrackEntries(I).FlagEnabled)
            nodeTrack.Nodes.Add("Default: " & mkvInfo.TrackEntries(I).FlagDefault)
            nodeTrack.Nodes.Add("Forced: " & mkvInfo.TrackEntries(I).FlagForced)
            nodeTrack.Nodes.Add("Lacing: " & mkvInfo.TrackEntries(I).FlagLacing)
            If mkvInfo.TrackEntries(I).MinCache > 0 Then nodeTrack.Nodes.Add("Minimum Cache: " & mkvInfo.TrackEntries(I).MinCache)
            If mkvInfo.TrackEntries(I).MaxCache > 0 Then nodeTrack.Nodes.Add("Maximum Cache: " & mkvInfo.TrackEntries(I).MaxCache)
            If mkvInfo.TrackEntries(I).DefaultDuration > 0 Then nodeTrack.Nodes.Add("Default Duration: " & Math.Round(1 / (mkvInfo.TrackEntries(I).DefaultDuration / 1000000000), 3) & " fps")
            If mkvInfo.TrackEntries(I).TrackTimecodeScale > 0 Then nodeTrack.Nodes.Add("Track Timecode Scale: " & mkvInfo.TrackEntries(I).TrackTimecodeScale)
            If mkvInfo.TrackEntries(I).TrackOffset > 0 Then nodeTrack.Nodes.Add("Track Offset: " & mkvInfo.TrackEntries(I).TrackOffset)
            If mkvInfo.TrackEntries(I).MaxBlockAdditionID > 0 Then nodeTrack.Nodes.Add("Maximum BlockAddID: " & mkvInfo.TrackEntries(I).MaxBlockAdditionID)
            If Not String.IsNullOrEmpty(mkvInfo.TrackEntries(I).TrackName) Then nodeTrack.Nodes.Add("Name: " & mkvInfo.TrackEntries(I).TrackName)
            If Not String.IsNullOrEmpty(mkvInfo.TrackEntries(I).Language) Then nodeTrack.Nodes.Add("Language: " & mkvInfo.TrackEntries(I).Language)
            If Not String.IsNullOrEmpty(mkvInfo.TrackEntries(I).CodecID) Then nodeTrack.Nodes.Add("Codec ID: " & mkvInfo.TrackEntries(I).CodecID)
            If mkvInfo.TrackEntries(I).CodecPrivate IsNot Nothing Then
              If mkvInfo.TrackEntries(I).CodecPrivate.Contains(0) Then
                nodeTrack.Nodes.Add("Codec Private: " & vbNewLine & FormatBytes(mkvInfo.TrackEntries(I).CodecPrivate))
              Else
                nodeTrack.Nodes.Add("Codec Private: " & System.Text.Encoding.GetEncoding(LATIN_1).GetString(mkvInfo.TrackEntries(I).CodecPrivate))
              End If


            End If
            If Not String.IsNullOrEmpty(mkvInfo.TrackEntries(I).CodecName) Then nodeTrack.Nodes.Add("Codec Name: " & mkvInfo.TrackEntries(I).CodecName)
            If mkvInfo.TrackEntries(I).AttachmentLink > 0 Then nodeTrack.Nodes.Add("Attachment Link: " & mkvInfo.TrackEntries(I).AttachmentLink)
            If Not String.IsNullOrEmpty(mkvInfo.TrackEntries(I).CodecSettings) Then nodeTrack.Nodes.Add("Codec Settings: " & mkvInfo.TrackEntries(I).CodecSettings)
            If Not String.IsNullOrEmpty(mkvInfo.TrackEntries(I).CodecInfoURL) Then nodeTrack.Nodes.Add("Codec Info URL: " & mkvInfo.TrackEntries(I).CodecInfoURL)
            If Not String.IsNullOrEmpty(mkvInfo.TrackEntries(I).CodecDownloadURL) Then nodeTrack.Nodes.Add("Codec Download URL: " & mkvInfo.TrackEntries(I).CodecDownloadURL)
            nodeTrack.Nodes.Add("Codec Decode All: " & mkvInfo.TrackEntries(I).CodecDecodeAll)
            If mkvInfo.TrackEntries(I).TrackOverlay IsNot Nothing Then
              For Each Overlay As UInt64 In mkvInfo.TrackEntries(I).TrackOverlay
                nodeTrack.Nodes.Add("Track Overlay: " & Overlay)
              Next
            End If
            If mkvInfo.TrackEntries(I).Translate IsNot Nothing Then
              For Each Translate As Seed.clsMKVHeaders.TranslateInfo In mkvInfo.TrackEntries(I).Translate
                Dim nodeTranslate = nodeTrack.Nodes.Add("Translate Info")
                If Translate.EditionUID IsNot Nothing Then
                  Dim aUIDs(Translate.EditionUID.Length - 1) As String
                  Array.Copy(Translate.EditionUID, aUIDs, Translate.EditionUID.Length)
                  nodeTranslate.Nodes.Add("Edition UID:" & Join(aUIDs, ", "))
                End If
                If Translate.Codec > 0 Then nodeTranslate.Nodes.Add("Codec: " & Translate.Codec)
                If Translate.TrackID IsNot Nothing Then nodeTranslate.Nodes.Add("Track ID: " & BitConverter.ToString(Translate.TrackID))
              Next
            End If
            If mkvInfo.TrackEntries(I).Video.Exists Then
              Dim nodeVideo = nodeTrack.Nodes.Add("Video")
              nodeVideo.Nodes.Add("Interlaced: " & mkvInfo.TrackEntries(I).Video.FlagInterlaced)
              If mkvInfo.TrackEntries(I).Video.StereoMode > 0 Then nodeVideo.Nodes.Add("Stereo-3D Mode: " & Seed.clsMKVHeaders.MKVReadable.VideoStereoMode(mkvInfo.TrackEntries(I).Video.StereoMode))
              If mkvInfo.TrackEntries(I).Video.OldStereoMode > 0 Then nodeVideo.Nodes.Add("Old StereMode: " & Seed.clsMKVHeaders.MKVReadable.VideoOldStereoMode(mkvInfo.TrackEntries(I).Video.OldStereoMode))
              nodeVideo.Nodes.Add("Pixel Resolution: " & mkvInfo.TrackEntries(I).Video.PixelWidth & "x" & mkvInfo.TrackEntries(I).Video.PixelHeight)
              If mkvInfo.TrackEntries(I).Video.PixelCropTop > 0 Then nodeVideo.Nodes.Add("Pixel Crop Top: " & mkvInfo.TrackEntries(I).Video.PixelCropTop)
              If mkvInfo.TrackEntries(I).Video.PixelCropBottom > 0 Then nodeVideo.Nodes.Add("Pixel Crop Bottom: " & mkvInfo.TrackEntries(I).Video.PixelCropBottom)
              If mkvInfo.TrackEntries(I).Video.PixelCropLeft > 0 Then nodeVideo.Nodes.Add("Pixel Crop Left: " & mkvInfo.TrackEntries(I).Video.PixelCropLeft)
              If mkvInfo.TrackEntries(I).Video.PixelCropRight > 0 Then nodeVideo.Nodes.Add("Pixel Crop Right: " & mkvInfo.TrackEntries(I).Video.PixelCropRight)
              nodeVideo.Nodes.Add("Display Resolution: " & mkvInfo.TrackEntries(I).Video.DisplayWidth & "x" & mkvInfo.TrackEntries(I).Video.DisplayHeight & " " & Seed.clsMKVHeaders.MKVReadable.VideoDisplayUnit(mkvInfo.TrackEntries(I).Video.DisplayUnit))
              nodeVideo.Nodes.Add("Aspect Ratio Type: " & Seed.clsMKVHeaders.MKVReadable.VideoAspectRatioType(mkvInfo.TrackEntries(I).Video.AspectRatioType))
              If mkvInfo.TrackEntries(I).Video.ColorSpace IsNot Nothing Then nodeVideo.Nodes.Add("Color Space: " & BitConverter.ToString(mkvInfo.TrackEntries(I).Video.ColorSpace))
              If mkvInfo.TrackEntries(I).Video.GammaValue <> 0 Then nodeVideo.Nodes.Add("Gamma Value: " & mkvInfo.TrackEntries(I).Video.GammaValue)
              If mkvInfo.TrackEntries(I).Video.FrameRate <> 0 Then nodeVideo.Nodes.Add("Frame Rate: " & mkvInfo.TrackEntries(I).Video.FrameRate)
            End If
            If mkvInfo.TrackEntries(I).Audio.Exists Then
              Dim nodeAudio = nodeTrack.Nodes.Add("Audio")
              nodeAudio.Nodes.Add("Sampling Frequency: " & KRater(mkvInfo.TrackEntries(I).Audio.SamplingFrequency, "Hz"))
              'If mkvInfo.TrackEntries(I).Audio.OutputSamplingFrequency > 0 Then
              nodeAudio.Nodes.Add("Output Sampling Frequency: " & KRater(mkvInfo.TrackEntries(I).Audio.OutputSamplingFrequency, "Hz"))
              If mkvInfo.TrackEntries(I).Audio.Channels > 0 Then nodeAudio.Nodes.Add("Channels: " & mkvInfo.TrackEntries(I).Audio.Channels)
              If mkvInfo.TrackEntries(I).Audio.ChannelPositions IsNot Nothing Then nodeAudio.Nodes.Add("Channel Positions: " & BitConverter.ToString(mkvInfo.TrackEntries(I).Audio.ChannelPositions))
              If mkvInfo.TrackEntries(I).Audio.BitDepth > 0 Then nodeAudio.Nodes.Add("Bit Depth: " & mkvInfo.TrackEntries(I).Audio.BitDepth)
            End If
            If mkvInfo.TrackEntries(I).TrackOperation.Exists Then
              Dim nodeOperation = nodeTrack.Nodes.Add("Operation")
              If mkvInfo.TrackEntries(I).TrackOperation.TrackCombinePlanes.TrackPlane IsNot Nothing Then
                Dim nodePlanes = nodeOperation.Nodes.Add("Combine Planes")
                For J As Integer = 0 To mkvInfo.TrackEntries(I).TrackOperation.TrackCombinePlanes.TrackPlane.Length - 1
                  Dim nodePlane = nodePlanes.Nodes.Add("Track Plane " & J)
                  nodePlane.Nodes.Add("UID: " & mkvInfo.TrackEntries(I).TrackOperation.TrackCombinePlanes.TrackPlane(J).UID)
                  nodePlane.Nodes.Add("Type: " & mkvInfo.TrackEntries(I).TrackOperation.TrackCombinePlanes.TrackPlane(J).PlaneType)
                Next
              End If
              If mkvInfo.TrackEntries(I).TrackOperation.TrackJoinBlocks IsNot Nothing Then
                Dim nodeBlocks = nodeTrack.Nodes.Add("Join Blocks")
                For J As Integer = 0 To mkvInfo.TrackEntries(I).TrackOperation.TrackJoinBlocks.Length - 1
                  nodeBlocks.Nodes.Add("Block " & J & " UID: " & mkvInfo.TrackEntries(I).TrackOperation.TrackJoinBlocks(J).UID)
                Next
              End If
            End If
            If mkvInfo.TrackEntries(I).TrickTrackUID > 0 Then nodeTrack.Nodes.Add("Trick Track UID: " & mkvInfo.TrackEntries(I).TrickTrackUID)
            If mkvInfo.TrackEntries(I).TrickTrackSegmentUID IsNot Nothing Then nodeTrack.Nodes.Add("Trick Track Segment UID: " & BitConverter.ToString(mkvInfo.TrackEntries(I).TrickMasterTrackSegmentUID))
            If mkvInfo.TrackEntries(I).TrickTrackFlag > 0 Then nodeTrack.Nodes.Add("Trick Track Flag: " & mkvInfo.TrackEntries(I).TrickTrackFlag)
            If mkvInfo.TrackEntries(I).TrickMasterTrackUID > 0 Then nodeTrack.Nodes.Add("Trick Master Track UID: " & mkvInfo.TrackEntries(I).TrickMasterTrackUID)
            If mkvInfo.TrackEntries(I).TrickMasterTrackSegmentUID IsNot Nothing Then nodeTrack.Nodes.Add("Trick Master Segment Track UID:" & BitConverter.ToString(mkvInfo.TrackEntries(I).TrickMasterTrackSegmentUID))
            If mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding IsNot Nothing Then
              Dim nodeEncodings = nodeTrack.Nodes.Add("Content Encodings")
              For J As Integer = 0 To mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding.Length - 1
                Dim nodeEncoding = nodeEncodings.Nodes.Add("Encoding " & J)
                nodeEncoding.Nodes.Add("Order: " & mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).Order)
                nodeEncoding.Nodes.Add("Scope: " & mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).Scope)
                nodeEncoding.Nodes.Add("Type: " & mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).EncType)
                Dim nodeCompression = nodeEncoding.Nodes.Add("Compression")
                nodeCompression.Nodes.Add("Algorithm: " & mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentCompression.Algorithm)
                If mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentCompression.Settings IsNot Nothing Then nodeCompression.Nodes.Add("Settings: " & BitConverter.ToString(mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentCompression.Settings))
                Dim nodeEncryption = nodeEncoding.Nodes.Add("Encryption")
                nodeEncryption.Nodes.Add("Algorithm: " & mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.Algorithm)
                If mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.KeyID IsNot Nothing Then nodeEncryption.Nodes.Add("Key ID: " & BitConverter.ToString(mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.KeyID))
                If mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.Signature IsNot Nothing Then nodeEncryption.Nodes.Add("Signature: " & BitConverter.ToString(mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.Signature))
                If mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.SigKeyID IsNot Nothing Then nodeEncryption.Nodes.Add("Signature Key ID: " & BitConverter.ToString(mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.SigKeyID))
                nodeEncryption.Nodes.Add("Signature Algorithm: " & mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.SigAlgorithm)
                nodeEncryption.Nodes.Add("Signature Hash Algorithm: " & mkvInfo.TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.SigHashAlgorithm)
              Next
            End If
          Next

          If mkvInfo.ChapterInfo.Editions IsNot Nothing Then
            Dim nodeChapters = tvMKV.Nodes.Add("Chapters")
            For I As Integer = 0 To mkvInfo.ChapterInfo.Editions.Length - 1
              Dim nodeEdition = nodeChapters.Nodes.Add("Edition " & I)
              nodeEdition.Nodes.Add("UID: " & mkvInfo.ChapterInfo.Editions(I).UID)
              nodeEdition.Nodes.Add("Hidden: " & mkvInfo.ChapterInfo.Editions(I).FlagHidden)
              nodeEdition.Nodes.Add("Default: " & mkvInfo.ChapterInfo.Editions(I).FlagDefault)
              nodeEdition.Nodes.Add("Ordered: " & mkvInfo.ChapterInfo.Editions(I).FlagOrdered)
              Dim nodeAtoms = nodeEdition.Nodes.Add("Atoms")
              For J As Integer = 0 To mkvInfo.ChapterInfo.Editions(I).Atoms.Length - 1
                Dim nodeAtom = nodeAtoms.Nodes.Add("Atom " & J)
                nodeAtom.Nodes.Add("UID: " & mkvInfo.ChapterInfo.Editions(I).Atoms(J).UID)
                nodeAtom.Nodes.Add("Start Time: " & mkvInfo.ChapterInfo.Editions(I).Atoms(J).TimeStart)
                If mkvInfo.ChapterInfo.Editions(I).Atoms(J).TimeEnd > 0 Then nodeAtom.Nodes.Add("End Time: " & mkvInfo.ChapterInfo.Editions(I).Atoms(J).TimeEnd)
                nodeAtom.Nodes.Add("Hidden: " & mkvInfo.ChapterInfo.Editions(I).Atoms(J).FlagHidden)
                nodeAtom.Nodes.Add("Enabled: " & mkvInfo.ChapterInfo.Editions(I).Atoms(J).FlagEnabled)
                If mkvInfo.ChapterInfo.Editions(I).Atoms(J).SegmentUID IsNot Nothing Then nodeAtom.Nodes.Add("Segment UID: " & BitConverter.ToString(mkvInfo.ChapterInfo.Editions(I).Atoms(J).SegmentUID))
                If mkvInfo.ChapterInfo.Editions(I).Atoms(J).SegmentEditionUID IsNot Nothing Then nodeAtom.Nodes.Add("Edition UID: " & BitConverter.ToString(mkvInfo.ChapterInfo.Editions(I).Atoms(J).SegmentEditionUID))
                nodeAtom.Nodes.Add("Physical Equiv: " & mkvInfo.ChapterInfo.Editions(I).Atoms(J).PhysicalEquiv)
                Dim nodeDisplays = nodeAtom.Nodes.Add("Displays")
                For K As Integer = 0 To mkvInfo.ChapterInfo.Editions(I).Atoms(J).Display.Length - 1
                  Dim nodeDisplay = nodeDisplays.Nodes.Add("Display " & K)
                  nodeDisplay.Nodes.Add("Title: " & mkvInfo.ChapterInfo.Editions(I).Atoms(J).Display(K).Title)
                  nodeDisplay.Nodes.Add("Language: " & Join(mkvInfo.ChapterInfo.Editions(I).Atoms(J).Display(K).Language, ", "))
                  nodeDisplay.Nodes.Add("Country: " & Join(mkvInfo.ChapterInfo.Editions(I).Atoms(J).Display(K).Country, " ,"))
                Next
                If mkvInfo.ChapterInfo.Editions(I).Atoms(J).Process IsNot Nothing Then
                  Dim nodeProcesses = nodeAtom.Nodes.Add("Processes")
                  For K As Integer = 0 To mkvInfo.ChapterInfo.Editions(I).Atoms(J).Process.Length - 1
                    Dim nodeProcess = nodeProcesses.Nodes.Add("Process " & K)
                    nodeProcess.Nodes.Add("Codec ID: " & mkvInfo.ChapterInfo.Editions(I).Atoms(J).Process(K).CodecID)
                    nodeProcess.Nodes.Add("Private: " & BitConverter.ToString(mkvInfo.ChapterInfo.Editions(I).Atoms(J).Process(K).ProcPrivate))
                    Dim nodeCommands = nodeProcess.Nodes.Add("Commands")
                    For L As Integer = 0 To mkvInfo.ChapterInfo.Editions(I).Atoms(J).Process(K).Command.Length - 1
                      Dim nodeCommand = nodeCommands.Nodes.Add("Command " & L)
                      nodeCommand.Nodes.Add("Process Time: " & mkvInfo.ChapterInfo.Editions(I).Atoms(J).Process(K).Command(L).Time)
                      nodeCommand.Nodes.Add("Process Data: " & BitConverter.ToString(mkvInfo.ChapterInfo.Editions(I).Atoms(J).Process(K).Command(L).Data))
                    Next
                  Next
                End If
              Next
            Next
          End If
          Return True
        Else
          Return False
        End If
      End Using
    Catch ex As Exception
      Return False
    End Try
  End Function

  Private Function LoadMIDI() As Boolean
    tvMIDI.Nodes.Clear()
    Try
      Using cHeader As New Seed.clsMIDI(sFile)
        If cHeader.IsMIDI Or 1 = 1 Then 'fuck validity

          Dim nodeHeader = tvMIDI.Nodes.Add("Header")
          Select Case cHeader.TrackFormat
            Case Seed.clsMIDI.TrackType.SingleTrack : nodeHeader.Nodes.Add("File Format: Single Track")
            Case Seed.clsMIDI.TrackType.MultiSynch : nodeHeader.Nodes.Add("File Format: Mulitple Synchronous Tracks")
            Case Seed.clsMIDI.TrackType.MultiAsynch : nodeHeader.Nodes.Add("File Format: Multiple Asynchronous Tracks")
            Case Seed.clsMIDI.TrackType.Unknown : nodeHeader.Nodes.Add("File Format: Unknown")
          End Select
          nodeHeader.Nodes.Add("Track Count: " & cHeader.TrackCount)
          'If (cHeader.TrackTime And &H8000) Then
          '  'Frames per second
          '  Debug.Print(cHeader.TrackTime And &H7FFF)
          'Else
          '  'Ticks per beat
          '  Debug.Print(cHeader.TrackTime Xor &H8000)
          'End If
          nodeHeader.Nodes.Add("Delta-time ticks per Quarter Note: " & cHeader.TrackTime)

          Dim nodeTracks = tvMIDI.Nodes.Add("Tracks")
          For I As Integer = 0 To cHeader.TrackCount - 1
            Dim nodeTrack = nodeTracks.Nodes.Add("Track " & I + 1 & ": " & cHeader.Tracks(I).TrackName)
            If cHeader.Tracks(I).SequenceNumber > 0 Then nodeTrack.Nodes.Add("Sequence Number: " & cHeader.Tracks(I).SequenceNumber)
            If Not String.IsNullOrEmpty(cHeader.Tracks(I).TextEvent) Then nodeTrack.Nodes.Add("Text Event: " & cHeader.Tracks(I).TextEvent)
            If Not String.IsNullOrEmpty(cHeader.Tracks(I).Copyright) Then nodeTrack.Nodes.Add("Copyright: " & cHeader.Tracks(I).Copyright)
            'If Not String.IsNullOrEmpty(cHeader.Tracks(I).TrackName) Then nodeTrack.Nodes.Add("Track Name: " & cHeader.Tracks(I).TrackName)
            If Not String.IsNullOrEmpty(cHeader.Tracks(I).TrackInstrument) Then nodeTrack.Nodes.Add("Instrument: " & cHeader.Tracks(I).TrackInstrument)
            If Not String.IsNullOrEmpty(cHeader.Tracks(I).TrackLyric) Then nodeTrack.Nodes.Add("Lyric: " & cHeader.Tracks(I).TrackLyric)
            If Not String.IsNullOrEmpty(cHeader.Tracks(I).TrackMarker) Then nodeTrack.Nodes.Add("Marker: " & cHeader.Tracks(I).TrackMarker)
            If Not String.IsNullOrEmpty(cHeader.Tracks(I).TrackCuePoint) Then nodeTrack.Nodes.Add("Cue Point: " & cHeader.Tracks(I).TrackCuePoint)
            If cHeader.Tracks(I).ChannelPrefix > 0 Then nodeTrack.Nodes.Add("Channel Prefix: " & cHeader.Tracks(I).ChannelPrefix)
            If cHeader.Tracks(I).PortPrefix > 0 Then nodeTrack.Nodes.Add("Port Prefix: " & cHeader.Tracks(I).PortPrefix)
            If cHeader.Tracks(I).TrackTempo > 0 Then nodeTrack.Nodes.Add("Tempo: " & Math.Floor(60000000 / cHeader.Tracks(I).TrackTempo) & " bpm")
            If cHeader.Tracks(I).TrackTime.Numerator > 0 Or cHeader.Tracks(I).TrackTime.Denominator > 0 Or cHeader.Tracks(I).TrackTime.TicksPerClick > 0 Or cHeader.Tracks(I).TrackTime.ThirtySecondsPerQuarter > 0 Then
              Dim nodeTime = nodeTrack.Nodes.Add("Time Signature: " & cHeader.Tracks(I).TrackTime.Numerator & "/" & (2 ^ cHeader.Tracks(I).TrackTime.Denominator))
              nodeTime.Nodes.Add("Ticks in Metronome Click: " & cHeader.Tracks(I).TrackTime.TicksPerClick)
              nodeTime.Nodes.Add("Thirty-second notes to the quarter note: " & cHeader.Tracks(I).TrackTime.ThirtySecondsPerQuarter)
            End If
            If cHeader.Tracks(I).TrackSignature.SharpsFlats > 0 And cHeader.Tracks(I).TrackSignature.MajorMinor > 0 Then
              Dim KeySig As String = String.Empty
              Select Case cHeader.Tracks(I).TrackSignature.SharpsFlats
                Case 0 : KeySig = "key of C"
                Case 1 : KeySig = "1 sharp"
                Case 2 To 7 : KeySig = cHeader.Tracks(I).TrackSignature.SharpsFlats & " sharps"
                Case 255 : KeySig = "1 flat"
                Case 254 : KeySig = "2 flats"
                Case 253 : KeySig = "3 flats"
                Case 252 : KeySig = "4 flats"
                Case 251 : KeySig = "5 flats"
                Case 250 : KeySig = "6 flats"
                Case 249 : KeySig = "7 flats"
                Case Else : KeySig = cHeader.Tracks(I).TrackSignature.SharpsFlats & "?"
              End Select
              Select Case cHeader.Tracks(I).TrackSignature.MajorMinor
                Case 0 : KeySig &= " major"
                Case 1 : KeySig &= " minor"
                Case Else : KeySig &= " (" & cHeader.Tracks(I).TrackSignature.MajorMinor & ")"
              End Select
              nodeTrack.Nodes.Add("Key Signature: " & KeySig)
            End If
            If cHeader.Tracks(I).SequencerInfo IsNot Nothing Then nodeTrack.Nodes.Add("Sequencer Information: " & BitConverter.ToString(cHeader.Tracks(I).SequencerInfo))
          Next
          Return cHeader.TrackCount > 0
        End If
      End Using
      Return False
    Catch ex As Exception
      Return False
    End Try
  End Function

  Private Function LoadRIFF() As Boolean
    tvRIFF.Nodes.Clear()
    Using cHeader As New Seed.clsRIFF(sFile)
      If cHeader.IsValid Then
        Dim nodeHeader = tvRIFF.Nodes.Add("RIFF Header")
        If cHeader.IsWAV Then
          Dim nodeWAV = nodeHeader.Nodes.Add("WAV Header")
          nodeWAV.Nodes.Add("Format: " & WAVAudioCodecs(cHeader.WAVData.Format.Format.wFormatTag))
          nodeWAV.Nodes.Add("Channels: " & cHeader.WAVData.Format.Format.nChannels)
          nodeWAV.Nodes.Add("Samples Rate: " & KRater(cHeader.WAVData.Format.Format.nSamplesPerSec, "Hz"))
          nodeWAV.Nodes.Add("Bitrate: " & KRater(cHeader.WAVData.Format.Format.nAvgBytesPerSec * 8, "bps"))
          nodeWAV.Nodes.Add("Block Align: " & cHeader.WAVData.Format.Format.nBlockAlign)
          If cHeader.WAVData.Format.cbSize > 0 Or cHeader.WAVData.Format.wBitsPerSample > 0 Then
            nodeWAV.Nodes.Add("Bits Per Sample: " & cHeader.WAVData.Format.wBitsPerSample)
            nodeWAV.Nodes.Add("Size: " & cHeader.WAVData.Format.cbSize)
          End If
          If cHeader.WAVData.Format.cbSize >= 22 Then
            Dim nodeWAVEXT = nodeHeader.Nodes.Add("WAV_FORMAT_EXTENSIBLE Header")
            nodeWAVEXT.Nodes.Add("Valid Bits Per Sample: " & cHeader.WAVData.Samples.wValidBitsPerSample)
            If cHeader.WAVData.dwChannelMask > 0 Then
              Dim sChan As String = String.Empty

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontLeft) = Seed.clsRIFF.ChannelStruct.FrontLeft Then sChan &= "Front Left, "
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontCenterLeft) = Seed.clsRIFF.ChannelStruct.FrontCenterLeft Then sChan &= "Front Center Left, "
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontCenter) = Seed.clsRIFF.ChannelStruct.FrontCenter Then sChan &= "Front Center, "
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontCenterRight) = Seed.clsRIFF.ChannelStruct.FrontCenterRight Then sChan &= "Front Center Right, "
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontRight) = Seed.clsRIFF.ChannelStruct.FrontRight Then sChan &= "Front Right, "

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.SideLeft) = Seed.clsRIFF.ChannelStruct.SideLeft Then sChan &= "Side Left, "
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.SideRight) = Seed.clsRIFF.ChannelStruct.SideRight Then sChan &= "Side Right, "

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.RearLeft) = Seed.clsRIFF.ChannelStruct.RearLeft Then sChan &= "Rear Left, "
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.RearCenter) = Seed.clsRIFF.ChannelStruct.RearCenter Then sChan &= "Rear Center, "
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.RearRight) = Seed.clsRIFF.ChannelStruct.RearRight Then sChan &= "Rear Right, "

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopCenter) = Seed.clsRIFF.ChannelStruct.TopCenter Then sChan &= "Top Center, "

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopFrontLeft) = Seed.clsRIFF.ChannelStruct.TopFrontLeft Then sChan &= "Top Front Left, "
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopFrontCenter) = Seed.clsRIFF.ChannelStruct.TopFrontCenter Then sChan &= "Top Front Center, "
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopFrontRight) = Seed.clsRIFF.ChannelStruct.TopFrontRight Then sChan &= "Top Front Right, "

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopRearLeft) = Seed.clsRIFF.ChannelStruct.TopRearLeft Then sChan &= "Top Rear Left, "
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopRearCenter) = Seed.clsRIFF.ChannelStruct.TopRearCenter Then sChan &= "Top Rear Center, "
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopRearRight) = Seed.clsRIFF.ChannelStruct.TopRearRight Then sChan &= "Top Rear Right, "

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.LFE) = Seed.clsRIFF.ChannelStruct.LFE Then sChan &= "Low Frequency Emitter, "

              If sChan.Length > 2 Then sChan = sChan.Substring(0, sChan.Length - 2)
              nodeWAVEXT.Nodes.Add("Channels: " & sChan)
            End If
            Select Case cHeader.WAVData.SubFormat.ToString.ToLower
              Case "6dba3190-67bd-11cf-a0f7-0020afd156e4" : nodeWAVEXT.Nodes.Add("Format: Analog")
              Case "00000001-0000-0010-8000-00aa00389b71" : nodeWAVEXT.Nodes.Add("Format: PCM")
              Case "00000003-0000-0010-8000-00aa00389b71" : nodeWAVEXT.Nodes.Add("Format: Float (IEEE)")
              Case "00000009-0000-0010-8000-00aa00389b71" : nodeWAVEXT.Nodes.Add("Format: DRM")
              Case "00000006-0000-0010-8000-00aa00389b71" : nodeWAVEXT.Nodes.Add("Format: A-Law")
              Case "00000007-0000-0010-8000-00aa00389b71" : nodeWAVEXT.Nodes.Add("Format: µ-Law")
              Case "00000002-0000-0010-8000-00aa00389b71" : nodeWAVEXT.Nodes.Add("Format: ADPCM")
              Case "00000050-0000-0010-8000-00aa00389b71" : nodeWAVEXT.Nodes.Add("Format: MPEG")
              Case "4995daee-9ee6-11d0-a40e-00a0c9223196" : nodeWAVEXT.Nodes.Add("Format: RIFF")
              Case "e436eb8b-524f-11ce-9f53-0020af0ba770" : nodeWAVEXT.Nodes.Add("Format: RIFF WAVE")
              Case "1d262760-e957-11cf-a5d6-28db04c10000" : nodeWAVEXT.Nodes.Add("Format: MIDI")
              Case "2ca15fa0-6cfe-11cf-a5d6-28dB04c10000" : nodeWAVEXT.Nodes.Add("Format: MIDI Bus")
              Case "4995daf0-9ee6-11d0-a40e-00a0c9223196" : nodeWAVEXT.Nodes.Add("Format: RIFF MIDI")
              Case Else : nodeWAVEXT.Nodes.Add("Format: Unknown {" & cHeader.WAVData.SubFormat.ToString & "}")
            End Select
          End If
          If cHeader.IsDTS Then
            Dim nodeDTS = nodeHeader.Nodes.Add("DTS Header")
            nodeDTS.Nodes.Add("Frame Type: " & IIf(cHeader.DTSData.bFTYPE, "Normal", "Termination"))
            nodeDTS.Nodes.Add("Deficit Samples: " & IIf(cHeader.DTSData.iSHORT = 31, "Normal", 32 - cHeader.DTSData.iSHORT))
            If cHeader.DTSData.bCPF Then nodeDTS.Nodes.Add("CRC Checksum: " & Hex(cHeader.DTSData.iHCRC))
            nodeDTS.Nodes.Add("Block Count: " & cHeader.DTSData.iNBLKS)
            nodeDTS.Nodes.Add("Encoding Window: " & 32 * (cHeader.DTSData.iNBLKS + 1))
            nodeDTS.Nodes.Add("Frame Size: " & cHeader.DTSData.iFSIZE + 1)
            Select Case cHeader.DTSData.iAMODE
              Case 0 : nodeDTS.Nodes.Add("Channels: Mono")
              Case 1 : nodeDTS.Nodes.Add("Channels: Dual Mono (A + B)")
              Case 2 : nodeDTS.Nodes.Add("Channels: Left and Right Stereo")
              Case 3 : nodeDTS.Nodes.Add("Channels: Sum-Difference (Left + Right) - (Left - Right)")
              Case 4 : nodeDTS.Nodes.Add("Channels: Totals Stereo")
              Case 5 : nodeDTS.Nodes.Add("Channels: Center, Left, Right")
              Case 6 : nodeDTS.Nodes.Add("Channels: Left, Right, Surround")
              Case 7 : nodeDTS.Nodes.Add("Channels: Center, Left, Right, Surround")
              Case 8 : nodeDTS.Nodes.Add("Channels: Left Right, Surround Left, Surround Right")
              Case 9 : nodeDTS.Nodes.Add("Channels: Center, Left, Right, Surround Left, Surround Right")
              Case 10 : nodeDTS.Nodes.Add("Channels: Center Left, Center Right, Left, Right, Surround Left, Surround Right")
              Case 11 : nodeDTS.Nodes.Add("Channels: Center, Left, Right, Left Rear, Right Rear, Overhead")
              Case 12 : nodeDTS.Nodes.Add("Channels: Center Front, Center Rear, Left Front, Right Front, Left Rear, Right Rear")
              Case 13 : nodeDTS.Nodes.Add("Channels: Center Left, Center, Center Right, Left, Right, Surround Left, Surround Right")
              Case 14 : nodeDTS.Nodes.Add("Channels:  Center Left, Center Right, Left, Right, Surround Left 1, Surround Left 2, Surround Right 1, Surround Right 2")
              Case 15 : nodeDTS.Nodes.Add("Channels:  Center Left, Center, Center Right, Left, Right, Surround Left, Surround, Surround Right")
              Case Else : nodeDTS.Nodes.Add("Channels: Custom [0x" & Hex(cHeader.DTSData.iAMODE) & "]")
            End Select
            Select Case cHeader.DTSData.iSFREQ
              Case 0, 4, 5, 9, 10, 15, 16 : nodeDTS.Nodes.Add("Sample Frequency: Invalid")
              Case 1 : nodeDTS.Nodes.Add("Sample Frequency: 8 KHz")
              Case 2 : nodeDTS.Nodes.Add("Sample Frequency: 16 KHz")
              Case 3 : nodeDTS.Nodes.Add("Sample Frequency: 32 KHz")
              Case 6 : nodeDTS.Nodes.Add("Sample Frequency: 11.025 KHz")
              Case 7 : nodeDTS.Nodes.Add("Sample Frequency: 22.050 KHz")
              Case 8 : nodeDTS.Nodes.Add("Sample Frequency: 44.1 KHz")
              Case 11 : nodeDTS.Nodes.Add("Sample Frequency: 12 KHz")
              Case 12 : nodeDTS.Nodes.Add("Sample Frequency: 24 KHz")
              Case 13 : nodeDTS.Nodes.Add("Sample Frequency: 48 KHz")
              Case Else : nodeDTS.Nodes.Add("Sample Frequency: Unknown [" & Hex(cHeader.DTSData.iSFREQ) & "]")
            End Select
            Select Case cHeader.DTSData.iRATE
              Case 0 : nodeDTS.Nodes.Add("Bitrate: 32 kbps")
              Case 1 : nodeDTS.Nodes.Add("Bitrate: 56 kbps")
              Case 2 : nodeDTS.Nodes.Add("Bitrate: 64 kbps")
              Case 3 : nodeDTS.Nodes.Add("Bitrate: 96 kbps")
              Case 4 : nodeDTS.Nodes.Add("Bitrate: 112 kbps")
              Case 5 : nodeDTS.Nodes.Add("Bitrate: 128 kbps")
              Case 6 : nodeDTS.Nodes.Add("Bitrate: 192 kbps")
              Case 7 : nodeDTS.Nodes.Add("Bitrate: 224 kbps")
              Case 8 : nodeDTS.Nodes.Add("Bitrate: 256 kbps")
              Case 9 : nodeDTS.Nodes.Add("Bitrate: 320 kbps")
              Case 10 : nodeDTS.Nodes.Add("Bitrate: 384 kbps")
              Case 11 : nodeDTS.Nodes.Add("Bitrate: 448 kbps")
              Case 12 : nodeDTS.Nodes.Add("Bitrate: 512 kbps")
              Case 13 : nodeDTS.Nodes.Add("Bitrate: 576 kbps")
              Case 14 : nodeDTS.Nodes.Add("Bitrate: 640 kbps")
              Case 15 : nodeDTS.Nodes.Add("Bitrate: 768 kbps")
              Case 16 : nodeDTS.Nodes.Add("Bitrate: 960 kbps")
              Case 17 : nodeDTS.Nodes.Add("Bitrate: 1024 kbps")
              Case 18 : nodeDTS.Nodes.Add("Bitrate: 1152 kbps")
              Case 19 : nodeDTS.Nodes.Add("Bitrate: 1280 kbps")
              Case 20 : nodeDTS.Nodes.Add("Bitrate: 1344 kbps")
              Case 21 : nodeDTS.Nodes.Add("Bitrate: 1408 kbps")
              Case 22 : nodeDTS.Nodes.Add("Bitrate: 1411.2 kbps")
              Case 23 : nodeDTS.Nodes.Add("Bitrate: 1472 kbps")
              Case 24 : nodeDTS.Nodes.Add("Bitrate: 1536 kbps")
              Case 25 : nodeDTS.Nodes.Add("Bitrate: Open")
            End Select
            nodeDTS.Nodes.Add("Fixed Bit: " & IIf(cHeader.DTSData.bFixedBit, "On [Invalid]", "Off"))
            nodeDTS.Nodes.Add("Dynamic Range Coefficients: " & IIf(cHeader.DTSData.bDYNF, "Present", "Not Present"))
            nodeDTS.Nodes.Add("Time Stamps: " & IIf(cHeader.DTSData.bTIMEF, "Present", "Not Present"))
            nodeDTS.Nodes.Add("Auxiliary Data: " & IIf(cHeader.DTSData.bAUXF, "Present", "Not Present"))
            nodeDTS.Nodes.Add("HDCD Mastering: " & IIf(cHeader.DTSData.bHDCD, "Yes", "No"))
            nodeDTS.Nodes.Add("Extended Audio Data: " & IIf(cHeader.DTSData.bEXT_AUDIO, "Present", "Not Present"))
            If cHeader.DTSData.bEXT_AUDIO Then
              Select Case cHeader.DTSData.iEXT_AUDIO_ID
                Case 0 : nodeDTS.Nodes.Add("Extended Audio Description: Channel Extension (XCh)")
                Case 2 : nodeDTS.Nodes.Add("Extended Audio Description: Frequency Extension (X96)")
                Case 6 : nodeDTS.Nodes.Add("Extended Audio Description: Channel Extension (XXCH)")
                Case Else : nodeDTS.Nodes.Add("Extended Audio Description: Reserved")
              End Select
            End If
            nodeDTS.Nodes.Add("DSYNC Placed at End of Each: " & IIf(cHeader.DTSData.bASPF, "Subframe", "Sub-subframe"))
            Select Case cHeader.DTSData.iLFF
              Case 0 : nodeDTS.Nodes.Add("Low Frequency Effects: Not Present")
              Case 1 : nodeDTS.Nodes.Add("Low Frequency Effects: Interpolated by 128")
              Case 2 : nodeDTS.Nodes.Add("Low Frequency Effects: Interpolated by 64")
              Case Else : nodeDTS.Nodes.Add("Low Frequency Effects: " & cHeader.DTSData.iLFF & " [Invalid]")
            End Select
            nodeDTS.Nodes.Add("Predictor History: " & IIf(cHeader.DTSData.bHFLAG, "Used", "Ignored"))
            nodeDTS.Nodes.Add("32-band Interpolation Filter: " & IIf(cHeader.DTSData.bFILTS, "Perfect Reconstruction", "Non-perfect Reconstruction"))
            nodeDTS.Nodes.Add("Encoder Revision: " & cHeader.DTSData.iVERNUM)
            nodeDTS.Nodes.Add("Copy History: " & cHeader.DTSData.iCHIST)
            Select Case cHeader.DTSData.iPCMR
              Case 0
                nodeDTS.Nodes.Add("Source PCM Resolution: 16 bit")
                nodeDTS.Nodes.Add("DTS ES: No")
              Case 1
                nodeDTS.Nodes.Add("Source PCM Resolution: 16 bit")
                nodeDTS.Nodes.Add("DTS ES: Yes")
              Case 2
                nodeDTS.Nodes.Add("Source PCM Resolution: 20 bit")
                nodeDTS.Nodes.Add("DTS ES: No")
              Case 3
                nodeDTS.Nodes.Add("Source PCM Resolution: 20 bit")
                nodeDTS.Nodes.Add("DTS ES: Yes")
              Case 6
                nodeDTS.Nodes.Add("Source PCM Resolution: 24 bit")
                nodeDTS.Nodes.Add("DTS ES: No")
              Case 5
                nodeDTS.Nodes.Add("Source PCM Resolution: 24 bit")
                nodeDTS.Nodes.Add("DTS ES: Yes")
              Case Else
                nodeDTS.Nodes.Add("Source PCM Resolution: Invalid")
                nodeDTS.Nodes.Add("DTS ES: Invalid")
            End Select
            nodeDTS.Nodes.Add("Front Sum/Difference Encoding: " & IIf(cHeader.DTSData.bSUMF, "L=L+R, R=L-R", "L=L, R=R"))
            nodeDTS.Nodes.Add("Surround Sum/Difference Encoding: " & IIf(cHeader.DTSData.bSUMS, "Ls=Ls+Rs, Rs=Ls-Rs", "Ls=Ls, Rs=Rs"))
            If cHeader.DTSData.iVERNUM = 6 Then
              nodeDTS.Nodes.Add("Dialog Normalization Gain: " & -(cHeader.DTSData.iDIALNORM + 16))
            ElseIf cHeader.DTSData.iVERNUM = 7 Then
              nodeDTS.Nodes.Add("Dialog Normalization Gain: " & -(cHeader.DTSData.iDIALNORM))
            End If
          End If
        End If
        If cHeader.IsAVI Then
          If cHeader.AVIINFOData IsNot Nothing Then
            Dim nodeAVIInfo = nodeHeader.Nodes.Add("AVI Info")
            For Each item In cHeader.AVIINFOData
              nodeAVIInfo.Nodes.Add(AVIInfoKeys(item.Key) & ": " & item.Value)
            Next
          End If

          If cHeader.AVIDIVXData.FileID = "DIVXTAG" Then
            Dim nodeAVIDIVXInfo = nodeHeader.Nodes.Add("AVI DivX Info")
            nodeAVIDIVXInfo.Nodes.Add("Movie: " & cHeader.AVIDIVXData.Movie)
            nodeAVIDIVXInfo.Nodes.Add("Author: " & cHeader.AVIDIVXData.Author)
            nodeAVIDIVXInfo.Nodes.Add("Year: " & cHeader.AVIDIVXData.Year)
            nodeAVIDIVXInfo.Nodes.Add("Comment: " & cHeader.AVIDIVXData.Comment)
            nodeAVIDIVXInfo.Nodes.Add("Genre: " & cHeader.AVIDIVXData.Genre.ToString)
            nodeAVIDIVXInfo.Nodes.Add("Rating: " & cHeader.AVIDIVXData.Rating.ToString)
          End If

          'If cHeader.AVIINDEXData.fcc = "idx1" Then
          '  Dim nodeAVIINDEXInfo = nodeHeader.Nodes.Add("AVI Old Index Info")
          '  For Each node In cHeader.AVIINDEXData.aIndex
          '    Dim nodeAVIIndexData = nodeAVIINDEXInfo.Nodes.Add("Chunk Offset: " & node.dwOffset)
          '    nodeAVIIndexData.Nodes.Add("Chunk ID: " & System.Text.Encoding.GetEncoding(LATIN_1).GetString(BitConverter.GetBytes(node.dwChunkID)))
          '    nodeAVIIndexData.Nodes.Add("Flags: " & node.dwFlags.ToString)
          '    nodeAVIIndexData.Nodes.Add("Chunk Size: " & node.dwSize)
          '  Next
          'End If

          Dim nodeAVIMain = nodeHeader.Nodes.Add("AVI Header")
          nodeAVIMain.Nodes.Add("Microseconds per Frame: " & cHeader.AVIMainData.dwMicroSecPerFrame)
          nodeAVIMain.Nodes.Add("Maximum Bytes per Second: " & cHeader.AVIMainData.dwMaxBytesPerSec)
          nodeAVIMain.Nodes.Add("Padding Granularity: " & cHeader.AVIMainData.dwPaddingGranularity)
          nodeAVIMain.Nodes.Add("Flags: " & cHeader.AVIMainData.dwFlags.ToString)
          nodeAVIMain.Nodes.Add("Total Frames: " & cHeader.AVIMainData.dwTotalFrames)
          nodeAVIMain.Nodes.Add("Initial Frames: " & cHeader.AVIMainData.dwInitialFrames)
          nodeAVIMain.Nodes.Add("Streams: " & cHeader.AVIMainData.dwStreams)
          nodeAVIMain.Nodes.Add("Suggested Buffer Size: " & cHeader.AVIMainData.dwSuggestedBufferSize)
          nodeAVIMain.Nodes.Add("Resolution: " & cHeader.AVIMainData.dwWidth & "x" & cHeader.AVIMainData.dwHeight)

          Dim v, a, m, t As Integer
          For I As Integer = 0 To cHeader.AVIStreamCount - 1
            Dim cStream As Seed.clsRIFF.AVISTREAMHEADER = cHeader.AVIStreamData(I)
            Dim nodeAVIStream = nodeAVIMain.Nodes.Add(cStream.fccType.ToUpper & " Node")
            If Not String.IsNullOrEmpty(cStream.fccHandler) Then nodeAVIStream.Nodes.Add("Handler: " & cStream.fccHandler)
            nodeAVIStream.Nodes.Add("Flags: " & cStream.dwFlags.ToString)
            nodeAVIStream.Nodes.Add("Priority: " & cStream.wPriority)
            nodeAVIStream.Nodes.Add("Language: " & cStream.wLanguage)
            nodeAVIStream.Nodes.Add("Initial Frames: " & cStream.dwInitialFrames)
            nodeAVIStream.Nodes.Add("Time Scale: " & cStream.dwRate / cStream.dwScale)
            nodeAVIStream.Nodes.Add("Start: " & cStream.dwStart)
            nodeAVIStream.Nodes.Add("Length: " & cStream.dwLength)
            nodeAVIStream.Nodes.Add("Suggested Buffer Size: " & cStream.dwSuggestedBufferSize)
            If cStream.dwQuality = &HFFFFFFFFUL Then
              nodeAVIStream.Nodes.Add("Quality: Default")
            Else
              nodeAVIStream.Nodes.Add("Quality: " & FormatPercent(cStream.dwQuality / 10000, 2, TriState.False, TriState.False, TriState.False))
            End If
            nodeAVIStream.Nodes.Add("Sample Size: " & cStream.dwSampleSize)
            nodeAVIStream.Nodes.Add("Frame: " & cStream.rcFrame.left & "," & cStream.rcFrame.top & " - " & cStream.rcFrame.right & "," & cStream.rcFrame.bottom)
            Select Case cStream.fccType
              Case "vids"
                If v < cHeader.AVIVideoCount Then
                  Dim cBMP As Seed.clsRIFF.BITMAPINFO = cHeader.AVIVideoData(v)
                  Dim nodeAVIVidStream = nodeAVIStream.Nodes.Add("Bitmap Info Header")
                  nodeAVIVidStream.Nodes.Add("Resolution: " & cBMP.bmiHeader.biWidth & "x" & cBMP.bmiHeader.biHeight)
                  nodeAVIVidStream.Nodes.Add("Planes: " & cBMP.bmiHeader.biPlanes)
                  Select Case cBMP.bmiHeader.biBitCount
                    Case 0 : nodeAVIVidStream.Nodes.Add("Bit Count: Specified/Implied")
                    Case 1 : nodeAVIVidStream.Nodes.Add("Bit Count: Monochrome")
                    Case 4 : nodeAVIVidStream.Nodes.Add("Bit Count: 16 Colors")
                    Case 8 : nodeAVIVidStream.Nodes.Add("Bit Count: 256 Colors")
                    Case 16 : nodeAVIVidStream.Nodes.Add("Bit Count: 16-bit Color")
                    Case 24 : nodeAVIVidStream.Nodes.Add("Bit Count: 24-bit Color")
                    Case 32 : nodeAVIVidStream.Nodes.Add("Bit Count: 32-bit Color")
                  End Select
                  nodeAVIVidStream.Nodes.Add("Compression: " & AVIVideoCodecs(cBMP.bmiHeader.biCompression))
                  nodeAVIVidStream.Nodes.Add("SizeImage: " & cBMP.bmiHeader.biSizeImage)
                  nodeAVIVidStream.Nodes.Add("X Pixels per Meter: " & cBMP.bmiHeader.biXPelsPerMeter)
                  nodeAVIVidStream.Nodes.Add("Y Pixels per Meter: " & cBMP.bmiHeader.biYPelsPerMeter)
                  nodeAVIVidStream.Nodes.Add("Used Colors: " & cBMP.bmiHeader.biClrUsed)
                  nodeAVIVidStream.Nodes.Add("Important Colors: " & cBMP.bmiHeader.biClrImportant)
                End If
                v += 1
              Case "auds"
                If a < cHeader.AVIAudioCount Then
                  Dim cWAV As Seed.clsRIFF.WAVEFORMATEX = cHeader.AVIAudioData(a)
                  Dim nodeWAV = nodeAVIStream.Nodes.Add("WAV Info Header")
                  nodeWAV.Nodes.Add("Format: " & WAVAudioCodecs(cWAV.Format.wFormatTag))
                  nodeWAV.Nodes.Add("Channels: " & cWAV.Format.nChannels)
                  nodeWAV.Nodes.Add("Samples Rate: " & KRater(cWAV.Format.nSamplesPerSec, "Hz"))
                  nodeWAV.Nodes.Add("Bitrate: " & KRater(cWAV.Format.nAvgBytesPerSec * 8, "bps"))
                  nodeWAV.Nodes.Add("Block Align: " & cWAV.Format.nBlockAlign)
                  If cWAV.cbSize > 0 Or cWAV.wBitsPerSample > 0 Then
                    nodeWAV.Nodes.Add("Bits Per Sample: " & cWAV.wBitsPerSample)
                    nodeWAV.Nodes.Add("Size: " & cWAV.cbSize)
                  End If
                End If
                a += 1
              Case "mids"

                m += 1
              Case "txts"

                t += 1
            End Select
          Next
        End If
        nodeHeader.ExpandAll()
        Return True
      End If
    End Using
    Return False
  End Function

  Public Sub ShowData(File As String)
    sFile = File
    mpPreview = New Seed.ctlSeed
    mpPreview.AudioDevice = My.Settings.Device
    If String.IsNullOrEmpty(File) Then Exit Sub
    If File.EndsWith("VIDEO_TS", True, Nothing) Then
      pctClipIcon.Image = Nothing
    Else
      Using ico As Drawing.Icon = Drawing.Icon.ExtractAssociatedIcon(File)
        pctClipIcon.Image = ico.ToBitmap
      End Using
    End If

    pctDetailsIcon.Image = pctClipIcon.Image
    pctAdvancedIcon.Image = pctClipIcon.Image
    If File.EndsWith("VIDEO_TS", True, Nothing) Then
      txtClipFileName.Text = My.Computer.FileSystem.GetDriveInfo(File.Substring(0, 3)).VolumeLabel
    Else
      txtClipFileName.Text = IO.Path.GetFileName(File)
    End If

    txtDetailsFileName.Text = txtClipFileName.Text
    txtAdvancedFileName.Text = txtClipFileName.Text
    txtFilePath.Text = IO.Path.GetDirectoryName(File)
    tbsTags.TabPages.Clear()
    If File.EndsWith("VIDEO_TS", True, Nothing) Then
      txtType.Text = "Digital Video Disc"
      txtSize.Text = ByteSize(My.Computer.FileSystem.GetDriveInfo(File.Substring(0, 3)).TotalSize) & " (" & FormatNumber(My.Computer.FileSystem.GetDriveInfo(File.Substring(0, 3)).TotalSize, 0) & " bytes)"
    Else
      'If LoadVorbis() Then tbsTags.TabPages.Add(tabVorbis)
      'If LoadMKV() Then tbsTags.TabPages.Add(tabMKV)
      'If LoadID3v1() Then tbsTags.TabPages.Add(tabID3v1)
      'If LoadID3v2() Then tbsTags.TabPages.Add(tabID3v2)
      'If LoadRIFF() Then tbsTags.TabPages.Add(tabRIFF)
      'If LoadMPEG() Then tbsTags.TabPages.Add(tabMPEG)
      'If LoadMIDI() Then tbsTags.TabPages.Add(tabMIDI)
      Select Case IO.Path.GetExtension(File).ToLower.Substring(1)
        Case "mp3"
          If LoadID3v1() Then tbsTags.TabPages.Add(tabID3v1)
          If LoadID3v2() Then tbsTags.TabPages.Add(tabID3v2)
          If LoadMPEG() Then tbsTags.TabPages.Add(tabMPEG)
        Case "mpe", "mpg", "mpeg", "m1v", "mp2", "m2v", "mp2v", "mpv2", "mpa", "m2ts"
          If LoadMPEG() Then tbsTags.TabPages.Add(tabMPEG)
        Case "ogg", "ogm"
          If LoadVorbis() Then tbsTags.TabPages.Add(tabVorbis)
        Case "flac"
          If LoadVorbis() Then tbsTags.TabPages.Add(tabVorbis)
          If LoadID3v1() Then tbsTags.TabPages.Add(tabID3v1)
          If LoadID3v2() Then tbsTags.TabPages.Add(tabID3v2)
        Case "mkv"
          If LoadMKV() Then tbsTags.TabPages.Add(tabMKV)
        Case "mid", "midi", "rmi"
          If LoadMIDI() Then tbsTags.TabPages.Add(tabMIDI)
        Case "wav", "avi"
          If LoadRIFF() Then tbsTags.TabPages.Add(tabRIFF)
        Case Else
          If LoadVorbis() Then tbsTags.TabPages.Add(tabVorbis)
          If LoadMKV() Then tbsTags.TabPages.Add(tabMKV)
          If LoadID3v1() Then tbsTags.TabPages.Add(tabID3v1)
          If LoadID3v2() Then tbsTags.TabPages.Add(tabID3v2)
          If LoadRIFF() Then tbsTags.TabPages.Add(tabRIFF)
          If LoadMPEG() Then tbsTags.TabPages.Add(tabMPEG)
          If LoadMIDI() Then tbsTags.TabPages.Add(tabMIDI)
      End Select
      Select Case IO.Path.GetExtension(File).ToLower.Substring(1)
        Case "mpc" : txtType.Text = "Musepack Audio File"
        Case "ac3" : txtType.Text = "Dolby Digital Advanced Coding-3 Audio File"
        Case "aif", "aiff" : txtType.Text = "Audio Interchange File"
        Case "aifc" : txtType.Text = "Audio Interchange File (Compressed)"
        Case "asf" : txtType.Text = "Advanced Systems File"
        Case "asx" : txtType.Text = "Advanced Systems Redirector"
        Case "au" : txtType.Text = "Audio Clip"
        Case "snd" : txtType.Text = "Sound Clip"
        Case "ape" : txtType.Text = "Monkey's Audio Lossless Compression File"
        Case "flac" : txtType.Text = "Free Lossless Audio Codec File"
        Case "ofr" : txtType.Text = "OptimFROG Encoded Audio File"
        Case "tta" : txtType.Text = "TrueAudio Free Lossless Audio Codec File"
        Case "wav" : txtType.Text = "Waveform Audio File"
        Case "mid", "midi" : txtType.Text = "Musical Instrument Digital Interface File"
        Case "rmi" : txtType.Text = "RIFF Encoded MIDI File"
        Case "avi" : txtType.Text = "Audio/Video Interleave File"
        Case "divx" : txtType.Text = "DivX Video File"
        Case "ivf" : txtType.Text = "Indeo Video File"
        Case "mkv" : txtType.Text = "Matroska Video File"
        Case "mka" : txtType.Text = "Matroska Audio File"
        Case "flv" : txtType.Text = "Adobe Flash Video File"
        Case "spl" : txtType.Text = "Adobe FutureSplash Object"
        Case "swf" : txtType.Text = "Adobe Shockwave Flash Object"
        Case "cda" : txtType.Text = "CD Audio Track Shortcut"
        Case "dat" : txtType.Text = "Video CD MPEG Movie File"
        Case "vob" : txtType.Text = "DVD Video File"
        Case "mpe" : txtType.Text = "MPEG-1 Movie Clip"
        Case "mpg", "mpeg" : txtType.Text = "MPEG-1 Layer I System Stream File"
        Case "m1v" : txtType.Text = "MPEG-1 Layer I Video File"
        Case "mp2" : txtType.Text = "MPEG-1 Layer II Audio File"
        Case "m2v", "mp2v", "mpv2" : txtType.Text = "MPEG-1 Layer II Video File"
        Case "mp3" : txtType.Text = "MPEG-1 Layer III Audio File"
        Case "mpa" : txtType.Text = "MPEG-1 Layer I Audio Stream"
        Case "aac" : txtType.Text = "MPEG-2 Advanced Audio Coding File"
        Case "m2ts" : txtType.Text = "MPEG-2 Transport Stream"
        Case "m4a" : txtType.Text = "MPEG-4 Audio Layer File"
        Case "m4p" : txtType.Text = "Protected Advanced Audio Coding File"
        Case "m4v", "mp4" : txtType.Text = "MPEG-4 Video File"
        Case "ogg" : txtType.Text = "OGG Vorbis File"
        Case "ogm" : txtType.Text = "OGG Vorbis Media Compressed Video File"
        Case "mov", "qt" : txtType.Text = "QuickTime Movie File"
        Case "dv", "dif" : txtType.Text = "Digital Video File"
        Case "3gp", "3gpp" : txtType.Text = "Third Generation Partnership Project File"
        Case "3g2", "3gp2" : txtType.Text = "Third Generation Partnership Project Version 2 File"
        Case "ra" : txtType.Text = "RealAudio File"
        Case "ram" : txtType.Text = "RealAudio Metafile"
        Case "rm" : txtType.Text = "RealMedia File"
        Case "rmm" : txtType.Text = "RealMedia Metafile"
        Case "rv" : txtType.Text = "RealVideo File"
        Case "vfw" : txtType.Text = "Video For Windows File"
        Case "wmp" : txtType.Text = "Windows Media Player File"
        Case "wm" : txtType.Text = "Windows Media Audio/Video File"
        Case "wmx" : txtType.Text = "Windows Media Audio/Video Redirector"
        Case "wma" : txtType.Text = "Windows Media Audio File"
        Case "wax" : txtType.Text = "Windows Media Audio Redirector"
        Case "wmv" : txtType.Text = "Windows Media Video File"
        Case "wvx" : txtType.Text = "Windows Media Video Redirector"
        Case "m3u" : txtType.Text = "MP3 Playlist File"
        Case "pls" : txtType.Text = "Generic Playlist File"
        Case "llpl" : txtType.Text = "Lime Light PlayList File"
        Case "gif" : txtType.Text = "Graphics Interchange Format Animated Image File"
        Case Else : txtType.Text = "Unknown File"
      End Select
      txtSize.Text = ByteSize(My.Computer.FileSystem.GetFileInfo(File).Length) & " (" & FormatNumber(My.Computer.FileSystem.GetFileInfo(File).Length, 0) & " bytes)"
    End If

    mpPreview.Mute = True
    mpPreview.FileName = File
    Dim Filters() As String = Nothing
    mpPreview.GetFilters(Filters)
    If mpPreview.IsFlash Then
      txtLength.Text = mpPreview.Duration & " frames"
      pctPreview.Image = Nothing
      pctPreview.SizeMode = PictureBoxSizeMode.Zoom
      txtVideoSize.Text = mpPreview.VideoWidth & " x " & mpPreview.VideoHeight
    Else
      txtLength.Text = ConvertTimeVal(mpPreview.Duration)
      If mpPreview.HasVid Then

        pctPreview.Image = Nothing
        pctPreview.SizeMode = PictureBoxSizeMode.Zoom
        txtVideoSize.Text = mpPreview.VideoWidth & " x " & mpPreview.VideoHeight
      Else
        pctPreview.Image = Nothing
        pctPreview.SizeMode = PictureBoxSizeMode.Zoom
        txtVideoSize.Text = "No video"
      End If
    End If
    txtCreated.Text = My.Computer.FileSystem.GetFileInfo(File).CreationTime.ToString
    If Filters IsNot Nothing Then
      lstFilters.Enabled = True
      cmdFilterProps.Enabled = True
      For I = 0 To UBound(Filters)
        lstFilters.Items.Add(Filters(I))
      Next I
    Else
      lstFilters.Enabled = False
      cmdFilterProps.Enabled = False
    End If
    Me.Opacity = 0
    Me.Show(frmMain)
    tmrGenPreview.Interval = 50
    tmrGenPreview.Enabled = True
    Application.DoEvents()
    tabProps.SelectedIndex = 1
  End Sub

  Private Sub cmdOK_Click(sender As System.Object, e As System.EventArgs) Handles cmdOK.Click
    Me.Close()
  End Sub

  Private Sub frmProps_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
    pctPreview.Image = Nothing
    If mpPreview IsNot Nothing Then
      mpPreview.Dispose()
      mpPreview = Nothing
    End If
  End Sub

  Private Sub cmdID3v1Reset_Click(sender As System.Object, e As System.EventArgs) Handles cmdID3v1Reset.Click
    LoadID3v1()
  End Sub

  Private Sub cmdID3v1Save_Click(sender As System.Object, e As System.EventArgs) Handles cmdID3v1Save.Click
    mpPreview.FileName = Nothing
    Using ID3v1Tags As New Seed.clsID3v1(sFile)
      ID3v1Tags.Track = dgvID3v1.Rows(0).Cells(1).Value
      ID3v1Tags.Title = dgvID3v1.Rows(1).Cells(1).Value
      ID3v1Tags.Artist = dgvID3v1.Rows(2).Cells(1).Value
      ID3v1Tags.Album = dgvID3v1.Rows(3).Cells(1).Value
      Dim cmbCell As DataGridViewComboBoxCell = dgvID3v1.Rows(4).Cells(1)
      Dim iVal As Byte = &HC
      For I As Integer = 0 To cmbCell.Items.Count - 1
        If cmbCell.Items(I) = cmbCell.Value Then
          iVal = I
          Exit For
        End If
      Next
      ID3v1Tags.Genre = iVal
      ID3v1Tags.Year = dgvID3v1.Rows(5).Cells(1).Value
      ID3v1Tags.Comment = dgvID3v1.Rows(6).Cells(1).Value
      ID3v1Tags.Save()
      MsgBox("ID3v1 Tags for " & IO.Path.GetFileName(sFile) & " have been saved!", MsgBoxStyle.Information, "ID3v1 Tags Saved!")
    End Using
    mpPreview.FileName = sFile
  End Sub

  Private Sub cmdID3v2Add_Click(sender As System.Object, e As System.EventArgs) Handles cmdID3v2Add.Click
    Using newTag As New frmNewID3v2Tag
      If newTag.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
        Dim TagID As frmNewID3v2Tag.ID3FrameList = CType(newTag.cmbType.SelectedItem, frmNewID3v2Tag.ID3FrameList)
        Select Case TagID.ID.Substring(0, 1)
          Case "W"
            dgvID3v2.Rows.Add(GenerateLinkRow(TagID.Name, "http://", "http://", TagID.ID))
          Case "T"

        End Select

      End If
    End Using
  End Sub

  Private Sub cmdID3v2Reset_Click(sender As System.Object, e As System.EventArgs) Handles cmdID3v2Reset.Click
    LoadID3v2()
  End Sub

  Private Sub cmdID3v2Save_Click(sender As Object, e As System.EventArgs) Handles cmdID3v2Save.Click
    mpPreview.FileName = Nothing
    Using ID3v2Tags As New Seed.clsID3v2(sFile)
      For Each row As DataGridViewRow In dgvID3v2.Rows
        ID3v2Tags.SetFrame(row.Tag, row.Cells(1).Value)

      Next
      ID3v2Tags.Save()
      MsgBox("ID3v2 Tags for " & IO.Path.GetFileName(sFile) & " have been saved!", MsgBoxStyle.Information, "ID3v2 Tags Saved!")
    End Using
  End Sub

  Private Sub DataGrid_CellContentClick(sender As Object, e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvID3v2.CellContentClick, dgvVorbis.CellContentClick
    If CType(sender, DataGridView).Rows(e.RowIndex).Cells(e.ColumnIndex).GetType Is GetType(DataGridViewButtonCell) Then
      Dim cmdPicture As DataGridViewButtonCell = CType(sender, DataGridView).Rows(e.RowIndex).Cells(e.ColumnIndex)
      Dim sMIME As String = cmdPicture.Tag(0)
      Dim sDesc As String = cmdPicture.Tag(1)
      Dim bID As Byte = cmdPicture.Tag(2)
      Dim iImg As Drawing.Image = cmdPicture.Tag(3)
      Dim frmTmp As New Form With {.FormBorderStyle = Windows.Forms.FormBorderStyle.SizableToolWindow, .Text = "[" & Seed.clsID3v2.ImageID(bID) & "] " & sDesc, .Size = New Drawing.Size(300, 320), .StartPosition = FormStartPosition.CenterParent, .ShowInTaskbar = False, .BackColor = Drawing.Color.Black, .Margin = New Padding(0), .Padding = New Padding(0)}
      Dim pctDisp As New PictureBox With {.Dock = DockStyle.Fill, .BorderStyle = BorderStyle.None, .SizeMode = PictureBoxSizeMode.Zoom, .Margin = New Padding(0), .Padding = New Padding(0)}
      frmTmp.Controls.Add(pctDisp)
      pctDisp.Image = iImg.Clone
      frmTmp.Show(Me)
    End If
  End Sub

  Private Sub dgvID3v2_CellLeave(sender As Object, e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvID3v2.CellLeave
    If dgvID3v2.Rows(e.RowIndex).Height > 22 Then dgvID3v2.Rows(e.RowIndex).Height = 22
  End Sub

  Private Sub dgvID3v2_EditingControlShowing(sender As Object, e As System.Windows.Forms.DataGridViewEditingControlShowingEventArgs) Handles dgvID3v2.EditingControlShowing
    If e.Control.GetType Is GetType(Button) Then
      Dim btn As Button = e.Control
      AddHandler btn.Click, AddressOf id3v2Button_Click
    ElseIf e.Control.GetType Is GetType(DataGridViewTextBoxEditingControl) Then
      Dim txt As DataGridViewTextBoxEditingControl = e.Control
      If txt.Text.Contains(vbCr) Then
        txt.Multiline = True
        txt.ScrollBars = ScrollBars.Vertical
        dgvID3v2.SelectedRows(0).Height = 200
      End If
    End If
  End Sub

  Private Sub id3v2Button_Click(sender As Object, e As EventArgs)
    Stop
  End Sub

  Private Sub dgvID3v1_CellLeave(sender As Object, e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvID3v1.CellLeave
    If e.RowIndex = 0 Then
      If dgvID3v1.Rows(0).Cells(1).Value = "0" Then
        CType(dgvID3v1.Rows(6).Cells(1), DataGridViewTextBoxCell).MaxInputLength = 30
      Else
        CType(dgvID3v1.Rows(6).Cells(1), DataGridViewTextBoxCell).MaxInputLength = 28
      End If
    End If
  End Sub

  Private Sub tmrGenPreview_Tick(sender As System.Object, e As System.EventArgs) Handles tmrGenPreview.Tick
    Static GenNum As Integer
    tmrGenPreview.Enabled = False
    Select Case GenNum
      Case 0
        mpPreview.FullScreenObj = pctPreview
        mpPreview.FullScreen = True
        tmrGenPreview.Interval = 50
        tmrGenPreview.Enabled = True
      Case 1
        If mpPreview.IsFlash Then
          mpPreview.Width = mpPreview.VideoWidth
          mpPreview.Height = mpPreview.VideoHeight
          pctPreview.Image = mpPreview.GetFileThumbnail(, True)
          pctPreview.SizeMode = PictureBoxSizeMode.Zoom
        Else
          If mpPreview.HasVid Then
            pctPreview.Image = mpPreview.GetFileThumbnail(mpPreview.FileName, True)
            pctPreview.SizeMode = PictureBoxSizeMode.Zoom
          Else
            pctPreview.Image = Nothing
            pctPreview.SizeMode = PictureBoxSizeMode.Zoom
          End If
        End If
        tmrGenPreview.Interval = 100
        tmrGenPreview.Enabled = True
      Case Else
        mpPreview.FullScreen = False
        mpPreview.FullScreenObj = Nothing
        tmrGenPreview.Enabled = False
        tabProps.SelectedIndex = 0
        Me.Opacity = 1
    End Select
    GenNum += 1
  End Sub
End Class