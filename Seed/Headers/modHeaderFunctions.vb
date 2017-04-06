Friend Module modHeaderFunctions
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
  Public Function GetWORD(bIn As Byte(), Optional ByVal lStart As Long = 0) As UInt16
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
  Public Function GetString(bIn As Byte(), ByVal lStart As Long, ByVal lLength As Long) As String
    If bIn.Length < lStart + lLength Then Return String.Empty
    Return System.Text.Encoding.GetEncoding(LATIN_1).GetString(bIn, lStart, lLength)
  End Function
  'Public Function GetBytePos(bIn As Byte(), ByVal bFind As Byte, Optional ByVal lStart As Integer = 0) As Integer
  '  If lStart = -1 Then lStart = 0
  '  For I As Integer = lStart To bIn.Length - 1
  '    If bIn(I) = bFind Then Return I
  '  Next
  '  Return -1
  'End Function

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
