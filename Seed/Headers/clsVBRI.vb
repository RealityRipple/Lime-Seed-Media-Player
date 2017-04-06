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
  Public Sub New(bFrame As Byte(), Optional ByVal Start As Integer = 0)
    Dim I As Integer
    Dim lPos As Long
    lPos = Start + 4 + 32
    vHeader.HeaderID = GetString(bFrame, lPos, 4)
    If vHeader.HeaderID <> "VBRI" Then
      vHeader.HeaderID = vbNullString
      Return
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
