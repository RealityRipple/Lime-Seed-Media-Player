Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Diagnostics

Friend Class clsGlass
  Private Const DTT_COMPOSITED As Integer = CInt((1 << 13))
  Private Const DTT_GLOWSIZE As Integer = CInt((1 << 11))

  'Text format consts
  Private Const DT_SINGLELINE As Integer = &H20
  Private Const DT_CENTER As Integer = &H1
  Private Const DT_VCENTER As Integer = &H4
  Private Const DT_NOPREFIX As Integer = &H800

  'Const for BitBlt
  Private Const SRCCOPY As Integer = &HCC0020

  'Consts for CreateDIBSection
  Private Const BI_RGB As Integer = 0
  Private Const DIB_RGB_COLORS As Integer = 0
  'color table in RGBs
  Private Structure MARGINS
    Public m_Left As Integer
    Public m_Right As Integer
    Public m_Top As Integer
    Public m_Bottom As Integer
  End Structure

  Private Structure POINTAPI
    Public x As Integer
    Public y As Integer
  End Structure

  Private Structure DTTOPTS
    Public dwSize As UInteger
    Public dwFlags As UInteger
    Public crText As UInteger
    Public crBorder As UInteger
    Public crShadow As UInteger
    Public iTextShadowType As Integer
    Public ptShadowOffset As POINTAPI
    Public iBorderSize As Integer
    Public iFontPropId As Integer
    Public iColorPropId As Integer
    Public iStateId As Integer
    Public fApplyOverlay As Integer
    Public iGlowSize As Integer
    Public pfnDrawTextCallback As IntPtr
    Public lParam As Integer
  End Structure

  Private Structure RECT
    Public left As Integer
    Public top As Integer
    Public right As Integer
    Public bottom As Integer
  End Structure

  Private Structure BITMAPINFOHEADER
    Public biSize As Integer
    Public biWidth As Integer
    Public biHeight As Integer
    Public biPlanes As Short
    Public biBitCount As Short
    Public biCompression As Integer
    Public biSizeImage As Integer
    Public biXPelsPerMeter As Integer
    Public biYPelsPerMeter As Integer
    Public biClrUsed As Integer
    Public biClrImportant As Integer
  End Structure

  Private Structure RGBQUAD
    Public rgbBlue As Byte
    Public rgbGreen As Byte
    Public rgbRed As Byte
    Public rgbReserved As Byte
  End Structure

  Private Structure BITMAPINFO
    Public bmiHeader As BITMAPINFOHEADER
    Public bmiColors As RGBQUAD
  End Structure


  'API declares
  <DllImport("dwmapi.dll")>
  Private Shared Sub DwmIsCompositionEnabled(ByRef enabledptr As Integer)
  End Sub
  <DllImport("dwmapi.dll")>
  Private Shared Sub DwmExtendFrameIntoClientArea(ByVal hWnd As IntPtr, ByRef margin As MARGINS)
  End Sub

  Private Declare Auto Function GetDC Lib "user32.dll" (ByVal hdc As IntPtr) As IntPtr
  Private Declare Auto Function SaveDC Lib "gdi32.dll" (ByVal hdc As IntPtr) As Integer
  Private Declare Auto Function ReleaseDC Lib "user32.dll" (ByVal hdc As IntPtr, ByVal state As Integer) As Integer
  Private Declare Auto Function CreateCompatibleDC Lib "gdi32.dll" (ByVal hDC As IntPtr) As IntPtr
  <DllImport("gdi32.dll", ExactSpelling:=True)>
  Private Shared Function SelectObject(ByVal hDC As IntPtr, ByVal hObject As IntPtr) As IntPtr
  End Function
  Private Declare Auto Function DeleteObject Lib "gdi32.dll" (ByVal hObject As IntPtr) As Boolean
  Private Declare Auto Function DeleteDC Lib "gdi32.dll" (ByVal hdc As IntPtr) As Boolean
  <DllImport("gdi32.dll")>
  Private Shared Function BitBlt(ByVal hdc As IntPtr, ByVal nXDest As Integer, ByVal nYDest As Integer, ByVal nWidth As Integer, ByVal nHeight As Integer, ByVal hdcSrc As IntPtr, ByVal nXSrc As Integer, ByVal nYSrc As Integer, ByVal dwRop As UInteger) As Boolean
  End Function

  Private Declare Unicode Function DrawThemeTextEx Lib "UxTheme.dll" (ByVal hTheme As IntPtr, ByVal hdc As IntPtr, ByVal iPartId As Integer, ByVal iStateId As Integer, ByVal text As String, ByVal iCharCount As Integer, ByVal dwFlags As Integer, ByRef pRect As RECT, ByRef pOptions As DTTOPTS) As Integer
  Private Declare Auto Function DrawThemeText Lib "UxTheme.dll" (ByVal hTheme As IntPtr, ByVal hdc As IntPtr, ByVal iPartId As Integer, ByVal iStateId As Integer, ByVal text As String, ByVal iCharCount As Integer, ByVal dwFlags1 As Integer, ByVal dwFlags2 As Integer, ByRef pRect As RECT) As Integer
  Private Declare Auto Function CreateDIBSection Lib "gdi32.dll" (ByVal hdc As IntPtr, ByRef pbmi As BITMAPINFO, ByVal iUsage As UInteger, ByVal ppvBits As Integer, ByVal hSection As IntPtr, ByVal dwOffset As UInteger) As IntPtr

  Public Shared Sub SetGlass(Form As Form, left As Integer, top As Integer, right As Integer, bottom As Integer)
    Dim mg As New MARGINS()
    mg.m_Bottom = bottom
    mg.m_Left = left
    mg.m_Right = right
    mg.m_Top = top
    If IsCompositionEnabled() Then DwmExtendFrameIntoClientArea(Form.Handle, mg)
  End Sub

  Public Shared Function IsCompositionEnabled() As Boolean
    If Environment.OSVersion.Version.Major < 6 Then Return False
    Dim compositionEnabled As Integer = 0
    DwmIsCompositionEnabled(compositionEnabled)
    Return CBool(compositionEnabled > 0)
  End Function

  Public Shared Sub DrawTextOnGlass(ByVal hwnd As IntPtr, ByVal text As String, ByVal Font As Font, ByVal ctlRct As Rectangle, ByVal Margin As Integer, ByVal iGlowSize As Integer)
    If IsCompositionEnabled() Then
      Dim TextRect As New RECT
      TextRect.top = Margin
      TextRect.left = Margin
      TextRect.bottom = ctlRct.Bottom - ctlRct.Top - Margin
      TextRect.right = ctlRct.Right - ctlRct.Left - Margin
      Dim destdc As IntPtr = GetDC(hwnd)
      'hwnd must be the handle of form, not control
      Dim Memdc As IntPtr = CreateCompatibleDC(destdc)
      ' Set up a memory DC where we'll draw the text.
      Dim bitmap As IntPtr
      Dim bitmapOld As IntPtr = IntPtr.Zero
      Dim logfnotOld As IntPtr
      Dim uFormat As Integer = DT_SINGLELINE Or DT_NOPREFIX Or DT_VCENTER 'Or DT_CENTER 
      'text format
      Dim dib As New BITMAPINFO()
      dib.bmiHeader.biHeight = -ctlRct.Height
      ' negative because DrawThemeTextEx() uses a top-down DIB
      dib.bmiHeader.biWidth = ctlRct.Width
      dib.bmiHeader.biPlanes = 1
      dib.bmiHeader.biSize = Marshal.SizeOf(GetType(BITMAPINFOHEADER))
      dib.bmiHeader.biBitCount = 32
      dib.bmiHeader.biCompression = BI_RGB
      If Not (SaveDC(Memdc) = 0) Then
        bitmap = CreateDIBSection(Memdc, dib, DIB_RGB_COLORS, 0, IntPtr.Zero, 0)
        ' Create a 32-bit bmp for use in offscreen drawing when glass is on
        If Not (bitmap = IntPtr.Zero) Then
          bitmapOld = SelectObject(Memdc, bitmap)
          Dim hFont As IntPtr = Font.ToHfont
          logfnotOld = SelectObject(Memdc, hFont)
          Try
            Dim renderer As New System.Windows.Forms.VisualStyles.VisualStyleRenderer(System.Windows.Forms.VisualStyles.VisualStyleElement.Window.Caption.Active)
            Dim dttOpts As New DTTOPTS()
            dttOpts.dwSize = CUInt(Marshal.SizeOf(GetType(DTTOPTS)))
            dttOpts.dwFlags = DTT_COMPOSITED Or DTT_GLOWSIZE
            dttOpts.iGlowSize = iGlowSize
            DrawThemeTextEx(renderer.Handle, Memdc, 0, 0, text, -1, uFormat, TextRect, dttOpts)
            BitBlt(destdc, ctlRct.Left, ctlRct.Top, ctlRct.Width, ctlRct.Height, Memdc, 0, 0, SRCCOPY)
          Catch e As Exception
            Trace.WriteLine(e.Message)
          Finally
            SelectObject(Memdc, bitmapOld)
            SelectObject(Memdc, logfnotOld)
            ReleaseDC(Memdc, -1)
            DeleteDC(Memdc)
            ReleaseDC(destdc, -1)
            DeleteDC(destdc)
            DeleteObject(logfnotOld)
            DeleteObject(bitmapOld)
            DeleteObject(hFont)
            DeleteObject(bitmap)
          End Try
        Else
          DeleteObject(bitmap)
        End If
      Else
        ReleaseDC(Memdc, -1)
        DeleteDC(Memdc)
        ReleaseDC(destdc, -1)
        DeleteDC(destdc)
      End If
    End If
  End Sub
End Class