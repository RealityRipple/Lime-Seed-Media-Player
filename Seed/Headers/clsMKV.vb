Public Class clsMKVHeaders
  Implements IDisposable

  Private Class MKVBinaryReader
    Inherits IO.BinaryReader

    Public Sub New(Path As String)
      MyBase.New(New IO.FileStream(Path, IO.FileMode.Open, IO.FileAccess.Read))
    End Sub

    Public Sub New(ByRef bReader As MKVBinaryReader)
      MyBase.New(New IO.MemoryStream(bReader.GetBytes))
    End Sub

    Public Function GetElementID() As UInt64
      If MyBase.BaseStream.Position < MyBase.BaseStream.Length - 1 Then
        Dim IDClass As Integer = GetLength(MyBase.ReadByte)
        MyBase.BaseStream.Position -= 1
        Return BytesToVal(MyBase.ReadBytes(IDClass))
      Else
        Return 0
      End If
    End Function

    Public Function GetSValue() As Long
      Return BytesToSVal(MyBase.ReadBytes(CalcLengthVal(Me)))
    End Function

    Public Function GetValue() As UInt64
      Return BytesToVal(MyBase.ReadBytes(CalcLengthVal(Me)))
    End Function

    Public Function GetFloat() As Single
      Return BytesToFloat(MyBase.ReadBytes(CalcLengthVal(Me)))
    End Function

    Public Function GetASCIIString() As String
      Return System.Text.Encoding.ASCII.GetString(MyBase.ReadBytes(CalcLengthVal(Me)))
    End Function

    Public Function GetUTF8String() As String
      Return System.Text.Encoding.UTF8.GetString(MyBase.ReadBytes(CalcLengthVal(Me)))
    End Function

    Public Function GetBytes() As Byte()
      Return MyBase.ReadBytes(CalcLengthVal(Me))
    End Function

    Public Function EndOfStream() As Boolean
      Return MyBase.BaseStream.Position >= MyBase.BaseStream.Length
    End Function

    Private Function GetLength(Peeked As Byte) As Integer
      Dim bPre As BitArray = New BitArray({Peeked})
      For I As Integer = 7 To 0 Step -1
        If bPre(I) Then Return (8 - I)
      Next
      Return 0
    End Function

    Public Function CalcLengthVal(ByRef bReader As MKVBinaryReader) As UInt64
      If MyBase.BaseStream.Position < MyBase.BaseStream.Length - 1 Then
        Dim LenLen As Integer = GetLength(bReader.ReadByte)
        bReader.BaseStream.Position -= 1
        Return BytesToVal(bReader.ReadBytes(LenLen)) Xor (1UL << (LenLen * 7))
      Else
        Return 0
      End If
    End Function

    Private Function BytesToSVal(bIn() As Byte) As Int64
      Dim bPad(7) As Byte
      Array.Reverse(bIn)
      Array.Copy(bIn, bPad, bIn.Length)
      Return BitConverter.ToInt64(bPad, 0)
    End Function

    Private Function BytesToVal(bIn() As Byte) As UInt64
      Try
        Dim bPad(7) As Byte
        Array.Reverse(bIn)
        Array.Copy(bIn, bPad, bIn.Length)
        Return BitConverter.ToUInt64(bPad, 0)
      Catch ex As Exception
        Return 0
      End Try
    End Function

    Private Function BytesToFloat(bIn() As Byte) As Double
      If bIn.Length <= 4 Then
        Dim bPad(3) As Byte
        Array.Reverse(bIn)
        Array.Copy(bIn, bPad, bIn.Length)
        Return BitConverter.ToSingle(bPad, 0)
      ElseIf bIn.Length <= 8 Then
        Dim bPad(8) As Byte
        Array.Reverse(bIn)
        Array.Copy(bIn, bPad, bIn.Length)
        Return BitConverter.ToDouble(bPad, 0)
      Else
        Return 0
      End If
    End Function
  End Class
  Public Class MKVReadable
    Public Shared Function SeekID(ByRef id As Byte()) As String
      If id.SequenceEqual({&H18, &H53, &H80, &H67}) Then Return "Segment"
      If id.SequenceEqual({&H11, &H4D, &H9B, &H74}) Then Return "Seek"
      If id.SequenceEqual({&H15, &H49, &HA9, &H66}) Then Return "Segment Info"
      If id.SequenceEqual({&H16, &H54, &HAE, &H6B}) Then Return "Track"
      If id.SequenceEqual({&H10, &H43, &HA7, &H70}) Then Return "Chapter"
      If id.SequenceEqual({&H1F, &H43, &HB6, &H75}) Then Return "Data Chunk"
      If id.SequenceEqual({&H1C, &H53, &HBB, &H6B}) Then Return "Cue Info"
      If id.SequenceEqual({&H19, &H41, &HA4, &H69}) Then Return "Attachment"
      If id.SequenceEqual({&HEC}) Then Return "Void Padding"
      Return BitConverter.ToString(id)
    End Function

    Public Shared Function TrackType(type As UInt64) As String
      Select Case type
        Case 1 : Return "Video"
        Case 2 : Return "Audio"
        Case 3 : Return "Complex"
        Case &H10 : Return "Logo"
        Case &H11 : Return "Subtitle"
        Case &H12 : Return "Buttons"
        Case &H20 : Return "Control"
        Case Else : Return "Unknown: " & Hex(type)
      End Select
    End Function

    Public Shared Function VideoStereoMode(mode As UInt64) As String
      Select Case mode
        Case 0 : Return "Mono"
        Case 1 : Return "Side by Side (Left eye first)"
        Case 2 : Return "Top to Bottom (Right eye first)"
        Case 3 : Return "Top to Bottom (Left eye first)"
        Case 4 : Return "Checkerboard (Right first)"
        Case 5 : Return "Checkerboard (Left first)"
        Case 6 : Return "Row Interleaved (Right first)"
        Case 7 : Return "Row Interleaved (Left first)"
        Case 8 : Return "Column Interleaved (Right first)"
        Case 9 : Return "Column Interleaved (Left first)"
        Case 10 : Return "Anaglyph (Cyan/Red)"
        Case 11 : Return "Side by Side (Right eye first)"
        Case 12 : Return "Anaglyph (Green/Magenta)"
        Case 13 : Return "Both eyes Laced in one Block (Left eye is first)"
        Case 14 : Return "Both eyes Laced in one Block (Right eye is first)"
        Case Else : Return "Unknown " & mode
      End Select
    End Function

    Public Shared Function VideoOldStereoMode(Mode As UInt64) As String
      Select Case Mode
        Case 0 : Return "Mono"
        Case 1 : Return "Right eye"
        Case 2 : Return "Left eye"
        Case 3 : Return "Both eyes"
        Case Else : Return "Unknown " & Mode
      End Select
    End Function

    Public Shared Function VideoDisplayUnit(Unit As UInt64) As String
      Select Case Unit
        Case 0, 255 : Return "Pixels"
        Case 1 : Return "Centimeters"
        Case 2 : Return "Inches"
        Case 3 : Return "Aspect Ratio"
        Case Else : Return "Unknown " & Unit
      End Select
    End Function

    Public Shared Function VideoAspectRatioType(RatioType As UInt64) As String
      Select Case RatioType
        Case 0 : Return "Free Resizing"
        Case 1 : Return "Keep Aspect Ratio"
        Case 2 : Return "Fixed"
        Case Else : Return "Unknown " & RatioType
      End Select
    End Function
  End Class
  Public Structure EBMLHeader
    Public Version As UInt64
    Public ReadVersion As UInt64
    Public MaxIDLength As UInt64
    Public MaxSizeLength As UInt64
    Public DocType As String
    Public DocTypeVersion As UInt64
    Public DocTypeReadVersion As UInt64
  End Structure
  Public Structure SeekContents
    Public SeekID As Byte()
    Public SeekPosition As UInt64
  End Structure
  Public Structure SeekEntry
    Public Contents() As SeekContents
  End Structure
  Public Structure TranslateInfo
    Public EditionUID As UInt64()
    Public Codec As UInt64
    Public TrackID As Byte()
  End Structure
  Public Structure SegmentInformation
    Public SegmentUID As Byte()
    Public SegmentFilename As String
    Public PrevUID As Byte()
    Public PrevFilename As String
    Public NextUID As Byte()
    Public NextFilename As String
    Public SegmentFamily As Byte()()
    Public ChapterTranslate() As TranslateInfo
    Public TimecodeScale As UInt64
    Public Duration As Double
    Public DateUTC As Date
    Public Title As String
    Public MuxingApp As String
    Public WritingApp As String
  End Structure
  Public Structure VideoSettings
    Public Exists As Boolean
    Public FlagInterlaced As Boolean
    Public StereoMode As UInt64
    Public OldStereoMode As UInt64
    Public PixelWidth As UInt64
    Public PixelHeight As UInt64
    Public PixelCropBottom As UInt64
    Public PixelCropTop As UInt64
    Public PixelCropLeft As UInt64
    Public PixelCropRight As UInt64
    Public DisplayWidth As UInt64
    Public DisplayHeight As UInt64
    Public DisplayUnit As UInt64
    Public AspectRatioType As UInt64
    Public ColorSpace As Byte()
    Public GammaValue As Double
    Public FrameRate As Double
  End Structure
  Public Structure AudioSettings
    Public Exists As Boolean
    Public SamplingFrequency As Double
    Public OutputSamplingFrequency As Double
    Public Channels As UInt64
    Public ChannelPositions As Byte()
    Public BitDepth As UInt64
  End Structure
  Public Structure Plane
    Public UID As UInt64
    Public PlaneType As UInt64
  End Structure
  Public Structure CombinePlanes
    Public TrackPlane() As Plane
  End Structure
  Public Structure JoinBlock
    Public UID As UInt64
  End Structure
  Public Structure Operation
    Public Exists As Boolean
    Public TrackCombinePlanes As CombinePlanes
    Public TrackJoinBlocks() As JoinBlock
  End Structure
  Public Structure Compression
    Public Algorithm As UInt64
    Public Settings As Byte()
  End Structure
  Public Structure Encryption
    Public Algorithm As UInt64
    Public KeyID As Byte()
    Public Signature As Byte()
    Public SigKeyID As Byte()
    Public SigAlgorithm As UInt64
    Public SigHashAlgorithm As UInt64
  End Structure
  Public Structure Encoding
    Public Order As UInt64
    Public Scope As UInt64
    Public EncType As UInt64
    Public ContentCompression As Compression
    Public ContentEncryption As Encryption
  End Structure
  Public Structure Encodings
    Public ContentEncoding() As Encoding
  End Structure
  Public Structure TrackInformation
    Public TrackNumber As UInt64
    Public TrackUID As UInt64
    Public TrackType As UInt64
    Public FlagEnabled As Boolean
    Public FlagDefault As Boolean
    Public FlagForced As Boolean
    Public FlagLacing As Boolean
    Public MinCache As UInt64
    Public MaxCache As UInt64
    Public DefaultDuration As UInt64
    Public TrackTimecodeScale As Double
    Public TrackOffset As Integer
    Public MaxBlockAdditionID As UInt64
    Public TrackName As String
    Public Language As String
    Public CodecID As String
    Public CodecPrivate As Byte()
    Public CodecName As String
    Public AttachmentLink As UInt64
    Public CodecSettings As String
    Public CodecInfoURL As String
    Public CodecDownloadURL As String
    Public CodecDecodeAll As Boolean
    Public TrackOverlay As UInt64()
    Public Translate() As TranslateInfo
    Public Video As VideoSettings
    Public Audio As AudioSettings
    Public TrackOperation As Operation
    Public TrickTrackUID As UInt64
    Public TrickTrackSegmentUID As Byte()
    Public TrickTrackFlag As UInt64
    Public TrickMasterTrackUID As UInt64
    Public TrickMasterTrackSegmentUID As Byte()
    Public ContentEncodings As Encodings
  End Structure
  Public Structure ChapterTrack
    Public ChapterTrackNumber() As UInt64
  End Structure
  Public Structure ChapterDisplay
    Public Title As String
    Public Language() As String
    Public Country() As String
  End Structure
  Public Structure ChapterProcessCommand
    Public Time As UInt64
    Public Data As Byte()
  End Structure
  Public Structure ChapterProcess
    Public CodecID As UInt64
    Public ProcPrivate As Byte()
    Public Command() As ChapterProcessCommand
  End Structure
  Public Structure ChapterAtom
    Public UID As UInt64
    Public TimeStart As UInt64
    Public TimeEnd As UInt64
    Public FlagHidden As Boolean
    Public FlagEnabled As Boolean
    Public SegmentUID As Byte()
    Public SegmentEditionUID As Byte()
    Public PhysicalEquiv As UInt64
    Public Track As ChapterTrack
    Public Display() As ChapterDisplay
    Public SubAtoms() As ChapterAtom
    Public Process() As ChapterProcess
  End Structure
  Public Structure EditionEntry
    Public UID As UInt64
    Public FlagHidden As Boolean
    Public FlagDefault As Boolean
    Public FlagOrdered As Boolean
    Public Atoms() As ChapterAtom
  End Structure
  Public Structure ChapterList
    Public Editions() As EditionEntry
  End Structure
  Public EBMLHead As EBMLHeader
  Public SeekHead As SeekEntry
  Public SegmentInfo As SegmentInformation
  Public TrackEntries() As TrackInformation
  Public ChapterInfo As ChapterList
  Private lSegmentsBegin As ULong
  Event Failure(Message As String)
  Private validMKV As Boolean
  Public ReadOnly Property HasMKV As Boolean
    Get
      Return validMKV
    End Get
  End Property
  Public Sub New(Path As String)
    validMKV = False
    Dim bReader As New MKVBinaryReader(Path)
    If Not ReadEBMLHeader(bReader) Then
      RaiseEvent Failure("No MKV Header Found!")
      Exit Sub
    End If
    Do
      Select Case ReadSegment(bReader)
        Case TriState.True
          'OK
        Case TriState.False
          RaiseEvent Failure("Segment parse failure!")
          Exit Sub
        Case TriState.UseDefault
          'Supposed to be a failure, but fuck it for now
          Continue Do
          'Exit Do
      End Select
      If SeekHead.Contents IsNot Nothing Then Exit Do
    Loop Until bReader.EndOfStream
    If SeekHead.Contents Is Nothing Then
      RaiseEvent Failure("No seek data found!")
      Exit Sub
    End If
    'Gather all seek data, now!
    Dim Contents() As SeekContents
    Contents = (From entry As SeekContents In SeekHead.Contents Where entry.SeekID.SequenceEqual({&H11, &H4D, &H9B, &H74})).ToArray
    For Each SeekItem In Contents
      bReader.BaseStream.Position = SeekItem.SeekPosition + lSegmentsBegin
      ReadSegment(bReader)
      Exit For
    Next
    'Next, all the Segments
    Contents = (From entry As SeekContents In SeekHead.Contents Where entry.SeekID.SequenceEqual({&H15, &H49, &HA9, &H66})).ToArray
    For Each SegmentItem In Contents
      bReader.BaseStream.Position = SegmentItem.SeekPosition + lSegmentsBegin
      ReadSegment(bReader)
      Exit For
    Next
    'Next, tracks
    Contents = (From entry As SeekContents In SeekHead.Contents Where entry.SeekID.SequenceEqual({&H16, &H54, &HAE, &H6B})).ToArray
    For Each TrackItem In Contents
      bReader.BaseStream.Position = TrackItem.SeekPosition + lSegmentsBegin
      ReadSegment(bReader)
      Exit For
    Next
    'Next, cues
    'Contents = (From entry As SeekContents In SeekHead.Contents Where entry.SeekID.SequenceEqual({&H1C, &H53, &HBB, &H6B})).ToArray
    'For Each CueItem In Contents
    '  bReader.BaseStream.Position = CueItem.SeekPosition + lSegmentsBegin
    '  ReadSegment(bReader)
    '  Exit For
    'Next
    'Last, chapters
    Contents = (From entry As SeekContents In SeekHead.Contents Where entry.SeekID.SequenceEqual({&H10, &H43, &HA7, &H70})).ToArray
    For Each ChapterItem In Contents
      bReader.BaseStream.Position = ChapterItem.SeekPosition + lSegmentsBegin
      ReadSegment(bReader)
      Exit For
    Next
    validMKV = True
    bReader.Close()
  End Sub

  Public Sub DebugData()
    Debug.Print("EBML:")
    If EBMLHead.Version > 0 Then Debug.Print("  Version: " & EBMLHead.Version)
    If EBMLHead.ReadVersion > 0 Then Debug.Print("  Read Version: " & EBMLHead.ReadVersion)
    If EBMLHead.MaxIDLength > 0 Then Debug.Print("  Max ID Length: " & EBMLHead.MaxIDLength)
    If EBMLHead.MaxSizeLength > 0 Then Debug.Print("  Max Size Length: " & EBMLHead.MaxSizeLength)
    If Not String.IsNullOrEmpty(EBMLHead.DocType) Then Debug.Print("  Doc Type: " & EBMLHead.DocType)
    If EBMLHead.DocTypeVersion > 0 Then Debug.Print("  Doc Type Version: " & EBMLHead.DocTypeVersion)
    If EBMLHead.DocTypeReadVersion > 0 Then Debug.Print("  Doc Type Read Version: " & EBMLHead.DocTypeReadVersion)

    Debug.Print("Seek:")
    For I As Integer = 0 To SeekHead.Contents.Length - 1
      If Not SeekHead.Contents(I).SeekID.SequenceEqual({&H1F, &H43, &HB6, &H75}) Then
        Debug.Print("  Seek Data " & I & ":")
        Debug.Print("    ID: " & BitConverter.ToString(SeekHead.Contents(I).SeekID))
        Debug.Print("    Position:" & SeekHead.Contents(I).SeekPosition)
      End If
    Next

    Debug.Print("Segment:")
    If SegmentInfo.SegmentUID IsNot Nothing Then Debug.Print("  Segment UID: " & BitConverter.ToString(SegmentInfo.SegmentUID))
    If Not String.IsNullOrEmpty(SegmentInfo.SegmentFilename) Then Debug.Print("  Segment Filename: " & SegmentInfo.SegmentFilename)
    If SegmentInfo.PrevUID IsNot Nothing Then Debug.Print("  Prev UID: " & BitConverter.ToString(SegmentInfo.PrevUID))
    If Not String.IsNullOrEmpty(SegmentInfo.PrevFilename) Then Debug.Print("  Prev Filename: " & SegmentInfo.PrevFilename)
    If SegmentInfo.NextUID IsNot Nothing Then Debug.Print("  Next UID: " & BitConverter.ToString(SegmentInfo.NextUID))
    If Not String.IsNullOrEmpty(SegmentInfo.NextFilename) Then Debug.Print("  Next Filename: " & SegmentInfo.NextFilename)
    If SegmentInfo.SegmentFamily IsNot Nothing Then
      For Each Family As Byte() In SegmentInfo.SegmentFamily
        Debug.Print("  Segment Family: " & BitConverter.ToString(Family))
      Next
    End If
    If SegmentInfo.ChapterTranslate IsNot Nothing Then
      For Each Translate As TranslateInfo In SegmentInfo.ChapterTranslate
        Debug.Print("  Chapter Translate:")
        If Translate.EditionUID IsNot Nothing Then
          Dim aUIDs(Translate.EditionUID.Length - 1) As String
          Array.Copy(Translate.EditionUID, aUIDs, Translate.EditionUID.Length)
          Debug.Print("    Edition UID: " & Join(aUIDs, ", "))
        End If
        If Translate.Codec > 0 Then Debug.Print("    Codec: " & Translate.Codec)
        If Translate.TrackID IsNot Nothing Then Debug.Print("    ID: " & BitConverter.ToString(Translate.TrackID))
      Next
    End If
    If SegmentInfo.TimecodeScale > 0 Then Debug.Print("  Timecode Scale: " & SegmentInfo.TimecodeScale)
    If SegmentInfo.Duration <> 0 Then Debug.Print("  Duration: " & SegmentInfo.Duration)
    If SegmentInfo.DateUTC.CompareTo(New Date(2001, 1, 1)) <> 0 Then Debug.Print("  Date (UTC): " & SegmentInfo.DateUTC.ToString)
    If Not String.IsNullOrEmpty(SegmentInfo.Title) Then Debug.Print("  Title: " & SegmentInfo.Title)
    If Not String.IsNullOrEmpty(SegmentInfo.MuxingApp) Then Debug.Print("  Muxing App: " & SegmentInfo.MuxingApp)
    If Not String.IsNullOrEmpty(SegmentInfo.WritingApp) Then Debug.Print("  Writing App: " & SegmentInfo.WritingApp)

    Debug.Print("Entries: ")
    For I As Integer = 0 To TrackEntries.Length - 1
      Debug.Print("  Entry " & I & ":")
      If TrackEntries(I).TrackNumber > 0 Then Debug.Print("    Track Number: " & TrackEntries(I).TrackNumber)
      If TrackEntries(I).TrackUID > 0 Then Debug.Print("    Track UID: " & TrackEntries(I).TrackUID)
      If TrackEntries(I).TrackType > 0 Then Debug.Print("    Track Type: " & TrackEntries(I).TrackType)
      Debug.Print("    Enabled: " & TrackEntries(I).FlagEnabled)
      Debug.Print("    Default: " & TrackEntries(I).FlagDefault)
      Debug.Print("    Forced: " & TrackEntries(I).FlagForced)
      Debug.Print("    Lacing: " & TrackEntries(I).FlagLacing)
      If TrackEntries(I).MinCache > 0 Then Debug.Print("    Minimum Cache: " & TrackEntries(I).MinCache)
      If TrackEntries(I).MaxCache > 0 Then Debug.Print("    Maximum Cache: " & TrackEntries(I).MaxCache)
      If TrackEntries(I).DefaultDuration > 0 Then Debug.Print("    Default Duration: " & TrackEntries(I).DefaultDuration)
      If TrackEntries(I).TrackTimecodeScale > 0 Then Debug.Print("    Track Timecode Scale: " & TrackEntries(I).TrackTimecodeScale)
      If TrackEntries(I).TrackOffset > 0 Then Debug.Print("    Track Offset: " & TrackEntries(I).TrackOffset)
      If TrackEntries(I).MaxBlockAdditionID > 0 Then Debug.Print("    Maximum BlockAddID: " & TrackEntries(I).MaxBlockAdditionID)
      If Not String.IsNullOrEmpty(TrackEntries(I).TrackName) Then Debug.Print("    Name: " & TrackEntries(I).TrackName)
      If Not String.IsNullOrEmpty(TrackEntries(I).Language) Then Debug.Print("    Language: " & TrackEntries(I).Language)
      If Not String.IsNullOrEmpty(TrackEntries(I).CodecID) Then Debug.Print("    Codec ID: " & TrackEntries(I).CodecID)
      If TrackEntries(I).CodecPrivate IsNot Nothing Then Debug.Print("    Codec Private: " & BitConverter.ToString(TrackEntries(I).CodecPrivate))
      If Not String.IsNullOrEmpty(TrackEntries(I).CodecName) Then Debug.Print("    Codec Name: " & TrackEntries(I).CodecName)
      If TrackEntries(I).AttachmentLink > 0 Then Debug.Print("    Attachment Link: " & TrackEntries(I).AttachmentLink)
      If Not String.IsNullOrEmpty(TrackEntries(I).CodecSettings) Then Debug.Print("    Codec Settings: " & TrackEntries(I).CodecSettings)
      If Not String.IsNullOrEmpty(TrackEntries(I).CodecInfoURL) Then Debug.Print("    Codec Info URL: " & TrackEntries(I).CodecInfoURL)
      If Not String.IsNullOrEmpty(TrackEntries(I).CodecDownloadURL) Then Debug.Print("    Codec Download URL: " & TrackEntries(I).CodecDownloadURL)
      Debug.Print("    Codec Decode All: " & TrackEntries(I).CodecDecodeAll)
      If TrackEntries(I).TrackOverlay IsNot Nothing Then
        For Each Overlay As UInt64 In TrackEntries(I).TrackOverlay
          Debug.Print("    Track Overlay: " & Overlay)
        Next
      End If
      If TrackEntries(I).Translate IsNot Nothing Then
        For Each Translate As TranslateInfo In TrackEntries(I).Translate
          Debug.Print("    Track Translate:")
          If Translate.EditionUID IsNot Nothing Then
            Dim aUIDs(Translate.EditionUID.Length - 1) As String
            Array.Copy(Translate.EditionUID, aUIDs, Translate.EditionUID.Length)
            Debug.Print("      Edition UID: " & Join(aUIDs, ", "))
          End If
          If Translate.Codec > 0 Then Debug.Print("      Codec: " & Translate.Codec)
          If Translate.TrackID IsNot Nothing Then Debug.Print("      Track ID: " & BitConverter.ToString(Translate.TrackID))
        Next
      End If
      If TrackEntries(I).Video.Exists Then
        Debug.Print("    Video:")
        Debug.Print("      Interlaced: " & TrackEntries(I).Video.FlagInterlaced)
        Debug.Print("      Stereo-3D Mode: " & TrackEntries(I).Video.StereoMode)
        Debug.Print("      Old StereMode: " & TrackEntries(I).Video.OldStereoMode)
        Debug.Print("      Pixel Resolution: " & TrackEntries(I).Video.PixelWidth & "x" & TrackEntries(I).Video.PixelHeight)
        If TrackEntries(I).Video.PixelCropTop > 0 Then Debug.Print("      Pixel Crop Top: " & TrackEntries(I).Video.PixelCropTop)
        If TrackEntries(I).Video.PixelCropBottom > 0 Then Debug.Print("      Pixel Crop Bottom: " & TrackEntries(I).Video.PixelCropBottom)
        If TrackEntries(I).Video.PixelCropLeft > 0 Then Debug.Print("      Pixel Crop Left: " & TrackEntries(I).Video.PixelCropLeft)
        If TrackEntries(I).Video.PixelCropRight > 0 Then Debug.Print("      Pixel Crop Right: " & TrackEntries(I).Video.PixelCropRight)
        Debug.Print("      Display Resolution: " & TrackEntries(I).Video.DisplayWidth & "x" & TrackEntries(I).Video.DisplayHeight)
        Debug.Print("      Aspect Ratio Type: " & TrackEntries(I).Video.AspectRatioType)
        If TrackEntries(I).Video.ColorSpace IsNot Nothing Then Debug.Print("      Color Space: " & BitConverter.ToString(TrackEntries(I).Video.ColorSpace))
        If TrackEntries(I).Video.GammaValue <> 0 Then Debug.Print("      Gamma Value: " & TrackEntries(I).Video.GammaValue)
        If TrackEntries(I).Video.FrameRate <> 0 Then Debug.Print("      Frame Rate: " & TrackEntries(I).Video.FrameRate)
      End If
      If TrackEntries(I).Audio.Exists Then
        Debug.Print("    Audio:")
        Debug.Print("      Sampling Frequency: " & TrackEntries(I).Audio.SamplingFrequency / 1000 & " kHz")
        If TrackEntries(I).Audio.OutputSamplingFrequency > 0 Then Debug.Print("      Output Sampling Frequency: " & TrackEntries(I).Audio.OutputSamplingFrequency / 1000 & " kHz")
        If TrackEntries(I).Audio.Channels > 0 Then Debug.Print("      Channels: " & TrackEntries(I).Audio.Channels)
        If TrackEntries(I).Audio.ChannelPositions IsNot Nothing Then Debug.Print("      Channel Positions: " & BitConverter.ToString(TrackEntries(I).Audio.ChannelPositions))
        If TrackEntries(I).Audio.BitDepth > 0 Then Debug.Print("      Bit Depth: " & TrackEntries(I).Audio.BitDepth)
      End If
      If TrackEntries(I).TrackOperation.Exists Then
        Debug.Print("    Track Operation:")
        If TrackEntries(I).TrackOperation.TrackCombinePlanes.TrackPlane IsNot Nothing Then
          Debug.Print("      Combine Planes:")
          For J As Integer = 0 To TrackEntries(I).TrackOperation.TrackCombinePlanes.TrackPlane.Length - 1
            Debug.Print("        Track Plane " & J & ":")
            Debug.Print("          UID: " & TrackEntries(I).TrackOperation.TrackCombinePlanes.TrackPlane(J).UID)
            Debug.Print("          Type: " & TrackEntries(I).TrackOperation.TrackCombinePlanes.TrackPlane(J).PlaneType)
          Next
        End If
        If TrackEntries(I).TrackOperation.TrackJoinBlocks IsNot Nothing Then
          Debug.Print("      Join Blocks:")
          For J As Integer = 0 To TrackEntries(I).TrackOperation.TrackJoinBlocks.Length - 1
            Debug.Print("        Block " & J & " UID: " & TrackEntries(I).TrackOperation.TrackJoinBlocks(J).UID)
          Next
        End If
      End If
      If TrackEntries(I).TrickTrackUID > 0 Then Debug.Print("      Trick Track UID: " & TrackEntries(I).TrickTrackUID)
      If TrackEntries(I).TrickTrackSegmentUID IsNot Nothing Then Debug.Print("      Trick Track Segment UID: " & BitConverter.ToString(TrackEntries(I).TrickMasterTrackSegmentUID))
      If TrackEntries(I).TrickTrackFlag > 0 Then Debug.Print("      Trick Track Flag: " & TrackEntries(I).TrickTrackFlag)
      If TrackEntries(I).TrickMasterTrackUID > 0 Then Debug.Print("      Trick Master Track UID: " & TrackEntries(I).TrickMasterTrackUID)
      If TrackEntries(I).TrickMasterTrackSegmentUID IsNot Nothing Then Debug.Print("      Trick Master Segment Track UID: " & BitConverter.ToString(TrackEntries(I).TrickMasterTrackSegmentUID))
      If TrackEntries(I).ContentEncodings.ContentEncoding IsNot Nothing Then
        Debug.Print("      Content Encodings:")
        For J As Integer = 0 To TrackEntries(I).ContentEncodings.ContentEncoding.Length - 1
          Debug.Print("        Content Encoding " & J & ":")
          Debug.Print("          Order: " & TrackEntries(I).ContentEncodings.ContentEncoding(J).Order)
          Debug.Print("          Scope: " & TrackEntries(I).ContentEncodings.ContentEncoding(J).Scope)
          Debug.Print("          Type: " & TrackEntries(I).ContentEncodings.ContentEncoding(J).EncType)
          Debug.Print("          Compression:")
          Debug.Print("            Algorithm: " & TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentCompression.Algorithm)
          Debug.Print("            Settings: " & BitConverter.ToString(TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentCompression.Settings))
          Debug.Print("          Encryption:")
          Debug.Print("            Algorithm: " & TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.Algorithm)
          Debug.Print("            Key ID: " & BitConverter.ToString(TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.KeyID))
          Debug.Print("            Signature: " & BitConverter.ToString(TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.Signature))
          Debug.Print("            Signature Key ID: " & BitConverter.ToString(TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.SigKeyID))
          Debug.Print("            Signature Algorithm: " & TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.SigAlgorithm)
          Debug.Print("            Signature Hash Algorithm: " & TrackEntries(I).ContentEncodings.ContentEncoding(J).ContentEncryption.SigHashAlgorithm)
        Next
      End If
    Next

    If ChapterInfo.Editions IsNot Nothing Then
      Debug.Print("Chapters:")
      For I As Integer = 0 To ChapterInfo.Editions.Length - 1
        Debug.Print("  Edition " & I & ":")
        Debug.Print("    UID: " & ChapterInfo.Editions(I).UID)
        Debug.Print("    Hidden: " & ChapterInfo.Editions(I).FlagHidden)
        Debug.Print("    Default: " & ChapterInfo.Editions(I).FlagDefault)
        Debug.Print("    Ordered: " & ChapterInfo.Editions(I).FlagOrdered)
        Debug.Print("    Atoms:")
        For J As Integer = 0 To ChapterInfo.Editions(I).Atoms.Length - 1
          Debug.Print("      Atom " & J & ":")
          Debug.Print("        UID: " & ChapterInfo.Editions(I).Atoms(J).UID)
          Debug.Print("        Start Time: " & ChapterInfo.Editions(I).Atoms(J).TimeStart)
          Debug.Print("        End Time: " & ChapterInfo.Editions(I).Atoms(J).TimeEnd)
          Debug.Print("        Hidden: " & ChapterInfo.Editions(I).Atoms(J).FlagHidden)
          Debug.Print("        Enabled: " & ChapterInfo.Editions(I).Atoms(J).FlagEnabled)
          If ChapterInfo.Editions(I).Atoms(J).SegmentUID IsNot Nothing Then Debug.Print("        Segment UID: " & BitConverter.ToString(ChapterInfo.Editions(I).Atoms(J).SegmentUID))
          If ChapterInfo.Editions(I).Atoms(J).SegmentEditionUID IsNot Nothing Then Debug.Print("        Edition UID: " & BitConverter.ToString(ChapterInfo.Editions(I).Atoms(J).SegmentEditionUID))
          Debug.Print("        Physical Equiv: " & ChapterInfo.Editions(I).Atoms(J).PhysicalEquiv)
          Debug.Print("        Display:")
          For K As Integer = 0 To ChapterInfo.Editions(I).Atoms(J).Display.Length - 1
            Debug.Print("          Title: " & ChapterInfo.Editions(I).Atoms(J).Display(K).Title)
            Debug.Print("          Language: " & Join(ChapterInfo.Editions(I).Atoms(J).Display(K).Language, ", "))
            Debug.Print("          Country: " & Join(ChapterInfo.Editions(I).Atoms(J).Display(K).Country, " ,"))
          Next
        Next
      Next
    End If
  End Sub

  Private Function ReadEBMLHeader(ByRef bReader As MKVBinaryReader) As Boolean
    Dim HeaderID As Byte()
    Do
      If bReader.EndOfStream Then Return False
      HeaderID = bReader.ReadBytes(4)
      bReader.BaseStream.Position -= 3
      If bReader.BaseStream.Position > 1 Then Return False
    Loop Until HeaderID.SequenceEqual({&H1A, &H45, &HDF, &HA3})
    bReader.BaseStream.Position += 3
    Dim rHead As New MKVBinaryReader(bReader)
    Do
      Dim ElID As Integer = rHead.GetElementID
      Select Case ElID
        Case &H4286 : EBMLHead.Version = rHead.GetValue
        Case &H42F7 : EBMLHead.ReadVersion = rHead.GetValue
        Case &H42F2 : EBMLHead.MaxIDLength = rHead.GetValue
        Case &H42F3 : EBMLHead.MaxSizeLength = rHead.GetValue
        Case &H4282 : EBMLHead.DocType = rHead.GetASCIIString
        Case &H4287 : EBMLHead.DocTypeVersion = rHead.GetValue
        Case &H4285 : EBMLHead.DocTypeReadVersion = rHead.GetValue
        Case &HEC
          Dim bVoid() As Byte = rHead.GetBytes
          'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
        Case Else : Debug.Print("Unrecognized Header Element ID: " & Hex(ElID) & ", Value: " & BitConverter.ToString(rHead.GetBytes))
      End Select
    Loop Until rHead.EndOfStream
    rHead.Close()
    Return True
  End Function

  Private Function ReadSegment(ByRef bReader As MKVBinaryReader) As Boolean
    Dim ElID As UInt64 = bReader.GetElementID
    Select Case ElID
      Case &H18538067
        Dim uSeg As UInt64 = bReader.CalcLengthVal(bReader)
        lSegmentsBegin = bReader.BaseStream.Position
        Return TriState.True
      Case &H114D9B74 : Return ReadSeekHeader(bReader)
      Case &H1549A966 : Return ReadSegmentInfo(bReader)
      Case &H1654AE6B : Return ReadTrackInfo(bReader)
      Case &H1043A770 : Return ReadChapterInfo(bReader)
      Case &H1F43B675 'Chunk
        Dim lLen As Long = bReader.CalcLengthVal(bReader)
        bReader.BaseStream.Position += lLen
        Return TriState.UseDefault
      Case &H1C53BB6B 'Cue
        Dim lLen As Long = bReader.CalcLengthVal(bReader)
        bReader.BaseStream.Position += lLen
        Return True
      Case &H1941A469 'Attachment
        Dim lLen As Long = bReader.CalcLengthVal(bReader)
        bReader.BaseStream.Position += lLen
        Return True
      Case &HEC
        Dim bVoid() As Byte = bReader.GetBytes
        Return bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length))
      Case Else
        Dim bSegData() As Byte = bReader.GetBytes
        Debug.Print("Unrecognized Segment ID: " & Hex(ElID) & ", Length: " & bSegData.Length)
        Return TriState.True
    End Select
  End Function

  Private Function ReadSeekHeader(ByRef bReader As MKVBinaryReader) As Boolean
    Using rSeek As New MKVBinaryReader(bReader)
      Dim SeekI As Integer = 0
      If SeekHead.Contents IsNot Nothing Then SeekI = SeekHead.Contents.Length
      Do
        Dim seID As UInt64 = rSeek.GetElementID
        If seID = &H4DBB Then
          ReDim Preserve SeekHead.Contents(SeekI)
          Using rSeekField As New MKVBinaryReader(rSeek)
            Do
              Dim elID As UInt64 = rSeekField.GetElementID
              Select Case elID
                Case &H53AB : SeekHead.Contents(SeekI).SeekID = rSeekField.GetBytes
                Case &H53AC : SeekHead.Contents(SeekI).SeekPosition = rSeekField.GetValue
                Case &HEC
                  Dim bVoid() As Byte = rSeekField.GetBytes
                  'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                Case Else : Debug.Print("Unrecognized Seek Field: " & Hex(elID) & ", Value: " & BitConverter.ToString(rSeek.GetBytes))
              End Select
            Loop Until rSeekField.EndOfStream
            rSeekField.Close()
          End Using
          SeekI += 1
        ElseIf seID = &HEC Then
          Dim bVoid() As Byte = rSeek.GetBytes
          'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
        Else
          Return False
        End If
      Loop Until rSeek.EndOfStream
      rSeek.Close()
    End Using
    Return True
    ' Else
    ' Return False
    ' End If
  End Function

  Private Function ReadSegmentInfo(ByRef bReader As MKVBinaryReader) As Boolean
    Using rInfo As New MKVBinaryReader(bReader)
      Do
        Dim elID As UInt64 = rInfo.GetElementID
        Select Case elID
          Case &H73A4 : SegmentInfo.SegmentUID = rInfo.GetBytes
          Case &H7384 : SegmentInfo.SegmentFilename = rInfo.GetUTF8String
          Case &H3CB923 : SegmentInfo.PrevUID = rInfo.GetBytes
          Case &H3C83AB : SegmentInfo.PrevFilename = rInfo.GetUTF8String
          Case &H3EB923 : SegmentInfo.NextUID = rInfo.GetBytes
          Case &H3E83BB : SegmentInfo.NextFilename = rInfo.GetUTF8String
          Case &H4444
            If SegmentInfo.SegmentFamily Is Nothing Then
              ReDim SegmentInfo.SegmentFamily(0)
            Else
              ReDim Preserve SegmentInfo.SegmentFamily(SegmentInfo.SegmentFamily.Length)
            End If
            SegmentInfo.SegmentFamily(SegmentInfo.SegmentFamily.Length - 1) = rInfo.GetBytes
          Case &H6924
            If SegmentInfo.ChapterTranslate Is Nothing Then
              ReDim SegmentInfo.ChapterTranslate(0)
            Else
              ReDim Preserve SegmentInfo.ChapterTranslate(SegmentInfo.ChapterTranslate.Length)
            End If
            Using rChapterTranslate As New MKVBinaryReader(rInfo)
              Dim TransI As Integer = SegmentInfo.ChapterTranslate.Length - 1
              Do
                Dim chID As UInt64 = rChapterTranslate.GetElementID
                Select Case chID
                  Case &H69FC
                    If SegmentInfo.ChapterTranslate(TransI).EditionUID Is Nothing Then
                      ReDim SegmentInfo.ChapterTranslate(TransI).EditionUID(0)
                    Else
                      ReDim Preserve SegmentInfo.ChapterTranslate(TransI).EditionUID(SegmentInfo.ChapterTranslate(TransI).EditionUID.Length)
                    End If
                    SegmentInfo.ChapterTranslate(TransI).EditionUID(SegmentInfo.ChapterTranslate(TransI).EditionUID.Length - 1) = rChapterTranslate.GetValue
                  Case &H69BF : SegmentInfo.ChapterTranslate(TransI).Codec = rChapterTranslate.GetValue
                  Case &H69A5 : SegmentInfo.ChapterTranslate(TransI).TrackID = rChapterTranslate.GetBytes
                  Case &HEC
                    Dim bVoid() As Byte = rChapterTranslate.GetBytes
                    'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                  Case Else : Debug.Print("Unrecognized Chapter Translate Field: " & Hex(chID) & ", Value: " & BitConverter.ToString(rChapterTranslate.GetBytes))
                End Select
              Loop Until rChapterTranslate.EndOfStream
              rChapterTranslate.Close()
            End Using
          Case &H2AD7B1 : SegmentInfo.TimecodeScale = rInfo.GetValue
          Case &H4489 : SegmentInfo.Duration = rInfo.GetFloat
          Case &H4461 : SegmentInfo.DateUTC = New Date(2001, 1, 1).AddMilliseconds(rInfo.GetValue \ 1000000UL)
          Case &H7BA9 : SegmentInfo.Title = rInfo.GetUTF8String
          Case &H4D80 : SegmentInfo.MuxingApp = rInfo.GetUTF8String
          Case &H5741 : SegmentInfo.WritingApp = rInfo.GetUTF8String
          Case &HEC
            Dim bVoid() As Byte = rInfo.GetBytes
            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
          Case Else : Debug.Print("Unrecognized Segment Field: " & Hex(elID) & ", Value: " & BitConverter.ToString(rInfo.GetBytes))
        End Select
      Loop Until rInfo.EndOfStream
      rInfo.Close()
    End Using
    Return True
  End Function

  Private Function ReadTrackInfo(ByRef bReader As MKVBinaryReader) As Boolean
    Using rInfo As New MKVBinaryReader(bReader)
      Dim TrackI As Integer = 0
      If TrackEntries IsNot Nothing Then TrackI = TrackEntries.Length
      Do
        Dim TrackEntryID As UInt64 = rInfo.GetElementID
        Select Case TrackEntryID
          Case &HAE
            ReDim Preserve TrackEntries(TrackI)
            TrackEntries(TrackI).FlagEnabled = True
            TrackEntries(TrackI).FlagDefault = True
            TrackEntries(TrackI).FlagForced = False
            TrackEntries(TrackI).FlagLacing = True
            TrackEntries(TrackI).TrackTimecodeScale = 1
            TrackEntries(TrackI).Language = "eng"
            TrackEntries(TrackI).CodecDecodeAll = True
            TrackEntries(TrackI).Audio.SamplingFrequency = 8000
            Using rEntry As New MKVBinaryReader(rInfo)
              Do
                Dim elID As UInt64 = rEntry.GetElementID
                Select Case elID
                  Case &HD7 : TrackEntries(TrackI).TrackNumber = rEntry.GetValue
                  Case &H73C5 : TrackEntries(TrackI).TrackUID = rEntry.GetValue
                  Case &H83 : TrackEntries(TrackI).TrackType = rEntry.GetValue
                  Case &HB9 : TrackEntries(TrackI).FlagEnabled = (rEntry.GetValue = 1)
                  Case &H88 : TrackEntries(TrackI).FlagDefault = (rEntry.GetValue = 1)
                  Case &H55AA : TrackEntries(TrackI).FlagForced = (rEntry.GetValue = 1)
                  Case &H9C : TrackEntries(TrackI).FlagLacing = (rEntry.GetValue = 1)
                  Case &H6DE7 : TrackEntries(TrackI).MinCache = rEntry.GetValue
                  Case &H6DF8 : TrackEntries(TrackI).MaxCache = rEntry.GetValue
                  Case &H23E383 : TrackEntries(TrackI).DefaultDuration = rEntry.GetValue
                  Case &H23314F : TrackEntries(TrackI).TrackTimecodeScale = rEntry.GetFloat
                  Case &H537F : TrackEntries(TrackI).TrackOffset = rEntry.GetSValue
                  Case &H55EE : TrackEntries(TrackI).MaxBlockAdditionID = rEntry.GetValue
                  Case &H536E : TrackEntries(TrackI).TrackName = rEntry.GetUTF8String
                  Case &H22B59C : TrackEntries(TrackI).Language = rEntry.GetASCIIString
                  Case &H86 : TrackEntries(TrackI).CodecID = rEntry.GetASCIIString
                  Case &H63A2 : TrackEntries(TrackI).CodecPrivate = rEntry.GetBytes
                  Case &H258688 : TrackEntries(TrackI).CodecName = rEntry.GetUTF8String
                  Case &H7446 : TrackEntries(TrackI).AttachmentLink = rEntry.GetValue
                  Case &H3A9697 : TrackEntries(TrackI).CodecSettings = rEntry.GetUTF8String
                  Case &H3B4040 : TrackEntries(TrackI).CodecInfoURL = rEntry.GetASCIIString
                  Case &H26B240 : TrackEntries(TrackI).CodecDownloadURL = rEntry.GetASCIIString
                  Case &HAA : TrackEntries(TrackI).CodecDecodeAll = (rEntry.GetValue = 1)
                  Case &H6FAB
                    If TrackEntries(TrackI).TrackOverlay Is Nothing Then
                      ReDim TrackEntries(TrackI).TrackOverlay(0)
                    Else
                      ReDim Preserve TrackEntries(TrackI).TrackOverlay(TrackEntries(TrackI).TrackOverlay.Length)
                    End If
                    TrackEntries(TrackI).TrackOverlay(TrackEntries(TrackI).TrackOverlay.Length - 1) = rEntry.GetValue
                  Case &H6624
                    If TrackEntries(TrackI).Translate Is Nothing Then
                      ReDim TrackEntries(TrackI).Translate(0)
                    Else
                      ReDim Preserve TrackEntries(TrackI).Translate(TrackEntries(TrackI).Translate.Length)
                    End If
                    Using rTranslate As New MKVBinaryReader(rEntry)
                      Dim transI As Integer = TrackEntries(TrackI).Translate.Length - 1
                      Do
                        Dim transID As UInt64 = rTranslate.GetElementID
                        Select Case transID
                          Case &H66FC
                            If TrackEntries(TrackI).Translate(transI).EditionUID Is Nothing Then
                              ReDim TrackEntries(TrackI).Translate(transI).EditionUID(0)
                            Else
                              ReDim Preserve TrackEntries(TrackI).Translate(transI).EditionUID(TrackEntries(TrackI).Translate(transI).EditionUID.Length)
                            End If
                            TrackEntries(TrackI).Translate(transI).EditionUID(TrackEntries(TrackI).Translate(transI).EditionUID.Length - 1) = rTranslate.GetValue
                          Case &H66BF : TrackEntries(TrackI).Translate(transI).Codec = rTranslate.GetValue
                          Case &H66A5 : TrackEntries(TrackI).Translate(transI).TrackID = rTranslate.GetBytes
                          Case &HEC
                            Dim bVoid() As Byte = rTranslate.GetBytes
                            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                          Case Else : Debug.Print("Unrecognized Track Translate Field: " & Hex(transID) & ", Value: " & BitConverter.ToString(rTranslate.GetBytes))
                        End Select
                      Loop Until rTranslate.EndOfStream
                      rTranslate.Close()
                    End Using
                  Case &HE0
                    Using rVideo As New MKVBinaryReader(rEntry)
                      TrackEntries(TrackI).Video.Exists = True
                      TrackEntries(TrackI).Video.DisplayUnit = 255
                      Do
                        Dim vidID As UInt64 = rVideo.GetElementID
                        Select Case vidID
                          Case &H9A : TrackEntries(TrackI).Video.FlagInterlaced = rVideo.GetValue
                          Case &H53B8 : TrackEntries(TrackI).Video.StereoMode = rVideo.GetValue
                          Case &H53B9 : TrackEntries(TrackI).Video.OldStereoMode = rVideo.GetValue
                          Case &HB0 : TrackEntries(TrackI).Video.PixelWidth = rVideo.GetValue
                          Case &HBA : TrackEntries(TrackI).Video.PixelHeight = rVideo.GetValue
                          Case &H54AA : TrackEntries(TrackI).Video.PixelCropBottom = rVideo.GetValue
                          Case &H54BB : TrackEntries(TrackI).Video.PixelCropTop = rVideo.GetValue
                          Case &H54CC : TrackEntries(TrackI).Video.PixelCropLeft = rVideo.GetValue
                          Case &H54DD : TrackEntries(TrackI).Video.PixelCropRight = rVideo.GetValue
                          Case &H54B0 : TrackEntries(TrackI).Video.DisplayWidth = rVideo.GetValue
                          Case &H54BA : TrackEntries(TrackI).Video.DisplayHeight = rVideo.GetValue
                          Case &H54B2 : TrackEntries(TrackI).Video.DisplayUnit = rVideo.GetValue
                          Case &H54B3 : TrackEntries(TrackI).Video.AspectRatioType = rVideo.GetValue
                          Case &H2EB524 : TrackEntries(TrackI).Video.ColorSpace = rVideo.GetBytes
                          Case &H2FB523 : TrackEntries(TrackI).Video.GammaValue = rVideo.GetFloat
                          Case &H2383E3 : TrackEntries(TrackI).Video.FrameRate = rVideo.GetFloat
                          Case &HEC
                            Dim bVoid() As Byte = rVideo.GetBytes
                            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                          Case Else : Debug.Print("Unrecognized Track Video Field: " & Hex(vidID) & ", Value: " & BitConverter.ToString(rVideo.GetBytes))
                        End Select
                      Loop Until rVideo.EndOfStream
                      rVideo.Close()
                    End Using
                  Case &HE1
                    Using rAudio As New MKVBinaryReader(rEntry)
                      TrackEntries(TrackI).Audio.Exists = True
                      Do
                        Dim audID As UInt64 = rAudio.GetElementID
                        Select Case audID
                          Case &HB5 : TrackEntries(TrackI).Audio.SamplingFrequency = rAudio.GetFloat
                          Case &H78B5 : TrackEntries(TrackI).Audio.OutputSamplingFrequency = rAudio.GetFloat
                          Case &H9F : TrackEntries(TrackI).Audio.Channels = rAudio.GetValue
                          Case &H7D7B : TrackEntries(TrackI).Audio.ChannelPositions = rAudio.GetBytes
                          Case &H6264 : TrackEntries(TrackI).Audio.BitDepth = rAudio.GetValue
                          Case &HEC
                            Dim bVoid() As Byte = rAudio.GetBytes
                            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                          Case Else : Debug.Print("Unrecognized Track Audio Field: " & Hex(audID) & ", Value: " & BitConverter.ToString(rAudio.GetBytes))
                        End Select
                      Loop Until rAudio.EndOfStream
                      rAudio.Close()
                    End Using
                  Case &HE2
                    Using rOperation As New MKVBinaryReader(rEntry)
                      TrackEntries(TrackI).TrackOperation.Exists = True
                      Do
                        Dim opID As UInt64 = rOperation.GetElementID
                        Select Case opID
                          Case &HE3
                            Using rCombinePlanes As New MKVBinaryReader(rOperation)
                              Dim PlaneI As Integer = 0
                              If TrackEntries(TrackI).TrackOperation.TrackCombinePlanes.TrackPlane IsNot Nothing Then PlaneI = TrackEntries(TrackI).TrackOperation.TrackCombinePlanes.TrackPlane.Length
                              Do
                                Dim combPlaneID As UInt64 = rCombinePlanes.GetElementID
                                Select Case combPlaneID
                                  Case &HE4
                                    ReDim Preserve TrackEntries(TrackI).TrackOperation.TrackCombinePlanes.TrackPlane(PlaneI)
                                    Using rPlane As New MKVBinaryReader(rCombinePlanes)
                                      Do
                                        Dim planeID As UInt64 = rPlane.GetElementID
                                        Select Case planeID
                                          Case &HE5 : TrackEntries(TrackI).TrackOperation.TrackCombinePlanes.TrackPlane(PlaneI).UID = rPlane.GetValue
                                          Case &HE6 : TrackEntries(TrackI).TrackOperation.TrackCombinePlanes.TrackPlane(PlaneI).PlaneType = rPlane.GetValue
                                          Case &HEC
                                            Dim bVoid() As Byte = rPlane.GetBytes
                                            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                                          Case Else : Debug.Print("Unrecognized Plane Field: " & Hex(planeID) & ", Value: " & BitConverter.ToString(rPlane.GetBytes))
                                        End Select
                                      Loop Until rPlane.EndOfStream
                                      PlaneI += 1
                                      rPlane.Close()
                                    End Using
                                  Case &HEC
                                    Dim bVoid() As Byte = rCombinePlanes.GetBytes
                                    'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                                  Case Else : Debug.Print("Unrecognized Combine Plane Field: " & Hex(combPlaneID) & ", Value: " & BitConverter.ToString(rCombinePlanes.GetBytes))
                                End Select
                              Loop Until rCombinePlanes.EndOfStream
                              rCombinePlanes.Close()
                            End Using
                          Case &HE9
                            Using rJoinBlocks As New MKVBinaryReader(rOperation)
                              Dim BlockI As Integer = 0
                              If TrackEntries(TrackI).TrackOperation.TrackJoinBlocks IsNot Nothing Then BlockI = TrackEntries(TrackI).TrackOperation.TrackJoinBlocks.Length
                              Do
                                Dim joinBlockID As UInt64 = rJoinBlocks.GetElementID
                                Select Case joinBlockID
                                  Case &HED
                                    ReDim Preserve TrackEntries(TrackI).TrackOperation.TrackJoinBlocks(BlockI)
                                    TrackEntries(TrackI).TrackOperation.TrackJoinBlocks(BlockI).UID = rJoinBlocks.GetValue
                                    BlockI += 1
                                  Case &HEC
                                    Dim bVoid() As Byte = rJoinBlocks.GetBytes
                                    'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                                  Case Else : Debug.Print("Unrecognized Join Block Field: " & Hex(joinBlockID) & ", Value: " & BitConverter.ToString(rJoinBlocks.GetBytes))
                                End Select
                              Loop Until rJoinBlocks.EndOfStream
                              rJoinBlocks.Close()
                            End Using
                          Case &HEC
                            Dim bVoid() As Byte = rOperation.GetBytes
                            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                          Case Else : Debug.Print("Unrecognized Track Operation Field: " & Hex(opID) & ", Value: " & BitConverter.ToString(rOperation.GetBytes))
                        End Select
                      Loop Until rOperation.EndOfStream
                      rOperation.Close()
                    End Using
                  Case &HC0 : TrackEntries(TrackI).TrickTrackUID = rEntry.GetValue
                  Case &HC1 : TrackEntries(TrackI).TrickTrackSegmentUID = rEntry.GetBytes
                  Case &HC6 : TrackEntries(TrackI).TrickTrackFlag = rEntry.GetValue
                  Case &HC7 : TrackEntries(TrackI).TrickMasterTrackUID = rEntry.GetValue
                  Case &HC4 : TrackEntries(TrackI).TrickMasterTrackSegmentUID = rEntry.GetBytes
                  Case &H6D80
                    Using rEncodings As New MKVBinaryReader(rEntry)
                      Dim EncodingI As Integer = 0
                      If TrackEntries(TrackI).ContentEncodings.ContentEncoding IsNot Nothing Then EncodingI = TrackEntries(TrackI).ContentEncodings.ContentEncoding.Length
                      Do
                        Dim encodingsID As UInt64 = rEncodings.GetElementID
                        Select Case encodingsID
                          Case &H6240
                            ReDim Preserve TrackEntries(TrackI).ContentEncodings.ContentEncoding(EncodingI)
                            Using rEncoding As New MKVBinaryReader(rEncodings)
                              Do
                                Dim encodingID As UInt64 = rEncoding.GetElementID
                                Select Case encodingID
                                  Case &H5031 : TrackEntries(TrackI).ContentEncodings.ContentEncoding(EncodingI).Order = rEncoding.GetValue
                                  Case &H5032 : TrackEntries(TrackI).ContentEncodings.ContentEncoding(EncodingI).Scope = rEncoding.GetValue
                                  Case &H5033 : TrackEntries(TrackI).ContentEncodings.ContentEncoding(EncodingI).EncType = rEncoding.GetValue
                                  Case &H5034
                                    Using rCompression As New MKVBinaryReader(rEncoding)
                                      Do
                                        Dim compressionID As UInt64 = rCompression.GetElementID
                                        Select Case compressionID
                                          Case &H4254 : TrackEntries(TrackI).ContentEncodings.ContentEncoding(EncodingI).ContentCompression.Algorithm = rCompression.GetValue
                                          Case &H4255 : TrackEntries(TrackI).ContentEncodings.ContentEncoding(EncodingI).ContentCompression.Settings = rCompression.GetBytes
                                          Case &HEC
                                            Dim bVoid() As Byte = rCompression.GetBytes
                                            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                                          Case Else : Debug.Print("Unrecognized Compression Field: " & Hex(compressionID) & ", Value: " & BitConverter.ToString(rCompression.GetBytes))
                                        End Select
                                      Loop Until rCompression.EndOfStream
                                      rCompression.Close()
                                    End Using
                                  Case &H5035
                                    Using rEncryption As New MKVBinaryReader(rEncoding)
                                      Do
                                        Dim encryptionID As UInt64 = rEncryption.GetElementID
                                        Select Case encryptionID
                                          Case &H47E1 : TrackEntries(TrackI).ContentEncodings.ContentEncoding(EncodingI).ContentEncryption.Algorithm = rEncryption.GetValue
                                          Case &H47E2 : TrackEntries(TrackI).ContentEncodings.ContentEncoding(EncodingI).ContentEncryption.KeyID = rEncryption.GetBytes
                                          Case &H47E3 : TrackEntries(TrackI).ContentEncodings.ContentEncoding(EncodingI).ContentEncryption.Signature = rEncryption.GetBytes
                                          Case &H47E4 : TrackEntries(TrackI).ContentEncodings.ContentEncoding(EncodingI).ContentEncryption.SigKeyID = rEncryption.GetBytes
                                          Case &H47E5 : TrackEntries(TrackI).ContentEncodings.ContentEncoding(EncodingI).ContentEncryption.SigAlgorithm = rEncryption.GetValue
                                          Case &H47E6 : TrackEntries(TrackI).ContentEncodings.ContentEncoding(EncodingI).ContentEncryption.SigHashAlgorithm = rEncryption.GetValue
                                          Case &HEC
                                            Dim bVoid() As Byte = rEncryption.GetBytes
                                            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                                          Case Else : Debug.Print("Unrecognized Compression Field: " & Hex(encryptionID) & ", Value: " & BitConverter.ToString(rEncryption.GetBytes))
                                        End Select
                                      Loop Until rEncryption.EndOfStream
                                      rEncryption.Close()
                                    End Using
                                  Case &HEC
                                    Dim bVoid() As Byte = rEncoding.GetBytes
                                    'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                                  Case 0 : Exit Do
                                  Case Else : Debug.Print("Unrecognized Encoding Field: " & Hex(encodingID) & ", Value: " & BitConverter.ToString(rEncoding.GetBytes))
                                End Select
                              Loop Until rEncoding.EndOfStream
                              rEncoding.Close()
                            End Using
                            EncodingI += 1
                          Case &HEC
                            Dim bVoid() As Byte = rEncodings.GetBytes
                            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                          Case Else : Debug.Print("Unrecognized Encodings Field: " & Hex(encodingsID) & ", Value: " & BitConverter.ToString(rEncodings.GetBytes))
                        End Select
                      Loop Until rEncodings.EndOfStream
                      rEncodings.Close()
                    End Using
                  Case &HEC
                    Dim bVoid() As Byte = rEntry.GetBytes
                    'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                  Case Else : Debug.Print("Unrecognized Track Field: " & Hex(elID) & ", Value: " & BitConverter.ToString(rEntry.GetBytes))
                End Select
              Loop Until rEntry.EndOfStream
              rEntry.Close()
            End Using
            TrackI += 1
          Case &HEC
            Dim bVoid() As Byte = rInfo.GetBytes
            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
          Case Else
            Debug.Print("Unrecognized Track Entry ID: " & Hex(TrackEntryID) & ", Value: " & BitConverter.ToString(rInfo.GetBytes))
            Return False
        End Select
      Loop Until rInfo.EndOfStream
      rInfo.Close()
    End Using
    Return True
  End Function

  Private Function ReadChapterInfo(ByRef bReader As IO.BinaryReader) As Boolean
    Using rInfo As New MKVBinaryReader(bReader)
      Do
        Dim elID As UInt64 = rInfo.GetElementID
        Select Case elID
          Case &H45B9
            If ChapterInfo.Editions Is Nothing Then
              ReDim ChapterInfo.Editions(0)
            Else
              ReDim Preserve ChapterInfo.Editions(ChapterInfo.Editions.Length)
            End If
            Using rChapter As New MKVBinaryReader(rInfo)
              Dim ChapterI As Integer = ChapterInfo.Editions.Length - 1
              Do
                Dim chID As UInt64 = rChapter.GetElementID
                Select Case chID
                  Case &H45BC : ChapterInfo.Editions(ChapterI).UID = rChapter.GetValue
                  Case &H45BD : ChapterInfo.Editions(ChapterI).FlagHidden = (rChapter.GetValue = 1)
                  Case &H45DB : ChapterInfo.Editions(ChapterI).FlagDefault = (rChapter.GetValue = 1)
                  Case &H45DD : ChapterInfo.Editions(ChapterI).FlagOrdered = (rChapter.GetValue = 1)
                  Case &HB6
                    If ChapterInfo.Editions(ChapterI).Atoms Is Nothing Then
                      ReDim ChapterInfo.Editions(ChapterI).Atoms(0)
                    Else
                      ReDim Preserve ChapterInfo.Editions(ChapterI).Atoms(ChapterInfo.Editions(ChapterI).Atoms.Length)
                    End If
                    ReadChapterAtom(rChapter, ChapterInfo.Editions(ChapterI).Atoms)
                  Case &HEC
                    Dim bVoid() As Byte = rChapter.GetBytes
                    'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                  Case Else : Debug.Print("Unrecognized Chapter Edition Field: " & Hex(chID) & ", Value: " & BitConverter.ToString(rChapter.GetBytes))
                End Select
              Loop Until rChapter.EndOfStream
              rChapter.Close()
            End Using
          Case &HEC
            Dim bVoid() As Byte = rInfo.GetBytes
            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
          Case Else : Debug.Print("Unrecognized Chapter Info Field: " & Hex(elID) & ", Value: " & BitConverter.ToString(rInfo.GetBytes))
        End Select
      Loop Until rInfo.EndOfStream
      rInfo.Close()
    End Using
    Return True
  End Function

  Private Function ReadChapterAtom(ByRef bReader As IO.BinaryReader, ByRef AtomList() As clsMKVHeaders.ChapterAtom) As Boolean
    Using rAtom As New MKVBinaryReader(bReader)
      Dim AtomI As Integer = AtomList.Length - 1
      Do
        Dim atomID As UInt64 = rAtom.GetElementID
        Select Case atomID
          Case &H73C4 : AtomList(AtomI).UID = rAtom.GetValue
          Case &H91 : AtomList(AtomI).TimeStart = rAtom.GetValue
          Case &H92 : AtomList(AtomI).TimeEnd = rAtom.GetValue
          Case &H98 : AtomList(AtomI).FlagHidden = (rAtom.GetValue = 1)
          Case &H4598 : AtomList(AtomI).FlagEnabled = (rAtom.GetValue = 1)
          Case &H6E67 : AtomList(AtomI).SegmentUID = rAtom.GetBytes
          Case &H6EBC : AtomList(AtomI).SegmentEditionUID = rAtom.GetBytes
          Case &H63C3 : AtomList(AtomI).PhysicalEquiv = rAtom.GetValue
          Case &H8F
            If AtomList(AtomI).Track.ChapterTrackNumber Is Nothing Then
              ReDim AtomList(AtomI).Track.ChapterTrackNumber(0)
            Else
              ReDim Preserve AtomList(AtomI).Track.ChapterTrackNumber(AtomList(AtomI).Track.ChapterTrackNumber.Length)
            End If
            Using rTrack As New MKVBinaryReader(rAtom)
              Dim TrackI As Integer = 0
              If AtomList(AtomI).Track.ChapterTrackNumber IsNot Nothing Then TrackI = AtomList(AtomI).Track.ChapterTrackNumber.Length
              Do
                Dim trackID As UInt64 = rTrack.GetElementID
                Select Case trackID
                  Case &H89
                    AtomList(AtomI).Track.ChapterTrackNumber(TrackI) = rTrack.GetValue
                    TrackI += 1
                  Case &HEC
                    Dim bVoid() As Byte = rTrack.GetBytes
                    'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                  Case Else : Debug.Print("Unrecognized Chapter Edition Atom Track Field: " & Hex(trackID) & ", Value: " & BitConverter.ToString(rTrack.GetBytes))
                End Select
              Loop Until rTrack.EndOfStream
              rTrack.Close()
            End Using
          Case &H80
            If AtomList(AtomI).Display Is Nothing Then
              ReDim AtomList(AtomI).Display(0)
            Else
              ReDim Preserve AtomList(AtomI).Display(AtomList(AtomI).Display.Length)
            End If
            Using rDisplay As New MKVBinaryReader(rAtom)
              Dim DisplayI As Integer = AtomList(AtomI).Display.Length - 1
              Do
                Dim trackID As UInt64 = rDisplay.GetElementID
                Select Case trackID
                  Case &H85 : AtomList(AtomI).Display(DisplayI).Title = rDisplay.GetUTF8String
                  Case &H437C
                    If AtomList(AtomI).Display(DisplayI).Language Is Nothing Then
                      ReDim AtomList(AtomI).Display(DisplayI).Language(0)
                    Else
                      ReDim AtomList(AtomI).Display(DisplayI).Language(AtomList(AtomI).Display(DisplayI).Language.Length)
                    End If
                    AtomList(AtomI).Display(DisplayI).Language(AtomList(AtomI).Display(DisplayI).Language.Length - 1) = rDisplay.GetASCIIString
                  Case &H437E
                    If AtomList(AtomI).Display(DisplayI).Country Is Nothing Then
                      ReDim AtomList(AtomI).Display(DisplayI).Country(0)
                    Else
                      ReDim AtomList(AtomI).Display(DisplayI).Country(AtomList(AtomI).Display(DisplayI).Country.Length)
                    End If
                    AtomList(AtomI).Display(DisplayI).Country(AtomList(AtomI).Display(DisplayI).Country.Length - 1) = rDisplay.GetASCIIString
                  Case &HEC
                    Dim bVoid() As Byte = rDisplay.GetBytes
                    'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                  Case Else : Debug.Print("Unrecognized Chapter Edition Atom Display Field: " & Hex(trackID) & ", Value: " & BitConverter.ToString(rDisplay.GetBytes))
                End Select
              Loop Until rDisplay.EndOfStream
              rDisplay.Close()
            End Using
          Case &H6944
            If AtomList(AtomI).Process Is Nothing Then
              ReDim AtomList(AtomI).Process(0)
            Else
              ReDim Preserve AtomList(AtomI).Process(AtomList(AtomI).Process.Length)
            End If
            Using rProcess As New MKVBinaryReader(rAtom)
              Dim ProcessI As Integer = AtomList(AtomI).Process.Length - 1
              Do
                Dim processID As UInt64 = rProcess.GetElementID
                Select Case processID
                  Case &H6955 : AtomList(AtomI).Process(ProcessI).CodecID = rProcess.GetValue
                  Case &H450D : AtomList(AtomI).Process(ProcessI).ProcPrivate = rProcess.GetBytes
                  Case &H6911
                    If AtomList(AtomI).Process(ProcessI).Command Is Nothing Then
                      ReDim AtomList(AtomI).Process(ProcessI).Command(0)
                    Else
                      ReDim Preserve AtomList(AtomI).Process(ProcessI).Command(AtomList(AtomI).Process(ProcessI).Command.Length)
                    End If
                    Using rCommand As New MKVBinaryReader(rProcess)
                      Dim CommandI As Integer = AtomList(AtomI).Process(ProcessI).Command.Length - 1
                      Do
                        Dim commandID As UInt64 = rCommand.GetElementID
                        Select Case commandID
                          Case &H6922 : AtomList(AtomI).Process(ProcessI).Command(CommandI).Time = rCommand.GetValue
                          Case &H6933 : AtomList(AtomI).Process(ProcessI).Command(CommandI).Data = rCommand.GetBytes
                          Case &HEC
                            Dim bVoid() As Byte = rProcess.GetBytes
                            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                          Case Else : Debug.Print("Unrecognized Chapter Edition Atom  Display Command Field: " & Hex(commandID) & ", Value: " & BitConverter.ToString(rCommand.GetBytes))
                        End Select
                      Loop Until rCommand.EndOfStream
                      rCommand.Close()
                    End Using
                  Case &HEC
                    Dim bVoid() As Byte = rProcess.GetBytes
                    'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
                  Case Else : Debug.Print("Unrecognized Chapter Edition Atom Display Field: " & Hex(processID) & ", Value: " & BitConverter.ToString(rProcess.GetBytes))
                End Select
              Loop Until rProcess.EndOfStream
              rProcess.Close()
            End Using
          Case &HB6
            ReadChapterAtom(rAtom, AtomList(AtomI).SubAtoms)
          Case &HEC
            Dim bVoid() As Byte = rAtom.GetBytes
            'If Not bVoid.SequenceEqual(Array.CreateInstance(GetType(Byte), bVoid.Length)) Then Return False
          Case Else : Debug.Print("Unrecognized Chapter Edition Atom Field: " & Hex(atomID) & ", Value: " & BitConverter.ToString(rAtom.GetBytes))
        End Select
      Loop Until rAtom.EndOfStream
      rAtom.Close()
    End Using
    Return True
  End Function

#Region "IDisposable Support"
  Private disposedValue As Boolean ' To detect redundant calls

  ' IDisposable
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        ' TODO: dispose managed state (managed objects).
      End If

      ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
      ' TODO: set large fields to null.
    End If
    Me.disposedValue = True
  End Sub

  ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
  'Protected Overrides Sub Finalize()
  '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
  '    Dispose(False)
  '    MyBase.Finalize()
  'End Sub

  ' This code added by Visual Basic to correctly implement the disposable pattern.
  Public Sub Dispose() Implements IDisposable.Dispose
    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region

End Class
