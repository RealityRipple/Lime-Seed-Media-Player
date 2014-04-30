Friend Module modHeaderFunctions
  Public Function GetDWORD(bIn() As Byte, Optional ByVal lStart As Long = 0) As UInt32
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
  Public Function GetWORD(bIn() As Byte, Optional ByVal lStart As Long = 0) As UInt16
    Dim bTmp(1) As Byte
    If lStart + 1 >= bIn.Length Then
      bTmp(0) = 0
    Else
      bTmp(0) = bIn(lStart + 1)
    End If
    If lStart >= bIn.Length Then
      bTmp(1) = 0
    Else
      bTmp(1) = bIn(lStart)
    End If
    Return BitConverter.ToUInt16(bTmp, 0)
  End Function
  Public Function BufferHex(lVal As Integer, Optional ByVal lCols As Integer = 2) As String
    Dim sHex As String = Hex(lVal)
    If sHex.Length < lCols Then
      Return StrDup(lCols - sHex.Length, "0"c) & sHex
    Else
      Return sHex
    End If
  End Function
  Public Function GetString(bIn() As Byte, ByVal lStart As Long, ByVal lLength As Long) As String
    If bIn.Length < lStart + lLength Then Return String.Empty
    Return System.Text.Encoding.GetEncoding(LATIN_1).GetString(bIn, lStart, lLength)
  End Function
  Public Function GetBytePos(bIn() As Byte, ByVal bFind As Byte, Optional ByVal lStart As Integer = 0) As Integer
    If lStart = -1 Then lStart = 0
    For I As Integer = lStart To bIn.Length - 1
      If bIn(I) = bFind Then Return I
    Next
    Return -1
  End Function

  Public Function SwapEndian16(uIn As UInt16) As UInt16
    Return (uIn >> 8) Or (uIn << 8)
  End Function
  Public Function SwapEndian32(uIn As UInt32) As UInt32
    Return (uIn >> 24) Or
      ((uIn << 8) And &HFF0000) Or
      ((uIn >> 8) And &HFF00) Or
      (uIn << 24)
  End Function
  Public Function SwapEndian64(uIn As UInt64) As UInt64
    Return (uIn >> 56) Or
      ((uIn << 40) And &HFF000000000000) Or
        ((uIn << 24) And &HFF0000000000UL) Or
        ((uIn << 8) And &HFF00000000UL) Or
        ((uIn >> 8) And &HFF000000UL) Or
        ((uIn >> 24) And &HFF0000UL) Or
        ((uIn >> 40) And &HFF00UL) Or
        (uIn << 56)
  End Function

  Public ReadOnly Property fileEncoding As System.Text.Encoding
    Get
      Return System.Text.Encoding.GetEncoding("latin1")
    End Get
  End Property
End Module

Public Class clsRIFF
  Implements IDisposable
  Private Structure ChunkHeader
    Public ChunkID As String
    Public ChunkSize As UInt32
    Public Format As String
  End Structure
  Private Structure Chunk
    Public Header As ChunkHeader
    Public Data() As Byte
  End Structure
  Private bValid As Boolean
  Private bDTS As Boolean
  Private bWAV As Boolean
  Private bAVI As Boolean
  'http://msdn.microsoft.com/en-us/library/ms779636%28VS.85%29.aspx
  'http://www.morgan-multimedia.com/download/odmlff2.pdf
  Public Structure AVIMAINHEADER
    Public fcc As String
    Public cb As UInt32
    Public dwMicroSecPerFrame As UInt32
    Public dwMaxBytesPerSec As UInt32
    Public dwPaddingGranularity As UInt32
    Public dwFlags As AVIMAINHEADER_FLAGS
    Public dwTotalFrames As UInt32
    Public dwInitialFrames As UInt32
    Public dwStreams As UInt32
    Public dwSuggestedBufferSize As UInt32
    Public dwWidth As UInt32
    Public dwHeight As UInt32
    Public dwReserved0 As UInt32
    Public dwReserved1 As UInt32
    Public dwReserved2 As UInt32
    Public dwReserved3 As UInt32
  End Structure

  <Flags()>
  Public Enum AVIMAINHEADER_FLAGS As UInt32
    AVIF_HASINDEX = &H10
    AVIF_MUSTUSEINDEX = &H20
    AVIF_ISINTERLEAVED = &H100
    AVIF_TRUSTCKTYPE = &H800
    AVIF_WASCAPTUREFILE = &H10000
    AVIF_COPYRIGHTED = &H20000
  End Enum


  Public Structure AVISTREAMHEADERFRAME
    Public left As Int16
    Public top As Int16
    Public right As Int16
    Public bottom As Int16
  End Structure

  <Flags()>
  Public Enum AVISTREAMHEADER_FLAGS As UInt32
    AVISF_DISABLED = &H1
    AVISF_VIDEO_PALCHANGES = &H10000
  End Enum

  Public Structure AVISTREAMHEADER
    Public fcc As String
    Public cb As UInt32
    Public fccType As String
    Public fccHandler As String
    Public dwFlags As AVISTREAMHEADER_FLAGS
    Public wPriority As UInt16
    Public wLanguage As UInt16
    Public dwInitialFrames As UInt32
    Public dwScale As UInt32
    Public dwRate As UInt32
    Public dwStart As UInt32
    Public dwLength As UInt32
    Public dwSuggestedBufferSize As UInt32
    Public dwQuality As UInt32
    Public dwSampleSize As UInt32
    Public rcFrame As AVISTREAMHEADERFRAME
  End Structure

  <Flags()>
  Public Enum AVIOLDINDEX_ENTRY_FLAGS
    AVIIF_LIST = &H1
    AVIIF_KEYFRAME = &H10
    AVIIF_NO_TIME = &H100
    AVIIF_COMPRESSOR = &HFFF0000
  End Enum

  Public Structure AVIOLDINDEX
    Public fcc As String
    Public cb As UInt32
    Public aIndex() As AVIOLDINDEX_ENTRY
  End Structure

  Public Structure AVIOLDINDEX_ENTRY
    Public dwChunkID As UInt32
    Public dwFlags As AVIOLDINDEX_ENTRY_FLAGS
    Public dwOffset As UInt32
    Public dwSize As UInt32
  End Structure

  Public Structure IDVX_INFO
    Public Movie As String
    Public Author As String
    Public Year As String
    Public Comment As String
    Public Genre As IDVX_GENRE
    Public Rating As IDVX_RATING
    Public Extra() As Byte
    Public FileID As String
  End Structure

  Public Enum IDVX_GENRE As Integer
    Action = 0
    <StringValue("Action/Adventure")> ActionAdventure
    Adventure
    Adult
    Anime
    Cartoon
    Claymation
    Comedy
    Commercial
    Documentary
    Drama
    <StringValue("Home Video")> HomeVideo
    Horror
    Infomercial
    Interactive
    Mystery
    <StringValue("Music Video")> MusicVideo
    Other
    Religion
    <StringValue("Science Fiction")> SciFi
    Thriller
    Western
  End Enum

  Public Enum IDVX_RATING As Byte
    Unrated = 0
    G
    PG
    <StringValue("PG-13")> PG13
    R
    <StringValue("NC-17")> NC17
  End Enum

  Public Structure BITMAPINFO
    Public bmiHeader As BITMAPINFOHEADER
    Public bmiColors() As RGBQUAD
  End Structure

  Public Structure BITMAPINFOHEADER
    Public biSize As UInt32
    Public biWidth As Int32
    Public biHeight As Int32
    Public biPlanes As UInt16
    Public biBitCount As UInt16
    Public biCompression As AVIFormatTag
    Public biSizeImage As UInt32
    Public biXPelsPerMeter As Int32
    Public biYPelsPerMeter As Int32
    Public biClrUsed As UInt32
    Public biClrImportant As UInt32
  End Structure

  Public Structure RGBQUAD
    Public rgbBlue As Byte
    Public rgbGreen As Byte
    Public rgbRed As Byte
    Public rgbReserved As Byte
    Public Sub New(data() As Byte)
      rgbBlue = data(0)
      rgbGreen = data(1)
      rgbRed = data(2)
      rgbReserved = data(3)
    End Sub
  End Structure

  Public Structure WAVEFORMAT
    Public wFormatTag As WAVFormatTag
    Public nChannels As UInt16
    Public nSamplesPerSec As UInt32
    Public nAvgBytesPerSec As UInt32
    Public nBlockAlign As UInt16
  End Structure

  Public Structure WAVEFORMATEX
    Public Format As WAVEFORMAT
    Public wBitsPerSample As UInt16
    Public cbSize As UInt16
  End Structure

  Public Structure WAVEFORMATEXTENSIBLE_SAMPLES
    Public wValidBitsPerSample As UInt16
    Public wSamplesPerBlock As UInt16
    Public wReserved As UInt16
  End Structure

  Public Structure WAVEFORMATEXTENSIBLE
    Public Format As WAVEFORMATEX
    Public Samples As WAVEFORMATEXTENSIBLE_SAMPLES
    Public dwChannelMask As UInt32
    Public SubFormat As Guid
  End Structure

  Public Enum WAVFormatTag As UInt16
    WAVE_FORMAT_UNKNOWN = &H0
    WAVE_FORMAT_PCM = &H1
    WAVE_FORMAT_ADPCM = &H2
    WAVE_FORMAT_IEEE_FLOAT = &H3
    WAVE_FORMAT_VSELP = &H4
    WAVE_FORMAT_IBM_CVSD = &H5
    WAVE_FORMAT_ALAW = &H6
    WAVE_FORMAT_MULAW = &H7
    WAVE_FORMAT_DTS = &H8
    WAVE_FORMAT_DRM = &H9
    WAVE_FORMAT_WMAVOICE9 = &HA
    WAVE_FORMAT_OKI_ADPCM = &H10
    WAVE_FORMAT_DVI_ADPCM = &H11
    WAVE_FORMAT_MEDIASPACE_ADPCM = &H12
    WAVE_FORMAT_SIERRA_ADPCM = &H13
    WAVE_FORMAT_G723_ADPCM = &H14
    WAVE_FORMAT_DIGISTD = &H15
    WAVE_FORMAT_DIGIFIX = &H16
    WAVE_FORMAT_DIALOGIC_OKI_ADPCM = &H17
    WAVE_FORMAT_MEDIAVISION_ADPCM = &H18
    WAVE_FORMAT_CU_CODEC = &H19
    WAVE_FORMAT_YAMAHA_ADPCM = &H20
    WAVE_FORMAT_SONARC = &H21
    WAVE_FORMAT_DSPGROUP_TRUESPEECH = &H22
    WAVE_FORMAT_ECHOSC1 = &H23
    WAVE_FORMAT_AUDIOFILE_AF36 = &H24
    WAVE_FORMAT_APTX = &H25
    WAVE_FORMAT_AUDIOFILE_AF10 = &H26
    WAVE_FORMAT_PROSODY_1612 = &H27
    WAVE_FORMAT_LRC = &H28
    WAVE_FORMAT_DOLBY_AC2 = &H30
    WAVE_FORMAT_GSM610 = &H31
    WAVE_FORMAT_MSNAUDIO = &H32
    WAVE_FORMAT_ANTEX_ADPCME = &H33
    WAVE_FORMAT_CONTROL_RES_VQLPC = &H34
    WAVE_FORMAT_DIGIREAL = &H35
    WAVE_FORMAT_DIGIADPCM = &H36
    WAVE_FORMAT_CONTROL_RES_CR10 = &H37
    WAVE_FORMAT_NMS_VBXADPCM = &H38
    WAVE_FORMAT_CS_IMAADPCM = &H39
    WAVE_FORMAT_ECHOSC3 = &H3A
    WAVE_FORMAT_ROCKWELL_ADPCM = &H3B
    WAVE_FORMAT_ROCKWELL_DIGITALK = &H3C
    WAVE_FORMAT_XEBEC = &H3D
    WAVE_FORMAT_G721_ADPCM = &H40
    WAVE_FORMAT_G728_CELP = &H41
    WAVE_FORMAT_MSG723 = &H42
    WAVE_FORMAT_MPEG = &H50
    WAVE_FORMAT_RT24 = &H52
    WAVE_FORMAT_PAC = &H53
    WAVE_FORMAT_MPEGLAYER3 = &H55
    WAVE_FORMAT_LUCENT_G723 = &H59
    WAVE_FORMAT_CIRRUS = &H60
    WAVE_FORMAT_ESPCM = &H61
    WAVE_FORMAT_VOXWARE = &H62
    WAVEFORMAT_CANOPUS_ATRAC = &H63
    WAVE_FORMAT_G726_ADPCM = &H64
    WAVE_FORMAT_G722_ADPCM = &H65
    WAVE_FORMAT_DSAT = &H66
    WAVE_FORMAT_DSAT_DISPLAY = &H67
    WAVE_FORMAT_VOXWARE_BYTE_ALIGNED = &H69
    WAVE_FORMAT_VOXWARE_AC8 = &H70
    WAVE_FORMAT_VOXWARE_AC10 = &H71
    WAVE_FORMAT_VOXWARE_AC16 = &H72
    WAVE_FORMAT_VOXWARE_AC20 = &H73
    WAVE_FORMAT_VOXWARE_RT24 = &H74
    WAVE_FORMAT_VOXWARE_RT29 = &H75
    WAVE_FORMAT_VOXWARE_RT29HW = &H76
    WAVE_FORMAT_VOXWARE_VR12 = &H77
    WAVE_FORMAT_VOXWARE_VR18 = &H78
    WAVE_FORMAT_VOXWARE_TQ40 = &H79
    WAVE_FORMAT_SOFTSOUND = &H80
    WAVE_FORMAT_VOXWARE_TQ60 = &H81
    WAVE_FORMAT_MSRT24 = &H82
    WAVE_FORMAT_G729A = &H83
    WAVE_FORMAT_MVI_MV12 = &H84
    WAVE_FORMAT_DF_G726 = &H85
    WAVE_FORMAT_DF_GSM610 = &H86
    WAVE_FORMAT_ISIAUDIO = &H88
    WAVE_FORMAT_ONLIVE = &H89
    WAVE_FORMAT_SBC24 = &H91
    WAVE_FORMAT_DOLBY_AC3_SPDIF = &H92
    WAVE_FORMAT_ZYXEL_ADPCM = &H97
    WAVE_FORMAT_PHILIPS_LPCBB = &H98
    WAVE_FORMAT_PACKED = &H99
    WAVE_FORMAT_RAW_AAC1 = &HFF
    WAVE_FORMAT_RHETOREX_ADPCM = &H100
    WAVE_FORMAT_IRAT = &H101
    WAVE_FORMAT_VIVO_G723 = &H111
    WAVE_FORMAT_VIVO_SIREN = &H112
    WAVE_FORMAT_DIGITAL_G723 = &H123
    WAVE_FORMAT_WMAUDIO2 = &H161
    WAVE_FORMAT_WMAUDIO3 = &H162
    WAVE_FORMAT_WMAUDIO_LOSSLESS = &H163
    WAVE_FORMAT_WMASPDIF = &H164
    WAVE_FORMAT_CREATIVE_ADPCM = &H200
    WAVE_FORMAT_CREATIVE_FASTSPEECH8 = &H202
    WAVE_FORMAT_CREATIVE_FASTSPEECH10 = &H203
    WAVE_FORMAT_QUARTERDECK = &H220
    WAVE_FORMAT_RAW_SPORT = &H240
    WAVE_FORMAT_ESST_AC3 = &H241
    WAVE_FORMAT_FM_TOWNS_SND = &H300
    WAVE_FORMAT_BTV_DIGITAL = &H400
    WAVE_FORMAT_VME_VMPCM = &H680
    WAVE_FORMAT_OLIGSM = &H1000
    WAVE_FORMAT_OLIADPCM = &H1001
    WAVE_FORMAT_OLICELP = &H1002
    WAVE_FORMAT_OLISBC = &H1003
    WAVE_FORMAT_OLIOPR = &H1004
    WAVE_FORMAT_LH_CODEC = &H1100
    WAVE_FORMAT_NORRIS = &H1400
    WAVE_FORMAT_ISIAUDIO2 = &H1401
    WAVE_FORMAT_SOUNDSPACE_MUSICOMPRESS = &H1500
    WAVE_FORMAT_MPEG_ADTS_AAC = &H1600
    WAVE_FORMAT_MPEG_LOAS = &H1602
    WAVE_FORMAT_MPEG_HEAAC = &H1610
    WAVE_FORMAT_DVM = &H2000
    WAVE_FORMAT_DTS2 = &H2001
    WAVE_FORMAT_EXTENSIBLE = &HFFFE
    WAVE_FORMAT_DEVELOPMENT = &HFFFF
  End Enum

  Public Class StringValueAttribute
    Inherits Attribute
    Public Property Value As String
    Public Sub New(ByVal val As String)
      Value = val
    End Sub
    Public Overrides Function ToString() As String
      Return Value
    End Function
  End Class

  Public Enum AVIFormatTag As ULong
    <StringValue("3IV1")> AVI_FORMAT_3IV1 = &H31564933
    <StringValue("3IV2")> AVI_FORMAT_3IV2 = &H32564933
    <StringValue("8BPS")> AVI_FORMAT_8BPS = &H53504238
    <StringValue("AASC")> AVI_FORMAT_AASC = &H43534141
    <StringValue("ABYR")> AVI_FORMAT_ABYR = &H52594241
    <StringValue("ADV1")> AVI_FORMAT_ADV1 = &H31564441
    <StringValue("ADVJ")> AVI_FORMAT_ADVJ = &H4A564441
    <StringValue("AEMI")> AVI_FORMAT_AEMI = &H494D4541
    <StringValue("AFLC")> AVI_FORMAT_AFLC = &H434C4641
    <StringValue("AFLI")> AVI_FORMAT_AFLI = &H494C4641
    <StringValue("AJPG")> AVI_FORMAT_AJPG = &H47504A41
    <StringValue("AMPG")> AVI_FORMAT_AMPG = &H47504D41
    <StringValue("ANIM")> AVI_FORMAT_ANIM = &H4D494E41
    <StringValue("AP41")> AVI_FORMAT_AP41 = &H31345041
    <StringValue("ASLC")> AVI_FORMAT_ASLC = &H434C5341
    <StringValue("ASV1")> AVI_FORMAT_ASV1 = &H31565341
    <StringValue("ASV2")> AVI_FORMAT_ASV2 = &H32565341
    <StringValue("ASVX")> AVI_FORMAT_ASVX = &H58565341
    <StringValue("AUR2")> AVI_FORMAT_AUR2 = &H32525541
    <StringValue("AURA")> AVI_FORMAT_AURA = &H41525541
    <StringValue("AVC1")> AVI_FORMAT_AVC1 = &H31435641
    <StringValue("AVRN")> AVI_FORMAT_AVRN = &H4E525641
    <StringValue("BA81")> AVI_FORMAT_BA81 = &H31384142
    <StringValue("BINK")> AVI_FORMAT_BINK = &H4B4E4942
    <StringValue("BLZ0")> AVI_FORMAT_BLZ0 = &H305A4C42
    <StringValue("BT20")> AVI_FORMAT_BT20 = &H30325442
    <StringValue("BTCV")> AVI_FORMAT_BTCV = &H56435442
    <StringValue("BW10")> AVI_FORMAT_BW10 = &H30315742
    <StringValue("BYR1")> AVI_FORMAT_BYR1 = &H31525942
    <StringValue("BYR2")> AVI_FORMAT_BYR2 = &H32525942
    <StringValue("CC12")> AVI_FORMAT_CC12 = &H32314343
    <StringValue("CDVC")> AVI_FORMAT_CDVC = &H43564443
    <StringValue("CFCC")> AVI_FORMAT_CFCC = &H43434643
    <StringValue("CGDI")> AVI_FORMAT_CGDI = &H49444743
    <StringValue("CHAM")> AVI_FORMAT_CHAM = &H4D414843
    <StringValue("CJPG")> AVI_FORMAT_CJPG = &H47504A43
    <StringValue("CLJR")> AVI_FORMAT_CLJR = &H524A4C43
    <StringValue("CMYK")> AVI_FORMAT_CMYK = &H4B594D43
    <StringValue("CPLA")> AVI_FORMAT_CPLA = &H414C5043
    <StringValue("CRAM")> AVI_FORMAT_CRAM = &H4D415243
    <StringValue("CSCD")> AVI_FORMAT_CSCD = &H44435343
    <StringValue("CTRX")> AVI_FORMAT_CTRX = &H58525443
    <StringValue("CVID")> AVI_FORMAT_CVID = &H44495643
    <StringValue("CWLT")> AVI_FORMAT_CWLT = &H544C5743
    <StringValue("CXY1")> AVI_FORMAT_CXY1 = &H31595843
    <StringValue("CXY2")> AVI_FORMAT_CXY2 = &H32595843
    <StringValue("CYUV")> AVI_FORMAT_CYUV = &H56555943
    <StringValue("CYUY")> AVI_FORMAT_CYUY = &H59555943
    <StringValue("D261")> AVI_FORMAT_D261 = &H31363244
    <StringValue("D263")> AVI_FORMAT_D263 = &H33363244
    <StringValue("DAVC")> AVI_FORMAT_DAVC = &H43564144
    <StringValue("DCL1")> AVI_FORMAT_DCL1 = &H314C4344
    <StringValue("DCL2")> AVI_FORMAT_DCL2 = &H324C4344
    <StringValue("DCL3")> AVI_FORMAT_DCL3 = &H334C4344
    <StringValue("DCL4")> AVI_FORMAT_DCL4 = &H344C4344
    <StringValue("DCL5")> AVI_FORMAT_DCL5 = &H354C4344
    <StringValue("DIV3")> AVI_FORMAT_DIV3 = &H33564944
    <StringValue("DIV4")> AVI_FORMAT_DIV4 = &H34564944
    <StringValue("DIV5")> AVI_FORMAT_DIV5 = &H35564944
    <StringValue("DIVX")> AVI_FORMAT_DIVX = &H58564944
    <StringValue("DM4V")> AVI_FORMAT_DM4V = &H56344D44
    <StringValue("DMB1")> AVI_FORMAT_DMB1 = &H31424D44
    <StringValue("DMB2")> AVI_FORMAT_DMB2 = &H32424D44
    <StringValue("DMK2")> AVI_FORMAT_DMK2 = &H324B4D44
    <StringValue("DSVD")> AVI_FORMAT_DSVD = &H44565344
    <StringValue("DUCK")> AVI_FORMAT_DUCK = &H4B435544
    <StringValue("DV25")> AVI_FORMAT_DV25 = &H35325644
    <StringValue("DV50")> AVI_FORMAT_DV50 = &H30355644
    <StringValue("DVAN")> AVI_FORMAT_DVAN = &H4E415644
    <StringValue("DVCS")> AVI_FORMAT_DVCS = &H53435644
    <StringValue("DVE2")> AVI_FORMAT_DVE2 = &H32455644
    <StringValue("DVH1")> AVI_FORMAT_DVH1 = &H31485644
    <StringValue("DVHD")> AVI_FORMAT_DVHD = &H44485644
    <StringValue("DVSD")> AVI_FORMAT_DVSD = &H44535644
    <StringValue("DVSL")> AVI_FORMAT_DVSL = &H4C535644
    <StringValue("DVX1")> AVI_FORMAT_DVX1 = &H31585644
    <StringValue("DVX2")> AVI_FORMAT_DVX2 = &H32585644
    <StringValue("DVX3")> AVI_FORMAT_DVX3 = &H33585644
    <StringValue("DX50")> AVI_FORMAT_DX50 = &H30355844
    <StringValue("DXGM")> AVI_FORMAT_DXGM = &H4D475844
    <StringValue("DXTC")> AVI_FORMAT_DXTC = &H43545844
    <StringValue("DXT1")> AVI_FORMAT_DXT1 = &H31545844
    <StringValue("DXT2")> AVI_FORMAT_DXT2 = &H32545844
    <StringValue("DXT3")> AVI_FORMAT_DXT3 = &H33545844
    <StringValue("DXT4")> AVI_FORMAT_DXT4 = &H34545844
    <StringValue("DXT5")> AVI_FORMAT_DXT5 = &H35545844
    <StringValue("EKQ0")> AVI_FORMAT_EKQ0 = &H30514B45
    <StringValue("ELK0")> AVI_FORMAT_ELK0 = &H304B4C45
    <StringValue("EM2V")> AVI_FORMAT_EM2V = &H56324D45
    <StringValue("ES07")> AVI_FORMAT_ES07 = &H37305345
    <StringValue("ESCP")> AVI_FORMAT_ESCP = &H50435345
    <StringValue("ETV1")> AVI_FORMAT_ETV1 = &H31565445
    <StringValue("ETV2")> AVI_FORMAT_ETV2 = &H32565445
    <StringValue("ETVC")> AVI_FORMAT_ETVC = &H43565445
    <StringValue("FFV1")> AVI_FORMAT_FFV1 = &H31564646
    <StringValue("FLJP")> AVI_FORMAT_FLJP = &H504A4C46
    <StringValue("FMP4")> AVI_FORMAT_FMP4 = &H34504D46
    <StringValue("FMVC")> AVI_FORMAT_FMVC = &H43564D46
    <StringValue("FPS1")> AVI_FORMAT_FPS1 = &H31535046
    <StringValue("FRWA")> AVI_FORMAT_FRWA = &H41575246
    <StringValue("FRWD")> AVI_FORMAT_FRWD = &H44575246
    <StringValue("FVF1")> AVI_FORMAT_FVF1 = &H31465646
    <StringValue("GEOX")> AVI_FORMAT_GEOX = &H584F4547
    <StringValue("GJPG")> AVI_FORMAT_GJPG = &H47504A47
    <StringValue("GLZW")> AVI_FORMAT_GLZW = &H575A4C47
    <StringValue("GPEG")> AVI_FORMAT_GPEG = &H47455047
    <StringValue("GWLT")> AVI_FORMAT_GWLT = &H544C5747
    <StringValue("H260")> AVI_FORMAT_H260 = &H30363248
    <StringValue("H261")> AVI_FORMAT_H261 = &H31363248
    <StringValue("H262")> AVI_FORMAT_H262 = &H32363248
    <StringValue("H263")> AVI_FORMAT_H263 = &H33363248
    <StringValue("H264")> AVI_FORMAT_H264 = &H34363248
    <StringValue("H265")> AVI_FORMAT_H265 = &H35363248
    <StringValue("H266")> AVI_FORMAT_H266 = &H36363248
    <StringValue("H267")> AVI_FORMAT_H267 = &H37363248
    <StringValue("H268")> AVI_FORMAT_H268 = &H38363248
    <StringValue("H269")> AVI_FORMAT_H269 = &H39363248
    <StringValue("HDYC")> AVI_FORMAT_HDYC = &H43594448
    <StringValue("HFYU")> AVI_FORMAT_HFYU = &H55594648
    <StringValue("HMCR")> AVI_FORMAT_HMCR = &H52434D48
    <StringValue("HMRR")> AVI_FORMAT_HMRR = &H52524D48
    <StringValue("I263")> AVI_FORMAT_I263 = &H33363249
    <StringValue("I420")> AVI_FORMAT_I420 = &H30323449
    <StringValue("IAN ")> AVI_FORMAT_IAN = &H204E4149
    <StringValue("ICLB")> AVI_FORMAT_ICLB = &H424C4349
    <StringValue("IGOR")> AVI_FORMAT_IGOR = &H524F4749
    <StringValue("IJPG")> AVI_FORMAT_IJPG = &H47504A49
    <StringValue("ILVC")> AVI_FORMAT_ILVC = &H43564C49
    <StringValue("ILVR")> AVI_FORMAT_ILVR = &H52564C49
    <StringValue("IPDV")> AVI_FORMAT_IPDV = &H56445049
    <StringValue("IR21")> AVI_FORMAT_IR21 = &H31325249
    <StringValue("IRAW")> AVI_FORMAT_IRAW = &H57415249
    <StringValue("ISME")> AVI_FORMAT_ISME = &H454D5349
    <StringValue("IV30")> AVI_FORMAT_IV30 = &H30335649
    <StringValue("IV31")> AVI_FORMAT_IV31 = &H31335649
    <StringValue("IV32")> AVI_FORMAT_IV32 = &H32335649
    <StringValue("IV33")> AVI_FORMAT_IV33 = &H33335649
    <StringValue("IV34")> AVI_FORMAT_IV34 = &H34335649
    <StringValue("IV35")> AVI_FORMAT_IV35 = &H35335649
    <StringValue("IV36")> AVI_FORMAT_IV36 = &H36335649
    <StringValue("IV37")> AVI_FORMAT_IV37 = &H37335649
    <StringValue("IV38")> AVI_FORMAT_IV38 = &H38335649
    <StringValue("IV39")> AVI_FORMAT_IV39 = &H39335649
    <StringValue("IV40")> AVI_FORMAT_IV40 = &H30345649
    <StringValue("IV41")> AVI_FORMAT_IV41 = &H31345649
    <StringValue("IV42")> AVI_FORMAT_IV42 = &H32345649
    <StringValue("IV43")> AVI_FORMAT_IV43 = &H33345649
    <StringValue("IV44")> AVI_FORMAT_IV44 = &H34345649
    <StringValue("IV45")> AVI_FORMAT_IV45 = &H35345649
    <StringValue("IV46")> AVI_FORMAT_IV46 = &H36345649
    <StringValue("IV47")> AVI_FORMAT_IV47 = &H37345649
    <StringValue("IV48")> AVI_FORMAT_IV48 = &H38345649
    <StringValue("IV49")> AVI_FORMAT_IV49 = &H39345649
    <StringValue("IV50")> AVI_FORMAT_IV50 = &H30355649
    <StringValue("JBYR")> AVI_FORMAT_JBYR = &H5259424A
    <StringValue("JPEG")> AVI_FORMAT_JPEG = &H4745504A
    <StringValue("JPGL")> AVI_FORMAT_JPGL = &H4C47504A
    <StringValue("KMVC")> AVI_FORMAT_KMVC = &H43564D4B
    <StringValue("L261")> AVI_FORMAT_L261 = &H3136324C
    <StringValue("L263")> AVI_FORMAT_L263 = &H3336324C
    <StringValue("LBYR")> AVI_FORMAT_LBYR = &H5259424C
    <StringValue("LCMW")> AVI_FORMAT_LCMW = &H574D434C
    <StringValue("LCW2")> AVI_FORMAT_LCW2 = &H3257434C
    <StringValue("LEAD")> AVI_FORMAT_LEAD = &H4441454C
    <StringValue("LGRY")> AVI_FORMAT_LGRY = &H5952474C
    <StringValue("LJ11")> AVI_FORMAT_LJ11 = &H31314A4C
    <StringValue("LJ22")> AVI_FORMAT_LJ22 = &H32324A4C
    <StringValue("LJ2K")> AVI_FORMAT_LJ2K = &H4B324A4C
    <StringValue("LJ44")> AVI_FORMAT_LJ44 = &H34344A4C
    <StringValue("LJPG")> AVI_FORMAT_LJPG = &H47504A4C
    <StringValue("LMP2")> AVI_FORMAT_LMP2 = &H32504D4C
    <StringValue("LMP4")> AVI_FORMAT_LMP4 = &H34504D4C
    <StringValue("LSVC")> AVI_FORMAT_LSVC = &H4356534C
    <StringValue("LSVM")> AVI_FORMAT_LSVM = &H4D56534C
    <StringValue("LSVX")> AVI_FORMAT_LSVX = &H5856534C
    <StringValue("LZO1")> AVI_FORMAT_LZO1 = &H314F5A4C
    <StringValue("M261")> AVI_FORMAT_M261 = &H3136324D
    <StringValue("M263")> AVI_FORMAT_M263 = &H3336324D
    <StringValue("M4CC")> AVI_FORMAT_M4CC = &H4343344D
    <StringValue("M4S2")> AVI_FORMAT_M4S2 = &H3253344D
    <StringValue("MC12")> AVI_FORMAT_MC12 = &H3231434D
    <StringValue("MCAM")> AVI_FORMAT_MCAM = &H4D41434D
    <StringValue("MJ2C")> AVI_FORMAT_MJ2C = &H43324A4D
    <StringValue("MJPG")> AVI_FORMAT_MJPG = &H47504A4D
    <StringValue("MMES")> AVI_FORMAT_MMES = &H53454D4D
    <StringValue("MP2A")> AVI_FORMAT_MP2A = &H4132504D
    <StringValue("MP2T")> AVI_FORMAT_MP2T = &H5432504D
    <StringValue("MP2V")> AVI_FORMAT_MP2V = &H5632504D
    <StringValue("MP42")> AVI_FORMAT_MP42 = &H3234504D
    <StringValue("MP43")> AVI_FORMAT_MP43 = &H3334504D
    <StringValue("MP4A")> AVI_FORMAT_MP4A = &H4134504D
    <StringValue("MP4S")> AVI_FORMAT_MP4S = &H5334504D
    <StringValue("MP4T")> AVI_FORMAT_MP4T = &H5434504D
    <StringValue("MP4V")> AVI_FORMAT_MP4V = &H5634504D
    <StringValue("MPEG")> AVI_FORMAT_MPEG = &H4745504D
    <StringValue("MPG4")> AVI_FORMAT_MPG4 = &H3447504D
    <StringValue("MPGI")> AVI_FORMAT_MPGI = &H4947504D
    <StringValue("MR16")> AVI_FORMAT_MR16 = &H3631524D
    <StringValue("MRCA")> AVI_FORMAT_MRCA = &H4143524D
    <StringValue("MRLE")> AVI_FORMAT_MRLE = &H454C524D
    <StringValue("MSVC")> AVI_FORMAT_MSVC = &H4356534D
    <StringValue("MSZH")> AVI_FORMAT_MSZH = &H485A534D
    <StringValue("MTX1")> AVI_FORMAT_MTX1 = &H3158544D
    <StringValue("MTX2")> AVI_FORMAT_MTX2 = &H3258544D
    <StringValue("MTX3")> AVI_FORMAT_MTX3 = &H3358544D
    <StringValue("MTX4")> AVI_FORMAT_MTX4 = &H3458544D
    <StringValue("MTX5")> AVI_FORMAT_MTX5 = &H3558544D
    <StringValue("MTX6")> AVI_FORMAT_MTX6 = &H3658544D
    <StringValue("MTX7")> AVI_FORMAT_MTX7 = &H3758544D
    <StringValue("MTX8")> AVI_FORMAT_MTX8 = &H3858544D
    <StringValue("MTX9")> AVI_FORMAT_MTX9 = &H3958544D
    <StringValue("MVI1")> AVI_FORMAT_MVI1 = &H3149564D
    <StringValue("MVI2")> AVI_FORMAT_MVI2 = &H3249564D
    <StringValue("MWV1")> AVI_FORMAT_MWV1 = &H3157564D
    <StringValue("NAVI")> AVI_FORMAT_NAVI = &H4956414E
    <StringValue("nAVI")> AVI_FORMAT_nAVI_LOWER = &H4956416E
    <StringValue("NDSC")> AVI_FORMAT_NDSC = &H4353444E
    <StringValue("NDSH")> AVI_FORMAT_NDSH = &H4853444E
    <StringValue("NDSM")> AVI_FORMAT_NDSM = &H4D53444E
    <StringValue("NDSP")> AVI_FORMAT_NDSP = &H5053444E
    <StringValue("NDSS")> AVI_FORMAT_NDSS = &H5353444E
    <StringValue("NDXC")> AVI_FORMAT_NDXC = &H4358444E
    <StringValue("NDXH")> AVI_FORMAT_NDXH = &H4858444E
    <StringValue("NDXM")> AVI_FORMAT_NDXM = &H4D58444E
    <StringValue("NDXP")> AVI_FORMAT_NDXP = &H5058444E
    <StringValue("NDXS")> AVI_FORMAT_NDXS = &H5358444E
    <StringValue("NHVU")> AVI_FORMAT_NHVU = &H5556484E
    <StringValue("NTN1")> AVI_FORMAT_NTN1 = &H314E544E
    <StringValue("NTN2")> AVI_FORMAT_NTN2 = &H324E544E
    <StringValue("NVDS")> AVI_FORMAT_NVDS = &H5344564E
    <StringValue("NVHS")> AVI_FORMAT_NVHS = &H5348564E
    <StringValue("NVS0")> AVI_FORMAT_NVS0 = &H3053564E
    <StringValue("NVS1")> AVI_FORMAT_NVS1 = &H3153564E
    <StringValue("NVS2")> AVI_FORMAT_NVS2 = &H3253564E
    <StringValue("NVS3")> AVI_FORMAT_NVS3 = &H3353564E
    <StringValue("NVS4")> AVI_FORMAT_NVS4 = &H3453564E
    <StringValue("NVS5")> AVI_FORMAT_NVS5 = &H3553564E
    <StringValue("NVT0")> AVI_FORMAT_NVT0 = &H3054564E
    <StringValue("NVT1")> AVI_FORMAT_NVT1 = &H3154564E
    <StringValue("NVT2")> AVI_FORMAT_NVT2 = &H3254564E
    <StringValue("NVT3")> AVI_FORMAT_NVT3 = &H3354564E
    <StringValue("NVT4")> AVI_FORMAT_NVT4 = &H3454564E
    <StringValue("NVT5")> AVI_FORMAT_NVT5 = &H3554564E
    <StringValue("PDVC")> AVI_FORMAT_PDVC = &H43564450
    <StringValue("PGVV")> AVI_FORMAT_PGVV = &H56564750
    <StringValue("PHMO")> AVI_FORMAT_PHMO = &H4F4D4850
    <StringValue("PIM1")> AVI_FORMAT_PIM1 = &H314D4950
    <StringValue("PIM2")> AVI_FORMAT_PIM2 = &H324D4950
    <StringValue("PIMJ")> AVI_FORMAT_PIMJ = &H4A4D4950
    <StringValue("PIXL")> AVI_FORMAT_PIXL = &H4C584950
    <StringValue("PJPG")> AVI_FORMAT_PJPG = &H47504A50
    <StringValue("PVEZ")> AVI_FORMAT_PVEZ = &H5A455650
    <StringValue("PVMM")> AVI_FORMAT_PVMM = &H4D4D5650
    <StringValue("PVW2")> AVI_FORMAT_PVW2 = &H32575650
    <StringValue("qpeg")> AVI_FORMAT_qpeg = &H67657071
    <StringValue("qpeq")> AVI_FORMAT_qpeq = &H71657071
    <StringValue("RGBT")> AVI_FORMAT_RGBT = &H54424752
    <StringValue("RLE ")> AVI_FORMAT_RLE = &H20454C52
    <StringValue("RLE4")> AVI_FORMAT_RLE4 = &H34454C52
    <StringValue("RLE8")> AVI_FORMAT_RLE8 = &H38454C52
    <StringValue("RMP4")> AVI_FORMAT_RMP4 = &H34504D52
    <StringValue("RPZA")> AVI_FORMAT_RPZA = &H415A5052
    <StringValue("RT21")> AVI_FORMAT_RT21 = &H31325452
    <StringValue("RV10")> AVI_FORMAT_RV10 = &H30315652
    <StringValue("RV13")> AVI_FORMAT_RV13 = &H33315652
    <StringValue("RV20")> AVI_FORMAT_RV20 = &H30325652
    <StringValue("RV30")> AVI_FORMAT_RV30 = &H30335652
    <StringValue("RV40")> AVI_FORMAT_RV40 = &H30345652
    <StringValue("RVX ")> AVI_FORMAT_RVX = &H20585652
    <StringValue("S422")> AVI_FORMAT_S422 = &H32323453
    <StringValue("SAN3")> AVI_FORMAT_SAN3 = &H334E4153
    <StringValue("SDCC")> AVI_FORMAT_SDCC = &H43434453
    <StringValue("SEDG")> AVI_FORMAT_SEDG = &H47444553
    <StringValue("SFMC")> AVI_FORMAT_SFMC = &H434D4653
    <StringValue("SMP4")> AVI_FORMAT_SMP4 = &H34504D53
    <StringValue("SMSC")> AVI_FORMAT_SMSC = &H43534D53
    <StringValue("SMSD")> AVI_FORMAT_SMSD = &H44534D53
    <StringValue("SMSV")> AVI_FORMAT_SMSV = &H56534D53
    <StringValue("SP40")> AVI_FORMAT_SP40 = &H30345053
    <StringValue("SP44")> AVI_FORMAT_SP44 = &H34345053
    <StringValue("SP45")> AVI_FORMAT_SP54 = &H34355053
    <StringValue("SPIG")> AVI_FORMAT_SPIG = &H47495053
    <StringValue("SPLC")> AVI_FORMAT_SPLC = &H434C5053
    <StringValue("SQZ2")> AVI_FORMAT_SQZ2 = &H325A5153
    <StringValue("STVA")> AVI_FORMAT_STVA = &H41565453
    <StringValue("STVB")> AVI_FORMAT_STVB = &H42565453
    <StringValue("STVC")> AVI_FORMAT_STVC = &H43565453
    <StringValue("STVX")> AVI_FORMAT_STVX = &H58565453
    <StringValue("STVY")> AVI_FORMAT_STVY = &H59565453
    <StringValue("SV10")> AVI_FORMAT_SV10 = &H30315653
    <StringValue("SVQ1")> AVI_FORMAT_SVQ1 = &H31515653
    <StringValue("SVQ3")> AVI_FORMAT_SVQ3 = &H33515653
    '<StringValue("")> AVI_FORMAT_ = &H
    <StringValue("TLMS")> AVI_FORMAT_TLMS = &H534D4C54
    <StringValue("TLST")> AVI_FORMAT_TLST = &H54534C54
    <StringValue("TM20")> AVI_FORMAT_TM20 = &H30324D54
    <StringValue("TMIC")> AVI_FORMAT_TMIC = &H43494D54
    <StringValue("tmot")> AVI_FORMAT_tmot = &H746F6D74
    <StringValue("TR20")> AVI_FORMAT_TR20 = &H30325254
    <StringValue("ULTI")> AVI_FORMAT_ULTI = &H49544C55
    <StringValue("UYVY")> AVI_FORMAT_UYVY = &H59565955
    <StringValue("V422")> AVI_FORMAT_V422 = &H32323456
    <StringValue("V655")> AVI_FORMAT_V655 = &H35353656
    <StringValue("VCR1")> AVI_FORMAT_VCR1 = &H31524356
    <StringValue("VCR2")> AVI_FORMAT_VCR2 = &H32524356
    <StringValue("VCR3")> AVI_FORMAT_VCR3 = &H33524356
    <StringValue("VCR4")> AVI_FORMAT_VCR4 = &H34524356
    <StringValue("VCR5")> AVI_FORMAT_VCR5 = &H35524356
    <StringValue("VCR6")> AVI_FORMAT_VCR6 = &H36524356
    <StringValue("VCR7")> AVI_FORMAT_VCR7 = &H37524356
    <StringValue("VCR8")> AVI_FORMAT_VCR8 = &H38524356
    <StringValue("VCR9")> AVI_FORMAT_VCR9 = &H39524356
    <StringValue("VDCT")> AVI_FORMAT_VDCT = &H54434456
    <StringValue("VIDS")> AVI_FORMAT_VIDS = &H53444956
    <StringValue("VIVO")> AVI_FORMAT_VIVO = &H4F564956
    <StringValue("vivo")> AVI_FORMAT_vivo_LOWER = &H6F766976
    <StringValue("VIXL")> AVI_FORMAT_VIXL = &H4C584956
    <StringValue("VLV1")> AVI_FORMAT_VLV1 = &H31564C56
    <StringValue("WBVC")> AVI_FORMAT_WBVC = &H43564257
    <StringValue("WHAM")> AVI_FORMAT_WHAM = &H4D414857
    <StringValue("X263")> AVI_FORMAT_X263 = &H33363258
    <StringValue("XLV0")> AVI_FORMAT_XLV0 = &H30564C58
    <StringValue("Y211")> AVI_FORMAT_Y211 = &H30564C58
    <StringValue("Y411")> AVI_FORMAT_Y411 = &H31313459
    <StringValue("Y41B")> AVI_FORMAT_Y41B = &H42313459
    <StringValue("Y41P")> AVI_FORMAT_Y41P = &H50313459
    <StringValue("Y41T")> AVI_FORMAT_Y41T = &H54313459
    <StringValue("Y42B")> AVI_FORMAT_Y42B = &H42323459
    <StringValue("Y42T")> AVI_FORMAT_Y42T = &H54323459
    <StringValue("YC12")> AVI_FORMAT_YC12 = &H32314359
    <StringValue("YUV8")> AVI_FORMAT_YUV8 = &H38565559
    <StringValue("YUV9")> AVI_FORMAT_YUV9 = &H39565559
    <StringValue("YUY2")> AVI_FORMAT_YUY2 = &H32595559
    <StringValue("YUYV")> AVI_FORMAT_YUYV = &H56595559
    <StringValue("YV12")> AVI_FORMAT_YV12 = &H32315659
    <StringValue("YVU9")> AVI_FORMAT_YVU9 = &H39555659
    <StringValue("YVYU")> AVI_FORMAT_YVYU = &H55595659
    <StringValue("ZPEG")> AVI_FORMAT_ZPEG = &H4745505A
  End Enum


  Public Enum ChannelStruct As UInt32
    FrontLeft = &H1
    FrontRight = &H2
    FrontCenter = &H4
    LFE = &H8
    RearLeft = &H10
    RearRight = &H20
    FrontCenterLeft = &H40
    FrontCenterRight = &H80
    RearCenter = &H100
    SideLeft = &H200
    SideRight = &H400
    TopCenter = &H800
    TopFrontLeft = &H1000
    TopFrontCenter = &H2000
    TopFrontRight = &H4000
    TopRearLeft = &H8000
    TopRearCenter = &H10000
    TopRearRight = &H20000
  End Enum

  Public Structure DTSInfo
    Public uSYNC As UInt32
    Public bFTYPE As Boolean
    Public iSHORT As UInt16
    Public bCPF As Boolean
    Public iNBLKS As UInt16
    Public iFSIZE As UInt16
    Public iAMODE As UInt16
    Public iSFREQ As UInt16
    Public iRATE As UInt16
    Public bFixedBit As Boolean
    Public bDYNF As Boolean
    Public bTIMEF As Boolean
    Public bAUXF As Boolean
    Public bHDCD As Boolean
    Public iEXT_AUDIO_ID As UInt16
    Public bEXT_AUDIO As Boolean
    Public bASPF As Boolean
    Public iLFF As UInt16
    Public bHFLAG As Boolean
    Public iHCRC As UInt16
    Public bFILTS As Boolean
    Public iVERNUM As UInt16
    Public iCHIST As UInt16
    Public iPCMR As UInt16
    Public bSUMF As Boolean
    Public bSUMS As Boolean
    Public iDIALNORM As UInt16
    Public iDNG As Integer
  End Structure

  Private wfEX As WAVEFORMATEXTENSIBLE
  Private dtsEX As DTSInfo
  Private aviMain As AVIMAINHEADER
  Private aviStreams As List(Of AVISTREAMHEADER)
  Private aviIDVX As IDVX_INFO
  Private aviINDEX As AVIOLDINDEX
  Private aviBMP As List(Of BITMAPINFO)
  Private aviWAV As List(Of WAVEFORMATEX)
  Private aviINFO As Dictionary(Of String, String)

  Public ReadOnly Property WAVData As WAVEFORMATEXTENSIBLE
    Get
      Return wfEX
    End Get
  End Property

  Public ReadOnly Property DTSData As DTSInfo
    Get
      Return dtsEX
    End Get
  End Property

  Public ReadOnly Property AVIMainData As AVIMAINHEADER
    Get
      Return aviMain
    End Get
  End Property

  Public ReadOnly Property AVIStreamCount As Integer
    Get
      If aviStreams Is Nothing Then Return 0
      Return aviStreams.Count
    End Get
  End Property

  Public ReadOnly Property AVIStreamData(Index As Integer) As AVISTREAMHEADER
    Get
      If aviStreams Is Nothing Then Return Nothing
      Return aviStreams(Index)
    End Get
  End Property

  Public ReadOnly Property AVIDIVXData As IDVX_INFO
    Get
      Return aviIDVX
    End Get
  End Property

  Public ReadOnly Property AVIINDEXData As AVIOLDINDEX
    Get
      Return aviINDEX
    End Get
  End Property

  Public ReadOnly Property AVIVideoCount As Integer
    Get
      If aviBMP Is Nothing Then Return 0
      Return aviBMP.Count
    End Get
  End Property

  Public ReadOnly Property AVIVideoData(Index As Integer) As BITMAPINFO
    Get
      If aviBMP Is Nothing Then Return Nothing
      Return aviBMP(Index)
    End Get
  End Property

  Public ReadOnly Property AVIAudioCount As Integer
    Get
      If aviWAV Is Nothing Then Return 0
      Return aviWAV.Count
    End Get
  End Property

  Public ReadOnly Property AVIAudioData(Index As Integer) As WAVEFORMATEX
    Get
      If aviWAV Is Nothing Then Return Nothing
      Return aviWAV(Index)
    End Get
  End Property

  Public ReadOnly Property AVIINFOData As Dictionary(Of String, String)
    Get
      Return aviINFO
    End Get
  End Property

  Public Sub New(FilePath As String)
    bValid = False
    If String.IsNullOrEmpty(FilePath) Then Exit Sub
    If Not My.Computer.FileSystem.FileExists(FilePath) Then Exit Sub
    Using ioFile As New IO.BinaryReader(New IO.FileStream(FilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read))
      Dim mChunk As Chunk
      mChunk.Header.ChunkID = ioFile.ReadChars(4)
      If Not mChunk.Header.ChunkID = "RIFF" Then Exit Sub
      mChunk.Header.ChunkSize = ioFile.ReadUInt32
      mChunk.Header.Format = ioFile.ReadChars(4)
      Select Case mChunk.Header.Format
        Case "WAVE"
          'WAVEFORMAT
          Do While ioFile.BaseStream.Position < mChunk.Header.ChunkSize
            Dim wavChunk As Chunk
            wavChunk.Header.ChunkID = ioFile.ReadChars(4)
            wavChunk.Header.ChunkSize = ioFile.ReadUInt32
            wavChunk.Data = ioFile.ReadBytes(wavChunk.Header.ChunkSize)
            Using ioData As New IO.BinaryReader(New IO.MemoryStream(wavChunk.Data))
              Select Case wavChunk.Header.ChunkID
                Case "fmt "
                  wfEX = New WAVEFORMATEXTENSIBLE
                  Select Case wavChunk.Header.ChunkSize
                    Case 16 'WAVEFORMAT
                      wfEX.Format.Format.wFormatTag = ioData.ReadUInt16
                      wfEX.Format.Format.nChannels = ioData.ReadUInt16
                      wfEX.Format.Format.nSamplesPerSec = ioData.ReadUInt32
                      wfEX.Format.Format.nAvgBytesPerSec = ioData.ReadUInt32
                      wfEX.Format.Format.nBlockAlign = ioData.ReadUInt16
                      bWAV = True
                      bValid = True
                    Case 18, 20 ' WAVEFORMATEX
                      wfEX.Format.Format.wFormatTag = ioData.ReadUInt16
                      wfEX.Format.Format.nChannels = ioData.ReadUInt16
                      wfEX.Format.Format.nSamplesPerSec = ioData.ReadUInt32
                      wfEX.Format.Format.nAvgBytesPerSec = ioData.ReadUInt32
                      wfEX.Format.Format.nBlockAlign = ioData.ReadUInt16
                      wfEX.Format.wBitsPerSample = ioData.ReadUInt16
                      wfEX.Format.cbSize = ioData.ReadUInt16
                      bWAV = True
                      bValid = True
                    Case 40 'WAVEFORMATEXTENSIBLE
                      wfEX.Format.Format.wFormatTag = ioData.ReadUInt16
                      wfEX.Format.Format.nChannels = ioData.ReadUInt16
                      wfEX.Format.Format.nSamplesPerSec = ioData.ReadUInt32
                      wfEX.Format.Format.nAvgBytesPerSec = ioData.ReadUInt32
                      wfEX.Format.Format.nBlockAlign = ioData.ReadUInt16
                      wfEX.Format.wBitsPerSample = ioData.ReadUInt16
                      wfEX.Format.cbSize = ioData.ReadUInt16
                      wfEX.Samples.wValidBitsPerSample = ioData.ReadUInt16
                      wfEX.dwChannelMask = ioData.ReadUInt32
                      wfEX.SubFormat = New Guid(ioData.ReadBytes(16))
                      bWAV = True
                      bValid = True
                    Case Else
                      Debug.Print("Unkown Size " & wavChunk.Header.ChunkSize)
                      Exit Sub
                  End Select
                Case "data"
                  Dim firstFour() As Byte = ioData.ReadBytes(4)
                  Do While firstFour(0) = 0
                    ioData.BaseStream.Position -= 3
                    firstFour = ioData.ReadBytes(4)
                  Loop
                  Select Case BitConverter.ToString(firstFour)
                    Case "7F-FE-80-01" 'Raw Big Endian
                    Case "FE-7F-01-80" 'Raw Little Endian
                    Case "1F-FF-E8-00" '14-bit Big Endian
                    Case "FF-1F-00-E8" '14-bit Little Endian
                      ioData.BaseStream.Position -= 4
                      Dim bDTSa() As Byte = BytesTo14BitL(ioData.ReadBytes(24))
                      If bDTSa(0) = &H7F And bDTSa(1) = &HFE And bDTSa(2) = &H80 And bDTSa(3) = &H1 Then
                        bDTS = True
                        sizeLeft = 8
                        currentByte = bDTSa(0)
                        dtsEX.uSYNC = ReadBits(bDTSa, 32)
                        dtsEX.bFTYPE = ReadBits(bDTSa, 1) = 1
                        dtsEX.iSHORT = ReadBits(bDTSa, 5)
                        dtsEX.bCPF = ReadBits(bDTSa, 1) = 1
                        dtsEX.iNBLKS = ReadBits(bDTSa, 7)
                        dtsEX.iFSIZE = ReadBits(bDTSa, 14)
                        dtsEX.iAMODE = ReadBits(bDTSa, 6)
                        dtsEX.iSFREQ = ReadBits(bDTSa, 4)
                        dtsEX.iRATE = ReadBits(bDTSa, 5)
                        dtsEX.bFixedBit = ReadBits(bDTSa, 1) = 1
                        dtsEX.bDYNF = ReadBits(bDTSa, 1) = 1
                        dtsEX.bTIMEF = ReadBits(bDTSa, 1) = 1
                        dtsEX.bAUXF = ReadBits(bDTSa, 1) = 1
                        dtsEX.bHDCD = ReadBits(bDTSa, 1) = 1
                        dtsEX.iEXT_AUDIO_ID = ReadBits(bDTSa, 3)
                        dtsEX.bEXT_AUDIO = ReadBits(bDTSa, 1) = 1
                        dtsEX.bASPF = ReadBits(bDTSa, 1) = 1
                        dtsEX.iLFF = ReadBits(bDTSa, 2)
                        dtsEX.bHFLAG = ReadBits(bDTSa, 1) = 1
                        If dtsEX.bCPF Then dtsEX.iHCRC = ReadBits(bDTSa, 16)
                        dtsEX.bFILTS = ReadBits(bDTSa, 1)
                        dtsEX.iVERNUM = ReadBits(bDTSa, 4)
                        dtsEX.iCHIST = ReadBits(bDTSa, 2)
                        dtsEX.iPCMR = ReadBits(bDTSa, 3)
                        dtsEX.bSUMF = ReadBits(bDTSa, 1) = 1
                        dtsEX.bSUMS = ReadBits(bDTSa, 1) = 1
                        dtsEX.iDIALNORM = ReadBits(bDTSa, 4)
                        Select Case dtsEX.iVERNUM
                          Case 6
                            dtsEX.iDNG = -1 * (16 + DTSData.iDIALNORM)
                          Case 7
                            dtsEX.iDNG = -1 * DTSData.iDIALNORM
                          Case Else
                            dtsEX.iDNG = 0
                            dtsEX.iDIALNORM = 0
                        End Select
                      Else
                        bDTS = False
                        Exit Sub
                      End If
                    Case Else
                      ioData.BaseStream.Position -= 4
                      Dim bDTSa() As Byte = BytesTo14BitL(ioData.ReadBytes(16))
                      Debug.Print("Unknown Data ID: " & Hex(bDTSa(0)) & Hex(bDTSa(1)) & Hex(bDTSa(2)) & Hex(bDTSa(3)))
                  End Select
                  Exit Do
                Case Else
                  Debug.Print("Unknown Chunk ID: " & wavChunk.Header.ChunkID)
              End Select
            End Using
          Loop
        Case "AVI "
          'AVIFORMAT
          bAVI = True
          Do While ioFile.BaseStream.Position < mChunk.Header.ChunkSize
            Dim aviChunk As Chunk
            aviChunk.Header.ChunkID = ioFile.ReadChars(4)
            aviChunk.Header.ChunkSize = ioFile.ReadUInt32
            Dim aviOffset As Long = ioFile.BaseStream.Position
            'aviChunk.Data = ioFile.ReadBytes(aviChunk.Header.ChunkSize)
            'Using ioData As New IO.BinaryReader(New IO.MemoryStream(aviChunk.Data))
            Select Case aviChunk.Header.ChunkID
              Case "LIST"
                aviChunk.Header.Format = ioFile.ReadChars(4)
                Select Case aviChunk.Header.Format
                  Case "hdrl"
                    Do While ioFile.BaseStream.Position - aviOffset < aviChunk.Header.ChunkSize
                      Dim mainChunk As Chunk
                      mainChunk.Header.ChunkID = ioFile.ReadChars(4)
                      mainChunk.Header.ChunkSize = ioFile.ReadUInt32
                      mainChunk.Data = ioFile.ReadBytes(mainChunk.Header.ChunkSize)
                      Using ioMain As New IO.BinaryReader(New IO.MemoryStream(mainChunk.Data))
                        Select Case mainChunk.Header.ChunkID
                          Case "avih"
                            aviMain = New AVIMAINHEADER
                            aviMain.fcc = mainChunk.Header.ChunkID
                            aviMain.cb = mainChunk.Header.ChunkSize
                            aviMain.dwMicroSecPerFrame = ioMain.ReadUInt32
                            aviMain.dwMaxBytesPerSec = ioMain.ReadUInt32
                            aviMain.dwPaddingGranularity = ioMain.ReadUInt32
                            aviMain.dwFlags = ioMain.ReadUInt32
                            aviMain.dwTotalFrames = ioMain.ReadUInt32
                            aviMain.dwInitialFrames = ioMain.ReadUInt32
                            aviMain.dwStreams = ioMain.ReadUInt32
                            aviMain.dwSuggestedBufferSize = ioMain.ReadUInt32
                            aviMain.dwWidth = ioMain.ReadUInt32
                            aviMain.dwHeight = ioMain.ReadUInt32
                            aviMain.dwReserved0 = ioMain.ReadUInt32
                            aviMain.dwReserved1 = ioMain.ReadUInt32
                            aviMain.dwReserved2 = ioMain.ReadUInt32
                            aviMain.dwReserved3 = ioMain.ReadUInt32
                            bValid = True
                          Case "LIST"
                            mainChunk.Header.Format = ioMain.ReadChars(4)
                            Select Case mainChunk.Header.Format
                              Case "strl"
                                Do While ioMain.BaseStream.Position < mainChunk.Header.ChunkSize
                                  Dim streamChunk As Chunk
                                  streamChunk.Header.ChunkID = ioMain.ReadChars(4)
                                  streamChunk.Header.ChunkSize = ioMain.ReadUInt32
                                  streamChunk.Data = ioMain.ReadBytes(streamChunk.Header.ChunkSize)
                                  Using ioStream As New IO.BinaryReader(New IO.MemoryStream(streamChunk.Data))
                                    Select Case streamChunk.Header.ChunkID
                                      Case "strh"
                                        streamChunk.Header.Format = ioStream.ReadChars(4)
                                        Dim streamHeader As New AVISTREAMHEADER
                                        streamHeader.fcc = streamChunk.Header.ChunkID
                                        streamHeader.cb = streamChunk.Header.ChunkSize
                                        streamHeader.fccType = streamChunk.Header.Format
                                        streamHeader.fccHandler = ioStream.ReadChars(4)
                                        streamHeader.dwFlags = ioStream.ReadUInt32
                                        streamHeader.wPriority = ioStream.ReadUInt16
                                        streamHeader.wLanguage = ioStream.ReadUInt16
                                        streamHeader.dwInitialFrames = ioStream.ReadUInt32
                                        streamHeader.dwScale = ioStream.ReadUInt32
                                        streamHeader.dwRate = ioStream.ReadUInt32
                                        streamHeader.dwStart = ioStream.ReadUInt32
                                        streamHeader.dwLength = ioStream.ReadUInt32
                                        streamHeader.dwSuggestedBufferSize = ioStream.ReadUInt32
                                        streamHeader.dwQuality = ioStream.ReadUInt32
                                        streamHeader.dwSampleSize = ioStream.ReadUInt32
                                        streamHeader.rcFrame.top = ioStream.ReadInt16
                                        streamHeader.rcFrame.left = ioStream.ReadInt16
                                        streamHeader.rcFrame.right = ioStream.ReadInt16
                                        streamHeader.rcFrame.bottom = ioStream.ReadInt16
                                        If aviStreams Is Nothing Then aviStreams = New List(Of AVISTREAMHEADER)
                                        aviStreams.Add(streamHeader)

                                        Select Case streamHeader.fccType
                                          Case "vids"
                                            Dim vidsChunk As Chunk
                                            vidsChunk.Header.ChunkID = ioMain.ReadChars(4)
                                            vidsChunk.Header.ChunkSize = ioMain.ReadUInt32
                                            vidsChunk.Data = ioMain.ReadBytes(vidsChunk.Header.ChunkSize)
                                            Using ioVids As New IO.BinaryReader(New IO.MemoryStream(vidsChunk.Data))
                                              Dim bmpInfo As New BITMAPINFO
                                              bmpInfo.bmiHeader.biSize = ioVids.ReadUInt32
                                              bmpInfo.bmiHeader.biWidth = ioVids.ReadUInt32
                                              bmpInfo.bmiHeader.biHeight = ioVids.ReadUInt32
                                              bmpInfo.bmiHeader.biPlanes = ioVids.ReadUInt16
                                              bmpInfo.bmiHeader.biBitCount = ioVids.ReadUInt16
                                              bmpInfo.bmiHeader.biCompression = ioVids.ReadUInt32
                                              bmpInfo.bmiHeader.biSizeImage = ioVids.ReadUInt32
                                              bmpInfo.bmiHeader.biXPelsPerMeter = ioVids.ReadUInt32
                                              bmpInfo.bmiHeader.biYPelsPerMeter = ioVids.ReadUInt32
                                              bmpInfo.bmiHeader.biClrUsed = ioVids.ReadUInt32
                                              bmpInfo.bmiHeader.biClrImportant = ioVids.ReadUInt32
                                              Select Case bmpInfo.bmiHeader.biBitCount
                                                Case 0 'specified or implied
                                                  Erase bmpInfo.bmiColors
                                                Case 1 'Monochrome
                                                  ReDim bmpInfo.bmiColors(1)
                                                  bmpInfo.bmiColors(0) = New RGBQUAD(ioVids.ReadBytes(4))
                                                  bmpInfo.bmiColors(1) = New RGBQUAD(ioVids.ReadBytes(4))
                                                Case 4 '16 Color
                                                  If bmpInfo.bmiHeader.biClrUsed = 0 Then
                                                    ReDim bmpInfo.bmiColors(15)
                                                  Else
                                                    ReDim bmpInfo.bmiColors(bmpInfo.bmiHeader.biClrUsed - 1)
                                                  End If
                                                  For I As Integer = 0 To bmpInfo.bmiColors.Length - 1
                                                    bmpInfo.bmiColors(I) = New RGBQUAD(ioVids.ReadBytes(4))
                                                  Next
                                                Case 8 '256 Color
                                                  If bmpInfo.bmiHeader.biClrUsed = 0 Then
                                                    ReDim bmpInfo.bmiColors(255)
                                                  Else
                                                    ReDim bmpInfo.bmiColors(bmpInfo.bmiHeader.biClrUsed - 1)
                                                  End If
                                                  For I As Integer = 0 To bmpInfo.bmiColors.Length - 1
                                                    bmpInfo.bmiColors(I) = New RGBQUAD(ioVids.ReadBytes(4))
                                                  Next
                                                Case 16 '16-bit color
                                                  If bmpInfo.bmiHeader.biClrUsed = 0 Then
                                                    Erase bmpInfo.bmiColors
                                                  Else
                                                    ReDim bmpInfo.bmiColors(bmpInfo.bmiHeader.biClrUsed - 1)
                                                    For I As Integer = 0 To bmpInfo.bmiColors.Length - 1
                                                      bmpInfo.bmiColors(I) = New RGBQUAD(ioVids.ReadBytes(4))
                                                    Next
                                                  End If
                                                Case 24
                                                  Erase bmpInfo.bmiColors
                                                Case 32 '32-bit color
                                                  If bmpInfo.bmiHeader.biClrUsed = 0 Then
                                                    Erase bmpInfo.bmiColors
                                                  Else
                                                    ReDim bmpInfo.bmiColors(bmpInfo.bmiHeader.biClrUsed - 1)
                                                    For I As Integer = 0 To bmpInfo.bmiColors.Length - 1
                                                      bmpInfo.bmiColors(I) = New RGBQUAD(ioVids.ReadBytes(4))
                                                    Next
                                                  End If
                                              End Select
                                              If aviBMP Is Nothing Then aviBMP = New List(Of BITMAPINFO)
                                              aviBMP.Add(bmpInfo)
                                            End Using
                                          Case "auds"
                                            Dim audsChunk As Chunk
                                            audsChunk.Header.ChunkID = ioMain.ReadChars(4)
                                            audsChunk.Header.ChunkSize = ioMain.ReadUInt32
                                            audsChunk.Data = ioMain.ReadBytes(audsChunk.Header.ChunkSize)
                                            Using ioAuds As New IO.BinaryReader(New IO.MemoryStream(audsChunk.Data))
                                              Dim wavInfo As New WAVEFORMATEX
                                              wavInfo.Format.wFormatTag = ioAuds.ReadUInt16
                                              wavInfo.Format.nChannels = ioAuds.ReadUInt16
                                              wavInfo.Format.nSamplesPerSec = ioAuds.ReadUInt32
                                              wavInfo.Format.nAvgBytesPerSec = ioAuds.ReadUInt32
                                              wavInfo.Format.nBlockAlign = ioAuds.ReadUInt16
                                              wavInfo.wBitsPerSample = ioAuds.ReadUInt16
                                              wavInfo.cbSize = ioAuds.ReadUInt16
                                              If aviWAV Is Nothing Then aviWAV = New List(Of WAVEFORMATEX)
                                              aviWAV.Add(wavInfo)
                                            End Using
                                          Case "mids"
                                            Dim midsChunk As Chunk
                                            midsChunk.Header.ChunkID = ioMain.ReadChars(4)
                                            midsChunk.Header.ChunkSize = ioMain.ReadUInt32
                                            ioMain.BaseStream.Position += midsChunk.Header.ChunkSize
                                          Case "txts"
                                            Dim txtsChunk As Chunk
                                            txtsChunk.Header.ChunkID = ioMain.ReadChars(4)
                                            txtsChunk.Header.ChunkSize = ioMain.ReadUInt32
                                            ioMain.BaseStream.Position += txtsChunk.Header.ChunkSize
                                          Case "JUNK"
                                            'Junk Data
                                            Dim junkChunk As Chunk
                                            junkChunk.Header.ChunkID = ioMain.ReadChars(4)
                                            junkChunk.Header.ChunkSize = ioMain.ReadUInt32
                                            ioMain.BaseStream.Position += junkChunk.Header.ChunkSize
                                          Case Else
                                            Debug.Print("Unknown AVI Stream Chunk ID: " & streamHeader.fccType)
                                            Dim unknownChunk As Chunk
                                            unknownChunk.Header.ChunkID = ioMain.ReadChars(4)
                                            unknownChunk.Header.ChunkSize = ioMain.ReadUInt32
                                            ioMain.BaseStream.Position += unknownChunk.Header.ChunkSize
                                        End Select
                                    End Select
                                  End Using
                                Loop
                              Case "odml"
                                Do While ioMain.BaseStream.Position < mainChunk.Header.ChunkSize
                                  Dim dmlChunk As Chunk
                                  dmlChunk.Header.ChunkID = ioMain.ReadChars(4)
                                  dmlChunk.Header.ChunkSize = ioMain.ReadUInt32
                                  dmlChunk.Data = ioMain.ReadBytes(dmlChunk.Header.ChunkSize)
                                  Using ioDML As New IO.BinaryReader(New IO.MemoryStream(dmlChunk.Data))
                                    Select Case dmlChunk.Header.ChunkID
                                      Case "dmlh"
                                        Dim Frames As UInt32 = ioDML.ReadUInt32
                                        Debug.Print("DML Frame Count: " & Frames)
                                      Case "JUNK"
                                        'Junk Data
                                      Case Else
                                        Debug.Print("Unknown Open DML Header Chunk ID: " & dmlChunk.Header.ChunkID)
                                    End Select
                                  End Using
                                Loop
                            End Select
                          Case "JUNK"
                            'Junk Data
                          Case Else
                            Debug.Print("Unknown AVI Header Chunk ID: " & mainChunk.Header.ChunkID)
                        End Select
                      End Using
                    Loop

                  Case "INFO"
                    If aviINFO Is Nothing Then aviINFO = New Dictionary(Of String, String)
                    Do While ioFile.BaseStream.Position - aviOffset < aviChunk.Header.ChunkSize
                      Dim infoChunk As Chunk
                      infoChunk.Header.ChunkID = ioFile.ReadChars(4)
                      infoChunk.Header.ChunkSize = ioFile.ReadUInt32
                      infoChunk.Header.Format = ioFile.ReadChars(infoChunk.Header.ChunkSize)
                      If infoChunk.Header.Format.Substring(infoChunk.Header.Format.Length - 1, 1) = vbNullChar Then infoChunk.Header.Format = infoChunk.Header.Format.Substring(0, infoChunk.Header.Format.Length - 1)
                      aviINFO.Add(infoChunk.Header.ChunkID, infoChunk.Header.Format)
                      If ioFile.BaseStream.Position - aviOffset = aviChunk.Header.ChunkSize - 1 Then ioFile.ReadByte()
                    Loop
                  Case "movi"
                    ioFile.BaseStream.Position += aviChunk.Header.ChunkSize - 4
                    'Do While ioFile.BaseStream.Position - aviOffset < aviChunk.Header.ChunkSize
                    '  Do Until ioFile.ReadByte > 0
                    '    If ioFile.BaseStream.Position - aviOffset >= aviChunk.Header.ChunkSize Then Exit Do
                    '  Loop
                    '  If ioFile.BaseStream.Position- aviOffset >= aviChunk.Header.ChunkSize Then Exit Do
                    '  ioFile.BaseStream.Position -= 1

                    '  Dim moviChunk As Chunk
                    '  moviChunk.Header.ChunkID = ioFile.ReadChars(4)
                    '  moviChunk.Header.ChunkSize = ioFile.ReadUInt32
                    '  If moviChunk.Header.ChunkSize > ioFile.BaseStream.Length - ioFile.BaseStream.Position Then Exit Do
                    '  moviChunk.Data = ioFile.ReadBytes(moviChunk.Header.ChunkSize)
                    '  'Dim code As String = moviChunk.Header.ChunkID.Substring(0, 2)
                    '  'Dim chunk As String = moviChunk.Header.ChunkID.Substring(2, 2)
                    'Loop
                  Case Else
                    Debug.Print("Unknown AVI Chunk Format: " & aviChunk.Header.Format)
                End Select

              Case "idx1"
                'Old Index
                aviINDEX = New AVIOLDINDEX
                aviINDEX.fcc = aviChunk.Header.ChunkID
                aviINDEX.cb = aviChunk.Header.ChunkSize
                Dim items As Integer = aviINDEX.cb / 16
                ReDim aviINDEX.aIndex(items - 1)
                For I As Integer = 0 To items - 1
                  aviINDEX.aIndex(I).dwChunkID = ioFile.ReadUInt32
                  aviINDEX.aIndex(I).dwFlags = ioFile.ReadUInt32
                  aviINDEX.aIndex(I).dwOffset = ioFile.ReadUInt32
                  aviINDEX.aIndex(I).dwSize = ioFile.ReadUInt32
                Next
              Case "indx"
                'OpenDML Index
                Stop
                ioFile.BaseStream.Position += aviChunk.Header.ChunkSize
              Case "JUNK"
                'Junk Data
                ioFile.BaseStream.Position += aviChunk.Header.ChunkSize
              Case Else
                Debug.Print("Unknown AVI Chunk ID: " & aviChunk.Header.ChunkID)
                ioFile.BaseStream.Position += aviChunk.Header.ChunkSize
            End Select
            'End Using
          Loop
        Case Else
          Debug.Print("Unknown RIFF Format: " & mChunk.Header.Format)
          Exit Sub
      End Select
      If ioFile.BaseStream.Position < ioFile.BaseStream.Length Then
        Dim idvxChunk As Chunk
        idvxChunk.Header.ChunkID = ioFile.ReadChars(4)
        idvxChunk.Header.ChunkSize = ioFile.ReadUInt32
        idvxChunk.Data = ioFile.ReadBytes(idvxChunk.Header.ChunkSize)
        Using ioIDVX As New IO.BinaryReader(New IO.MemoryStream(idvxChunk.Data))
          If idvxChunk.Header.ChunkID = "IDVX" Then
            aviIDVX = New IDVX_INFO
            aviIDVX.Movie = Trim(ioIDVX.ReadChars(32))
            aviIDVX.Author = Trim(ioIDVX.ReadChars(28))
            aviIDVX.Year = Trim(ioIDVX.ReadChars(4))
            aviIDVX.Comment = Trim(ioIDVX.ReadChars(48))
            aviIDVX.Genre = Val(Trim(ioIDVX.ReadChars(3)))
            aviIDVX.Rating = ioIDVX.ReadByte
            aviIDVX.Extra = ioIDVX.ReadBytes(5)
            aviIDVX.FileID = ioIDVX.ReadChars(7)
            If Not aviIDVX.FileID = "DIVXTAG" Then aviIDVX = Nothing
          End If
        End Using
      End If
    End Using
  End Sub

  Public ReadOnly Property IsValid As Boolean
    Get
      Return bValid
    End Get
  End Property

  Public ReadOnly Property IsDTS As Boolean
    Get
      Return bDTS
    End Get
  End Property

  Public ReadOnly Property IsWAV As Boolean
    Get
      Return bWAV
    End Get
  End Property

  Public ReadOnly Property IsAVI As Boolean
    Get
      Return bAVI
    End Get
  End Property

  Private Function BytesTo14BitL(inBytes() As Byte) As Byte()
    Dim bitPairs() As Byte = Nothing
    Dim j As Integer = 0
    For I As Integer = 0 To inBytes.Count - 1 Step 2
      Dim b1 As Byte = inBytes(I)
      Dim b0 As Byte = inBytes(I + 1)
      ReDim Preserve bitPairs(j + 6)
      bitPairs(j) = (b0 And &H30) >> 4
      bitPairs(j + 1) = (b0 And &HC) >> 2
      bitPairs(j + 2) = (b0 And &H3)
      bitPairs(j + 3) = (b1 And &HC0) >> 6
      bitPairs(j + 4) = (b1 And &H30) >> 4
      bitPairs(j + 5) = (b1 And &HC) >> 2
      bitPairs(j + 6) = (b1 And &H3)
      j += 7
    Next
    Dim bytes(bitPairs.Count / 4 - 1) As Byte
    j = 0
    For I As Integer = 0 To bitPairs.Count - 1 Step 4
      bytes(j) = (bitPairs(I) << 6) + (bitPairs(I + 1) << 4) + (bitPairs(I + 2) << 2) + (bitPairs(I + 3))
      j += 1
    Next
    Return bytes
  End Function

  Private sizeLeft As Integer
  Private currentByte As Byte
  Private idx As Integer
  Private Function ReadBits(bData() As Byte, size As Integer) As UInt32
    Dim ret As UInt32 = 0
    If (size <= sizeLeft) Then
      sizeLeft -= size
      ret = (currentByte >> sizeLeft) And (Math.Pow(2, size) - 1)
    Else
      Dim oSize As Integer = sizeLeft
      ret = ReadBits(bData, sizeLeft) << size - oSize
      ret = ret Or ReadBits(bData, size - oSize)
    End If
    If sizeLeft = 0 Then
      idx += 1
      currentByte = bData(idx)
      sizeLeft = 8
    End If
    Return ret
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

Public Class clsMPEG
  Private MPEGHeader(32) As Boolean
  Public Sub New(lHeader As Long)
    MPEGHeader(0) = False
    MPEGHeader(1) = CBool(lHeader And &H80000000)
    MPEGHeader(2) = CBool(lHeader And &H40000000)
    MPEGHeader(3) = CBool(lHeader And &H20000000)
    MPEGHeader(4) = CBool(lHeader And &H10000000)
    MPEGHeader(5) = CBool(lHeader And &H8000000)
    MPEGHeader(6) = CBool(lHeader And &H4000000)
    MPEGHeader(7) = CBool(lHeader And &H2000000)
    MPEGHeader(8) = CBool(lHeader And &H1000000)
    MPEGHeader(9) = CBool(lHeader And &H800000)
    MPEGHeader(10) = CBool(lHeader And &H400000)
    MPEGHeader(11) = CBool(lHeader And &H200000)
    MPEGHeader(12) = CBool(lHeader And &H100000)
    MPEGHeader(13) = CBool(lHeader And &H80000)
    MPEGHeader(14) = CBool(lHeader And &H40000)
    MPEGHeader(15) = CBool(lHeader And &H20000)
    MPEGHeader(16) = CBool(lHeader And &H10000)
    MPEGHeader(17) = CBool(lHeader And &H8000&)
    MPEGHeader(18) = CBool(lHeader And &H4000)
    MPEGHeader(19) = CBool(lHeader And &H2000)
    MPEGHeader(20) = CBool(lHeader And &H1000)
    MPEGHeader(21) = CBool(lHeader And &H800)
    MPEGHeader(22) = CBool(lHeader And &H400)
    MPEGHeader(23) = CBool(lHeader And &H200)
    MPEGHeader(24) = CBool(lHeader And &H100)
    MPEGHeader(25) = CBool(lHeader And &H80)
    MPEGHeader(26) = CBool(lHeader And &H40)
    MPEGHeader(27) = CBool(lHeader And &H20)
    MPEGHeader(28) = CBool(lHeader And &H10)
    MPEGHeader(29) = CBool(lHeader And &H8)
    MPEGHeader(30) = CBool(lHeader And &H4)
    MPEGHeader(31) = CBool(lHeader And &H2)
    MPEGHeader(32) = CBool(lHeader And &H1)
  End Sub
  Public Function CheckValidity() As Boolean
    If CheckSync() Then
      If Not GetMPEGVer() = 0 Then
        If Not GetMPEGLayer() = 0 Then
          If Not GetBitrate() = 0 Then
            If CheckLIIChannel() Then
              If GetFrameSize() > 20 Then
                Return True
              End If
            End If
          End If
        End If
      End If
    End If
    Return False
  End Function
  Private Function CheckLIIChannel() As Boolean
    If GetMPEGLayer() = 2 Then
      Select Case GetBitrate()
        Case 32000 : Return GetChannels() = "Single Channel"
        Case 48000 : Return GetChannels() = "Single Channel"
        Case 56000 : Return GetChannels() = "Single Channel"
        Case 80000 : Return GetChannels() = "Single Channel"
        Case 224000 : Return Not (GetChannels() = "Single Channel")
        Case 256000 : Return Not (GetChannels() = "Single Channel")
        Case 320000 : Return Not (GetChannels() = "Single Channel")
        Case 384000 : Return Not (GetChannels() = "Single Channel")
        Case Else : Return True
      End Select
    Else
      Return True
    End If
  End Function
  Private Function CheckSync() As Boolean
    Dim I As Integer
    For I = 1 To 11
      If Not MPEGHeader(I) Then
        Return False
      End If
    Next I
    Return True
  End Function
  '0 = Reserverd
  '1 = MPEG Version 1 (ISO/IEC 11172-3)
  '2 = MPEG Version 2 (ISO/IEC 13818-3)
  '3 = MPEG Version 2.5
  Public Function GetMPEGVer() As Byte
    If Not MPEGHeader(12) And Not MPEGHeader(13) Then
      Return 3
    ElseIf Not MPEGHeader(12) And MPEGHeader(13) Then
      Return 0
    ElseIf MPEGHeader(12) And Not MPEGHeader(13) Then
      Return 2
    ElseIf MPEGHeader(12) And MPEGHeader(13) Then
      Return 1
    End If
    Return 0
  End Function
  '0 = Reserved
  '1 = Layer I
  '2 = Layer II
  '3 = Layer III
  Public Function GetMPEGLayer() As Byte
    If Not MPEGHeader(14) And Not MPEGHeader(15) Then
      Return 0
    ElseIf Not MPEGHeader(14) And MPEGHeader(15) Then
      Return 3
    ElseIf MPEGHeader(14) And Not MPEGHeader(15) Then
      Return 2
    ElseIf MPEGHeader(14) And MPEGHeader(15) Then
      Return 1
    End If
    Return 0
  End Function
  Public Function GetProtected() As Boolean
    Return Not MPEGHeader(16)
  End Function
  Public Function GetBitrate() As Long
    Select Case GetMPEGVer()
      Case 1
        Select Case GetMPEGLayer()
          Case 1
            If Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 32000
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 64000
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 96000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 128000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 160000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 192000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 224000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 256000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 288000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 320000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 352000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 384000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 416000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 488000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            End If
          Case 2
            If Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 32000
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 48000
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 56000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 64000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 80000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 96000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 112000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 128000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 160000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 192000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 224000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 256000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 320000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 384000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            End If
          Case 3
            If Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 32000
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 40000
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 48000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 56000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 64000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 80000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 96000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 112000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 128000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 160000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 192000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 224000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 256000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 320000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            End If
        End Select
      Case 2, 3
        Select Case GetMPEGLayer()
          Case 1
            If Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 32000
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 48000
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 56000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 64000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 80000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 96000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 112000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 128000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 144000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 160000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 176000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 192000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 224000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 256000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            End If
          Case 2, 3
            If Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 8000
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 16000
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 24000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 32000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 40000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 48000
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 56000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 64000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 80000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 96000
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 112000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 128000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 144000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 160000
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            End If
        End Select
    End Select
    Return 0
  End Function
  Public Function GetBitQual() As Long
    Select Case GetMPEGVer()
      Case 1
        Select Case GetMPEGLayer()
          Case 1
            If Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 0
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 2
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 2
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 3
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 4
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 5
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 6
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 7
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 7
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 8
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 8
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 9
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            End If
          Case 2
            If Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 0
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 2
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 2
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 3
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 4
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 5
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 6
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 7
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 7
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 8
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 8
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 9
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            End If
          Case 3
            If Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 0
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 2
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 2
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 3
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 4
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 5
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 6
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 7
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 7
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 8
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 8
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 9
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            End If
        End Select
      Case 2, 3
        Select Case GetMPEGLayer()
          Case 1
            If Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 0
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 2
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 2
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 3
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 4
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 5
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 6
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 7
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 7
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 8
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 8
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 9
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            End If
          Case 2, 3
            If Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 0
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 1
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 2
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 2
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 3
            ElseIf Not MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 4
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 5
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 6
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 7
            ElseIf MPEGHeader(17) And Not MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 7
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 8
            ElseIf MPEGHeader(17) And MPEGHeader(18) And Not MPEGHeader(19) And MPEGHeader(20) Then
              Return 8
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And Not MPEGHeader(20) Then
              Return 9
            ElseIf MPEGHeader(17) And MPEGHeader(18) And MPEGHeader(19) And MPEGHeader(20) Then
              Return 0
            End If
        End Select
    End Select
    Return 0
  End Function
  Public Function GetSampleRate() As Long
    Select Case GetMPEGVer()
      Case 1
        If Not MPEGHeader(21) And Not MPEGHeader(22) Then
          Return 44100
        ElseIf Not MPEGHeader(21) And MPEGHeader(22) Then
          Return 48000
        ElseIf MPEGHeader(21) And Not MPEGHeader(22) Then
          Return 32000
        Else
          Return 0
        End If
      Case 2
        If Not MPEGHeader(21) And Not MPEGHeader(22) Then
          Return 22050
        ElseIf Not MPEGHeader(21) And MPEGHeader(22) Then
          Return 24000
        ElseIf MPEGHeader(21) And Not MPEGHeader(22) Then
          Return 16000
        Else
          Return 0
        End If
      Case 3
        If Not MPEGHeader(21) And Not MPEGHeader(22) Then
          Return 11025
        ElseIf Not MPEGHeader(21) And MPEGHeader(22) Then
          Return 12000
        ElseIf MPEGHeader(21) And Not MPEGHeader(22) Then
          Return 8000
        Else
          Return 0
        End If
      Case Else
        Return 0
    End Select
  End Function
  Public Function GetPadding() As Long
    If MPEGHeader(23) Then
      Select Case GetMPEGLayer()
        Case 1 : Return 4
        Case 2, 3 : Return 1
        Case Else : Return 0
      End Select
    Else
      Return 0
    End If
  End Function
  Public Function GetFrameSize() As Long
    Dim lMult As Long
    If GetSampleRate() = 0 Then
      Return 0
    ElseIf GetBitrate() < 2 Then
      Return 0
    Else
      Select Case GetMPEGVer()
        Case 1
          Select Case GetMPEGLayer()
            Case 1 : lMult = 48
            Case 2, 3 : lMult = 144
          End Select
        Case 2, 3
          Select Case GetMPEGLayer()
            Case 1 : lMult = 48
            Case 2 : lMult = 144
            Case 3 : lMult = 72
          End Select
      End Select
      If lMult > 0 Then Return Int(lMult * GetBitrate() / GetSampleRate() + GetPadding())
    End If
    Return 0
  End Function
  Public Function GetPrivateBit() As Boolean
    Return MPEGHeader(24)
  End Function
  Public Function GetChannels() As String
    If Not MPEGHeader(25) And Not MPEGHeader(26) Then
      Return "Stereo"
    ElseIf Not MPEGHeader(25) And MPEGHeader(26) Then
      Return "Joint Stereo"
    ElseIf MPEGHeader(25) And Not MPEGHeader(26) Then
      Return "Dual Channel"
    ElseIf MPEGHeader(25) And MPEGHeader(26) Then
      Return "Single Channel"
    End If
    Return "Unknown"
  End Function
  Public Function GetModeExtension() As String
    If GetChannels() = "Joint Stereo" Then
      Select Case GetMPEGLayer()
        Case 1, 2
          If Not MPEGHeader(27) And Not MPEGHeader(28) Then
            Return "4-31"
          ElseIf Not MPEGHeader(27) And MPEGHeader(28) Then
            Return "8-31"
          ElseIf MPEGHeader(27) And Not MPEGHeader(28) Then
            Return "12-31"
          ElseIf MPEGHeader(27) And MPEGHeader(28) Then
            Return "16-31"
          End If
        Case 3
          If Not MPEGHeader(27) And Not MPEGHeader(28) Then
            Return "None"
          ElseIf Not MPEGHeader(27) And MPEGHeader(28) Then
            Return "Intensity"
          ElseIf MPEGHeader(27) And Not MPEGHeader(28) Then
            Return "M/S"
          ElseIf MPEGHeader(27) And MPEGHeader(28) Then
            Return "Intensity & M/S"
          End If
      End Select
    End If
    Return vbNullString
  End Function
  Public Function GetCopyright() As Boolean
    Return MPEGHeader(29)
  End Function
  Public Function GetOriginal() As Boolean
    Return MPEGHeader(30)
  End Function
  Public Function GetEmphasis() As String
    If Not MPEGHeader(31) And Not MPEGHeader(32) Then
      Return "None"
    ElseIf Not MPEGHeader(31) And MPEGHeader(32) Then
      Return "50/15 ms"
    ElseIf MPEGHeader(31) And Not MPEGHeader(32) Then
      Return vbNullString
    ElseIf MPEGHeader(31) And MPEGHeader(32) Then
      Return "CCIT J.17"
    End If
    Return vbNullString
  End Function
End Class

Public Class clsVBRI
  Private Structure VBRIHeader
    Public HeaderID As String  ' String ' * 4 '4 Bytes
    Public Version As Integer    '2 Bytes
    Public Delay As Integer     '2 Bytes
    Public Quality As Integer    '2 Bytes
    Public Bytes As Integer       '4 Bytes
    Public Frames As Integer       '4 Bytes
    Public ToCCount As Integer    '2 Bytes
    Public ToCScale As Integer    '2 Bytes
    Public BPTable As Integer    '2 Bytes
    Public FPTable As Integer    '2 Bytes
    Public ToC() As String     'Redim ToC(ToCCount) as String; ToC(X) = String$(BPTable, 0)
  End Structure
  Private vHeader As VBRIHeader
  Public Sub New(bFrame() As Byte, Optional ByVal Start As Integer = 0)
    Dim I As Integer
    Dim lPos As Long
    lPos = 4 + 32
    vHeader.HeaderID = GetString(bFrame, lPos, 4) ' Mid$(sFrame, lPos, 4)
    If vHeader.HeaderID <> "VBRI" Then
      vHeader.HeaderID = vbNullString
      Exit Sub
    End If
    lPos += 4
    vHeader.Version = GetWORD(bFrame, lPos)
    lPos += 2
    vHeader.Delay = GetWORD(bFrame, lPos)
    lPos += 2
    vHeader.Quality = GetWORD(bFrame, lPos)
    lPos += 2
    vHeader.Bytes = GetDWORD(bFrame, lPos)
    lPos += 4
    vHeader.Frames = GetDWORD(bFrame, lPos)
    lPos += 4
    vHeader.ToCCount = GetDWORD(bFrame, lPos)
    lPos += 2
    vHeader.ToCScale = GetDWORD(bFrame, lPos)
    lPos += 2
    vHeader.BPTable = GetDWORD(bFrame, lPos)
    lPos += 2
    vHeader.FPTable = GetDWORD(bFrame, lPos)
    lPos += 2
    ReDim vHeader.ToC(vHeader.ToCCount)
    For I = 1 To vHeader.ToCCount
      vHeader.ToC(I) = GetString(bFrame, lPos, vHeader.BPTable)
      lPos += vHeader.BPTable
    Next I
  End Sub
  Public ReadOnly Property HeaderID() As String
    Get
      Return vHeader.HeaderID
    End Get
  End Property
  Public ReadOnly Property Version() As Integer
    Get
      Return vHeader.Version
    End Get
  End Property
  Public ReadOnly Property Delay() As Integer
    Get
      Return vHeader.Delay
    End Get
  End Property
  Public ReadOnly Property Quality() As Integer
    Get
      Return vHeader.Quality
    End Get
  End Property
  Public ReadOnly Property Bytes() As Long
    Get
      Return vHeader.Bytes
    End Get
  End Property
  Public ReadOnly Property Frames() As Long
    Get
      Return vHeader.Frames
    End Get
  End Property
  Public ReadOnly Property ToCCount() As Integer
    Get
      Return vHeader.ToCCount
    End Get
  End Property
  Public ReadOnly Property ToCScale() As Integer
    Get
      Return vHeader.ToCScale
    End Get
  End Property
  Public ReadOnly Property BPTable() As Integer
    Get
      Return vHeader.BPTable
    End Get
  End Property
  Public ReadOnly Property FPTable() As Integer
    Get
      Return vHeader.FPTable
    End Get
  End Property
  Public ReadOnly Property ToC(Index As Integer) As String
    Get
      If Index > 0 And Index < 101 Then
        Return vHeader.ToC(Index)
      Else
        Return String.Empty
      End If
    End Get
  End Property
End Class

Public Class clsXING
  Private Structure XINGHeader
    Public headerID As String
    Public FieldFlags As Long
    Public Frames As Long
    Public Bytes As Long
    Public ToC() As Byte
    Public Quality As Long
  End Structure
  Private Structure XINGExtend
    Public EncoderVStr As String
    Public InfoTagRev As Byte
    Public LowpassFliter As Byte
    Public PeakAmplitude As Long
    Public RadioGain As Integer
    Public IdealGain As Integer
    Public EncodFlags As Byte
    Public Bitrate As Byte
    Public EncoderDelay As Long
    Public Misc As Byte
    Public MP3Gain As Byte
    Public PresetFlags As Integer
    Public MusicLength As Long
    Public CRC As Integer
    Public InfoCRC As Integer
  End Structure
  Private Const FIELD_FRAMES As Long = &H1
  Private Const FIELD_BYTES As Long = &H2
  Private Const FIELD_TOC As Long = &H4
  Private Const FIELD_QUALITY As Long = &H8
  Private xHeader As XINGHeader
  Private xExtend As XINGExtend

  Public Sub New(bFrame() As Byte, ByRef cMPEG As clsMPEG)
    Dim I As Integer
    Dim lStrt As Long
    Dim lPos As Long
    If cMPEG.GetMPEGVer = 1 Then
      If cMPEG.GetChannels = "Single Channel" Then
        lStrt = 17 + 4
      Else
        lStrt = 32 + 4
      End If
    Else
      If cMPEG.GetChannels = "Single Channel" Then
        lStrt = 9 + 4
      Else
        lStrt = 17 + 4
      End If
    End If
    lPos = lStrt
    xHeader.headerID = GetString(bFrame, lPos, 4)
    If xHeader.headerID <> "Xing" And xHeader.headerID <> "Info" Then
      xHeader.headerID = vbNullString
      Exit Sub
    End If
    lPos += 4
    xHeader.FieldFlags = GetDWORD(bFrame, lPos)
    lPos += 4
    If CBool(xHeader.FieldFlags And FIELD_FRAMES) Then
      xHeader.Frames = GetDWORD(bFrame, lPos)
      lPos += 4
    End If
    If CBool(xHeader.FieldFlags And FIELD_BYTES) Then
      xHeader.Bytes = GetDWORD(bFrame, lPos)
      lPos += 4
    End If
    If CBool(xHeader.FieldFlags And FIELD_TOC) Then
      ReDim xHeader.ToC(99)
      For I = 0 To 99
        xHeader.ToC(I) = bFrame(lPos + I) ' Asc(Mid$(sFrame, lPos + I, 1))
      Next I
      lPos += 100
    End If
    If CBool(xHeader.FieldFlags And FIELD_QUALITY) Then
      xHeader.Quality = GetDWORD(bFrame, lPos)
      lPos += 4
    End If
    xExtend.EncoderVStr = GetString(bFrame, lPos, 9)
    If Replace(Trim(xExtend.EncoderVStr), vbNullChar, vbNullString) = vbNullString Then Exit Sub
    lPos += 9
    xExtend.InfoTagRev = bFrame(lPos)
    lPos += 1
    xExtend.LowpassFliter = bFrame(lPos)
    lPos += 1
    xExtend.PeakAmplitude = GetDWORD(bFrame, lPos)
    lPos += 4
    xExtend.RadioGain = GetWORD(bFrame, lPos)
    lPos += 2
    xExtend.IdealGain = GetWORD(bFrame, lPos)
    lPos += 2
    xExtend.EncodFlags = bFrame(lPos)
    lPos += 1
    xExtend.Bitrate = bFrame(lPos)
    lPos += 1
    xExtend.EncoderDelay = CLng("&H00" & BufferHex(bFrame(lPos)) & BufferHex(bFrame(lPos + 1)) & BufferHex(bFrame(lPos + 2)))
    lPos += 3
    xExtend.Misc = bFrame(lPos)
    lPos += 1
    xExtend.MP3Gain = bFrame(lPos)
    lPos += 1
    xExtend.PresetFlags = GetWORD(bFrame, lPos)
    lPos += 2
    xExtend.MusicLength = GetDWORD(bFrame, lPos)
    lPos += 4
    xExtend.CRC = GetWORD(bFrame, lPos)
    lPos += 2
    xExtend.InfoCRC = GetWORD(bFrame, lPos)
    lPos += 2
  End Sub
  Public ReadOnly Property HeaderID() As String
    Get
      Return xHeader.headerID
    End Get
  End Property
  Public ReadOnly Property FrameCount() As Long
    Get
      Return xHeader.Frames
    End Get
  End Property
  Public ReadOnly Property ByteCount() As Long
    Get
      Return xHeader.Bytes
    End Get
  End Property
  Public ReadOnly Property ToC(Index As Integer) As Byte
    Get
      If Index > 0 And Index < 101 Then
        Return xHeader.ToC(Index - 1)
      Else
        Return 0
      End If
    End Get
  End Property
  Public ReadOnly Property Quality() As Long
    Get
      Return xHeader.Quality
    End Get

  End Property
  Public ReadOnly Property EncoderVersion() As String
    Get
      Return xExtend.EncoderVStr
    End Get
  End Property
  Public ReadOnly Property InfoTagRevision() As String
    Get
      If (xExtend.InfoTagRev And &HF0) \ &H10 = 15 Then
        Return vbNullString
      Else
        Return "r" & Trim(Str((xExtend.InfoTagRev And &HF0) \ &H10))
      End If
    End Get
  End Property
  Public ReadOnly Property VBRMethod() As String
    Get
      Select Case xExtend.InfoTagRev And &HF
        Case 1 : Return "CBR"
        Case 2 : Return "ABR"
        Case 3 : Return "VBR rh"
        Case 4 : Return "VBR mt/rh"
        Case 5 : Return "VBR mt"
        Case 8 : Return "2-Pass CBR"
        Case 9 : Return "2-Pass ABR"
        Case 15 : Return vbNullString
        Case Else : Return "Full VBR Method " & Trim(Str(xExtend.InfoTagRev And &HF))
      End Select
    End Get
  End Property
  Public ReadOnly Property LowpassFilter() As Long
    Get
      Return xExtend.LowpassFliter * 100
    End Get
  End Property
  Public ReadOnly Property PeakSignalAmplitude() As String
    Get
      Return Trim(Str(xExtend.PeakAmplitude / 8388608)) & "%"
    End Get
  End Property
  Public ReadOnly Property RadioReplayGain() As String
    Get
      Dim sName As String
      Dim sOrig As String
      Dim bSign As Boolean
      Dim lGain As Long
      'a: E000
      'b: 1C00
      'c:  200
      'd:  1FF
      'First 3 bits determine Adjustment Name
      'Next 3 bits determine Originator (Who set the adjustment)
      'One bit for +/-
      'Last 9 bits for adjustment (divide by 10 for decimal value)
      Select Case ((xExtend.RadioGain And &HE000) \ 2 ^ 13)
        Case 0 : sName = "Not Set"
        Case 1 : sName = "Radio"
        Case 2 : sName = "Audiophile"
        Case Else : sName = "Unknown"
      End Select
      Select Case ((xExtend.RadioGain And &H1C00) \ 2 ^ 10)
        Case 0 : sOrig = "Not Set"
        Case 1 : sOrig = "Artist"
        Case 2 : sOrig = "User"
        Case 3 : sOrig = "Model"
        Case 4 : sOrig = "RMS Average"
        Case Else : sOrig = "Unknown"
      End Select
      bSign = ((xExtend.RadioGain And &H200) \ 2 ^ 9) = 1
      lGain = (xExtend.RadioGain And &H1FF)
      If sName = "Not Set" And sOrig = "Not Set" And lGain = 0 Then
        Return "Not Set"
      Else
        Return sName & " gain set by " & sOrig & " to " & IIf(bSign, "-", "+") & Trim(Str(lGain / 10))
      End If
    End Get
  End Property
  Public ReadOnly Property AudiophileReplayGain() As String
    Get
      Dim sName As String
      Dim sOrig As String
      Dim bSign As Boolean
      Dim lGain As Long
      Select Case ((xExtend.IdealGain And &HE000) \ 2 ^ 13)
        Case 0 : sName = "Not Set"
        Case 1 : sName = "Radio"
        Case 2 : sName = "Audiophile"
        Case Else : sName = "Unknown"
      End Select
      Select Case ((xExtend.IdealGain And &H1C00) \ 2 ^ 10)
        Case 0 : sOrig = "Not Set"
        Case 1 : sOrig = "Artist"
        Case 2 : sOrig = "User"
        Case 3 : sOrig = "Model"
        Case 4 : sOrig = "RMS Average"
        Case Else : sOrig = "Unknown"
      End Select
      bSign = ((xExtend.IdealGain And &H200) \ 2 ^ 9) = 1
      lGain = CBool(xExtend.IdealGain And &H1FF)
      If sName = "Not Set" And sOrig = "Not Set" And lGain = 0 Then
        Return "Not Set"
      Else
        Return sName & " gain set by " & sOrig & " to " & IIf(bSign, "-", "+") & Trim(Str(lGain / 10))
      End If
    End Get
  End Property
  Public ReadOnly Property EncodingFlags() As String
    Get
      Dim bNSPSYTUNE As Boolean = CBool(xExtend.EncodFlags And &H80)
      Dim bNSSAFEJOINT As Boolean = CBool(xExtend.EncodFlags And &H40)
      Dim bNOGAPtoNEXT As Boolean = CBool(xExtend.EncodFlags And &H20)
      Dim bNOGAPtoLAST As Boolean = CBool(xExtend.EncodFlags And &H10)
      EncodingFlags = Trim(IIf(bNSPSYTUNE, "--nspsytune", vbNullString) & " " & _
                           IIf(bNSSAFEJOINT, "--nssafejoint", vbNullString) & " " & _
                           IIf(bNOGAPtoNEXT, "--nogap (to next)", vbNullString) & " " & _
                           IIf(bNOGAPtoLAST, "--nogap (from last)", vbNullString))
    End Get
  End Property
  Public ReadOnly Property ATHType() As Byte
    Get
      Return (xExtend.EncodFlags And &HFF)
    End Get
  End Property
  Public ReadOnly Property MinimalBitrate() As Byte
    Get
      Return xExtend.Bitrate
    End Get
  End Property
  Public ReadOnly Property StartEncoderDelay() As Integer
    Get
      Return ((xExtend.EncoderDelay And &HFFF000) \ 2 ^ 12)
    End Get
  End Property
  Public ReadOnly Property EndEncoderDelay() As Integer
    Get
      Return (xExtend.EncoderDelay And &HFFF)
    End Get
  End Property
  Public ReadOnly Property NoiseSharpening() As Byte
    Get
      Return ((xExtend.Misc And &HC0) \ 2 ^ 6)
    End Get
  End Property
  Public ReadOnly Property StereoMode() As String
    Get
      Select Case ((xExtend.Misc And &H38) \ 2 ^ 3)
        Case 0 : Return "Mono"
        Case 1 : Return "Stereo"
        Case 2 : Return "Dual"
        Case 3 : Return "Joint"
        Case 4 : Return "Force"
        Case 5 : Return "Auto"
        Case 6 : Return "Intensity"
        Case Else : Return "Different"
      End Select
    End Get
  End Property
  Public ReadOnly Property UnwiseSetting() As Boolean
    Get
      Return CBool(((xExtend.Misc And &H4) \ 2 ^ 2) = 1)
    End Get
  End Property
  Public ReadOnly Property SourceFrequency() As String
    Get
      Select Case (xExtend.Misc And &H3)
        Case 0 : Return "<=32000"
        Case 1 : Return "44100"
        Case 2 : Return "48000"
        Case Else : Return ">48000"
      End Select
    End Get
  End Property
  Public ReadOnly Property MP3Gain() As Single
    Get
      If xExtend.MP3Gain > 128 Then
        Return (128 - xExtend.MP3Gain) * (3 / 2)
      ElseIf xExtend.MP3Gain < 128 Then
        Return xExtend.MP3Gain * (3 / 2)
      Else
        Return 0
      End If
    End Get
  End Property
  Public ReadOnly Property SurroundInfo() As String
    Get
      Select Case ((xExtend.PresetFlags And &H3800) \ 2 ^ 11)
        Case 0 : Return "No Information"
        Case 1 : Return "DPL Encoding"
        Case 2 : Return "DPL2 Encoding"
        Case 3 : Return "Ambisonic Encoding"
        Case 8 : Return "Reserved"
        Case Else : Return "Unknown"
      End Select
    End Get
  End Property
  Public ReadOnly Property PresetUsed() As Integer
    Get
      Return (xExtend.PresetFlags And &H7FF)
    End Get
  End Property
  Public ReadOnly Property MusicLength() As Long
    Get
      Return xExtend.MusicLength
    End Get
  End Property
  Public ReadOnly Property MusicCRC() As Integer
    Get
      Return xExtend.CRC
    End Get
  End Property
  Public ReadOnly Property InfoTagCRC() As Integer
    Get
      Return xExtend.InfoCRC
    End Get
  End Property
End Class

Public Class clsID3v1
  Implements IDisposable
  Private m_sMp3File As String
  Private m_HasTag As Boolean
  Private m_sTitle As String
  Private m_sArtist As String
  Private m_sAlbum As String
  Private m_sYear As String
  Private m_sComment As String
  Private m_bGenre As Byte
  Private m_bTrack As Byte

  Public Sub New(MP3File As String)
    m_sMp3File = MP3File
    LoadID3v1()
  End Sub

  Public Property MP3File() As String
    Get
      Return m_sMp3File
    End Get
    Set(value As String)
      m_sMp3File = value
      LoadID3v1()
    End Set
  End Property

  Public ReadOnly Property HasID3v1Tag() As Boolean
    Get
      Return m_HasTag
    End Get
  End Property

  Private Sub LoadID3v1()
    Try
      If Not My.Computer.FileSystem.FileExists(m_sMp3File) Then Exit Sub
      m_HasTag = False
      m_bTrack = 0
      m_sTitle = vbNullString
      m_sArtist = vbNullString
      m_sAlbum = vbNullString
      m_sYear = vbNullString
      m_bGenre = &HFF
      m_sComment = vbNullString
      Using bR As New IO.BinaryReader(New IO.FileStream(m_sMp3File, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read), fileEncoding)
        If bR.BaseStream.Length > &H80 Then
          bR.BaseStream.Position = bR.BaseStream.Length - &H80
          If bR.ReadChars(3) = "TAG" Then
            m_HasTag = True
            m_sTitle = TrimNull(bR.ReadChars(30))
            m_sArtist = TrimNull(bR.ReadChars(30))
            m_sAlbum = TrimNull(bR.ReadChars(30))
            m_sYear = TrimNull(bR.ReadChars(4))
            m_sComment = bR.ReadChars(28)
            If bR.PeekChar = 0 Then
              bR.ReadByte()
              m_bTrack = bR.ReadByte
            Else
              m_sComment &= bR.ReadChars(2)
            End If
            m_sComment = TrimNull(m_sComment)
            m_bGenre = bR.ReadByte
          End If
        End If
        bR.Close()
      End Using
    Catch ex As Exception
      m_HasTag = False
      Debug.Print("ID3v1 Read error: " & ex.Message)
    End Try
  End Sub

  Public Sub Save(Optional ByVal SaveAs As String = Nothing)
    If String.IsNullOrEmpty(SaveAs) Then SaveAs = m_sMp3File
    Dim bTrim As Boolean = False
    Using bR As New IO.BinaryReader(New IO.FileStream(m_sMp3File, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read), fileEncoding)
      If bR.BaseStream.Length > &H80 Then
        bR.BaseStream.Position = bR.BaseStream.Length - &H80
        If bR.ReadChars(3) = "TAG" Then bTrim = True
      End If
      bR.Close()
    End Using
    If String.IsNullOrEmpty(m_sTitle) And String.IsNullOrEmpty(m_sArtist) And String.IsNullOrEmpty(m_sAlbum) And String.IsNullOrEmpty(m_sYear) And String.IsNullOrEmpty(m_sComment) And m_bGenre = &HC And m_bTrack = 0 Then
      If bTrim Then
        Using bW As New IO.BinaryWriter(New IO.FileStream(SaveAs, IO.FileMode.Open, IO.FileAccess.ReadWrite, IO.FileShare.ReadWrite), fileEncoding)
          bW.BaseStream.SetLength(bW.BaseStream.Length - &H80)
          bW.Close()
        End Using
      Else
        'Golden
      End If
    Else
      If bTrim Then
        Using bW As New IO.BinaryWriter(New IO.FileStream(SaveAs, IO.FileMode.Open, IO.FileAccess.ReadWrite, IO.FileShare.ReadWrite), fileEncoding)
          bW.BaseStream.Position = bW.BaseStream.Length - &H80
          bW.Write(fileEncoding.GetBytes("TAG"))
          bW.Write(fileEncoding.GetBytes(MakeNull(m_sTitle)))
          bW.Write(fileEncoding.GetBytes(MakeNull(m_sArtist)))
          bW.Write(fileEncoding.GetBytes(MakeNull(m_sAlbum)))
          bW.Write(fileEncoding.GetBytes(MakeNull(m_sYear, 4)))
          If m_sComment.Length > 28 And m_bTrack = 0 Then
            bW.Write(fileEncoding.GetBytes(MakeNull(m_sComment, 30)))
          Else
            bW.Write(fileEncoding.GetBytes(MakeNull(m_sComment, 28)))
            bW.Write(CByte(0))
            bW.Write(m_bTrack)
          End If
          bW.Write(m_bGenre)
          bW.Close()
        End Using
      Else
        Using bW As New IO.BinaryWriter(New IO.FileStream(SaveAs, IO.FileMode.Append, IO.FileAccess.Write, IO.FileShare.ReadWrite), fileEncoding)
          bW.Write(fileEncoding.GetBytes("TAG"))
          bW.Write(fileEncoding.GetBytes(MakeNull(m_sTitle)))
          bW.Write(fileEncoding.GetBytes(MakeNull(m_sArtist)))
          bW.Write(fileEncoding.GetBytes(MakeNull(m_sAlbum)))
          bW.Write(fileEncoding.GetBytes(MakeNull(m_sYear, 4)))
          If m_sComment.Length > 28 And m_bTrack = 0 Then
            bW.Write(fileEncoding.GetBytes(MakeNull(m_sComment, 30)))
          Else
            bW.Write(fileEncoding.GetBytes(MakeNull(m_sComment, 28)))
            bW.Write(CByte(0))
            bW.Write(m_bTrack)
          End If
          bW.Write(m_bGenre)
          bW.Close()
        End Using
      End If
    End If
  End Sub

  Public Property Album() As String
    Get
      Return m_sAlbum
    End Get
    Set(value As String)
      m_sAlbum = Left(value, 30)
    End Set
  End Property

  Public Property Artist() As String
    Get
      Return m_sArtist
    End Get
    Set(value As String)
      m_sArtist = Left(value, 30)
    End Set
  End Property

  Public Property Comment() As String
    Get
      Return m_sComment
    End Get
    Set(value As String)
      m_sComment = Left(value, 30)
    End Set
  End Property

  Public Property Genre() As Byte
    Get
      Return m_bGenre
    End Get
    Set(value As Byte)
      m_bGenre = value
    End Set
  End Property

  Public Property Title() As String
    Get
      Return m_sTitle
    End Get
    Set(value As String)
      m_sTitle = Left(value, 30)
    End Set
  End Property

  Public Property Track() As Byte
    Get
      Return m_bTrack
    End Get
    Set(value As Byte)
      m_bTrack = value
    End Set
  End Property

  Public Property Year() As String
    Get
      Return m_sYear
    End Get
    Set(value As String)
      m_sYear = Left(value, 4)
    End Set
  End Property

  Public Shared ReadOnly Property GenreName(Genre As Byte) As String
    Get
      Select Case Genre
        Case &H0 : Return "Blues"
        Case &H1 : Return "Classic Rock"
        Case &H2 : Return "Country"
        Case &H3 : Return "Dance"
        Case &H4 : Return "Disco"
        Case &H5 : Return "Funk"
        Case &H6 : Return "Grunge"
        Case &H7 : Return "Hip Hop"
        Case &H8 : Return "Jazz"
        Case &H9 : Return "Metal"
        Case &HA : Return "New Age"
        Case &HB : Return "Oldies"
        Case &HC : Return "Other"
        Case &HD : Return "Pop"
        Case &HE : Return "R&B"
        Case &HF : Return "Rap"
        Case &H10 : Return "Reggae"
        Case &H11 : Return "Rock"
        Case &H12 : Return "Techno"
        Case &H13 : Return "Industrial"
        Case &H14 : Return "Alternative"
        Case &H15 : Return "Ska"
        Case &H16 : Return "Death Metal"
        Case &H17 : Return "Pranks"
        Case &H18 : Return "Soundtrack"
        Case &H19 : Return "Euro Techno"
        Case &H1A : Return "Ambient"
        Case &H1B : Return "Trip Hop"
        Case &H1C : Return "Vocal"
        Case &H1D : Return "Jazz/Funk"
        Case &H1E : Return "Fusion"
        Case &H1F : Return "Trance"
        Case &H20 : Return "Classical"
        Case &H21 : Return "Instrumental"
        Case &H22 : Return "Acid"
        Case &H23 : Return "House"
        Case &H24 : Return "Game"
        Case &H25 : Return "Sound Clip"
        Case &H26 : Return "Gospel"
        Case &H27 : Return "Noise"
        Case &H28 : Return "Alternative Rock"
        Case &H29 : Return "Bass"
        Case &H2A : Return "Soul"
        Case &H2B : Return "Punk"
        Case &H2C : Return "Space"
        Case &H2D : Return "Meditative"
        Case &H2E : Return "Instrumental Pop"
        Case &H2F : Return "Instrumental Rock"
        Case &H30 : Return "Ethnic"
        Case &H31 : Return "Gothic"
        Case &H32 : Return "Darkwave"
        Case &H33 : Return "Industrial Techno"
        Case &H34 : Return "Electronic"
        Case &H35 : Return "Folk Pop"
        Case &H36 : Return "Eurodance"
        Case &H37 : Return "Dream"
        Case &H38 : Return "Southern Rock"
        Case &H39 : Return "Comedy"
        Case &H3A : Return "Cult"
        Case &H3B : Return "Gangsta Rap"
        Case &H3C : Return "Top 40"
        Case &H3D : Return "Christian Rap"
        Case &H3E : Return "Funk Pop"
        Case &H3F : Return "Jungle"
        Case &H40 : Return "Native American"
        Case &H41 : Return "Cabaret"
        Case &H42 : Return "New Wave"
        Case &H43 : Return "Psychedelic"
        Case &H44 : Return "Rave"
        Case &H45 : Return "Showtunes"
        Case &H46 : Return "Trailer"
        Case &H47 : Return "Lo-fi"
        Case &H48 : Return "Tribal"
        Case &H49 : Return "Acid Punk"
        Case &H4A : Return "Acid Jazz"
        Case &H4B : Return "Polka"
        Case &H4C : Return "Retro"
        Case &H4D : Return "Musical"
        Case &H4E : Return "Rock'n'Roll"
        Case &H4F : Return "Hard Rock"
        Case &H50 : Return "Folk"
        Case &H51 : Return "Folk Rock"
        Case &H52 : Return "National Folk"
        Case &H53 : Return "Swing"
        Case &H54 : Return "Fast Fusion"
        Case &H55 : Return "Bebob"
        Case &H56 : Return "Latin"
        Case &H57 : Return "Revival"
        Case &H58 : Return "Celtic"
        Case &H59 : Return "Blue Grass"
        Case &H5A : Return "Avant Garde"
        Case &H5B : Return "Gothic Rock"
        Case &H5C : Return "Progressive Rock"
        Case &H5D : Return "Psychedelic Rock"
        Case &H5E : Return "Symphonic Rock"
        Case &H5F : Return "Slow Rock"
        Case &H60 : Return "Big Band"
        Case &H61 : Return "Chorus"
        Case &H62 : Return "Easy Listening"
        Case &H63 : Return "Acoustic"
        Case &H64 : Return "Humour"
        Case &H65 : Return "Speech"
        Case &H66 : Return "Chanson"
        Case &H67 : Return "Opera"
        Case &H68 : Return "Chamber Music"
        Case &H69 : Return "Sonata"
        Case &H6A : Return "Symphony"
        Case &H6B : Return "Booty Bass"
        Case &H6C : Return "Primus"
        Case &H6D : Return "Porn Groove"
        Case &H6E : Return "Satire"
        Case &H6F : Return "Slow Jam"
        Case &H70 : Return "Club"
        Case &H71 : Return "Tango"
        Case &H72 : Return "Samba"
        Case &H73 : Return "Folklore"
        Case &H74 : Return "Ballad"
        Case &H75 : Return "Power Ballad"
        Case &H76 : Return "Rhythmic Soul"
        Case &H77 : Return "Freestyle"
        Case &H78 : Return "Duet"
        Case &H79 : Return "Punk Rock"
        Case &H7A : Return "Drum Solo"
        Case &H7B : Return "A Capella"
        Case &H7C : Return "Euro - House"
        Case &H7D : Return "Dance Hall"
        Case &H7E : Return "Goa"
        Case &H7F : Return "Drum & Bass"
        Case &H80 : Return "Club - House"
        Case &H81 : Return "Hardcore"
        Case &H82 : Return "Terror"
        Case &H83 : Return "Indie"
        Case &H84 : Return "Brit Pop"
        Case &H85 : Return "Negerpunk"
        Case &H86 : Return "Polsk Punk"
        Case &H87 : Return "Beat"
        Case &H88 : Return "Christian Gangsta Rap"
        Case &H89 : Return "Heavy Metal"
        Case &H8A : Return "Black Metal"
        Case &H8B : Return "Crossover"
        Case &H8C : Return "Contemporary Christian"
        Case &H8D : Return "Christian Rock"
        Case &H8E : Return "Merengue"
        Case &H8F : Return "Salsa"
        Case &H90 : Return "Thrash Metal"
        Case &H91 : Return "Anime"
        Case &H92 : Return "JPop"
        Case &H93 : Return "Synth Pop"
        Case Else : Return "Unknown (" & Hex(Genre) & ")"
      End Select
    End Get
  End Property

  Private Function MakeNull(sBuf As String, Optional ByVal lLen As Integer = 30) As String
    If String.IsNullOrEmpty(sBuf) Then Return StrDup(lLen, ChrW(0))
    sBuf = sBuf.Trim
    If lLen < sBuf.Length Then Return sBuf
    Return sBuf & StrDup(lLen - sBuf.Length, ChrW(0))
  End Function

  Private Function TrimNull(sBuf As String) As String
    If InStr(sBuf, vbNullChar) Then sBuf = Left(sBuf, InStr(sBuf, vbNullChar) - 1)
    Return Trim(sBuf)
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

  ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
  'Protected Overrides Sub Finalize()
  '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
  '    Dispose(False)
  '    MyBase.Finalize()
  'End Sub

  ' This code added by Visual Basic to correctly implement the disposable pattern.
  Public Sub Dispose() Implements IDisposable.Dispose
    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region
End Class

Public Class clsID3v2
  Implements IDisposable

  '0 = Set
  '1 = Added
  '2 = Removed
  '3 = Read only
  '4 = General Failure

  Public Class CONSTS
    Public Shared HFLG_UNSYNCH As Byte = &H80
    Public Shared HFLG_EXTENDED As Byte = &H40
    Public Shared HFLG_EXPERIMENTAL As Byte = &H20
    Public Shared HFLG_FOOTER As Byte = &H10

    Public Shared FFLG_TAGALTER As Integer = &H4000
    Public Shared FFLG_FILEALTER As Integer = &H2000
    Public Shared FFLG_READONLY As Integer = &H1000
    Public Shared FFLG_GROUP As Integer = &H40
    Public Shared FFLG_COMPRESS As Integer = &H8
    Public Shared FFLG_ENCRYPT As Integer = &H4
    Public Shared FFLG_UNSYNCH As Integer = &H2
    Public Shared FFLG_DATALENGTH As Integer = &H1
  End Class



  Private Structure ID3v2Header
    Public Identifier As String '* 3
    Public Version() As Byte ' 0 to 1
    Public Flags As Byte
    Public Size As UInteger
  End Structure
  Private Structure ID3v2Frame2
    Public FrameName As String ' * 3
    Public FrameSize() As Byte ' 0 to 2
  End Structure
  Private Structure ID3v2Frame1
    Public FrameName As String '* 4
    Public FrameSize As UInteger
    Public FrameFlags() As Byte '0 to 1
  End Structure
  Private Structure ID3v2Frame
    Public FrameName As String
    Public FrameData As String
    Public FrameFlags As UInteger
  End Structure

  Private m_sMp3File As String
  Private ID3Header As ID3v2Header
  Private HasID3v2 As Boolean
  Private ID3Frames As ArrayList
  Private lID3Len As Integer

  Public Function AddFrame(sName As String, ByVal sData As String, Optional ByVal iFlags As Integer = 0) As Byte
    ID3Frames.Add(New ID3v2Frame With {.FrameName = sName, .FrameData = sData, .FrameFlags = iFlags})
    Return 1
  End Function

  Public Sub PutFrame(sName As String, value As String)
    If value.Contains(vbCr) Then
      Do
        value = value.Replace(vbNewLine, vbCr).Replace(vbLf, vbCr).Replace(vbCr & vbCr, vbCr)
      Loop While value.Contains(vbCr & vbCr) Or value.Contains(vbLf) Or value.Contains(vbNewLine)
      RemoveFrame(sName)
      For Each sVal As String In value.Split(vbCr)
        AddFrame(sName, vbNullChar & sVal)
      Next
    Else
      SetFrame(sName, vbNullChar & value)
    End If
  End Sub

  Private Function CheckID3v2(ByRef ioReader As IO.BinaryReader) As Integer
    Try
      ioReader.BaseStream.Position = 0
      Do Until ioReader.BaseStream.Position >= ioReader.BaseStream.Length - 1
        Dim sFile As String = ioReader.ReadChars(4096)
        If sFile.Contains("ID3") Then
          Return ioReader.BaseStream.Position - 4096 + sFile.IndexOf("ID3")
        End If
      Loop
      Return -1
    Catch ex As Exception
      Return -1
    End Try
  End Function

  Public Function ReadFrame(sName As String) As Byte()()
    Try
      Dim sNewName As String = GetNewName(sName)
      Return (From id3frame As ID3v2Frame In ID3Frames Where id3frame.FrameName = sName Or id3frame.FrameName = sNewName Select fileEncoding.GetBytes(id3frame.FrameData)).ToArray
    Catch ex As Exception
      Return Nothing
    End Try
  End Function

  Public Function FindFrame(sName As String) As String
    Try
      Dim sNewName As String = GetNewName(sName)
      Return Join((From id3frame As ID3v2Frame In ID3Frames Where (id3frame.FrameName = sName Or id3frame.FrameName = sNewName) And Not IsNothing(ParseFrame(sName, id3frame.FrameData)) Select Join(ParseFrame(sName, id3frame.FrameData), vbNewLine)).ToArray, vbNewLine)
    Catch ex As Exception
      Return Nothing
    End Try
  End Function

  Public ReadOnly Property FrameCount() As Integer
    Get
      Return ID3Frames.Count
    End Get
  End Property

  Public ReadOnly Property FrameData(Index As Integer) As String
    Get
      Return CType(ID3Frames(Index), ID3v2Frame).FrameData
    End Get
  End Property

  Public ReadOnly Property FrameFlags(Index As Integer) As Integer
    Get
      Return CType(ID3Frames(Index), ID3v2Frame).FrameFlags
    End Get
  End Property

  Public ReadOnly Property FrameName(Index As Integer) As String
    Get
      Return CType(ID3Frames(Index), ID3v2Frame).FrameName
    End Get
  End Property

  Public Property Genre() As String
    Get
      Try
        Dim gData As String = FindFrame("TCO")
        gData = gData.Replace("("c, vbNullString).Replace(")"c, vbNullString)

        Dim gList() As String = gData.Split(vbNullChar)
        For I As Integer = 0 To gList.Length - 1
          If Not String.IsNullOrEmpty(gList(I)) AndAlso IsNumeric(gList(I)) Then gList(I) = clsID3v1.GenreName(gList(I))
        Next I
        Dim sGenre As String = Join(gList, vbNewLine)
        Do While sGenre.StartsWith(vbNewLine)
          sGenre = sGenre.Substring(2)
        Loop
        Do While sGenre.EndsWith(vbNewLine)
          sGenre = sGenre.Substring(0, sGenre.Length - 2)
        Loop
        Do While sGenre.Contains(vbNewLine & vbNewLine)
          sGenre = sGenre.Replace(vbNewLine & vbNewLine, vbNewLine)
        Loop
        Return sGenre
      Catch ex As Exception
        Genre = vbNullString
      End Try
    End Get
    Set(value As String)
      PutFrame("TCO", value)
    End Set
  End Property

  Private Sub GetExtendedHeader(ByRef ioReader As IO.BinaryReader, ByVal StartLoc As Integer)
    Try
      Dim lLen As Integer
      If StartLoc > 0 Then
        ioReader.BaseStream.Position = StartLoc + 10
        lLen = ioReader.ReadUInt32
        lLen = GetSize(lLen)
        ioReader.BaseStream.Position = StartLoc + 14 + lLen
      End If
    Catch ex As Exception
      Debug.Print("ID3v2 Extended Header Read error: " & ex.Message)
    End Try
  End Sub

  Private Sub GetFrame(ByRef ioReader As IO.BinaryReader)
    Try
      Dim ID3Frame As ID3v2Frame1
      ID3Frame.FrameName = ioReader.ReadChars(4)
      Dim sSize As String = ioReader.ReadChars(4)
      ID3Frame.FrameSize = GetDWORD(sSize)
      If ID3Header.Version(0) = 4 Then ID3Frame.FrameSize = UnSynchSafe(ID3Frame.FrameSize)
      If ID3Frame.FrameSize < ioReader.BaseStream.Length And ID3Frame.FrameSize > 0 Then
        ReDim ID3Frame.FrameFlags(1)
        ID3Frame.FrameFlags = ioReader.ReadBytes(2)
        Dim sFrameData As String = ioReader.ReadChars(ID3Frame.FrameSize)
        AddFrame(ID3Frame.FrameName, sFrameData, Val("&H" & Hex$(ID3Frame.FrameFlags(1)) & Hex$(ID3Frame.FrameFlags(0))))
      End If
    Catch ex As Exception
      Debug.Print("ID3v2 Frame v3/4 Read error: " & ex.Message)
    End Try
  End Sub

  Private Sub GetFrame2(ByRef ioReader As IO.BinaryReader)
    Try
      Dim ID3Frame As ID3v2Frame2
      ID3Frame.FrameName = ioReader.ReadChars(3)
      ReDim ID3Frame.FrameSize(2)
      ID3Frame.FrameSize = ioReader.ReadBytes(3)
      Dim lFrameSize As Integer = CLng("&H00" & BufferHex(ID3Frame.FrameSize(0)) & BufferHex(ID3Frame.FrameSize(1)) & BufferHex(ID3Frame.FrameSize(2)))
      If lFrameSize < ioReader.BaseStream.Length And lFrameSize > 0 Then
        Dim sFrameData As String = ioReader.ReadChars(lFrameSize)
        AddFrame(ID3Frame.FrameName, sFrameData)
      End If
    Catch ex As Exception
      Debug.Print("ID3v2 Frame v2 Read error: " & ex.Message)
    End Try
  End Sub

  Private Sub GetHeader(ByRef ioReader As IO.BinaryReader, ByVal StartLoc As Integer)
    Try
      HasID3v2 = False
      If StartLoc > -1 Then
        ioReader.BaseStream.Position = StartLoc
        ID3Header.Identifier = ioReader.ReadChars(3)
        If ID3Header.Identifier = "ID3" Then
          ReDim ID3Header.Version(1)
          ID3Header.Version = ioReader.ReadBytes(2)
          ID3Header.Flags = ioReader.ReadByte
          ID3Header.Size = ioReader.ReadUInt32
          ID3Header.Size = UnSynchSafe(GetSize(ID3Header.Size))
          Select Case ID3Header.Version(0)
            Case 2, 3, 4
              Select Case ID3Header.Version(1)
                Case 0
                  If (ID3Header.Flags And &HE0) = ID3Header.Flags Then HasID3v2 = True
              End Select
            Case Else
              'Debug.Print("Unknown ID3 Version: 2." & ID3Header.Version(0) & "." & ID3Header.Version(1))
              HasID3v2 = False
          End Select
        End If
      End If
    Catch ex As Exception
      HasID3v2 = False
    End Try
  End Sub

  Private Function BufferHex(lIn As UInteger, Optional lLen As UShort = 2) As String
    Dim sVal As String = Hex(lIn)
    Do While sVal.Length < lLen : sVal = "0" & sVal : Loop
    Return sVal
  End Function

  Private Function GetNewName(sNewName As String) As String
    Select Case sNewName
      Case "BUF" : GetNewName = "RBUF"
      Case "CNT" : GetNewName = "PCNT"
      Case "COM" : GetNewName = "COMM"
      Case "CRA" : GetNewName = "AENC"
      Case "CRM" : GetNewName = sNewName
      Case "ETC" : GetNewName = "ETCO"
      Case "EQU" : GetNewName = "EQU2"
      Case "GEO" : GetNewName = "GEOB"
      Case "IPL" : GetNewName = "TIPL"
      Case "LNK" : GetNewName = "LINK"
      Case "MCI" : GetNewName = "MCDI"
      Case "MLL" : GetNewName = "MLLT"
      Case "PIC" : GetNewName = "APIC"
      Case "POP" : GetNewName = "POPM"
      Case "REV" : GetNewName = "RVRB"
      Case "RVA" : GetNewName = "RVA2"
      Case "SLT" : GetNewName = "SYLT"
      Case "STC" : GetNewName = "SYTC"
      Case "TAL" : GetNewName = "TALB"
      Case "TBP" : GetNewName = "TBPM"
      Case "TCM" : GetNewName = "TCOM"
      Case "TCO" : GetNewName = "TCON"
      Case "TCR" : GetNewName = "TCOP"
      Case "TDA" : GetNewName = sNewName
      Case "TDY" : GetNewName = "TDLY"
      Case "TEN" : GetNewName = "TENC"
      Case "TFT" : GetNewName = "TFLT"
      Case "TIM" : GetNewName = sNewName
      Case "TKE" : GetNewName = "TKEY"
      Case "TLA" : GetNewName = "TLAN"
      Case "TLE" : GetNewName = "TLEN"
      Case "TMT" : GetNewName = "TMED"
      Case "TOA" : GetNewName = "TOPE"
      Case "TOF" : GetNewName = "TOFN"
      Case "TOL" : GetNewName = "TOLY"
      Case "TOR" : GetNewName = "TDOR"
      Case "TOT" : GetNewName = "TOAL"
      Case "TP1" : GetNewName = "TPE1"
      Case "TP2" : GetNewName = "TPE2"
      Case "TP3" : GetNewName = "TPE3"
      Case "TP4" : GetNewName = "TPE4"
      Case "TPA" : GetNewName = "TPOS"
      Case "TPB" : GetNewName = "TPUB"
      Case "TRC" : GetNewName = "TSRC"
      Case "TRD" : GetNewName = "TDRC"
      Case "TRK" : GetNewName = "TRCK"
      Case "TSI" : GetNewName = "TSIZ"
      Case "TSS" : GetNewName = "TSSE"
      Case "TT1" : GetNewName = "TIT1"
      Case "TT2" : GetNewName = "TIT2"
      Case "TT3" : GetNewName = "TIT3"
      Case "TXT" : GetNewName = "TEXT"
      Case "TXX" : GetNewName = "TXXX"
      Case "TYE" : GetNewName = "TYER"
      Case "UFI" : GetNewName = "UFID"
      Case "ULT" : GetNewName = "USLT"
      Case "WAF" : GetNewName = "WOAF"
      Case "WAR" : GetNewName = "WOAR"
      Case "WAS" : GetNewName = "WOAS"
      Case "WCM" : GetNewName = "WCOM"
      Case "WCP" : GetNewName = "WCOP"
      Case "WPB" : GetNewName = "WPUB"
      Case "WXX" : GetNewName = "WXXX"
      Case Else : GetNewName = sNewName
    End Select
  End Function

  Public Shared Function GetFrameName(sName As String) As String
    Select Case sName
      Case "ASPI" : Return "Audio Seek Point Index"
      Case "BUF", "RBUF" : Return "Recommended Buffer Size"
      Case "CNT", "PCNT" : Return "Play Counter"
      Case "COM", "COMM" : Return "Comments"
      Case "COMR" : Return "Commercial"
      Case "CRA", "AENC" : Return "Audio Encryption"
      Case "CRM" : Return "Encrypted META Frame"
      Case "ENCR" : Return "Encryption Method Registration"
      Case "EQU", "EQUA", "EQU2" : Return "Equalization"
      Case "ETC", "ETCO" : Return "Event Timing Codes"
      Case "GEO", "GEOB" : Return "General Encapsulated Object"
      Case "GRID" : Return "Group ID Registration"
      Case "IPL", "TIPL" : Return "Involved People List"
      Case "LNK", "LINK" : Return "Linked Information"
      Case "MCI", "MCDI" : Return "Music CD Identifier"
      Case "MLL", "MLLT" : Return "MPEG Location Lookup Table"
      Case "OWNE" : Return "Ownership"
      Case "PIC", "APIC" : Return "Picture"
      Case "PRIV" : Return "Private"
      Case "POP", "POPM" : Return "Popularimeter"
      Case "POSS" : Return "Position Synchronization"
      Case "REV", "RVRB" : Return "Reverb"
      Case "RVA", "RVAD", "RVA2" : Return "Relative Volume Advustment"
      Case "SEEK" : Return "Seek"
      Case "SIGN" : Return "Signature"
      Case "SLT", "SYLT" : Return "Synced Lyrics"
      Case "STC", "SYTC" : Return "Synced Tempo Codes"
      Case "TAL", "TALB" : Return "Album"
      Case "TBB", "TBPM" : Return "Beats per Minute"
      Case "TCM", "TCOM" : Return "Composer"
      Case "TCO", "TCON" : Return "Genre"
      Case "TCR", "TCOP" : Return "Copyright Message"
      Case "TDA", "TDAT" : Return "Date"
      Case "TDTG" : Return "Tagging Time"
      Case "TDEN" : Return "Encoding Time"
      Case "TDY", "TDLY" : Return "Playlist Delay"
      Case "TEN", "TENC" : Return "Encoded By"
      Case "TFT", "TFLT" : Return "File Type"
      Case "TIM", "TIME", "TDRL" : Return "Time"
      Case "TKE", "TKEY" : Return "Initial Key"
      Case "TLA", "TLAN" : Return "Language"
      Case "TLE", "TLEN" : Return "Length"
      Case "TMCL" : Return "Musician Credits"
      Case "TMT", "TMED" : Return "Media Type"
      Case "TMOO" : Return "Mood"
      Case "TOA", "TOPE" : Return "Original Artist"
      Case "TOF", "TOFN" : Return "Original Filename"
      Case "TOL", "TOLY" : Return "Original Lyricist"
      Case "TOR", "TDOR" : Return "Original Release Year"
      Case "TOT", "TOAL" : Return "Original Album"
      Case "TOWN" : Return "File Owner"
      Case "TP1", "TPE1" : Return "Artist"
      Case "TP2", "TPE2" : Return "Band"
      Case "TP3", "TPE3" : Return "Conductor"
      Case "TP4", "TPE4" : Return "Mix By"
      Case "TPA", "TPOS" : Return "Set"
      Case "TPRO" : Return "Produced"
      Case "TPB", "TPUB" : Return "Publisher"
      Case "TRC", "TSRC" : Return "ISRC"
      Case "TRD", "TDRC" : Return "Recording Date"
      Case "TRK", "TRCK" : Return "Track"
      Case "TRSN" : Return "Internet Radio Station Name"
      Case "TRSO" : Return "Internet Ratio Station Owner"
      Case "TSOA" : Return "Album Sort Order"
      Case "TSOP" : Return "Performer Sort Order"
      Case "TSOT" : Return "Title Sort Order"
      Case "TSI", "TSIZ" : Return "Size"
      Case "TSS", "TSSE" : Return "Encoding Settings"
      Case "TT1", "TIT1" : Return "Set Title"
      Case "TSST" : Return "Set Subtitle"
      Case "TT2", "TIT2" : Return "Title"
      Case "TT3", "TIT3" : Return "Subtitle"
      Case "TXT", "TEXT" : Return "Lyricist"
      Case "TXX", "TXXX" : Return "Custom Text"
      Case "TYE", "TYER" : Return "Year"
      Case "UFI", "UFID" : Return "Unique File ID"
      Case "USER" : Return "Terms of Use"
      Case "ULT", "USLT" : Return "Lyrics"
      Case "WAF", "WOAF" : Return "Song Webpage"
      Case "WAR", "WOAR" : Return "Artist Webpage"
      Case "WAS", "WOAS" : Return "Source Webpage"
      Case "WCM", "WCOM" : Return "Commercial Information"
      Case "WCP", "CWOP" : Return "Copyright Information"
      Case "WORS" : Return "Interent Radio Station Webpage"
      Case "WPAY" : Return "Payment"
      Case "WPB", "WPUB" : Return "Publisher Webpage"
      Case "WXX", "WXXX" : Return "Custom Webpage"
      Case Else : Return sName
    End Select
  End Function

  Private Function GetSize(lFromFile As UInteger) As UInteger
    Dim bFF() As Byte = BitConverter.GetBytes(lFromFile)
    Array.Reverse(bFF)
    Return BitConverter.ToUInt32(bFF, 0)
  End Function

  Private Function UnSynchSafe(lIn As UInteger) As UInteger
    Dim lOut As UInteger = 0
    lOut = (lOut Or lIn And &H7F000000) \ 2
    lOut = (lOut Or lIn And &H7F0000) \ 2
    lOut = (lOut Or lIn And &H7F00) \ 2
    lOut = lOut Or lIn And &H7F
    Return lOut
  End Function

  Private Function SynchSafe(lIn As UInteger) As UInteger
    Return lIn * 2 ^ 3 And &H7F000000 Or (lIn * 2 ^ 2) And &H7F0000 Or (lIn * 2 ^ 1) And &H7F00 Or lIn And &H7F
  End Function

  Public ReadOnly Property HasID3v2Tag() As Boolean
    Get
      Return HasID3v2
    End Get
  End Property

  Public Property ID3v2Ver() As String
    Get
      If ID3Header.Version Is Nothing Then ReDim ID3Header.Version(1)
      Return "2." & ID3Header.Version(0) & "." & ID3Header.Version(1)
    End Get
    Set(value As String)
      If ID3Header.Version Is Nothing Then ReDim ID3Header.Version(1)
      If value.Contains(".") Then
        Dim Parts() As String = Split(value, ".")
        If Parts.Length = 3 AndAlso (IsNumeric(Parts(0)) And IsNumeric(Parts(1)) And IsNumeric(Parts(2))) Then
          If Parts(0) = "2" Then
            Try
              ID3Header.Version(0) = CByte(Parts(1))
              ID3Header.Version(1) = CByte(Parts(2))
            Catch ex As Exception

            End Try
          End If
        End If
      End If
    End Set
  End Property

  Public ReadOnly Property ID3v2Len() As Integer
    Get
      Return lID3Len
    End Get
  End Property

  Private Sub LoadFile(sFile As String)
    If Not My.Computer.FileSystem.FileExists(sFile) Then Exit Sub
    Dim lStart As Integer
    Dim bTest As Byte
    Dim lLast As Integer
    Try
      ID3Frames.Clear()

      Using bR As New IO.BinaryReader(New IO.FileStream(sFile, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read), fileEncoding)
        lStart = CheckID3v2(bR)
        If lStart = -1 Then Exit Sub
        GetHeader(bR, lStart)
        If HasID3v2 Then
          If ID3Header.Flags And CONSTS.HFLG_EXTENDED Then GetExtendedHeader(bR, lStart)
          Do While bR.BaseStream.Position < bR.BaseStream.Length - 1
            If ID3Header.Version(0) = 2 Then
              GetFrame2(bR)
            Else
              GetFrame(bR)
            End If
            lLast = bR.BaseStream.Position
            bTest = bR.ReadByte
            If (bTest = 0) Or (bTest = &HFF) Then Exit Do
            bR.BaseStream.Position = lLast
          Loop
          If lLast < bR.BaseStream.Length - 1 Then
            lID3Len = lLast
          Else
            lID3Len = (ID3Header.Size + IIf(ID3Header.Flags And CONSTS.HFLG_FOOTER, &H15, &HB))
            Exit Sub
          End If
        Else
          lID3Len = 1
        End If
        bR.Close()
      End Using
    Catch ex As Exception
      Debug.Print("ID3v2 Read error: " & ex.Message)
      lID3Len = 1
    End Try
  End Sub

  Private Function MakeDWORD(lIn As UInt32) As String
    Return StrReverse(fileEncoding.GetString(BitConverter.GetBytes(lIn)))
  End Function

  Private Function MakeWORD(iIn As UInt16) As String
    Return StrReverse(fileEncoding.GetString(BitConverter.GetBytes(iIn)))
  End Function

  Private Function GetDWORD(sIn As String) As UInt32
    If sIn.Length > 4 Then sIn = sIn.Substring(0, 4)
    Return BitConverter.ToUInt32(fileEncoding.GetBytes(StrReverse(sIn)), 0)
  End Function

  Private Function GetWORD(sIn As String) As UInt16
    If sIn.Length > 2 Then sIn = sIn.Substring(0, 2)
    Return BitConverter.ToUInt32(fileEncoding.GetBytes(StrReverse(sIn)), 0)
  End Function

  Public Sub New(File As String)
    ID3Frames = New ArrayList
    m_sMp3File = File
    LoadFile(File)
  End Sub

  Public Property MP3File() As String
    Get
      Return m_sMp3File
    End Get
    Set(value As String)
      m_sMp3File = value
      LoadFile(value)
    End Set
  End Property

  Private Function ConvertGenre(sIn As String) As String
    If IsNumeric(sIn) AndAlso (sIn > -1 And sIn < 256) Then
      Return clsID3v1.GenreName(sIn)
    ElseIf sIn = "RX" Then
      Return "Remix"
    ElseIf sIn = "CR" Then
      Return "Cover"
    Else
      Return sIn
    End If
  End Function

  Public Function ParseFrame(sName As String, ByVal sData As String) As Object()
    Dim ioData As New IO.BinaryReader(New IO.MemoryStream(fileEncoding.GetBytes(sData)))
    Select Case sName
      Case "UFI", "UFID"
        Dim sOwner As String = Nothing
        Dim bTmp As Byte
        Do
          bTmp = ioData.ReadByte
          If bTmp > 0 Then sOwner &= ChrW(bTmp)
        Loop Until bTmp = 0
        If String.IsNullOrEmpty(sOwner) Then Return Nothing
        Dim Ident() As Byte = ioData.ReadBytes(sData.Length - (sOwner.Length + 1))
        Return {sOwner, BitConverter.ToString(Ident)}
      Case "TXX", "TXXX"
        Dim bEncoding As Byte = Asc(sData.Substring(0, 1))
        Dim sFind As String = vbNullChar
        If bEncoding = 1 Or bEncoding = 2 Then sFind &= vbNullChar
        Dim sDesc As String = sData.Substring(1, sData.LastIndexOf(sFind) + (sFind.Length - 1))
        Dim sValue As String = sData.Substring(sDesc.Length + 1)
        sDesc = ParseString(bEncoding, sDesc)
        sValue = ParseString(bEncoding, sValue)
        If String.IsNullOrEmpty(sDesc) Or sDesc = ChrW(&HFEFF) Then Return {sValue}
        Return {sDesc, sValue}
      Case "TCO", "TCON"
        If ID3Header.Version(0) = "4" Then
          Dim Genres As New ArrayList
          Do
            Dim sGenre As String = Nothing
            Dim bTmp As Byte
            Do
              bTmp = ioData.ReadByte
              If bTmp > 0 Then sGenre &= ChrW(bTmp)
            Loop Until bTmp = 0 Or ioData.BaseStream.Position >= ioData.BaseStream.Length - 1
            Genres.Add(ConvertGenre(sGenre))
          Loop Until ioData.BaseStream.Position >= ioData.BaseStream.Length - 1
          Return {Join(Genres.ToArray, "/")}
        Else
          Dim sBak As String = sData
          Dim bEncoding As Byte = Asc(sData.Substring(0, 1))
          sData = ParseString(bEncoding, sData.Substring(1))
          If String.IsNullOrEmpty(sData) Then Return Nothing
          Dim sTmp As String = String.Empty
          Dim lPos As Integer = 0
          Dim startVal As Boolean = False
          Dim Genres As New ArrayList
          Do
            Dim sI As String = sData.Substring(lPos, 1)
            If sI = "(" And Not startVal Then
              If Not String.IsNullOrEmpty(sTmp) Then
                Genres.Add(ConvertGenre(sTmp))
                sTmp = String.Empty
              End If
              startVal = True
            ElseIf sI = ")" And startVal Then
              startVal = False
              If Not String.IsNullOrEmpty(sTmp) Then
                Genres.Add(ConvertGenre(sTmp))
                sTmp = String.Empty
              End If
            Else
              sTmp &= sI
            End If
            lPos += 1
          Loop While lPos < sData.Length
          If Not String.IsNullOrEmpty(sTmp) Then
            Genres.Add(ConvertGenre(sTmp))
            sTmp = String.Empty
          End If
          Return {Join(Genres.ToArray, "/")}
        End If
      Case "MCI", "MCDI" : Return {BitConverter.ToString(fileEncoding.GetBytes(sData))}
      Case "COM", "COMM" : Return {ParseString(Asc(sData.Substring(0, 1)), sData.Substring(4))}
      Case "PIC"
        Dim bEnc As Byte = Asc(sData.Substring(0, 1))
        Dim sFormat As String = sData.Substring(1, 3)
        Dim bPicType As Byte = Asc(sData.Substring(4, 1))
        Dim sDesc As String = sData.Substring(5)
        Dim sFind As String = vbNullChar
        If bEnc = 1 Or bEnc = 2 Then sFind &= vbNullChar
        If sDesc.Contains(sFind) Then
          Dim sPicture As String = sDesc.Substring(sDesc.IndexOf(sFind) + sFind.Length)
          sDesc = sDesc.Substring(0, sDesc.IndexOf(sFind))
          sDesc = ParseString(bEnc, sDesc)
          Return {sFormat, sDesc, bPicType, sPicture}
        Else
          Return Nothing
        End If
      Case "APIC"
        Dim bEnc As Byte = Asc(sData.Substring(0, 1))
        Dim sMIME As String = sData.Substring(1)
        If sMIME.Contains(vbNullChar) Then
          sMIME = sMIME.Substring(0, sMIME.IndexOf(vbNullChar))
          If sData.Length > sMIME.Length + 2 Then
            Dim bPicType As Byte = Asc(sData.Substring(sMIME.Length + 2, 1))
            Dim sDesc As String = sData.Substring(sMIME.Length + 3)
            Dim sFind As String = vbNullChar
            If bEnc = 1 Or bEnc = 2 Then sFind &= vbNullChar
            If sDesc.Contains(sFind) Then
              Dim sPicture As String = sDesc.Substring(sDesc.IndexOf(sFind) + sFind.Length)
              sDesc = sDesc.Substring(0, sDesc.IndexOf(sFind))
              sDesc = ParseString(bEnc, sDesc)
              Return {sMIME, sDesc, bPicType, sPicture}
            Else
              Return Nothing
            End If
          Else
            Return Nothing
          End If
        Else
          Return Nothing
        End If
      Case "ULT", "USLT"
        If Asc(sData.Substring(0, 1)) < 4 Then
          If sData.Substring(4, 1) = vbNullChar Then
            Return {sData.Substring(1, 3), ParseString(Asc(sData.Substring(0, 1)), sData.Substring(5))}
          Else
            Return {ParseString(Asc(sData.Substring(0, 1)), sData.Substring(1))}
          End If
        Else
          Return {sData}
        End If
      Case "WXX", "WXXX"
        Dim bEncoding As Byte = Asc(sData.Substring(0, 1))
        Dim sFind As String = vbNullChar
        If bEncoding = 1 Or bEncoding = 2 Then sFind &= vbNullChar
        Dim sDesc As String = sData.Substring(1, sData.LastIndexOf(sFind) + (sFind.Length - 1))
        Dim sValue As String = sData.Substring(sDesc.Length + 1)
        sDesc = ParseString(bEncoding, sDesc)
        sValue = ParseString(0, sValue)
        If String.IsNullOrWhiteSpace(sDesc) Or sDesc = ChrW(&HFEFF) Then Return {sValue}
        Return {sDesc, sValue}
      Case "GEO", "GEOB"
        Dim bEncoding As Byte = Asc(sData.Substring(0, 1))
        Dim sMIME As String = sData.Substring(1, sData.IndexOf(vbNullChar, 1) - 1)
        Dim sPos As Integer = sMIME.Length + 2
        Dim sFind As String = vbNullChar
        If bEncoding = 1 Or bEncoding = 2 Then sFind &= vbNullChar
        Dim sFile As String = sData.Substring(sPos, sData.IndexOf(sFind, sPos) + (sFind.Length - 1) - sPos)
        sPos += sFile.Length + 1
        Dim sDesc As String = sData.Substring(sPos, sData.IndexOf(sFind, sPos) + (sFind.Length - 1) - sPos)
        sPos += sDesc.Length + 1
        Dim sContent As String = sData.Substring(sPos)
        sFile = ParseString(bEncoding, sFile)
        sDesc = ParseString(bEncoding, sDesc)
        Return {sMIME, sFile, sDesc, sContent}
      Case Else
        If sName.StartsWith("T") Then
          Return {ParseString(Asc(sData.Substring(0, 1)), sData.Substring(1))}
        Else
          'If sName.StartsWith("T") Or sName.StartsWith("W") T  hen
          If Asc(sData.Substring(0, 1)) < 4 Then
            Return {ParseString(Asc(sData.Substring(0, 1)), sData.Substring(1))}
          Else
            Return {sData}
          End If
          'Return ParseString(Asc(Left$(sData, 1)), Mid$(sData, 2))
          'Else
          'Return sData
          'End If
        End If

    End Select
  End Function

  Public Shared Function ImageID(ID As Byte) As String
    Select Case ID
      Case &H0 : Return "Other"
      Case &H1 : Return "32x32 Icon"
      Case &H2 : Return "Icon"
      Case &H3 : Return "Front Cover"
      Case &H4 : Return "Back Cover"
      Case &H5 : Return "Leaflet"
      Case &H6 : Return "Media"
      Case &H7 : Return "Lead Artist"
      Case &H8 : Return "Artist"
      Case &H9 : Return "Conductor"
      Case &HA : Return "Band"
      Case &HB : Return "Composer"
      Case &HC : Return "Lyricist"
      Case &HD : Return "Recording Location"
      Case &HE : Return "During Recording"
      Case &HF : Return "During Performance"
      Case &H10 : Return "Video Screen Capture"
      Case &H11 : Return "Red Herring"
      Case &H12 : Return "Illustration"
      Case &H13 : Return "Band Logo"
      Case &H14 : Return "Publisher Logo"
      Case Else : Return "Unknown (" & Hex(ID) & ")"
    End Select
  End Function

  Private Function ParseString(Encoding As Byte, ByVal Text As String) As String
    Select Case Encoding
      Case 0
        'ISO (NT)
      Case 1
        'UTF 16
        Text = System.Text.Encoding.Unicode.GetString(fileEncoding.GetBytes(Text))
      Case 2
        'UTF 16 BE
        Text = System.Text.Encoding.BigEndianUnicode.GetString(fileEncoding.GetBytes(Text))
      Case 3
        'UTF 8
        Text = System.Text.Encoding.UTF8.GetString(fileEncoding.GetBytes(Text))
      Case Else
        'Probably not encoded...
        'Debug.Print("Unknown Content encoding: " & Encoding & " [" & Text & "]!")
    End Select
    If Text.Contains(vbNullChar) Then Text = Text.Substring(0, Text.IndexOf(vbNullChar))
    Do While Text.Contains(vbNullChar)
      Text = Text.Replace(vbNullChar, "[NULL]")
    Loop
    Do While Text.Contains(vbLf)
      Text = Text.Replace(vbLf, "[LF]")
    Loop
    'Text = Text.Replace(vbCr, vbNewLine)
    Do While Text.StartsWith(vbCr)
      Text = Text.Substring(1)
    Loop
    Do While Text.EndsWith(vbCr)
      Text = Text.Substring(0, Text.Length - 1)
    Loop
    Return Text
  End Function

  Public Function RemoveAll() As Byte
    ID3Frames.Clear()
    RemoveAll = 2
  End Function

  Public Function RemoveFrame(sName As String) As Byte
    For I As Integer = 0 To ID3Frames.Count - 1
      If ID3Frames(I).FrameName = sName Or ID3Frames(I).FrameName = GetNewName(sName) Then
        If ID3Frames(I).FrameFlags And CONSTS.FFLG_READONLY Then
          Return 3
        Else
          ID3Frames.RemoveAt(I)
          Return 2
        End If
      End If
    Next I
    Return 4
  End Function

  Public Sub Save(Optional SaveAs As String = Nothing)
    If String.IsNullOrEmpty(SaveAs) Then SaveAs = m_sMp3File
    Dim FileData() As Byte = TrimID3v2(m_sMp3File)
    Using ID3Data As New IO.FileStream(SaveAs, IO.FileMode.OpenOrCreate, IO.FileAccess.Write, IO.FileShare.None)
      Using ID3Writer As New IO.BinaryWriter(ID3Data, fileEncoding)
        ID3Header.Identifier = "ID3"
        ID3Header.Flags = 0

        ID3Writer.Write(fileEncoding.GetBytes(ID3Header.Identifier))
        ID3Writer.Write(ID3Header.Version(0))
        ID3Writer.Write(ID3Header.Version(1))
        ID3Writer.Write(ID3Header.Flags)
        Dim bContent() As Byte
        Using ID3Content As New IO.MemoryStream
          Using ID3ContentWriter As New IO.BinaryWriter(ID3Content, fileEncoding)

            For I = 0 To FrameCount - 1
              If Not FrameData(I) = vbNullChar Then
                Dim sName As String = FrameName(I)
                If ID3Header.Version(0) = 3 Or ID3Header.Version(0) = 4 Then sName = GetNewName(sName)
                If Not ((FrameFlags(I) And CONSTS.FFLG_TAGALTER) Or (FrameFlags(I) And CONSTS.FFLG_FILEALTER)) Then
                  Dim bData() As Byte = fileEncoding.GetBytes(FrameData(I))
                  'Name
                  ID3ContentWriter.Write(fileEncoding.GetBytes(sName))
                  'DWORD Size
                  If ID3Header.Version(0) = 4 Then
                    ID3ContentWriter.Write(fileEncoding.GetBytes(MakeDWORD(SynchSafe(bData.Length))))
                  Else
                    ID3ContentWriter.Write(fileEncoding.GetBytes(MakeDWORD(bData.Length)))
                  End If
                  'WORD Flags
                  ID3ContentWriter.Write(fileEncoding.GetBytes(MakeWORD(FrameFlags(I))))
                  'Data
                  ID3ContentWriter.Write(bData)
                End If
              End If
            Next I
            Dim Spaces As Integer = &H200
            Do Until ID3Content.Length + &HA < Spaces
              Spaces += &H200
            Loop
            Spaces = Spaces - (ID3Content.Length + &HA)
            ID3ContentWriter.Write(fileEncoding.GetBytes(StrDup(Spaces, vbNullChar)))
          End Using
          bContent = ID3Content.ToArray
        End Using

        ID3Header.Size = bContent.Length
        ID3Writer.Write(fileEncoding.GetBytes(MakeDWORD(SynchSafe(ID3Header.Size))))
        ID3Writer.Write(bContent)

        ID3Writer.Write(FileData)
        ID3Writer.Flush()
      End Using
      ID3Data.Close()
    End Using

  End Sub

  Private Function TrimID3v2(Path As String) As Byte()
    If String.IsNullOrEmpty(Path) Then Return Nothing
    If Not My.Computer.FileSystem.FileExists(Path) Then Return Nothing
    If My.Computer.FileSystem.GetFileInfo(Path).Length >= 1024L * 1024L * 1024L * 4L Then Return Nothing
    Dim bFile() As Byte = My.Computer.FileSystem.ReadAllBytes(Path)
    Dim lFrameStart As Integer = 0
    Do
      lFrameStart = GetBytePos(bFile, &HFF, lFrameStart)
      If lFrameStart >= 0 AndAlso CheckMPEG(bFile, lFrameStart) Then Exit Do
      lFrameStart += 1
    Loop
    If lFrameStart = 0 Then Return bFile
    Dim lFrameLen As Integer = bFile.Length - lFrameStart
    Dim bReturn(lFrameLen - 1) As Byte
    Array.ConstrainedCopy(bFile, lFrameStart, bReturn, 0, lFrameLen)
    Return bReturn
  End Function

  Public Function SetFrame(sName As String, ByVal sData As String, Optional ByVal iFlags As Integer = 0) As Byte
    Dim B As Boolean = False
    If Not String.IsNullOrEmpty(sData) Then
      For I As Integer = 0 To ID3Frames.Count - 1
        If ID3Frames(I).FrameName = sName Or ID3Frames(I).FrameName = GetNewName(sName) Then
          If ID3Frames(I).FrameFlags And CONSTS.FFLG_READONLY Then
            SetFrame = 3
          Else
            B = True
            ID3Frames(I).FrameData = sData
            ID3Frames(I).FrameFlags = iFlags
            Return 0
          End If
        End If
      Next I
      If Not B Then Return AddFrame(GetNewName(sName), sData, iFlags)
      Return 4
    Else
      Return RemoveFrame(sName)
    End If
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

  ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
  'Protected Overrides Sub Finalize()
  '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
  '    Dispose(False)
  '    MyBase.Finalize()
  'End Sub

  ' This code added by Visual Basic to correctly implement the disposable pattern.
  Public Sub Dispose() Implements IDisposable.Dispose
    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region
End Class

Public Class clsHeaderLoader
  Implements IDisposable
  Public cMPEG As clsMPEG
  Public cXING As clsXING
  Public cVBRI As clsVBRI
  Public cID3v1 As clsID3v1
  Public cID3v2 As clsID3v2
  Private lSize As Long
  '0 = CBR
  '1 = VBR XING
  '2 = VBR Fraunhofer
  Private bFmt As Byte
  Private fStart As Long
  Private fLen As Long
  Private sPath As String

  Public Sub New(FilePath As String)
    If String.IsNullOrEmpty(FilePath) Then Exit Sub
    If Not My.Computer.FileSystem.FileExists(FilePath) Then Exit Sub
    If My.Computer.FileSystem.GetFileInfo(FilePath).Length >= 1024L * 1024L * 1024L * 4L Then Exit Sub
    sPath = FilePath
    Dim bFile() As Byte = My.Computer.FileSystem.ReadAllBytes(FilePath)
    lSize = bFile.Length
    Dim Id3v1 As Boolean = (GetString(bFile, bFile.Length - &H80, 3) = "TAG")
    Dim lFram As Integer = -1
    Do
      lFram = GetBytePos(bFile, &HFF, lFram)
      If lFram >= 0 Then
        If CheckMPEG(bFile, lFram) Then
          Dim cMPGHead As New clsMPEG(GetDWORD(bFile, lFram)) ' Left$(sFile, 4)))
          Dim lFrameSize As Long = cMPGHead.GetFrameSize
          Dim bTmp(lFrameSize - 1) As Byte
          Array.ConstrainedCopy(bFile, lFram, bTmp, 0, lFrameSize)
          ReDim bFile(lFrameSize - 1)
          Array.Copy(bTmp, bFile, lFrameSize)
          If lFram = -1 Then lFram = 0
          Exit Do
        End If
      Else
        Exit Do
      End If
      lFram = lFram + 1
    Loop
    If lFram = -1 Then Exit Sub
    fStart = lFram
    fLen = lSize - fStart - IIf(Id3v1, &H80, 0)
    cMPEG = New clsMPEG(GetDWORD(bFile))
    cXING = New clsXING(bFile, cMPEG)
    cVBRI = New clsVBRI(bFile)
    cID3v1 = New clsID3v1(FilePath)
    cID3v2 = New clsID3v2(FilePath)
    If cXING.HeaderID = "Xing" Or cXING.HeaderID = "Info" Then
      bFmt = 1
    ElseIf cVBRI.HeaderID = "VBRI" Then
      bFmt = 2
    Else
      bFmt = 3
    End If
  End Sub

  Private Function GetVBRDuration() As Double
    Dim dDur As Double = 0
    Dim bFile() As Byte = My.Computer.FileSystem.ReadAllBytes(sPath)
    Dim lFram As Integer = -1
    Dim lFrames As UInt32 = 0
    Do
      lFram = GetBytePos(bFile, &HFF, lFram)
      If lFram >= 0 Then
        If CheckMPEG(bFile, lFram) Then
          lFrames += 1
          Dim cMPGHead As New clsMPEG(GetDWORD(bFile, lFram)) ' Left$(sFile, 4)))
          Dim dFrameDur As Double = (GetSamplesPerFame(cMPGHead) / cMPGHead.GetSampleRate) '(cMPGHead.GetFrameSize / cMPGHead.GetBitrate) * 8
          'debug.Print("Frame " & lFrames & ": Frame Size = " & cMPGHead.GetFrameSize & ", Bitrate = " & cMPGHead.GetBitrate & ", Frame Duration = " & dFrameDur)
          'Debug.Print("Frame " & lFrames & ": Samples per Frame = " & GetSamplesPerFame(cMPGHead) & ", Sample Rate = " & cMPGHead.GetSampleRate & ", Frame Duration = " & dFrameDur)
          dDur += dFrameDur
          cMPGHead = Nothing
          If lFram = -1 And dDur > 0 Then Exit Do
        End If
      Else
        If dDur > 0 Then Exit Do
      End If
      lFram += 1
    Loop

    'If lFram = -1 Then Return -1
    'Debug.Print(dDur)
    Return dDur
  End Function

  Public ReadOnly Property RateFormat() As String
    Get
      Select Case bFmt
        Case 1 : RateFormat = cXING.VBRMethod & " (XING)"
        Case 2 : RateFormat = "VBR (Fraunhofer)"
        Case 3 : RateFormat = "CBR"
        Case Else : RateFormat = "Unknown"
      End Select
    End Get
  End Property

  Public ReadOnly Property Duration() As Double
    Get
      If bFmt = 1 Or bFmt = 2 Then
        Return GetVBRDuration()
      Else
        Return Frames * GetSamplesPerFame(cMPEG) / cMPEG.GetSampleRate
      End If
    End Get
  End Property

  Private Function GetSamplesPerFame(ByRef cMPEG As clsMPEG) As Long
    Select Case cMPEG.GetMPEGVer
      Case 1
        Select Case cMPEG.GetMPEGLayer
          Case 1 : Return 384
          Case 2, 3 : Return 1152
          Case Else : Return 0
        End Select
      Case 2, 3
        Select Case cMPEG.GetMPEGLayer
          Case 1 : Return 384
          Case 2 : Return 1152
          Case 3 : Return 576
          Case Else : Return 0
        End Select
      Case Else
        Return 0
    End Select
  End Function

  Public ReadOnly Property Bitrate() As Long
    Get
      Select Case bFmt
        Case 1, 2 : Bitrate = CLng(fLen / (Frames * GetSamplesPerFame(cMPEG) / cMPEG.GetSampleRate) * 8)
        Case 3 : Bitrate = cMPEG.GetBitrate
        Case Else : Bitrate = 0
      End Select
    End Get
  End Property

  Public ReadOnly Property Frames() As Long
    Get
      Select Case bFmt
        Case 1 : Frames = cXING.FrameCount
        Case 2 : Frames = cVBRI.Frames
        Case 3 : Frames = lSize / cMPEG.GetFrameSize
        Case Else : Frames = 0
      End Select
    End Get
  End Property

  Public ReadOnly Property MPEGStart() As Long
    Get
      MPEGStart = fStart
    End Get
  End Property

  Public ReadOnly Property MPEGLength() As Long
    Get
      MPEGLength = fLen
    End Get
  End Property

#Region "IDisposable Support"
  Private disposedValue As Boolean ' To detect redundant calls

  ' IDisposable
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        cMPEG = Nothing
        cVBRI = Nothing
        cXING = Nothing
        ' TODO: dispose managed state (managed objects).
      End If

      ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
      ' TODO: set large fields to null.
    End If
    Me.disposedValue = True
  End Sub

  ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
  'Protected Overrides Sub Finalize()
  '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
  '    Dispose(False)
  '    MyBase.Finalize()
  'End Sub

  ' This code added by Visual Basic to correctly implement the disposable pattern.
  Public Sub Dispose() Implements IDisposable.Dispose
    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region

End Class

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

Public Class clsVorbis
  Implements IDisposable

  Private Class BitReader
    Implements IDisposable
    Private bIn() As Byte
    Private lPos As ULong
    Private bitData As BitArray

    Public Property Position As ULong
      Get
        Return lPos
      End Get
      Set(value As ULong)
        lPos = value
      End Set
    End Property

    Public ReadOnly Property GetAllData() As Byte()
      Get
        Return bIn
      End Get
    End Property

    Public Sub New(array As Byte())
      bIn = array
      bitData = FlipBits(bIn)
      lPos = 0
    End Sub

    Private Function FlipBits(Bytes() As Byte) As BitArray
      Dim bTmp(Bytes.Length - 1) As Byte
      For I As Integer = 0 To Bytes.Length - 1
        For J As Integer = 0 To 7
          If ((Bytes(I) And CByte(1 << J)) <> 0) Then bTmp(I) += CByte(1 << (7 - J))
        Next
      Next
      Return New BitArray(bTmp)
    End Function

    Public Sub Append(array As Byte())
      If bIn Is Nothing Then
        bIn = array
      Else
        Dim oldLen As Long = bIn.LongLength
        ReDim Preserve bIn(oldLen + array.Length - 1)
        For I As Integer = 0 To array.Length - 1
          bIn(oldLen + I) = array(I)
        Next
      End If
      bitData = FlipBits(bIn)
    End Sub

    Public Function ReadNumber(Bits As UInt64) As ULong
      If Bits > 64 Then
        Throw New ArgumentException("Can not read over 64 bits", "Bits")
        Return 0
      End If
      Dim lLoc As UInt64 = lPos + Bits
      If lLoc > bitData.Length Then
        Throw New ArgumentException("Out of buffered data.")
        Return 0
      End If
      Dim uRet As ULong
      For I As Integer = 0 To Bits - 1
        If bitData(lPos + I) Then uRet = uRet Or (1UL << (Bits - (I + 1)))
      Next
      lPos += Bits
      Return uRet
    End Function

    Public Function ReadBytes(Bytes As UInt64) As Byte()
      Dim lLoc As UInt64 = lPos + (Bytes * 8)
      If lLoc > bitData.Length Then
        Throw New ArgumentException("Out of buffered data.")
        Return Nothing
      End If
      Dim bTmp(Bytes - 1) As Byte
      Array.ConstrainedCopy(bIn, Math.Floor(lPos / 8), bTmp, 0, Bytes)
      lPos = lLoc
      Return bTmp
    End Function

    Public Function ReadChars(Chars As Integer) As String
      Dim bTmp() As Byte = ReadBytes(Chars)
      Return System.Text.Encoding.ASCII.GetString(bTmp)
    End Function

    Public Function ReadChars(Chars As Integer, CodePage As Integer) As String
      Dim bTmp() As Byte = ReadBytes(Chars)
      Return System.Text.Encoding.GetEncoding(CodePage).GetString(bTmp)
    End Function

    Public Function ReadDWord() As String
      'Dim val As ULong = ReadNumber(32)
      Dim sTmp As String = System.Text.Encoding.ASCII.GetString(ReadBytes(4))
      sTmp = sTmp.Substring(sTmp.LastIndexOf(vbNullChar) + 1)
      Return sTmp
    End Function

    Public Shared Function ToByteArray(bits As BitArray) As Byte()
      Dim numBytes As Integer = bits.Count / 8
      If bits.Count Mod 8 <> 0 Then numBytes += 1
      Dim bytes As Byte() = New Byte(numBytes - 1) {}
      Dim byteIndex As Integer = 0, bitIndex As Integer = 0
      For i As Integer = 0 To bits.Count - 1
        If bits(i) Then bytes(byteIndex) = bytes(byteIndex) Or CByte(1 << (7 - bitIndex))
        bitIndex += 1
        If bitIndex = 8 Then
          bitIndex = 0
          byteIndex += 1
        End If
      Next
      Return bytes
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

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
      ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
      Dispose(True)
      GC.SuppressFinalize(Me)
    End Sub
#End Region
  End Class

  Private crcLookup() As UInt32 = {&H0UI, &H4C11DB7UI, &H9823B6EUI, &HD4326D9UI, &H130476DCUI, &H17C56B6BUI, &H1A864DB2UI, &H1E475005UI, &H2608EDB8UI, &H22C9F00FUI, &H2F8AD6D6UI, &H2B4BCB61UI, &H350C9B64UI, &H31CD86D3UI, &H3C8EA00AUI, &H384FBDBDUI, &H4C11DB70UI, &H48D0C6C7UI, &H4593E01EUI, &H4152FDA9UI, &H5F15ADACUI, &H5BD4B01BUI, &H569796C2UI, &H52568B75UI, &H6A1936C8UI, &H6ED82B7FUI, &H639B0DA6UI, &H675A1011UI, &H791D4014UI, &H7DDC5DA3UI, &H709F7B7AUI, &H745E66CDUI, &H9823B6E0UI, &H9CE2AB57UI, &H91A18D8EUI, &H95609039UI, &H8B27C03CUI, &H8FE6DD8BUI, &H82A5FB52UI, &H8664E6E5UI, &HBE2B5B58UI, &HBAEA46EFUI, &HB7A96036UI, &HB3687D81UI, &HAD2F2D84UI, &HA9EE3033UI, &HA4AD16EAUI, &HA06C0B5DUI, &HD4326D90UI, &HD0F37027UI, &HDDB056FEUI, &HD9714B49UI, &HC7361B4CUI, &HC3F706FBUI, &HCEB42022UI, &HCA753D95UI, &HF23A8028UI, &HF6FB9D9FUI, &HFBB8BB46UI, &HFF79A6F1UI, &HE13EF6F4UI, &HE5FFEB43UI, &HE8BCCD9AUI, &HEC7DD02DUI, &H34867077UI, &H30476DC0UI, &H3D044B19UI, &H39C556AEUI, &H278206ABUI, &H23431B1CUI, &H2E003DC5UI, &H2AC12072UI, &H128E9DCFUI, &H164F8078UI, &H1B0CA6A1UI, &H1FCDBB16UI, &H18AEB13UI, &H54BF6A4UI, &H808D07DUI, &HCC9CDCAUI, &H7897AB07UI, &H7C56B6B0UI, &H71159069UI, &H75D48DDEUI, &H6B93DDDBUI, &H6F52C06CUI, &H6211E6B5UI, &H66D0FB02UI, &H5E9F46BFUI, &H5A5E5B08UI, &H571D7DD1UI, &H53DC6066UI, &H4D9B3063UI, &H495A2DD4UI, &H44190B0DUI, &H40D816BAUI, &HACA5C697UI, &HA864DB20UI, &HA527FDF9UI, &HA1E6E04EUI, &HBFA1B04BUI, &HBB60ADFCUI, &HB6238B25UI, &HB2E29692UI, &H8AAD2B2FUI, &H8E6C3698UI, &H832F1041UI, &H87EE0DF6UI, &H99A95DF3UI, &H9D684044UI, &H902B669DUI, &H94EA7B2AUI, &HE0B41DE7UI, &HE4750050UI, &HE9362689UI, &HEDF73B3EUI, &HF3B06B3BUI, &HF771768CUI, &HFA325055UI, &HFEF34DE2UI, &HC6BCF05FUI, &HC27DEDE8UI, &HCF3ECB31UI, &HCBFFD686UI, &HD5B88683UI, &HD1799B34UI, &HDC3ABDEDUI, &HD8FBA05AUI, &H690CE0EEUI, &H6DCDFD59UI, &H608EDB80UI, &H644FC637UI, &H7A089632UI, &H7EC98B85UI, &H738AAD5CUI, &H774BB0EBUI, &H4F040D56UI, &H4BC510E1UI, &H46863638UI, &H42472B8FUI, &H5C007B8AUI, &H58C1663DUI, &H558240E4UI, &H51435D53UI, &H251D3B9EUI, &H21DC2629UI, &H2C9F00F0UI, &H285E1D47UI, &H36194D42UI, &H32D850F5UI, &H3F9B762CUI, &H3B5A6B9BUI, &H315D626UI, &H7D4CB91UI, &HA97ED48UI, &HE56F0FFUI, &H1011A0FAUI, &H14D0BD4DUI, &H19939B94UI, &H1D528623UI, &HF12F560EUI, &HF5EE4BB9UI, &HF8AD6D60UI, &HFC6C70D7UI, &HE22B20D2UI, &HE6EA3D65UI, &HEBA91BBCUI, &HEF68060BUI, &HD727BBB6UI, &HD3E6A601UI, &HDEA580D8UI, &HDA649D6FUI, &HC423CD6AUI, &HC0E2D0DDUI, &HCDA1F604UI, &HC960EBB3UI, &HBD3E8D7EUI, &HB9FF90C9UI, &HB4BCB610UI, &HB07DABA7UI, &HAE3AFBA2UI, &HAAFBE615UI, &HA7B8C0CCUI, &HA379DD7BUI, &H9B3660C6UI, &H9FF77D71UI, &H92B45BA8UI, &H9675461FUI, &H8832161AUI, &H8CF30BADUI, &H81B02D74UI, &H857130C3UI, &H5D8A9099UI, &H594B8D2EUI, &H5408ABF7UI, &H50C9B640UI, &H4E8EE645UI, &H4A4FFBF2UI, &H470CDD2BUI, &H43CDC09CUI, &H7B827D21UI, &H7F436096UI, &H7200464FUI, &H76C15BF8UI, &H68860BFDUI, &H6C47164AUI, &H61043093UI, &H65C52D24UI, &H119B4BE9UI, &H155A565EUI, &H18197087UI, &H1CD86D30UI, &H29F3D35UI, &H65E2082UI, &HB1D065BUI, &HFDC1BECUI, &H3793A651UI, &H3352BBE6UI, &H3E119D3FUI, &H3AD08088UI, &H2497D08DUI, &H2056CD3AUI, &H2D15EBE3UI, &H29D4F654UI, &HC5A92679UI, &HC1683BCEUI, &HCC2B1D17UI, &HC8EA00A0UI, &HD6AD50A5UI, &HD26C4D12UI, &HDF2F6BCBUI, &HDBEE767CUI, &HE3A1CBC1UI, &HE760D676UI, &HEA23F0AFUI, &HEEE2ED18UI, &HF0A5BD1DUI, &HF464A0AAUI, &HF9278673UI, &HFDE69BC4UI, &H89B8FD09UI, &H8D79E0BEUI, &H803AC667UI, &H84FBDBD0UI, &H9ABC8BD5UI, &H9E7D9662UI, &H933EB0BBUI, &H97FFAD0CUI, &HAFB010B1UI, &HAB710D06UI, &HA6322BDFUI, &HA2F33668UI, &HBCB4666DUI, &HB8757BDAUI, &HB5365D03UI, &HB1F740B4UI}

  Private Function CRC32(inData() As Byte) As UInt32
    Dim crc_reg As UInt32 = 0
    For I As Integer = 0 To inData.Length - 1
      Dim b As Byte = inData(I)
      crc_reg = (crc_reg << 8) Xor crcLookup(((crc_reg >> 24) And &HFF) Xor b)
    Next
    Return crc_reg
  End Function

  Private sFile As String
  Private sTitle As String
  Private sVersion As String
  Private sAlbum As String
  Private sTrack As String
  Private sArtist As String
  Private sPerformer As String
  Private sCopyright As String
  Private sLicense As String
  Private sLabel As String
  Private sDescription As String
  Private sGenre As String
  Private sDate As String
  Private sLocation As String
  Private sContact As String
  Private sISRC As String
  Private sOther() As String
  Private lVorbDuration As Double
  Private lVorbVer As Long
  Private bVorbChannels As Byte
  Private lVorbRate As Long
  Private lVorbQuality, lVorbQMin, lVorbQMax As Integer
  Private sVendor As String
  Private bolHasVorb As Boolean
  Private pctImages() As FLAC_PIC
  Private lHeaderLength As Long

  Public Structure FLAC_PIC
    Public MIME As String
    Public Descr As String
    Public PicType As Byte
    Public Size As Drawing.Size
    Public ColorDepth As UInt32
    Public ColorIndexes As UInt32
    Public Image As Drawing.Image
  End Structure

  Private Structure IDHeader
    Public Version As UInt32
    Public Channels As Byte
    Public SampleRate As UInt32
    Public MaxRate As Int32
    Public NomRate As Int32
    Public MinRate As Int32
    Public Block As Integer
    Public FrameFlag As Byte
  End Structure

  Private Enum Header_Type_Flags
    Continued = &H1
    FirstPage = &H2
    LastPage = &H4
  End Enum

  Private Structure Page
    Public Header As Long
    Public Version As Byte
    Public TypeFlag As Header_Type_Flags
    Public GranulePosition As ULong
    Public Serial As Long
    Public Sequence As Long
    Public Checksum As Long
    Public Segments As Byte
    Public Segs() As Byte
    Public HeadLen As Long
    Public Length As Long
  End Structure

  Public ReadOnly Property Album() As String
    Get
      Return sAlbum
    End Get
  End Property

  Public ReadOnly Property Artist() As String
    Get
      Return sArtist
    End Get
  End Property

  Public ReadOnly Property Contact() As String
    Get
      Return sContact
    End Get
  End Property

  Public ReadOnly Property Copyright() As String
    Get
      Return sCopyright
    End Get
  End Property

  Public ReadOnly Property Description() As String
    Get
      Return sDescription
    End Get
  End Property

  Public ReadOnly Property File_Channels() As Byte
    Get
      Return bVorbChannels
    End Get
  End Property

  Public ReadOnly Property File_Rate() As Long
    Get
      Return lVorbRate
    End Get
  End Property

  Public ReadOnly Property File_Quality As Integer
    Get
      Return lVorbQuality
    End Get
  End Property

  Public ReadOnly Property File_MinQuality As Integer
    Get
      Return lVorbQMin
    End Get
  End Property

  Public ReadOnly Property File_MaxQuality As Integer
    Get
      Return lVorbQMax
    End Get
  End Property

  Public ReadOnly Property File_Vendor() As String
    Get
      Return sVendor
    End Get
  End Property

  Public ReadOnly Property File_Version() As Long
    Get
      Return lVorbVer
    End Get
  End Property

  Public ReadOnly Property File_Duration() As Double
    Get
      Return lVorbDuration
    End Get
  End Property

  Private Sub FillInData(Raw As String)
    Dim Field As String
    Dim Data As String
    Dim I As Long
    Dim bDid As Boolean
    Field = Split(Raw, "=", 2)(0)
    Data = Split(Raw, "=", 2)(1) ' UTF8ToANSI(Mid$(Raw, Len(Field) + 2))
    Select Case UCase$(Field)
      Case "TITLE"
        If Not String.IsNullOrEmpty(sTitle) Then
          sTitle = sTitle & ", " & Data
        Else
          sTitle = Data
        End If
      Case "VERSION"
        If Not String.IsNullOrEmpty(sVersion) Then
          sVersion = sVersion & ", " & Data
        Else
          sVersion = Data
        End If
      Case "ALBUM"
        If Not String.IsNullOrEmpty(sAlbum) Then
          sAlbum = sAlbum & ", " & Data
        Else
          sAlbum = Data
        End If
      Case "TRACKNUMBER"
        If Not String.IsNullOrEmpty(sTrack) Then
          sTrack = sTrack & ", " & Data
        Else
          sTrack = Data
        End If
      Case "ARTIST"
        If Not String.IsNullOrEmpty(sArtist) Then
          sArtist = sArtist & ", " & Data
        Else
          sArtist = Data
        End If
      Case "PERFORMER"
        If Not String.IsNullOrEmpty(sPerformer) Then
          sPerformer = sPerformer & ", " & Data
        Else
          sPerformer = Data
        End If
      Case "COPYRIGHT"
        If Not String.IsNullOrEmpty(sCopyright) Then
          sCopyright = sCopyright & ", " & Data
        Else
          sCopyright = Data
        End If
      Case "LICENSE"
        If Not String.IsNullOrEmpty(sLicense) Then
          sLicense = sLicense & ", " & Data
        Else
          sLicense = Data
        End If
      Case "ORGANIZATION"
        If Not String.IsNullOrEmpty(sLabel) Then
          sLabel = sLabel & ", " & Data
        Else
          sLabel = Data
        End If
      Case "DESCRIPTION"
        If Not String.IsNullOrEmpty(sDescription) Then
          sDescription = sDescription & ", " & Data
        Else
          sDescription = Data
        End If
      Case "GENRE"
        If Not String.IsNullOrEmpty(sGenre) Then
          sGenre = sGenre & ", " & Data
        Else
          sGenre = Data
        End If
      Case "DATE"
        If Not String.IsNullOrEmpty(sDate) Then
          sDate = sDate & ", " & Data
        Else
          sDate = Data
        End If
      Case "LOCATION"
        If Not String.IsNullOrEmpty(sLocation) Then
          sLocation = sLocation & ", " & Data
        Else
          sLocation = Data
        End If
      Case "CONTACT"
        If Not String.IsNullOrEmpty(sContact) Then
          sContact = sContact & ", " & Data
        Else
          sContact = Data
        End If
      Case "ISRC"
        If Not String.IsNullOrEmpty(sISRC) Then
          sISRC = sISRC & ", " & Data
        Else
          sISRC = Data
        End If
      Case Else
        bDid = False
        If sOther IsNot Nothing Then
          For I = 0 To UBound(sOther)
            If UCase(Left$(sOther(I), InStr(sOther(I), ":") - 1)) = UCase$(Field) Then
              sOther(I) = sOther(I) & ", " & Data
              bDid = True
              Exit For
            End If
          Next I
          If Not bDid Then
            ReDim Preserve sOther(UBound(sOther) + 1)
            sOther(UBound(sOther)) = Field & ": " & Data
          End If
        Else
          ReDim sOther(0)
          sOther(0) = Field & ": " & Data
        End If
        'Debug.Print "Unknown Field type: " & Field & "=" & Data
    End Select
  End Sub

  Public ReadOnly Property Genre() As String
    Get
      Return sGenre
    End Get
  End Property

  Public ReadOnly Property HasVorbis() As Boolean
    Get
      Return bolHasVorb
    End Get
  End Property

  Public ReadOnly Property ISRC() As String
    Get
      Return sISRC
    End Get
  End Property

  Public ReadOnly Property Label() As String
    Get
      Return sLabel
    End Get
  End Property

  Public ReadOnly Property License() As String
    Get
      Return sLicense
    End Get
  End Property

  Public ReadOnly Property Pictures() As FLAC_PIC()
    Get
      Return pctImages
    End Get
  End Property

  Private Sub LoadVorbis()
    Const H_OggS As UInt32 = &H4F676753
    Const H_fLaC As UInt32 = &H664C6143
    Dim ioFile As IO.FileStream = IO.File.Open(sFile, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
    Dim lStartPos As Long = -1
    If IO.Path.GetExtension(sFile).ToLower = ".flac" Then
      lStartPos = FindLONG(ioFile, H_fLaC)
      If lStartPos = -1 Then lStartPos = FindLONG(ioFile, H_OggS)
    Else
      lStartPos = FindLONG(ioFile, H_OggS)
      If lStartPos = -1 Then lStartPos = FindLONG(ioFile, H_fLaC)
    End If
    If lStartPos > -1 Then
      ioFile.Position = lStartPos
      Dim dHeader As UInt32 = ReadLONG(ioFile)
      Select Case dHeader
        Case H_OggS
          ioFile.Position -= 4
          Do
            dHeader = ReadLONG(ioFile)
            ioFile.Position -= 4
            If dHeader = H_OggS Then
              Dim PageChunk As Page = ReadPage(ioFile)
              Dim bPage(PageChunk.Length - 1) As Byte
              ioFile.Position -= PageChunk.HeadLen
              ioFile.Read(bPage, 0, PageChunk.Length)
              ioFile.Position -= PageChunk.Length - PageChunk.HeadLen
              bPage(22) = 0
              bPage(23) = 0
              bPage(24) = 0
              bPage(25) = 0
              Dim Checksum As UInt32 = CRC32(bPage)
              If Not PageChunk.Checksum = Checksum Then
                Debug.Print("Checksum Mismatch!")
                Stop
              End If
            End If
            Dim bChunkType As Byte = ioFile.ReadByte
            Select Case bChunkType
              Case 1 'Type
                Dim sChunkID As String = ReadString(ioFile, 6)
                Select Case sChunkID
                  Case "vorbis"
                    Dim ID As IDHeader
                    ID.Version = ReadDWORD(ioFile)
                    ID.Channels = ioFile.ReadByte
                    ID.SampleRate = ReadDWORD(ioFile)
                    ID.MaxRate = ReadINT(ioFile)
                    ID.NomRate = ReadINT(ioFile)
                    ID.MinRate = ReadINT(ioFile)
                    ID.Block = ioFile.ReadByte
                    ID.FrameFlag = ioFile.ReadByte
                    lVorbVer = ID.Version
                    lVorbRate = ID.SampleRate
                    bVorbChannels = ID.Channels
                    'Debug.Print(ID.MaxRate & " > " & ID.NomRate & " > " & ID.MinRate)
                    lVorbQuality = ID.NomRate
                    lVorbQMin = ID.MinRate
                    lVorbQMax = ID.MaxRate
                  Case "video" & vbNullChar
                    Dim bTmp As Byte = ioFile.ReadByte
                    Dim bTmp2 As Byte = ioFile.ReadByte
                    Dim sCodec As String = ReadString(ioFile, 8)
                    Dim Unknown1 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown2 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown3 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown4 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown5 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown6 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown7 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown8 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown9 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown10 As UInt32 = ReadDWORD(ioFile)
                  Case "text" & vbNullChar & vbNullChar
                    Dim bTmp As Byte = ioFile.ReadByte
                    Dim bTmp2 As Byte = ioFile.ReadByte
                    Dim sCodec As String = ReadString(ioFile, 8)
                    Dim Unknown1 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown2 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown3 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown4 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown5 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown6 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown7 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown8 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown9 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown10 As UInt32 = ReadDWORD(ioFile)
                  Case Else
                    Debug.Print("Unknown Type Chunk ID: " & sChunkID)
                    Exit Do
                End Select
              Case 3 'Comment
                Dim sChunkID As String = ReadString(ioFile, 6)
                Select Case sChunkID
                  Case "vorbis"
                    bolHasVorb = True
                    Dim venLen As Long = ReadDWORD(ioFile)
                    sVendor = ReadString(ioFile, venLen)
                    Dim lCom As UInt32 = ReadDWORD(ioFile)
                    For I As Integer = 1 To lCom
                      Dim dataLen As Long = ReadDWORD(ioFile)
                      Dim sData As String = ReadString(ioFile, dataLen, System.Text.Encoding.UTF8.CodePage)
                      FillInData(sData)
                    Next I
                    Dim endByte As Byte = ioFile.ReadByte
                    If endByte <> 1 Then Debug.Print("End byte: " & Hex(endByte))
                    'Exit Do
                  Case Else
                    Debug.Print("Unknown Comment Chunk ID: " & sChunkID)
                    Exit Do
                End Select
              Case 5 'Setup 
                'search for H_OggS and find the last instance of it
                If ioFile.Length > 65535 Then ioFile.Seek(-65535, IO.SeekOrigin.End)

                Dim lastPage As Long = ioFile.Position
                Do
                  Dim thisPage As Long = FindLONG(ioFile, H_OggS, lastPage)
                  ioFile.Position = thisPage
                  Dim thisData As Page = ReadPage(ioFile)
                  If FindLONG(ioFile, H_OggS, thisPage + 4) > 0 Then
                    lastPage = thisPage + 4
                    Continue Do
                  End If
                  If (thisData.TypeFlag And Header_Type_Flags.LastPage) = Header_Type_Flags.LastPage Then
                    If thisData.GranulePosition = &HFFFFFFFFFFFFFFFFUL Then
                      'No finish on last page?
                      lVorbDuration = 0
                      Stop
                      Exit Do
                    Else
                      Dim TotalSamples As UInt64 = thisData.GranulePosition
                      lVorbDuration = TotalSamples / lVorbRate
                      Exit Do
                    End If
                  Else
                    'last page is not last page?
                    lVorbDuration = 0
                    Stop
                    Exit Do
                  End If
                Loop Until ioFile.Position >= ioFile.Length
                Exit Do
              Case 0 'Padding
    Continue Do
              Case Else
    Debug.Print("Unknown Chunk Type: " & bChunkType)
    Exit Do
            End Select
          Loop While ioFile.Position < ioFile.Length
        Case H_fLaC
          Do
            Dim bTmp(3) As Byte
            ioFile.Read(bTmp, 0, 4)
            Using bRead As New BitReader(bTmp)
              Dim LastBlock As Boolean = bRead.ReadNumber(1) = 1
              Dim BlockType As Byte = bRead.ReadNumber(7)
              Dim BlockLen As UInt32 = bRead.ReadNumber(24)
              Dim bBlock(BlockLen - 1) As Byte
              ioFile.Read(bBlock, 0, BlockLen)
              bRead.Append(bBlock)
              Select Case BlockType
                Case 0 'streaminfo
                  Dim MinSamples As UInt16 = bRead.ReadNumber(16)
                  Dim MaxSamples As UInt16 = bRead.ReadNumber(16)
                  Dim MinFrames As UInt32 = bRead.ReadNumber(24)
                  Dim MaxFrames As UInt32 = bRead.ReadNumber(24)
                  Dim SampleRate As UInt32 = bRead.ReadNumber(20)
                  lVorbRate = SampleRate
                  Dim Channels As Byte = bRead.ReadNumber(3) + 1
                  bVorbChannels = Channels
                  Dim BitsPerSample As Byte = bRead.ReadNumber(5) + 1
                  Dim SamplesInStream As UInt64 = bRead.ReadNumber(36)
                  Dim MD5Sig() As Byte = bRead.ReadBytes(16)
                Case 1 'padding
                  Debug.Print(BitConverter.ToString(bRead.GetAllData))
                  Stop
                Case 2 'app
                  Dim AppID As UInt32 = bRead.ReadNumber(32)
                  Debug.Print(BitConverter.ToString(bRead.GetAllData))
                  Stop
                Case 3 'seektable
                  Dim SeekPoints As UInt32 = BlockLen / 18
                  For I As Integer = 0 To SeekPoints - 1
                    Dim FirstSample As UInt64 = bRead.ReadNumber(64)
                    Dim Offset As UInt64 = bRead.ReadNumber(64)
                    Dim SamplesInFrame As UInt16 = bRead.ReadNumber(16)
                  Next
                Case 4 'comment
                  bolHasVorb = True
                  Dim venLen As UInt32 = SwapEndian32(bRead.ReadNumber(32))
                  sVendor = bRead.ReadChars(venLen) ' ReadString(ioFile, venLen)
                  Dim lCom As UInt32 = SwapEndian32(bRead.ReadNumber(32))
                  For I As Integer = 1 To lCom
                    Dim dataLen As UInt32 = SwapEndian32(bRead.ReadNumber(32))
                    Dim sData As String = bRead.ReadChars(dataLen, System.Text.Encoding.UTF8.CodePage)
                    FillInData(sData)
                  Next I
                Case 5 'cuesheet
                  Debug.Print(BitConverter.ToString(bRead.GetAllData))
                  Stop
                Case 6 'picture
                  Dim PicType As UInt32 = bRead.ReadNumber(32)
                  Dim MIMELen As UInt32 = bRead.ReadNumber(32)
                  Dim MIMEType As String = bRead.ReadChars(MIMELen)
                  Dim DescLen As UInt32 = bRead.ReadNumber(32)
                  Dim sDesc As String = bRead.ReadChars(DescLen, System.Text.Encoding.UTF8.CodePage)
                  Dim Width As UInt32 = bRead.ReadNumber(32)
                  Dim Height As UInt32 = bRead.ReadNumber(32)
                  Dim Depth As UInt32 = bRead.ReadNumber(32)
                  Dim ColorIndex As UInt32 = bRead.ReadNumber(32)
                  Dim PicLen As UInt32 = bRead.ReadNumber(32)
                  Dim bPic() As Byte = bRead.ReadBytes(PicLen)
                  Dim imgPic As Drawing.Image = Nothing
                  Dim sTmpPath As String = IO.Path.GetTempFileName
                  My.Computer.FileSystem.WriteAllBytes(sTmpPath, bPic, False)
                  Try
                    imgPic = PathToImg(sTmpPath)
                  Catch ex As Exception
                    Debug.Print("Image Error: " & ex.Message)
                    imgPic = Nothing
                  Finally
                    My.Computer.FileSystem.DeleteFile(sTmpPath)
                  End Try
                  If imgPic.Width <> Width Or imgPic.Height <> Height Then
                    Debug.Print("Size Mismatch!")
                    Stop
                  End If
                  If imgPic IsNot Nothing Then
                    Dim pctImgCount As Integer
                    If pctImages Is Nothing Then
                      ReDim pctImages(0)
                      pctImgCount = 0
                    Else
                      pctImgCount = pctImages.Length
                      ReDim Preserve pctImages(pctImgCount)
                    End If
                    With pctImages(pctImgCount)
                      .PicType = PicType
                      .MIME = MIMEType
                      .Descr = sDesc
                      .Size = New Drawing.Size(Width, Height)
                      .ColorDepth = Depth
                      .ColorIndexes = ColorIndex
                      .Image = imgPic
                    End With
                  End If
                  'Stop
                Case Else
                  Debug.Print("Unknown block type " & BlockType)
                  Debug.Print(BitConverter.ToString(bRead.GetAllData))
                  Stop
              End Select
              If LastBlock Or bolHasVorb Then Exit Do
            End Using
          Loop
        Case Else
          bolHasVorb = False
          Debug.Print("Invalid format: " & Hex(dHeader))
      End Select
      lHeaderLength = ioFile.Position
    End If
    ioFile.Close()
  End Sub

  Private Function ReadPage(ByRef ioFile As IO.FileStream) As Page
    Dim pg As Page
    pg.Header = ReadLONG(ioFile)
    pg.Version = ioFile.ReadByte
    pg.TypeFlag = ioFile.ReadByte
    pg.GranulePosition = ReadQWORD(ioFile)
    pg.Serial = ReadDWORD(ioFile)
    pg.Sequence = ReadDWORD(ioFile)
    pg.Checksum = ReadDWORD(ioFile)
    pg.Segments = ioFile.ReadByte
    pg.HeadLen = 27 + pg.Segments
    pg.Length = pg.HeadLen
    ReDim pg.Segs(pg.Segments)
    For I = 0 To pg.Segments - 1
      pg.Segs(I) = ioFile.ReadByte
      pg.Length = pg.Length + pg.Segs(I)
    Next I
    Return pg
  End Function

  Public ReadOnly Property Location() As String
    Get
      Return sLocation
    End Get
  End Property

  Public ReadOnly Property File() As String
    Get
      Return sFile
    End Get
  End Property

  Public Sub New(File As String)
    sFile = File
    ResetData()
    LoadVorbis()
  End Sub

  Public ReadOnly Property Other() As String
    Get
      Return Join(sOther, vbNewLine)
    End Get
  End Property

  Public ReadOnly Property Performer() As String
    Get
      Return sPerformer
    End Get
  End Property

  Public ReadOnly Property RecordDate() As String
    Get
      Return sDate
    End Get
  End Property

  Private Sub ResetData()
    sTitle = vbNullString
    sVersion = vbNullString
    sAlbum = vbNullString
    sTrack = vbNullString
    sArtist = vbNullString
    sPerformer = vbNullString
    sCopyright = vbNullString
    sLicense = vbNullString
    sLabel = vbNullString
    sDescription = vbNullString
    sGenre = vbNullString
    sDate = vbNullString
    sLocation = vbNullString
    sContact = vbNullString
    sISRC = vbNullString
    lVorbVer = 0
    bVorbChannels = 0
    lVorbRate = 0
    lVorbDuration = 0.0
    sVendor = vbNullString
  End Sub

  Public ReadOnly Property Title() As String
    Get
      Return sTitle
    End Get
  End Property

  Public ReadOnly Property Track() As String
    Get
      Return sTrack
    End Get
  End Property

  Public ReadOnly Property Version() As String
    Get
      Return sVersion
    End Get
  End Property

  Public ReadOnly Property HeaderLength As Long
    Get
      Return lHeaderLength
    End Get
  End Property

  Private Function FindLONG(ioFile As IO.FileStream, toFind As UInt32) As Int64
    Const ChunkSize As Integer = 256
    Const FindSize As Integer = 4
    Dim ioPos As Long = ioFile.Position
    Dim ioFound As Int64 = -1
    Dim bFind As Byte = (toFind And &HFF000000UL) >> 24
    For I As Long = 0 To ioFile.Length - 1 Step ChunkSize
      ioFile.Seek(I, IO.SeekOrigin.Begin)
      Dim bChunk(ChunkSize - 1) As Byte
      ioFile.Read(bChunk, 0, ChunkSize)
      If bChunk.Contains(bFind) Then
        Dim iTryLoc As Integer = 0
        Do Until iTryLoc > ChunkSize
          Dim newLoc As Integer = Array.IndexOf(bChunk, bFind, iTryLoc)
          If newLoc = -1 Then Exit Do
          ioFile.Seek(I + newLoc, IO.SeekOrigin.Begin)
          Dim bData(FindSize - 1) As Byte
          If ioFile.Read(bData, 0, FindSize) <> 0 Then
            If GetDWORD(bData, 0) = toFind Then
              ioFound = ioFile.Position - FindSize
              Exit For
            End If
          End If
          iTryLoc = newLoc + 1
        Loop
      End If
    Next
    ioFile.Position = ioPos
    Return ioFound
  End Function

  Private Function FindLONG(ioFile As IO.FileStream, toFind As UInt32, startingPoint As Long) As Int64
    Const ChunkSize As Integer = 256
    Const FindSize As Integer = 4
    Dim ioPos As Long = ioFile.Position
    Dim ioFound As Int64 = -1
    Dim bFind As Byte = (toFind And &HFF000000UL) >> 24
    For I As Long = startingPoint To ioFile.Length - 1 Step ChunkSize
      ioFile.Seek(I, IO.SeekOrigin.Begin)
      Dim bChunk(ChunkSize - 1) As Byte
      ioFile.Read(bChunk, 0, ChunkSize)
      If bChunk.Contains(bFind) Then
        Dim iTryLoc As Integer = 0
        Do Until iTryLoc > ChunkSize
          Dim newLoc As Integer = Array.IndexOf(bChunk, bFind, iTryLoc)
          If newLoc = -1 Then Exit Do
          ioFile.Seek(I + newLoc, IO.SeekOrigin.Begin)
          Dim bData(FindSize - 1) As Byte
          If ioFile.Read(bData, 0, FindSize) <> 0 Then
            If GetDWORD(bData, 0) = toFind Then
              ioFound = ioFile.Position - FindSize
              Exit For
            End If
          End If
          iTryLoc = newLoc + 1
        Loop
      End If
    Next
    ioFile.Position = ioPos
    Return ioFound
  End Function

  Private Function ReadLONG(ioFile As IO.FileStream) As UInt32
    Dim bData(3) As Byte
    If ioFile.Read(bData, 0, 4) <> 0 Then
      Return GetDWORD(bData, 0)
    Else
      Return 0
    End If
  End Function

  Private Function ReadINT(ioFile As IO.FileStream) As Int32
    Dim bData(3) As Byte
    If ioFile.Read(bData, 0, 4) <> 0 Then
      Return BitConverter.ToInt32(bData, 0)
    Else
      Return 0
    End If
  End Function

  Private Function ReadUSHORT(ioFile As IO.FileStream) As UInt16
    Dim bData(1) As Byte
    If ioFile.Read(bData, 0, 2) <> 0 Then
      Return BitConverter.ToUInt16(bData, 0)
    Else
      Return 0
    End If
  End Function

  Private Function ReadDWORD(ioFile As IO.FileStream) As UInt32
    Dim bData(3) As Byte
    If ioFile.Read(bData, 0, 4) <> 0 Then
      Return BitConverter.ToUInt32(bData, 0)
    Else
      Return 0
    End If
  End Function

  Private Function ReadQWORD(ioFile As IO.FileStream) As UInt64
    Dim bData(7) As Byte
    If ioFile.Read(bData, 0, 8) <> 0 Then
      Return BitConverter.ToUInt64(bData, 0)
    Else
      Return 0
    End If
  End Function

  Private Function ReadString(ioFile As IO.FileStream, Length As UInt32, Optional codepage As Integer = LATIN_1) As String
    Dim bData(Length - 1) As Byte
    If ioFile.Read(bData, 0, Length) <> 0 Then
      Return System.Text.Encoding.GetEncoding(codepage).GetString(bData)
    Else
      Return 0
    End If
  End Function

  Private Function PathToImg(Path As String) As Drawing.Image
    If My.Computer.FileSystem.FileExists(Path) Then
      Using iStream As New IO.FileStream(Path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
        Return Drawing.Image.FromStream(iStream, True, True)
      End Using
    Else
      Return Nothing
    End If
  End Function

#Region "IDisposable Support"
  Private disposedValue As Boolean ' To detect redundant calls

  ' IDisposable
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        ' TODO: dispose managed state (managed objects).
        ResetData()
      End If

      ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
      ' TODO: set large fields to null.
    End If
    Me.disposedValue = True
  End Sub

  ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
  'Protected Overrides Sub Finalize()
  '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
  '    Dispose(False)
  '    MyBase.Finalize()
  'End Sub

  ' This code added by Visual Basic to correctly implement the disposable pattern.
  Public Sub Dispose() Implements IDisposable.Dispose
    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region
End Class

Public Class clsMIDI
  Implements IDisposable

  Private Structure TrackHeader
    Public Format As TrackType
    Public TrackCount As UInt16
    Public TrackTime As UInt16
  End Structure
  Public Enum TrackType
    SingleTrack = 0
    MultiSynch = 1
    MultiAsynch = 2
    Unknown
  End Enum
  Public Structure TrackChunk
    Public Structure TimeSignature
      Public Numerator As Byte
      Public Denominator As Byte
      Public TicksPerClick As Byte
      Public ThirtySecondsPerQuarter As Byte
    End Structure
    Public Structure KeySignature
      Public SharpsFlats As Byte
      Public MajorMinor As Byte
    End Structure
    Public SequenceNumber As UInt16
    Public TextEvent As String
    Public Copyright As String
    Public TrackName As String
    Public TrackInstrument As String
    Public TrackLyric As String
    Public TrackMarker As String
    Public TrackCuePoint As String
    Public ChannelPrefix As Byte
    Public PortPrefix As Byte
    Public TrackTempo As UInt32
    Public SMPTEOffset As UInt64
    Public TrackTime As TimeSignature
    Public TrackSignature As KeySignature
    Public SequencerInfo() As Byte
    'Audio track data...
  End Structure
  Private bValid As Boolean
  Private mHeader As TrackHeader
  Private mChunks() As TrackChunk
  Private Const LOCALE As Integer = LATIN_1

  Public ReadOnly Property IsMIDI As Boolean
    Get
      Return bValid
    End Get
  End Property

  Public ReadOnly Property TrackFormat As TrackType
    Get
      Return mHeader.Format
    End Get
  End Property

  Public ReadOnly Property TrackCount As UInt16
    Get
      Return mHeader.TrackCount
    End Get
  End Property

  Public ReadOnly Property TrackTime As UInt16
    Get
      Return mHeader.TrackTime
    End Get
  End Property

  Public ReadOnly Property Tracks As TrackChunk()
    Get
      Return mChunks
    End Get
  End Property

  Public Sub New(FilePath As String)
    bValid = False
    If String.IsNullOrEmpty(FilePath) Then Exit Sub
    If Not My.Computer.FileSystem.FileExists(FilePath) Then Exit Sub
    If My.Computer.FileSystem.GetFileInfo(FilePath).Length >= 1024L * 1024L * 1024L * 4L Then Exit Sub
    Dim bFile() As Byte = My.Computer.FileSystem.ReadAllBytes(FilePath)
    Dim lPos As Long = 0
    ReadHeader(bFile, lPos)
    ReDim mChunks(mHeader.TrackCount - 1)
    For I As Integer = 0 To mHeader.TrackCount - 1
      mChunks(I) = ReadTrack(bFile, lPos)
      If Not bValid Then Exit For
    Next
  End Sub

  Private Sub ReadHeader(ByRef bFile() As Byte, ByRef lPos As Long)
    Select Case GetDWORD(bFile, lPos)
      Case &H4D546864
        lPos += 4
        bValid = True
        Dim uLen As UInt32 = GetDWORD(bFile, lPos)
        lPos += 4
        If uLen = 6 Then
          Dim iTrackFormat As UInt16 = GetWORD(bFile, lPos)
          If iTrackFormat < 3 Then
            mHeader.Format = iTrackFormat
          Else
            mHeader.Format = TrackType.Unknown
          End If
          lPos += 2
          mHeader.TrackCount = GetWORD(bFile, lPos)
          lPos += 2
          mHeader.TrackTime = GetWORD(bFile, lPos)
          lPos += 2
        Else
          bValid = False
          Exit Sub
        End If
      Case &H52494646
        Debug.Print("RMI")
        lPos += 20
        ReadHeader(bFile, lPos)
      Case Else
        lPos += 4
        bValid = False
        Exit Sub
    End Select
  End Sub

  Private Function ReadTrack(ByRef bFile() As Byte, ByRef lPos As Long) As TrackChunk
    If GetDWORD(bFile, lPos) = &H4D54726B Then
      lPos += 4
      Dim newChunk As New TrackChunk
      Dim uLen As UInt32 = GetDWORD(bFile, lPos)
      lPos += 4
      'Debug.Print("Track Length: " & uLen)
      Dim lLen As UInt32 = lPos + uLen
      Do
        Dim vTime As UInt64 = ReadVar(bFile, lPos) '= bFile(lPos)
        'lPos += 1
        'Debug.Print(" Time Delta: " & vTime)
        Dim CommandByte As Byte = bFile(lPos)
        lPos += 1
        'Debug.Print(" CommandByte: " & Hex(CommandByte))
        If CommandByte = &HFF Then
          Dim MetaEvent As Byte = bFile(lPos)
          lPos += 1
          Dim MetaLen As UInt64 = ReadVar(bFile, lPos)
          'lPos += 1
          'Debug.Print("  Meta Event " & Hex(MetaEvent))
          Select Case MetaEvent
            Case &H0
              If MetaLen = 2 Then
                newChunk.SequenceNumber = GetWORD(bFile, lPos)
                lPos += 4
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H1
              If String.IsNullOrEmpty(newChunk.TextEvent) Then
                newChunk.TextEvent = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.TextEvent &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H2
              If String.IsNullOrEmpty(newChunk.Copyright) Then
                newChunk.Copyright = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.Copyright &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H3
              If String.IsNullOrEmpty(newChunk.TrackName) Then
                newChunk.TrackName = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.TrackName &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H4
              If String.IsNullOrEmpty(newChunk.TrackInstrument) Then
                newChunk.TrackInstrument = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.TrackInstrument &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H5
              If String.IsNullOrEmpty(newChunk.TrackLyric) Then
                newChunk.TrackLyric = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.TrackLyric &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H6
              If String.IsNullOrEmpty(newChunk.TrackMarker) Then
                newChunk.TrackMarker = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.TrackMarker &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H7
              If String.IsNullOrEmpty(newChunk.TrackCuePoint) Then
                newChunk.TrackCuePoint = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.TrackCuePoint &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H20
              If MetaLen = 1 Then
                newChunk.ChannelPrefix = bFile(lPos)
                lPos += 1
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H21
              If MetaLen = 1 Then
                newChunk.PortPrefix = bFile(lPos)
                lPos += 1
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H2F
              If MetaLen = 0 Then
                Exit Do
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H51
              If MetaLen = 3 Then
                newChunk.TrackTempo = GetWORDandBYTE(bFile, lPos)
                lPos += 3
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H54
              Dim Unknown(7) As Byte
              Array.ConstrainedCopy(bFile, lPos, Unknown, 7 - MetaLen, MetaLen)
              lPos += MetaLen
              newChunk.SMPTEOffset = BitConverter.ToUInt64(Unknown, 0)
              'Debug.Print("Unknown Event: " & Hex(MetaEvent) & ": " & BitConverter.ToString(Unknown))
            Case &H58
              If MetaLen = 4 Then
                newChunk.TrackTime.Numerator = bFile(lPos)
                lPos += 1
                newChunk.TrackTime.Denominator = bFile(lPos)
                lPos += 1
                newChunk.TrackTime.TicksPerClick = bFile(lPos)
                lPos += 1
                newChunk.TrackTime.ThirtySecondsPerQuarter = bFile(lPos)
                lPos += 1
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H59
              If MetaLen = 2 Then
                newChunk.TrackSignature.SharpsFlats = bFile(lPos)
                lPos += 1
                newChunk.TrackSignature.MajorMinor = bFile(lPos)
                lPos += 1
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H7F
              ReDim newChunk.SequencerInfo(MetaLen - 1)
              Array.ConstrainedCopy(bFile, lPos, newChunk.SequencerInfo, 0, MetaLen)
              lPos += MetaLen
            Case Else
              Dim Unknown(MetaLen - 1) As Byte
              Array.ConstrainedCopy(bFile, lPos, Unknown, 0, MetaLen)
              lPos += MetaLen
              'Debug.Print("    Unknown Event: " & Hex(MetaEvent) & ": " & BitConverter.ToString(Unknown))
          End Select
        Else
          Dim Command As Byte = (&HF0 And CommandByte) >> 4
          Dim Channel As Byte = (&HF And CommandByte) '<< 4
          'Debug.Print("   Command: " & Hex(Command) & ", Channel: " & Channel)
          Select Case Command
            Case &H8
              Dim Note As Byte = bFile(lPos)
              lPos += 1
              Dim Velocity As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " note " & GetNote(Note) & " off [" & Velocity & "]")
            Case &H9
              Dim Note As Byte = bFile(lPos)
              lPos += 1
              Dim Velocity As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " note " & GetNote(Note) & " on [" & Velocity & "]")
            Case &HA
              Dim Key As Byte = bFile(lPos)
              lPos += 1
              Dim Velocity As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " note " & GetNote(Key) & " after-touch [" & Velocity & "]")
            Case &HB
              Dim Controller As Byte = bFile(lPos)
              lPos += 1
              Dim Velocity As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " control " & Hex(Controller) & " change to " & Velocity)
            Case &HC
              Dim Program As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " program " & Hex(Program) & " change")
            Case &HD
              Dim chanNum As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " chan " & Hex(chanNum) & " after-touch")
            Case &HE
              Dim Bottom As Byte = bFile(lPos)
              lPos += 1
              Dim Top As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " pitch wheel change " & Hex(Bottom) & " " & Hex(Top))
            Case Else
              'Debug.Print("    Unknown: " & Hex(Command) & " " & Channel & " " & Hex(CommandByte) & ", skipping ahead...")
              'Debug.Print("Error at " & lPos)
              Do
                If lPos >= lLen Then
                  lPos = lLen
                  Exit Do
                End If
                If bFile(lPos) And &H80 Then
                  lPos -= 1
                  Exit Do
                Else
                  lPos += 1
                End If
              Loop

              'bValid = False
              'Return newChunk
          End Select
        End If

      Loop Until lPos >= lLen
      Debug.Print(lPos & " " & lLen)
      lPos = lLen
      Return newChunk
    Else
      lPos += 4
      bValid = False
      Return Nothing
    End If
  End Function

  Private Function ReadVar(ByRef bFile() As Byte, ByRef lPos As Long) As UInt64
    Dim val As UInt32
    Dim c As Byte
    val = bFile(lPos)
    lPos += 1
    If val And &H80 Then
      val = val And &H7F
      Do
        c = bFile(lPos)
        lPos += 1
        val = (val << 7) + (c And &H7F)
      Loop While (c And &H80)
    End If
    Return val
  End Function

  Private Function GetWORDandBYTE(bIn() As Byte, Optional ByVal lStart As Long = 0) As UInt32
    Dim bTmp(3) As Byte

    If lStart + 2 >= bIn.Length Then
      bTmp(0) = 0
    Else
      bTmp(0) = bIn(lStart + 2)
    End If

    If lStart + 1 >= bIn.Length Then
      bTmp(1) = 0
    Else
      bTmp(1) = bIn(lStart + 1)
    End If
    If lStart >= bIn.Length Then
      bTmp(2) = 0
    Else
      bTmp(2) = bIn(lStart)
    End If
    Return BitConverter.ToUInt32(bTmp, 0)
  End Function

  Private Function GetNote(Number As Byte) As String
    Dim Row As Byte = Number \ 12
    Dim Col As Byte = Number Mod 12
    Select Case Col
      Case 0 : Return "C (Octave " & Row & ")"
      Case 1 : Return "C# (Octave " & Row & ")"
      Case 2 : Return "D (Octave " & Row & ")"
      Case 3 : Return "D# (Octave " & Row & ")"
      Case 4 : Return "E (Octave " & Row & ")"
      Case 5 : Return "F (Octave " & Row & ")"
      Case 6 : Return "F# (Octave " & Row & ")"
      Case 7 : Return "G (Octave " & Row & ")"
      Case 8 : Return "G# (Octave " & Row & ")"
      Case 9 : Return "A (Octave " & Row & ")"
      Case 10 : Return "A (Octave " & Row & ")"
      Case 11 : Return "B (Octave " & Row & ")"
      Case Else : Return "Unknown " & Col & " (Octave " & Row & ")"
    End Select
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

  ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
  'Protected Overrides Sub Finalize()
  '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
  '    Dispose(False)
  '    MyBase.Finalize()
  'End Sub

  ' This code added by Visual Basic to correctly implement the disposable pattern.
  Public Sub Dispose() Implements IDisposable.Dispose
    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region
End Class