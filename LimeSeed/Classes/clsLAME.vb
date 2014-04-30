Imports System.Runtime.InteropServices
Imports System.Runtime.Serialization
Imports System.IO
Public Enum WaveFormats
  Pcm = 1
  Float = 3
End Enum
<StructLayout(LayoutKind.Sequential)> _
Public Class WaveFormat
  Public wFormatTag As Short
  Public nChannels As Short
  Public nSamplesPerSec As Integer
  Public nAvgBytesPerSec As Integer
  Public nBlockAlign As Short
  Public wBitsPerSample As Short
  Public cbSize As Short
  Public Sub New(rate As Integer, bits As Integer, channels As Integer)
    wFormatTag = CShort(WaveFormats.Pcm)
    nChannels = CShort(channels)
    nSamplesPerSec = rate
    wBitsPerSample = CShort(bits)
    cbSize = 0
    nBlockAlign = CShort(channels * (bits \ 8))
    nAvgBytesPerSec = nSamplesPerSec * nBlockAlign
  End Sub
End Class
Public Enum VBRMETHOD As Integer
  VBR_METHOD_NONE = -1
  VBR_METHOD_DEFAULT = 0
  VBR_METHOD_OLD = 1
  VBR_METHOD_NEW = 2
  VBR_METHOD_MTRH = 3
  VBR_METHOD_ABR = 4
End Enum
' MPEG modes 
Public Enum MpegMode As UInteger
  STEREO = 0
  JOINT_STEREO
  DUAL_CHANNEL
  ' LAME doesn't supports this! 
  MONO
  NOT_SET
  MAX_INDICATOR
  ' Don't use this! It's used for sanity checks. 
End Enum
Public Enum LAME_QUALITY_PRESET As Integer
  LQP_NOPRESET = -1
  ' QUALITY PRESETS
  LQP_NORMAL_QUALITY = 0
  LQP_LOW_QUALITY = 1
  LQP_HIGH_QUALITY = 2
  LQP_VOICE_QUALITY = 3
  LQP_R3MIX = 4
  LQP_VERYHIGH_QUALITY = 5
  LQP_STANDARD = 6
  LQP_FAST_STANDARD = 7
  LQP_EXTREME = 8
  LQP_FAST_EXTREME = 9
  LQP_INSANE = 10
  LQP_ABR = 11
  LQP_CBR = 12
  LQP_MEDIUM = 13
  LQP_FAST_MEDIUM = 14
  ' NEW PRESET VALUES
  LQP_PHONE = 1000
  LQP_SW = 2000
  LQP_AM = 3000
  LQP_FM = 4000
  LQP_VOICE = 5000
  LQP_RADIO = 6000
  LQP_TAPE = 7000
  LQP_HIFI = 8000
  LQP_CD = 9000
  LQP_STUDIO = 10000
End Enum
<StructLayout(LayoutKind.Sequential), Serializable()> _
Public Structure MP3
  'BE_CONFIG_MP3
  Public dwSampleRate As UInteger
  ' 48000, 44100 and 32000 allowed
  Public byMode As Byte
  ' BE_MP3_MODE_STEREO, BE_MP3_MODE_DUALCHANNEL, BE_MP3_MODE_MONO
  Public wBitrate As UShort
  ' 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256 and 320 allowed
  Public bPrivate As Integer
  Public bCRC As Integer
  Public bCopyright As Integer
  Public bOriginal As Integer
End Structure
<StructLayout(LayoutKind.Sequential, Size:=327), Serializable()> _
Public Structure LHV1
  ' BE_CONFIG_LAME LAME header version 1
  Public Const MPEG1 As UInteger = 1
  Public Const MPEG2 As UInteger = 0

  ' STRUCTURE INFORMATION
  Public dwStructVersion As UInteger
  Public dwStructSize As UInteger
  ' BASIC ENCODER SETTINGS
  Public dwSampleRate As UInteger
  ' SAMPLERATE OF INPUT FILE
  Public dwReSampleRate As UInteger
  ' DOWNSAMPLERATE, 0=ENCODER DECIDES  
  Public nMode As MpegMode
  ' STEREO, MONO
  Public dwBitrate As UInteger
  ' CBR bitrate, VBR min bitrate
  Public dwMaxBitrate As UInteger
  ' CBR ignored, VBR Max bitrate
  Public nPreset As LAME_QUALITY_PRESET
  ' Quality preset
  Public dwMpegVersion As UInteger
  ' MPEG-1 OR MPEG-2
  Public dwPsyModel As UInteger
  ' FUTURE USE, SET TO 0
  Public dwEmphasis As UInteger
  ' FUTURE USE, SET TO 0
  ' BIT STREAM SETTINGS
  Public bPrivate As Integer
  ' Set Private Bit (TRUE/FALSE)
  Public bCRC As Integer
  ' Insert CRC (TRUE/FALSE)
  Public bCopyright As Integer
  ' Set Copyright Bit (TRUE/FALSE)
  Public bOriginal As Integer
  ' Set Original Bit (TRUE/FALSE)
  ' VBR STUFF
  Public bWriteVBRHeader As Integer
  ' WRITE XING VBR HEADER (TRUE/FALSE)
  Public bEnableVBR As Integer
  ' USE VBR ENCODING (TRUE/FALSE)
  Public nVBRQuality As Integer
  ' VBR QUALITY 0..9
  Public dwVbrAbr_bps As UInteger
  ' Use ABR in stead of nVBRQuality
  Public nVbrMethod As VBRMETHOD
  Public bNoRes As Integer
  ' Disable Bit resorvoir (TRUE/FALSE)
  ' MISC SETTINGS
  Public bStrictIso As Integer
  ' Use strict ISO encoding rules (TRUE/FALSE)
  Public nQuality As UShort
  ' Quality Setting, HIGH BYTE should be NOT LOW byte, otherwhise quality=5
  ' FUTURE USE, SET TO 0, align strucutre to 331 bytes
  '[ MarshalAs( UnmanagedType.ByValArray, SizeConst=255-4*4-2 )]
  'public byte[]   btReserved;//[255-4*sizeof(DWORD) - sizeof( WORD )];
  Public Sub New(wformat As WaveFormat, MpeBitRate As UInteger)
    If wformat.wFormatTag <> CShort(WaveFormats.Pcm) Then
      Throw New ArgumentOutOfRangeException("format", "Only PCM format supported")
    End If
    If wformat.wBitsPerSample <> 16 Then
      Throw New ArgumentOutOfRangeException("format", "Only 16 bits samples supported")
    End If
    dwStructVersion = 1
    dwStructSize = CUInt(Marshal.SizeOf(GetType(BE_CONFIG)))
    Select Case wformat.nSamplesPerSec
      Case 16000, 22050, 24000
        dwMpegVersion = MPEG2
        Exit Select
      Case 32000, 44100, 48000
        dwMpegVersion = MPEG1
        Exit Select
      Case Else
        Throw New ArgumentOutOfRangeException("format", "Unsupported sample rate")
    End Select
    dwSampleRate = CUInt(wformat.nSamplesPerSec)
    ' INPUT FREQUENCY
    dwReSampleRate = 0
    ' DON'T RESAMPLE
    Select Case wformat.nChannels
      Case 1
        nMode = MpegMode.MONO
        Exit Select
      Case 2
        nMode = MpegMode.STEREO
        Exit Select
      Case Else
        Throw New ArgumentOutOfRangeException("format", "Invalid number of channels")
    End Select
    Select Case MpeBitRate
      Case 32, 40, 48, 56, 64, 80, _
       96, 112, 128, 160
        'Allowed bit rates in MPEG1 and MPEG2
        Exit Select
      Case 192, 224, 256, 320
        'Allowed only in MPEG1
        If dwMpegVersion <> MPEG1 Then
          Throw New ArgumentOutOfRangeException("MpsBitRate", "Bit rate not compatible with input format")
        End If
        Exit Select
      Case 8, 16, 24, 144
        'Allowed only in MPEG2
        If dwMpegVersion <> MPEG2 Then
          Throw New ArgumentOutOfRangeException("MpsBitRate", "Bit rate not compatible with input format")
        End If
        Exit Select
      Case Else
        Throw New ArgumentOutOfRangeException("MpsBitRate", "Unsupported bit rate")
    End Select
    dwBitrate = MpeBitRate
    ' MINIMUM BIT RATE
    nPreset = LAME_QUALITY_PRESET.LQP_NORMAL_QUALITY
    ' QUALITY PRESET SETTING
    dwPsyModel = 0
    ' USE DEFAULT PSYCHOACOUSTIC MODEL 
    dwEmphasis = 0
    ' NO EMPHASIS TURNED ON
    bOriginal = 1
    ' SET ORIGINAL FLAG
    bWriteVBRHeader = 1
    bNoRes = 0
    ' No Bit resorvoir
    bCopyright = 0
    bCRC = 0
    bEnableVBR = 0
    bPrivate = 0
    bStrictIso = 0
    dwMaxBitrate = 0
    dwVbrAbr_bps = 0
    nQuality = 0
    nVbrMethod = VBRMETHOD.VBR_METHOD_NONE
    nVBRQuality = 0
  End Sub
End Structure
<StructLayout(LayoutKind.Sequential), Serializable()> _
Public Structure ACC
  Public dwSampleRate As UInteger
  Public byMode As Byte
  Public wBitrate As UShort
  Public byEncodingMethod As Byte
End Structure
<StructLayout(LayoutKind.Explicit), Serializable()> _
Public Class wFormat
  <FieldOffset(0)> _
  Public mp3 As MP3
  <FieldOffset(0)> _
  Public lhv1 As LHV1
  <FieldOffset(0)> _
  Public acc As ACC

  Public Sub New(format__1 As WaveFormat, MpeBitRate As UInteger)
    lhv1 = New LHV1(format__1, MpeBitRate)
  End Sub
End Class
<StructLayout(LayoutKind.Sequential), Serializable()> _
Public Class BE_CONFIG
  ' encoding formats
  Public Const BE_CONFIG_MP3 As UInteger = 0
  Public Const BE_CONFIG_LAME As UInteger = 256

  Public dwConfig As UInteger
  Public wformat As wFormat

  Public Sub New(wformat As WaveFormat, MpeBitRate As UInteger)
    Me.dwConfig = BE_CONFIG_LAME
    Me.wformat = New wFormat(wformat, MpeBitRate)
  End Sub
  Public Sub New(format As WaveFormat)
    Me.New(format, 128)
  End Sub
End Class
<StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)> _
Public Class BE_VERSION
  Public Const BE_MAX_HOMEPAGE As UInteger = 256
  Public byDLLMajorVersion As Byte
  Public byDLLMinorVersion As Byte
  Public byMajorVersion As Byte
  Public byMinorVersion As Byte
  ' DLL Release date
  Public byDay As Byte
  Public byMonth As Byte
  Public wYear As UShort
  'Homepage URL
  'BE_MAX_HOMEPAGE+1
  <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=257)> _
  Public zHomepage As String
  Public byAlphaLevel As Byte
  Public byBetaLevel As Byte
  Public byMMXEnabled As Byte
  <MarshalAs(UnmanagedType.ByValArray, SizeConst:=125)> _
  Public btReserved As Byte()
  Public Sub New()
    btReserved = New Byte(124) {}
  End Sub
End Class
''' <summary>
''' Lame_enc DLL functions
''' </summary>
Public Class Lame_encDll
  'Error codes
  Public Const BE_ERR_SUCCESSFUL As UInteger = 0
  Public Const BE_ERR_INVALID_FORMAT As UInteger = 1
  Public Const BE_ERR_INVALID_FORMAT_PARAMETERS As UInteger = 2
  Public Const BE_ERR_NO_MORE_HANDLES As UInteger = 3
  Public Const BE_ERR_INVALID_HANDLE As UInteger = 4

  ''' <summary>
  ''' This function is the first to call before starting an encoding stream.
  ''' </summary>
  ''' <param name="pbeConfig">Encoder settings</param>
  ''' <param name="dwSamples">Receives the number of samples (not bytes, each sample is a SHORT) to send to each beEncodeChunk() on return.</param>
  ''' <param name="dwBufferSize">Receives the minimun number of bytes that must have the output(result) buffer</param>
  ''' <param name="phbeStream">Receives the stream handle on return</param>
  ''' <returns>On success: BE_ERR_SUCCESSFUL</returns>
  <DllImport("lame_enc.dll")> _
  Public Shared Function beInitStream(pbeConfig As BE_CONFIG, ByRef dwSamples As UInteger, ByRef dwBufferSize As UInteger, ByRef phbeStream As UInteger) As UInteger
  End Function
  ''' <summary>
  ''' Encodes a chunk of samples. Please note that if you have set the output to 
  ''' generate mono MP3 files you must feed beEncodeChunk() with mono samples
  ''' </summary>
  ''' <param name="hbeStream">Handle of the stream.</param>
  ''' <param name="nSamples">Number of samples to be encoded for this call. 
  ''' This should be identical to what is returned by beInitStream(), 
  ''' unless you are encoding the last chunk, which might be smaller.</param>
  ''' <param name="pInSamples">Array of 16-bit signed samples to be encoded. 
  ''' These should be in stereo when encoding a stereo MP3 
  ''' and mono when encoding a mono MP3</param>
  ''' <param name="pOutput">Buffer where to write the encoded data. 
  ''' This buffer should be at least of the minimum size returned by beInitStream().</param>
  ''' <param name="pdwOutput">Returns the number of bytes of encoded data written. 
  ''' The amount of data written might vary from chunk to chunk</param>
  ''' <returns>On success: BE_ERR_SUCCESSFUL</returns>
  <DllImport("lame_enc.dll")> _
  Public Shared Function beEncodeChunk(hbeStream As UInteger, nSamples As UInteger, pInSamples As Short(), <[In](), Out()> pOutput As Byte(), ByRef pdwOutput As UInteger) As UInteger
  End Function
  ''' <summary>
  ''' Encodes a chunk of samples. Please note that if you have set the output to 
  ''' generate mono MP3 files you must feed beEncodeChunk() with mono samples
  ''' </summary>
  ''' <param name="hbeStream">Handle of the stream.</param>
  ''' <param name="nSamples">Number of samples to be encoded for this call. 
  ''' This should be identical to what is returned by beInitStream(), 
  ''' unless you are encoding the last chunk, which might be smaller.</param>
  ''' <param name="pSamples">Pointer at the 16-bit signed samples to be encoded. 
  ''' InPtr is used to pass any type of array without need of make memory copy, 
  ''' then gaining in performance. Note that nSamples is not the number of bytes,
  ''' but samples (is sample is a SHORT)</param>
  ''' <param name="pOutput">Buffer where to write the encoded data. 
  ''' This buffer should be at least of the minimum size returned by beInitStream().</param>
  ''' <param name="pdwOutput">Returns the number of bytes of encoded data written. 
  ''' The amount of data written might vary from chunk to chunk</param>
  ''' <returns>On success: BE_ERR_SUCCESSFUL</returns>
  <DllImport("lame_enc.dll")> _
  Protected Shared Function beEncodeChunk(hbeStream As UInteger, nSamples As UInteger, pSamples As IntPtr, <[In](), Out()> pOutput As Byte(), ByRef pdwOutput As UInteger) As UInteger
  End Function
  ''' <summary>
  ''' Encodes a chunk of samples. Samples are contained in a byte array
  ''' </summary>
  ''' <param name="hbeStream">Handle of the stream.</param>
  ''' <param name="buffer">Bytes to encode</param>
  ''' <param name="index">Position of the first byte to encode</param>
  ''' <param name="nBytes">Number of bytes to encode (not samples, samples are two byte lenght)</param>
  ''' <param name="pOutput">Buffer where to write the encoded data.
  ''' This buffer should be at least of the minimum size returned by beInitStream().</param>
  ''' <param name="pdwOutput">Returns the number of bytes of encoded data written. 
  ''' The amount of data written might vary from chunk to chunk</param>
  ''' <returns>On success: BE_ERR_SUCCESSFUL</returns>
  Public Shared Function EncodeChunk(hbeStream As UInteger, buffer As Byte(), index As Integer, nBytes As UInteger, pOutput As Byte(), ByRef pdwOutput As UInteger) As UInteger
    Dim res As UInteger
    Dim handle As GCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned)
    Try
      Dim ptr As IntPtr = CType(handle.AddrOfPinnedObject().ToInt32() + index, IntPtr)
      'Samples
      res = beEncodeChunk(hbeStream, nBytes \ 2, ptr, pOutput, pdwOutput)
    Finally
      handle.Free()
    End Try
    Return res
  End Function
  ''' <summary>
  ''' Encodes a chunk of samples. Samples are contained in a byte array
  ''' </summary>
  ''' <param name="hbeStream">Handle of the stream.</param>
  ''' <param name="buffer">Bytes to encode</param>
  ''' <param name="pOutput">Buffer where to write the encoded data.
  ''' This buffer should be at least of the minimum size returned by beInitStream().</param>
  ''' <param name="pdwOutput">Returns the number of bytes of encoded data written. 
  ''' The amount of data written might vary from chunk to chunk</param>
  ''' <returns>On success: BE_ERR_SUCCESSFUL</returns>
  Public Shared Function EncodeChunk(hbeStream As UInteger, buffer As Byte(), pOutput As Byte(), ByRef pdwOutput As UInteger) As UInteger
    Return EncodeChunk(hbeStream, buffer, 0, CUInt(buffer.Length), pOutput, pdwOutput)
  End Function
  ''' <summary>
  ''' This function should be called after encoding the last chunk in order to flush 
  ''' the encoder. It writes any encoded data that still might be left inside the 
  ''' encoder to the output buffer. This function should NOT be called unless 
  ''' you have encoded all of the chunks in your stream.
  ''' </summary>
  ''' <param name="hbeStream">Handle of the stream.</param>
  ''' <param name="pOutput">Where to write the encoded data. This buffer should be 
  ''' at least of the minimum size returned by beInitStream().</param>
  ''' <param name="pdwOutput">Returns number of bytes of encoded data written.</param>
  ''' <returns>On success: BE_ERR_SUCCESSFUL</returns>
  <DllImport("lame_enc.dll")> _
  Public Shared Function beDeinitStream(hbeStream As UInteger, <[In](), Out()> pOutput As Byte(), ByRef pdwOutput As UInteger) As UInteger
  End Function
  ''' <summary>
  ''' Last function to be called when finished encoding a stream. 
  ''' Should unlike beDeinitStream() also be called if the encoding is canceled.
  ''' </summary>
  ''' <param name="hbeStream">Handle of the stream.</param>
  ''' <returns>On success: BE_ERR_SUCCESSFUL</returns>
  <DllImport("lame_enc.dll")> _
  Public Shared Function beCloseStream(hbeStream As UInteger) As UInteger
  End Function
  ''' <summary>
  ''' Returns information like version numbers (both of the DLL and encoding engine), 
  ''' release date and URL for lame_enc's homepage. 
  ''' All this information should be made available to the user of your product 
  ''' through a dialog box or something similar.
  ''' </summary>
  ''' <param name="pbeVersion">Where version number, release date and URL for homepage 
  ''' is returned.</param>
  <DllImport("lame_enc.dll")> _
  Public Shared Sub beVersion(<Out()> pbeVersion As BE_VERSION)
  End Sub
  <DllImport("lame_enc.dll", CharSet:=CharSet.Ansi)> _
  Public Shared Sub beWriteVBRHeader(pszMP3FileName As String)
  End Sub
  <DllImport("lame_enc.dll")> _
  Public Shared Function beEncodeChunkFloatS16NI(hbeStream As UInteger, nSamples As UInteger, <[In]()> buffer_l As Single(), <[In]()> buffer_r As Single(), <[In](), Out()> pOutput As Byte(), ByRef pdwOutput As UInteger) As UInteger
  End Function
  <DllImport("lame_enc.dll")> _
  Public Shared Function beFlushNoGap(hbeStream As UInteger, <[In](), Out()> pOutput As Byte(), ByRef pdwOutput As UInteger) As UInteger
  End Function
  <DllImport("lame_enc.dll", CharSet:=CharSet.Ansi)> _
  Public Shared Function beWriteInfoTag(hbeStream As UInteger, lpszFileName As String) As UInteger
  End Function
End Class
<Serializable()> _
Public Class AudioWriterConfig
  Implements ISerializable
  Protected m_Format As WaveFormat

  ''' <summary>
  ''' A constructor with this signature must be implemented by descendants. 
  ''' <see cref="System.Runtime.Serialization.ISerializable"/> for more information
  ''' </summary>
  ''' <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> where is the serialized data.</param>
  ''' <param name="context">The source (see <see cref="System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
  Protected Sub New(info As SerializationInfo, context As StreamingContext)
    Dim rate As Integer = info.GetInt32("Format.Rate")
    Dim bits As Integer = info.GetInt32("Format.Bits")
    Dim channels As Integer = info.GetInt32("Format.Channels")
    m_Format = New WaveFormat(rate, bits, channels)
  End Sub

  Public Sub New(f As WaveFormat)
    m_Format = New WaveFormat(f.nSamplesPerSec, f.wBitsPerSample, f.nChannels)
  End Sub

  Public Sub New()
    Me.New(New WaveFormat(44100, 16, 2))
  End Sub

  Public Property Format() As WaveFormat
    Get
      Return m_Format
    End Get
    Set(value As WaveFormat)
      m_Format = value
    End Set
  End Property

#Region "ISerializable Members"

  Public Overridable Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
    info.AddValue("Format.Rate", m_Format.nSamplesPerSec)
    info.AddValue("Format.Bits", m_Format.wBitsPerSample)
    info.AddValue("Format.Channels", m_Format.nChannels)
  End Sub

#End Region
End Class
Public MustInherit Class AudioWriter
  Inherits BinaryWriter
  Protected m_InputDataFormat As WaveFormat

  Public Sub New(Output As Stream, InputDataFormat As WaveFormat)
    MyBase.New(Output, System.Text.Encoding.ASCII)
    m_InputDataFormat = InputDataFormat
  End Sub


  Public Sub New(Output As Stream, Config As AudioWriterConfig)
    Me.New(Output, Config.Format)
  End Sub


  Protected MustOverride Function GetOptimalBufferSize() As Integer

  Private Shared m_ConfigWidth As Integer = 368
  Private Shared m_ConfigHeight As Integer = 264

  Protected Overridable Function GetWriterConfig() As AudioWriterConfig
    Return New AudioWriterConfig(m_InputDataFormat)
  End Function

  Public ReadOnly Property WriterConfig() As AudioWriterConfig
    Get
      Return GetWriterConfig()
    End Get
  End Property

  ''' <summary>
  ''' Width of the config control
  ''' </summary>
  Public Shared Property ConfigWidth() As Integer
    Get
      Return m_ConfigWidth
    End Get
    Set(value As Integer)
      m_ConfigWidth = value
    End Set
  End Property

  ''' <summary>
  ''' Height of the config control
  ''' </summary>
  Public Shared Property ConfigHeight() As Integer
    Get
      Return m_ConfigHeight
    End Get
    Set(value As Integer)
      m_ConfigHeight = value
    End Set
  End Property

  ''' <summary>
  ''' Optimal size of the buffer used in each write operation to obtain best performance.
  ''' This value must be greater than 0 
  ''' </summary>
  Public ReadOnly Property OptimalBufferSize() As Integer
    Get
      Return GetOptimalBufferSize()
    End Get
  End Property

  Public Overrides Sub Write(value As String)
    Throw New NotSupportedException("Write(string value) is not supported")
  End Sub

  Public Overrides Sub Write(value As Single)
    Throw New NotSupportedException("Write(float value) is not supported")
  End Sub

  Public Overrides Sub Write(value As ULong)
    Throw New NotSupportedException("Write(ulong value) is not supported")
  End Sub

  Public Overrides Sub Write(value As Long)
    Throw New NotSupportedException("Write(long value) is not supported")
  End Sub

  Public Overrides Sub Write(value As UInteger)
    Throw New NotSupportedException("Write(uint value) is not supported")
  End Sub

  Public Overrides Sub Write(value As Integer)
    Throw New NotSupportedException("Write(int value) is not supported")
  End Sub

  Public Overrides Sub Write(value As UShort)
    Throw New NotSupportedException("Write(ushort value) is not supported")
  End Sub

  Public Overrides Sub Write(value As Short)
    Throw New NotSupportedException("Write(short value) is not supported")
  End Sub

  Public Overrides Sub Write(value As Decimal)
    Throw New NotSupportedException("Write(decimal value) is not supported")
  End Sub

  Public Overrides Sub Write(value As Double)
    Throw New NotSupportedException("Write(double value) is not supported")
  End Sub

  Public Overrides Sub Write(chars As Char(), index As Integer, count As Integer)
    Throw New NotSupportedException("Write(char[] chars, int index, int count) is not supported")
  End Sub

  Public Overrides Sub Write(chars As Char())
    Throw New NotSupportedException("Write(char[] chars) is not supported")
  End Sub

  Public Overrides Sub Write(ch As Char)
    Throw New NotSupportedException("Write(char ch) is not supported")
  End Sub

  Public Overrides Sub Write(value As SByte)
    Throw New NotSupportedException("Write(sbyte value) is not supported")
  End Sub

  Public Overrides Sub Write(value As Byte)
    Throw New NotSupportedException("Write(byte value) is not supported")
  End Sub

  Public Overrides Sub Write(value As Boolean)
    Throw New NotSupportedException("Write(bool value) is not supported")
  End Sub
End Class
<Serializable()> _
Public Class Mp3WriterConfig
  Inherits AudioWriterConfig
  Private m_BeConfig As BE_CONFIG

  Protected Sub New(info As SerializationInfo, context As StreamingContext)
    MyBase.New(info, context)
    m_BeConfig = DirectCast(info.GetValue("BE_CONFIG", GetType(BE_CONFIG)), BE_CONFIG)
  End Sub

  Public Sub New(InFormat As WaveFormat, beconfig As BE_CONFIG)
    MyBase.New(InFormat)
    m_BeConfig = beconfig
  End Sub

  Public Sub New(InFormat As WaveFormat)
    Me.New(InFormat, New BE_CONFIG(InFormat))
  End Sub

  Public Sub New()
    Me.New(New WaveFormat(44100, 16, 2))
  End Sub

  Public Overrides Sub GetObjectData(info As System.Runtime.Serialization.SerializationInfo, context As System.Runtime.Serialization.StreamingContext)
    MyBase.GetObjectData(info, context)
    info.AddValue("BE_CONFIG", m_BeConfig, m_BeConfig.[GetType]())
  End Sub

  Public Property Mp3Config() As BE_CONFIG
    Get
      Return m_BeConfig
    End Get
    Set(value As BE_CONFIG)
      m_BeConfig = value
    End Set
  End Property
End Class
''' <summary>
''' Convert PCM audio data to PCM format
''' The data received through the method write is assumed as PCM audio data. 
''' This data is converted to MP3 format and written to the result stream. 
''' </summary>
Public Class Mp3Writer
  Inherits AudioWriter
  Private closed As Boolean = False
  Private m_Mp3Config As BE_CONFIG = Nothing
  Private m_hLameStream As UInteger = 0
  Private m_InputSamples As UInteger = 0
  Private m_OutBufferSize As UInteger = 0
  Private m_InBuffer As Byte() = Nothing
  Private m_InBufferPos As Integer = 0
  Private m_OutBuffer As Byte() = Nothing
  Private m_Path As String

  ''' <summary>
  ''' Create a Mp3Writer with the default MP3 format
  ''' </summary>
  ''' <param name="Output">Stream that will hold the MP3 resulting data</param>
  ''' <param name="InputDataFormat">PCM format of input data</param>
  Public Sub New(Output As Stream, InputDataFormat As WaveFormat)
    Me.New(Output, InputDataFormat, New BE_CONFIG(InputDataFormat))
  End Sub

  ''' <summary>
  ''' Create a Mp3Writer with specific MP3 format
  ''' </summary>
  ''' <param name="Output">Stream that will hold the MP3 resulting data</param>
  ''' <param name="cfg">Writer Config</param>
  Public Sub New(Output As Stream, cfg As Mp3WriterConfig)
    Me.New(Output, cfg.Format, cfg.Mp3Config)
  End Sub

  ''' <summary>
  ''' Create a Mp3Writer with specific MP3 format
  ''' </summary>
  ''' <param name="Output">Stream that will hold the MP3 resulting data</param>
  ''' <param name="InputDataFormat">PCM format of input data</param>
  ''' <param name="Mp3Config">Desired MP3 config</param>
  Public Sub New(Output As Stream, InputDataFormat As WaveFormat, Mp3Config As BE_CONFIG)
    MyBase.New(Output, InputDataFormat)
    Try
      m_Mp3Config = Mp3Config
      Dim LameResult As UInteger = Lame_encDll.beInitStream(m_Mp3Config, m_InputSamples, m_OutBufferSize, m_hLameStream)
      If LameResult <> Lame_encDll.BE_ERR_SUCCESSFUL Then
        Throw New ApplicationException(String.Format("Lame_encDll.beInitStream failed with the error code {0}", LameResult))
      End If
      m_InBuffer = New Byte(m_InputSamples * 2 - 1) {}
      'Input buffer is expected as short[]
      m_OutBuffer = New Byte(m_OutBufferSize - 1) {}
    Catch
      MyBase.Close()
      Throw
    End Try
  End Sub

  Public Property Path As String
    Get
      Return m_Path
    End Get
    Set(value As String)
      m_Path = value
    End Set
  End Property

  ''' <summary>
  ''' MP3 Config of final data
  ''' </summary>
  Public ReadOnly Property Mp3Config() As BE_CONFIG
    Get
      Return m_Mp3Config
    End Get
  End Property

  Protected Overrides Function GetOptimalBufferSize() As Integer
    Return m_InBuffer.Length
  End Function

  Public Overrides Sub Close()
    If Not closed Then
      Try
        Dim EncodedSize As UInteger = 0
        If m_InBufferPos > 0 Then
          If Lame_encDll.EncodeChunk(m_hLameStream, m_InBuffer, 0, CUInt(m_InBufferPos), m_OutBuffer, EncodedSize) = Lame_encDll.BE_ERR_SUCCESSFUL Then
            If EncodedSize > 0 Then
              MyBase.Write(m_OutBuffer, 0, CInt(EncodedSize))
            End If
          End If
        End If
        EncodedSize = 0
        If Lame_encDll.beDeinitStream(m_hLameStream, m_OutBuffer, EncodedSize) = Lame_encDll.BE_ERR_SUCCESSFUL Then
          If EncodedSize > 0 Then
            MyBase.Write(m_OutBuffer, 0, CInt(EncodedSize))
          End If
        End If
      Finally
        Lame_encDll.beCloseStream(m_hLameStream)
      End Try
    End If
    closed = True
    MyBase.Close()
    MyBase.Dispose()
    Lame_encDll.beWriteInfoTag(m_hLameStream, m_Path)
    Lame_encDll.beCloseStream(m_hLameStream)
  End Sub


  ''' <summary>
  ''' Send to the compressor an array of bytes.
  ''' </summary>
  ''' <param name="buffer__1">Input buffer</param>
  ''' <param name="index">Start position</param>
  ''' <param name="count">Bytes to process. The optimal size, to avoid buffer copy, is a multiple of <see cref="AudioWriter.OptimalBufferSize"/></param>
  Public Overrides Sub Write(buffer__1 As Byte(), index As Integer, count As Integer)
    Dim ToCopy As Integer = 0
    Dim EncodedSize As UInteger = 0
    Dim LameResult As UInteger
    While count > 0
      If m_InBufferPos > 0 Then
        ToCopy = Math.Min(count, m_InBuffer.Length - m_InBufferPos)
        Buffer.BlockCopy(buffer__1, index, m_InBuffer, m_InBufferPos, ToCopy)
        m_InBufferPos += ToCopy
        index += ToCopy
        count -= ToCopy
        If m_InBufferPos >= m_InBuffer.Length Then
          m_InBufferPos = 0
          If (InlineAssignHelper(LameResult, Lame_encDll.EncodeChunk(m_hLameStream, m_InBuffer, m_OutBuffer, EncodedSize))) = Lame_encDll.BE_ERR_SUCCESSFUL Then
            If EncodedSize > 0 Then
              MyBase.Write(m_OutBuffer, 0, CInt(EncodedSize))
            End If
          Else
            Throw New ApplicationException(String.Format("Lame_encDll.EncodeChunk failed with the error code {0}", LameResult))
          End If
        End If
      Else
        If count >= m_InBuffer.Length Then
          If (InlineAssignHelper(LameResult, Lame_encDll.EncodeChunk(m_hLameStream, buffer__1, index, CUInt(m_InBuffer.Length), m_OutBuffer, EncodedSize))) = Lame_encDll.BE_ERR_SUCCESSFUL Then
            If EncodedSize > 0 Then
              MyBase.Write(m_OutBuffer, 0, CInt(EncodedSize))
            End If
          Else
            Throw New ApplicationException(String.Format("Lame_encDll.EncodeChunk failed with the error code {0}", LameResult))
          End If
          count -= m_InBuffer.Length
          index += m_InBuffer.Length
        Else
          Buffer.BlockCopy(buffer__1, index, m_InBuffer, 0, count)
          m_InBufferPos = count
          index += count
          count = 0
        End If
      End If
    End While
  End Sub

  ''' <summary>
  ''' Send to the compressor an array of bytes.
  ''' </summary>
  ''' <param name="buffer">The optimal size, to avoid buffer copy, is a multiple of <see cref="AudioWriter.OptimalBufferSize"/></param>
  Public Overrides Sub Write(buffer As Byte())
    Me.Write(buffer, 0, buffer.Length)
  End Sub

  Protected Overrides Function GetWriterConfig() As AudioWriterConfig
    Return New Mp3WriterConfig(m_InputDataFormat, Mp3Config)
  End Function
  Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
    target = value
    Return value
  End Function
End Class
