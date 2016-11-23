Public Class clsMPEG
  Private MPEGHeader(32) As Boolean
  Public Sub New(lHeader As UInteger)
    MPEGHeader(0) = False
    MPEGHeader(1) = CBool(lHeader And &H80000000UI)
    MPEGHeader(2) = CBool(lHeader And &H40000000UI)
    MPEGHeader(3) = CBool(lHeader And &H20000000UI)
    MPEGHeader(4) = CBool(lHeader And &H10000000UI)
    MPEGHeader(5) = CBool(lHeader And &H8000000UI)
    MPEGHeader(6) = CBool(lHeader And &H4000000UI)
    MPEGHeader(7) = CBool(lHeader And &H2000000UI)
    MPEGHeader(8) = CBool(lHeader And &H1000000UI)
    MPEGHeader(9) = CBool(lHeader And &H800000UI)
    MPEGHeader(10) = CBool(lHeader And &H400000UI)
    MPEGHeader(11) = CBool(lHeader And &H200000UI)
    MPEGHeader(12) = CBool(lHeader And &H100000UI)
    MPEGHeader(13) = CBool(lHeader And &H80000UI)
    MPEGHeader(14) = CBool(lHeader And &H40000UI)
    MPEGHeader(15) = CBool(lHeader And &H20000UI)
    MPEGHeader(16) = CBool(lHeader And &H10000UI)
    MPEGHeader(17) = CBool(lHeader And &H8000UI)
    MPEGHeader(18) = CBool(lHeader And &H4000UI)
    MPEGHeader(19) = CBool(lHeader And &H2000UI)
    MPEGHeader(20) = CBool(lHeader And &H1000UI)
    MPEGHeader(21) = CBool(lHeader And &H800UI)
    MPEGHeader(22) = CBool(lHeader And &H400UI)
    MPEGHeader(23) = CBool(lHeader And &H200UI)
    MPEGHeader(24) = CBool(lHeader And &H100UI)
    MPEGHeader(25) = CBool(lHeader And &H80UI)
    MPEGHeader(26) = CBool(lHeader And &H40UI)
    MPEGHeader(27) = CBool(lHeader And &H20UI)
    MPEGHeader(28) = CBool(lHeader And &H10UI)
    MPEGHeader(29) = CBool(lHeader And &H8UI)
    MPEGHeader(30) = CBool(lHeader And &H4UI)
    MPEGHeader(31) = CBool(lHeader And &H2UI)
    MPEGHeader(32) = CBool(lHeader And &H1UI)
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
