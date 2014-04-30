Public Class Form1

  Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

  End Sub


  Private Function ReadStore() As Bitmap()
    Dim fStore As String = IO.Path.GetTempPath & "thumbSTORE.db"
    If Not My.Computer.FileSystem.FileExists(fStore) Then Return Nothing

    
    Using sFile As New IO.FileStream(fStore, IO.FileMode.Open)
      Using sRead As New IO.BinaryReader(sFile)
        Dim iHeaderSize As UInteger = sRead.ReadUInt32
        If iHeaderSize > 0 Then
          Dim bItems As New Collections.Generic.List(Of Bitmap)
          For I As UInteger = 0 To iHeaderSize - 1
            Dim fileHeader As String
            fileHeader = sRead.ReadString

            Dim fileSize As UInteger = sRead.ReadUInt32
            Dim fileData(fileSize - 1) As Byte
            fileData = sRead.ReadBytes(fileSize)
            bItems.Add(New Bitmap(New IO.MemoryStream(fileData)))

          Next
          sRead.Close()

          Return bItems.ToArray
        Else
          sRead.Close()
          Return Nothing
        End If

      End Using
    End Using
  End Function

  Private Sub WriteStore(image As Bitmap, filename As String, mtime As Runtime.InteropServices.ComTypes.FILETIME, ctime As Runtime.InteropServices.ComTypes.FILETIME, fSize As Long)
    Dim fStore As String = IO.Path.GetTempPath & "thumbSTORE.db"
    Dim cMD5 As New Security.Cryptography.MD5Cng
    Dim fID As String = IO.Path.GetFileNameWithoutExtension(Replace(filename, " ", "_")) & Hex(mtime.dwHighDateTime) & Hex(mtime.dwLowDateTime) & "_" & Hex(ctime.dwHighDateTime) & Hex(ctime.dwLowDateTime) & "_" & fSize
    If My.Computer.FileSystem.FileExists(fStore) AndAlso My.Computer.FileSystem.GetFileInfo(fStore).CreationTime.Subtract(Now).TotalHours < 24 Then
      Using sFile As New IO.FileStream(fStore, IO.FileMode.OpenOrCreate, IO.FileAccess.ReadWrite, IO.FileShare.Read)
        Using sRead As New IO.BinaryReader(sFile)
          Dim iHeaderSize As UInteger = sRead.ReadUInt32
          If iHeaderSize > 0 Then
            For I As UInteger = 0 To iHeaderSize - 1
              Dim fileHeader As String
              fileHeader = sRead.ReadString
              If fileHeader = fID Then
                sRead.Close()
                Exit Sub
              End If
              Dim fileSize As UInteger = sRead.ReadUInt32
              Dim fileData(fileSize - 1) As Byte
              fileData = sRead.ReadBytes(fileSize)
            Next
            Using sWrite As New IO.BinaryWriter(sFile)
              sWrite.Seek(0, IO.SeekOrigin.Begin)
              sWrite.Write(iHeaderSize + 1UI)
              sWrite.Seek(0, IO.SeekOrigin.End)
              'sWrite.Seek(1, IO.SeekOrigin.Current)
              sWrite.Write(fID)
              Dim bData() As Byte = BtBA(image)
              Dim iLen As UInteger = bData.LongLength
              sWrite.Write(iLen)
              sWrite.Write(bData)
              sWrite.Close()
            End Using
          Else
            Using sWrite As New IO.BinaryWriter(sFile)
              sWrite.Seek(0, IO.SeekOrigin.Begin)
              sWrite.Write(1UI)
              sWrite.Write(fID)
              Dim bData() As Byte = BtBA(image)
              Dim iLen As UInteger = bData.LongLength
              sWrite.Write(iLen)
              sWrite.Write(bData)
              sWrite.Close()
            End Using
          End If
        End Using

      End Using
    Else
      If My.Computer.FileSystem.FileExists(fStore) Then My.Computer.FileSystem.DeleteFile(fStore)
      Using sFile As New IO.FileStream(fStore, IO.FileMode.OpenOrCreate, IO.FileAccess.ReadWrite, IO.FileShare.Read)
        Using sWrite As New IO.BinaryWriter(sFile)
          sWrite.Write(1UI)
          sWrite.Write(fID)
          Dim bData() As Byte = BtBA(image)
          Dim iLen As UInteger = bData.LongLength
          sWrite.Write(iLen)
          sWrite.Write(bData)
          sWrite.Close()
        End Using
      End Using
    End If
  End Sub

  Private Function BtBA(img As Bitmap) As Byte()
    Dim bData() As Byte
    Using ms As New IO.MemoryStream
      img.Save(ms, Imaging.ImageFormat.Png)
      bData = ms.ToArray
    End Using
    Return bData
  End Function

  Dim bItems() As Bitmap
  Dim iIndex As Integer
  Private Sub cmdReload_Click(sender As System.Object, e As System.EventArgs) Handles cmdReload.Click
    bItems = ReadStore()
    If iIndex > bItems.Count - 1 Then iIndex = 0
    pctPreview.Image = bItems(iIndex)
    Me.Text = iIndex + 1 & " / " & bItems.Count
  End Sub

  Private Sub cmdBack_Click(sender As System.Object, e As System.EventArgs) Handles cmdBack.Click
    iIndex -= 1
    If iIndex < 0 Then iIndex = bItems.Count - 1
    pctPreview.Image = bItems(iIndex)
    Me.Text = iIndex + 1 & " / " & bItems.Count
  End Sub

  Private Sub cmdNext_Click(sender As System.Object, e As System.EventArgs) Handles cmdNext.Click
    iIndex += 1
    If iIndex >= bItems.Count Then iIndex = 0
    pctPreview.Image = bItems(iIndex)
    Me.Text = iIndex + 1 & " / " & bItems.Count
  End Sub

  Private Sub pctPreview_Click(sender As System.Object, e As System.EventArgs) Handles pctPreview.Click
    Using cdlOpen As New OpenFileDialog
      cdlOpen.Filter = "PNGs|*.png"
      cdlOpen.Title = "Add Thumbnail to DataBase..."
      If cdlOpen.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
        Dim sFile As String = cdlOpen.FileName
        Dim bFile As Bitmap = Bitmap.FromFile(sFile)
        Dim fInfo = My.Computer.FileSystem.GetFileInfo(sFile)
        Dim mtime, ctime As Runtime.InteropServices.ComTypes.FILETIME
        mtime.dwLowDateTime = CInt(fInfo.LastWriteTime.ToFileTime And &HFFFFFFFFL)
        mtime.dwHighDateTime = CInt((fInfo.LastWriteTime.ToFileTime And &HFFFFFFFF00000000L) >> 8)
        ctime.dwLowDateTime = CInt(fInfo.CreationTime.ToFileTime And &HFFFFFFFFL)
        ctime.dwHighDateTime = CInt((fInfo.CreationTime.ToFileTime And &HFFFFFFFF00000000L) >> 8)


        WriteStore(bFile, IO.Path.GetFileName(sFile), mtime, ctime, fInfo.Length)
        cmdReload.PerformClick()
      End If
    End Using
  End Sub
End Class
