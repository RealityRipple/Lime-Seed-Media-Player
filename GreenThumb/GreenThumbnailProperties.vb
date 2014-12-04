Imports System.Runtime.InteropServices

<Guid("D82F694E-46DA-4B5A-8DA4-FCE35A1F6CE1"), ClassInterface(ClassInterfaceType.None), ComVisible(True), ProgId("GreenThumb.PropertyHandler")>
Public Class PropertyHandler
  Implements IPropertyStore, IPropertyStoreCapabilities, IInitializeWithFile, IInitializeWithItem

  Private Const S_OK As Integer = 0
  Private Const S_FALSE As Integer = 1

  Private PSGUID_DRM As New Guid("AEAC19E4-89AE-4508-B9B7-BB867ABEE2ED")
  Private PSGUID_Document As New Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9")
  Private PSGUID_Music As New Guid("56A3372E-CE9C-11D2-9F0E-006097C686F6")
  Private PSGUID_Audio As New Guid("64440490-4C8B-11D1-8B70-080036B11A03")
  Private PSGUID_Video As New Guid("64440491-4C8B-11D1-8B70-080036B11A03")

  Private sPath As String = Nothing
  Private PropertyList As Collections.Generic.Dictionary(Of PROPERTYKEY, Object)

  Private PK_DRM_Protected As New PROPERTYKEY(PSGUID_DRM, 2) 'Bool

  Private PK_Document_Title As New PROPERTYKEY(PSGUID_Document, 2) 'str
  Private PK_Document_Comment As New PROPERTYKEY(PSGUID_Document, 6) 'str

  Private PK_Music_Artist As New PROPERTYKEY(PSGUID_Music, 2) 'str
  Private PK_Music_Album As New PROPERTYKEY(PSGUID_Music, 4) 'str
  Private PK_Music_Year As New PROPERTYKEY(PSGUID_Music, 5) 'str
  Private PK_Music_Track As New PROPERTYKEY(PSGUID_Music, 7) 'ui4
  Private PK_Music_Genre As New PROPERTYKEY(PSGUID_Music, &HB) 'str

  Private PK_Audio_Format As New PROPERTYKEY(PSGUID_Audio, 2) 'str
  Private PK_Audio_Duration As New PROPERTYKEY(PSGUID_Audio, 3) 'ui8
  Private PK_Audio_Bitrate As New PROPERTYKEY(PSGUID_Audio, 4) 'ui4
  Private PK_Audio_Samplerate As New PROPERTYKEY(PSGUID_Audio, 5) 'ui4
  Private PK_Audio_Channels As New PROPERTYKEY(PSGUID_Audio, 7) 'ui4

  Private PK_Video_Title As New PROPERTYKEY(PSGUID_Video, 2) 'str
  Private PK_Video_Width As New PROPERTYKEY(PSGUID_Video, 3) 'ui4
  Private PK_Video_Height As New PROPERTYKEY(PSGUID_Video, 4) 'ui4
  Private PK_Video_Duration As New PROPERTYKEY(PSGUID_Video, 5) 'ui4
  Private PK_Video_FrameRate As New PROPERTYKEY(PSGUID_Video, 6) 'ui4
  Private PK_Video_Bitrate As New PROPERTYKEY(PSGUID_Video, 8) 'ui4
  Private PK_Video_Format As New PROPERTYKEY(PSGUID_Video, &HA) 'str

  Public Sub Initialize_Item(psi As IShellItem, grfMode As Integer) Implements IInitializeWithItem.Initialize
    Try
      Dim pszFilePath As String
      Dim pzPtr As IntPtr
      psi.GetDisplayName(SIGDN.FILESYSPATH, pzPtr)
      pszFilePath = Marshal.PtrToStringAuto(pzPtr)
      Marshal.FreeCoTaskMem(pzPtr)
      sPath = pszFilePath
      psi = Nothing
    Catch ex As Exception
      sPath = Nothing
    End Try
    InitProps()
  End Sub

  Public Sub Initialize_File(pszFilePath As String, grfMode As UInteger) Implements IInitializeWithFile.Initialize
    Try
      sPath = pszFilePath
    Catch ex As Exception
      sPath = Nothing
    End Try
    InitProps()
  End Sub

  Private Sub InitProps()
    PropertyList = New Dictionary(Of PROPERTYKEY, Object)
    Select Case IO.Path.GetExtension(sPath).ToLower.Substring(1)
      Case "ogg", "ogm"
        LoadVorbis()
      Case "flac"
        LoadVorbis()
        LoadID3v1()
        LoadID3v2()
      Case "mkv"
        LoadMKV()
      Case "mid", "midi", "rmi"
        LoadMIDI()
      Case Else
        LoadVorbis()
        LoadMKV()
        LoadID3v1()
        LoadID3v2()
        LoadRIFF()
        LoadMPEG()
        LoadMIDI()
    End Select
  End Sub

  Public Function Commit() As Integer Implements IPropertyStore.Commit
    Return S_OK
  End Function

  Public Function GetAt(iProp As UInteger, ByRef pkey As PROPERTYKEY) As Integer Implements IPropertyStore.GetAt
    pkey = PropertyList.ElementAt(iProp).Key
    Return S_OK
  End Function

  Public Function GetCount(ByRef cProps As UInteger) As Integer Implements IPropertyStore.GetCount
    cProps = PropertyList.Count
    Return S_OK
  End Function

  Public Function GetValue(key As PROPERTYKEY, ByRef pv As PropVariant) As Integer Implements IPropertyStore.GetValue
    pv = New PropVariant
    Select Case key.fmtid
      Case PSGUID_DRM
        Select Case key.pid
          Case PK_DRM_Protected.pid
            pv.type = VarEnum.VT_BOOL
            pv.union.boolValue = VARIANT_BOOL.VARIANT_FALSE
        End Select
      Case PSGUID_Document
        Select Case key.pid
          Case PK_Document_Comment.pid
            pv.type = typedef.VT_LPWSTR
            pv.union.lpwstrValue = Marshal.StringToHGlobalUni(PropertyList(key))
          Case PK_Document_Title.pid
            pv.type = typedef.VT_LPWSTR
            pv.union.lpwstrValue = Marshal.StringToHGlobalUni(PropertyList(key))
          Case Else
            pv.type = typedef.VT_EMPTY
            pv.union.ptr = IntPtr.Zero
        End Select
      Case PSGUID_Music
        Select Case key.pid
          Case PK_Music_Album.pid
            pv.type = typedef.VT_LPWSTR
            pv.union.lpwstrValue = Marshal.StringToHGlobalUni(PropertyList(key))
          Case PK_Music_Artist.pid
            pv.type = typedef.VT_LPWSTR
            pv.union.lpwstrValue = Marshal.StringToHGlobalUni(PropertyList(key))
          Case PK_Music_Genre.pid
            pv.type = typedef.VT_LPWSTR
            pv.union.lpwstrValue = Marshal.StringToHGlobalUni(PropertyList(key))
          Case PK_Music_Track.pid
            pv.type = typedef.VT_UI4
            pv.union.ui4Value = PropertyList(key)
          Case PK_Music_Year.pid
            pv.type = typedef.VT_LPWSTR
            pv.union.lpwstrValue = Marshal.StringToHGlobalUni(PropertyList(key))
          Case Else
            pv.type = typedef.VT_EMPTY
            pv.union.ptr = IntPtr.Zero
        End Select
      Case PSGUID_Audio
        Select Case key.pid
          Case PK_Audio_Bitrate.pid
            pv.type = typedef.VT_UI4
            pv.union.ui4Value = PropertyList(key)
          Case PK_Audio_Channels.pid
            pv.type = typedef.VT_UI4
            pv.union.ui4Value = PropertyList(key)
          Case PK_Audio_Duration.pid
            pv.type = typedef.VT_UI8
            pv.union.ui8Value = PropertyList(key)
          Case PK_Audio_Format.pid
            pv.type = typedef.VT_LPWSTR
            pv.union.lpwstrValue = Marshal.StringToHGlobalUni(PropertyList(key))
          Case PK_Audio_Samplerate.pid
            pv.type = typedef.VT_UI4
            pv.union.ui4Value = PropertyList(key)
          Case Else
            pv.type = typedef.VT_EMPTY
            pv.union.ptr = IntPtr.Zero
        End Select
      Case PSGUID_Video
        Select Case key.pid
          Case PK_Video_Bitrate.pid
            pv.type = typedef.VT_UI4
            pv.union.ui4Value = PropertyList(key)
          Case PK_Video_Duration.pid
            pv.type = typedef.VT_UI4
            pv.union.ui4Value = PropertyList(key)
          Case PK_Video_Format.pid
            pv.type = typedef.VT_LPWSTR
            pv.union.lpwstrValue = Marshal.StringToHGlobalUni(PropertyList(key))
          Case PK_Video_FrameRate.pid
            pv.type = typedef.VT_UI4
            pv.union.ui4Value = PropertyList(key)
          Case PK_Video_Height.pid
            pv.type = typedef.VT_UI4
            pv.union.ui4Value = PropertyList(key)
          Case PK_Video_Title.pid
            pv.type = typedef.VT_LPWSTR
            pv.union.lpwstrValue = Marshal.StringToHGlobalUni(PropertyList(key))
          Case PK_Video_Width.pid
            pv.type = typedef.VT_UI4
            pv.union.ui4Value = PropertyList(key)
          Case Else
            pv.type = typedef.VT_EMPTY
            pv.union.ptr = IntPtr.Zero
        End Select
      Case Else
        pv.type = typedef.VT_EMPTY
        pv.union.ptr = IntPtr.Zero
    End Select
    Return S_OK
  End Function

  Public Function SetValue(key As PROPERTYKEY, ByRef pv As Object) As Integer Implements IPropertyStore.SetValue
    Return S_OK
  End Function

  Public Function IsPropertyWritable(ByRef key As PROPERTYKEY) As Integer Implements IPropertyStoreCapabilities.IsPropertyWritable
    Return S_FALSE
  End Function

#Region "Parse"
  Private Sub LoadID3v1()
    Using ID3v1Tags As New Seed.clsID3v1(sPath)
      If ID3v1Tags.HasID3v1Tag Then
        If Not PropertyList.ContainsKey(PK_Music_Track) Then PropertyList.Add(PK_Music_Track, ID3v1Tags.Track)
        If Not PropertyList.ContainsKey(PK_Document_Title) Then PropertyList.Add(PK_Document_Title, ID3v1Tags.Title)
        If Not PropertyList.ContainsKey(PK_Music_Artist) Then PropertyList.Add(PK_Music_Artist, ID3v1Tags.Artist)
        If Not PropertyList.ContainsKey(PK_Music_Album) Then PropertyList.Add(PK_Music_Album, ID3v1Tags.Album)
        If Not PropertyList.ContainsKey(PK_Music_Genre) Then PropertyList.Add(PK_Music_Genre, Seed.clsID3v1.GenreName(ID3v1Tags.Genre))
        If Not PropertyList.ContainsKey(PK_Music_Year) Then PropertyList.Add(PK_Music_Year, ID3v1Tags.Year)
        If Not PropertyList.ContainsKey(PK_Document_Comment) Then PropertyList.Add(PK_Document_Comment, ID3v1Tags.Comment)
      End If
    End Using
  End Sub

  Private Sub LoadID3v2()
    Using ID3v2Tags As New Seed.clsID3v2(sPath)
      If ID3v2Tags.HasID3v2Tag Then
        For I As Integer = 0 To ID3v2Tags.FrameCount - 1
          Dim sFName As String = ID3v2Tags.FrameName(I)
          Dim sFrame As String = Seed.clsID3v2.GetFrameName(sFName)
          Dim fData() As Object = ID3v2Tags.ParseFrame(sFName, ID3v2Tags.FrameData(I))
          If sFName.StartsWith("T") Or sFName.StartsWith("W") Then
            For J As Integer = 0 To fData.Length - 1
              fData(J) = fData(J).Replace(vbCr, vbNewLine)
            Next
          End If
          Select Case sFrame
            Case "Track" : If Not PropertyList.ContainsKey(PK_Music_Track) Then PropertyList.Add(PK_Music_Track, fData(0))
            Case "Title" : If Not PropertyList.ContainsKey(PK_Document_Title) Then PropertyList.Add(PK_Document_Title, fData(0))
            Case "Artist" : If Not PropertyList.ContainsKey(PK_Music_Artist) Then PropertyList.Add(PK_Music_Artist, fData(0))
            Case "Album" : If Not PropertyList.ContainsKey(PK_Music_Album) Then PropertyList.Add(PK_Music_Album, fData(0))
            Case "Genre" : If Not PropertyList.ContainsKey(PK_Music_Genre) Then PropertyList.Add(PK_Music_Genre, fData(0))
            Case "Year" : If Not PropertyList.ContainsKey(PK_Music_Year) Then PropertyList.Add(PK_Music_Year, fData(0))
            Case "Comments" : If Not PropertyList.ContainsKey(PK_Document_Comment) Then PropertyList.Add(PK_Document_Comment, fData(0))
          End Select
        Next
      End If
    End Using
  End Sub

  Private Sub LoadMPEG()
    Using cHeader As New Seed.clsHeaderLoader(sPath)
      If cHeader.cMPEG IsNot Nothing AndAlso cHeader.cMPEG.CheckValidity Then
        If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "MPEG-" & cHeader.cMPEG.GetMPEGVer & " Layer-" & StrDup(cHeader.cMPEG.GetMPEGLayer, "I"))
        If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, cHeader.cMPEG.GetBitrate)
        If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, cHeader.cMPEG.GetSampleRate)
        If Not cHeader.cMPEG.GetChannels = "Unknown" Then
          If Not PropertyList.ContainsKey(PK_Audio_Channels) Then
            If cHeader.cMPEG.GetChannels = "Single Channel" Then
              PropertyList.Add(PK_Audio_Channels, 1)
            Else
              PropertyList.Add(PK_Audio_Channels, 2)
            End If
          End If
        End If
        If Not PropertyList.ContainsKey(PK_Audio_Duration) Then PropertyList.Add(PK_Audio_Duration, cHeader.Duration)
      End If
    End Using
  End Sub

  Private Sub LoadVorbis()
    Using cVorbis As New Seed.clsVorbis(sPath)
      If cVorbis.HasVorbis Then
        If Not String.IsNullOrEmpty(cVorbis.Track) Then If Not PropertyList.ContainsKey(PK_Music_Track) Then PropertyList.Add(PK_Music_Track, cVorbis.Track)
        If Not String.IsNullOrEmpty(cVorbis.Title) Then If Not PropertyList.ContainsKey(PK_Document_Title) Then PropertyList.Add(PK_Document_Title, cVorbis.Title)
        If Not String.IsNullOrEmpty(cVorbis.Artist) Then If Not PropertyList.ContainsKey(PK_Music_Artist) Then PropertyList.Add(PK_Music_Artist, cVorbis.Artist)
        If Not String.IsNullOrEmpty(cVorbis.Album) Then If Not PropertyList.ContainsKey(PK_Music_Album) Then PropertyList.Add(PK_Music_Album, cVorbis.Album)
        If Not String.IsNullOrEmpty(cVorbis.Genre) Then If Not PropertyList.ContainsKey(PK_Music_Genre) Then PropertyList.Add(PK_Music_Genre, cVorbis.Genre)
        If Not String.IsNullOrEmpty(cVorbis.RecordDate) Then If Not PropertyList.ContainsKey(PK_Music_Year) Then PropertyList.Add(PK_Music_Year, cVorbis.RecordDate)
        If Not String.IsNullOrEmpty(cVorbis.Description) Then If Not PropertyList.ContainsKey(PK_Document_Comment) Then PropertyList.Add(PK_Document_Comment, cVorbis.Description)
        Dim dDur As Double = 0
        Using mpTemp As New Seed.ctlSeed
          dDur = mpTemp.GetFileDuration(sPath)
        End Using
        Dim lLen As Long = My.Computer.FileSystem.GetFileInfo(sPath).Length - (IIf(cVorbis.HasVorbis, cVorbis.HeaderLength, 0))
        If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, (lLen * 8 / dDur))
        If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, cVorbis.File_Rate)
        If cVorbis.File_Channels > 0 Then
          If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, cVorbis.File_Channels)
        End If
        If Not PropertyList.ContainsKey(PK_Audio_Duration) Then PropertyList.Add(PK_Audio_Duration, dDur * 10000000)
      End If
    End Using
  End Sub

  Private Sub LoadMKV()
    Try
      Using mkvInfo As New Seed.clsMKVHeaders(sPath)
        If mkvInfo.HasMKV Then
          If Not String.IsNullOrEmpty(mkvInfo.SegmentInfo.Title) Then If Not PropertyList.ContainsKey(PK_Video_Title) Then PropertyList.Add(PK_Video_Title, mkvInfo.SegmentInfo.Title)
          If mkvInfo.SegmentInfo.DateUTC.CompareTo(New Date(2001, 1, 1)) <> 0 Then If Not PropertyList.ContainsKey(PK_Music_Year) Then PropertyList.Add(PK_Music_Year, mkvInfo.SegmentInfo.DateUTC.Year.ToString)
          For I As Integer = 0 To mkvInfo.TrackEntries.Length - 1
            If mkvInfo.TrackEntries(I).Video.Exists Then
              If Not PropertyList.ContainsKey(PK_Video_Width) Then PropertyList.Add(PK_Video_Width, mkvInfo.TrackEntries(I).Video.PixelWidth)
              If Not PropertyList.ContainsKey(PK_Video_Height) Then PropertyList.Add(PK_Video_Height, mkvInfo.TrackEntries(I).Video.PixelHeight)
              If mkvInfo.TrackEntries(I).DefaultDuration <> 0 Then If Not PropertyList.ContainsKey(PK_Video_FrameRate) Then PropertyList.Add(PK_Video_FrameRate, CUShort(Math.Round(1 / (mkvInfo.TrackEntries(I).DefaultDuration / 1000000000), 3) * 1000))
            End If
        If mkvInfo.TrackEntries(I).Audio.Exists Then
          If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, mkvInfo.TrackEntries(I).Audio.SamplingFrequency)
          If mkvInfo.TrackEntries(I).Audio.Channels > 0 Then If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, mkvInfo.TrackEntries(I).Audio.Channels)
        End If
          Next
          If mkvInfo.SegmentInfo.Duration <> 0 Then
            If Not PropertyList.ContainsKey(PK_Video_Duration) Then PropertyList.Add(PK_Video_Duration, mkvInfo.SegmentInfo.Duration * 10000)
            If Not PropertyList.ContainsKey(PK_Audio_Duration) Then PropertyList.Add(PK_Audio_Duration, mkvInfo.SegmentInfo.Duration * 10000)
          End If
        End If
      End Using
    Catch ex As Exception
    End Try
  End Sub

  Private Sub LoadMIDI()
    Try
      Using cHeader As New Seed.clsMIDI(sPath)
        If cHeader.IsMIDI Or 1 = 1 Then 'fuck validity
          If cHeader.TrackCount > 0 Then
            Select Case cHeader.TrackFormat
              Case Seed.clsMIDI.TrackType.SingleTrack : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "Single Track")
              Case Seed.clsMIDI.TrackType.MultiSynch : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "Mulitple Synchronous Tracks")
              Case Seed.clsMIDI.TrackType.MultiAsynch : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "Multiple Asynchronous Tracks")
            End Select
            Dim dDur As Double = 0
            Using mpTemp As New Seed.ctlSeed
              dDur = mpTemp.GetFileDuration(sPath)
            End Using
            If Not PropertyList.ContainsKey(PK_Audio_Duration) Then PropertyList.Add(PK_Audio_Duration, ConvertTimeVal(dDur))
          End If
        End If
      End Using
    Catch ex As Exception
    End Try
  End Sub

  Private Sub LoadRIFF()
    Using cHeader As New Seed.clsRIFF(sPath)
      If cHeader.IsValid Then

        If cHeader.IsWAV Then
          Dim didFormat As Boolean = False
          Dim didBitrate As Boolean = False
          Dim didSampleRate As Boolean = False
          Dim didChannels As Boolean = False

          If cHeader.WAVData.Format.cbSize >= 22 Then
            Select Case cHeader.WAVData.SubFormat.ToString.ToLower
              Case "6dba3190-67bd-11cf-a0f7-0020afd156e4" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "Analog") : didFormat = True
              Case "00000001-0000-0010-8000-00aa00389b71" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "PCM") : didFormat = True
              Case "00000003-0000-0010-8000-00aa00389b71" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "Float (IEEE)") : didFormat = True
              Case "00000009-0000-0010-8000-00aa00389b71" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "DRM") : didFormat = True
              Case "00000006-0000-0010-8000-00aa00389b71" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "A-Law") : didFormat = True
              Case "00000007-0000-0010-8000-00aa00389b71" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "µ-Law") : didFormat = True
              Case "00000002-0000-0010-8000-00aa00389b71" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "ADPCM") : didFormat = True
              Case "00000050-0000-0010-8000-00aa00389b71" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "MPEG") : didFormat = True
              Case "4995daee-9ee6-11d0-a40e-00a0c9223196" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "RIFF") : didFormat = True
              Case "e436eb8b-524f-11ce-9f53-0020af0ba770" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "RIFF WAVE") : didFormat = True
              Case "1d262760-e957-11cf-a5d6-28db04c10000" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "MIDI") : didFormat = True
              Case "2ca15fa0-6cfe-11cf-a5d6-28dB04c10000" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "MIDI Bus") : didFormat = True
              Case "4995daf0-9ee6-11d0-a40e-00a0c9223196" : If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "RIFF MIDI") : didFormat = True
            End Select
            If cHeader.WAVData.dwChannelMask > 0 Then
              Dim iChan As Integer = 0

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontLeft) = Seed.clsRIFF.ChannelStruct.FrontLeft Then iChan += 1
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontCenterLeft) = Seed.clsRIFF.ChannelStruct.FrontCenterLeft Then iChan += 1
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontCenter) = Seed.clsRIFF.ChannelStruct.FrontCenter Then iChan += 1
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontCenterRight) = Seed.clsRIFF.ChannelStruct.FrontCenterRight Then iChan += 1
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontRight) = Seed.clsRIFF.ChannelStruct.FrontRight Then iChan += 1

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.SideLeft) = Seed.clsRIFF.ChannelStruct.SideLeft Then iChan += 1
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.SideRight) = Seed.clsRIFF.ChannelStruct.SideRight Then iChan += 1

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.RearLeft) = Seed.clsRIFF.ChannelStruct.RearLeft Then iChan += 1
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.RearCenter) = Seed.clsRIFF.ChannelStruct.RearCenter Then iChan += 1
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.RearRight) = Seed.clsRIFF.ChannelStruct.RearRight Then iChan += 1

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopCenter) = Seed.clsRIFF.ChannelStruct.TopCenter Then iChan += 1

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopFrontLeft) = Seed.clsRIFF.ChannelStruct.TopFrontLeft Then iChan += 1
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopFrontCenter) = Seed.clsRIFF.ChannelStruct.TopFrontCenter Then iChan += 1
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopFrontRight) = Seed.clsRIFF.ChannelStruct.TopFrontRight Then iChan += 1

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopRearLeft) = Seed.clsRIFF.ChannelStruct.TopRearLeft Then iChan += 1
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopRearCenter) = Seed.clsRIFF.ChannelStruct.TopRearCenter Then iChan += 1
              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopRearRight) = Seed.clsRIFF.ChannelStruct.TopRearRight Then iChan += 1

              If (cHeader.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.LFE) = Seed.clsRIFF.ChannelStruct.LFE Then iChan += 1

              If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, iChan)
              didChannels = True
            End If
          End If

          If cHeader.IsDTS Then
            If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, "DTS")
            didFormat = True
            Select Case cHeader.DTSData.iRATE
              Case 0 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 32 * 1024) : didSampleRate = True
              Case 1 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 56 * 1024) : didSampleRate = True
              Case 2 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 64 * 1024) : didSampleRate = True
              Case 3 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 96 * 1024) : didSampleRate = True
              Case 4 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 112 * 1024) : didSampleRate = True
              Case 5 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 128 * 1024) : didSampleRate = True
              Case 6 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 192 * 1024) : didSampleRate = True
              Case 7 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 224 * 1024) : didSampleRate = True
              Case 8 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 256 * 1024) : didSampleRate = True
              Case 9 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 320 * 1024) : didSampleRate = True
              Case 10 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 384 * 1024) : didSampleRate = True
              Case 11 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 448 * 1024) : didSampleRate = True
              Case 12 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 512 * 1024) : didSampleRate = True
              Case 13 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 576 * 1024) : didSampleRate = True
              Case 14 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 640 * 1024) : didSampleRate = True
              Case 15 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 768 * 1024) : didSampleRate = True
              Case 16 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 960 * 1024) : didSampleRate = True
              Case 17 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 1024 * 1024) : didSampleRate = True
              Case 18 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 1152 * 1024) : didSampleRate = True
              Case 19 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 1280 * 1024) : didSampleRate = True
              Case 20 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 1344 * 1024) : didSampleRate = True
              Case 21 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 1408 * 1024) : didSampleRate = True
              Case 22 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 1411.2 * 1024) : didSampleRate = True
              Case 23 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 1472 * 1024) : didSampleRate = True
              Case 24 : If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, 1536 * 1024) : didSampleRate = True
                'Case 25 : PropertyList.Add(PK_Audio_Bitrate, "Open") : didSampleRate = True
            End Select
            Select Case cHeader.DTSData.iSFREQ
              Case 1 : If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, 8000) : didSampleRate = True
              Case 2 : If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, 16000) : didSampleRate = True
              Case 3 : If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, 32000) : didSampleRate = True
              Case 6 : If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, 11025) : didSampleRate = True
              Case 7 : If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, 22050) : didSampleRate = True
              Case 8 : If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, 44100) : didSampleRate = True
              Case 11 : If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, 12000) : didSampleRate = True
              Case 12 : If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, 24000) : didSampleRate = True
              Case 13 : If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, 48000) : didSampleRate = True
            End Select
            Select Case cHeader.DTSData.iAMODE
              Case 0 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 1) : didChannels = True
              Case 1 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 1) : didChannels = True
              Case 2 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 2) : didChannels = True
              Case 3 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 2) : didChannels = True
              Case 4 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 2) : didChannels = True
              Case 5 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 3) : didChannels = True
              Case 6 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 3) : didChannels = True
              Case 7 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 4) : didChannels = True
              Case 8 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 4) : didChannels = True
              Case 9 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 5) : didChannels = True
              Case 10 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 6) : didChannels = True
              Case 11 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 6) : didChannels = True
              Case 12 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 6) : didChannels = True
              Case 13 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 7) : didChannels = True
              Case 14 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 8) : didChannels = True
              Case 15 : If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, 8) : didChannels = True
            End Select
          End If

          If Not didFormat Then If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, WAVAudioCodecs(cHeader.WAVData.Format.Format.wFormatTag))
          If Not didBitrate Then If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, cHeader.WAVData.Format.Format.nAvgBytesPerSec)
          If Not didSampleRate Then If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, cHeader.WAVData.Format.Format.nSamplesPerSec)
          If Not didChannels Then If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, cHeader.WAVData.Format.Format.nChannels)


        End If
        If cHeader.IsAVI Then
          If cHeader.AVIDIVXData.FileID = "DIVXTAG" Then
            If Not PropertyList.ContainsKey(PK_Video_Title) Then PropertyList.Add(PK_Video_Title, cHeader.AVIDIVXData.Movie)
            If Not PropertyList.ContainsKey(PK_Music_Artist) Then PropertyList.Add(PK_Music_Artist, cHeader.AVIDIVXData.Author)
            If Not PropertyList.ContainsKey(PK_Music_Year) Then PropertyList.Add(PK_Music_Year, cHeader.AVIDIVXData.Year)
            If Not PropertyList.ContainsKey(PK_Document_Comment) Then PropertyList.Add(PK_Document_Comment, cHeader.AVIDIVXData.Comment)
            If Not PropertyList.ContainsKey(PK_Music_Genre) Then PropertyList.Add(PK_Music_Genre, cHeader.AVIDIVXData.Genre.ToString)
          End If
          If cHeader.AVIINFOData IsNot Nothing Then
            For Each item In cHeader.AVIINFOData
              Select Case AVIInfoKeys(item.Key)
                Case "Name" : If Not PropertyList.ContainsKey(PK_Video_Title) Then PropertyList.Add(PK_Video_Title, item.Value)
                Case "Original Artist" : If Not PropertyList.ContainsKey(PK_Music_Artist) Then PropertyList.Add(PK_Music_Artist, item.Value)
                Case "Genre" : If Not PropertyList.ContainsKey(PK_Music_Year) Then PropertyList.Add(PK_Music_Year, cHeader.AVIDIVXData.Year)
                Case "Comments" : If Not PropertyList.ContainsKey(PK_Document_Comment) Then PropertyList.Add(PK_Document_Comment, cHeader.AVIDIVXData.Comment)
              End Select
            Next
          End If
          If Not PropertyList.ContainsKey(PK_Video_Width) Then PropertyList.Add(PK_Video_Width, cHeader.AVIMainData.dwWidth)
          If Not PropertyList.ContainsKey(PK_Video_Height) Then PropertyList.Add(PK_Video_Height, cHeader.AVIMainData.dwHeight)

          Dim a As Integer
          For I As Integer = 0 To cHeader.AVIStreamCount - 1
            Dim cStream As Seed.clsRIFF.AVISTREAMHEADER = cHeader.AVIStreamData(I)
            Select Case cStream.fccType
              Case "auds"
                If a < cHeader.AVIAudioCount Then
                  Dim cWAV As Seed.clsRIFF.WAVEFORMATEX = cHeader.AVIAudioData(a)
                  If Not PropertyList.ContainsKey(PK_Audio_Format) Then PropertyList.Add(PK_Audio_Format, WAVAudioCodecs(cWAV.Format.wFormatTag))
                  If Not PropertyList.ContainsKey(PK_Audio_Bitrate) Then PropertyList.Add(PK_Audio_Bitrate, cWAV.Format.nAvgBytesPerSec * 8)
                  If Not PropertyList.ContainsKey(PK_Audio_Samplerate) Then PropertyList.Add(PK_Audio_Samplerate, cWAV.Format.nSamplesPerSec)
                  If Not PropertyList.ContainsKey(PK_Audio_Channels) Then PropertyList.Add(PK_Audio_Channels, cWAV.Format.nChannels)
                End If
                a += 1
            End Select
          Next
        End If
        Dim dDur As Double = 0
        Using mpTemp As New Seed.ctlSeed
          dDur = mpTemp.GetFileDuration(sPath)
        End Using
        If Not PropertyList.ContainsKey(PK_Video_Duration) Then PropertyList.Add(PK_Video_Duration, ConvertTimeVal(dDur))
        If Not PropertyList.ContainsKey(PK_Audio_Duration) Then PropertyList.Add(PK_Audio_Duration, ConvertTimeVal(dDur))
      End If
    End Using
  End Sub

#End Region





  '    [ComRegisterFunctionAttribute]
  '    public static void RegisterFunction(Type t)
  '    {

  '      try
  '      {
  '        RegistryKey regHKCR = Registry.ClassesRoot;
  '        regHKCR = regHKCR.CreateSubKey(".test");
  '        regHKCR.SetValue(null, "TestShell.PropertyHandler");

  '        regHKCR = Registry.ClassesRoot;
  '        regHKCR = regHKCR.CreateSubKey("CLSID\\{9BC59AF4-41E3-49B1-9A62-17F4C92D081F}");
  '        regHKCR.SetValue(null, "Test Property");
  '        regHKCR.SetValue("ManualSafeSave", 1);
  '        regHKCR.SetValue("Title", 2);
  '        regHKCR.SetValue("Whatever", 3);
  '        regHKCR = regHKCR.CreateSubKey("InProcServer32");

  '        regHKCR.SetValue(null, @"C:\Windows\System32\mscoree.dll");
  '        regHKCR.SetValue("ThreadingModel", "Apartment");

  '        RegistryKey regHKLM;
  '        regHKLM = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\PropertySystem\\PropertyHandlers\\.test");
  '        regHKLM.SetValue(null, "{9BC59AF4-41E3-49B1-9A62-17F4C92D081F}");
  '        regHKLM = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved");
  '        regHKLM.SetValue("{9BC59AF4-41E3-49B1-9A62-17F4C92D081F}", "Test Property");
  '      }

  '      catch (Exception ex)//HKEY_CLASSES_ROOT\CLSID\{9BC59AF4-41E3-49B1-9A62-17F4C92D081F}\Implemented Categories\{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}
  '      {
  '#If DEBUG Then
  '        System.Windows.Forms.MessageBox.Show(ex.Message + System.Environment.NewLine + ex.StackTrace);
  '#End If
  '      }
  '#If DEBUG Then
  '      //SHShellRestart();
  '#End If

  Private Function KRater(bitrate As Long, ext As String) As String
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
  Private Function ConvertTimeVal(Seconds As Double) As String
    If Seconds < 0 Then Return "--:--"
    Dim lHours As Long = Seconds \ 60 \ 60
    Seconds = Seconds - (lHours * 60 * 60)
    Dim lMinutes As Long = Seconds \ 60
    Dim lSeconds As Long = Seconds - (lMinutes * 60)
    If lHours > 0 Then
      Return lHours & ":" & Format(lMinutes, "00") & ":" & Format(lSeconds, "00")
    Else
      Return lMinutes & ":" & Format(lSeconds, "00")
    End If
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
End Class