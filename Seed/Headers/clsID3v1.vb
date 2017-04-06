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

  Public Property MP3File As String
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

  Private Function ToUTF8(inANSI As String) As String
    If String.IsNullOrEmpty(inANSI) Then Return Nothing
    Return System.Text.Encoding.UTF8.GetString(fileEncoding.GetBytes(inANSI))
  End Function
  Private Function FromUTF8(inUTF As String) As String
    If String.IsNullOrEmpty(inUTF) Then Return Nothing
    Return fileEncoding.GetString(System.Text.Encoding.UTF8.GetBytes(inUTF))
  End Function

  Private Sub LoadID3v1()
    Try
      If Not io.file.exists(m_sMp3File) Then Return
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
            m_sTitle = TrimNull(ToUTF8(bR.ReadChars(30)))
            m_sArtist = TrimNull(ToUTF8(bR.ReadChars(30)))
            m_sAlbum = TrimNull(ToUTF8(bR.ReadChars(30)))
            m_sYear = TrimNull(bR.ReadChars(4))
            m_sComment = bR.ReadChars(28)
            If bR.PeekChar = 0 Then
              bR.ReadByte()
              m_bTrack = bR.ReadByte
            Else
              m_sComment &= bR.ReadChars(2)
            End If
            m_sComment = TrimNull(ToUTF8(m_sComment))
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

  Public Function Save(Optional ByVal SaveAs As String = Nothing) As Boolean
    If String.IsNullOrEmpty(SaveAs) Then SaveAs = m_sMp3File
    Dim newSave As String = SaveAs & ".new_id" & (New Random).Next(0, 255).ToString("x2")
    Dim oldSave As String = SaveAs & ".old_id" & (New Random).Next(0, 255).ToString("x2")
    If IO.File.Exists(newSave) Then
      Try
        IO.File.Delete(newSave)
      Catch ex As Exception
        Return False
      End Try
    End If
    Try
      Dim bTrim As Boolean = False
      Using bR As New IO.BinaryReader(New IO.FileStream(m_sMp3File, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read), fileEncoding)
        If bR.BaseStream.Length > &H80 Then
          bR.BaseStream.Position = bR.BaseStream.Length - &H80
          If bR.ReadChars(3) = "TAG" Then bTrim = True
        End If
        bR.Close()
      End Using
      IO.File.Copy(m_sMp3File, newSave)
      If String.IsNullOrEmpty(m_sTitle) And String.IsNullOrEmpty(m_sArtist) And String.IsNullOrEmpty(m_sAlbum) And String.IsNullOrEmpty(m_sYear) And String.IsNullOrEmpty(m_sComment) And m_bGenre = &HC And m_bTrack = 0 Then
        If bTrim Then
          Using bW As New IO.BinaryWriter(New IO.FileStream(newSave, IO.FileMode.Open, IO.FileAccess.ReadWrite, IO.FileShare.ReadWrite), fileEncoding)
            bW.BaseStream.SetLength(bW.BaseStream.Length - &H80)
            bW.Close()
          End Using
        Else
          'Golden
        End If
      Else
        If bTrim Then
          Using bW As New IO.BinaryWriter(New IO.FileStream(newSave, IO.FileMode.Open, IO.FileAccess.ReadWrite, IO.FileShare.ReadWrite), fileEncoding)
            bW.BaseStream.Position = bW.BaseStream.Length - &H80
            bW.Write(fileEncoding.GetBytes("TAG"))
            bW.Write(fileEncoding.GetBytes(MakeNull(FromUTF8(m_sTitle))))
            bW.Write(fileEncoding.GetBytes(MakeNull(FromUTF8(m_sArtist))))
            bW.Write(fileEncoding.GetBytes(MakeNull(FromUTF8(m_sAlbum))))
            bW.Write(fileEncoding.GetBytes(MakeNull(m_sYear, 4)))
            If (Not String.IsNullOrEmpty(m_sComment) AndAlso FromUTF8(m_sComment).Length > 28) And m_bTrack = 0 Then
              bW.Write(fileEncoding.GetBytes(MakeNull(FromUTF8(m_sComment), 30)))
            Else
              bW.Write(fileEncoding.GetBytes(MakeNull(FromUTF8(m_sComment), 28)))
              bW.Write(CByte(0))
              bW.Write(m_bTrack)
            End If
            bW.Write(m_bGenre)
            bW.Close()
          End Using
        Else
          Using bW As New IO.BinaryWriter(New IO.FileStream(newSave, IO.FileMode.Append, IO.FileAccess.Write, IO.FileShare.ReadWrite), fileEncoding)
            bW.Write(fileEncoding.GetBytes("TAG"))
            bW.Write(fileEncoding.GetBytes(MakeNull(FromUTF8(m_sTitle))))
            bW.Write(fileEncoding.GetBytes(MakeNull(FromUTF8(m_sArtist))))
            bW.Write(fileEncoding.GetBytes(MakeNull(FromUTF8(m_sAlbum))))
            bW.Write(fileEncoding.GetBytes(MakeNull(m_sYear, 4)))
            If (Not String.IsNullOrEmpty(m_sComment) AndAlso FromUTF8(m_sComment).Length > 28) And m_bTrack = 0 Then
              bW.Write(fileEncoding.GetBytes(MakeNull(FromUTF8(m_sComment), 30)))
            Else
              bW.Write(fileEncoding.GetBytes(MakeNull(FromUTF8(m_sComment), 28)))
              bW.Write(CByte(0))
              bW.Write(m_bTrack)
            End If
            bW.Write(m_bGenre)
            bW.Close()
          End Using
        End If
      End If
      If CompareFiles(SaveAs, newSave) Then Return True
      If IO.File.Exists(SaveAs) Then
        Try
          IO.File.Move(SaveAs, oldSave)
        Catch ex As Exception
          Return False
        End Try
      End If
      Try
        IO.File.Move(newSave, SaveAs)
      Catch ex As Exception
        Return False
      End Try
      Try
        IO.File.Delete(oldSave)
      Catch ex As Exception
      End Try
      Return True
    Catch ex As Exception
      Return False
    Finally
      If IO.File.Exists(newSave) Then IO.File.Delete(newSave)
      If IO.File.Exists(oldSave) Then
        If IO.File.Exists(SaveAs) Then
          IO.File.Delete(oldSave)
        Else
          IO.File.Move(oldSave, SaveAs)
        End If
      End If
    End Try
  End Function

  Private Function CompareFiles(oldFile As String, newFile As String) As Boolean
    Dim bOld As Byte() = IO.File.ReadAllBytes(oldFile)
    Dim bNew As Byte() = IO.File.ReadAllBytes(newFile)
    If Not bOld.Length = bNew.Length Then Return False
    For I As Integer = 0 To bOld.Length - 1
      If Not bOld(I) = bNew(I) Then Return False
    Next
    Return True
  End Function

  Public Property Album As String
    Get
      Return m_sAlbum
    End Get
    Set(value As String)
      m_sAlbum = Left(value, 30)
    End Set
  End Property

  Public Property Artist As String
    Get
      Return m_sArtist
    End Get
    Set(value As String)
      m_sArtist = Left(value, 30)
    End Set
  End Property

  Public Property Comment As String
    Get
      Return m_sComment
    End Get
    Set(value As String)
      m_sComment = Left(value, 30)
    End Set
  End Property

  Public Property Genre As Byte
    Get
      Return m_bGenre
    End Get
    Set(value As Byte)
      m_bGenre = value
    End Set
  End Property

  Public Property Title As String
    Get
      Return m_sTitle
    End Get
    Set(value As String)
      m_sTitle = Left(value, 30)
    End Set
  End Property

  Public Property Track As Byte
    Get
      Return m_bTrack
    End Get
    Set(value As Byte)
      m_bTrack = value
    End Set
  End Property

  Public Property Year As String
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
        Case &H8C : Return "Contemporary Classical"
        Case &H8D : Return "Christian Rock"
        Case &H8E : Return "Merengue"
        Case &H8F : Return "Salsa"
        Case &H90 : Return "Thrash Metal"
        Case &H91 : Return "Anime"
        Case &H92 : Return "JPop"
        Case &H93 : Return "Synth Pop"
        Case &H94 : Return "Abstract"
        Case &H95 : Return "Art Rock"
        Case &H96 : Return "Baroque"
        Case &H97 : Return "Bhangra"
        Case &H98 : Return "Big Beat"
        Case &H99 : Return "Breakbeat"
        Case &H9A : Return "Chillout"
        Case &H9B : Return "Downtempo"
        Case &H9C : Return "Dub"
        Case &H9D : Return "Electronic Body"
        Case &H9E : Return "Eclectic"
        Case &H9F : Return "Electro"
        Case &HA0 : Return "Electroclash"
        Case &HA1 : Return "Emo"
        Case &HA2 : Return "Experimental"
        Case &HA3 : Return "Garage"
        Case &HA4 : Return "Global"
        Case &HA5 : Return "Intelligent Dance"
        Case &HA6 : Return "Illbient"
        Case &HA7 : Return "Industro-Goth"
        Case &HA8 : Return "Jam Band"
        Case &HA9 : Return "Krautrock"
        Case &HAA : Return "Leftfield"
        Case &HAB : Return "Lounge"
        Case &HAC : Return "Math Rock"
        Case &HAD : Return "New Romantic"
        Case &HAE : Return "Nu-Breakz"
        Case &HAF : Return "Post-Punk"
        Case &HB0 : Return "Post-Rock"
        Case &HB1 : Return "Psytrance"
        Case &HB2 : Return "Shoegaze"
        Case &HB3 : Return "Space Rock"
        Case &HB4 : Return "Trop Rock"
        Case &HB5 : Return "World Music"
        Case &HB6 : Return "Neoclassical"
        Case &HB7 : Return "Audiobook"
        Case &HB8 : Return "Audio Theatre"
        Case &HB9 : Return "Neue Deutsche Welle"
        Case &HBA : Return "Podcast"
        Case &HBB : Return "Indie Rock"
        Case &HBC : Return "G-Funk"
        Case &HBD : Return "Dubstep"
        Case &HBE : Return "Garage Rock"
        Case &HBF : Return "Psybient"
        Case &HFF : Return "None"
        Case Else : Return "Unknown (" & Hex(Genre) & ")"
      End Select
    End Get
  End Property

  Private Function MakeNull(sBuf As String, Optional ByVal lLen As Integer = 30) As String
    If String.IsNullOrEmpty(sBuf) Then Return StrDup(lLen, ChrW(0))
    sBuf = sBuf.Trim
    If lLen < sBuf.Length Then Return sBuf.Substring(0, lLen)
    Return sBuf & StrDup(lLen - sBuf.Length, ChrW(0))
  End Function

  Private Function TrimNull(sBuf As String) As String
    If InStr(sBuf, vbNullChar) Then sBuf = Left(sBuf, InStr(sBuf, vbNullChar) - 1)
    Return Trim(sBuf)
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
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region
End Class
