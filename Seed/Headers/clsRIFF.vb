Public Class clsRIFF
  Implements IDisposable
  Private Structure ChunkHeader
    Public ChunkID As String
    Public ChunkSize As UInt32
    Public Format As String
  End Structure
  Private Structure Chunk
    Public Header As ChunkHeader
    Public Data As Byte()
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
    Public StreamName As String
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
    Public Extra As Byte()
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
    Public Sub New(data As Byte())
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
    <StringValue("XVID")> AVI_FORMAT_XVID = &H44495658
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
    If String.IsNullOrEmpty(FilePath) Then Return
    If Not My.Computer.FileSystem.FileExists(FilePath) Then Return
    Using ioFile As New IO.BinaryReader(New IO.FileStream(FilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read))
      Dim mChunk As New Chunk
      mChunk.Header.ChunkID = ioFile.ReadChars(4)
      If Not mChunk.Header.ChunkID = "RIFF" Then Return
      mChunk.Header.ChunkSize = ioFile.ReadUInt32
      mChunk.Header.Format = ioFile.ReadChars(4)
      Select Case mChunk.Header.Format
        Case "WAVE"
          'WAVEFORMAT
          Do While ioFile.BaseStream.Position < mChunk.Header.ChunkSize
            Dim wavChunk As New Chunk
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
                      Return
                  End Select
                Case "data"
                  Dim firstFour As Byte() = ioData.ReadBytes(4)
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
                      Dim bDTSa As Byte() = BytesTo14BitL(ioData.ReadBytes(24))
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
                        Return
                      End If
                    Case Else
                      ioData.BaseStream.Position -= 4
                      Dim bDTSa As Byte() = BytesTo14BitL(ioData.ReadBytes(16))
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
            Dim aviChunk As New Chunk
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
                      Dim mainChunk As New Chunk
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
                                  Dim streamChunk As New Chunk
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
                                            Dim vidsChunk As New Chunk
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
                                            Dim audsChunk As New Chunk
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
                                              If Not ioAuds.BaseStream.Position = ioAuds.BaseStream.Length Then
                                                wavInfo.cbSize = ioAuds.ReadUInt16
                                              End If
                                              If aviWAV Is Nothing Then aviWAV = New List(Of WAVEFORMATEX)
                                              aviWAV.Add(wavInfo)
                                            End Using
                                          Case "mids"
                                            Dim midsChunk As New Chunk
                                            midsChunk.Header.ChunkID = ioMain.ReadChars(4)
                                            midsChunk.Header.ChunkSize = ioMain.ReadUInt32
                                            ioMain.BaseStream.Position += midsChunk.Header.ChunkSize
                                          Case "txts"
                                            Dim txtsChunk As New Chunk
                                            txtsChunk.Header.ChunkID = ioMain.ReadChars(4)
                                            txtsChunk.Header.ChunkSize = ioMain.ReadUInt32
                                            ioMain.BaseStream.Position += txtsChunk.Header.ChunkSize
                                          Case "JUNK"
                                            'Junk Data
                                            Dim junkChunk As New Chunk
                                            junkChunk.Header.ChunkID = ioMain.ReadChars(4)
                                            junkChunk.Header.ChunkSize = ioMain.ReadUInt32
                                            ioMain.BaseStream.Position += junkChunk.Header.ChunkSize
                                          Case Else
                                            Debug.Print("Unknown AVI Stream Chunk ID: " & streamHeader.fccType)
                                            Dim unknownChunk As New Chunk
                                            unknownChunk.Header.ChunkID = ioMain.ReadChars(4)
                                            unknownChunk.Header.ChunkSize = ioMain.ReadUInt32
                                            ioMain.BaseStream.Position += unknownChunk.Header.ChunkSize
                                        End Select
                                      Case "strn"
                                        Dim aStream = aviStreams(aviStreams.Count - 1)
                                        If Array.IndexOf(Of Byte)(streamChunk.Data, 0) = -1 Then
                                          aStream.StreamName = GetString(streamChunk.Data, 0, streamChunk.Data.Length)
                                        Else
                                          aStream.StreamName = GetString(streamChunk.Data, 0, Array.IndexOf(Of Byte)(streamChunk.Data, 0))
                                        End If
                                        aviStreams(aviStreams.Count - 1) = aStream
                                        If ioMain.PeekChar = 0 Then ioMain.ReadByte()
                                      Case Else
                                        If ioMain.PeekChar = 0 Then ioMain.ReadByte()
                                    End Select
                                  End Using
                                Loop
                              Case "odml"
                                Do While ioMain.BaseStream.Position < mainChunk.Header.ChunkSize
                                  Dim dmlChunk As New Chunk
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
                      Dim infoChunk As New Chunk
                      infoChunk.Header.ChunkID = ioFile.ReadChars(4)
                      infoChunk.Header.ChunkSize = ioFile.ReadUInt32
                      If infoChunk.Header.ChunkSize = 0 Then Continue Do
                      infoChunk.Header.Format = ioFile.ReadChars(infoChunk.Header.ChunkSize)
                      If infoChunk.Header.Format.Length = 0 Then Continue Do
                      If infoChunk.Header.Format.Substring(infoChunk.Header.Format.Length - 1, 1) = vbNullChar Then infoChunk.Header.Format = infoChunk.Header.Format.Substring(0, infoChunk.Header.Format.Length - 1)
                      aviINFO.Add(infoChunk.Header.ChunkID, infoChunk.Header.Format)
                      'IS THERE JUST ONE EXTRA BYTE AT THE END OF THIS CHUNK?
                      If ioFile.BaseStream.Position - aviOffset = aviChunk.Header.ChunkSize - 1 Then ioFile.ReadByte()

                      'SOME SPECIFIC REQUIREMENT FOR READING ANOTHER BYTE EXISTS HERE
                      ' WHAT IS IT?
                      ' WHY DOES IT EXIST?
                      ' DOES IT MEAN SOME ADDITIONAL DATA MAY EXIST OR IS IT ALWAYS NULL?
                      If ioFile.PeekChar = 0 Then
                        'Stop
                        ioFile.ReadByte()
                        Debug.Print("Extra Null Byte at end of this sub-chunk, but not the end of this whole chunk?")
                      End If
                    Loop
                  Case "movi"
                    ioFile.BaseStream.Position += aviChunk.Header.ChunkSize - 4


                    'Do While ioFile.BaseStream.Position - aviOffset < aviChunk.Header.ChunkSize
                    '  Do Until ioFile.ReadByte > 0
                    '    If ioFile.BaseStream.Position - aviOffset >= aviChunk.Header.ChunkSize Then Exit Do
                    '  Loop
                    '  If ioFile.BaseStream.Position- aviOffset >= aviChunk.Header.ChunkSize Then Exit Do
                    '  ioFile.BaseStream.Position -= 1

                    '  Dim moviChunk as new chunk
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
          Return
      End Select
      If ioFile.BaseStream.Position < ioFile.BaseStream.Length Then
        Dim idvxChunk As New Chunk
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

  Private Function BytesTo14BitL(inBytes As Byte()) As Byte()
    Dim bitPairs As Byte() = Nothing
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
  Private Function ReadBits(bData As Byte(), size As Integer) As UInt32
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
  Private disposedValue As Boolean 
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
      End If
    End If
    Me.disposedValue = True
  End Sub

  Public Sub Dispose() Implements IDisposable.Dispose
    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region

End Class
