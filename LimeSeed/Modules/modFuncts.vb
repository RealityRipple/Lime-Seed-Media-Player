Imports System.Runtime.InteropServices
Imports System.IO

Module modFuncts
  Public Const LATIN_1 As Integer = 28591
  Public Const UTF_8 As Integer = 949
  Public objStorageLock As New Object
  Private Declare Function ShowCursor Lib "user32" (bShow As Integer) As Integer
  Private Declare Function SystemParametersInfo Lib "user32" Alias "SystemParametersInfoA" (uAction As Integer, ByVal uParam As Integer, ByVal lpvParam As Integer, ByVal fuWinIni As Integer) As Integer
  Private Declare Sub ReleaseCapture Lib "user32" ()
  Private Declare Sub SendMessage Lib "user32" Alias "SendMessageA" (ByVal hWnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As Integer)
  Private Const WM_NCLBUTTONDOWN As Integer = &HA1
  Private Const HTCAPTION As Integer = 2
  Public Sub ClickDrag(hWnd As IntPtr)
    ReleaseCapture()
    SendMessage(hWnd, WM_NCLBUTTONDOWN, HTCAPTION, 0&)
  End Sub

  Public Sub EnableScreenSaver(bStatus As Boolean)
    Static bSet As Boolean
    If Not bSet = bStatus Then SystemParametersInfo(&H11, IIf(bStatus, 1, 0), 0, 0)
    bSet = bStatus
  End Sub

  Public Function StrToLong(inStr As String) As Long
    Dim Octets() As String = Split(inStr, ".", 4)
    Dim ocVals(3) As String
    For I As Integer = 0 To 3
      ocVals(I) = Hex(Octets(3 - I)) : If ocVals(I).Length = 1 Then ocVals(I) = "0" & ocVals(I)
    Next
    Return Long.Parse(Join(ocVals, ""), Globalization.NumberStyles.HexNumber)
  End Function

  Public Function StrEquiv(strA As String, strB As String) As Boolean
    strA = strA.Trim.Normalize.ToLower
    strB = strB.Trim.Normalize.ToLower
    Return (strA.Contains(strB) Or strB.Contains(strA) Or String.Compare(strA, strB, True) = 0)
  End Function

  Public Function BufferHex(lVal As Integer, Optional ByVal lCols As Integer = 2) As String
    Dim sHex As String = Hex(lVal)
    If sHex.Length < lCols Then
      Return StrDup(lCols - sHex.Length, "0"c) & sHex
    Else
      Return sHex
    End If
  End Function

  Public Function GetDWORD(bIn As Byte(), Optional ByVal lStart As Long = 0) As UInt32
    Dim bTmp(3) As Byte
    If lStart + 3 >= bIn.Length Then
      bTmp(0) = 0
    Else
      bTmp(0) = bIn(lStart + 3)
    End If
    If lStart + 2 >= bIn.Length Then
      bTmp(1) = 0
    Else
      bTmp(1) = bIn(lStart + 2)
    End If
    If lStart + 1 >= bIn.Length Then
      bTmp(2) = 0
    Else
      bTmp(2) = bIn(lStart + 1)
    End If
    If lStart >= bIn.Length Then
      bTmp(3) = 0
    Else
      bTmp(3) = bIn(lStart)
    End If
    Return BitConverter.ToUInt32(bTmp, 0)
  End Function

  Public Function GenNextRndFile(Optional Extension As String = "mp3") As String
    SyncLock objStorageLock
      Static rI As Integer
      Dim sTmp As String
      Do
        rI += 1
        If rI > &HFFFFFF Then rI = 1
        sTmp = IO.Path.GetTempPath & "lsmpTEMP" & BufferHex(rI, 6) & "." & Extension
      Loop While IO.File.Exists(sTmp)
      Return sTmp
    End SyncLock
  End Function

  Public Sub WipeRndFiles()
    For Each Item In IO.Directory.GetFiles(IO.Path.GetTempPath, "lsmpTEMP*.*")
      Try
        IO.File.Delete(Item)
      Catch
        'Debug.Print(Item & " is busy...")
      End Try
    Next
    For Each Item In IO.Directory.GetFiles(IO.Path.GetTempPath, "seedTEMP*.*")
      Try
        IO.File.Delete(Item)
      Catch
        'Debug.Print(Item & " is busy...")
      End Try
    Next
  End Sub

  Public Function ConvertTimeVal(Seconds As Double) As String
    If Seconds < 0 Then Return "--:--"
    Dim lDays As Long = Seconds \ 60 \ 60 \ 24
    Seconds = Seconds - (lDays * 60 * 60 * 24)
    Dim lHours As Long = Seconds \ 60 \ 60
    Seconds = Seconds - (lHours * 60 * 60)
    Dim lMinutes As Long = Seconds \ 60
    Dim lSeconds As Long = Seconds - (lMinutes * 60)
    If lDays > 0 Then
      ConvertTimeVal = lDays & ":" & Format(lHours, "00") & ":" & Format(lMinutes, "00") & ":" & Format(lSeconds, "00")
    ElseIf lHours > 0 Then
      ConvertTimeVal = lHours & ":" & Format(lMinutes, "00") & ":" & Format(lSeconds, "00")
    Else
      ConvertTimeVal = lMinutes & ":" & Format(lSeconds, "00")
    End If
  End Function

  Public Function RevertTimeVal(Time As String) As Double
    Dim dRet As Double = 0
    If Time = "--:--" Then Return -1
    If Time.Contains(":") Then
      Dim parts() As String = Split(Time, ":")
      If parts.Count = 4 Then
        dRet = parts(0) * 60 * 60 * 24
        dRet += parts(1) * 60 * 60
        dRet += parts(2) * 60
        dRet += parts(3)
      ElseIf parts.Count = 3 Then
        dRet = parts(0) * 60 * 60
        dRet += parts(1) * 60
        dRet += parts(2)
      ElseIf parts.Count = 2 Then
        dRet = parts(0) * 60
        dRet += parts(1)
      Else
        dRet = -1
      End If
    ElseIf Time.EndsWith("frames") Then
      dRet = Val(Time) / 10
    Else
      dRet = CDbl(Time)
    End If
      Return dRet
  End Function

  Public Function TrackToNo(sPath As String) As Integer
    Dim trackNo As Integer = 1
    If IO.Path.GetExtension(sPath).ToLower = ".cda" Then
      trackNo = CInt(IO.Path.GetFileNameWithoutExtension(sPath).Substring(5, 2))
    Else
      trackNo = CInt(sPath.Substring(8))
    End If
    Return trackNo
  End Function

  Public Function ByteSize(InBytes As UInt64) As String
    If InBytes >= 1000 Then
      If InBytes / 1024 >= 1000 Then
        If InBytes / 1024 / 1024 >= 1000 Then
          If InBytes / 1024 / 1024 / 1024 >= 1000 Then
            If InBytes / 1024 / 1024 / 1024 / 1024 >= 1000 Then
              Return Format((InBytes) / 1024 / 1024 / 1024 / 1024 / 1024, "0.##") & " PB"
            Else
              Return Format((InBytes) / 1024 / 1024 / 1024 / 1024, "0.##") & " TB"
            End If
          Else
            Return Format((InBytes) / 1024 / 1024 / 1024, "0.##") & " GB"
          End If
        Else
          Return Format((InBytes) / 1024 / 1024, "0.##") & " MB"
        End If
      Else
        Return Format((InBytes) / 1024, "0.#") & " KB"
      End If
    Else
      Return InBytes & " B"
    End If
  End Function

  Public Function KRater(bitrate As Long, ext As String) As String
    Select Case bitrate
      Case Is >= 1000 * 1000
        Return Format(bitrate / 1000 / 1000, "0.##") & " m" & ext
      Case Is >= 1000
        Return Format(bitrate / 1000, "0.##") & " k" & ext
      Case Is > 0
        Return bitrate & " " & ext
      Case Is = 0
        Return "Unset"
      Case Else
        Return "Invalid [" & bitrate & "]"
    End Select
  End Function

  Public Function CheckMPEG(bFile As Byte(), ByVal lPos As Long) As Boolean
    Dim lLen As Long
    Dim cMPTest As New Seed.clsMPEG(GetDWORD(bFile, lPos))
    If cMPTest.CheckValidity Then
      lLen = cMPTest.GetFrameSize
      lPos += lLen
      If lPos = bFile.Length Then Return True 'End of File, used to be false, but set to true because technically an MP3 file could only contain a single frame and then end...
      If lPos > bFile.Length Then Return False 'However, it should not be greater than this end, because that would be an incomplete frame or a fake header.
      cMPTest = New Seed.clsMPEG(GetDWORD(bFile, lPos))
      If cMPTest.CheckValidity Then
        lLen = cMPTest.GetFrameSize
        lPos += lLen
        If lPos = bFile.Length Then Return True
        If lPos > bFile.Length Then Return False
        cMPTest = New Seed.clsMPEG(GetDWORD(bFile, lPos))
        If cMPTest.CheckValidity Then
          lLen = cMPTest.GetFrameSize
          lPos += lLen
          If lPos > bFile.Length Then Return False
          Return True
        End If
      End If
    End If
    cMPTest = Nothing
    Return False
  End Function

  Public Sub GetMKVPDSizes(ByRef mkvHeader As Seed.clsMKVHeaders, ByRef pixelSize As Drawing.Size, ByRef displaySize As Drawing.Size)
    For I As Integer = 0 To mkvHeader.TrackEntries.Length - 1
      If mkvHeader.TrackEntries(I).Video.Exists Then
        Dim pX As UInt64 = mkvHeader.TrackEntries(I).Video.PixelWidth
        Dim pY As UInt64 = mkvHeader.TrackEntries(I).Video.PixelHeight
        Dim pRX As UInt64 = mkvHeader.TrackEntries(I).Video.DisplayWidth
        Dim pRY As UInt64 = mkvHeader.TrackEntries(I).Video.DisplayHeight
        pixelSize = New Drawing.Size(pX, pY)
        displaySize = New Drawing.Size(pRX, pRY)
        Return
      End If
    Next I
    pixelSize = Drawing.Size.Empty
    displaySize = Drawing.Size.Empty
  End Sub

  Public Sub GetMKVDisplaySize(ByRef mkvHeader As Seed.clsMKVHeaders, ByRef Size As Drawing.Size, ByRef Crop As Drawing.Rectangle)
    For I As Integer = 0 To mkvHeader.TrackEntries.Length - 1
      If mkvHeader.TrackEntries(I).Video.Exists Then
        If mkvHeader.TrackEntries(I).Video.PixelCropLeft > 0 Or mkvHeader.TrackEntries(I).Video.PixelCropTop > 0 Or mkvHeader.TrackEntries(I).Video.PixelCropBottom > 0 Or mkvHeader.TrackEntries(I).Video.PixelCropRight > 0 Then Crop = New Drawing.Rectangle(mkvHeader.TrackEntries(I).Video.PixelCropLeft, mkvHeader.TrackEntries(I).Video.PixelCropTop, mkvHeader.TrackEntries(I).Video.PixelCropRight, mkvHeader.TrackEntries(I).Video.PixelCropBottom)
        Dim pX As UInt64 = mkvHeader.TrackEntries(I).Video.PixelWidth
        Dim pY As UInt64 = mkvHeader.TrackEntries(I).Video.PixelHeight
        Dim pRX As UInt64 = mkvHeader.TrackEntries(I).Video.DisplayWidth
        Dim pRY As UInt64 = mkvHeader.TrackEntries(I).Video.DisplayHeight
        If pRX > 0 And pRY > 0 Then
          Select Case mkvHeader.TrackEntries(I).Video.DisplayUnit
            Case 0
              If pRX < 100 And pRY < 100 Then
                'probably a ratio, not a resolution!
                If pX / pRX * pRY = pY Then
                  'No need to change!
                  Size = New Drawing.Size(pX, pY)
                  Exit For
                ElseIf pX / pRX * pRY > pY Then
                  Size = New Drawing.Size(pX, pX / pRX * pRY)
                  Exit For
                Else
                  Size = New Drawing.Size(pX / pRX * pRY, pY)
                  Exit For
                End If
                Size = New Drawing.Size(pRX, mkvHeader.TrackEntries(I).Video.DisplayHeight)
                Exit For
              Else
                Size = New Drawing.Size(pRX, pRY)
                Exit For
              End If
            Case 1
              Dim g As Drawing.Graphics = Drawing.Graphics.FromImage(New Drawing.Bitmap(1, 1))
              Size = New Drawing.Size(pRX * (g.DpiX * 2.54), pRY * (g.DpiY * 2.54))
              Exit For
            Case 2
              Dim g As Drawing.Graphics = Drawing.Graphics.FromImage(New Drawing.Bitmap(1, 1))
              Size = New Drawing.Size(pRX * g.DpiX, pRY * g.DpiY)
              Exit For
            Case 3
              'ensure ratio, increase a dimension if necessary
              If pX > 0 And pY > 0 Then
                If pX / pRX * pRY = pY Then
                  'No need to change!
                  Size = New Drawing.Size(pX, pY)
                  Exit For
                ElseIf pX / pRX * pRY > pY Then
                  Size = New Drawing.Size(pX, pX / pRX * pRY)
                  Exit For
                Else
                  Debug.Print("Less")
                  Size = New Drawing.Size(pX / pRX * pRY, pY)
                  Exit For
                End If
              End If
            Case 255
              If pRX < 100 And pRY < 100 Then
                'probably a ratio, not a resolution!
                If pX / pRX * pRY = pY Then
                  'No need to change!
                  Size = New Drawing.Size(pX, pY)
                  Exit For
                ElseIf pX / pRX * pRY > pY Then
                  Size = New Drawing.Size(pX, pX / pRX * pRY)
                  Exit For
                Else
                  Size = New Drawing.Size(pX / pRX * pRY, pY)
                  Exit For
                End If
                Size = New Drawing.Size(pRX, mkvHeader.TrackEntries(I).Video.DisplayHeight)
                Exit For
              Else
                Size = New Drawing.Size(pRX, pRY)
                Exit For
              End If
          End Select
        End If
        If pX > 0 And pY > 0 Then
          Size = New Drawing.Size(pX, pY)
          Exit For
        End If
      End If
    Next
  End Sub

  Public Sub FindAdditionalMKVChapters(Path As String, Direction As Byte, ByRef ChapterCollection As List(Of frmMain.ChapterListing), ByRef RunningTime As ULong)
    If IO.File.Exists(Path) Then
      Dim SearchDir As String = IO.Path.GetDirectoryName(Path)
      If Not SearchDir.EndsWith(IO.Path.DirectorySeparatorChar) Then SearchDir &= IO.Path.DirectorySeparatorChar
      Using mkvHeader As New Seed.clsMKVHeaders(Path)
        If mkvHeader.HasMKV Then
          If Direction = 0 Then

            Dim SearchPath As String = ""
            If Not String.IsNullOrEmpty(mkvHeader.SegmentInfo.PrevFilename) Then
              SearchPath = SearchDir & mkvHeader.SegmentInfo.PrevFilename
            ElseIf Not mkvHeader.SegmentInfo.PrevUID Is Nothing AndAlso Not mkvHeader.SegmentInfo.PrevUID.Count = 0 Then
              For Each sFile As String In IO.Directory.GetFiles(SearchDir, "*.mkv")
                Using testMKVHeader As New Seed.clsMKVHeaders(sFile)
                  If testMKVHeader.HasMKV Then
                    If BitConverter.ToString(testMKVHeader.SegmentInfo.SegmentUID) = BitConverter.ToString(mkvHeader.SegmentInfo.PrevUID) Then
                      SearchPath = sFile
                      Exit For
                    End If
                  End If
                End Using
              Next
            End If

            If Not String.IsNullOrEmpty(SearchPath) Then
              If IO.File.Exists(SearchPath) Then
                Dim searchMKVHeader As New Seed.clsMKVHeaders(SearchPath)
                If searchMKVHeader.HasMKV Then

                  Dim PrevPath As String = ""
                  If Not String.IsNullOrEmpty(searchMKVHeader.SegmentInfo.PrevFilename) Then
                    PrevPath = SearchDir & searchMKVHeader.SegmentInfo.PrevFilename
                  ElseIf Not searchMKVHeader.SegmentInfo.PrevUID Is Nothing AndAlso Not searchMKVHeader.SegmentInfo.PrevUID.Count = 0 Then
                    For Each sFile As String In IO.Directory.GetFiles(SearchDir, "*.mkv")
                      Using testMKVHeader As New Seed.clsMKVHeaders(sFile)
                        If testMKVHeader.HasMKV Then
                          If BitConverter.ToString(testMKVHeader.SegmentInfo.SegmentUID) = BitConverter.ToString(searchMKVHeader.SegmentInfo.PrevUID) Then
                            PrevPath = sFile
                            Exit For
                          End If
                        End If
                      End Using
                    Next
                  End If
                  If Not String.IsNullOrEmpty(PrevPath) Then
                    If IO.File.Exists(PrevPath) Then
                      FindAdditionalMKVChapters(SearchPath, Direction, ChapterCollection, RunningTime)
                    End If
                  End If

                  Debug.Print("Adding Chapters from " & SearchPath)
                  For Each Edition In searchMKVHeader.ChapterInfo.Editions
                    If Not Edition.FlagHidden Then
                      Dim LastAtomEnd As Double = 0.0
                      For Each Atom In Edition.Atoms
                        If Not Atom.FlagHidden Then
                          Dim dChapterStart As Double = CDbl(RunningTime + Atom.TimeStart / 1000000000.0)
                          If dChapterStart = 0.0 Then dChapterStart = LastAtomEnd
                          LastAtomEnd = CDbl(RunningTime + Atom.TimeEnd / 1000000000.0)
                          Dim sChapterText As String = Atom.Display(0).Title & ": " & ConvertTimeVal(dChapterStart)
                          Dim sChapterLang As String
                          If Atom.Display(0).Language Is Nothing OrElse Atom.Display(0).Language.Count = 0 Then
                            sChapterLang = "und"
                          ElseIf Atom.Display(0).Language.Count = 1 Then
                            sChapterLang = Atom.Display(0).Language(0)
                          Else
                            sChapterLang = Join(Atom.Display(0).Language, ", ")
                          End If
                          ChapterCollection.Add(New frmMain.ChapterListing(dChapterStart, sChapterText, sChapterLang))
                        End If
                      Next
                    End If
                  Next
                  If searchMKVHeader.SegmentInfo.Duration > 0 Then
                    RunningTime += (searchMKVHeader.SegmentInfo.Duration / 1000)
                  Else
                    Using mpTest As New Seed.ctlSeed
                      RunningTime += mpTest.GetFileDuration(SearchPath)
                    End Using
                  End If
                  Return
                End If
              End If
            End If

          ElseIf Direction = 1 Then

            Dim SearchPath As String = ""
            If Not String.IsNullOrEmpty(mkvHeader.SegmentInfo.NextFilename) Then
              SearchPath = SearchDir & mkvHeader.SegmentInfo.NextFilename
            ElseIf Not mkvHeader.SegmentInfo.NextUID Is Nothing AndAlso Not mkvHeader.SegmentInfo.NextUID.Count = 0 Then
              For Each sFile As String In IO.Directory.GetFiles(SearchDir, "*.mkv")
                Using testMKVHeader As New Seed.clsMKVHeaders(sFile)
                  If testMKVHeader.HasMKV Then
                    If BitConverter.ToString(testMKVHeader.SegmentInfo.SegmentUID) = BitConverter.ToString(mkvHeader.SegmentInfo.NextUID) Then
                      SearchPath = sFile
                      Exit For
                    End If
                  End If
                End Using
              Next
            End If
            If Not String.IsNullOrEmpty(SearchPath) Then
              If IO.File.Exists(SearchPath) Then
                Dim searchMKVHeader As New Seed.clsMKVHeaders(SearchPath)
                If searchMKVHeader.HasMKV Then

                  Debug.Print("Adding Chapters from " & SearchPath)
                  For Each Edition In searchMKVHeader.ChapterInfo.Editions
                    If Not Edition.FlagHidden Then
                      Dim LastAtomEnd As Double = 0.0
                      For Each Atom In Edition.Atoms
                        If Not Atom.FlagHidden Then
                          Dim dChapterStart As Double = CDbl(RunningTime + Atom.TimeStart / 1000000000.0)
                          If dChapterStart = 0.0 Then dChapterStart = LastAtomEnd
                          LastAtomEnd = CDbl(RunningTime + Atom.TimeEnd / 1000000000.0)
                          Dim sChapterText As String = Atom.Display(0).Title & ": " & ConvertTimeVal(dChapterStart)
                          Dim sChapterLang As String
                          If Atom.Display(0).Language Is Nothing OrElse Atom.Display(0).Language.Count = 0 Then
                            sChapterLang = "und"
                          ElseIf Atom.Display(0).Language.Count = 1 Then
                            sChapterLang = Atom.Display(0).Language(0)
                          Else
                            sChapterLang = Join(Atom.Display(0).Language, ", ")
                          End If
                          ChapterCollection.Add(New frmMain.ChapterListing(dChapterStart, sChapterText, sChapterLang))
                        End If
                      Next
                    End If
                  Next
                  If searchMKVHeader.SegmentInfo.Duration > 0 Then
                    RunningTime += (searchMKVHeader.SegmentInfo.Duration / 1000)
                  Else
                    Using mpTest As New Seed.ctlSeed
                      RunningTime += mpTest.GetFileDuration(SearchPath)
                    End Using
                  End If
                  Dim NextPath As String = ""
                  If Not String.IsNullOrEmpty(searchMKVHeader.SegmentInfo.NextFilename) Then
                    NextPath = SearchDir & searchMKVHeader.SegmentInfo.NextFilename
                  ElseIf Not searchMKVHeader.SegmentInfo.NextUID Is Nothing AndAlso Not searchMKVHeader.SegmentInfo.NextUID.Count = 0 Then
                    For Each sFile As String In IO.Directory.GetFiles(SearchDir, "*.mkv")
                      Using testMKVHeader As New Seed.clsMKVHeaders(sFile)
                        If testMKVHeader.HasMKV Then
                          If BitConverter.ToString(testMKVHeader.SegmentInfo.SegmentUID) = BitConverter.ToString(searchMKVHeader.SegmentInfo.NextUID) Then
                            NextPath = sFile
                            Exit For
                          End If
                        End If
                      End Using
                    Next
                  End If
                  If Not String.IsNullOrEmpty(NextPath) Then
                    If IO.File.Exists(NextPath) Then
                      FindAdditionalMKVChapters(SearchPath, Direction, ChapterCollection, RunningTime)
                    End If
                  End If

                  Return
                End If
              End If
            End If

          End If
        End If
      End Using
    End If
  End Sub

  Public Function FindInByteArray(bIn As Byte(), ByVal bFind As Byte(), Optional ByVal lStart As Integer = 0) As Integer
    For I As Integer = lStart To bIn.Length - bFind.Length - 1
      Dim bFound As Boolean = True
      For J As Integer = 0 To bFind.Length - 1
        If bIn(I + J) <> bFind(J) Then
          bFound = False
          Exit For
        End If
      Next
      If bFound Then Return I
    Next
    Return -1
  End Function

  Public Sub MediaClientSetup(Install As Boolean)
    If Install Then
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("RegisteredApplications").SetValue(My.Application.Info.Description, "SOFTWARE\Clients\Media\LSMP\Capabilities")

      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").SetValue(String.Empty, My.Application.Info.Description)
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").SetValue("Application Description", "Lime Seed is a compact, simple media player. It plays most standard file types of audio and video, and also plays Shockwave Flash files.")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").SetValue("Application Icon", Application.StartupPath & ",0")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").SetValue("Application Name", My.Application.Info.Description)

      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".3g2", "3G2")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".3gp", "3GP")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".3gp2", "3GP2")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".3gpp", "3GPP")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".aac", "AAC")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".aif", "AIF")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".aifc", "AIFC")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".aiff", "AIFF")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".ape", "APE")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".asf", "ASF")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".asx", "ASX")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".au", "AU")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".avi", "AVI")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".divx", "DIVX")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".cda", "CDA")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".dat", "DAT")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".dif", "DIF")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".dv", "DV")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".flac", "FLAC")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".flv", "FLV")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".ivf", "IVF")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".llpl", "LLPL")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".m1v", "M1V")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".m2ts", "M2TS")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".m3u", "M3U")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".m3u8", "M3U")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".m4a", "M4A")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".m4p", "M4P")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".m4v", "M4V")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mid", "MID")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".midi", "MIDI")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mkv", "MKV")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mov", "MOV")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mp2", "MP2")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mp2v", "MP2V")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mp3", "MP3")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mp4", "MP4")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mpa", "MPA")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mpc", "MPC")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mpe", "MPE")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mpeg", "MPEG")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mpg", "MPG")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".mpv2", "MPV2")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".ofr", "OFR")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".ogg", "OGG")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".ogm", "OGM")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".pls", "PLS")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".qt", "QT")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".ra", "RA")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".ram", "RAM")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".rm", "RM")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".rmi", "RMI")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".rmm", "RMM")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".rv", "RV")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".snd", "SND")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".spl", "SPL")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".swf", "SWF")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".tta", "TTA")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".vfw", "VFW")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".vob", "VOB")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".wav", "WAV")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".wax", "WAX")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".wm", "WM")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".wma", "WMA")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".wmp", "WMP")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".wmv", "WMV")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".wmx", "WMX")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("Capabilities").CreateSubKey("FileAssociations").SetValue(".wx", "WX")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("InstallInfo").SetValue("HideIconsCommand", """" & Application.StartupPath & """ /hideicons")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("InstallInfo").SetValue("ReinstallCommand", """" & Application.StartupPath & """ /reinstall")
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("InstallInfo").SetValue("IconsVisible", 0, Microsoft.Win32.RegistryValueKind.DWord)
      My.Computer.Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Clients").CreateSubKey("Media").CreateSubKey("LSMP").CreateSubKey("InstallInfo").SetValue("ShowIconsCommand", """" & Application.StartupPath & """ /showicons")
    Else
      My.Computer.Registry.LocalMachine.OpenSubKey("SOFTWARE", True).OpenSubKey("RegisteredApplications", True).DeleteValue(My.Application.Info.Description, False)

      My.Computer.Registry.LocalMachine.OpenSubKey("SOFTWARE", True).OpenSubKey("Clients", True).OpenSubKey("Media", True).DeleteSubKeyTree("LSMP", False)
    End If
  End Sub

  Public Function SafeName(FileName As String) As String
    Dim sSafe As String = FileName.Replace("?", "{QUESTION}").Replace(":", "{COLON}").Replace("*", "{ASTERISK}").Replace("""", "{QUOTE}").Replace(">", "{GREATER}").Replace("<", "{LESS}").Replace("|", "{PIPE}").Replace("\", "{BACKSLASH}").Replace("/", "{SLASH}").Replace("...", "{ELLIPSIS}")
    Do While sSafe.EndsWith(".")
      sSafe = sSafe.Substring(0, sSafe.Length - 1)
    Loop
    Return sSafe
  End Function

  Public Function CoolName(SafeFileName As String) As String
    Return SafeFileName.Replace("{QUESTION}", "？").Replace("{COLON}", ChrW(&HA789)).Replace("{ASTERISK}", "•").Replace("{QUOTE}", "ʺ").Replace("{GREATER}", "˃").Replace("{LESS}", "˂").Replace("{PIPE}", "¦").Replace("{BACKSLASH}", "〵").Replace("{SLASH}", " ⁄ ").Replace("{ELLIPSIS}", "…")
  End Function

  Public Sub SetCursor(Visible As Boolean)
    If Visible Then
      For I As Integer = ShowCursor(1) To 0
        ShowCursor(1)
      Next I
    Else
      For I As Integer = 0 To ShowCursor(1)
        ShowCursor(0)
      Next I
    End If
  End Sub

  Public Sub FFDShowRemote(Enable As Boolean)
    If Enable Then
      If Environment.Is64BitProcess Then
        If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("GNU") IsNot Nothing Then
          If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("GNU").OpenSubKey("ffdshow64") IsNot Nothing Then
            My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow64", True).SetValue("isRemote", 1, Microsoft.Win32.RegistryValueKind.DWord)
            My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow64", True).SetValue("remoteMessageMode", 1, Microsoft.Win32.RegistryValueKind.DWord)
            My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow64", True).SetValue("remoteMessageUser", 32786, Microsoft.Win32.RegistryValueKind.DWord)
          End If
        End If
      Else
        If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("GNU") IsNot Nothing Then
          If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("GNU").OpenSubKey("ffdshow") IsNot Nothing Then
            My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow", True).SetValue("isRemote", 1, Microsoft.Win32.RegistryValueKind.DWord)
            My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow", True).SetValue("remoteMessageMode", 1, Microsoft.Win32.RegistryValueKind.DWord)
            My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow", True).SetValue("remoteMessageUser", 32786, Microsoft.Win32.RegistryValueKind.DWord)
          End If
        End If
      End If
    Else
      If Environment.Is64BitProcess Then
        If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("GNU") IsNot Nothing Then
          If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("GNU").OpenSubKey("ffdshow64") IsNot Nothing Then
            My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow64", True).SetValue("isRemote", 0, Microsoft.Win32.RegistryValueKind.DWord)
            My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow64", True).SetValue("remoteMessageMode", 0, Microsoft.Win32.RegistryValueKind.DWord)
          End If
        End If
      Else
        If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("GNU") IsNot Nothing Then
          If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("GNU").OpenSubKey("ffdshow") IsNot Nothing Then
            My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow", True).SetValue("isRemote", 0, Microsoft.Win32.RegistryValueKind.DWord)
            My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow", True).SetValue("remoteMessageMode", 0, Microsoft.Win32.RegistryValueKind.DWord)
          End If
        End If
      End If
    End If
  End Sub

  Public Function PathToImg(Path As String) As Drawing.Image
    If IO.File.Exists(Path) Then
      If (New IO.FileInfo(Path)).Length >= 1024L * 1024L * 1024L * 4L Then Return Nothing
      Try
        Dim bData As Byte() = IO.File.ReadAllBytes(Path)
        Dim pStream As New IO.MemoryStream(bData)
        Return Drawing.Image.FromStream(pStream, True, True)
      Catch ex As Exception
        Return Nothing
      End Try
    Else
      Return Nothing
    End If
  End Function

  Public Function CompareImages(image1 As Drawing.Image, image2 As Drawing.Image) As Boolean
    Try
      If image1 Is Nothing OrElse image2 Is Nothing Then Return False
      If Not image1.Size = image2.Size Then Return False
      Using ms1 As New IO.MemoryStream, ms2 As New IO.MemoryStream
        image1.Save(ms1, image1.RawFormat)
        image2.Save(ms2, image2.RawFormat)
        Dim ba1 As Byte()
        Dim ba2 As Byte()
        ba1 = ms1.ToArray
        ba2 = ms2.ToArray
        Return ba1.SequenceEqual(ba2)
      End Using
    Catch ex As Exception
      Return False
    End Try
  End Function

  Public Function CompareImages(image1 As Drawing.Icon, image2 As Drawing.Icon) As Boolean
    If image1 Is Nothing OrElse image2 Is Nothing Then Return False
    If Not image1.Size = image2.Size Then Return False
    Using ms1 As New IO.MemoryStream, ms2 As New IO.MemoryStream
      image1.Save(ms1)
      image2.Save(ms2)
      Dim ba1 As Byte()
      Dim ba2 As Byte()
      ba1 = ms1.ToArray
      ba2 = ms2.ToArray
      Return ba1.SequenceEqual(ba2)
    End Using
  End Function

  Public Function FormatBytes(bData As Byte()) As String
    Dim sBuild As New System.Text.StringBuilder
    sBuild.Append("0000   ")
    If bData.Length = 0 Then
      sBuild.Append("(null)")
      Return sBuild.ToString
    End If
    Dim sASCII As New System.Text.StringBuilder(16, 16)
    For I As Integer = 0 To bData.Length - 1
      Dim curData As Char = Chr(bData(I))
      If Char.IsLetterOrDigit(curData) Or Char.IsPunctuation(curData) Or Char.IsSymbol(curData) Or Char.ToString(curData) = " " Then
        sASCII.Append(curData)
      Else
        sASCII.Append(".")
      End If
      sBuild.AppendFormat("{0:x2}", bData(I))
      If (I + 1) Mod 8 = 0 Then sBuild.Append(" ")
      If (I + 1) Mod 16 = 0 Or (I + 1) = bData.Length Then
        If (I + 1) = bData.Length And (I + 1) Mod 16 <> 0 Then
          Dim lenOfCurStr As Integer = (I Mod 16) * 2 + 16
          If ((I + 1) Mod 16) < 8 Then lenOfCurStr -= 1
          For J As Integer = 0 To (47 - lenOfCurStr) - 1
            sBuild.Append(" ")
          Next
        End If
        sBuild.AppendFormat("  {0}", sASCII.ToString)
        sASCII = New System.Text.StringBuilder(16, 16)
        sBuild.Append(vbNewLine)
        If bData.Length > I + 1 Then sBuild.AppendFormat("{0:x4}   ", I + 1)
      End If
    Next
    Return sBuild.ToString
  End Function

  Public Function FastPC(Optional first As Boolean = False) As Boolean
    Static isFast As Boolean
    If first Then
      If Environment.ProcessorCount > 1 Then
        isFast = True
      Else
        Dim CPUSpeed As String = ((New System.Management.ManagementObjectSearcher("root\CIMV2", "SELECT * FROM Win32_Processor")).Get)(0).GetPropertyValue("MaxClockSpeed").ToString
        isFast = False
        Dim RamSize As ULong = My.Computer.Info.TotalPhysicalMemory
        Dim GB As ULong = 1024UL * 1024UL * 1024UL
        If RamSize >= 1 * GB And CPUSpeed > 1500 Then
          isFast = True
        ElseIf RamSize >= 3 * GB And CPUSpeed > 1000 Then
          isFast = True
        ElseIf RamSize >= 0.5 * GB And CPUSpeed > 2000 Then
          isFast = True
        End If
      End If
    End If
    Return isFast
  End Function

  Public Sub MKVAudioCodecs(CodecID As String, ByRef iEncQ As Integer, ByRef sCodec As String)
    Select Case CodecID
      Case "A_MPEG/L3"
        iEncQ = 3
        sCodec = "MPEG Layer III"
      Case "A_MPEG/L2"
        iEncQ = 3
        sCodec = "MPEG Layer II"
      Case "A_MPEG/L1"
        iEncQ = 3
        sCodec = "MPEG Layer I"
      Case "A_PCM/INT/BIG"
        iEncQ = 4
        sCodec = "PCM Integer (Big Endian)"
      Case "A_PCM/INT/LIT"
        iEncQ = 4
        sCodec = "PCM Integer (Little Endian)"
      Case "A_PCM/FLOAT/IEEE"
        iEncQ = 4
        sCodec = "PCM Floating Point (IEEE Compatible)"
      Case "A_MPC"
        iEncQ = 3
        sCodec = "Musepack SV8"
      Case "A_AC3"
        iEncQ = 2
        sCodec = "Dolby™ AC-3"
      Case "A_AC3/BSID9"
        iEncQ = 2
        sCodec = "Dolby™ AC-3 BSID 9"
      Case "A_AC3/BSID10"
        iEncQ = 2
        sCodec = "Dolby™ AC-3 BSID 10"
      Case "A_DTS"
        iEncQ = 2
        sCodec = "DTS"
      Case "A_DTS/EXPRESS"
        iEncQ = 2
        sCodec = "DTS Express"
      Case "A_DTS/LOSSLESS"
        iEncQ = 4
        sCodec = "DTS Lossless"
      Case "A_VORBIS"
        iEncQ = 3
        sCodec = "Vorbis"
      Case "A_FLAC"
        iEncQ = 4
        sCodec = "FLAC"
      Case "A_REAL/14_4"
        iEncQ = 1
        sCodec = "Real Audio 1"
      Case "A_REAL/28_8"
        iEncQ = 1
        sCodec = "Real Audio 2"
      Case "A_REAL/COOK"
        iEncQ = 3
        sCodec = "Real Audio Cook Codec"
      Case "A_REAL/SIPR"
        iEncQ = 3
        sCodec = "Sipro Voice Codec"
      Case "A_REAL/RALF"
        iEncQ = 4
        sCodec = "Real Audio Lossless Format"
      Case "A_REAL/ATRC"
        iEncQ = 3
        sCodec = "Sony Atrac3 Codec"
      Case "A_MS/ACM"
        iEncQ = 4
        sCodec = "Microsoft™ ACM"
      Case "A_AAC"
        iEncQ = 3
        sCodec = "Advanced Audio Codec"
      Case "A_AAC/MPEG2/MAIN"
        iEncQ = 3
        sCodec = "AAC MPEG2 Main Profile"
      Case "A_AAC/MPEG2/LC"
        iEncQ = 3
        sCodec = "AAC MPEG2 Low Complexity Profile"
      Case "A_AAC/MPEG2/LC/SBR"
        iEncQ = 3
        sCodec = "AAC MPEG2 Low Complexity with Spectral Band Replication Profile"
      Case "A_AAC/MPEG2/SSR"
        iEncQ = 3
        sCodec = "AAC MPEG2 Scalable Sampling Rate Profile"
      Case "A_AAC/MPEG4/MAIN"
        iEncQ = 2
        sCodec = "AAC MPEG4 Main Profile"
      Case "A_AAC/MPEG4/LC"
        iEncQ = 2
        sCodec = "AAC MPEG4 Low Complexity Profile"
      Case "A_AAC/MPEG4/LC/SBR"
        iEncQ = 2
        sCodec = "AAC MPEG4 Low Complexity with Spectra Band Replication Profile"
      Case "A_AAC/MPEG4/SSR"
        iEncQ = 2
        sCodec = "AAC MPEG4 Scalable Sampling Rate Profile"
      Case "A_AAC/MPEG4/LTP"
        iEncQ = 2
        sCodec = "AAC MPEG4 Long Term Prediction Profile"
      Case "A_QUICKTIME/QDMC"
        iEncQ = 1
        sCodec = "QuickTime QDesign Music"
      Case "A_QUICKTIME/QDM2"
        iEncQ = 1
        sCodec = "QuickTime QDesign Music v2"
      Case "A_TTA1"
        iEncQ = 4
        sCodec = "The True Audio Lossless"
      Case "A_WAVPACK4"
        iEncQ = 4
        sCodec = "WavPack Losless"
      Case Else
        iEncQ = 1
        sCodec = "Unknown " & CodecID
    End Select
  End Sub

  Public Function MKVVideoCodecs(CodecID As String) As String
    Select Case CodecID
      Case "V_MS/VFW/FOURCC" : Return "Microsoft™ Video Codec Manager"
      Case "V_UNCOMPRESSED" : Return "Uncompressed"
      Case "V_MPEGH/ISO/HEVC" : Return "MPEGH ISO High Efficiency Video Codec"
      Case "V_MPEG4/ISO/SP" : Return "MPEG4 ISO simple profile (DivX4)"
      Case "V_MPEG4/ISO/ASP" : Return "MPEG4 ISO advanced simple profile (DivX4, XviD, FFMPEG)"
      Case "V_MPEG4/ISO/AP" : Return "MPEG4 ISO advanced profile"
      Case "V_MPEG4/ISO/AVC" : Return "MPEG4 AVC"
      Case "V_MPEG4/MS/V3" : Return "Microsoft™ MPEG4 V3"
      Case "V_MPEG1" : Return "MPEG 1"
      Case "V_MPEG2" : Return "MPEG 2"
      Case "V_REAL/RV10" : Return "RealVideo 5"
      Case "V_REAL/RV20" : Return "RealVideo G2"
      Case "V_REAL/RV30" : Return "RealVideo 8"
      Case "V_REAL/RV40" : Return "RealVideo 9"
      Case "V_QUICKTIME" : Return "QuickTime™ Video"
      Case "V_THEORA" : Return "Theora"
      Case "V_PRORES" : Return "Apple ProRes"
      Case Else : Return "Unknown " & CodecID
    End Select
  End Function

  Public Function WAVAudioCodecs(formatTag As Seed.clsRIFF.WAVFormatTag) As String
    Select Case formatTag
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_UNKNOWN : Return "Unknown Wave Format"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_PCM : Return "Microsoft PCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ADPCM : Return "Microsoft ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_IEEE_FLOAT : Return "Float (IEEE)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VSELP : Return "Compaq Computer's VSELP codec for Windows CE 2.0 devices"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_IBM_CVSD : Return "IBM CVSD"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ALAW : Return "A-Law"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_MULAW : Return "µ-Law"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DTS : Return "Digital Theater Systems (DTS)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DRM : Return "DRM Encryped"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_WMAVOICE9 : Return "Windows Media Audio 9 Voice"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_OKI_ADPCM : Return "OKI ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DVI_ADPCM : Return "Intel's DVI ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_MEDIASPACE_ADPCM : Return "Videologic's MediaSpace ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_SIERRA_ADPCM : Return "Sierra ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_G723_ADPCM : Return "G.723 ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DIGISTD : Return "DSP Solution's DIGISTD"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DIGIFIX : Return "DSP Solution's DIGIFIX"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DIALOGIC_OKI_ADPCM : Return "Dialogic OKI ADPCM for OKI ADPCM chips or firmware"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_MEDIAVISION_ADPCM : Return "MediaVision ADPCM for Jazz 16 chip set"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_CU_CODEC : Return "HP CU"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_YAMAHA_ADPCM : Return "Yamaha ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_SONARC : Return "Speech Compression's Sonarc"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DSPGROUP_TRUESPEECH : Return "DSP Group's True Speech"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ECHOSC1 : Return "Echo Speech's EchoSC1"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_AUDIOFILE_AF36 : Return "Audiofile AF36"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_APTX : Return "APTX"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_AUDIOFILE_AF10 : Return "AudioFile AF10"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_PROSODY_1612 : Return "Prosody 1612 CTI Speech Card"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_LRC : Return "LRC"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DOLBY_AC2 : Return "Dolby AC2"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_GSM610 : Return "GSM610"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_MSNAUDIO : Return "Microsoft MSN Audio Codec"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ANTEX_ADPCME : Return "Antex ADPCME"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_CONTROL_RES_VQLPC : Return "Control Res VQLPC"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DIGIREAL : Return "Digireal"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DIGIADPCM : Return "DigiADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_CONTROL_RES_CR10 : Return "Control Res CR10"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_NMS_VBXADPCM : Return "NMS VBXADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_CS_IMAADPCM : Return "Crystal Semiconductor IMA ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ECHOSC3 : Return "EchoSC3 Proprietary Compression"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ROCKWELL_ADPCM : Return "Rockwell ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ROCKWELL_DIGITALK : Return "Rockwell Digit LK DIGITALK"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_XEBEC : Return "Xebec Proprietary Compression"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_G721_ADPCM : Return "Antex Electronics G.721"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_G728_CELP : Return "G.728 CELP"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_MSG723 : Return "MSG723"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_MPEG : Return "MPEG"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_RT24 : Return "Voxware MetaVoice MSRT24"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_PAC : Return "PAC"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_MPEGLAYER3 : Return "ISO/MPEG Layer3"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_LUCENT_G723 : Return "Lucent G.723"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_CIRRUS : Return "Cirrus"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ESPCM : Return "ESPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE : Return "Voxware (Obsolete)"
      Case Seed.clsRIFF.WAVFormatTag.WAVEFORMAT_CANOPUS_ATRAC : Return "Canopus Atrac"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_G726_ADPCM : Return "G.726 ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_G722_ADPCM : Return "G.722 ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DSAT : Return "DSAT"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DSAT_DISPLAY : Return "DSAT Display"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE_BYTE_ALIGNED : Return "Voxware Byte Aligned (Obsolete)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE_AC8 : Return "Voxware AC8 (Obsolete)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE_AC10 : Return "Voxware AC10 (Obsolete)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE_AC16 : Return "Voxware AC16 (Obsolete)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE_AC20 : Return "Voxware AC20 (Obsolete)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE_RT24 : Return "Voxware MetaVoice RT24"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE_RT29 : Return "Voxware MetaSound RT29"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE_RT29HW : Return "Voxware MetaSound Hardware RT29HW (Obsolete)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE_VR12 : Return "Voxware VR12 (Obsolete)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE_VR18 : Return "Voxware VR18 (Obsolete)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE_TQ40 : Return "Voxware TQ40 (Obsolete)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_SOFTSOUND : Return "Softsound"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VOXWARE_TQ60 : Return "Voxware TQ60 (Obsolete)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_MSRT24 : Return "Voxware MetaVoice MSRT24"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_G729A : Return "AT&T G.729A"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_MVI_MV12 : Return "MVI MV12"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DF_G726 : Return "DataFusion G.726"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DF_GSM610 : Return "DataFusion GSM610"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ISIAUDIO : Return "Iterated Systems, Inc. Audio"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ONLIVE : Return "OnLive!"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_SBC24 : Return "Siemens Business Communications Systems SBC24"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DOLBY_AC3_SPDIF : Return "Dolby AC3 SPDIF"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ZYXEL_ADPCM : Return "ZyXEL ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_PHILIPS_LPCBB : Return "Philips LPCBB"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_PACKED : Return "Packed"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_RAW_AAC1 : Return "Advanced Audio Coding"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_RHETOREX_ADPCM : Return "Rhetorex ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_IRAT : Return "BeCubed Software's IRAT"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VIVO_G723 : Return "Vivo G.723"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VIVO_SIREN : Return "Vivo Siren"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DIGITAL_G723 : Return "Digital G.723"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_WMAUDIO2 : Return "WMA 8/9"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_WMAUDIO3 : Return "WMA 9 Professional"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_WMAUDIO_LOSSLESS : Return "WMA 9 Lossless"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_WMASPDIF : Return "WMA over S/PDIF"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_CREATIVE_ADPCM : Return "Creative ADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_CREATIVE_FASTSPEECH8 : Return "Creative FastSpeech8"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_CREATIVE_FASTSPEECH10 : Return "Creative FastSpeech10"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_QUARTERDECK : Return "Quarterdeck"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_RAW_SPORT : Return "AC-3 over S/PDIF"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ESST_AC3 : Return "AC-3 over S/PDIF"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_FM_TOWNS_SND : Return "Fujitsu FM Towns Snd"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_BTV_DIGITAL : Return "Brooktree digital audio"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_VME_VMPCM : Return "AT&T VME VMPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_OLIGSM : Return "OLIGSM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_OLIADPCM : Return "OLIADPCM"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_OLICELP : Return "OLICELP"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_OLISBC : Return "OLISBC"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_OLIOPR : Return "OLIOPR"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_LH_CODEC : Return "Lernout & Hauspie"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_NORRIS : Return "Norris"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_ISIAUDIO2 : Return "AT&T ISIAudio"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_SOUNDSPACE_MUSICOMPRESS : Return "Soundspace Music Compression"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_MPEG_ADTS_AAC : Return "DTS Advanced Audio Coding"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_MPEG_LOAS : Return "MPEG-4 Audio with Synchronization and Multiplex Layers"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_MPEG_HEAAC : Return "MPEG Advanced Audio Coding"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DVM : Return "Dolby Digital AC-3"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_DTS2 : Return "Digital Theater Systems (DTS)"
      Case Seed.clsRIFF.WAVFormatTag.WAVE_FORMAT_EXTENSIBLE : Return "WAVE_FORMAT_EXTENSIBLE" : Debug.Print("Format Tag: WAVE_FORMAT_EXTENSIBLE")
      Case Else : Return "Unknown: " & CUInt(formatTag)
    End Select

  End Function

  Public Function AVIVideoCodecs(formatTag As Seed.clsRIFF.AVIFormatTag) As String
    'http://www.fourcc.org/codecs.php
    'http://www.ietf.org/rfc/rfc2361.txt
    'https://github.com/niphlod/w2p_tvseries/blob/master/modules/enzyme/fourcc.py
    Select Case formatTag
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_3IV1 : Return "3ivx v1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_3IV2 : Return "3ivx v4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_8BPS : Return "QuickTime (Planar RGB)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_AASC : Return "Autodesk Animator"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ABYR : Return "ABYR"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ADV1 : Return "WaveCodec"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ADVJ : Return "Avid AVRN"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_AEMI : Return "Array VideoONE MPEG-1 I"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_AFLC : Return "Autodesk Animator FLC"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_AFLI : Return "Autodesk Animator FLI"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_AJPG : Return "JPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_AMPG : Return "Array VideoONE MPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ANIM : Return "Intel RDX"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_AP41 : Return "AngelPotion Definitive"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ASLC : Return "Alparysoft Lossless"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ASV1 : Return "Asus TNT Video v1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ASV2 : Return "Asus Video v2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ASVX : Return "Asus Video 2.0"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_AURA : Return "AuraVision Aura 1 (YUV 4:1:1)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_AUR2 : Return "AuraVision Aura 2 (YUV 4:2:2)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_AVC1 : Return "Apple MPEG-4 Part 10/h.264"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_BA81 : Return "Bayer 8-bit"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_BINK : Return "Bink Video"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_BLZ0 : Return "Blizzard Entertainment MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_BT20 : Return "Brooktree MediaStream"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_BTCV : Return "Brooktree Composite Video"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_BW10 : Return "Broadway MPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_BYR1 : Return "Bayer 8-bit"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_BYR2 : Return "Bayer 16-bit"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CC12 : Return "AuraVision Aura 2 (Intel YUV12)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CDVC : Return "Canopus DV"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CFCC : Return "DPS Perception"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CGDI : Return "Office97 Camcorder"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CHAM : Return "Winnov Caviara Cham"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CJPG : Return "Creative WebCam (JPEG)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CLJR : Return "Proprietary YUV 4 pixels/DWORD"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CMYK : Return "Uncompressed 32-bit CMYK"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CPLA : Return "Weitek (Planar YUV 4:2:0)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CRAM : Return "Microsoft Video 1 (CRAM)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CSCD : Return "CamStudio"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CTRX : Return "Citrix"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CVID : Return "Cinepak by Supermac"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CWLT : Return "Microsoft Color WLT (DIB)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CXY1 : Return "Conexant (YUV 4:1:1)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CXY2 : Return "Conexant (YUV 4:2:2)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CYUV : Return "Creative Labs YUV"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_CYUY : Return "ATI Technologies YUV"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_D261 : Return "h.261 (24-bit)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_D263 : Return "h.263 (24-bit)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DAVC : Return "Dicas h.264/MPEG-4 AVC"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DCL1 : Return "Data Connection Ltd. Conference v1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DCL2 : Return "Data Connection Ltd. Conference v2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DCL3 : Return "Data Connection Ltd. Conference v3"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DCL4 : Return "Data Connection Ltd. Conference v4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DCL5 : Return "Data Connection Ltd. Conference v5"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DIV3 : Return "DivX 3 MPEG-4 Low Motion"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DIV4 : Return "DivX 3 MPEG-4 Fast Motion"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DIV5 : Return "DivX 3 MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DIVX : Return "DivX 4 MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DM4V : Return "DivX 4 MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DMB1 : Return "Rainbow Runner (Motion JPEG)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DMB2 : Return "Paradigm (Motion JPEG)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DMK2 : Return "ViewSonic V36 PDA"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DSVD : Return "DV/VFW"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DUCK : Return "TrueMotion 1.0"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DV25 : Return "DVC Professional (25mbps)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DV50 : Return "DVC (50mbps)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DVAN : Return "DVAN"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DVCS : Return "Generic DV"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DVE2 : Return "DVE-2 Videoconferencing"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DVH1 : Return "DVC HD (100mbps)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DVHD : Return "Consumer DV HD (50mbps)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DVSD : Return "Consumer DV SD (25mbps)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DVSL : Return "Consumer DV SL (12.5mbps)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DVX1 : Return "Lucent DVX1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DVX2 : Return "Lucent DVX2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DVX3 : Return "Lucent DVX3"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DX50 : Return "DivX 5 MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DXGM : Return "CinemaWare DXGM"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DXTC : Return "DirectX S3 Texture"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DXT1 : Return "DirectX S3 Texture 1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DXT2 : Return "DirectX S3 Texture 2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DXT3 : Return "DirectX S3 Texture 3"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DXT4 : Return "DirectX S3 Texture 4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_DXT5 : Return "DirectX S3 Texture 5"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_EKQ0 : Return "Elsa Quick Codec"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ELK0 : Return "Elsa (YUV)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_EM2V : Return "Etymonix MPEG-2 Video (YUV)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ES07 : Return "Eyestream 7"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ESCP : Return "Eidos ESCAPE"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ETV1 : Return "eTreppid Video v1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ETV2 : Return "eTreppid Video v2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ETVC : Return "eTreppid Video"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_FFV1 : Return "FFMPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_FLJP : Return "Field Encoded Motion JPEG with LSI"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_FMP4 : Return "FFMPEG MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_FMVC : Return "Fox Magic Screen Capture"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_FPS1 : Return "Fraps Screen Capture"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_FRWA : Return "Forward Motion JPEG with Alpha channel"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_FRWA : Return "Forward Motion JPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_FVF1 : Return "Fractal Video"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_GEOX : Return "GeoVision MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_GJPG : Return "Grand Tech JPG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_GLZW : Return "Motion LZW"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_GPEG : Return "Motion JPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_GWLT : Return "Grayscale WLT DIB"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_H260 : Return "h.260"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_H261 : Return "h.261"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_H262 : Return "h.262"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_H263 : Return "h.263"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_H264 : Return "h.264"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_H265 : Return "h.265"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_H266 : Return "h.266"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_H267 : Return "h.267"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_H268 : Return "h.268"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_H269 : Return "h.269"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_HDYC : Return "Raw YUV 4:2:2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_HFYU : Return "Huffman Lossless"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_HMCR : Return "Rendition V2x00 Surface"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_HMRR : Return "Rendition Surface"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_I263 : Return "Intel I263"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_I420 : Return "Intel Indeo 4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IAN : Return "Intel RDX"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ICLB : Return "CellB Videoconferencing"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IGOR : Return "PowerDVD MPEG-2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IJPG : Return "Intergraph JPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ILVC : Return "Intel Layered Video"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ILVR : Return "ITU-T h.263+"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IPDV : Return "I-O Data Device's 1394 DV Control & Capture"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IR21 : Return "Intel Indeo 2.1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IRAW : Return "Intel YUV uncompressed"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_ISME : Return "Roxio ISME"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV30 : Return "Intel Indeo Video 3"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV31 : Return "Intel Indeo Video 3.1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV32 : Return "Intel Indeo Video 3.2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV33 : Return "Intel Indeo Video 3.3"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV34 : Return "Intel Indeo Video 3.4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV35 : Return "Intel Indeo Video 3.5"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV36 : Return "Intel Indeo Video 3.6"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV37 : Return "Intel Indeo Video 3.7"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV38 : Return "Intel Indeo Video 3.8"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV39 : Return "Intel Indeo Video 3.9"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV40 : Return "Intel Indeo Video 4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV41 : Return "Intel Indeo Video 4.1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV42 : Return "Intel Indeo Video 4.2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV43 : Return "Intel Indeo Video 4.3"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV44 : Return "Intel Indeo Video 4.4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV45 : Return "Intel Indeo Video 4.5"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV46 : Return "Intel Indeo Video 4.6"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV47 : Return "Intel Indeo Video 4.7"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV48 : Return "Intel Indeo Video 4.8"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV49 : Return "Intel Indeo Video 4.9"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_IV50 : Return "Intel Indeo Video 5"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_JBYR : Return "Bayer JPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_JPEG : Return "Still Image JPEG DIB"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_JPGL : Return "JPEG Light"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_KMVC : Return "Worms KMVC"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_L261 : Return "LEAD h.261"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_L263 : Return "LEAD h.263"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LBYR : Return "LEAD Bayer"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LCMW : Return "LEAD Wavelet MCMW Video"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LCW2 : Return "LEAD Motion JPEG 2000"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LEAD : Return "LEAD Video"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LGRY : Return "LEAD Grayscale"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LJ11 : Return "LEAD JPEG (YUV 4:1:1)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LJ22 : Return "LEAD JPEG (YUV 4:2:2)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LJ2K : Return "LEAD Motion JPEG 2000"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LJ44 : Return "LEAD JPEG (YUV 4:4:4)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LJPG : Return "LEAD Motion JPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LMP2 : Return "LEAD MPEG-2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LMP4 : Return "LEAD h.264 MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LSVC : Return "Lightning Strike (VC)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LSVM : Return "Lightning Strike (VM)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LSVX : Return "Lightning Strike (VX)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_LZO1 : Return "Lempel-Ziv-Oberhumer Lossless"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_M261 : Return "Microsoft h.261"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_M263 : Return "Microsoft h.263"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_M4CC : Return "ISO MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_M4S2 : Return "Microsoft ISO MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MC12 : Return "ATI MPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MCAM : Return "ATI MPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MJ2C : Return "Motion JPEG 2000"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MJPG : Return "Motion JPEG (DIB)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MMES : Return "Matrox MPEG-2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MP2A : Return "MPEG-2 Audio"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MP2T : Return "MPEG-2 Transport Stream"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MP2V : Return "MPEG-2 Video"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MP42 : Return "Microsoft MPEG-4 Video Codec V2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MP43 : Return "Microsoft MPEG-4 Video Codec V3"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MP4A : Return "MPEG-4 Audio"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MP4S : Return "Sharp MPEG-4 ISO"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MP4T : Return "MPEG-4 Transport Stream"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MP4V : Return "MPEG-4 Video"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MPEG : Return "MPEG-1 Video"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MPG4 : Return "MPEG-4 Video High Speed Compressor"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MPGI : Return "Sigma Designs Editable MPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MR16 : Return "MR16"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MRCA : Return "Martin Regen MR Codec"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MRLE : Return "Run Length Encoding"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MSVC : Return "Microsoft Video 1 (MSVC)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MSZH : Return "AVImszh"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MTX1 : Return "Matrox Motion JPEG v1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MTX2 : Return "Matrox Motion JPEG v2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MTX3 : Return "Matrox Motion JPEG v3"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MTX4 : Return "Matrox Motion JPEG v4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MTX5 : Return "Matrox Motion JPEG v5"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MTX6 : Return "Matrox Motion JPEG v6"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MTX7 : Return "Matrox Motion JPEG v7"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MTX8 : Return "Matrox Motion JPEG v8"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MTX9 : Return "Matrox Motion JPEG v9"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MVI1 : Return "Motion Pixels v1 (TreasureQuest)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MVI2 : Return "Motion Pixels v2 (Movie CD)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_MWV1 : Return "Aware Motion Wavelets"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NAVI : Return "Shadow Movie Realm NAVI Vx3 MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_nAVI_LOWER : Return "Shadow Movie Realm nAVI Vx3 MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NDSC : Return "Nero Digital Cinema MPEG-4 Part II"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NDSH : Return "Nero Digital HDTV MPEG-4 Part II"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NDSM : Return "Nero Digital Mobile MPEG-4 Part II"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NDSP : Return "Nero Digital Portable MPEG-4 Part II"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NDSS : Return "Nero Digital Standard MPEG-4 Part II"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NDXC : Return "Nero Digital Cinema h.264/MPEG-4 Part X"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NDXH : Return "Nero Digital HDTV h.264/MPEG-4 Part X"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NDXM : Return "Nero Digital Mobile h.264/MPEG-4 Part X"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NDXP : Return "Nero Digital Portable h.264/MPEG-4 Part X"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NDXS : Return "Nero Digital Standard h.264/MPEG-4 Part X"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NHVU : Return "nVidia GeForce 3 Texture"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NTN1 : Return "Nogatech Video Compression v1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NTN2 : Return "Nogatech Video Compression v2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVDS : Return "nVidia Texture"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVHS : Return "nVidia GeForce 3 Texture"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVS0 : Return "nVidia GeForce Texture S0"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVS1 : Return "nVidia GeForce Texture S1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVS2 : Return "nVidia GeForce Texture S2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVS3 : Return "nVidia GeForce Texture S3"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVS4 : Return "nVidia GeForce Texture S4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVS5 : Return "nVidia GeForce Texture S5"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVT0 : Return "nVidia GeForce Texture T0"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVT1 : Return "nVidia GeForce Texture T1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVT2 : Return "nVidia GeForce Texture T2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVT3 : Return "nVidia GeForce Texture T3"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVT4 : Return "nVidia GeForce Texture T4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_NVT5 : Return "nVidia GeForce Texture T5"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_PDVC : Return "I-O Data Device's DV Capture"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_PGVV : Return "Radius Video Vision Telecast"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_PHMO : Return "IBM Photomotion"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_PIM1 : Return "Pinnacle Systems MPEG-1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_PIM2 : Return "Pinnacle Systems DC1000"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_PIMJ : Return "Predictor 1 Lossless JPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_PIXL : Return "Pinnacle Video XL Motion JPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_PJPG : Return "PA Motion JPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_PVEZ : Return "Horizons Technology PowerEZ TrueMotion"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_PVMM : Return "PacketVideo MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_PVW2 : Return "Pegasus Wavelet 2000"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_qpeg : Return "Q-Team QPEG v1.0"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_qpeq : Return "Q-Team QPEG v1.1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RGBT : Return "Brooktree RGB + Transparency Plane Uncompressed"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RLE : Return "Microsoft Run Length Encoder"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RLE4 : Return "Microsoft Run Length Encoder (4bpp)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RLE8 : Return "Microsoft Run Length Encoder (8bpp)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RMP4 : Return "REALmagic MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RPZA : Return "Apple Video 16 bit ""Road Pizza"""
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RT21 : Return "Indeo 2.1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RV10 : Return "RealVideo (h.263)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RV13 : Return "RealVideo 1.3 (h.263)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RV20 : Return "RealVideo G2 (h.263)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RV30 : Return "RealVideo 3 (h.264 draft)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RV40 : Return "RealVideo 4 (h.264 prototype)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_RVX : Return "Intel RDX"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_S422 : Return "VideoCap C210 (YUV 4:2:2)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SAN3 : Return "DivX 3 MPEG-4 (SAN3)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SDCC : Return "Sun Communications Digital Camera Codec"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SEDG : Return "Samsung MPEG-4 DV"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SFMC : Return "Crystal Net Surface Fitting Method"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SMP4 : Return "Samsung DivX 4 MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SMSC : Return "Radius SMSC"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SMSD : Return "Radius SMSD"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SMSV : Return "WorldConnect Wavelet"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SP40 : Return "SunPlus YUV"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SP44 : Return "SunPlus Motion JPEG SP44"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SP54 : Return "SunPlus Motion JPEG SP54"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SPIG : Return "Radius Spigot"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SPLC : Return "Splash Studios ACM"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SQZ2 : Return "Microsoft Vxtreme Video Codec V2"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_STVA : Return "ST CMOS Imager Data (Bayer)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_STVB : Return "ST CMOS Imager Data (Nudged Bayer)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_STVC : Return "ST CMOS Imager Data (Bunched)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_STVX : Return "ST CMOS Imager Data (Extended CODEC Data Format)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_STVY : Return "ST CMOS Imager Data (Extended CODEC Data Format with Correction Data)"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_SV10 : Return "Sorenson Video R1"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_XVID : Return "XviD MPEG-4"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_VIXL : Return "Miro Video XL Motion JPEG"
      Case Seed.clsRIFF.AVIFormatTag.AVI_FORMAT_WHAM : Return "Microsoft Video 1 (WHAM)"
      Case Else
        Dim bTag As Byte() = BitConverter.GetBytes(CUInt(formatTag))
        Dim sTag As String = Text.Encoding.GetEncoding(LATIN_1).GetString(bTag)
        Return "Unknown: " & sTag
    End Select
  End Function

  Public Function AVIInfoKeys(Key As String) As String
    Select Case Key
      Case "IARL" : Return "Archival Location"
      Case "IART" : Return "Original Artist"
      Case "ICMS" : Return "Commissioned"
      Case "ICMT" : Return "Comments"
      Case "ICOP" : Return "Copyright"
      Case "ICRD" : Return "Creation Date"
      Case "ICRP" : Return "Cropped"
      Case "IDIM" : Return "Dimensions"
      Case "IDPI" : Return "Dots Per Inch"
      Case "IENG" : Return "Engineer"
      Case "IGNR" : Return "Genre"
      Case "IKEY" : Return "Keywords"
      Case "ILGT" : Return "Lightness"
      Case "IMED" : Return "Medium"
      Case "INAM" : Return "Name"
      Case "IPLT" : Return "Palette Setting"
      Case "IPRD" : Return "Product"
      Case "ISBJ" : Return "Subject"
      Case "ISFT" : Return "Software"
      Case "ISHP" : Return "Sharpness"
      Case "ISRC" : Return "Source"
      Case "ISRF" : Return "Source Form"
      Case "ITCH" : Return "Technician"
      Case Else : Return Key
    End Select
  End Function

  Public Function CompareLanguages(LangA As String, LangB As String) As Boolean
    If String.IsNullOrEmpty(LangA) Then LangA = "und"
    If String.IsNullOrEmpty(LangB) Then LangB = "und"
    If LangA = "0" Then LangA = "und"
    If LangB = "0" Then LangB = "und"
    If LangA.Contains(" [") And LangA.Substring(LangA.IndexOf(" [") + 2).Contains("]") Then
      LangA = LangA.Substring(LangA.IndexOf(" [") + 2)
      LangA = LangA.Substring(0, LangA.IndexOf("]"))
    End If
    If LangB.Contains(" [") And LangB.Substring(LangB.IndexOf(" [") + 2).Contains("]") Then
      LangB = LangB.Substring(LangB.IndexOf(" [") + 2)
      LangB = LangB.Substring(0, LangB.IndexOf("]"))
    End If
    LangA = LangA.ToLower
    LangB = LangB.ToLower
    Dim locA As Globalization.CultureInfo = Nothing
    If IsNumeric(LangA) Then
      locA = New Globalization.CultureInfo(CInt(LangA))
    Else
      For Each culture In Globalization.CultureInfo.GetCultures(Globalization.CultureTypes.NeutralCultures)
        If culture.NativeName.Contains(LangA) Or
          LangA.Contains(culture.NativeName) Or
          LangA.Contains(culture.EnglishName) Or
          LangA.Contains(culture.ThreeLetterISOLanguageName) Or
          LangA.Contains(culture.ThreeLetterWindowsLanguageName) Or
          LangA.Contains(culture.DisplayName) Or
          LangA.Contains(culture.Name) Or
          LangA.Contains(culture.IetfLanguageTag) Or
          LangA.Contains(culture.TwoLetterISOLanguageName) Then
          locA = culture
          Exit For
        End If
      Next
    End If
    If locA Is Nothing Then Debug.Print("Unknown Localization: " & LangA)

    Dim locB As Globalization.CultureInfo = Nothing
    If IsNumeric(LangB) Then
      locB = New Globalization.CultureInfo(CInt(LangB))
    Else
      For Each culture In Globalization.CultureInfo.GetCultures(Globalization.CultureTypes.NeutralCultures)
        If culture.NativeName.Contains(LangB) Or
          LangB.Contains(culture.NativeName) Or
          LangB.Contains(culture.EnglishName) Or
          LangB.Contains(culture.ThreeLetterISOLanguageName) Or
          LangB.Contains(culture.ThreeLetterWindowsLanguageName) Or
          LangB.Contains(culture.DisplayName) Or
          LangB.Contains(culture.Name) Or
          LangB.Contains(culture.IetfLanguageTag) Or
          LangB.Contains(culture.TwoLetterISOLanguageName) Then
          locB = culture
          Exit For
        End If
      Next
    End If
    If locB Is Nothing Then Debug.Print("Unknown Localization: " & LangB)

    If locA Is Nothing And locB Is Nothing Then Return True
    If locA Is Nothing Then Return False
    If locB Is Nothing Then Return False

    Return locA.LCID = locB.LCID
  End Function

  ''' <summary>
  ''' Attempts to see if a file is in use, waiting up to five seconds for it to be freed.
  ''' </summary>
  ''' <param name="Filename">The exact path to the file which needs to be checked.</param>
  ''' <param name="access">Write permissions required for checking.</param>
  ''' <returns>True on available, false on in use.</returns>
  ''' <remarks></remarks>
  Public Function InUseChecker(Filename As String, access As IO.FileAccess) As Boolean
    If Not IO.File.Exists(Filename) Then Return True
    Dim iStart As Integer = Environment.TickCount
    Do
      Try
        Select Case access
          Case FileAccess.Read
            'only check for ability to read
            Using fs As FileStream = IO.File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
              If fs.CanRead Then
                Return True
                Exit Do
              End If
            End Using
          Case FileAccess.Write, FileAccess.ReadWrite
            'check for ability to write
            Using fs As FileStream = IO.File.Open(Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite Or FileShare.Delete)
              If fs.CanWrite Then
                Return True
                Exit Do
              End If
            End Using
        End Select
      Catch ex As Exception
      End Try
      Application.DoEvents()
    Loop While Environment.TickCount - iStart < 5000
    Return False
  End Function
End Module

Public Class Sound

#Region "Comments"
  ' This class is responisble for all interactions for muting and controlling the master volume level

  ' There is no warranty associated with this code - if you use this and it blows up you machine.
  ' Well, that will teach you to write you own code in future, won't it ;-)

  ' Mark Dryden (aka Drydo@vbcity.com)
#End Region

#Region "Constants"
  Private Const MMSYSERR_NOERROR As Integer = 0
  Private Const MAXPNAMELEN As Integer = 32
  Private Const MIXER_LONG_NAME_CHARS As Integer = 64
  Private Const MIXER_SHORT_NAME_CHARS As Integer = 16
  Private Const MIXERCONTROL_CT_CLASS_FADER As Integer = &H50000000
  Private Const MIXERCONTROL_CT_UNITS_UNSIGNED As Integer = &H30000
  Private Const MIXERCONTROL_CT_UNITS_BOOLEAN As Integer = &H10000
  Private Const MIXERCONTROL_CT_CLASS_SWITCH As Integer = &H20000000
  Private Const MIXERLINE_COMPONENTTYPE_DST_FIRST As Integer = &H0&
  Private Const MIXERLINE_COMPONENTTYPE_DST_SPEAKERS As Integer = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 4)
  Private Const MIXERCONTROL_CONTROLTYPE_FADER As Integer = (MIXERCONTROL_CT_CLASS_FADER Or MIXERCONTROL_CT_UNITS_UNSIGNED)
  Private Const MIXERCONTROL_CONTROLTYPE_VOLUME As Integer = (MIXERCONTROL_CONTROLTYPE_FADER + 1)
  Private Const MIXER_GETLINEINFOF_COMPONENTTYPE As Integer = &H3&
  Private Const MIXER_GETLINECONTROLSF_ONEBYTYPE As Integer = &H2
  Private Const MIXERCONTROL_CONTROLTYPE_BASS As Integer = (MIXERCONTROL_CONTROLTYPE_FADER + 2)
  Private Const MIXERCONTROL_CONTROLTYPE_TREBLE As Integer = (MIXERCONTROL_CONTROLTYPE_FADER + 3)
  Private Const MIXERCONTROL_CONTROLTYPE_EQUALIZER As Integer = (MIXERCONTROL_CONTROLTYPE_FADER + 4)
  Private Const MIXERCONTROL_CONTROLTYPE_BOOLEAN As Integer = (MIXERCONTROL_CT_CLASS_SWITCH Or MIXERCONTROL_CT_UNITS_BOOLEAN)
  Private Const MIXERCONTROL_CONTROLTYPE_MUTE As Integer = (MIXERCONTROL_CONTROLTYPE_BOOLEAN + 2)
#End Region

#Region "Structs"

  <StructLayout(LayoutKind.Sequential)> _
  Private Structure MIXERCONTROL
    Dim cbStruct As UInt32
    Dim dwControlID As UInt32
    Dim dwControlType As UInt32
    Dim fdwControl As UInt32
    Dim cMultipleItems As UInt32
    <MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst:=MIXER_SHORT_NAME_CHARS)> Dim szShortName As String
    <MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst:=MIXER_LONG_NAME_CHARS)> Dim szName As String
    Dim lMinimum As UInt32
    Dim lMaximum As UInt32
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=11, ArraySubType:=UnmanagedType.AsAny)> Public reserved() As Integer
  End Structure
  <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=4)> _
  Public Structure MIXERCONTROLDETAILS
    Private cbStruct As UInt32
    Private dwControlID As UInt32
    Private cChannels As UInt32
    Private hwndOwner As IntPtr
    '     private UInt32 cMultipleItems; //Unioned with hwndOwner /* if _MULTIPLE, the number of items per channel */
    Private cbDetails As UInt32
    Private paDetails As IntPtr

#Region "Properties"

    ''' <summary>size in bytes of MIXERCONTROLDETAILS</summary>
    Public Property StructSize() As UInt32
      Get
        Return Me.cbStruct
      End Get
      Set(value As UInt32)
        Me.cbStruct = value
      End Set
    End Property
    ''' <summary>control id to get/set details on</summary>
    Public Property ControlID() As UInt32
      Get
        Return Me.dwControlID
      End Get
      Set(value As UInt32)
        Me.dwControlID = value
      End Set
    End Property
    ''' <summary>number of channels in paDetails array</summary>
    Public Property Channels() As UInt32
      Get
        Return Me.cChannels
      End Get
      Set(value As UInt32)
        Me.cChannels = value
      End Set
    End Property
    ''' <summary>for MIXER_SETCONTROLDETAILSF_CUSTOM</summary>
    Public Property OwnerHandle() As IntPtr
      Get
        Return Me.hwndOwner
      End Get
      Set(value As IntPtr)
        Me.hwndOwner = value
      End Set
    End Property
    ''' <summary>if _MULTIPLE, the number of items per channel</summary>
    Public Property MultipleItems() As Int32
      Get
        Return Me.hwndOwner.ToInt32
      End Get
      Set(value As Int32)
        Me.hwndOwner = New IntPtr(value)
      End Set
    End Property
    ''' <summary>size of _one_ details_XX struct</summary>
    Public Property DetailsItemSize() As UInt32
      Get
        Return Me.cbDetails
      End Get
      Set(value As UInt32)
        Me.cbDetails = value
      End Set
    End Property
    ''' <summary>pointer to array of details_XX structs</summary>
    Public Property DetailsPointer() As IntPtr
      Get
        Return Me.paDetails
      End Get
      Set(value As IntPtr)
        Me.paDetails = value
      End Set
    End Property

#End Region
  End Structure

  <StructLayout(LayoutKind.Sequential)> _
  Private Structure MIXERCONTROLDETAILS_UNSIGNED
    Dim dwValue As Integer
  End Structure

  <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=4)> _
  Public Structure MIXERLINECONTROLS
    Private cbStruct As UInt32       '/* size in bytes of MIXERLINECONTROLS */
    Private dwLineID As UInt32       '/* line id (from MIXERLINE.dwLineID) */
    Private dwControlID As UInt32     '/* MIXER_GETLINECONTROLSF_ONEBYID */
    'private UInt32 dwControlType;  //UNIONED with dwControlID /* MIXER_GETLINECONTROLSF_ONEBYTYPE */
    Private cControls As UInt32      '/* count of controls pmxctrl points to */
    Private cbmxctrl As UInt32       '/* size in bytes of _one_ MIXERCONTROL */
    Private pamxctrl As IntPtr       '/* pointer to first MIXERCONTROL array */

    '/// <summary>size in bytes of MIXERLINECONTROLS</summary>
    Public Property StructSize As UInt32
      Get
        Return cbStruct
      End Get
      Set(ByVal value As UInt32)
        cbStruct = value
      End Set
    End Property

    '/// <summary>line id (from MIXERLINE.dwLineID)</summary>
    Public Property LineID As UInt32
      Get
        Return dwLineID
      End Get
      Set(ByVal value As UInt32)
        dwLineID = value
      End Set
    End Property

    '/// <summary>MIXER_GETLINECONTROLSF_ONEBYID</summary>
    Public Property ControlID As UInt32
      Get
        Return dwControlID
      End Get
      Set(ByVal value As UInt32)
        dwControlID = value
      End Set
    End Property

    '/// <summary>MIXER_GETLINECONTROLSF_ONEBYTYPE</summary>
    Public Property ControlType As UInt32
      Get
        Return dwControlID
      End Get
      Set(ByVal value As UInt32)
        dwControlID = value
      End Set
    End Property

    '/// <summary>count of controls pmxctrl points to</summary>
    Public Property Controls As UInt32
      Get
        Return cControls
      End Get
      Set(ByVal value As UInt32)
        cControls = value
      End Set
    End Property

    '/// <summary>size in bytes of _one_ MIXERCONTROL</summary>
    Public Property MixerControlItemSize As UInt32
      Get
        Return cbmxctrl
      End Get
      Set(ByVal value As UInt32)
        cbmxctrl = value
      End Set
    End Property

    '/// <summary>pointer to first MIXERCONTROL array</summary>
    Public Property MixerControlArray As IntPtr
      Get
        Return pamxctrl
      End Get
      Set(ByVal value As IntPtr)
        pamxctrl = value
      End Set
    End Property

  End Structure

  Enum AudioLineStatus As UInteger
    MIXERLINE_LINEF_ACTIVE = 1UL
    MIXERLINE_LINEF_DISCONNECTED = &H8000UL
    MIXERLINE_LINEF_SOURCE = &H80000000UL
  End Enum

  Enum MixerLineComponentType As UInteger
    MIXERLINE_COMPONENTTYPE_DST_UNDEFINED = &H0
    MIXERLINE_COMPONENTTYPE_DST_DIGITAL = &H1
    MIXERLINE_COMPONENTTYPE_DST_LINE = &H2
    MIXERLINE_COMPONENTTYPE_DST_MONITOR = &H3
    MIXERLINE_COMPONENTTYPE_DST_SPEAKERS = &H4
    MIXERLINE_COMPONENTTYPE_DST_HEADPHONES = &H5
    MIXERLINE_COMPONENTTYPE_DST_TELEPHONE = &H6
    MIXERLINE_COMPONENTTYPE_DST_WAVEIN = &H7
    MIXERLINE_COMPONENTTYPE_DST_VOICEIN = &H8
    MIXERLINE_COMPONENTTYPE_SRC_UNDEFINED = &H1000
    MIXERLINE_COMPONENTTYPE_SRC_DIGITAL = &H1001
    MIXERLINE_COMPONENTTYPE_SRC_LINE = &H1002
    MIXERLINE_COMPONENTTYPE_SRC_MICROPHONE = &H1003
    MIXERLINE_COMPONENTTYPE_SRC_SYNTHESIZER = &H1004
    MIXERLINE_COMPONENTTYPE_SRC_COMPACTDISC = &H1005
    MIXERLINE_COMPONENTTYPE_SRC_TELEPHONE = &H1006
    MIXERLINE_COMPONENTTYPE_SRC_PCSPEAKER = &H1007
    MIXERLINE_COMPONENTTYPE_SRC_WAVEOUT = &H1008
    MIXERLINE_COMPONENTTYPE_SRC_AUXILIARY = &H1009
    MIXERLINE_COMPONENTTYPE_SRC_ANALOG = &H100A
  End Enum

  Enum AudioLineType As UInteger
    MIXERLINE_TARGETTYPE_UNDEFINED = 0
    MIXERLINE_TARGETTYPE_WAVEOUT = 1
    MIXERLINE_TARGETTYPE_WAVEIN = 2
    MIXERLINE_TARGETTYPE_MIDIOUT = 3
    MIXERLINE_TARGETTYPE_MIDIIN = 4
    MIXERLINE_TARGETTYPE_AUX = 5
  End Enum

  <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=4)> _
  Public Structure MIXERLINE
    Private cbStruct As UInt32
    Private dwDestination As UInt32
    Private dwSource As UInt32
    Private dwLineID As UInt32
    Private fdwLine As UInt32
    Private dwUser As IntPtr
    Private dwComponentType As UInt32
    Private cChannels As UInt32
    Private cConnections As UInt32
    Private cControls As UInt32
    <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MIXER_SHORT_NAME_CHARS)> Private szShortName As String
    <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MIXER_LONG_NAME_CHARS)> Private szName As String
    Public Target As MIXERLINETARGET

#Region "Properties"

    ''' <summary>size of MIXERLINE structure</summary>
    Public Property StructSize() As UInt32
      Get
        Return Me.cbStruct
      End Get
      Set(value As UInt32)
        Me.cbStruct = value
      End Set
    End Property
    ''' <summary>zero based destination index</summary>
    Public Property Destination() As UInt32
      Get
        Return Me.dwDestination
      End Get
      Set(value As UInt32)
        Me.dwDestination = value
      End Set
    End Property
    ''' <summary>zero based source index (if source)</summary>
    Public Property Source() As UInt32
      Get
        Return Me.dwSource
      End Get
      Set(value As UInt32)
        Me.dwSource = value
      End Set
    End Property
    ''' <summary>unique line id for mixer device</summary>
    Public Property LineID() As UInt32
      Get
        Return Me.dwLineID
      End Get
      Set(value As UInt32)
        Me.dwLineID = value
      End Set
    End Property
    ''' <summary>state/information about line</summary>
    Public Property LineInformation() As AudioLineStatus
      Get
        Return DirectCast(Me.fdwLine, AudioLineStatus)
      End Get
      Set(value As AudioLineStatus)
        Me.fdwLine = DirectCast(value, UInt32)
      End Set
    End Property
    ''' <summary>driver specific information</summary>
    Public Property UserPointer() As IntPtr
      Get
        Return Me.dwUser
      End Get
      Set(value As IntPtr)
        Me.dwUser = value
      End Set
    End Property
    ''' <summary>component type line connects to</summary>
    Public Property ComponentType() As MixerLineComponentType
      Get
        Return DirectCast(Me.dwComponentType, MixerLineComponentType)
      End Get
      Set(value As MixerLineComponentType)
        Me.dwComponentType = DirectCast(value, UInt32)
      End Set
    End Property
    ''' <summary>number of channels line supports</summary>
    Public Property Channels() As UInt32
      Get
        Return Me.cChannels
      End Get
      Set(value As UInt32)
        Me.cChannels = value
      End Set
    End Property
    ''' <summary>number of connections [possible]</summary>
    Public Property Connections() As UInt32
      Get
        Return Me.cConnections
      End Get
      Set(value As UInt32)
        Me.cConnections = value
      End Set
    End Property
    ''' <summary>number of controls at this line</summary>
    Public Property Controls() As UInt32
      Get
        Return Me.cControls
      End Get
      Set(value As UInt32)
        Me.cControls = value
      End Set
    End Property
    ''' <summary></summary>
    Public Property ShortName() As String
      Get
        Return Me.szShortName
      End Get
      Set(value As String)
        Me.szShortName = value
      End Set
    End Property
    ''' <summary></summary>
    Public Property Name() As String
      Get
        Return Me.szName
      End Get
      Set(value As String)
        Me.szName = value
      End Set
    End Property

#End Region
  End Structure

  <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=2)> _
  Public Structure MIXERLINETARGET
    Private dwType As UInt32
    Private dwDeviceID As UInt32
    Private wMid As UInt16
    Private wPid As UInt16
    Private vDriverVersion As UInt32
    <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAXPNAMELEN)> _
    Private szPname As String

#Region "Properties"

    ''' <summary>MIXERLINE_TARGETTYPE_xxxx</summary>
    Public Property Type() As AudioLineType
      Get
        Return DirectCast(Me.dwType, AudioLineType)
      End Get
      Set(value As AudioLineType)
        Me.dwType = DirectCast(value, UInt32)
      End Set
    End Property
    ''' <summary>target device ID of device type</summary>
    Public Property DeviceID() As UInt32
      Get
        Return Me.dwDeviceID
      End Get
      Set(value As UInt32)
        Me.dwDeviceID = value
      End Set
    End Property
    ''' <summary>of target device</summary>
    Public Property ManufacturerID() As UInt16
      Get
        Return Me.wMid
      End Get
      Set(value As UInt16)
        Me.wMid = value
      End Set
    End Property
    ''' <summary></summary>
    Public Property ProductID() As UInt16
      Get
        Return Me.wPid
      End Get
      Set(value As UInt16)
        Me.wPid = value
      End Set
    End Property
    ''' <summary></summary>
    Public Property DriverVersion() As UInt32
      Get
        Return Me.vDriverVersion
      End Get
      Set(value As UInt32)
        Me.vDriverVersion = value
      End Set
    End Property
    ''' <summary></summary>
    Public Property PName() As String
      Get
        Return Me.szPname
      End Get
      Set(value As String)
        Me.szPname = value
      End Set
    End Property

#End Region
  End Structure

#End Region

#Region "API Declarations"

  Private Declare Function mixerOpen Lib "winmm.dll" (ByRef phmx As IntPtr, ByVal uMxId As Integer, ByVal dwCallback As Integer, ByVal dwInstance As Integer, ByVal fdwOpen As Integer) As Integer

  Private Declare Function mixerGetLineInfo Lib "winmm.dll" Alias "mixerGetLineInfoA" (ByVal hmxobj As IntPtr, ByRef pmxl As MIXERLINE, ByVal fdwInfo As Integer) As Integer

  Private Declare Function mixerGetLineControls Lib "winmm.dll" Alias "mixerGetLineControlsA" (ByVal hmxobj As IntPtr, ByRef pmxlc As MIXERLINECONTROLS, ByVal fdwControls As Integer) As Integer

  Private Declare Function mixerSetControlDetails Lib "winmm.dll" (ByVal hmxobj As IntPtr, ByRef pmxcd As MIXERCONTROLDETAILS, ByVal fdwDetails As Integer) As Integer

#End Region

  Public Sub SetVolume(ByVal Level As Integer)
    ' Sets the volume to a specific percentage as passed through
    Dim hmixer As UInteger
    Dim volCtrl As New MIXERCONTROL
    Dim lngReturn As Integer
    Dim lngVolSetting As Integer

    ' Obtain the hmixer struct
    lngReturn = mixerOpen(hmixer, 0, 0, 0, 0)

    ' Error check
    If lngReturn <> 0 Then Return

    ' Obtain the volumne control object
    Call GetVolumeControl(hmixer, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS, MIXERCONTROL_CONTROLTYPE_VOLUME, volCtrl)

    ' Then determine the value of the volume
    lngVolSetting = CType(volCtrl.lMaximum * (Level / 100), Integer)
    'Stop
    ' Then set the volume
    SetVolumeControl(hmixer, volCtrl, lngVolSetting)
  End Sub

  Public Sub SetSound(ByVal boolMute As Boolean)
    ' This routine sets the volume setting of the current unit depending on the value passed through
    Dim hmixer As IntPtr
    Dim volCtrl As New MIXERCONTROL
    Dim lngReturn As Integer
    Dim lngVolSetting As Integer

    ' Obtain the hmixer struct
    lngReturn = mixerOpen(hmixer, 0, 0, 0, 0)

    ' Error check
    If lngReturn <> 0 Then Return

    ' Obtain the volumne control object
    Call GetVolumeControl(hmixer, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS, MIXERCONTROL_CONTROLTYPE_MUTE, volCtrl)

    ' Then determine the value of the volume
    If boolMute Then
      ' Mute
      lngVolSetting = 1
    Else
      ' Turn the sound on
      lngVolSetting = 0
    End If

    ' Then set the volume
    SetVolumeControl(hmixer, volCtrl, lngVolSetting)
  End Sub

  Private Function GetVolumeControl(ByVal hmixer As IntPtr, ByVal componentType As Integer, ByVal ctrlType As Integer, ByRef mxc As MIXERCONTROL) As Boolean
    ' Obtains an appropriate pointer and info for the volume control

    ' [Note: original source taken from MSDN http://support.microsoft.com/default.aspx?scid=KB;EN-US;Q178456&]

    ' This function attempts to obtain a mixer control. Returns True if successful.
    Dim mxlc As New MIXERLINECONTROLS
    Dim mxl As New MIXERLINE
    Dim rc As Integer, pmem As IntPtr

    mxl.StructSize = Marshal.SizeOf(mxl)
    mxl.ComponentType = componentType

    ' Obtain a line corresponding to the component type
    rc = mixerGetLineInfo(hmixer, mxl, MIXER_GETLINEINFOF_COMPONENTTYPE)
    If (MMSYSERR_NOERROR = rc) Then
      mxlc.StructSize = Marshal.SizeOf(mxlc)
      mxlc.LineID = mxl.LineID
      mxlc.ControlID = ctrlType
      mxlc.Controls = 1
      mxlc.MixerControlItemSize = Marshal.SizeOf(mxc)

      ' Allocate a buffer for the control
      pmem = Marshal.AllocHGlobal(Marshal.SizeOf(mxc))
      mxlc.MixerControlArray = pmem

      mxc.cbStruct = Marshal.SizeOf(mxc)

      ' Get the control
      rc = mixerGetLineControls(hmixer, mxlc, MIXER_GETLINECONTROLSF_ONEBYTYPE)

      If (MMSYSERR_NOERROR = rc) Then

        mxc = CType(Marshal.PtrToStructure(mxlc.MixerControlArray, GetType(MIXERCONTROL)), MIXERCONTROL)

        Marshal.FreeHGlobal(pmem)
        Return True
      End If
      Marshal.FreeHGlobal(pmem)
    End If

    Return False
  End Function

  Private Function SetVolumeControl(ByVal hmixer As IntPtr, ByVal mxc As MIXERCONTROL, ByVal volume As Integer) As Boolean
    ' Sets the volumne from the pointer of the object passed through

    ' [Note: original source taken from MSDN http://support.microsoft.com/default.aspx?scid=KB;EN-US;Q178456&]

    'This function sets the value for a volume control. Returns True if successful

    Dim mxcd As MIXERCONTROLDETAILS
    Dim vol As MIXERCONTROLDETAILS_UNSIGNED
    Dim rc As Integer

    Dim hptr As IntPtr

    mxcd.OwnerHandle = 0
    mxcd.ControlID = mxc.dwControlID
    mxcd.StructSize = Marshal.SizeOf(mxcd)
    mxcd.DetailsItemSize = Marshal.SizeOf(vol)

    hptr = Marshal.AllocHGlobal(Marshal.SizeOf(vol))

    ' Allocate a buffer for the control value buffer
    mxcd.DetailsPointer = hptr
    mxcd.Channels = 1
    vol.dwValue = volume

    Marshal.StructureToPtr(vol, hptr, False)

    ' Set the control value
    rc = mixerSetControlDetails(hmixer, mxcd, 0)

    Marshal.FreeHGlobal(hptr)

    If (MMSYSERR_NOERROR = rc) Then
      Return True
    Else
      Return False
    End If
  End Function
End Class

Friend Class DataGridViewProgressColumn
  Inherits DataGridViewImageColumn
  Public Sub New()
    Me.CellTemplate = New DataGridViewProgressCell
  End Sub
End Class

Friend Class DataGridViewProgressCell
  Inherits DataGridViewImageCell
  Sub New()
    ValueType = Type.GetType("Double")
  End Sub
  Protected Overrides Function GetFormattedValue(value As Object, ByVal rowIndex As Integer, ByRef cellStyle As DataGridViewCellStyle, ByVal valueTypeConverter As System.ComponentModel.TypeConverter, ByVal formattedValueTypeConverter As System.ComponentModel.TypeConverter, ByVal context As DataGridViewDataErrorContexts) As Object
    Static emptyImage As System.Drawing.Bitmap = New System.Drawing.Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
    GetFormattedValue = emptyImage
  End Function
  Protected Overrides Sub Paint(g As System.Drawing.Graphics, ByVal clipBounds As System.Drawing.Rectangle, ByVal cellBounds As System.Drawing.Rectangle, ByVal rowIndex As Integer, ByVal cellState As System.Windows.Forms.DataGridViewElementStates, ByVal value As Object, ByVal formattedValue As Object, ByVal errorText As String, ByVal cellStyle As System.Windows.Forms.DataGridViewCellStyle, ByVal advancedBorderStyle As System.Windows.Forms.DataGridViewAdvancedBorderStyle, ByVal paintParts As System.Windows.Forms.DataGridViewPaintParts)
    If IsNumeric(value) And ProgressBarRenderer.IsSupported Then
      Dim progressVal As Double = CType(value, Double)
      MyBase.Paint(g, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts)
      If progressVal < 0 Then
        'Negative means unknown
        ProgressBarRenderer.DrawHorizontalBar(g, New System.Drawing.Rectangle(cellBounds.Left + 1, cellBounds.Top + 1, cellBounds.Width - 3, cellBounds.Height - 3))
        Dim pTextWidth As Integer = MeasureTextWidth(g, ". . .", cellStyle.Font, cellBounds.Height, TextFormatFlags.Default)
        Using foreBrush As New System.Drawing.SolidBrush(cellStyle.ForeColor)
          g.DrawString(". . .", cellStyle.Font, foreBrush, cellBounds.X + cellBounds.Width / 2 - pTextWidth / 2, cellBounds.Y + 2)
        End Using
      Else
        ProgressBarRenderer.DrawHorizontalBar(g, New System.Drawing.Rectangle(cellBounds.Left + 1, cellBounds.Top + 1, cellBounds.Width - 3, cellBounds.Height - 3))
        ProgressBarRenderer.DrawHorizontalChunks(g, New System.Drawing.Rectangle(cellBounds.Left + 2, cellBounds.Top + 2, (progressVal / 100) * (cellBounds.Width - 5), cellBounds.Height - 5))
        Dim pTextWidth As Integer = MeasureTextWidth(g, progressVal.ToString & "%", cellStyle.Font, cellBounds.Height, TextFormatFlags.Default)
        Using foreBrush As New System.Drawing.SolidBrush(cellStyle.ForeColor)
          g.DrawString(progressVal.ToString & "%", cellStyle.Font, foreBrush, cellBounds.X + cellBounds.Width / 2 - pTextWidth / 2, cellBounds.Y + 2)
        End Using
      End If
    Else
      MyBase.Paint(g, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts)
    End If
  End Sub
End Class

Friend Class INIReader

  <DllImport("kernel32.dll", CharSet:=Runtime.InteropServices.CharSet.Unicode, setlasterror:=True)>
  Friend Shared Function GetPrivateProfileStringW(lpApplicationName As String, lpKeyName As String, lpDefault As String, lpReturnedString As String, nSize As Int32, lpFileName As String) As Integer
  End Function

  Private INIFile As String
  Private Sections() As String
  Private Keys()() As String

  Public Sub New(FileName As String)
    INIFile = FileName
    Dim sTmp As String = CleanRet(INIRead(INIFile, Nothing, Nothing, ""))
    Sections = sTmp.Split(vbNullChar)
    ReDim Keys(Sections.Length - 1)
    For I = 0 To Sections.Length - 1
      If Sections(I).Length > 0 Then
        sTmp = CleanRet(INIRead(INIFile, Sections(I), Nothing, ""))
        sTmp.Replace(vbNullChar & vbNullChar, vbNullChar)
        Keys(I) = sTmp.Split(vbNullChar)
      End If
    Next
  End Sub

  Public Function GetSections() As String()
    Return Sections
  End Function

  Public Function GetKeys(Section As String) As String()
    For I As Integer = 0 To Sections.Length - 1
      If Sections(I) = Section Then
        Return Keys(I)
      End If
    Next
    Return Nothing
  End Function

  Public Function GetValue(Section As String, Key As String) As String
    Return INIRead(INIFile, Section, Key, String.Empty)
  End Function

  Private Function CleanRet(sTmp As String) As String
    sTmp.Replace(vbNullChar & vbNullChar, vbNullChar)
    If sTmp.Length > 0 Then
      Do While sTmp.Substring(sTmp.Length - 1) = vbNullChar
        sTmp = sTmp.Substring(0, sTmp.Length - 1)
      Loop
    End If
    Return sTmp
  End Function

  Private Function INIRead(INIPath As String, SectionName As String, KeyName As String, DefaultValue As String) As String
    Dim sData As String = Space(1024)
    Dim n As Integer = GetPrivateProfileStringW(SectionName, KeyName, DefaultValue, sData, sData.Length, INIPath)
    If n > 0 Then
      INIRead = sData.Substring(0, n)
    Else
      INIRead = DefaultValue
    End If
  End Function
End Class

Friend Class AlbumInfo
  Implements IDisposable

  Private WithEvents cInfo As CDDBNet
  Private WithEvents cArtwork As AppleNet
  Private WithEvents cCDText As New ReadCdTextWorker
  Public Structure TrackInfo
    Public Title As String
    Public Artist As String
  End Structure
  Private cDrive As IO.DriveInfo

  Public Event Info(Artist As String, Album As String, Tracks() As TrackInfo, Genre As String)
  Public Event Artwork(Picture As Drawing.Image)
  Private _artist, _album, _genre As String
  Private Rows As Integer

  Public Sub New(Drive As IO.DriveInfo)
    cDrive = Drive
    cCDText.RunWorkerAsync(Drive)
  End Sub

  Private Sub cCDText_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles cCDText.RunWorkerCompleted
    Dim result As ReadCdTextWorkerResult = TryCast(e.Result, ReadCdTextWorkerResult)
    If result.Success AndAlso result.CDText.TrackData.Rows.Count > 0 Then
      Dim sArtist, sAlbum As String
      sAlbum = StrConv(result.CDText.TrackData.Rows(0).Item(2), VbStrConv.ProperCase)
      sArtist = StrConv(result.CDText.TrackData.Rows(0).Item(3), VbStrConv.ProperCase)

      cArtwork = New AppleNet(sAlbum, AppleNet.Term.Album)
      Dim SpecialArtist As Boolean = True
      For I As Integer = 1 To result.CDText.TrackData.Rows.Count - 1
        If Split(result.CDText.TrackData.Rows(I).Item(2), "-").Length <> 2 Then
          SpecialArtist = False
          Exit For
        End If
      Next
      Dim Trks(result.CDText.TrackData.Rows.Count - 2) As TrackInfo

      For I As Integer = 1 To result.CDText.TrackData.Rows.Count - 1
        Dim sTitle As String = result.CDText.TrackData.Rows(I).Item(2)
        If SpecialArtist Then
          Trks(I - 1).Title = StrConv(Split(sTitle, "-", 2)(0).Trim, VbStrConv.ProperCase)
          Trks(I - 1).Artist = StrConv(Split(sTitle, "-", 2)(1).Trim, VbStrConv.ProperCase)
        Else
          If Not String.IsNullOrWhiteSpace(result.CDText.TrackData.Rows(I).Item(3)) Then sArtist = StrConv(result.CDText.TrackData.Rows(I).Item(3), VbStrConv.ProperCase)
          Trks(I - 1).Title = StrConv(sTitle, VbStrConv.ProperCase)
          Trks(I - 1).Artist = StrConv(sArtist, VbStrConv.ProperCase)
        End If
      Next
      _artist = sArtist
      _album = sAlbum
      RaiseEvent Info(sArtist, sAlbum, Trks, Nothing)
    Else
      Debug.Print(result.ErrorMessage)
      Try
        If cInfo Is Nothing Then cInfo = New CDDBNet
        Dim ID As String = CalcDiscID(cDrive.Name(0))
        cInfo.FindDisc(ID)
      Catch ex As Exception

      End Try
    End If
  End Sub

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
    Using drv As New LimeSeed.CDDrive
      drv.Open(Disc)
      If drv.IsCDReady And drv.Refresh And drv.GetNumTracks > 0 Then
        Rows = drv.GetNumTracks
        For I As Integer = 1 To drv.GetNumTracks
          StartSum += sumOfDigits(Math.Floor((drv.GetStartSector(I) + 150) / 75))
          LenSum += drv.TrackSize(I) / 176400
        Next
        StartSum = StartSum Mod 255
        If StartSum > &HF Then
          sTmp = Hex(StartSum)
        Else
          sTmp = "0" & Hex(StartSum)
        End If
        Dim iLenSum As UInteger = Math.Floor(LenSum)
        sTmp &= IIf(Hex(iLenSum).Length = 4, Hex(iLenSum), StrDup(4 - Hex(iLenSum).Length, "0"c) & Hex(iLenSum))
        If drv.GetNumTracks > &HF Then
          sTmp &= Hex(drv.GetNumTracks)
        Else
          sTmp &= "0" & Hex(drv.GetNumTracks)
        End If
        If FullID Then
          sTmp &= " " & drv.GetNumTracks
          For I As Integer = 1 To drv.GetNumTracks
            sTmp &= " " & (drv.GetStartSector(I) + 150)
          Next
          sTmp &= " " & (iLenSum + 2)
        End If
      End If
    End Using
    Return sTmp
  End Function

  Private Sub cInfo_FindError(Message As String) Handles cInfo.FindError
    Debug.Print("Error: " & Message)
  End Sub

  Private Sub cInfo_Finding(Artist As String, Album As String, Genre As String) Handles cInfo.Finding
    Debug.Print("Finding info for " & Album & " by " & Artist & "...")
    _album = Album
    _artist = Artist
    _genre = StrConv(Genre, VbStrConv.ProperCase)
  End Sub

  Private Sub cInfo_Finished() Handles cInfo.Finished
    'cInfo = Nothing
  End Sub

  Private Sub cInfo_Found(Data As String) Handles cInfo.Found

    Dim sSplits() As String = Split(Data, vbNewLine)
    Dim sArtist, sAlbum As String

    If GetLine(sSplits, "DTITLE").Contains(" / ") Then
      sArtist = Split(GetLine(sSplits, "DTITLE"), " / ")(0)
      sAlbum = Split(GetLine(sSplits, "DTITLE"), " / ")(1)
    Else
      sArtist = GetLine(sSplits, "DTITLE")
      sAlbum = GetLine(sSplits, "DTITLE")
    End If
    If Not sArtist = sAlbum Then
      cArtwork = New AppleNet(sAlbum, AppleNet.Term.Album)
    Else
      cArtwork = New AppleNet(sArtist, AppleNet.Term.Artist)
    End If

    Dim SpecialArtist As Boolean = True
    For I As Integer = 0 To Rows - 1
      If Split(GetLine(sSplits, "TTITLE" & I), " / ").Length <> 2 Then
        SpecialArtist = False
        Exit For
      End If
    Next
    Dim Trks(Rows - 1) As TrackInfo
    For I As Integer = 0 To Rows - 1
      Dim sTitle As String = GetLine(sSplits, "TTITLE" & I)
      If SpecialArtist Then
        Trks(I).Title = Split(sTitle, " / ", 2)(1)
        Trks(I).Artist = Split(sTitle, " / ", 2)(0)
      Else
        Trks(I).Title = sTitle
        Trks(I).Artist = sArtist
      End If
    Next
    RaiseEvent Info(sArtist, sAlbum, Trks, _genre)
  End Sub

  Private Function GetLine(Lines() As String, ByVal ID As String) As String
    For Each Line As String In Lines
      If Line.StartsWith(ID & "=") Then Return Line.Substring(Line.IndexOf("="c) + 1)
      If Line.StartsWith("# " & ID & ": ") Then Return Line.Substring(Line.IndexOf(": ") + 2)
    Next
    Return String.Empty
  End Function

  Private Sub cArtwork_Choices(sender As Object, e As AppleNet.ChoicesEventArgs) Handles cArtwork.Choices
    For Each ret In (From fRet In e.Rows Where fRet("artistName").ToString.ToLower.Contains(_artist.ToLower) And fRet("collectionName").ToString.ToLower.Contains(_album.ToLower))
      If Not ret("artworkUrl100") Is Nothing Then
        'For Each ret In (From fRet In e.Rows Where fRet.Contains("""artistName"":""" & _artist & """") And fRet.Contains("""collectionName"":""" & _album & """"))
        '  If ret.Contains("artworkUrl100") Then
        cArtwork.ChooseRow(ret)
        Return
      End If
    Next
    For Each ret In e.Rows
      If Not ret("artworkUrl100") Is Nothing Then
        cArtwork.ChooseRow(ret)
        Return
      End If
    Next
    Stop
    cArtwork = Nothing
  End Sub

  Private Sub cArtwork_Complete(sender As Object, e As AppleNet.CompleteEventArgs) Handles cArtwork.Complete
    RaiseEvent Artwork(Drawing.Image.FromStream(New IO.MemoryStream(e.Cover)).Clone)
    cArtwork = Nothing
  End Sub

  Private Sub cArtwork_Failed(sender As Object, e As AppleNet.FailEventArgs) Handles cArtwork.Failed
    Debug.Print("Failed: " & e.Error.Message)
    cArtwork = Nothing
  End Sub

#Region "IDisposable Support"
  Private disposedValue As Boolean
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        cInfo = Nothing
        cArtwork = Nothing
        cCDText.Dispose()
        cCDText = Nothing
      End If
    End If
    Me.disposedValue = True
  End Sub

  Public Sub Dispose() Implements IDisposable.Dispose
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region
End Class

Friend Class PowerProfile
  <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)> _
  Public Structure PowerCapabilities
    <MarshalAs(UnmanagedType.I1)> Public PowerButtonPresent As Boolean
    <MarshalAs(UnmanagedType.I1)> Public SleepButtonPresent As Boolean
    <MarshalAs(UnmanagedType.I1)> Public LidPresent As Boolean
    <MarshalAs(UnmanagedType.I1)> Public SystemS1 As Boolean
    <MarshalAs(UnmanagedType.I1)> Public SystemS2 As Boolean
    <MarshalAs(UnmanagedType.I1)> Public SystemS3 As Boolean
    <MarshalAs(UnmanagedType.I1)> Public SystemS4 As Boolean
    <MarshalAs(UnmanagedType.I1)> Public SystemS5 As Boolean
    <MarshalAs(UnmanagedType.I1)> Public HiberFilePresent As Boolean
    <MarshalAs(UnmanagedType.I1)> Public FullWake As Boolean
    <MarshalAs(UnmanagedType.I1)> Public VideoDimPresent As Boolean
    <MarshalAs(UnmanagedType.I1)> Public ApmPresent As Boolean
    <MarshalAs(UnmanagedType.I1)> Public UpsPresent As Boolean
    <MarshalAs(UnmanagedType.I1)> Public ThermalControl As Boolean
    <MarshalAs(UnmanagedType.I1)> Public ProcessorThrottle As Boolean
    Public ProcessorMinThrottle As Byte
    Public ProcessorMaxThrottle As Byte
    <MarshalAs(UnmanagedType.I1)> Public FastSystemS4 As Boolean
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=3)> _
    Public spare2 As Byte()
    <MarshalAs(UnmanagedType.I1)> Public DiskSpinDown As Boolean
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)> _
    Public spare3 As Byte()
    <MarshalAs(UnmanagedType.I1)> Public SystemBatteriesPresent As Boolean
    <MarshalAs(UnmanagedType.I1)> Public BatteriesAreShortTerm As Boolean
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=3)> _
    Public BatteryScale() As BatteryReportingScale
    Public AcOnLineWake As SystemPowerState
    Public SoftLidWake As SystemPowerState
    Public RtcWake As SystemPowerState
    Public MinDeviceWakeState As SystemPowerState
    Public DefaultLowLatencyWake As SystemPowerState
  End Structure

  <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)> _
  Public Structure BatteryReportingScale
    Dim Granularity As UInteger
    Dim Capacity As UInteger
  End Structure

  Public Enum SystemPowerState
    PowerSystemUnspecified = 0
    PowerSystemWorking = 1
    PowerSystemSleeping1 = 2
    PowerSystemSleeping2 = 3
    PowerSystemSleeping3 = 4
    PowerSystemHibernate = 5
    PowerSystemShutdown = 6
    PowerSystemMaximum = 7
  End Enum

  <DllImport("powrprof.dll", EntryPoint:="GetPwrCapabilities", SetLastError:=False)> _
  Public Shared Function GetPowerCapabilities(<[Out]()> ByRef pc As PowerCapabilities) As Boolean
  End Function

  Public Shared Function CanHibernate() As Boolean
    Dim pC As New PowerCapabilities
    If GetPowerCapabilities(pC) Then
      If pC.HiberFilePresent And pC.SystemS4 Then
        Return True
      Else
        Return False
      End If
    Else
      Return False
    End If
  End Function


End Class

Friend Class TaskbarFinder
  <DllImport("user32.dll", CharSet:=CharSet.Unicode)> _
  Private Shared Function FindWindow(lpClassName As String, lpWindowName As String) As IntPtr
  End Function
  <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)> _
  Public Shared Function FindWindowEx(parentHandle As IntPtr, childAfter As IntPtr, lclassName As String, windowTitle As String) As IntPtr
  End Function

  Friend Shared Function TaskbarVisible() As Boolean
    Dim shellTrayWnd As IntPtr = FindWindow("Shell_TrayWnd", String.Empty)
    If shellTrayWnd.Equals(IntPtr.Zero) Then
      Return False
    Else
      Dim trayNotifyWnd As IntPtr = FindWindowEx(shellTrayWnd, 0, "TrayNotifyWnd", String.Empty)
      If trayNotifyWnd.Equals(IntPtr.Zero) Then
        Return False
      Else
        Dim sysPagerWnd As IntPtr = FindWindowEx(trayNotifyWnd, 0, "SysPager", String.Empty)
        If sysPagerWnd.Equals(IntPtr.Zero) Then
          Return False
        Else
          Return True
        End If
      End If
    End If
  End Function
End Class