Imports DirectShowLib
Imports System.Runtime.InteropServices
Friend Class SeedFuncts
  <DllImport("oleaut32.dll")>
  Public Shared Function OleCreatePropertyFrame(hwndOwner As IntPtr, x As Integer, y As Integer, <MarshalAs(UnmanagedType.LPWStr)> lpszCaption As String, cObjects As Integer, <MarshalAs(UnmanagedType.[Interface], ArraySubType:=UnmanagedType.IUnknown)> ByRef ppUnk As Object, cPages As Integer, lpPageClsID As IntPtr, lcid As Integer, dwReserved As Integer, lpvReserved As IntPtr) As Integer
  End Function
  <DllImport("winmm.dll")> _
  Public Shared Function mciSendString(command As String, buffer As System.Text.StringBuilder, bufferSize As Integer, hwndCallback As IntPtr) As Integer
  End Function
  <DllImport("User32.dll", SetLastError:=True)> _
  Public Shared Function PrintWindow(hwnd As IntPtr, hDC As IntPtr, nFlags As UInteger) As Boolean
  End Function
  <DllImport("Gdi32.dll")> _
  Public Shared Function SelectObject(ByVal hdc As IntPtr, ByVal hObject As IntPtr) As IntPtr
  End Function
End Class

Public Class ctlSeed
  Public Enum MediaState
    mPlaying
    mPaused
    mStopped
    mClosed
    mOpen
  End Enum
  Private Const WS_VISIBLE As Long = &H10000000
  Private Const WM_GRAPHNOTIFY As Long = &HBC17& '48151
  Private Const WM_DVD_EVENT As Long = &H464
  Private Const WM_ERASEBKGND As Long = &H14
  Private Const EC_COMPLETE = &H1
  Private Const EC_USERABORT = &H2
  Private Const EC_ERRORABORT = &H3
  Private obj_mpAudio As IBasicAudio
  Private obj_mpVideo As IBasicVideo2
  Private obj_mpVidWindow As IVideoWindow
  Private obj_mpControl As IMediaControl
  Private obj_mpPosition As IMediaPosition
  Private obj_mpEvent As IMediaEventEx

  Private obj_dvdGraph As Dvd.IDvdGraphBuilder
  Private obj_dvdControl As Dvd.IDvdControl2
  Private obj_dvdInfo As Dvd.IDvdInfo2
  Private obj_dvdBuilder As IGraphBuilder
  Private dvdFPS As Double
  Private dvdLParam As IntPtr

  Private CFm_Filename As String
  Private TempFilename As String
  Private obj_queuedControl As IMediaControl
  Private CFm_queuedFilename As String
  Private CFm_queuedTempName As String
  Private CFm_queueTime As Double
  Private CFm_State As MediaState
  Private CFm_FullScreen As Boolean
  Private CFm_FSObj As Object
  Private CFm_Mute As Boolean
  Private CFm_HasVid As Boolean
  Private CFm_HasAud As Boolean
  Private CFm_Volume As Integer
  Private CFm_Balance As Integer
  Private CFm_Rate As Double
  Private CFm_VideoHeight As Long
  Private CFm_VideoWidth As Long
  Private CFm_StateFade As Boolean
  Private CFm_PadStart As Double
  Private CFm_PadEnd As Double
  Private CFm_AudioDevice As String
  Private CFm_Loop As Boolean
  Private bFading As Byte
  Private lFadePos As Long
  Private CleanupItems As New ArrayList
  Private WithEvents sckDownload As SocketWrapper
  Private WithEvents swfVideo As AxShockwaveFlashObjects.AxShockwaveFlash
  Private swfTemplate As AxShockwaveFlashObjects.AxShockwaveFlash
  Event VidDoubleClick(sender As Object, e As EventArgs)
  Event VidClick(sender As Object, e As MouseEventArgs)
  Event NetDownload(sender As Object, e As DownloadChangedEventArgs)
  Public Class MediaErrorEventArgs
    Inherits System.EventArgs
    Private mErr As Exception
    Private mFunct As String
    Public Sub New(e As Exception, Funct As String)
      mErr = e
      mFunct = Funct
    End Sub
    Public ReadOnly Property E As Exception
      Get
        Return mErr
      End Get
    End Property
    Public ReadOnly Property Funct As String
      Get
        Return mFunct
      End Get
    End Property
  End Class
  Public Class DownloadChangedEventArgs
    Inherits System.ComponentModel.ProgressChangedEventArgs
    Private m_Done As ULong
    Private m_Total As ULong
    Public ReadOnly Property BytesReceived As ULong
      Get
        Return m_Done
      End Get
    End Property
    Public ReadOnly Property TotalBytesToReceive As ULong
      Get
        Return m_Total
      End Get
    End Property
    Public ReadOnly Property BytesToReceive As ULong
      Get
        Return m_Total - m_Done
      End Get
    End Property
    Public Sub New()
      MyBase.New(0, Nothing)
    End Sub
    Public Sub New(done As Long, total As Long)
      MyBase.New(Math.Floor(done / total * 100), Nothing)
      m_Done = CULng(done)
      m_Total = CULng(total)
    End Sub
    Public Sub New(done As ULong, total As ULong)
      MyBase.New(Math.Floor(done / total * 100), Nothing)
      m_Done = done
      m_Total = total
    End Sub
    Public Sub New(done As Long, total As Long, UserState As Object)
      MyBase.New(Math.Floor(done / total * 100), UserState)
      m_Done = CULng(done)
      m_Total = CULng(total)
    End Sub
    Public Sub New(done As ULong, total As ULong, UserState As Object)
      MyBase.New(Math.Floor(done / total * 100), UserState)
      m_Done = done
      m_Total = total
    End Sub
  End Class

  Public Event EndOfFile(Status As Integer)
  Public Event CantOpen(Reason As String)
  Public Event MediaError(e As MediaErrorEventArgs)

  Public Class StreamInformation
    Private c_title As String
    Private c_artist As String
    Private c_album As String
    Public ReadOnly Property Title As String
      Get
        Return c_title
      End Get
    End Property
    Public ReadOnly Property Artist As String
      Get
        Return c_artist
      End Get
    End Property
    Public ReadOnly Property Album As String
      Get
        Return c_album
      End Get
    End Property
    Public Sub New(sTitle As String, sArtist As String, sAlbum As String)
      c_title = sTitle
      c_artist = sArtist
      c_album = sAlbum
    End Sub
  End Class
  Public StreamInfo As StreamInformation

  Public Property AudioDevice As String
    Get
      Return CFm_AudioDevice
    End Get
    Set(value As String)
      If value <> CFm_AudioDevice Then
        Dim bPlaying As Boolean = State = MediaState.mPlaying
        Dim pos As Double = Me.Position
        Dim sFile As String = FileName
        FileClose()
        CFm_AudioDevice = value
        If FileOpen(sFile) = 0 Then
          Me.Position = pos
          If bPlaying Then mpPlay()
          If Not String.IsNullOrEmpty(CFm_queuedFilename) Then
            sFile = CFm_queuedFilename
            SetNoQueue()
            FileQueue(CFm_queuedFilename)
          End If
        End If
      End If
    End Set
  End Property

  Public Property Balance() As Integer
    Get
      If IsSTA() AndAlso obj_mpAudio IsNot Nothing Then
        Dim iBal As Integer
        obj_mpAudio.get_Balance(iBal)
        Return iBal
      Else
        Return CFm_Balance
      End If
    End Get
    Set(value As Integer)
      If value > 10000 Then
        value = 10000
      ElseIf value < -10000 Then
        value = -10000
      End If
      If IsSTA() AndAlso obj_mpAudio IsNot Nothing Then
        obj_mpAudio.put_Balance(value)
      End If
      CFm_Balance = value
    End Set
  End Property

  Public ReadOnly Property Duration() As Double
    Get
      Static lastFile As String, lastDur As Double
      If Not String.IsNullOrEmpty(CFm_Filename) Then
        If CFm_Filename.EndsWith("VIDEO_TS", True, Nothing) Then
          Dim dvdTime As New Dvd.DvdHMSFTimeCode, dvdTimeFlags As New Dvd.DvdTimeCodeFlags
          obj_dvdInfo.GetTotalTitleTime(dvdTime, dvdTimeFlags)
          If dvdTime.bHours = 0 And dvdTime.bMinutes = 0 And dvdTime.bSeconds = 0 And dvdTime.bFrames = 0 Then Return 100
          If (dvdTimeFlags And Dvd.DvdTimeCodeFlags.FPS25) = Dvd.DvdTimeCodeFlags.FPS25 Then
            dvdFPS = 25
          ElseIf (dvdTimeFlags And Dvd.DvdTimeCodeFlags.FPS30) = Dvd.DvdTimeCodeFlags.FPS30 Then
            dvdFPS = 29.97
          Else
            dvdFPS = 23.976
          End If
          Return getDvdSeconds(dvdTime, dvdFPS)
        End If
        If lastFile = CFm_Filename Then Return lastDur
        Select Case IO.Path.GetExtension(CFm_Filename).Substring(1).ToLower
          Case "mp3" ', "mpe", "mpg", "mpeg", "m1v", "mp2", "m2v", "mp2v", "mpv2", "mpa", "aac", "m2ts", "m4a", "m4p", "m4v", "mp4"
            Using cHeader As New Seed.clsHeaderLoader(CFm_Filename)
              If cHeader.cMPEG IsNot Nothing AndAlso cHeader.cMPEG.CheckValidity Then
                lastFile = CFm_Filename
                lastDur = cHeader.Duration
                Return cHeader.Duration
              End If
            End Using
          Case "swf", "spl", "fla"
            If swfVideo Is Nothing Then Return -1
            Return swfVideo.TotalFrames
        End Select
      End If
      If IsSTA() AndAlso obj_mpPosition IsNot Nothing Then
        Dim lDur As Double
        obj_mpPosition.get_Duration(lDur)
        If lDur > 0 Then
          obj_mpPosition.get_StopTime(lDur)
          lastFile = CFm_Filename
          lastDur = lDur
          Return lDur
        Else
          lastFile = CFm_Filename
          lastDur = -1
          Return -1
        End If
      Else
        lastFile = CFm_Filename
        lastDur = -1
        Return -1
      End If
    End Get
  End Property

  Public ReadOnly Property IsFlash As Boolean
    Get
      If swfVideo Is Nothing Then Return False
      Return True
    End Get
  End Property

  Public ReadOnly Property IsStreaming As Boolean
    Get
      If String.IsNullOrEmpty(CFm_Filename) Then Return False
      Return CFm_Filename.Contains(IO.Path.Combine(IO.Path.GetTempPath, "seedTemp"))
    End Get
  End Property

  Public Property FileName As String
    Get
      Return CFm_Filename
    End Get
    Set(value As String)
      If String.Compare(CFm_Filename, value) <> 0 Then
        Dim ret As Integer = FileOpen(value)
        If ret < 0 Then
          Select Case ret
            Case -1 : CFm_Filename = "Unable to create graph manager."
            Case -2
              If Not String.IsNullOrEmpty(value) Then CFm_Filename = "File not found."
              If sckDownload IsNot Nothing Then
                sckDownload.Disconnect()
                sckDownload.Dispose()
                sckDownload = Nothing
              End If
              If StreamInfo IsNot Nothing Then StreamInfo = Nothing
            Case -3 : CFm_Filename = "Failed to download file."
            Case -4 : CFm_Filename = CFm_Filename
            Case -5 : CFm_Filename = Nothing
            Case Else : CFm_Filename = "Unable to create graph manager: " & DShowError(ret)
          End Select
        Else
          If Duration <= 0 Then CFm_Filename = "Duration is null."
        End If
      End If
    End Set
  End Property

  Public Property FullScreen() As Boolean
    Get
      FullScreen = CFm_FullScreen
    End Get
    Set(value As Boolean)
      CFm_FullScreen = value
      SetFullScreen()
    End Set
  End Property

  Public Property FullScreenObj() As Object
    Get
      FullScreenObj = CFm_FSObj
    End Get
    Set(value As Object)
      CFm_FSObj = value
      SetFullScreen()
    End Set
  End Property

  Public Property QueueTime() As Double
    Get
      Return CFm_queueTime
    End Get
    Set(value As Double)
      CFm_queueTime = value
    End Set
  End Property

  Private Function TrimFile(Path As String) As String
    If String.IsNullOrEmpty(Path) Then Return Nothing
    If Not My.Computer.FileSystem.FileExists(Path) Then Return Nothing
    If IO.Path.GetExtension(Path).ToLower = ".mp3" Then
      If My.Computer.FileSystem.GetFileInfo(Path).Length >= 1024L * 1024L * 1024L * 4L Then Return Nothing
      Dim bFile As Byte() = My.Computer.FileSystem.ReadAllBytes(Path)
      Dim lFrameStart As Integer = 0
      Do
        lFrameStart = GetBytePos(bFile, &HFF, lFrameStart)
        If lFrameStart < 1 Then Return Nothing
        If lFrameStart >= 0 AndAlso CheckMPEG(bFile, lFrameStart) Then Exit Do
        lFrameStart += 1
      Loop
      If lFrameStart < 1 Then Return Nothing
      Dim lFrameLen As Integer = bFile.Length - lFrameStart - IIf((GetString(bFile, bFile.Length - &H80, 3) = "TAG"), &H80, 0)
      Dim bReturn(lFrameLen - 1) As Byte
      Array.ConstrainedCopy(bFile, lFrameStart, bReturn, 0, lFrameLen)
      Dim sTmp As String = GenNextRndFile()
      My.Computer.FileSystem.WriteAllBytes(sTmp, bReturn, False)
      Return sTmp
    Else
      Return Nothing
    End If
  End Function

  Private Function RandBetween(min As Double, max As Double, decimals As Integer) As Double
    Return Math.Round((Rnd() * (max - min)) + min, decimals)
  End Function

  Private Function GetThumbnailFromControl(ByRef MPCtl As IMediaControl, free As Boolean, Optional TimeTry As Double = -1, Optional failMessage As Boolean = False) As Drawing.Bitmap
    Dim BasicVid As IBasicVideo = Nothing
    Dim PosCtl As IMediaPosition = Nothing
    'Dim ActCtl As IMediaControl = Nothing
    Dim dibImage As IntPtr = IntPtr.Zero
    Dim wasPos As Double
    Try
      BasicVid = MPCtl
      PosCtl = MPCtl
      'ActCtl = MPCtl
      PosCtl.get_CurrentPosition(wasPos)
      Dim dur As Double
      PosCtl.get_Duration(dur)
      If TimeTry = -1 Then
        TimeTry = dur * RandBetween(0.1, 0.2, 3) ' 0.15
        If TimeTry > 60 * 10.0 Then TimeTry = 60 * RandBetween(8.5, 10.0, 3) '10.0
      End If
      If TimeTry > dur Then TimeTry = 0.0
      If TimeTry < 0 Then TimeTry = 0.0
      Do
        PosCtl.put_CurrentPosition(TimeTry)
        Dim bufferSize As Integer
        'ActCtl.Pause()
        Dim shr As Integer = BasicVid.GetCurrentImage(bufferSize, IntPtr.Zero)
        If Not shr = 0 Or bufferSize < 1 Then
          Dim sFail As String = HRMessage(shr)
          Dim exr = Marshal.GetExceptionForHR(shr)
          If sFail = "Specified cast is not valid." Then Return Nothing
          Debug.Print(exr.Message)
          Debug.Print("Capture Thumbnail Buffer Failed: " & sFail)
          Dim errBmp As New Bitmap(256, 192)
          Using g As Graphics = Graphics.FromImage(errBmp)
            g.Clear(Color.Black)
            g.DrawString("Buffer Error:" & vbNewLine & sFail, New Font(FontFamily.GenericSansSerif, 20), Brushes.White, New RectangleF(16, 16, 224, 160))
          End Using
          PosCtl.put_CurrentPosition(wasPos)
          Return errBmp
        End If
        dibImage = Marshal.AllocHGlobal(bufferSize)
        Dim hr As Integer = BasicVid.GetCurrentImage(bufferSize, dibImage)
        If hr = 0 Then
          Dim struct As New BitmapInfoHeader
          Marshal.PtrToStructure(dibImage, struct)
          Dim imgSize As Integer = struct.ImageSize
          If imgSize > bufferSize - 40 Then
            bufferSize = imgSize + 40
            dibImage = Marshal.AllocHGlobal(bufferSize)
            hr = BasicVid.GetCurrentImage(bufferSize, dibImage)
            If hr = 0 Then
              Marshal.PtrToStructure(dibImage, struct)
              imgSize = struct.ImageSize
              If imgSize > bufferSize - 40 Then
                imgSize = bufferSize - 40
              End If
            Else
              Marshal.FreeHGlobal(dibImage)
              dibImage = IntPtr.Zero
              Dim sFail As String = HRMessage(shr)
              Dim exr = Marshal.GetExceptionForHR(shr)
              Debug.Print(exr.Message)
              Debug.Print("Capture Thumbnail Buffer Failed: " & sFail)
              Dim errBmp As New Bitmap(256, 192)
              Using g As Graphics = Graphics.FromImage(errBmp)
                g.Clear(Color.Black)
                g.DrawString("Thumb XBuf Error:" & vbNewLine & sFail, New Font(FontFamily.GenericSansSerif, 20), Brushes.White, New RectangleF(16, 16, 224, 160))
              End Using
              PosCtl.put_CurrentPosition(wasPos)
              Return errBmp
            End If
          End If
          Dim bpp As Drawing.Imaging.PixelFormat = Imaging.PixelFormat.Format32bppRgb
          Select Case struct.BitCount
            Case 4 : bpp = Imaging.PixelFormat.Format4bppIndexed
            Case 8 : bpp = Imaging.PixelFormat.Format8bppIndexed
            Case 16 : bpp = Imaging.PixelFormat.Format16bppRgb555
            Case 24 : bpp = Imaging.PixelFormat.Format24bppRgb
            Case 32 : bpp = Imaging.PixelFormat.Format32bppRgb
            Case 48 : bpp = Imaging.PixelFormat.Format48bppRgb
            Case 64 : bpp = Imaging.PixelFormat.Format64bppArgb
            Case Else : Debug.Print("Bit Count: " & struct.BitCount)
          End Select
          Dim dataStart As Long = dibImage.ToInt64 + struct.Size
          Dim bData(imgSize - 1) As Byte
          For I As Integer = 0 To imgSize - 1
            Try
              bData(I) = Marshal.ReadByte(New IntPtr(dataStart + I))
            Catch ex As Exception
              Debug.Print("FAILED TO READ BYTE " & I)
              bData(I) = 0
            End Try
          Next
          Dim BMP As New Bitmap(struct.Width, struct.Height, bpp)
          Select Case struct.BitCount
            Case 4, 8, 16, 48, 64
              Debug.Print("Haven't handled this BPP value: " & struct.BitCount)
            Case 24
              Dim x, y As Integer
              For I As Integer = 0 To bData.Length - 1 Step 3
                If x >= struct.Width Then
                  y += 1
                  x = 0
                End If
                If y >= struct.Height Then
                  Debug.Print("OUT OF RANGE")
                  Exit For
                End If
                Dim pxBlue As Byte = bData(I)
                Dim pxGreen As Byte = bData(I + 1)
                Dim pxRed As Byte = bData(I + 2)
                Dim cVal As Color = Color.FromArgb(pxRed, pxGreen, pxBlue)
                BMP.SetPixel(x, y, cVal)
                x += 1
              Next
            Case 32
              Dim x, y As Integer
              For I As Integer = 0 To bData.Length - 1 Step 4
                If x >= struct.Width Then
                  y += 1
                  x = 0
                End If
                If y >= struct.Height Then
                  Debug.Print("OUT OF RANGE")
                  Exit For
                End If
                Dim pxBlue As Byte = bData(I)
                Dim pxGreen As Byte = bData(I + 1)
                Dim pxRed As Byte = bData(I + 2)
                Dim pxAlpha As Byte = bData(I + 3)
                Dim cVal As Color = Color.FromArgb(pxAlpha, pxRed, pxGreen, pxBlue)
                BMP.SetPixel(x, y, cVal)
                x += 1
              Next
          End Select
          Dim newBmp As Drawing.Bitmap = BMP.Clone
          BMP.Dispose()
          Marshal.FreeHGlobal(dibImage)
          dibImage = IntPtr.Zero
          newBmp.RotateFlip(Drawing.RotateFlipType.RotateNoneFlipY)
          Dim bBmp As Byte() = BtBA(newBmp)
          Dim lastA, lastR, lastG, lastB As Int16
          lastB = bBmp(&H36)
          lastG = bBmp(&H37)
          lastR = bBmp(&H38)
          lastA = bBmp(&H39)
          For I As Integer = &H3A To bBmp.Length - 1 Step 4 * 50
            Dim b As Int16 = bBmp(I)
            Dim g As Int16 = bBmp(I + 1)
            Dim r As Int16 = bBmp(I + 2)
            Dim a As Int16 = bBmp(I + 3)
            If Math.Abs(b - lastB) > 8 Or Math.Abs(g - lastG) > 8 Or Math.Abs(r - lastR) > 8 Or Math.Abs(a - lastA) > 8 Then
              PosCtl.put_CurrentPosition(wasPos)
              Return newBmp
              Exit For
            Else
              lastB = b
              lastG = g
              lastR = r
              lastA = a
            End If
          Next
          TimeTry += dur * RandBetween(0.4, 0.6, 3) ' 0.05
          If TimeTry > dur / 2 Then
            PosCtl.put_CurrentPosition(wasPos)
            Return newBmp
          End If
        Else
          Marshal.FreeHGlobal(dibImage)
          dibImage = IntPtr.Zero
          If failMessage Then
            Dim sFail As String = HRMessage(hr)
            Dim exr = Marshal.GetExceptionForHR(hr)
            Debug.Print(exr.Message)
            Debug.Print("Capture Thumbnail Failed: " & sFail)
            Dim newBMP As New Bitmap(256, 192)
            Using g As Graphics = Graphics.FromImage(newBMP)
              g.Clear(Color.Black)
              g.DrawString("Thumb Error:" & vbNewLine & sFail, New Font(FontFamily.GenericSansSerif, 20), Brushes.White, New RectangleF(16, 16, 224, 160))
            End Using
            PosCtl.put_CurrentPosition(wasPos)
            Return newBMP
          Else
            PosCtl.put_CurrentPosition(wasPos)
            Return Nothing
          End If
        End If
        'ActCtl.Run()
      Loop
    Catch
      PosCtl.put_CurrentPosition(wasPos)
      Return Nothing
    Finally
      If dibImage <> IntPtr.Zero Then Marshal.FreeHGlobal(dibImage)
      If free Then
        If BasicVid IsNot Nothing Then
          Marshal.FinalReleaseComObject(BasicVid)
          BasicVid = Nothing
        End If
        If PosCtl IsNot Nothing Then
          Marshal.FinalReleaseComObject(PosCtl)
          PosCtl = Nothing
        End If
      End If
    End Try
  End Function

  Private Function BtBA(img As Bitmap) As Byte()
    Dim bData As Byte()
    Using ms As New IO.MemoryStream
      img.Save(ms, Imaging.ImageFormat.Bmp)
      bData = ms.ToArray
    End Using
    Return bData
  End Function

  ''' <summary>
  ''' Generate a thumbnail of a Video file.
  ''' </summary>
  ''' <param name="FilePath">If entered, the path to the file to be rendered into a Bitmap. If empty, the currently loaded file in this control will be used.</param>
  ''' <param name="failMessage">Toggles fail messages displayed as a bitmap in place of a blank response.</param>
  ''' <param name="changePosition">Toggles setting the position to a specific location in the file.</param>
  ''' <returns>Thumbnail as Bitmap.</returns>
  Public Function GetFileThumbnail(Optional FilePath As String = Nothing, Optional failMessage As Boolean = False, Optional changePosition As Boolean = True) As Drawing.Bitmap
    If Not String.IsNullOrEmpty(FilePath) Then ResetFFResize()
    Dim posTry As Double = -1
    If changePosition = False Then posTry = Position
    Dim MediaCtl As IMediaControl = Nothing
    Dim sTmp As String = Nothing
    Dim Thumb As Bitmap = Nothing
    Try
      If IsSTA() AndAlso String.IsNullOrEmpty(FilePath) Then
        If obj_mpControl IsNot Nothing Then
          Dim iFilters As IBaseFilter() = GetFilterList(obj_mpControl)
          If iFilters IsNot Nothing Then
            Dim fList() As FilterInfo = Nothing
            Dim fVal As Integer = 0
            For Each objFilter As IBaseFilter In iFilters
              ReDim Preserve fList(fVal)
              objFilter.QueryFilterInfo(fList(fVal))
              fVal += 1
            Next
            If (From FilterItem As FilterInfo In fList Where FilterItem.achName = "Video Renderer").Count > 0 Then
              Thumb = GetThumbnailFromControl(obj_mpControl, False, posTry, failMessage)
            Else
              If failMessage Then
                Dim sFail As String = "No Video in Filter List"
                Dim newBMP As New Bitmap(256, 192)
                Using g As Graphics = Graphics.FromImage(newBMP)
                  g.Clear(Color.Black)
                  g.DrawString(sFail, New Font(FontFamily.GenericSansSerif, 20), Brushes.White, New RectangleF(16, 16, 224, 160))
                End Using
                Return newBMP
              Else
                Return Nothing
              End If
            End If
          Else
            Thumb = GetThumbnailFromControl(obj_mpControl, False, posTry, failMessage)
          End If
        ElseIf swfVideo IsNot Nothing Then
          Using g As Graphics = swfVideo.CreateGraphics
            Using bmp As New Bitmap(swfVideo.Size.Width, swfVideo.Size.Height)
              Using memGraphics As Graphics = Graphics.FromImage(bmp)
                Dim dc As IntPtr = memGraphics.GetHdc
                SeedFuncts.SelectObject(dc, bmp.GetHbitmap)
                Dim ret As Boolean = SeedFuncts.PrintWindow(swfVideo.Handle, dc, 0)
                memGraphics.ReleaseHdc(dc)
              End Using
              Thumb = bmp.Clone
            End Using
          End Using
          'Thumb = Nothing
        Else
          Thumb = Nothing
        End If
      ElseIf My.Computer.FileSystem.FileExists(FilePath) And My.Computer.FileSystem.GetFileInfo(FilePath).Length <> 0 Then
        sTmp = TrimFile(FilePath)
        If String.IsNullOrEmpty(sTmp) Then
          If IO.Path.GetExtension(FilePath).ToLower = ".swf" Then
            Using swfTemp As New AxShockwaveFlashObjects.AxShockwaveFlash
              swfTemp.LoadMovie(0, FilePath)
              Using g As Graphics = swfTemp.CreateGraphics
                Using bmp As New Bitmap(swfTemp.Size.Width, swfTemp.Size.Height)
                  Using memGraphics As Graphics = Graphics.FromImage(bmp)
                    Dim dc As IntPtr = memGraphics.GetHdc
                    Dim ret As Boolean = SeedFuncts.PrintWindow(swfTemp.Handle, dc, 0)
                    memGraphics.ReleaseHdc(dc)
                  End Using
                  Return bmp.Clone
                End Using
              End Using
            End Using
          Else
            Dim mkvSize As Drawing.Size, mkvCrop As Drawing.Rectangle
            Try
              Dim mkvHeader As New clsMKVHeaders(FilePath)
              If mkvHeader IsNot Nothing AndAlso mkvHeader.EBMLHead.DocType = "matroska" Then
                GetMKVDisplaySize(mkvHeader, mkvSize, mkvCrop)
              Else
                mkvSize = Drawing.Size.Empty
                mkvCrop = Drawing.Rectangle.Empty
              End If
            Catch ex As Exception
              Err.Clear()
              mkvSize = Drawing.Size.Empty
              mkvCrop = Drawing.Rectangle.Empty
            End Try
            MediaCtl = New FilterGraph
            MediaCtl.RenderFile(FilePath)
            Dim iFilters As IBaseFilter() = GetFilterList(MediaCtl)
            If iFilters IsNot Nothing Then
              Dim fList() As FilterInfo = Nothing
              Dim fVal As Integer = 0
              For Each objFilter As IBaseFilter In iFilters
                ReDim Preserve fList(fVal)
                objFilter.QueryFilterInfo(fList(fVal))
                fVal += 1
              Next
              If (From FilterItem As FilterInfo In fList Where FilterItem.achName = "Video Renderer").Count < 1 Then Return Nothing
            End If
            If mkvSize = Drawing.Size.Empty And mkvCrop = Drawing.Rectangle.Empty Then
              Thumb = GetThumbnailFromControl(MediaCtl, True, posTry, failMessage)
            Else
              If mkvCrop = Drawing.Rectangle.Empty Then
                'just size
                Dim UnsizedThumb As Bitmap = GetThumbnailFromControl(MediaCtl, True, posTry, failMessage)
                If UnsizedThumb Is Nothing Then Return Nothing
                Thumb = New Bitmap(mkvSize.Width, mkvSize.Height)
                Using g As Graphics = Graphics.FromImage(Thumb)
                  g.DrawImage(UnsizedThumb, 0, 0, mkvSize.Width, mkvSize.Height)
                End Using
              ElseIf mkvSize = Drawing.Size.Empty Then
                'just crop
                Dim UncroppedThumb As Bitmap = GetThumbnailFromControl(MediaCtl, True, posTry, failMessage)
                If UncroppedThumb Is Nothing Then Return Nothing
                Dim CroppedRes As Rectangle = New Rectangle(mkvCrop.Left, mkvCrop.Top, UncroppedThumb.Width - (mkvCrop.Left + mkvCrop.Width), UncroppedThumb.Height - (mkvCrop.Top + mkvCrop.Height))
                If CroppedRes.Width < 1 Or CroppedRes.Height < 1 Then Return Nothing
                Thumb = New Bitmap(CroppedRes.Width, CroppedRes.Height)
                Using g As Graphics = Graphics.FromImage(Thumb)
                  g.DrawImageUnscaledAndClipped(UncroppedThumb, CroppedRes)
                End Using
              Else
                'both
                Dim UncroppedThumb As Bitmap = GetThumbnailFromControl(MediaCtl, True, posTry, failMessage)
                If UncroppedThumb Is Nothing Then
                  Return Nothing
                End If
                If failMessage And UncroppedThumb.Width = 256 And UncroppedThumb.Height = 192 Then
                  Return UncroppedThumb
                End If
                Dim SizedThumb As New Bitmap(mkvSize.Width, mkvSize.Height)
                Using g As Graphics = Graphics.FromImage(SizedThumb)
                  g.DrawImage(UncroppedThumb, 0, 0, mkvSize.Width, mkvSize.Height)
                End Using

                Dim CroppedRes As Rectangle = New Rectangle(mkvCrop.Left, mkvCrop.Top, UncroppedThumb.Width - (mkvCrop.Left + mkvCrop.Width), UncroppedThumb.Height - (mkvCrop.Top + mkvCrop.Height))
                If CroppedRes.Width < 1 Or CroppedRes.Height < 1 Then Return Nothing
                Thumb = New Bitmap(CroppedRes.Width, CroppedRes.Height)
                Using g As Graphics = Graphics.FromImage(Thumb)
                  g.DrawImage(SizedThumb, New Rectangle(Point.Empty, CroppedRes.Size), CroppedRes, GraphicsUnit.Pixel)
                  'g.DrawImageUnscaledAndClipped(SizedThumb, CroppedRes)
                End Using


              End If
            End If
          End If
        Else
          MediaCtl = New FilterGraph
          MediaCtl.RenderFile(sTmp)
          Dim iFilters As IBaseFilter() = GetFilterList(MediaCtl)
          If iFilters IsNot Nothing Then
            Dim fList() As FilterInfo = Nothing
            Dim fVal As Integer = 0
            For Each objFilter As IBaseFilter In iFilters
              ReDim Preserve fList(fVal)
              objFilter.QueryFilterInfo(fList(fVal))
              fVal += 1
            Next
            If (From FilterItem As FilterInfo In fList Where FilterItem.achName = "Video Renderer").Count > 0 Then
              Thumb = GetThumbnailFromControl(MediaCtl, True, posTry, failMessage)
            Else
              Return Nothing
            End If
          Else
            Thumb = GetThumbnailFromControl(MediaCtl, True, posTry, failMessage)
          End If
        End If
      Else
        Return Nothing
      End If
      Return Thumb
    Catch ex As Exception
      Debug.Print("Thumbnail error: " & ex.Message)
      Return Nothing
    Finally
      If MediaCtl IsNot Nothing Then
        Marshal.FinalReleaseComObject(MediaCtl)
        MediaCtl = Nothing
      End If
      If Not String.IsNullOrEmpty(sTmp) AndAlso My.Computer.FileSystem.FileExists(sTmp) Then
        Do
          Try
            My.Computer.FileSystem.DeleteFile(sTmp)
          Catch ex As Exception
            Err.Clear()
            Continue Do
          End Try
        Loop While My.Computer.FileSystem.FileExists(sTmp)
      End If
    End Try
  End Function

  'Public Function GetStreamThumbnail(FileStream As ComTypes.IStream) As Drawing.Bitmap
  '  Dim MediaCtl As IGraphBuilder = Nothing


  '  Dim Thumb As Bitmap = Nothing
  '  Try

  '    MediaCtl = New FilterGraph

  '    Dim pX As IPin = Nothing

  '    pX()

  '    MediaCtl.Render(pX)
  '    'MediaCtl.RenderFile(FilePath)
  '    Dim iFilters As IBaseFilter() = GetFilterList(MediaCtl)
  '    If iFilters IsNot Nothing Then
  '      Dim fList() As FilterInfo = Nothing
  '      Dim fVal As Integer = 0
  '      For Each objFilter As IBaseFilter In iFilters
  '        ReDim Preserve fList(fVal)
  '        objFilter.QueryFilterInfo(fList(fVal))
  '        fVal += 1
  '      Next
  '      If (From FilterItem As FilterInfo In fList Where FilterItem.achName = "Video Renderer").Count > 0 Then
  '        Thumb = GetThumbnailFromControl(MediaCtl, True)
  '      Else
  '        Return Nothing
  '      End If
  '    Else
  '      Thumb = GetThumbnailFromControl(MediaCtl, True)
  '    End If
  '    Return Thumb
  '  Catch ex As Exception
  '    Debug.Print("Thumbnail error: " & ex.Message)
  '    Return Nothing
  '  Finally
  '    If MediaCtl IsNot Nothing Then
  '      Marshal.FinalReleaseComObject(MediaCtl)
  '      MediaCtl = Nothing
  '    End If
  '  End Try
  'End Function

  Public Function GetFileDuration(FilePath As String) As Double
    If Not My.Computer.FileSystem.FileExists(FilePath) Then Return 0
    Select Case IO.Path.GetExtension(FilePath).Substring(1).ToLower
      Case "mp3" ', "mpe", "mpg", "mpeg", "m1v", "mp2", "m2v", "mp2v", "mpv2", "mpa", "aac", "m2ts", "m4a", "m4p", "m4v", "mp4"
        Using cHeader As New Seed.clsHeaderLoader(FilePath)
          If cHeader.cMPEG IsNot Nothing AndAlso cHeader.cMPEG.CheckValidity Then Return cHeader.Duration
        End Using
      Case "swf", "spl" ', "fla"
        Dim iRet As Integer = 0
        Dim swR As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSeed))
        Using swTime As New AxShockwaveFlashObjects.AxShockwaveFlash
          swTime.BeginInit()
          Me.SuspendLayout()
          swTime.Location = New Point(0, 0)
          swTime.Name = "swTime"
          swTime.OcxState = swR.GetObject("swfTemplate.OcxState")
          swTime.Size = New Size(1, 1)
          swTime.TabIndex = 0
          Me.Controls.Add(swTime)
          swTime.EndInit()
          Me.ResumeLayout(False)
          swTime.LoadMovie(0, FilePath)
          iRet = swTime.TotalFrames
          Me.Controls.Remove(swTime)
        End Using
        Return iRet
    End Select
    Dim MediaCtl As IMediaControl = Nothing
    Dim PosCtl As IMediaPosition = Nothing
    Dim sTmp As String = Nothing
    Try
      If My.Computer.FileSystem.FileExists(FilePath) And My.Computer.FileSystem.GetFileInfo(FilePath).Length > 0 Then
        sTmp = TrimFile(FilePath)
        If String.IsNullOrEmpty(sTmp) Then
          MediaCtl = New FilterGraph
          MediaCtl.RenderFile(FilePath)
          PosCtl = MediaCtl
          Dim dDur As Double
          PosCtl.get_Duration(dDur)
          Return dDur
        Else
          MediaCtl = New FilterGraph
          MediaCtl.RenderFile(sTmp)
          PosCtl = MediaCtl
          Dim dDur As Double
          PosCtl.get_Duration(dDur)
          Return dDur
        End If
      Else
        Return 0
      End If
    Catch ex As Exception
      Debug.Print("Duration error: " & ex.Message)
      Return 0
    Finally
      If PosCtl IsNot Nothing Then
        Marshal.FinalReleaseComObject(PosCtl)
        PosCtl = Nothing
      End If
      If MediaCtl IsNot Nothing Then
        Marshal.FinalReleaseComObject(MediaCtl)
        MediaCtl = Nothing
      End If
      If Not String.IsNullOrEmpty(sTmp) Then My.Computer.FileSystem.DeleteFile(sTmp)
    End Try
  End Function

  Public Sub GetFilters(ByRef Filters() As String)
    Dim FilterList() As String = Nothing
    Dim I As Integer
    I = 0
    If IsSTA() AndAlso obj_mpControl IsNot Nothing Then
      Dim iFilters As IBaseFilter() = GetFilterList(obj_mpControl)
      If iFilters Is Nothing Then
        Filters = Nothing
      Else
        For Each objFilter As IBaseFilter In iFilters
          Dim fInfo As FilterInfo = Nothing
          objFilter.QueryFilterInfo(fInfo)
          If Not My.Computer.FileSystem.FileExists(fInfo.achName) Then
            ReDim Preserve FilterList(I)
            FilterList(I) = fInfo.achName
            I = I + 1
          End If
        Next
        Filters = FilterList
      End If
    End If
  End Sub

  Public ReadOnly Property HasAud() As Boolean
    Get
      Return CFm_HasAud
    End Get
  End Property

  Public ReadOnly Property HasVid() As Boolean
    Get
      Return CFm_HasVid
    End Get
  End Property

  Public Sub mpPause()
    Try
      If Me.State = MediaState.mPaused Then Return
      If IsSTA() AndAlso obj_mpControl IsNot Nothing Then
        If CFm_HasVid Or (Not CFm_StateFade) Then
          obj_mpControl.Pause()
        Else
          CFm_State = MediaState.mPaused
          lFadePos = Me.Position
          bFading = 2
          Dim lStartVol As Long = Me.LogVolume
          Dim lTic As Long = Environment.TickCount + (lStartVol * 0.0025)
          Do Until Me.LogVolume <= 1
            If Not bFading = 2 Then Return
            Dim lTmpVol As Long = (lTic - Environment.TickCount) / 0.0025
            If lTmpVol < 0 Or lTmpVol > lStartVol Then
              Me.LogVolume = 0
            Else
              Me.LogVolume = lTmpVol
            End If
          Loop
          obj_mpControl.Pause()
          Me.Position = lFadePos
          Me.LogVolume = lStartVol
          bFading = 0
        End If
      ElseIf swfVideo IsNot Nothing Then
        swfVideo.Playing = False
      End If
    Catch ex As Exception
      Debug.Print("Failed to pause: " & ex.Message)
    End Try
  End Sub

  Public Sub mpPlay()
    Try
      If Me.State = MediaState.mPlaying Then Return
      If IsSTA() AndAlso obj_mpControl IsNot Nothing Then
        Dim lCurPos As Double = Me.Position
        If Not CFm_StateFade OrElse CFm_HasVid OrElse lCurPos <= CFm_PadStart Then
          obj_mpControl.Run()
        Else
          CFm_State = MediaState.mPlaying
          lFadePos = Me.Position
          bFading = 1
          Dim lStartVol As Long = Me.LogVolume
          Me.LogVolume = 0
          obj_mpControl.Run()
          Dim lTic As Long = Environment.TickCount + (lStartVol * 0.0025)
          Do Until Me.LogVolume >= lStartVol
            If Not bFading = 1 Then Return
            Dim lTmpVol As Long = (((Environment.TickCount - lTic) + (lStartVol * 0.0025)) / 0.0025)
            If lTmpVol > lStartVol Or lTmpVol < 0 Then
              Me.LogVolume = lStartVol
            Else
              Me.LogVolume = lTmpVol
            End If
          Loop
          Me.LogVolume = lStartVol
          bFading = 0
        End If
      ElseIf swfVideo IsNot Nothing Then
        swfVideo.Playing = True
      End If
    Catch ex As Exception
      Debug.Print("Failed to play: " & ex.Message)
    End Try
  End Sub

  Public Sub mpStop()
    On Error GoTo Erred
    If Me.State = MediaState.mStopped Then Return
    If IsSTA() AndAlso obj_mpControl IsNot Nothing Then
      If CFm_HasVid Or (Not CFm_StateFade) Then
        obj_mpControl.Stop()
        obj_mpPosition.put_CurrentPosition(CFm_PadStart)
      Else
        Dim lCurPos, lStopTime As Double
        lCurPos = Me.Position
        lStopTime = Me.Duration
        If lCurPos >= lStopTime Then
          obj_mpControl.Stop()
          obj_mpPosition.put_CurrentPosition(CFm_PadStart)
        Else
          CFm_State = MediaState.mStopped
          lFadePos = Me.Position
          bFading = 3
          Dim lStartVol As Long = Me.LogVolume
          Dim lTic As Long = Environment.TickCount + (lStartVol * 0.05)
          Do Until Me.LogVolume <= 1
            If Not bFading = 3 Then Return
            Dim lTmpVol As Long = (lTic - Environment.TickCount) / 0.05
            If lTmpVol < 0 Or lTmpVol > lStartVol Then
              Me.LogVolume = 0
            Else
              Me.LogVolume = lTmpVol
            End If
          Loop
          obj_mpControl.Stop()
          obj_mpPosition.put_CurrentPosition(CFm_PadStart)
          Me.LogVolume = lStartVol
          bFading = 0
        End If
      End If
    ElseIf swfVideo IsNot Nothing Then
      swfVideo.Playing = False
      swfVideo.GotoFrame(0)
    End If
    Return
Erred:
    RaiseEvent MediaError(New MediaErrorEventArgs(Err.GetException, "mpStop"))
    Resume Next
  End Sub

  Public Property Mute() As Boolean
    Get
      Mute = CFm_Mute
    End Get
    Set(value As Boolean)
      If Not CFm_Mute = value Then
        CFm_Mute = value
        If IsSTA() AndAlso obj_mpAudio IsNot Nothing Then
          If CFm_Mute Then
            obj_mpAudio.get_Volume(CFm_Volume)
            obj_mpAudio.put_Volume(-10000)
          Else
            obj_mpAudio.put_Volume(CFm_Volume)
          End If
        End If
      End If
    End Set
  End Property

  Public Property Repeat() As Boolean
    Get
      Return CFm_Loop
    End Get
    Set(value As Boolean)
      If swfVideo IsNot Nothing Then swfVideo.Loop = value
      CFm_Loop = value
    End Set
  End Property

  Public Property Position() As Double
    Get
      Try
        If IsSTA() AndAlso obj_mpPosition IsNot Nothing Then
          If bFading = 0 Then
            Dim lPos As Double
            If CFm_Filename.EndsWith("VIDEO_TS", True, Nothing) Then
              Dim dvdLoc As New Dvd.DvdPlaybackLocation2
              obj_dvdInfo.GetCurrentLocation(dvdLoc)

              If (dvdLoc.TimeCodeFlags And Dvd.DvdTimeCodeFlags.FPS25) = Dvd.DvdTimeCodeFlags.FPS25 Then
                dvdFPS = 25
              ElseIf (dvdLoc.TimeCodeFlags And Dvd.DvdTimeCodeFlags.FPS30) = Dvd.DvdTimeCodeFlags.FPS30 Then
                dvdFPS = 29.97
              Else
                dvdFPS = 23.976
              End If
              Return getDvdSeconds(dvdLoc.TimeCode, dvdFPS)
            Else
              obj_mpPosition.get_CurrentPosition(lPos)
            End If
            Return lPos
          Else
            Return lFadePos
          End If
        ElseIf swfVideo IsNot Nothing Then
          Return swfVideo.CurrentFrame
        Else
          Return 0
        End If
      Catch ex As Exception
        Return 0
      End Try
    End Get
    Set(value As Double)
      If (IsSTA() AndAlso obj_mpPosition IsNot Nothing And bFading = 0) Or swfVideo IsNot Nothing Then SeekPosition(value)
    End Set
  End Property

  Public ReadOnly Property DurDiscrep As Boolean
    Get
      Return Math.Abs(Me.SecondDur - Me.Duration) > 1
    End Get
  End Property

  Public ReadOnly Property SecondDur
    Get
      Dim lDur As Double
      If IsSTA() AndAlso obj_mpPosition IsNot Nothing Then
        obj_mpPosition.get_Duration(lDur)
      ElseIf swfVideo IsNot Nothing Then
        lDur = swfVideo.TotalFrames
      Else
        lDur = -1
      End If
      Return lDur
    End Get
  End Property

  Public Property Rate() As Double
    Get
      If IsSTA() AndAlso obj_mpPosition IsNot Nothing Then
        Dim lRate As Double
        obj_mpPosition.get_Rate(lRate)
        Return lRate
      Else
        Return CFm_Rate
      End If
    End Get
    Set(value As Double)
      Dim Pos As Double = Me.Position
      Dim DoPlay As Boolean
      If IsSTA() AndAlso obj_mpPosition IsNot Nothing Then
        DoPlay = Me.State = MediaState.mPlaying
        If DoPlay Then mpPause()
        If value > 2 Then value = 2
        If value < 0.01 Then value = 0.01
        Try
          If obj_mpPosition IsNot Nothing Then obj_mpPosition.put_Rate(value)
        Catch ex As Exception
          CFm_Rate = 1
          If obj_mpPosition IsNot Nothing Then obj_mpPosition.put_Rate(1)
          If DoPlay Then mpPlay()
          Exit Property
        End Try
        Application.DoEvents()
        CFm_Rate = value
        If obj_mpPosition IsNot Nothing Then SeekPosition(Pos)
        If DoPlay Then mpPlay()
      Else
        CFm_Rate = 1
        If IsSTA() AndAlso obj_mpPosition IsNot Nothing Then obj_mpPosition.put_Rate(1)
        If DoPlay Then mpPlay()
      End If
    End Set
  End Property

  Public Function ShowProperties(FilterName As String, ByVal ParenthWnd As IntPtr) As Boolean
    If IsSTA() AndAlso obj_mpControl IsNot Nothing Then
      Dim iFilters As IBaseFilter() = GetFilterList(obj_mpControl)
      If iFilters Is Nothing Then Return False
      For Each objFilter As IBaseFilter In iFilters
        Dim fInfo As FilterInfo = Nothing
        objFilter.QueryFilterInfo(fInfo)
        If String.Compare(fInfo.achName, FilterName, True) = 0 Then
          Dim pProp As ISpecifyPropertyPages = TryCast(objFilter, ISpecifyPropertyPages)
          Dim hr As Integer = 0
          If pProp Is Nothing Then
            Dim compressDialog As IAMVfwCompressDialogs = TryCast(objFilter, IAMVfwCompressDialogs)
            If compressDialog IsNot Nothing Then
              hr = compressDialog.ShowDialog(VfwCompressDialogs.Config, IntPtr.Zero)
              If hr <> 0 Then Return False
            End If
            Return False
          End If
          Dim filterInfo As FilterInfo = Nothing
          hr = objFilter.QueryFilterInfo(filterInfo)
          If hr <> 0 Then Return False
          Dim caGUID As DsCAUUID
          hr = pProp.GetPages(caGUID)
          If hr <> 0 Then Return False
          Dim oDevice As Object = DirectCast(objFilter, Object)
          hr = SeedFuncts.OleCreatePropertyFrame(ParenthWnd, 0, 0, filterInfo.achName, 1, oDevice, caGUID.cElems, caGUID.pElems, 0, 0, IntPtr.Zero)
          If hr <> 0 Then Return False
          Marshal.FreeCoTaskMem(caGUID.pElems)
          Marshal.ReleaseComObject(pProp)
          Marshal.ReleaseComObject(filterInfo.pGraph)
          Return True
        End If
      Next
    End If
    Return False
  End Function

  Public ReadOnly Property State() As MediaState
    Get
      If bFading = 0 Then GetState()
      Return CFm_State
    End Get
  End Property

  Public Property VideoHeight() As Long
    Get
      Return CFm_VideoHeight
    End Get
    Set(value As Long)
      Dim VidW As Long
      Dim VidH As Long
      If IsSTA() AndAlso obj_mpVideo IsNot Nothing Then
        obj_mpVideo.GetVideoSize(VidW, VidH)
      ElseIf swfVideo IsNot Nothing Then
        VidW = swfVideo.TGetPropertyAsNumber("/", 8)
        VidH = swfVideo.TGetPropertyAsNumber("/", 9)
      Else
        VidW = 0
        VidH = 0
      End If
      If value > 0 Then
        CFm_VideoHeight = value
      Else
        CFm_VideoHeight = VidH
      End If
    End Set
  End Property

  Public Property VideoWidth As Long
    Get
      Return CFm_VideoWidth
    End Get
    Set(value As Long)
      Dim VidW As Long
      Dim VidH As Long
      If IsSTA() AndAlso obj_mpVideo IsNot Nothing Then
        obj_mpVideo.GetVideoSize(VidW, VidH)
      ElseIf swfVideo IsNot Nothing Then
        VidW = swfVideo.TGetPropertyAsNumber("/", 8)
        VidH = swfVideo.TGetPropertyAsNumber("/", 9)
      Else
        VidW = 0
        VidH = 0
      End If
      If value > 0 Then
        CFm_VideoWidth = value
      Else
        CFm_VideoWidth = VidW
      End If
    End Set
  End Property

  Public Property LinearVolume As Long '0 to 100
    Get
      If Not IsSTA() Or CFm_Mute Or obj_mpAudio Is Nothing Or Not bFading = 0 Then
        Return (CFm_Volume + 10000) / 100
      Else
        Dim iVal As Integer
        obj_mpAudio.get_Volume(iVal)
        Return (iVal + 10000) / 100
      End If
    End Get
    Set(value As Long)
      If value > 100 Then
        value = 100
      ElseIf value < 0 Then
        value = 0
      End If
      If value = 0 Then
        CFm_Volume = -10000
      Else
        CFm_Volume = value * 100 - 10000
      End If
      If IsSTA() AndAlso obj_mpAudio IsNot Nothing Then
        If CFm_Mute Then
          obj_mpAudio.put_Volume(-10000)
        Else
          obj_mpAudio.put_Volume(CFm_Volume)
        End If
      End If
    End Set
  End Property

  Public Property LogVolume() As Long '1 to 10000
    Get
      If Not IsSTA() Or CFm_Mute Or obj_mpAudio Is Nothing Or Not bFading = 0 Then
        Return 10 ^ ((10000 + CFm_Volume) * 4 / 10000)
      Else
        Dim iVal As Integer
        obj_mpAudio.get_Volume(iVal)
        Return (10 ^ ((10000 + iVal) * 4 / 10000))
      End If
    End Get
    Set(value As Long)
      If value > 10000 Then
        value = 10000
      ElseIf value < 1 Then
        value = 1
      End If
      CFm_Volume = Math.Floor(Math.Log10(value) / 4 * 10000) - 10000
      If IsSTA() AndAlso obj_mpAudio IsNot Nothing Then
        If CFm_Mute Then
          obj_mpAudio.put_Volume(-10000)
        Else
          obj_mpAudio.put_Volume(CFm_Volume)
        End If
      End If
    End Set
  End Property

  Public Property StateFade() As Boolean
    Get
      Return CFm_StateFade
    End Get
    Set(value As Boolean)
      CFm_StateFade = value
    End Set
  End Property

  Public Shared Function GetRenderers(RenderType As String) As DsDevice()
    Dim cats As New List(Of DsDevice)(DsDevice.GetDevicesOfCat(FilterCategory.ActiveMovieCategories))
    Dim AudioRenderer = From xItem In cats Where xItem.Name.ToLower = RenderType.ToLower
    If AudioRenderer.Count > 0 Then
      Dim devices As New List(Of DsDevice)(DsDevice.GetDevicesOfCat(GetMonikerGuid(AudioRenderer(0).Mon)))
      Return devices.ToArray
    Else
      Return Nothing
    End If
  End Function

  Public Shared Function GetMonikerGuid(m_Mon As ComTypes.IMoniker) As Guid
    Dim bag As IPropertyBag = Nothing
    Dim ret As Guid = Guid.Empty
    Dim bagObj As Object = Nothing
    Dim val As Object = Nothing
    Try
      Dim bagId As Guid = GetType(IPropertyBag).GUID
      m_Mon.BindToStorage(Nothing, Nothing, bagId, bagObj)
      bag = DirectCast(bagObj, IPropertyBag)

      Dim hr As Integer = bag.Read("clsid", val, Nothing)
      DsError.ThrowExceptionForHR(hr)

      ret = New Guid(TryCast(val, String))
    Catch
      ret = Guid.Empty
    Finally
      bag = Nothing
      If bagObj IsNot Nothing Then
        Marshal.ReleaseComObject(bagObj)
        bagObj = Nothing
      End If
    End Try

    Return ret
  End Function

  Private Function CreateManager(Filename As String, ByRef TempName As String, ByRef result As Integer) As IMediaControl
    Const DEFAULTDEVICE As String = "Default DirectSound Device"
    Const FFDSHOWAUDIO As String = "ffdshow Audio Decoder"
    Dim tmpGraph As IFilterGraph = New FilterGraph
    Dim tmpControl As IMediaControl = tmpGraph
    TempName = TrimFile(Filename)
    AddToBuilder(tmpGraph, "Audio Renderers", "DirectSound: " & CFm_AudioDevice, DEFAULTDEVICE)
    AddToBuilder(tmpGraph, "directshow filters", FFDSHOWAUDIO)
    Try
      Dim iRet As Integer
      If String.IsNullOrEmpty(TempName) Then
        iRet = tmpControl.RenderFile(Filename)
      Else
        iRet = tmpControl.RenderFile(TempName)
      End If
      result = iRet
      If iRet < 0 Then
        'Debug.Print("Render Return: " & iRet)
        Return Nothing
      End If
    Catch ex As Exception
      Debug.Print(Hex$(Err.Number) & ": " & Err.Description)
      Return Nothing
    End Try
    Return tmpControl
  End Function

  Private Sub AddToBuilder(ByRef Builder As IGraphBuilder, Renderer As String, Filter As String, Optional Name As String = Nothing)
    Try
      Dim dsDev As DsDevice = (From devs In GetRenderers(Renderer) Where String.Compare(devs.Name, Filter, True) = 0).FirstOrDefault
      If dsDev IsNot Nothing Then
        Dim gu As Guid = GetType(IBaseFilter).GUID
        Dim o As Object = Nothing
        dsDev.Mon.BindToObject(Nothing, Nothing, gu, o)
        Dim iDecoder = DirectCast(o, IBaseFilter)
        If String.IsNullOrEmpty(Name) Then Name = Filter
        Builder.AddFilter(iDecoder, Name)
      End If
    Catch ex As Exception
      Debug.Print("Can't Add " & Filter & ".")
    End Try
  End Sub


  Private Function CreateDVDManager(VIDEO_TSfolder As String, ByRef Result As Integer) As IMediaControl
    Const FFDSHOWVIDEO As String = "ffdshow Video Decoder"
    Const FFDSHOWRAWVID As String = "ffdshow raw video filter"
    Dim hr As Integer
    Dim status As Dvd.AMDvdRenderStatus
    Dim comobj As Object = Nothing
    Try
      obj_dvdGraph = DirectCast(New DvdGraphBuilder(), Dvd.IDvdGraphBuilder)
      hr = obj_dvdGraph.GetFiltergraph(obj_dvdBuilder)
      DsError.ThrowExceptionForHR(hr)
      AddToBuilder(obj_dvdBuilder, "DirectShow Filters", FFDSHOWVIDEO)
      AddToBuilder(obj_dvdBuilder, "DirectShow Filters", FFDSHOWRAWVID)
      hr = obj_dvdGraph.RenderDvdVideoVolume(VIDEO_TSfolder, Dvd.AMDvdGraphFlags.None, status)

      If status.iNumStreamsFailed > 0 Then
        Dim NoAud As Boolean = CBool((status.dwFailedStreamsFlag And Dvd.AMDvdStreamFlags.Audio) = Dvd.AMDvdStreamFlags.Audio)
        Dim NoVid As Boolean = CBool((status.dwFailedStreamsFlag And Dvd.AMDvdStreamFlags.Video) = Dvd.AMDvdStreamFlags.Video)
        Dim NoSub As Boolean = CBool((status.dwFailedStreamsFlag And Dvd.AMDvdStreamFlags.SubPic) = Dvd.AMDvdStreamFlags.SubPic)
        Debug.Print(IIf(NoAud, "No Audio ", String.Empty) & IIf(NoVid, "No Video", String.Empty))
        If NoAud And NoVid And NoSub Then
          Result = &H80040218
        ElseIf NoAud And NoVid Then
          Result = &H80040218
        ElseIf NoAud Then
          Result = &H40258
        ElseIf NoVid Then
          Result = &H40257
        ElseIf NoSub Then
          Result = &H40242
        Else
          Result = 0
        End If
      End If

      DsError.ThrowExceptionForHR(hr)

      hr = obj_dvdGraph.GetDvdInterface(GetType(Dvd.IDvdInfo2).GUID, comobj)
      DsError.ThrowExceptionForHR(hr)
      obj_dvdInfo = DirectCast(comobj, Dvd.IDvdInfo2)
      comobj = Nothing

      hr = obj_dvdGraph.GetDvdInterface(GetType(Dvd.IDvdControl2).GUID, comobj)
      DsError.ThrowExceptionForHR(hr)
      obj_dvdControl = DirectCast(comobj, Dvd.IDvdControl2)
      comobj = Nothing

      SetDVDCaptioning(False)

      hr = obj_dvdControl.SetOption(Dvd.DvdOptionFlag.HMSFTimeCodeEvents, True)

      DsError.ThrowExceptionForHR(hr)

      hr = obj_dvdControl.SetOption(Dvd.DvdOptionFlag.ResetOnStop, False)
      DsError.ThrowExceptionForHR(hr)
      Result = 0
      Return DirectCast(obj_dvdBuilder, IMediaControl)
    Catch ee As Exception
      Debug.Print("DVD Error: " & ee.Message)
      Result = Err.Number
      Return Nothing
    Finally
      If comobj IsNot Nothing Then
        Marshal.ReleaseComObject(comobj)
        comobj = Nothing
      End If
    End Try
  End Function

  Private Function OpenMedia(Filename As String) As Integer
    Dim ret As Integer
    If Filename.EndsWith("VIDEO_TS", True, Nothing) Then
      obj_mpControl = CreateDVDManager(Filename, ret)
    Else
      obj_mpControl = CreateManager(Filename, TempFilename, ret)
    End If
    If ret < 0 Then
      CloseMedia()
      Debug.Print("Could not open: " & Hex(ret))
      Return ret
    End If
    If obj_mpControl Is Nothing Then
      CloseMedia()
      Return -1
    End If
    Return 0
  End Function

  Private Sub CloseMedia()
    Try

      If swfVideo IsNot Nothing AndAlso Not swfVideo.IsDisposed Then
        swfVideo.Movie = " "
        swfVideo.Visible = False
        swfVideo.Dispose()
        swfVideo = Nothing
      End If

      If obj_mpControl IsNot Nothing Then obj_mpControl.Stop()
      If obj_mpVidWindow IsNot Nothing Then
        obj_mpVidWindow.put_Visible(OABool.False)
        obj_mpVidWindow.put_MessageDrain(IntPtr.Zero)
        obj_mpVidWindow.put_Owner(IntPtr.Zero)
      End If
      If obj_mpEvent IsNot Nothing Then
        obj_mpEvent.FreeEventParams(WM_GRAPHNOTIFY, 0, 0)
      End If
      If obj_mpVideo IsNot Nothing Then
        Marshal.FinalReleaseComObject(obj_mpVideo)
        obj_mpVideo = Nothing
      End If
      If obj_mpVidWindow IsNot Nothing Then
        Marshal.FinalReleaseComObject(obj_mpVidWindow)
        obj_mpVidWindow = Nothing
      End If
      If obj_mpAudio IsNot Nothing Then
        Marshal.FinalReleaseComObject(obj_mpAudio)
        obj_mpAudio = Nothing
      End If
      If obj_mpEvent IsNot Nothing Then
        Marshal.FinalReleaseComObject(obj_mpEvent)
        obj_mpEvent = Nothing
      End If
      If obj_mpPosition IsNot Nothing Then
        Marshal.FinalReleaseComObject(obj_mpPosition)
        obj_mpPosition = Nothing
      End If
      If obj_mpControl IsNot Nothing Then
        Marshal.ReleaseComObject(obj_mpControl)
        obj_mpControl = Nothing
      End If
      If obj_dvdInfo IsNot Nothing Then
        Marshal.FinalReleaseComObject(obj_dvdInfo)
        obj_dvdInfo = Nothing
      End If
      If obj_dvdControl IsNot Nothing Then
        Marshal.FinalReleaseComObject(obj_dvdControl)
        obj_dvdControl = Nothing
      End If
      If obj_dvdGraph IsNot Nothing Then
        Marshal.FinalReleaseComObject(obj_dvdGraph)
        obj_dvdGraph = Nothing
      End If
      If obj_dvdBuilder IsNot Nothing Then
        Marshal.FinalReleaseComObject(obj_dvdBuilder)
        obj_dvdBuilder = Nothing
      End If
      dvdFPS = 0
      If Not String.IsNullOrEmpty(TempFilename) Then
        If String.Compare(TempFilename, CFm_Filename) <> 0 AndAlso IO.Path.GetDirectoryName(TempFilename) = My.Computer.FileSystem.SpecialDirectories.Temp Then
          If My.Computer.FileSystem.FileExists(TempFilename) Then My.Computer.FileSystem.DeleteFile(TempFilename)
          If My.Computer.FileSystem.FileExists(TempFilename) Then If Not CleanupItems.Contains(TempFilename) Then CleanupItems.Add(TempFilename)
        End If
      End If
    Catch e As Exception
      Debug.Print("Close Media error: " & e.Message)
      RaiseEvent MediaError(New MediaErrorEventArgs(e, "CloseMedia"))
    End Try
  End Sub

  Private Sub GetState()
    If IsSTA() Then
      If obj_mpControl IsNot Nothing Then
        Dim iState As Long
        obj_mpControl.GetState(100, iState)
        Select Case iState
          Case 0
            CFm_State = MediaState.mStopped
          Case 1
            CFm_State = MediaState.mPaused
          Case 2
            CFm_State = MediaState.mPlaying
          Case Else
            CFm_State = MediaState.mOpen
        End Select
      ElseIf swfVideo IsNot Nothing Then
        If swfVideo.IsPlaying Then
          CFm_State = MediaState.mPlaying
        Else
          CFm_State = MediaState.mPaused
        End If
      Else
        CFm_State = MediaState.mClosed
      End If
    Else
      CFm_State = MediaState.mOpen
    End If
  End Sub

  Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
    Dim Code As DirectShowLib.EventCode
    Dim P1 As IntPtr
    Dim P2 As IntPtr
    If m.HWnd = Me.Handle Then
      Select Case m.Msg
        Case WM_GRAPHNOTIFY
          If obj_mpEvent IsNot Nothing Then
            obj_mpEvent.GetEvent(Code, P1, P2, 0)
            Select Case Code
              Case EventCode.Complete ' EC_COMPLETE
                If CFm_Loop Then
                  Me.Position = 0
                  mpPlay()
                Else
                  If obj_queuedControl IsNot Nothing Then
                    FinalizeQueue()
                    RaiseEvent EndOfFile(-1)
                  Else
                    RaiseEvent EndOfFile(P1.ToInt32)
                  End If
                End If
              Case EventCode.UserAbort ' EC_USERABORT
                Debug.Print("USER ABORT")
              Case EventCode.ErrorAbort ' EC_ERRORABORT
                If P1.ToInt32 = -2004287465 Then
                  'Application.DoEvents()
                  'mpStop()
                  ' Application.DoEvents()
                  'mpPlay()
                  RaiseEvent CantOpen("Not enough memory")
                Else
                  Debug.Print("ERROR ABORT " & Hex(P1.ToInt64))
                End If
              Case Else
                'Debug.Print(Code.ToString)
            End Select
          End If
        Case WM_ERASEBKGND
          'return
        Case &H200 'WM_MOUSEMOVE
          If (MouseButtons And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left Then
            'bah
          Else
            dvdLParam = m.LParam
            SelectButton(m.LParam, False)
          End If
        Case &H202 ' WM_LBUTTONUP 
          If dvdLParam <> IntPtr.Zero Then
            SelectButton(dvdLParam, True)
          Else
            SelectButton(m.LParam, True)
          End If
          Dim iP As Point = Int64ToPoint(m.LParam)
          RaiseEvent VidClick(Me, New MouseEventArgs(Windows.Forms.MouseButtons.Left, 1, iP.X, iP.Y, 0))
        Case &H203 'WM_LBUTTONDBLCLK
          RaiseEvent VidDoubleClick(Me, New EventArgs)
        Case &H205 'WM_RBUTTONUP
          Dim iP As Point = Int64ToPoint(m.LParam)
          RaiseEvent VidClick(Me, New MouseEventArgs(Windows.Forms.MouseButtons.Right, 1, iP.X, iP.Y, 0))
      End Select
    End If
    MyBase.WndProc(m)
  End Sub

  Private Function Int64ToPoint([in] As Int64) As Point
    Return New Point(([in] And &HFFFF), ([in] And &HFFFF0000) >> 16)
  End Function

  Public Sub SelectButton(lParam As IntPtr, Activate As Boolean)
    If obj_dvdControl Is Nothing Then Return
    Dim pt As Point = Int64ToPoint(lParam.ToInt64)
    If Activate Then
      obj_dvdControl.ActivateAtPosition(pt)
    Else
      obj_dvdControl.SelectAtPosition(pt)
    End If

  End Sub

  Private Sub SeekPosition(Pos As Double)
    If swfVideo IsNot Nothing Then
      swfVideo.GotoFrame(Pos)
    Else
      If IsSTA() Then
        If CFm_Filename.EndsWith("VIDEO_TS", True, Nothing) Then
          If dvdFPS = 0 Then
            Debug.Print("Can't seek yet! No FPS set!")
          Else
            obj_dvdControl.PlayAtTime(getDVDTime(Pos, dvdFPS), Dvd.DvdCmdFlags.None, Nothing)
          End If
        Else
          If Pos < CFm_PadStart Then
            obj_mpPosition.put_CurrentPosition(CFm_PadStart)
          ElseIf Pos >= Me.SecondDur Then
            obj_mpPosition.put_CurrentPosition(Me.SecondDur)
          ElseIf Pos >= Me.Duration Then
            obj_mpPosition.put_CurrentPosition(Me.Duration)
          Else
            obj_mpPosition.put_CurrentPosition(Pos)
          End If
        End If
      End If
    End If
  End Sub

  Private Sub SetFullScreen()
    If swfVideo IsNot Nothing Then
      'nothing
    Else
      If IsSTA() AndAlso obj_mpVidWindow IsNot Nothing And CFm_FSObj IsNot Nothing Then
        If CFm_FullScreen Then
          Dim hWnd As IntPtr = CFm_FSObj.Handle
          obj_mpVidWindow.put_Owner(hWnd)
          VideoFullSize()
        Else
          obj_mpVidWindow.put_Owner(Me.Handle)
          VideoAutoSize()
        End If
      End If
    End If
  End Sub

  Public Sub CropVideo(Left As Integer, Top As Integer, Width As Integer, Height As Integer)
    If IsSTA() AndAlso obj_mpVideo IsNot Nothing Then obj_mpVideo.SetSourcePosition(Left, Top, Width, Height)
  End Sub

  Public Sub New()
    InitializeComponent()
    CFm_VideoWidth = 320
    CFm_VideoHeight = 240
    If IsSTA() Then
      swfTemplate = New AxShockwaveFlashObjects.AxShockwaveFlash
    Else
      swfTemplate = Nothing
    End If
    VideoAutoSize()
    CFm_Volume = 0
    CFm_Balance = 0
    CFm_queueTime = 0.17
    tmrJustBefore.Enabled = False
    FileClose()
  End Sub


  Private Sub UserControl_Resize() Handles Me.Resize
    imgLogo.Top = Me.Height / 2 - imgLogo.Height / 2
    imgLogo.Left = Me.Width / 2 - imgLogo.Width / 2
    If Not CFm_FullScreen Then VideoAutoSize()
  End Sub

  Private Sub UserControl_Disposed() Handles Me.Disposed
    DownloadingState = "CLOSE"
    tmrJustBefore.Enabled = False
    FileClose()
    For Each Item In CleanupItems
      Try
        If My.Computer.FileSystem.FileExists(Item) Then My.Computer.FileSystem.DeleteFile(Item)
      Catch ex As Exception
        Debug.Print(Item & " is busy...")
      End Try
    Next
  End Sub

  Private Sub VideoAutoSize()
    Dim useX, useY, useW, useH As Integer
    If CFm_VideoWidth <> 0 And CFm_VideoHeight <> 0 Then
      If Me.Width / CFm_VideoWidth > Me.Height / CFm_VideoHeight Then
        useH = Me.Height
        useW = (Me.Height / CFm_VideoHeight) * CFm_VideoWidth
      Else
        useW = Me.Width
        useH = (Me.Width / CFm_VideoWidth) * CFm_VideoHeight
      End If
      useX = ((Me.Width) / 2) - ((useW) / 2)
      useY = ((Me.Height) / 2) - ((useH) / 2)
      If IsSTA() AndAlso obj_mpVidWindow IsNot Nothing Then
        obj_mpVidWindow.put_Top(useY)
        obj_mpVidWindow.put_Left(useX)
        obj_mpVidWindow.put_Width(useW)
        obj_mpVidWindow.put_Height(useH)
      End If
    End If
  End Sub

  Private Sub VideoFullSize()
    Dim useX, useY, useW, useH As Integer
    If CFm_VideoWidth <> 0 And CFm_VideoHeight <> 0 Then
      If CFm_FSObj.Width / CFm_VideoWidth > CFm_FSObj.Height / CFm_VideoHeight Then
        useH = CFm_FSObj.Height
        useW = (CFm_FSObj.Height / CFm_VideoHeight) * CFm_VideoWidth
      Else
        useW = CFm_FSObj.Width
        useH = (CFm_FSObj.Width / CFm_VideoWidth) * CFm_VideoHeight
      End If
      useX = ((CFm_FSObj.Width) / 2) - ((useW) / 2)
      useY = ((CFm_FSObj.Height) / 2) - ((useH) / 2)
      If IsSTA() AndAlso obj_mpVidWindow IsNot Nothing Then
        obj_mpVidWindow.put_Top(useY)
        obj_mpVidWindow.put_Left(useX)
        obj_mpVidWindow.put_Width(useW)
        obj_mpVidWindow.put_Height(useH)
      End If
    End If
  End Sub

  '0 = ok
  '-1 = failed to open
  '-2 = doesn't exist
  '-3 = Download Error
  '-4 = Other Error
  'else = error code

  Private Function FileOpen(FilePath As String) As Integer
    Try
      FileClose()
      If String.IsNullOrEmpty(FilePath) Then Return -2
      If (My.Computer.FileSystem.FileExists(FilePath) AndAlso My.Computer.FileSystem.GetFileInfo(FilePath).Length > 0) Or (FilePath.EndsWith("VIDEO_TS", True, Nothing) And My.Computer.FileSystem.DirectoryExists(FilePath)) Then
        Select Case IO.Path.GetExtension(FilePath).ToLower
          Case ".swf", ".spl"
            swfVideo = New AxShockwaveFlashObjects.AxShockwaveFlash
            swfVideo.BeginInit()
            Me.SuspendLayout()
            swfVideo.Location = New Point(0, 0)
            swfVideo.Name = "swfVideo"
            Dim swR As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSeed))
            swfVideo.OcxState = swR.GetObject("swfTemplate.OcxState")
            swfVideo.Dock = DockStyle.Fill
            swfVideo.TabIndex = 0
            Me.Controls.Add(swfVideo)
            swfVideo.EndInit()
            Me.ResumeLayout(False)
            swfVideo.Visible = True
            imgLogo.Visible = False
            swfVideo.LoadMovie(0, FilePath)
            swfVideo.Menu = False
            swfVideo.Loop = CFm_Loop
            CFm_Filename = FilePath
            CFm_HasAud = False
            CFm_HasVid = True
            CFm_VideoHeight = swfVideo.TGetPropertyAsNumber("/", 9)
            CFm_VideoWidth = swfVideo.TGetPropertyAsNumber("/", 8)
            Return 0
          Case Else
            Dim Ret As Integer = OpenMedia(FilePath)
            Select Case Ret
              Case 0
                SpreadManagement(obj_mpControl, FilePath)
                If CFm_HasVid Then SetFullScreen()
                Return 0
              Case Else
                Return Ret
            End Select
        End Select
      ElseIf FilePath.ToLower.StartsWith("http://") Then
        Try
          Dim tmpURI As New Uri(FilePath)
          Dim tmpFile As String = IO.Path.Combine(IO.Path.GetTempPath, "seedTemp", IO.Path.GetFileName(tmpURI.LocalPath))
          If Not IO.Directory.Exists(IO.Path.Combine(IO.Path.GetTempPath, "seedTemp")) Then IO.Directory.CreateDirectory(IO.Path.Combine(IO.Path.GetTempPath, "seedTemp"))
          If IO.File.Exists(tmpFile) Then IO.File.Delete(tmpFile)
          sckDownload = New SocketWrapper
          downloadURL = tmpURI
          downloadDest = tmpFile
          downloadType = 0
          downloadHeader = False
          cacheHeader = Nothing
          sckDownload.RemoteEndPoint = New Net.DnsEndPoint(tmpURI.Host, tmpURI.Port)
          sckDownload.Connect()
          DownloadingState = Nothing
          Do While String.IsNullOrEmpty(DownloadingState)
            Application.DoEvents()
            Threading.Thread.Sleep(0)
          Loop
          If sckDownload IsNot Nothing And downloadType = 0 Then sckDownload.Dispose()
          If DownloadingState = "CLOSE" Then Return -4
          If DownloadingState = "DONE" Then
            Return FileOpen(tmpFile)
          ElseIf downloadType = 1 Then
            Return FileOpen(DownloadingState)
          Else
            CFm_Filename = DownloadingState
            Return -4
          End If
        Catch ex As Exception
          Return -3
        End Try
      Else
        Return -2
      End If
    Catch ex As Exception
      CFm_Filename = ex.Message
      Return -4
    End Try
  End Function

  Private DownloadingState As String
  Private downloadURL As Uri
  Private downloadDid As ULong
  Private downloadSize As ULong
  Private downloadDest As String

  Private downloadType As Byte
  Private downloadHeader As Boolean
  Private streamFile(2) As String
  Private streamSave As Byte
  Private icyStation As String
  Private icyBitRate As UShort
  Private icyMetaInt As UInteger
  Private icyLoopVal As UInteger

  Private Sub sckDownload_SocketConnected(sender As Object, e As EventArgs) Handles sckDownload.SocketConnected
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New EventHandler(AddressOf sckDownload_SocketConnected), sender, e)
      Catch ex As Exception

      End Try
    Else
      Dim sSend As String = "GET " & downloadURL.LocalPath & " HTTP/1.1" & vbCrLf
      sSend &= "Host: " & downloadURL.Host & vbCrLf
      sSend &= "Accept: */*" & vbCrLf
      sSend &= "Icy-MetaData:1" & vbCrLf
      sSend &= vbCrLf
      Dim bSend As Byte() = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes(sSend)
      sckDownload.Send(bSend)
    End If
  End Sub

  Private cacheHeader As String
  Private Sub sckDownload_SocketReceived(sender As Object, e As SocketReceivedEventArgs) Handles sckDownload.SocketReceived
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New EventHandler(Of SocketReceivedEventArgs)(AddressOf sckDownload_SocketReceived), sender, e)
      Catch ex As Exception

      End Try
    Else
      If sckDownload Is Nothing Then Return
      If Not sckDownload.IsConnected Then Return
      If downloadType = 0 Then
        'direct
        If Not downloadHeader Then
          Dim sData As String = System.Text.Encoding.GetEncoding(LATIN_1).GetString(e.Data)
          Dim sNewLine As String = Nothing
          If sData.Contains(vbCrLf) Then
            sNewLine = vbCrLf
          ElseIf sData.Contains(vbLf) Then
            sNewLine = vbLf
          ElseIf sData.Contains(vbCr) Then
            sNewLine = vbCr
          End If
          If Not String.IsNullOrEmpty(sNewLine) Then
            Dim bData As Byte() = Nothing
            If sData.Contains(sNewLine & sNewLine) Then
              sData = sData.Substring(0, sData.IndexOf(sNewLine & sNewLine))
              ReDim bData(e.Data.Length - (sData.Length + 4) - 1)
              Array.ConstrainedCopy(e.Data, sData.Length + 4, bData, 0, e.Data.Length - (sData.Length + 4))
            Else
              cacheHeader &= sData
              Return
            End If
            If Not String.IsNullOrEmpty(cacheHeader) Then sData = cacheHeader & sData
            Dim sHeader() As String = Split(sData, sNewLine)
            If sHeader(0).Contains("200") Then
              'Stop
              icyMetaInt = 0
              For Each sH As String In sHeader
                If downloadType = 0 Then
                  If sH.ToLower.StartsWith("content-length:") Then
                    downloadSize = ULong.Parse(sH.Substring(sH.IndexOf(":") + 1).Trim)
                    Exit For
                  End If
                End If
                If sH.ToLower.StartsWith("icy-") Then
                  downloadType = 1
                  If sH.ToLower.StartsWith("icy-metaint") Then
                    icyMetaInt = UInteger.Parse(sH.Substring(sH.IndexOf(":") + 1).Trim)
                  ElseIf sH.ToLower.StartsWith("icy-name") Then
                    icyStation = sH.Substring(sH.IndexOf(":") + 1).Trim
                    StreamInfo = New StreamInformation(icyStation, Nothing, Nothing)
                  ElseIf sH.ToLower.StartsWith("icy-br") Then
                    icyBitRate = UShort.Parse(sH.Substring(sH.IndexOf(":") + 1).Trim)
                  End If
                End If
              Next
              If downloadType = 1 Then
                Dim sExt As String = "mp3"
                streamSave = 0
                streamFile(0) = IO.Path.Combine(IO.Path.GetTempPath, "seedTemp", "stream1." & sExt)
                streamFile(1) = IO.Path.Combine(IO.Path.GetTempPath, "seedTemp", "stream2." & sExt)
                streamFile(2) = IO.Path.Combine(IO.Path.GetTempPath, "seedTemp", "stream3." & sExt)
                Try
                  For I As Integer = 0 To 2
                    If IO.File.Exists(streamFile(I)) Then IO.File.Delete(streamFile(I))
                  Next
                Catch ex As Exception

                End Try
                If bData IsNot Nothing AndAlso bData.Count > 0 Then
                  If icyMetaInt > 0 AndAlso icyLoopVal + bData.Length >= icyMetaInt Then
                    PullMetaDataFromStream(bData)
                  Else
                    icyLoopVal += bData.Length
                  End If
                  My.Computer.FileSystem.WriteAllBytes(streamFile(streamSave), bData, True)

                End If
              Else
                If bData IsNot Nothing AndAlso bData.Count > 0 Then
                  My.Computer.FileSystem.WriteAllBytes(downloadDest, bData, True)
                  downloadDid += bData.Length
                  RaiseEvent NetDownload(Me, New DownloadChangedEventArgs(downloadDid, downloadSize, "BUFFER"))
                End If
              End If
              downloadHeader = True
              Return
            Else
              DownloadingState = sHeader(0)
              Return
            End If
          End If
        End If
        If e.Data IsNot Nothing AndAlso e.Data.Count > 0 Then
          My.Computer.FileSystem.WriteAllBytes(downloadDest, e.Data, True)
          downloadDid += e.Data.Length
          RaiseEvent NetDownload(Me, New DownloadChangedEventArgs(downloadDid, downloadSize, "DOWNLOAD"))
          If downloadDid >= downloadSize Then
            DownloadingState = "DONE"
          End If
        End If
      ElseIf downloadType = 1 Then
        'streaming
        Dim packetData As Byte() = e.Data
        If icyMetaInt > 0 AndAlso icyLoopVal + packetData.Length >= icyMetaInt Then
          PullMetaDataFromStream(packetData)
        Else
          icyLoopVal += packetData.Length
        End If

        Dim minSize As Long = 128 * icyBitRate * 10
        Dim firstSize As Long = Math.Floor(minSize * 1.5)
        Dim streamNext As Byte = streamSave + 1
        If streamNext = 3 Then streamNext = 0
        Dim streamPrevious As Byte = streamNext + 1
        If streamPrevious = 3 Then streamPrevious = 0
        If IO.File.Exists(streamFile(streamSave)) Then
          Dim curSize As Long = My.Computer.FileSystem.GetFileInfo(streamFile(streamSave)).Length
          If curSize >= firstSize And String.IsNullOrEmpty(CFm_Filename) Then
            If CFm_Filename = streamFile(streamPrevious) Then
              'duplicate of "buffered enough" below somehow
              For I As Integer = packetData.Length - 2 To 0 Step -1
                If packetData(I) = 255 Then
                  Dim possibleHeader As UInteger = GetDWORD(packetData, I)
                  Dim possibleMPEG As New clsMPEG(possibleHeader)
                  If possibleMPEG.CheckValidity Then
                    If possibleMPEG.GetMPEGLayer = 3 Then
                      Dim topHalf(I - 2) As Byte
                      Dim bottomHalf(packetData.Length - I - 1) As Byte
                      Array.ConstrainedCopy(packetData, 0, topHalf, 0, I - 1)
                      Array.ConstrainedCopy(packetData, I, bottomHalf, 0, packetData.Length - I)
                      My.Computer.FileSystem.WriteAllBytes(streamFile(streamSave), topHalf, True)
                      If IO.File.Exists(streamFile(streamNext)) Then IO.File.Delete(streamFile(streamNext))
                      If State = MediaState.mPlaying Then
                        FileQueue(streamFile(streamSave))
                      Else
                        FileOpen(streamFile(streamSave))
                        mpPlay()
                      End If
                      streamSave = streamNext
                      My.Computer.FileSystem.WriteAllBytes(streamFile(streamSave), bottomHalf, True)
                      Exit For
                    End If
                  End If
                End If
              Next
            ElseIf Not My.Computer.FileSystem.FileExists(streamFile(streamNext)) And Not My.Computer.FileSystem.FileExists(streamFile(streamPrevious)) Then
              'No Next/After files, fresh stream

              For I As Integer = packetData.Length - 2 To 0 Step -1
                If packetData(I) = 255 Then
                  Dim possibleHeader As UInteger = GetDWORD(packetData, I)
                  Dim possibleMPEG As New clsMPEG(possibleHeader)
                  If possibleMPEG.CheckValidity Then
                    If possibleMPEG.GetMPEGLayer = 3 Then
                      Dim topHalf(I - 2) As Byte
                      Dim bottomHalf(packetData.Length - I - 1) As Byte
                      Array.ConstrainedCopy(packetData, 0, topHalf, 0, I - 1)
                      Array.ConstrainedCopy(packetData, I, bottomHalf, 0, packetData.Length - I)
                      My.Computer.FileSystem.WriteAllBytes(streamFile(streamSave), topHalf, True)
                      DownloadingState = streamFile(streamSave)
                      If IO.File.Exists(streamFile(streamNext)) Then IO.File.Delete(streamFile(streamNext))
                      streamSave = streamNext
                      My.Computer.FileSystem.WriteAllBytes(streamFile(streamSave), bottomHalf, True)
                      Exit For
                    End If
                  End If
                End If
              Next
            Else
              My.Computer.FileSystem.WriteAllBytes(streamFile(streamSave), packetData, True)
            End If
          ElseIf curSize >= minSize And CFm_Filename = streamFile(streamPrevious) Then
            'buffered enough
            For I As Integer = packetData.Length - 2 To 0 Step -1
              If packetData(I) = 255 Then
                Dim possibleHeader As UInteger = GetDWORD(packetData, I)
                Dim possibleMPEG As New clsMPEG(possibleHeader)
                If possibleMPEG.CheckValidity Then
                  If possibleMPEG.GetMPEGLayer = 3 Then
                    Dim topHalf(I - 2) As Byte
                    Dim bottomHalf(packetData.Length - I - 1) As Byte
                    Array.ConstrainedCopy(packetData, 0, topHalf, 0, I - 1)
                    Array.ConstrainedCopy(packetData, I, bottomHalf, 0, packetData.Length - I)
                    My.Computer.FileSystem.WriteAllBytes(streamFile(streamSave), topHalf, True)
                    If IO.File.Exists(streamFile(streamNext)) Then IO.File.Delete(streamFile(streamNext))
                    If State = MediaState.mPlaying Then
                      FileQueue(streamFile(streamSave))
                    Else
                      FileOpen(streamFile(streamSave))
                      mpPlay()
                    End If
                    streamSave = streamNext
                    My.Computer.FileSystem.WriteAllBytes(streamFile(streamSave), bottomHalf, True)
                    Exit For
                  End If
                End If
              End If
            Next
          Else
            My.Computer.FileSystem.WriteAllBytes(streamFile(streamSave), packetData, True)
            If State = MediaState.mStopped Or Not IsStreaming Then
              curSize += packetData.Length
              Dim maxSize As Long = firstSize
              If CFm_Filename = streamFile(streamPrevious) Then maxSize = minSize
              RaiseEvent NetDownload(Me, New DownloadChangedEventArgs(curSize, maxSize, "BUFFER"))
            End If
          End If
        Else
          My.Computer.FileSystem.WriteAllBytes(streamFile(streamSave), packetData, True)
          If State = MediaState.mStopped Or Not IsStreaming Then
            Dim curSize As Long = packetData.Length
            Dim maxSize As Long = firstSize
            If CFm_Filename = streamFile(streamPrevious) Then maxSize = minSize
            RaiseEvent NetDownload(Me, New DownloadChangedEventArgs(curSize, maxSize, "BUFFER"))
          End If
        End If
      End If
    End If
  End Sub

  Private Sub PullMetaDataFromStream(ByRef packetData As Byte())
    Dim icyPos As Integer = -1
    For I As Integer = 0 To packetData.Length - 1
      icyLoopVal += 1
      If icyLoopVal > icyMetaInt Then
        icyPos = I
        Exit For
      End If
    Next
    If icyPos = -1 Then Return
    Dim topHalf(icyPos - 1) As Byte
    Array.ConstrainedCopy(packetData, 0, topHalf, 0, icyPos)
    Dim totalLen As Integer = packetData(icyPos) * 16
    If icyPos + totalLen > packetData.Length Then
      Return
    End If
    Dim endPos As Integer = icyPos + totalLen
    If totalLen > 0 Then
      Dim endLoc As Integer = icyPos
      For I As Integer = icyPos + 1 To endPos
        If packetData(I) = 0 Then
          endLoc = I
          Exit For
        End If
      Next
      If endLoc = icyPos Then endLoc = endPos
      If endLoc > 0 Then
        Dim icyMetaData(endLoc - (icyPos + 1) - 1) As Byte
        Array.ConstrainedCopy(packetData, icyPos + 1, icyMetaData, 0, endLoc - (icyPos + 1))
        Dim sMetaData As String = System.Text.Encoding.GetEncoding(LATIN_1).GetString(icyMetaData)
        Dim sTitle As String = Nothing
        Dim sArtist As String = Nothing
        Dim sAlbum As String = Nothing
        Dim MetaTags As New Collections.Generic.Dictionary(Of String, String)
        If sMetaData.Contains(";") Then
          Dim mdN As Integer = 0
          Dim sMetaDatas() As String = Split(sMetaData, ";")
          For Each sData In sMetaDatas
            If String.IsNullOrEmpty(sData) Then Continue For
            If sData.Contains("=") Then
              Dim sMetaName As String = sData.Substring(0, sData.IndexOf("="))
              Dim sMetaVal As String = sData.Substring(sData.IndexOf("=") + 1)
              If sMetaVal.StartsWith("'") Then
                sMetaVal = sMetaVal.Substring(1)
                If sMetaVal.EndsWith("'") Then sMetaVal = sMetaVal.Substring(0, sMetaVal.Length - 1)
              ElseIf sMetaVal.StartsWith("""") Then
                sMetaVal = sMetaVal.Substring(1)
                If sMetaVal.EndsWith("""") Then sMetaVal = sMetaVal.Substring(0, sMetaVal.Length - 1)
              End If
              MetaTags.Add(sMetaName, sMetaVal)
            Else
              mdN += 1
              MetaTags.Add("MetaData" & mdN, sData)
            End If
          Next
        Else
          If sMetaData.Contains("=") Then
            Dim sMetaName As String = sMetaData.Substring(0, sMetaData.IndexOf("="))
            Dim sMetaVal As String = sMetaData.Substring(sMetaData.IndexOf("=") + 1)
            If sMetaVal.StartsWith("'") Then
              sMetaVal = sMetaVal.Substring(1)
              If sMetaVal.EndsWith("'") Then sMetaVal = sMetaVal.Substring(0, sMetaVal.Length - 1)
            ElseIf sMetaVal.StartsWith("""") Then
              sMetaVal = sMetaVal.Substring(1)
              If sMetaVal.EndsWith("""") Then sMetaVal = sMetaVal.Substring(0, sMetaVal.Length - 1)
            End If
            If sMetaName.Contains("&amp;") Then sMetaName = sMetaName.Replace("&amp;", "&")
            If sMetaVal.Contains("&amp;") Then sMetaVal = sMetaVal.Replace("&amp;", "&")
            MetaTags.Add(sMetaName, sMetaVal)
          Else
            MetaTags.Add("MetaData", sMetaData)
          End If
        End If
        If MetaTags.ContainsKey("StreamTitle") Then
          If MetaTags("StreamTitle").Contains(" - ") Then
            sArtist = MetaTags("StreamTitle").Substring(0, MetaTags("StreamTitle").IndexOf(" - "))
            sTitle = MetaTags("StreamTitle").Substring(MetaTags("StreamTitle").IndexOf(" - ") + 3)
          ElseIf MetaTags("StreamTitle").Contains("-") Then
            sArtist = MetaTags("StreamTitle").Substring(0, MetaTags("StreamTitle").IndexOf("-"))
            sTitle = MetaTags("StreamTitle").Substring(MetaTags("StreamTitle").IndexOf("-") + 1)
          Else
            sTitle = MetaTags("StreamTitle")
          End If
        Else
          sTitle = sMetaData
        End If
        If String.IsNullOrEmpty(sAlbum) And Not String.IsNullOrEmpty(icyStation) Then sAlbum = icyStation
        StreamInfo = New StreamInformation(sTitle, sArtist, sAlbum)
      End If
    End If
    icyLoopVal = 0
    Dim bottomHalf(packetData.Length - endPos - 2) As Byte
    Array.ConstrainedCopy(packetData, endPos + 1, bottomHalf, 0, packetData.Length - endPos - 1)
    PullMetaDataFromStream(bottomHalf)
    ReDim packetData(topHalf.Length + bottomHalf.Length - 1)
    Array.ConstrainedCopy(topHalf, 0, packetData, 0, topHalf.Length)
    Array.ConstrainedCopy(bottomHalf, 0, packetData, topHalf.Length, bottomHalf.Length)
  End Sub

  Private Sub sckDownload_SocketDisconnected(sender As Object, e As SocketErrorEventArgs) Handles sckDownload.SocketDisconnected
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New EventHandler(Of SocketErrorEventArgs)(AddressOf sckDownload_SocketDisconnected), sender, e)
      Catch ex As Exception

      End Try
    Else
      If String.IsNullOrEmpty(e.ErrorDetails) Then
        DownloadingState = "Disconnected"
      Else
        DownloadingState = e.ErrorDetails
      End If
    End If
  End Sub

  Public Sub SetNoQueue()
    tmrJustBefore.Enabled = False
    If obj_queuedControl IsNot Nothing Then
      Marshal.ReleaseComObject(obj_queuedControl)
      obj_queuedControl = Nothing
    End If
    CFm_queuedFilename = String.Empty
    CFm_queuedTempName = String.Empty
  End Sub

  Private Delegate Function FileQueueInvoker(FilePath As String) As Boolean
  Public Function FileQueue(FilePath As String) As Boolean
    If Me.InvokeRequired Then
      Return Me.Invoke(New FileQueueInvoker(AddressOf FileQueue), FilePath)
    Else
      Try
        If String.IsNullOrEmpty(FilePath) Then Return False
        If My.Computer.FileSystem.FileExists(FilePath) And My.Computer.FileSystem.GetFileInfo(FilePath).Length > 0 Then
          If IO.Path.GetExtension(FilePath).ToLower = ".swf" Or IO.Path.GetExtension(FilePath).ToLower = ".spl" Then Return False
          CFm_queuedFilename = FilePath
          tmrJustBefore.Enabled = True
          Dim ret As Integer
          obj_queuedControl = CreateManager(FilePath, CFm_queuedTempName, ret)
          If ret <> 0 Or obj_queuedControl IsNot Nothing Then
            Dim qPosition As IMediaPosition = obj_queuedControl
            Try
              Dim qAudio As IBasicAudio = obj_queuedControl
              Dim qVidWindow As IVideoWindow = obj_queuedControl
              qAudio.put_Volume(-10000)
              qVidWindow.put_AutoShow(OABool.False)
              obj_queuedControl.Run()
              obj_queuedControl.Pause()
              qPosition.put_CurrentPosition(0.0)
              If CFm_Mute Then
                qAudio.put_Volume(-10000)
              Else
                qAudio.put_Volume(CFm_Volume)
              End If
              qAudio.put_Balance(CFm_Balance)
              qPosition.put_Rate(1)
              qPosition = Nothing
              qAudio = Nothing
              qVidWindow = Nothing
              Return True
            Catch ex As Exception
              SetNoQueue()
              Return False
            End Try
          Else
            Debug.Print("Could not queue: " & Hex(ret))
            SetNoQueue()
            Return False
          End If
        Else
          SetNoQueue()
          Return False
        End If
      Catch
        SetNoQueue()
        Return False
      End Try
    End If
  End Function

  Private Function GetFilterList(ByRef manager As IMediaControl) As IBaseFilter()
    If swfVideo IsNot Nothing Then Return Nothing
    Dim objGraph As IFilterGraph = manager
    Dim objFilters As IEnumFilters = Nothing
    objGraph.EnumFilters(objFilters)
    objFilters.Reset()
    Dim fFilters() As IBaseFilter = Nothing
    Dim iFilter As Integer = 0
    Do
      Dim Renderer(0) As IBaseFilter
      Dim Fetched As IntPtr
      Dim iRet As Integer = objFilters.Next(1, Renderer, Fetched)
      If iRet <> 0 Then Exit Do
      ReDim Preserve fFilters(iFilter)
      fFilters(iFilter) = Renderer(0)
      iFilter += 1
    Loop
    Marshal.FinalReleaseComObject(objFilters)
    Return fFilters
  End Function

  Private Sub ResetFFResize()
    If My.Computer.Registry.CurrentUser.OpenSubKey("Software").GetSubKeyNames.Contains("GNU") Then
      If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("GNU").GetSubKeyNames.Contains("ffdshow") Then
        If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("GNU").OpenSubKey("ffdshow").GetSubKeyNames.Contains("default") Then
          My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow", True).OpenSubKey("default", True).SetValue("isResize", 0, Microsoft.Win32.RegistryValueKind.DWord)
          My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow", True).OpenSubKey("default", True).SetValue("resizeDx", 320, Microsoft.Win32.RegistryValueKind.DWord)
          My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow", True).OpenSubKey("default", True).SetValue("resizeDy", 240, Microsoft.Win32.RegistryValueKind.DWord)
        End If
      End If
      If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("GNU").GetSubKeyNames.Contains("ffdshow64") Then
        If My.Computer.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("GNU").OpenSubKey("ffdshow64").GetSubKeyNames.Contains("default") Then
          My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow64", True).OpenSubKey("default", True).SetValue("isResize", 0, Microsoft.Win32.RegistryValueKind.DWord)
          My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow64", True).OpenSubKey("default", True).SetValue("resizeDx", 320, Microsoft.Win32.RegistryValueKind.DWord)
          My.Computer.Registry.CurrentUser.OpenSubKey("Software", True).OpenSubKey("GNU", True).OpenSubKey("ffdshow64", True).OpenSubKey("default", True).SetValue("resizeDy", 240, Microsoft.Win32.RegistryValueKind.DWord)
        End If
      End If
    End If
  End Sub

  Private Sub SpreadManagement(ByRef Manager As IMediaControl, ByVal FilePath As String)
    If Not IsSTA() Then
      RaiseEvent CantOpen("Not STA")
      Return
    End If
    Dim fList() As FilterInfo = Nothing
    Dim fVal As Integer = 0
    Dim iFilters As IBaseFilter() = GetFilterList(Manager)
    If iFilters IsNot Nothing Then
      For Each objFilter As IBaseFilter In iFilters
        ReDim Preserve fList(fVal)
        objFilter.QueryFilterInfo(fList(fVal))
        fVal += 1
      Next

      If (From FilterItem As FilterInfo In fList Where (FilterItem.achName = "Video Renderer" Or FilterItem.achName.Contains("Video"))).Count > 0 Then
        Try
          obj_mpVideo = Manager
          obj_mpVidWindow = Manager
          obj_mpVidWindow.put_Visible(OABool.True)
          obj_mpVidWindow.put_WindowStyle(WindowStyle.Visible)
          obj_mpVidWindow.put_AutoShow(OABool.True)
          obj_mpVidWindow.put_Top(0)
          obj_mpVidWindow.put_Left(0)
          obj_mpVidWindow.put_Width(Me.Width)
          obj_mpVidWindow.put_Height(Me.Height)
          obj_mpVidWindow.put_Owner(Me.Handle)
          obj_mpVidWindow.put_MessageDrain(Me.Handle)
        Catch ex As Exception
          obj_mpVideo = Nothing
          obj_mpVidWindow = Nothing
        End Try
      Else
        obj_mpVideo = Nothing
        obj_mpVidWindow = Nothing
      End If
    Else
      obj_mpVideo = Nothing
      obj_mpVidWindow = Nothing
    End If
    obj_mpEvent = Manager
    obj_mpEvent.SetNotifyWindow(Me.Handle, WM_GRAPHNOTIFY, IntPtr.Zero)
    'obj_mpEvent.SetNotifyWindow(Me.Handle, WM_DVD_EVENT, IntPtr.Zero)
    Try
      obj_mpAudio = Manager
      If CFm_Mute Then
        obj_mpAudio.put_Volume(-10000)
      Else
        obj_mpAudio.put_Volume(CFm_Volume)
      End If
      obj_mpAudio.put_Balance(CFm_Balance)
    Catch ex As Exception
      obj_mpAudio = Nothing
    End Try
    CFm_Filename = FilePath
    CFm_HasAud = obj_mpAudio IsNot Nothing
    If obj_mpVideo IsNot Nothing Then
      Try
        ResetFFResize()
        Dim VidW, VidH As Long
        obj_mpVideo.GetVideoSize(VidW, VidH)
        CFm_HasVid = True
        imgLogo.Visible = False
        CFm_VideoWidth = VidW
        CFm_VideoHeight = VidH
      Catch
        CFm_HasVid = False
        imgLogo.Visible = True
        CFm_VideoWidth = 0
        CFm_VideoHeight = 0
      End Try
    Else
      CFm_HasVid = False
      imgLogo.Visible = True
      CFm_VideoWidth = 0
      CFm_VideoHeight = 0
    End If
    obj_mpPosition = Manager
    obj_mpPosition.put_Rate(1)
    Try
      If Not CFm_HasVid And Not CFm_HasAud Then RaiseEvent CantOpen("No video or audio streams detected!")
      If String.Compare(IO.Path.GetExtension(FilePath), ".mp3", True) = 0 Then
        Using cInfo As New clsHeaderLoader(FilePath)
          CFm_PadStart = 0
          CFm_PadEnd = 0
          If cInfo.cXING IsNot Nothing Then
            If cInfo.cXING.StartEncoderDelay > 0 Then CFm_PadStart = cInfo.cXING.StartEncoderDelay / 1152 * 0.026
            If cInfo.cXING.EndEncoderDelay > 0 Then CFm_PadEnd = cInfo.cXING.EndEncoderDelay / 1152 * 0.026
          End If
          If CFm_PadStart = 0 Or CFm_PadEnd = 0 Then
            CFm_PadStart = 0.013 'Default MP3 beginning pad
            CFm_PadEnd = 0.02 'An average, the best I can do for now
          End If
          Dim lCurPos As Double = Me.Position
          If lCurPos = 0 Then
            obj_mpPosition.put_CurrentPosition(CFm_PadStart)
            Dim lLastStop As Double
            obj_mpPosition.get_StopTime(lLastStop)
            obj_mpPosition.put_StopTime(lLastStop - CFm_PadEnd)
          End If
        End Using
      Else
        CFm_PadStart = 0
        CFm_PadEnd = 0
      End If
    Catch e As Exception
Erred:
      RaiseEvent MediaError(New MediaErrorEventArgs(e, "SpreadManagement"))
    End Try
  End Sub

  Private Sub FileClose()
    DownloadingState = "CLOSE"
    Try
      CloseMedia()
      imgLogo.Visible = True
      CFm_HasVid = False
      CFm_VideoWidth = 320
      CFm_VideoHeight = 240
      ResetFFResize()
      Try
        If Not String.IsNullOrEmpty(CFm_Filename) Then
          If CFm_Filename.StartsWith(IO.Path.Combine(IO.Path.GetTempPath, "seedTemp")) Then IO.File.Delete(CFm_Filename)
        End If
      Catch ex As Exception
      End Try
      CFm_Filename = vbNullString
    Catch e As Exception
      RaiseEvent MediaError(New MediaErrorEventArgs(e, "FileClose"))
    End Try
  End Sub

  Private Sub tmrJustBefore_Tick(sender As System.Object, e As System.EventArgs) Handles tmrJustBefore.Tick
    If CFm_Loop Then Return
    If Not IsSTA() Or obj_mpPosition Is Nothing Or obj_queuedControl Is Nothing Then Return
    If Me.Position >= Me.Duration - CFm_queueTime Then
      Dim fState As FilterState
      Dim qAudio As IBasicAudio = obj_queuedControl
      If CFm_Mute Then
        qAudio.put_Volume(-10000)
      Else
        qAudio.put_Volume(CFm_Volume)
      End If
      obj_queuedControl.GetState(100, fState)
      Static bWait As Integer
      If Not fState = FilterState.Running Then
        bWait = 0
        obj_queuedControl.Run()
      Else
        If Me.Position >= Me.Duration OrElse (Me.Duration - Me.Position) < 1 Then
          If bWait = 0 Then
            bWait = Environment.TickCount + (CFm_queueTime * 1000)
          ElseIf bWait <= Environment.TickCount Then
            bWait = 0
            Debug.Print("Forced Stop and Wait Reset")
            FinalizeQueue()
            RaiseEvent EndOfFile(-1)
          End If
        End If
      End If
    End If
  End Sub

  Private Function DShowError(ErrNo As Integer) As String
    Select Case ErrNo
      Case &H40258 : Return "Partial Success. Audio not rendered."
      Case &H4022D : Return "Success. Filter Graph Manager modified filter name to avoid duplication."
      Case &H40242 : Return "Some of the streams are in an unsupported format."
      Case &H40257 : Return "Partial Success. Video not rendered."
      Case &H80004004 : Return "Operation Aborted."
      Case &H80004005 : Return "Unknown Failure."
      Case &H80000003 : Return "Invalid command request."
      Case &H8007000E : Return "Insufficient memory."
      Case &H80004003 : Return "Invalid pointer."
      Case &H80040217 : Return "No combination of intermediate filters could be found to make the connection."
      Case &H80040241 : Return "The source filter for this file could not be loaded."
      Case &H80040218 : Return "No combination of filters could be found to render the stream."
      Case &H8004022F : Return "The file format is invalid."
      Case &H80040216 : Return "An object or name was not found."
      Case &H80040240 : Return "The media type of this file is not recognized."
      Case &H80040265 : Return "The format is not supported."
      Case Else : Return "Unknown error 0x" & Hex(ErrNo) & "."
    End Select
  End Function

  Private Function IsSTA() As Boolean
    Return Threading.Thread.CurrentThread.GetApartmentState = Threading.ApartmentState.STA
  End Function

  Private Sub FinalizeQueue()
    CloseMedia()
    TempFilename = CFm_queuedTempName
    tmrJustBefore.Enabled = True
    If obj_queuedControl IsNot Nothing Then SpreadManagement(obj_queuedControl, CFm_queuedFilename)
    obj_mpControl = obj_queuedControl
    Dim qAudio As IBasicAudio = obj_mpControl
    If CFm_Mute Then
      qAudio.put_Volume(-10000)
    Else
      qAudio.put_Volume(CFm_Volume)
    End If
    If CFm_HasVid Then
      SetFullScreen()
    End If
    If Not Me.State = MediaState.mPlaying Then mpPlay()
    SetNoQueue()
  End Sub

#Region "DVD"

  Private Function getDvdSeconds(dvdTime As Dvd.DvdHMSFTimeCode, dvdFPS As Double) As Double
    Return CDbl((CDbl(dvdTime.bFrames) / dvdFPS)) + (dvdTime.bSeconds) + (dvdTime.bMinutes * 60) + (dvdTime.bHours * 60 * 60)
  End Function

  Private Function getDVDTime(dvdSeconds As Double, dvdFPS As Double) As Dvd.DvdHMSFTimeCode
    Dim Seconds As Long = Int(dvdSeconds)
    If Seconds < 0 Then Return New Dvd.DvdHMSFTimeCode
    Dim Frames As Byte = dvdFPS * (dvdSeconds - Seconds)
    Dim _Hours As Byte = Seconds \ 60 \ 60
    Seconds = Seconds - (_Hours * 60 * 60)
    Dim _Minutes As Byte = Seconds \ 60
    Dim _Seconds As Byte = Seconds - (_Minutes * 60)
    Return New Dvd.DvdHMSFTimeCode With {.bHours = _Hours, .bMinutes = _Minutes, .bSeconds = _Seconds, .bFrames = Frames}
  End Function

  Public Function GetDVDAudioInfo(Index As Integer) As Dvd.DvdAudioAttributes
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim pAtr As New Dvd.DvdAudioAttributes
    obj_dvdInfo.GetAudioAttributes(Index, pAtr)
    Return pAtr
  End Function

  Public Function GetDVDCurrentAudioStream() As Integer
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim Current, Available As Integer
    obj_dvdInfo.GetCurrentAudio(Available, Current)
    Return Current
  End Function

  Public Sub SetDVDCurrentAudioStream(Index As Integer)
    If obj_dvdControl Is Nothing Then Return
    obj_dvdControl.SelectAudioStream(Index, Dvd.DvdCmdFlags.None, Nothing)
  End Sub

  Public Function GetDVDAvailableAudioStreams() As Integer
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim Current, Available As Integer
    obj_dvdInfo.GetCurrentAudio(Available, Current)
    Return Available
  End Function

  Public Function GetDVDCurrentDomain() As Dvd.DvdDomain
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim Domain As Dvd.DvdDomain
    obj_dvdInfo.GetCurrentDomain(Domain)
    Return Domain
  End Function

  Public Function GetDVDCurrentSubStream() As Integer
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim Current, Available As Integer, IsDisabled As Boolean
    obj_dvdInfo.GetCurrentSubpicture(Available, Current, IsDisabled)
    Return IIf(IsDisabled, -1, Current)
  End Function

  Public Sub SetDVDCurrentSubStream(Index As Integer)
    If obj_dvdControl Is Nothing Then Return
    Dim hr As Integer = 0
    If Index = -1 Then
      hr = obj_dvdControl.SetSubpictureState(False, Dvd.DvdCmdFlags.None, Nothing)
      Marshal.ThrowExceptionForHR(hr)
    Else
      hr = obj_dvdControl.SetSubpictureState(True, Dvd.DvdCmdFlags.None, Nothing)
      hr = obj_dvdControl.SelectSubpictureStream(Index, Dvd.DvdCmdFlags.None, Nothing)
      Marshal.ThrowExceptionForHR(hr)
    End If
  End Sub

  Public Function GetDVDAvailableSubStreams() As Integer
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim Current, Available As Integer, IsDisabled As Boolean
    obj_dvdInfo.GetCurrentSubpicture(Available, Current, IsDisabled)
    Return Available
  End Function

  Public Function GetDVDSubAttributes(Index As Integer) As Dvd.DvdSubpictureAttributes
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim pATR As Dvd.DvdSubpictureAttributes
    obj_dvdInfo.GetSubpictureAttributes(Index, pATR)
    Return pATR
  End Function

  Public Function GetDVDCurrentUOPS() As Dvd.ValidUOPFlag
    If obj_dvdInfo Is Nothing Then Return Nothing
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim pulUOPS As Dvd.ValidUOPFlag
    obj_dvdInfo.GetCurrentUOPS(pulUOPS)
    Return pulUOPS
  End Function

  Public Function GetDVDVideoInfo() As Dvd.DvdVideoAttributes
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim pATR As New Dvd.DvdVideoAttributes
    obj_dvdInfo.GetCurrentVideoAttributes(pATR)
    Return pATR
  End Function

  Public Function GetDVDDecoderCaps() As Dvd.DvdDecoderCaps
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim pCaps As Dvd.DvdDecoderCaps
    obj_dvdInfo.GetDecoderCaps(pCaps)
    Return pCaps
  End Function

  Public Function GetDVDChapters(TitleIndex As Integer) As Integer
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim Chapters As Integer
    obj_dvdInfo.GetNumberOfChapters(TitleIndex, Chapters)
    Return Chapters
  End Function

  Public Function GetDVDVolumes() As Integer
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim NumOfVolumes, Volume, NumOfTitles As Integer, pSide As Dvd.DvdDiscSide
    obj_dvdInfo.GetDVDVolumeInfo(NumOfVolumes, Volume, pSide, NumOfTitles)
    Return NumOfVolumes
  End Function

  Public Function GetDVDCurrentVolume() As Integer
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim NumOfVolumes, Volume, NumOfTitles As Integer, pSide As Dvd.DvdDiscSide
    obj_dvdInfo.GetDVDVolumeInfo(NumOfVolumes, Volume, pSide, NumOfTitles)
    Return Volume
  End Function

  Public Function GetDVDCurrentTitle() As Integer
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim Location As New Dvd.DvdPlaybackLocation2
    obj_dvdInfo.GetCurrentLocation(Location)
    Return Location.TitleNum
  End Function

  Public Function GetDVDCurrentChapter()
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim Location As New Dvd.DvdPlaybackLocation2
    obj_dvdInfo.GetCurrentLocation(Location)
    Return Location.ChapterNum
  End Function

  Public Sub SetDVDCurrentTitle(Index As Integer)
    If obj_dvdControl Is Nothing Then Return
    obj_dvdControl.PlayTitle(Index, Dvd.DvdCmdFlags.None, Nothing)
  End Sub

  Public Sub SetDVDCurrentChapter(Index As Integer)
    If obj_dvdControl Is Nothing Then Return
    obj_dvdControl.PlayChapter(Index, Dvd.DvdCmdFlags.None, Nothing)
  End Sub

  Public Function GetDVDSide() As Dvd.DvdDiscSide
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim NumOfVolumes, Volume, NumOfTitles As Integer, pSide As Dvd.DvdDiscSide
    obj_dvdInfo.GetDVDVolumeInfo(NumOfVolumes, Volume, pSide, NumOfTitles)
    Return pSide
  End Function

  Public Function GetDVDTitles() As Integer
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim NumOfVolumes, Volume, NumOfTitles As Integer, pSide As Dvd.DvdDiscSide
    obj_dvdInfo.GetDVDVolumeInfo(NumOfVolumes, Volume, pSide, NumOfTitles)
    Return NumOfTitles
  End Function

  Public Function GetDVDMenuAttributes(TitleIndex As Integer) As Dvd.DvdMenuAttributes
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim pMenu As New Dvd.DvdMenuAttributes, pTitle As New Dvd.DvdTitleAttributes
    obj_dvdInfo.GetTitleAttributes(TitleIndex, pMenu, pTitle)
    Return pMenu
  End Function

  Public Function GetDVDTitleAttributes(TitleIndex As Integer) As Dvd.DvdTitleAttributes
    If obj_dvdInfo Is Nothing Then Return Nothing
    Dim pMenu As New Dvd.DvdMenuAttributes, pTitle As New Dvd.DvdTitleAttributes
    obj_dvdInfo.GetTitleAttributes(TitleIndex, pMenu, pTitle)
    Return pTitle
  End Function

  Public Sub SetDVDCaptioning(Enabled As Boolean)
    Dim comObj As Object = Nothing
    Dim hr As Integer = obj_dvdGraph.GetDvdInterface(GetType(IAMLine21Decoder).GUID, comObj)
    DsError.ThrowExceptionForHR(hr)
    Dim obj_Captions = DirectCast(comObj, IAMLine21Decoder)
    comObj = Nothing
    obj_Captions.SetServiceState(IIf(Enabled, AMLine21CCState.On, AMLine21CCState.Off))
  End Sub

  Public Sub DVDPrevious()
    If obj_dvdControl Is Nothing Then Return
    obj_dvdControl.PlayPrevChapter(Dvd.DvdCmdFlags.None, Nothing)
  End Sub

  Public Sub DVDNext()
    If obj_dvdControl Is Nothing Then Return
    obj_dvdControl.PlayNextChapter(Dvd.DvdCmdFlags.None, Nothing)
  End Sub

  Public Sub DVDMenu(ID As Dvd.DvdMenuId)
    If obj_dvdControl Is Nothing Then Return
    obj_dvdControl.ShowMenu(ID, Dvd.DvdCmdFlags.None, Nothing)
  End Sub

  Public Sub DVDSelectButton(Direction As Dvd.DvdRelativeButton)
    If obj_dvdControl Is Nothing Then Return
    obj_dvdControl.SelectRelativeButton(Direction)
  End Sub

  Public Sub DVDActivateButton()
    If obj_dvdControl Is Nothing Then Return
    obj_dvdControl.ActivateButton()
  End Sub

  Public Sub DVDSetPreferredRatio(Ratio As Dvd.DvdPreferredDisplayMode)
    If obj_dvdControl Is Nothing Then Return
    obj_dvdControl.SelectVideoModePreference(Ratio)
  End Sub
#End Region
End Class
