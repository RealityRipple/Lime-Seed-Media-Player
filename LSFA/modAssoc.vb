Module modAssoc
  Private Declare Sub SHChangeNotify Lib "shell32.dll" (ByVal wEventId As Integer, ByVal uFlags As Integer, dwItem1 As IntPtr, dwItem2 As IntPtr)
  Private Const SHCNE_ASSOCCHANGED As Long = &H8000000
  Private Const SHCNF_IDLIST As Long = &H0&
  Public Enum FileTypes
    File_MPC
    File_AC3
    File_AIF   'AIFC AIFF
    File_ASF    'ASX
    File_AU      'SND
    File_MIDI  'MID RMI
    File_APE
    File_FLAC
    File_OFR
    File_TTA
    File_WAV
    File_AVI
    File_DIVX
    File_IVF
    File_MKV
    File_MKA
    File_FLV
    File_SPL
    File_SWF
    File_CDA  'AudioCD
    File_DAT
    File_VOB  'DVD
    File_MPE
    File_MPG 'MPEG
    File_M1V
    File_MP2
    File_M2V  'MP2V MPV2
    File_MP3
    File_MPA
    File_AAC
    File_M2TS
    File_M4A
    File_M4P
    File_M4V  'MP4
    File_OGG
    File_OGM
    File_MOV  'QT
    File_DV   'DIF
    File_3GP  '3GPP
    File_3G2  '3GP2
    File_RA   'RAM
    File_RM   'RMM
    File_RV
    File_VFW
    File_WMP
    File_WM   'WMX
    File_WMA  'WAX
    File_WMV  'WVX
    File_M3U
    File_PLS
    File_LLPL
    File_Directory
  End Enum
  Const EXEName As String = "LSMP.exe"
  Private Thumbnails As Boolean

  Public Sub Main()
    If String.IsNullOrEmpty(Command) Then
      MsgBox("Please change association settings within Lime Seed or your Windows Association control panel item.", MsgBoxStyle.Exclamation, "Lime Seed File Association")
    Else
      If InStr(Command, " THUMB ") > 0 Then
        Thumbnails = True
        Shell("C:\Windows\Microsoft.NET\Framework" & IIf(Environment.Is64BitOperatingSystem, "64", String.Empty) & "\v4.0.30319\RegASM.exe """ & Application.StartupPath & "\GreenThumb.dll"" /codebase", AppWinStyle.Hide, True)
        For Each CRSub In My.Computer.Registry.ClassesRoot.GetSubKeyNames
          If String.Compare(CRSub, "CLSID", True) = 0 Then
            For Each IDSub In My.Computer.Registry.ClassesRoot.OpenSubKey(CRSub).GetSubKeyNames
              If String.Compare(IDSub, "{F0DAFCE8-5D35-4609-B0BE-48CBF0FC21DB}", True) = 0 Then
                My.Computer.Registry.ClassesRoot.OpenSubKey(CRSub, True).OpenSubKey(IDSub, True).SetValue("DisableProcessIsolation", 1, Microsoft.Win32.RegistryValueKind.DWord)
                Exit For
              End If
            Next
            Exit For
          End If
        Next
      Else
        Thumbnails = False
        For Each CRSub In My.Computer.Registry.ClassesRoot.GetSubKeyNames
          If String.Compare(CRSub, "CLSID", True) = 0 Then
            For Each IDSub In My.Computer.Registry.ClassesRoot.OpenSubKey(CRSub).GetSubKeyNames
              If String.Compare(IDSub, "{F0DAFCE8-5D35-4609-B0BE-48CBF0FC21DB}", True) = 0 Then
                For Each guVal In My.Computer.Registry.ClassesRoot.OpenSubKey(CRSub).OpenSubKey(IDSub).GetValueNames
                  If String.Compare(guVal, "DisableProcessIsolation") = 0 Then
                    My.Computer.Registry.ClassesRoot.OpenSubKey(CRSub, True).OpenSubKey(IDSub, True).DeleteValue(guVal)
                    Exit For
                  End If
                Next
                Exit For
              End If
            Next
            Exit For
          End If
        Next
        Shell("C:\Windows\Microsoft.NET\Framework" & IIf(Environment.Is64BitOperatingSystem, "64", String.Empty) & "\v4.0.30319\RegASM.exe """ & Application.StartupPath & "\GreenThumb.dll"" /u", AppWinStyle.Hide, True)
      End If
      If InStr(Command, " MPC ") > 0 Then
        AddType(FileTypes.File_MPC)
      Else
        RemType(FileTypes.File_MPC)
      End If
      If InStr(Command, " AC3 ") > 0 Then
        AddType(FileTypes.File_AC3)
      Else
        RemType(FileTypes.File_AC3)
      End If
      If InStr(Command, " AIF ") > 0 Then
        AddType(FileTypes.File_AIF)
      Else
        RemType(FileTypes.File_AIF)
      End If
      If InStr(Command, " ASF ") > 0 Then
        AddType(FileTypes.File_ASF)
      Else
        RemType(FileTypes.File_ASF)
      End If
      If InStr(Command, " AU ") > 0 Then
        AddType(FileTypes.File_AU)
      Else
        RemType(FileTypes.File_AU)
      End If
      If InStr(Command, " MID ") > 0 Then
        AddType(FileTypes.File_MIDI)
      Else
        RemType(FileTypes.File_MIDI)
      End If
      If InStr(Command, " APE ") > 0 Then
        AddType(FileTypes.File_APE)
      Else
        RemType(FileTypes.File_APE)
      End If
      If InStr(Command, " FLAC ") > 0 Then
        AddType(FileTypes.File_FLAC)
      Else
        RemType(FileTypes.File_FLAC)
      End If
      If InStr(Command, " OFR ") > 0 Then
        AddType(FileTypes.File_OFR)
      Else
        RemType(FileTypes.File_OFR)
      End If
      If InStr(Command, " TTA ") > 0 Then
        AddType(FileTypes.File_TTA)
      Else
        RemType(FileTypes.File_TTA)
      End If
      If InStr(Command, " WAV ") > 0 Then
        AddType(FileTypes.File_WAV)
      Else
        RemType(FileTypes.File_WAV)
      End If
      If InStr(Command, " AVI ") > 0 Then
        AddType(FileTypes.File_AVI)
      Else
        RemType(FileTypes.File_AVI)
      End If
      If InStr(Command, " DIVX ") > 0 Then
        AddType(FileTypes.File_DIVX)
      Else
        RemType(FileTypes.File_DIVX)
      End If
      If InStr(Command, " IVF ") > 0 Then
        AddType(FileTypes.File_IVF)
      Else
        RemType(FileTypes.File_IVF)
      End If
      If InStr(Command, " MKV ") > 0 Then
        AddType(FileTypes.File_MKV)
      Else
        RemType(FileTypes.File_MKV)
      End If
      If InStr(Command, " MKA ") > 0 Then
        AddType(FileTypes.File_MKA)
      Else
        RemType(FileTypes.File_MKA)
      End If
      If InStr(Command, " FLV ") > 0 Then
        AddType(FileTypes.File_FLV)
      Else
        RemType(FileTypes.File_FLV)
      End If
      If InStr(Command, " SPL ") > 0 Then
        AddType(FileTypes.File_SPL)
      Else
        RemType(FileTypes.File_SPL)
      End If
      If InStr(Command, " SWF ") > 0 Then
        AddType(FileTypes.File_SWF)
      Else
        RemType(FileTypes.File_SWF)
      End If
      If InStr(Command, " CDA ") > 0 Then
        AddType(FileTypes.File_CDA)
      Else
        RemType(FileTypes.File_CDA)
      End If
      If InStr(Command, " VCD ") > 0 Then
        AddType(FileTypes.File_DAT)
      Else
        RemType(FileTypes.File_DAT)
      End If
      If InStr(Command, " DVD ") > 0 Then
        AddType(FileTypes.File_VOB)
      Else
        RemType(FileTypes.File_VOB)
      End If
      If InStr(Command, " MPE ") > 0 Then
        AddType(FileTypes.File_MPE)
      Else
        RemType(FileTypes.File_MPE)
      End If
      If InStr(Command, " MPG ") > 0 Then
        AddType(FileTypes.File_MPG)
      Else
        RemType(FileTypes.File_MPG)
      End If
      If InStr(Command, " M1V ") > 0 Then
        AddType(FileTypes.File_M1V)
      Else
        RemType(FileTypes.File_M1V)
      End If
      If InStr(Command, " MP2 ") > 0 Then
        AddType(FileTypes.File_MP2)
      Else
        RemType(FileTypes.File_MP2)
      End If
      If InStr(Command, " M2V ") > 0 Then
        AddType(FileTypes.File_M2V)
      Else
        RemType(FileTypes.File_M2V)
      End If
      If InStr(Command, " MP3 ") > 0 Then
        AddType(FileTypes.File_MP3)
      Else
        RemType(FileTypes.File_MP3)
      End If
      If InStr(Command, " MPA ") > 0 Then
        AddType(FileTypes.File_MPA)
      Else
        RemType(FileTypes.File_MPA)
      End If
      If InStr(Command, " AAC ") > 0 Then
        AddType(FileTypes.File_AAC)
      Else
        RemType(FileTypes.File_AAC)
      End If
      If InStr(Command, " M2TS ") > 0 Then
        AddType(FileTypes.File_M2TS)
      Else
        RemType(FileTypes.File_M2TS)
      End If
      If InStr(Command, " M4A ") > 0 Then
        AddType(FileTypes.File_M4A)
      Else
        RemType(FileTypes.File_M4A)
      End If
      If InStr(Command, " M4P ") > 0 Then
        AddType(FileTypes.File_M4P)
      Else
        RemType(FileTypes.File_M4P)
      End If
      If InStr(Command, " M4V ") > 0 Then
        AddType(FileTypes.File_M4V)
      Else
        RemType(FileTypes.File_M4V)
      End If
      If InStr(Command, " OGG ") > 0 Then
        AddType(FileTypes.File_OGG)
      Else
        RemType(FileTypes.File_OGG)
      End If
      If InStr(Command, " OGM ") > 0 Then
        AddType(FileTypes.File_OGM)
      Else
        RemType(FileTypes.File_OGM)
      End If
      If InStr(Command, " MOV ") > 0 Then
        AddType(FileTypes.File_MOV)
      Else
        RemType(FileTypes.File_MOV)
      End If
      If InStr(Command, " DV ") > 0 Then
        AddType(FileTypes.File_DV)
      Else
        RemType(FileTypes.File_DV)
      End If
      If InStr(Command, " 3GP ") > 0 Then
        AddType(FileTypes.File_3GP)
      Else
        RemType(FileTypes.File_3GP)
      End If
      If InStr(Command, " 3G2 ") > 0 Then
        AddType(FileTypes.File_3G2)
      Else
        RemType(FileTypes.File_3G2)
      End If
      If InStr(Command, " RA ") > 0 Then
        AddType(FileTypes.File_RA)
      Else
        RemType(FileTypes.File_RA)
      End If
      If InStr(Command, " RM ") > 0 Then
        AddType(FileTypes.File_RM)
      Else
        RemType(FileTypes.File_RM)
      End If
      If InStr(Command, " RV ") > 0 Then
        AddType(FileTypes.File_RV)
      Else
        RemType(FileTypes.File_RV)
      End If
      If InStr(Command, " VFW ") > 0 Then
        AddType(FileTypes.File_VFW)
      Else
        RemType(FileTypes.File_VFW)
      End If
      If InStr(Command, " WMP ") > 0 Then
        AddType(FileTypes.File_WMP)
      Else
        RemType(FileTypes.File_WMP)
      End If
      If InStr(Command, " WM ") > 0 Then
        AddType(FileTypes.File_WM)
      Else
        RemType(FileTypes.File_WM)
      End If
      If InStr(Command, " WMA ") > 0 Then
        AddType(FileTypes.File_WMA)
      Else
        RemType(FileTypes.File_WMA)
      End If
      If InStr(Command, " WMV ") > 0 Then
        AddType(FileTypes.File_WMV)
      Else
        RemType(FileTypes.File_WMV)
      End If
      If InStr(Command, " M3U ") > 0 Then
        AddType(FileTypes.File_M3U)
      Else
        RemType(FileTypes.File_M3U)
      End If
      If InStr(Command, " PLS ") > 0 Then
        AddType(FileTypes.File_PLS)
      Else
        RemType(FileTypes.File_PLS)
      End If
      If InStr(Command, " LLPL ") > 0 Then
        AddType(FileTypes.File_LLPL)
      Else
        RemType(FileTypes.File_LLPL)
      End If
      If InStr(Command, " DIR ") > 0 Then
        AddType(FileTypes.File_Directory)
      Else
        RemType(FileTypes.File_Directory)
      End If
      ConfirmFileTypes()
    End If
  End Sub

  Private Sub Add_Associate(ByVal sType As String, ByVal bIcon As Byte, ByVal sDescr As String, ParamArray sExt() As String)
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).SetValue(String.Empty, sDescr)
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).CreateSubKey("shell").SetValue(String.Empty, "Play")
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).CreateSubKey("shell").CreateSubKey("Play").SetValue(String.Empty, "&Play")
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).CreateSubKey("shell").CreateSubKey("Play").SetValue(String.Empty, "&Play")
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).CreateSubKey("shell").CreateSubKey("Play").CreateSubKey("command").SetValue(String.Empty, """" & Application.StartupPath & "\" & EXEName & """ ""%1""")
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).CreateSubKey("DefaultIcon").SetValue(String.Empty, Application.StartupPath & "\" & EXEName & "," & bIcon.ToString.Trim)
    Select Case sType.ToLower
      Case "avi", "divx", "ivf", "mkv", "flv", "spl", "swf", "mpe", "mpg", "mpeg", "m1v", "m2v", "mp2v", "mpv2", "m2ts", "mp4", "m4v", "ogm", "mov", "qt", "dv", "dif", "rm", "rv", "vfw", "wmp", "wm", "wmv"
        My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("Treatment", 3, Microsoft.Win32.RegistryValueKind.DWord)
        My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("TypeOverlay", "", Microsoft.Win32.RegistryValueKind.String) 'Application.StartupPath & "\" & EXEName & "@" & bIcon.ToString.Trim
        For I As Integer = 0 To UBound(sExt)
          My.Computer.Registry.ClassesRoot.CreateSubKey(sExt(I)).SetValue(String.Empty, sType)
          If Thumbnails Then
            Dim MyCR, MyExt, MySE As Microsoft.Win32.RegistryKey
            MyCR = Nothing
            MyExt = Nothing
            MySE = Nothing
            For Each CRSub In My.Computer.Registry.ClassesRoot.GetSubKeyNames
              If String.Compare(CRSub, sExt(I), True) = 0 Then
                MyCR = My.Computer.Registry.ClassesRoot.OpenSubKey(CRSub, True)
                For Each EXTSub In MyCR.GetSubKeyNames
                  If String.Compare(EXTSub, "ShellEx", True) = 0 Then
                    MyExt = MyCR.OpenSubKey(EXTSub, True)
                    For Each SESub In MyExt.GetSubKeyNames
                      If String.Compare(SESub, "{E357FCCD-A995-4576-B01F-234630154E96}", True) = 0 Then
                        MySE = MyExt.OpenSubKey(SESub, True)
                        Exit For
                      End If
                    Next
                    Exit For
                  End If
                Next
                Exit For
              End If
            Next
            If MyCR Is Nothing Then MyCR = My.Computer.Registry.ClassesRoot.CreateSubKey(sExt(I), Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree)
            If MyExt Is Nothing Then MyExt = MyCR.CreateSubKey("ShellEx", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree)
            If MySE Is Nothing Then MySE = MyExt.CreateSubKey("{E357FCCD-A995-4576-B01F-234630154E96}", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree)
            If String.IsNullOrEmpty(MySE.GetValue(String.Empty)) Then
              MySE.SetValue(String.Empty, "{F0DAFCE8-5D35-4609-B0BE-48CBF0FC21DB}")
            ElseIf MySE.GetValue(String.Empty) = "{F0DAFCE8-5D35-4609-B0BE-48CBF0FC21DB}" Then
              'do nothing
            Else
              MySE.SetValue("OLD", MySE.GetValue(String.Empty))
              MySE.SetValue(String.Empty, "{F0DAFCE8-5D35-4609-B0BE-48CBF0FC21DB}")
            End If

          Else
            Dim MyCR, MyExt, MySE As Microsoft.Win32.RegistryKey
            MyCR = Nothing
            MyExt = Nothing
            MySE = Nothing
            For Each CRSub In My.Computer.Registry.ClassesRoot.GetSubKeyNames
              If String.Compare(CRSub, sExt(I), True) = 0 Then
                MyCR = My.Computer.Registry.ClassesRoot.OpenSubKey(CRSub, True)
                For Each EXTSub In MyCR.GetSubKeyNames
                  If String.Compare(EXTSub, "ShellEx", True) = 0 Then
                    MyExt = MyCR.OpenSubKey(EXTSub, True)
                    For Each SESub In MyExt.GetSubKeyNames
                      If String.Compare(SESub, "{E357FCCD-A995-4576-B01F-234630154E96}", True) = 0 Then
                        MySE = MyExt.OpenSubKey(SESub, True)
                        Exit For
                      End If
                    Next
                    Exit For
                  End If
                Next
                Exit For
              End If
            Next
            If MyCR Is Nothing Then MyCR = My.Computer.Registry.ClassesRoot.CreateSubKey(sExt(I), Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree)
            If MyExt IsNot Nothing Then
              If MySE IsNot Nothing Then
                If String.IsNullOrEmpty(MySE.GetValue(String.Empty)) Then
                  'Nothing
                ElseIf MySE.GetValue(String.Empty) = "{F0DAFCE8-5D35-4609-B0BE-48CBF0FC21DB}" Then
                  Dim DidSomething As Boolean = False
                  If Not String.IsNullOrEmpty(MySE.GetValue("OLD")) Then
                    MySE.SetValue(String.Empty, MySE.GetValue("OLD"))
                    MySE.DeleteValue("OLD")
                    DidSomething = True
                  End If
                  For Each SESub In MyExt.GetSubKeyNames
                    If String.Compare(SESub, "{BB2E617C-0920-11D1-9A0B-00C04FC2D6C1}", True) = 0 Then
                      If String.Compare(MyExt.OpenSubKey(SESub).GetValue(String.Empty), "{9DBD2C50-62AD-11D0-B806-00C04FD706EC}", True) = 0 Then
                        MySE.SetValue(String.Empty, "{9DBD2C50-62AD-11D0-B806-00C04FD706EC}")
                        DidSomething = True
                      End If
                      Exit For
                    End If
                  Next
                  If Not DidSomething Then
                    For Each SESub In MyExt.GetSubKeyNames
                      If String.Compare(SESub, "{E357FCCD-A995-4576-B01F-234630154E96}", True) = 0 Then
                        MyExt.DeleteSubKeyTree(SESub)
                        Exit For
                      End If
                    Next
                  End If
                Else
                  'Nothing
                End If
              End If
            End If



          End If
        Next
      Case Else
        If Array.Exists(My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).GetValueNames.ToArray, Function(sVal) String.Compare(sVal, "Treatment", True) = 0) Then My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).DeleteValue("Treatment")
        If Array.Exists(My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).GetValueNames.ToArray, Function(sVal) String.Compare(sVal, "TypeOverlay", True) = 0) Then My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).DeleteValue("TypeOverlay")
        For I As Integer = 0 To UBound(sExt)
          My.Computer.Registry.ClassesRoot.CreateSubKey(sExt(I)).SetValue(String.Empty, sType)
        Next I
    End Select
    Add_Props(sType, sDescr, sExt)
  End Sub

  Private Const INPROC As String = "InProcServer32"
  Private CLSID_THUMBPROP As New Guid("D82F694E-46DA-4B5A-8DA4-FCE35A1F6CE1")
  Private Sub Add_Props(ByVal sType As String, ByVal sDescr As String, ParamArray sExt() As String)
    Select Case sType
      Case "OGG", "OGM", "MKV"
        If Not My.Computer.Registry.ClassesRoot.OpenSubKey("CLSID").GetSubKeyNames.Contains(CLSID_THUMBPROP.ToString("B").ToUpper) Then My.Computer.Registry.ClassesRoot.OpenSubKey("CLSID", True).CreateSubKey(CLSID_THUMBPROP.ToString("B").ToUpper)
        If Not My.Computer.Registry.ClassesRoot.GetSubKeyNames.Contains(sType) Then My.Computer.Registry.ClassesRoot.CreateSubKey(sType)
        Select Case sType

          Case "OGG", "OGM"
            My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("FullDetails", "Prop:System.Music.TrackNumber;System.Title;System.Music.Artist;System.Music.Album;System.Music.Genre;System.Media.Year;System.Comment;System.Audio.EncodingBitrate;System.Audio.SampleRate;System.Audio.ChannelCount;System.Media.Duration", Microsoft.Win32.RegistryValueKind.String)
            My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("PreviewTitle", "Prop:System.Media.Duration;System.Size", Microsoft.Win32.RegistryValueKind.String)
            My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("PreviewDetails", "Prop:System.Music.Artist;System.Music.AlbumTitle;System.Music.Genre;*System.Media.Duration;System.Rating;System.Media.Year;*System.Size;System.Music.TrackNumber;System.Music.AlbumArtist;System.Title;*System.Audio.EncodingBitrate", Microsoft.Win32.RegistryValueKind.String)
            My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("TileInfo", "Prop:System.Media.Duration;System.Size", Microsoft.Win32.RegistryValueKind.String)
            My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("ExtendedTileInfo", "Prop:System.ItemType;System.Size;System.Music.Artist;System.Media.Duration", Microsoft.Win32.RegistryValueKind.String)
            My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("InfoTip", "Prop:System.ItemType;System.Size;System.Music.Artist;System.Media.Duration", Microsoft.Win32.RegistryValueKind.String)
          Case "MKV"
            My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("FullDetails", "Prop:System.Video.StreamName;System.Media.Year;System.Video.FrameWidth;System.Video.FrameHeight;System.Video.FrameRate;System.Audio.SampleRate;System.Audio.ChannelCount;VideoTimeLength;System.Media.Duration", Microsoft.Win32.RegistryValueKind.String)
            My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("PreviewTitle", "Prop:System.Media.Duration;System.Size", Microsoft.Win32.RegistryValueKind.String)
            My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("TileInfo", "Prop:System.Media.Duration;System.Size", Microsoft.Win32.RegistryValueKind.String)
            My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("ExtendedTileInfo", "Prop:System.ItemType;System.Size;System.Video.FrameWidth;System.Video.FrameHeight;System.Media.Duration", Microsoft.Win32.RegistryValueKind.String)
            My.Computer.Registry.ClassesRoot.OpenSubKey(sType, True).SetValue("InfoTip", "Prop:System.Video.StreamName;System.ItemType;System.Video.FrameWidth;System.Video.FrameHeight;System.Media.Duration", Microsoft.Win32.RegistryValueKind.String)
        End Select
        If Not My.Computer.Registry.LocalMachine.
          GetSubKeyNames.Contains("SOFTWARE") Then
          My.Computer.Registry.LocalMachine.
            CreateSubKey("SOFTWARE")
        End If
        If Not My.Computer.Registry.LocalMachine.
          OpenSubKey("SOFTWARE").
          GetSubKeyNames.Contains("Microsoft") Then
          My.Computer.Registry.LocalMachine.
            OpenSubKey("SOFTWARE", True).
            CreateSubKey("Microsoft")
        End If
        If Not My.Computer.Registry.LocalMachine.
          OpenSubKey("SOFTWARE").
          OpenSubKey("Microsoft").
          GetSubKeyNames.Contains("Windows") Then
          My.Computer.Registry.LocalMachine.
            OpenSubKey("SOFTWARE", True).
            OpenSubKey("Microsoft", True).
            CreateSubKey("Windows")
        End If
        If Not My.Computer.Registry.LocalMachine.
          OpenSubKey("SOFTWARE").
          OpenSubKey("Microsoft").
          OpenSubKey("Windows").
          GetSubKeyNames.Contains("CurrentVersion") Then
          My.Computer.Registry.LocalMachine.
            OpenSubKey("SOFTWARE", True).
            OpenSubKey("Microsoft", True).
            OpenSubKey("Windows", True).
            CreateSubKey("CurrentVersion")
        End If
        If Not My.Computer.Registry.LocalMachine.
          OpenSubKey("SOFTWARE").
          OpenSubKey("Microsoft").
          OpenSubKey("Windows").
          OpenSubKey("CurrentVersion").
          GetSubKeyNames.Contains("PropertySystem") Then
          My.Computer.Registry.LocalMachine.
            OpenSubKey("SOFTWARE", True).
            OpenSubKey("Microsoft", True).
            OpenSubKey("Windows", True).
            OpenSubKey("CurrentVersion", True).
            CreateSubKey("PropertySystem")
        End If
        If Not My.Computer.Registry.LocalMachine.
          OpenSubKey("SOFTWARE").
          OpenSubKey("Microsoft").
          OpenSubKey("Windows").
          OpenSubKey("CurrentVersion").
          OpenSubKey("PropertySystem").
          GetSubKeyNames.Contains("PropertyHandlers") Then
          My.Computer.Registry.LocalMachine.
            OpenSubKey("SOFTWARE", True).
            OpenSubKey("Microsoft", True).
            OpenSubKey("Windows", True).
            OpenSubKey("CurrentVersion", True).
            OpenSubKey("PropertySystem", True).
            CreateSubKey("PropertyHandlers")
        End If
        For Each ext In sExt
          If Not My.Computer.Registry.LocalMachine.
            OpenSubKey("SOFTWARE").
            OpenSubKey("Microsoft").
            OpenSubKey("Windows").
            OpenSubKey("CurrentVersion").
            OpenSubKey("PropertySystem").
            OpenSubKey("PropertyHandlers").
            GetSubKeyNames.Contains(ext) Then
            My.Computer.Registry.LocalMachine.
              OpenSubKey("SOFTWARE", True).
              OpenSubKey("Microsoft", True).
              OpenSubKey("Windows", True).
              OpenSubKey("CurrentVersion", True).
              OpenSubKey("PropertySystem", True).
              OpenSubKey("PropertyHandlers", True).
              CreateSubKey(ext)
          End If
          My.Computer.Registry.LocalMachine.
              OpenSubKey("SOFTWARE", True).
              OpenSubKey("Microsoft", True).
              OpenSubKey("Windows", True).
              OpenSubKey("CurrentVersion", True).
              OpenSubKey("PropertySystem", True).
              OpenSubKey("PropertyHandlers", True).
              OpenSubKey(ext, True).
              SetValue(String.Empty, CLSID_THUMBPROP.ToString("B").ToUpper, Microsoft.Win32.RegistryValueKind.String)
        Next
        If Not My.Computer.Registry.LocalMachine.
          OpenSubKey("SOFTWARE").
          OpenSubKey("Microsoft").
          OpenSubKey("Windows").
          OpenSubKey("CurrentVersion").
          GetSubKeyNames.Contains("Shell Extensions") Then
          My.Computer.Registry.LocalMachine.
            OpenSubKey("SOFTWARE", True).
            OpenSubKey("Microsoft", True).
            OpenSubKey("Windows", True).
            OpenSubKey("CurrentVersion", True).
            CreateSubKey("Shell Extensions")
        End If
        If Not My.Computer.Registry.LocalMachine.
          OpenSubKey("SOFTWARE").
          OpenSubKey("Microsoft").
          OpenSubKey("Windows").
          OpenSubKey("CurrentVersion").
          OpenSubKey("Shell Extensions").
          GetSubKeyNames.Contains("Approved") Then
          My.Computer.Registry.LocalMachine.
            OpenSubKey("SOFTWARE", True).
            OpenSubKey("Microsoft", True).
            OpenSubKey("Windows", True).
            OpenSubKey("CurrentVersion", True).
            OpenSubKey("Shell Extensions", True).
            CreateSubKey("Approved")
        End If
        My.Computer.Registry.LocalMachine.
              OpenSubKey("SOFTWARE", True).
              OpenSubKey("Microsoft", True).
              OpenSubKey("Windows", True).
              OpenSubKey("CurrentVersion", True).
              OpenSubKey("Shell Extensions", True).
              OpenSubKey("Approved", True).
              SetValue(CLSID_THUMBPROP.ToString("B").ToUpper, "GreenThumb.PropertyHandler", Microsoft.Win32.RegistryValueKind.String)
    End Select
  End Sub

  Private Sub Add_Disc(ByVal sType As String, ByVal bIcon As Byte, ByVal sDescr As String)
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).SetValue(String.Empty, sDescr)
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).SetValue("BaseClass", "Drive")
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).SetValue("BrowserFlags", 8)
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).SetValue("EditFlags", 2)
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).CreateSubKey("shell").SetValue(String.Empty, "Play")
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).CreateSubKey("shell").CreateSubKey("Play").SetValue(String.Empty, "&Play")
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).CreateSubKey("shell").CreateSubKey("Play").CreateSubKey("command").SetValue(String.Empty, """" & Application.StartupPath & "\" & EXEName & """ /" & sType & " %1")
    My.Computer.Registry.ClassesRoot.CreateSubKey(sType).CreateSubKey("DefaultIcon").SetValue(String.Empty, Application.StartupPath & "\" & EXEName & "," & bIcon.ToString.Trim)
  End Sub

  Public Sub AddType(ByVal fType As FileTypes)
    Select Case fType
      Case FileTypes.File_MPC
        Add_Associate("MPC", 1, "Musepack Audio File", ".mpc")
      Case FileTypes.File_AC3
        Add_Associate("AC3", 2, "Dolby Digital Advanced Coding-3 Audio File", ".ac3")
      Case FileTypes.File_AIF
        Add_Associate("AIF", 3, "Audio Interchange File", ".aif")
        Add_Associate("AIFC", 4, "Audio Interchange File (Compressed)", ".aifc")
        Add_Associate("AIFF", 5, "Audio Interchange File Format", ".aiff")
      Case FileTypes.File_ASF
        Add_Associate("ASF", 6, "Advanced Systems File", ".asf")
        Add_Associate("ASX", 7, "Advanced Systems Redirector", ".asx")
      Case FileTypes.File_AU
        Add_Associate("AU", 8, "Audio Clip", ".au")
        Add_Associate("SND", 9, "Sound Clip", ".snd")
      Case FileTypes.File_MIDI
        Add_Associate("MID", 10, "Musical Instrument Digital Interface File", ".mid")
        Add_Associate("MIDI", 11, "Musical Instrument Digital Interface File", ".midi")
        Add_Associate("RMI", 12, "RIFF Encoded MIDI File", ".rmi")
      Case FileTypes.File_APE
        Add_Associate("APE", 13, "Monkey's Audio Lossless Audio Compression Format File", ".ape")
      Case FileTypes.File_FLAC
        Add_Associate("FLAC", 14, "Free Lossless Audio Codec File", ".flac")
      Case FileTypes.File_OFR
        Add_Associate("OFR", 15, "OptimFROG Encoded Audio File", ".ofr")
      Case FileTypes.File_TTA
        Add_Associate("TTA", 16, "TrueAudio Free Lossless Audio Codec File", ".tta")
      Case FileTypes.File_WAV
        Add_Associate("WAV", 17, "Waveform Audio File", ".wav")
      Case FileTypes.File_AVI
        Add_Associate("AVI", 18, "Audio/Video Interleave File", ".avi")
      Case FileTypes.File_DIVX
        Add_Associate("DIVX", 19, "DivX Video File", ".divx")
      Case FileTypes.File_IVF
        Add_Associate("IVF", 20, "Indeo Video File", ".ivf")
      Case FileTypes.File_MKV
        Add_Associate("MKV", 21, "Matroska Video File", ".mkv")
      Case FileTypes.File_MKA
        Add_Associate("MKA", 22, "Matroska Audio File", ".mka")
      Case FileTypes.File_FLV
        Add_Associate("FLV", 23, "Adobe Flash Video File", ".flv")
        Add_Associate("F4V", 24, "Adobe Flash MPEG-4 Video File", ".f4v")
      Case FileTypes.File_SPL
        Add_Associate("SPL", 25, "FutureSplash Object", ".spl")
      Case FileTypes.File_SWF
        Add_Associate("SWF", 26, "Shockwave Flash Object", ".swf")
      Case FileTypes.File_CDA
        Add_Associate("CDA", 77, "CD Audio Track Shortcut", ".cda")
        Add_Disc("AudioCD", 31, "Audio Compact Disc")
      Case FileTypes.File_DAT
        Add_Associate("DAT", 79, "Video CD MPEG Movie File", ".dat")
      Case FileTypes.File_VOB
        Add_Associate("VOB", 28, "DVD Video File", ".vob")
        Add_Disc("DVD", 78, "Digital Video Disc")
      Case FileTypes.File_MPE
        Add_Associate("MPE", 29, "MPEG-1 Movie Clip", ".mpe")
      Case FileTypes.File_MPG
        Add_Associate("MPG", 30, "MPEG-1 Layer I System Stream File", ".mpg")
        Add_Associate("MPEG", 31, "MPEG-1 Layer I System Stream File", ".mpeg")
      Case FileTypes.File_M1V
        Add_Associate("M1V", 32, "MPEG-1 Layer I Video File", ".m1v")
        Add_Associate("MPV", 33, "MPEG-1 Layer I Video File", ".mpv")
      Case FileTypes.File_MP2
        Add_Associate("MP2", 34, "MPEG-1 Layer II Audio File", ".mp2")
        Add_Associate("M2A", 35, "MPEG-1 Layer II Audio File", ".m2a")
      Case FileTypes.File_M2V
        Add_Associate("M2V", 36, "MPEG-1 Layer II Video File", ".m2v")
        Add_Associate("MP2V", 37, "MPEG-1 Layer II Video File", ".mp2v")
        Add_Associate("MPV2", 38, "MPEG-1 Layer II Video File", ".mpv2")
      Case FileTypes.File_MP3
        Add_Associate("MP3", 39, "MPEG-1 Layer III Audio File", ".mp3")
      Case FileTypes.File_MPA
        Add_Associate("MPA", 40, "MPEG-1 Layer 1 Audio Stream", ".mpa")
        Add_Associate("MP1", 41, "MPEG-1 Layer I Audio Stream", ".mp1")
        Add_Associate("M1A", 42, "MPEG-1 Layer I Audio Stream", ".m1a")
      Case FileTypes.File_AAC
        Add_Associate("AAC", 43, "MPEG-2 Advanced Audio Coding File", ".aac")
      Case FileTypes.File_M2TS
        Add_Associate("M2TS", 44, "MPEG-2 Transport Stream File", ".m2ts")
      Case FileTypes.File_M4A
        Add_Associate("M4A", 45, "MPEG-4 Audio Layer File", ".m4a")
      Case FileTypes.File_M4P
        Add_Associate("M4P", 46, "Protected Advanced Audio Coding File", ".m4p")
      Case FileTypes.File_M4V
        Add_Associate("M4V", 47, "MPEG-4 Video File", ".m4v")
        Add_Associate("MP4", 48, "MPEG-4 Video File", ".mp4")
      Case FileTypes.File_OGG
        Add_Associate("OGG", 49, "OGG Vorbis File", ".ogg")
        Add_Associate("OGA", 50, "OGG Vorbis Audio File", ".oga")
        Add_Associate("OGV", 51, "OGG Vorbis Video File", ".ogv")
      Case FileTypes.File_OGM
        Add_Associate("OGM", 52, "OGG Vorbis Media Compressed Video File", ".ogm")
      Case FileTypes.File_MOV
        Add_Associate("MOV", 53, "QuickTime Movie File", ".mov")
        Add_Associate("QT", 54, "QuickTime File", ".qt")
      Case FileTypes.File_DV
        Add_Associate("DV", 55, "Digital Video File", ".dv")
        Add_Associate("DIF", 56, "Digital Interface Format Video File", ".dif")
      Case FileTypes.File_3GP
        Add_Associate("3GP", 57, "Third Generation Partnership File", ".3gp")
        Add_Associate("3GPP", 58, "Third Generation Partnership Project File", ".3gpp")
      Case FileTypes.File_3G2
        Add_Associate("3G2", 59, "Third Generation Partnership Version 2 File", ".3g2")
        Add_Associate("3GP2", 60, "Third Generation Partnership Project Version 2 File", ".3gp2")
      Case FileTypes.File_RA
        Add_Associate("RA", 61, "RealAudio File", ".ra")
        Add_Associate("RAM", 62, "RealAudio Metafile", ".ram")
      Case FileTypes.File_RM
        Add_Associate("RM", 63, "RealMedia File", ".rm")
        Add_Associate("RMM", 64, "RealMedia Metafile", ".rmm")
        Add_Associate("RMVB", 63, "RealMedia Variable Bitrate File", ".rmvb")
      Case FileTypes.File_RV
        Add_Associate("RV", 65, "RealVideo File", ".rv")
      Case FileTypes.File_VFW
        Add_Associate("VFW", 66, "Video For Windows File", ".vfw")
      Case FileTypes.File_WMP
        Add_Associate("WMP", 67, "Windows Media Player File", ".wmp")
      Case FileTypes.File_WM
        Add_Associate("WM", 68, "Windows Media Audio/Video File", ".wm")
        Add_Associate("WMX", 69, "Windows Media Audio/Video Redirector", ".wmx")
      Case FileTypes.File_WMA
        Add_Associate("WMA", 70, "Windows Media Audio File", ".wma")
        Add_Associate("WAX", 71, "Windows Media Audio Redirector", ".wax")
      Case FileTypes.File_WMV
        Add_Associate("WMV", 72, "Windows Media Video File", ".wmv")
        Add_Associate("WVX", 73, "Windows Media Video Redirector", ".wvx")
      Case FileTypes.File_M3U
        Add_Associate("M3U", 74, "MP3 Playlist File", ".m3u")
      Case FileTypes.File_PLS
        Add_Associate("PLS", 75, "Generic Playlist File", ".pls")
      Case FileTypes.File_LLPL
        Add_Associate("LLPL", 76, "Lime Light PlayList File", ".llpl")
      Case FileTypes.File_Directory
        My.Computer.Registry.ClassesRoot.CreateSubKey("Directory").CreateSubKey("shell").CreateSubKey("Play").CreateSubKey("command").SetValue(String.Empty, """" & Application.StartupPath & "\" & EXEName & """ ""%1""")
    End Select
  End Sub

  Private Sub Rem_Associate(ByVal sType As String, ParamArray sExt() As String)
    If My.Computer.Registry.ClassesRoot.GetSubKeyNames.Contains(sType) Then My.Computer.Registry.ClassesRoot.DeleteSubKeyTree(sType)
    For I As Integer = 0 To UBound(sExt)
      Select sType.ToLower
        Case "avi", "divx", "ivf", "mkv", "flv", "spl", "swf", "mpe", "mpg", "mpeg", "m1v", "m2v", "mp2v", "mpv2", "m2ts", "mp4", "m4v", "ogg", "ogm", "mov", "qt", "dv", "dif", "rm", "rv", "vfw", "wmp", "wm", "wmv"
          Dim MyCR, MyExt, MySE As Microsoft.Win32.RegistryKey
          MyCR = Nothing
          MyExt = Nothing
          MySE = Nothing
          For Each CRSub In My.Computer.Registry.ClassesRoot.GetSubKeyNames
            If String.Compare(CRSub, sExt(I), True) = 0 Then
              MyCR = My.Computer.Registry.ClassesRoot.OpenSubKey(CRSub, True)
              For Each EXTSub In MyCR.GetSubKeyNames
                If String.Compare(EXTSub, "ShellEx", True) = 0 Then
                  MyExt = MyCR.OpenSubKey(EXTSub, True)
                  For Each SESub In MyExt.GetSubKeyNames
                    If String.Compare(SESub, "{E357FCCD-A995-4576-B01F-234630154E96}", True) = 0 Then
                      MySE = MyExt.OpenSubKey(SESub, True)
                      Exit For
                    End If
                  Next
                  Exit For
                End If
              Next
              Exit For
            End If
          Next
          If MyCR IsNot Nothing Then
            If MyExt IsNot Nothing Then
              If MySE IsNot Nothing Then
                If String.IsNullOrEmpty(MySE.GetValue(String.Empty)) Then
                  'Nothing
                ElseIf MySE.GetValue(String.Empty) = "{F0DAFCE8-5D35-4609-B0BE-48CBF0FC21DB}" Then
                  Dim DidSomething As Boolean = False
                  If Not String.IsNullOrEmpty(MySE.GetValue("OLD")) Then
                    MySE.SetValue(String.Empty, MySE.GetValue("OLD"))
                    MySE.DeleteValue("OLD")
                    DidSomething = True
                  End If
                  For Each SESub In MyExt.GetSubKeyNames
                    If String.Compare(SESub, "{BB2E617C-0920-11D1-9A0B-00C04FC2D6C1}", True) = 0 Then
                      If String.Compare(MyExt.OpenSubKey(SESub).GetValue(String.Empty), "{9DBD2C50-62AD-11D0-B806-00C04FD706EC}", True) = 0 Then
                        MySE.SetValue(String.Empty, "{9DBD2C50-62AD-11D0-B806-00C04FD706EC}")
                        DidSomething = True
                      End If
                      Exit For
                    End If
                  Next
                  If Not DidSomething Then
                    For Each SESub In MyExt.GetSubKeyNames
                      If String.Compare(SESub, "{E357FCCD-A995-4576-B01F-234630154E96}", True) = 0 Then
                        MyExt.DeleteSubKeyTree(SESub)
                        Exit For
                      End If
                    Next
                  End If
                Else
                  'Nothing
                End If
              End If
            End If
          End If

      End Select
      'If My.Computer.Registry.ClassesRoot.GetSubKeyNames.Contains(sExt(I)) Then My.Computer.Registry.ClassesRoot.DeleteSubKeyTree(sExt(I))
    Next I
    'regDelete_Sub_Key(HKEY_CLASSES_ROOT, sType & "\shell", vbNullString)
    'regDelete_Sub_Key(HKEY_CLASSES_ROOT, sType & "\shell\Play", vbNullString)
    'regDelete_Sub_Key(HKEY_CLASSES_ROOT, sType & "\shell\Play\command", vbNullString)
    'regDelete_Sub_Key(HKEY_CLASSES_ROOT, sType & "\DefaultIcon", vbNullString)
  End Sub

  Private Sub Rem_Disc(ByVal sType As String)
    If My.Computer.Registry.ClassesRoot.GetSubKeyNames.Contains(sType) Then My.Computer.Registry.ClassesRoot.DeleteSubKeyTree(sType)
    'regDelete_Sub_Key(HKEY_CLASSES_ROOT, sType, vbNullString)
    'regDelete_Sub_Key(HKEY_CLASSES_ROOT, sType & "\shell\Play", vbNullString)
    'regDelete_Sub_Key(HKEY_CLASSES_ROOT, sType & "\shell\Play\command", vbNullString)
    'regDelete_Sub_Key(HKEY_CLASSES_ROOT, sType & "\DefaultIcon", vbNullString)
  End Sub

  Public Sub RemType(ByVal fType As FileTypes)
    Select Case fType
      Case FileTypes.File_MPC
        Rem_Associate("MPC", ".mpc")
      Case FileTypes.File_AC3
        Rem_Associate("AC3", ".ac3")
      Case FileTypes.File_AIF
        Rem_Associate("AIF", ".aif")
        Rem_Associate("AIFC", ".aifc")
        Rem_Associate("AIFF", ".aiff")
      Case FileTypes.File_ASF
        Rem_Associate("ASF", ".asf")
        Rem_Associate("ASX", ".asx")
      Case FileTypes.File_AU
        Rem_Associate("AU", ".au")
        Rem_Associate("SND", ".snd")
      Case FileTypes.File_MIDI
        Rem_Associate("MID", ".mid")
        Rem_Associate("MIDI", ".midi")
        Rem_Associate("RMI", ".rmi")
      Case FileTypes.File_APE
        Rem_Associate("APE", ".ape")
      Case FileTypes.File_FLAC
        Rem_Associate("FLAC", ".flac")
      Case FileTypes.File_OFR
        Rem_Associate("OFR", ".ofr")
      Case FileTypes.File_TTA
        Rem_Associate("TTA", ".tta")
      Case FileTypes.File_WAV
        Rem_Associate("WAV", ".wav")
      Case FileTypes.File_AVI
        Rem_Associate("AVI", ".avi")
      Case FileTypes.File_DIVX
        Rem_Associate("DIVX", ".divx")
      Case FileTypes.File_IVF
        Rem_Associate("IVF", ".ivf")
      Case FileTypes.File_MKV
        Rem_Associate("MKV", ".mkv")
      Case FileTypes.File_MKA
        Rem_Associate("MKA", ".mka")
      Case FileTypes.File_FLV
        Rem_Associate("FLV", ".flv")
        Rem_Associate("F4V", ".f4v")
      Case FileTypes.File_SPL
        Rem_Associate("SPL", ".spl")
      Case FileTypes.File_SWF
        Rem_Associate("SWF", ".swf")
      Case FileTypes.File_CDA
        Rem_Associate("CDA", ".cda")
        Rem_Disc("AudioCD")
      Case FileTypes.File_DAT
        Rem_Associate("DAT", ".dat")
      Case FileTypes.File_VOB
        Rem_Associate("VOB", ".vob")
        Rem_Disc("DVD")
      Case FileTypes.File_MPE
        Rem_Associate("MPE", ".mpe")
      Case FileTypes.File_MPG
        Rem_Associate("MPG", ".mpg")
        Rem_Associate("MPEG", ".mpeg")
      Case FileTypes.File_M1V
        Rem_Associate("M1V", ".m1v")
        Rem_Associate("MPV", ".mpv")
      Case FileTypes.File_MP2
        Rem_Associate("MP2", ".mp2")
        Rem_Associate("M2A", ".m2a")
      Case FileTypes.File_M2V
        Rem_Associate("M2V", ".m2v")
        Rem_Associate("MP2V", ".mp2v")
        Rem_Associate("MPV2", ".mpv2")
      Case FileTypes.File_MP3
        Rem_Associate("MP3", ".mp3")
      Case FileTypes.File_MPA
        Rem_Associate("MPA", ".mpa")
        Rem_Associate("MP1", ".mp1")
        Rem_Associate("M1A", ".m1a")
      Case FileTypes.File_AAC
        Rem_Associate("AAC", ".aac")
      Case FileTypes.File_M2TS
        Rem_Associate("M2TS", ".m2ts")
      Case FileTypes.File_M4A
        Rem_Associate("M4A", ".m4a")
      Case FileTypes.File_M4P
        Rem_Associate("M4P", ".m4p")
      Case FileTypes.File_M4V
        Rem_Associate("M4V", ".m4v")
        Rem_Associate("MP4", ".mp4")
      Case FileTypes.File_OGG
        Rem_Associate("OGG", ".ogg")
      Case FileTypes.File_OGM
        Rem_Associate("OGM", ".ogm")
      Case FileTypes.File_MOV
        Rem_Associate("MOV", ".mov")
        Rem_Associate("QT", ".qt")
      Case FileTypes.File_DV
        Rem_Associate("DV", ".dv")
        Rem_Associate("DIF", ".dif")
      Case FileTypes.File_3GP
        Rem_Associate("3GP", ".3gp")
        Rem_Associate("3GPP", ".3gpp")
      Case FileTypes.File_3G2
        Rem_Associate("3G2", ".3g2")
        Rem_Associate("3GP2", ".3gp2")
      Case FileTypes.File_RA
        Rem_Associate("RA", ".ra")
        Rem_Associate("RAM", ".ram")
      Case FileTypes.File_RM
        Rem_Associate("RM", ".rm")
        Rem_Associate("RMM", ".rmm")
      Case FileTypes.File_RV
        Rem_Associate("RV", ".rv")
      Case FileTypes.File_VFW
        Rem_Associate("VFW", ".vfw")
      Case FileTypes.File_WMP
        Rem_Associate("WMP", ".wmp")
      Case FileTypes.File_WM
        Rem_Associate("WM", ".wm")
        Rem_Associate("WMX", ".wmx")
      Case FileTypes.File_WMA
        Rem_Associate("WMA", ".wma")
        Rem_Associate("WAX", ".wax")
      Case FileTypes.File_WMV
        Rem_Associate("WMV", ".wmv")
        Rem_Associate("WVX", ".wvx")
      Case FileTypes.File_M3U
        Rem_Associate("M3U", ".m3u")
      Case FileTypes.File_PLS
        Rem_Associate("PLS", ".pls")
      Case FileTypes.File_LLPL
        Rem_Associate("LLPL", ".llpl")
      Case FileTypes.File_Directory
        If My.Computer.Registry.ClassesRoot.GetSubKeyNames.Contains("Directory") AndAlso
          My.Computer.Registry.ClassesRoot.OpenSubKey("Directory").GetSubKeyNames.Contains("shell") AndAlso
          My.Computer.Registry.ClassesRoot.OpenSubKey("Directory").OpenSubKey("shell").GetSubKeyNames.Contains("Play") Then My.Computer.Registry.ClassesRoot.OpenSubKey("Directory", True).OpenSubKey("shell", True).DeleteSubKeyTree("Play")
    End Select
  End Sub

  Public Sub ConfirmFileTypes()
    SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, 0, 0)
  End Sub

End Module
