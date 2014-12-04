Imports System.Drawing
Imports System.Runtime.InteropServices
<Guid("F0DAFCE8-5D35-4609-B0BE-48CBF0FC21DB"), ClassInterface(ClassInterfaceType.None), ComVisible(True), ProgId("GreenThumb.ThumbnailProvider")>
Public Class ThumbnailProvider
  Implements IThumbnailProvider, IInitializeWithFile, IInitializeWithItem
  Private sPath As String

  Public Sub Initialize_Item(ByVal psi As IShellItem, ByVal grfMode As Integer) Implements IInitializeWithItem.Initialize
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
  End Sub

  Public Sub Initialize_File(ByVal pszFilePath As String, ByVal grfMode As UInteger) Implements IInitializeWithFile.Initialize
    Try
      sPath = pszFilePath
    Catch ex As Exception
      sPath = Nothing
    End Try
  End Sub

  Public Sub GetThumbnail(ByVal cx As UInteger, ByRef hBitmap As System.IntPtr, ByRef bitmapType As WTS_ALPHATYPE) Implements IThumbnailProvider.GetThumbnail
    Try
      If String.IsNullOrEmpty(sPath) Then
        hBitmap = 0
        bitmapType = WTS_ALPHATYPE.WTSAT_UNKNOWN
      Else
        Using bRet As Bitmap = LoadImage(sPath, cx)
          If bRet Is Nothing Then
            hBitmap = 0
            bitmapType = WTS_ALPHATYPE.WTSAT_UNKNOWN
          End If
          'Dim bImg As Bitmap = bRet.Clone
          hBitmap = bRet.GetHbitmap
          bitmapType = WTS_ALPHATYPE.WTSAT_ARGB
        End Using
      End If
    Catch ex As Exception
      hBitmap = 0
      bitmapType = WTS_ALPHATYPE.WTSAT_UNKNOWN
    End Try
    GC.Collect()
  End Sub

  Private Function ScaleImage(bFile As Bitmap, cx As UInteger) As Drawing.Bitmap
    Try
      Dim lWidth, lHeight As UInteger
      If bFile.Width > bFile.Height Then
        If cx > bFile.Width Then cx = bFile.Width
        lWidth = cx
        lHeight = (bFile.Height / bFile.Width) * cx
      ElseIf bFile.Width < bFile.Height Then
        If cx > bFile.Height Then cx = bFile.Height
        lWidth = (bFile.Width / bFile.Height) * cx
        lHeight = cx
      Else
        If cx > bFile.Width Then cx = bFile.Width
        lWidth = cx
        lHeight = cx
      End If
      Dim bThumb As Bitmap = New Bitmap(lWidth, lHeight, Imaging.PixelFormat.Format32bppArgb)
      Using g As Graphics = Graphics.FromImage(bThumb)
        g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
        g.DrawImage(bFile, New Rectangle(0, 0, lWidth, lHeight), 0, 0, bFile.Width, bFile.Height, GraphicsUnit.Pixel)
        bFile.Dispose()
        bFile = Nothing
      End Using
      Return bThumb.Clone
    Catch ex As Exception
      Return Nothing
    End Try
  End Function

  Private Function LoadImage(Path As String, Optional cx As UInteger = 0) As Drawing.Bitmap
    Try
      Dim tmpImg As Image = Nothing
      Using mpTemp As New Seed.ctlSeed
        tmpImg = mpTemp.GetFileThumbnail(Path)
        mpTemp.FileName = Nothing
      End Using
      If tmpImg Is Nothing Then
        Return Nothing
      End If
      If cx > 0 Then
        Return ScaleImage(tmpImg, cx).Clone
      Else
        Return tmpImg.Clone
      End If
    Catch ex As Exception
      Return Nothing
    End Try
  End Function
End Class