Public Class frmSettings
  Private Enum FileTypes
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

  Private interceptKey As Boolean
  Private WithEvents cJoy As clsJoyDetection
  Private volDevice As CoreAudioApi.MMDevice
  Private objDraw As Object
  Private methodDraw As System.Reflection.MethodInfo

  Private Sub cmdOK_Click(sender As System.Object, e As System.EventArgs) Handles cmdOK.Click
    Me.Close()
  End Sub

  Private Sub cmdCancel_Click(sender As System.Object, e As System.EventArgs) Handles cmdCancel.Click
    Me.Close()
  End Sub

  Private Sub frmSettings_Load(sender As Object, e As System.EventArgs) Handles Me.Load
    Me.Tag = "LOAD"
    CheckAssociations()
    numGapless.Value = My.Settings.Gapless * 1000
    chkSingleInstance.Checked = My.Settings.SingleInstance
    cmbAudioOutput.Items.Clear()
    cmbAudioOutput.Items.Add("Windows Default Device")
    For Each item As DirectShowLib.DsDevice In Seed.ctlSeed.GetRenderers("audio renderers")
      If item.Name.StartsWith("DirectSound: ") Then
        cmbAudioOutput.Items.Add(item.Name.Substring(13))
        If item.Name.Substring(13) = My.Settings.Device Then
          cmbAudioOutput.SelectedIndex = cmbAudioOutput.Items.Count - 1
        End If
      End If
    Next
    If cmbAudioOutput.SelectedIndex < 0 Then cmbAudioOutput.SelectedIndex = 0

    cmbLocale.Items.Clear()
    Dim iDefault As Integer = -1
    For Each culture In Globalization.CultureInfo.GetCultures(Globalization.CultureTypes.NeutralCultures)
      If culture.LCID = 127 Then Continue For
      If String.Compare(culture.NativeName, culture.DisplayName, True) = 0 Then
        cmbLocale.Items.Add(culture.NativeName)
      Else
        cmbLocale.Items.Add(culture.NativeName & " [" & culture.DisplayName & "]")
      End If
      If culture.TwoLetterISOLanguageName = Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName Then iDefault = cmbLocale.Items.Count - 1
    Next
    If String.IsNullOrEmpty(My.Settings.DefaultLocale) Then
      cmbLocale.SelectedIndex = iDefault
    ElseIf cmbLocale.Items.Contains(My.Settings.DefaultLocale) Then
      cmbLocale.Text = My.Settings.DefaultLocale
    Else
      cmbLocale.SelectedIndex = iDefault
    End If
    chkSubtitles.Checked = My.Settings.Subtitles
    'cmbLocale.Sorted = True

    lstVis.Items.Clear()
    lstVis.Items.Add("None")
    If My.Computer.FileSystem.DirectoryExists(Application.StartupPath & "\Visualizations") Then
      For Each file In My.Computer.FileSystem.GetFiles(Application.StartupPath & "\Visualizations")
        If file.EndsWith(".dll") Then lstVis.Items.Add(IO.Path.GetFileNameWithoutExtension(file))
      Next
    End If
    If lstVis.Items.Contains(My.Settings.Visualization) Then
      lstVis.Text = My.Settings.Visualization
    Else
      lstVis.Text = "None"
    End If
    txtRate.Value = My.Settings.Visualization_Rate

    chkKeyboard.Checked = My.Settings.Keyboard
    txtKeyOpen.Text = My.Settings.Keyboard_Open
    txtKeyClose.Text = My.Settings.Keyboard_Close
    txtKeyFileProperties.Text = My.Settings.Keyboard_Props
    txtKeySettings.Text = My.Settings.Keyboard_Settings
    txtKeyWebpage.Text = My.Settings.Keyboard_Webpage
    txtKeyAbout.Text = My.Settings.Keyboard_About
    txtKeyDVDMenu.Text = My.Settings.Keyboard_DVDMenu
    txtKeyDiscEject.Text = My.Settings.Keyboard_DiscEject
    txtKeyPlayPause.Text = My.Settings.Keyboard_PlayPause
    txtKeyStop.Text = My.Settings.Keyboard_Stop
    txtKeyLast.Text = My.Settings.Keyboard_Last
    txtKeyNext.Text = My.Settings.Keyboard_Next
    txtKeyMute.Text = My.Settings.Keyboard_Mute
    txtKeyFullScreen.Text = My.Settings.Keyboard_FS
    txtKeyVolUp.Text = My.Settings.Keyboard_VolUp
    txtKeyVolDown.Text = My.Settings.Keyboard_VolDown
    txtKeySkipBack.Text = My.Settings.Keyboard_SkipBack
    txtKeySkipFwd.Text = My.Settings.Keyboard_SkipFwd
    txtKeyAddToPL.Text = My.Settings.Keyboard_AddToPL
    txtKeyRemoveFromPL.Text = My.Settings.Keyboard_RemoveFromPL
    txtKeyClearPL.Text = My.Settings.Keyboard_ClearPL
    txtKeySavePL.Text = My.Settings.Keyboard_SavePL
    txtKeyOpenPL.Text = My.Settings.Keyboard_OpenPL
    txtKeyShuffle.Text = My.Settings.Keyboard_Shuffle
    txtKeyRepeatTrack.Text = My.Settings.Keyboard_RepeatTrack
    txtKeyRepeatPL.Text = My.Settings.Keyboard_RepeatPL
    txtKeyRenamePL.Text = My.Settings.Keyboard_RenamePL
    chkKeyboard_CheckedChanged(chkKeyboard, New EventArgs())

    chkGamepad.Checked = My.Settings.Gamepad
    txtPadOpen.Text = My.Settings.Gamepad_Open
    txtPadClose.Text = My.Settings.Gamepad_Close
    txtPadProps.Text = My.Settings.Gamepad_Props
    txtPadSettings.Text = My.Settings.Gamepad_Settings
    txtPadWebpage.Text = My.Settings.Gamepad_Webpage
    txtPadAbout.Text = My.Settings.Gamepad_About
    txtPadDVDMenu.Text = My.Settings.Gamepad_DVDMenu
    txtPadDiscEject.Text = My.Settings.Gamepad_DiscEject
    txtPadPlayPause.Text = My.Settings.Gamepad_PlayPause
    txtPadStop.Text = My.Settings.Gamepad_Stop
    txtPadLast.Text = My.Settings.Gamepad_Last
    txtPadNext.Text = My.Settings.Gamepad_Next
    txtPadMute.Text = My.Settings.Gamepad_Mute
    txtPadFullScreen.Text = My.Settings.Gamepad_FS
    txtPadVolUp.Text = My.Settings.Gamepad_VolUp
    txtPadVolDown.Text = My.Settings.Gamepad_VolDown
    txtPadSkipBack.Text = My.Settings.Gamepad_SkipBack
    txtPadSkipFwd.Text = My.Settings.Gamepad_SkipFwd
    txtPadAddToPL.Text = My.Settings.Gamepad_AddToPL
    txtPadRemoveFromPL.Text = My.Settings.Gamepad_RemoveFromPL
    txtPadClearPL.Text = My.Settings.Gamepad_ClearPL
    txtPadSavePL.Text = My.Settings.Gamepad_SavePL
    txtPadOpenPL.Text = My.Settings.Gamepad_OpenPL
    txtPadShuffle.Text = My.Settings.Gamepad_Shuffle
    txtPadRepeatTrack.Text = My.Settings.Gamepad_RepeatTrack
    txtPadRepeatPL.Text = My.Settings.Gamepad_RepeatPL
    txtPadRenamePL.Text = My.Settings.Gamepad_RenamePL
    chkGamepad_CheckedChanged(chkGamepad, New EventArgs)

    Me.Tag = Nothing
    Dim devEnum As New CoreAudioApi.MMDeviceEnumerator
    volDevice = devEnum.GetDefaultAudioEndpoint(CoreAudioApi.EDataFlow.eRender, CoreAudioApi.ERole.eMultimedia)

    mnuShortcuts.Renderer = New ToolStripSystemRenderer(True)

    tbsSettings.SelectedIndex = 0
    Me.Size = Me.MinimumSize
  End Sub

  Private Sub ChildTags(Node As TreeNode, ByRef List As String)
    If Node.Nodes.Count > 0 Then
      For Each Item As TreeNode In Node.Nodes
        ChildTags(Item, List)
      Next
    Else
      If Node.Checked Then List &= " " & Node.Tag
    End If
  End Sub

  Private Function FindNode(InNode As TreeNode, ByVal Name As String) As TreeNode
    For Each Item As TreeNode In InNode.Nodes
      If String.Compare(Item.Name, Name, True) = 0 Then
        Return Item
      Else
        Dim tN As TreeNode = FindNode(Item, Name)
        If tN IsNot Nothing Then Return tN
      End If
    Next
    Return Nothing
  End Function

  Private Sub CheckAssociations()
    chkThumbnails.Checked = My.Computer.Registry.ClassesRoot.OpenSubKey("CLSID").GetSubKeyNames.Contains("{F0DAFCE8-5D35-4609-B0BE-48CBF0FC21DB}")

    FindNode(tvAssoc.Nodes(0), "nodeMPC").Checked = CheckType(FileTypes.File_MPC)
    FindNode(tvAssoc.Nodes(0), "nodeAC3").Checked = CheckType(FileTypes.File_AC3)
    FindNode(tvAssoc.Nodes(0), "nodeAIF").Checked = CheckType(FileTypes.File_AIF)
    FindNode(tvAssoc.Nodes(0), "nodeASF").Checked = CheckType(FileTypes.File_ASF)
    FindNode(tvAssoc.Nodes(0), "nodeAU").Checked = CheckType(FileTypes.File_AU)
    FindNode(tvAssoc.Nodes(0), "nodeMID").Checked = CheckType(FileTypes.File_MIDI)
    FindNode(tvAssoc.Nodes(0), "nodeAPE").Checked = CheckType(FileTypes.File_APE)
    FindNode(tvAssoc.Nodes(0), "nodeFLAC").Checked = CheckType(FileTypes.File_FLAC)
    FindNode(tvAssoc.Nodes(0), "nodeOFR").Checked = CheckType(FileTypes.File_OFR)
    FindNode(tvAssoc.Nodes(0), "nodeTTA").Checked = CheckType(FileTypes.File_TTA)
    FindNode(tvAssoc.Nodes(0), "nodeWAV").Checked = CheckType(FileTypes.File_WAV)
    CheckParents(FindNode(tvAssoc.Nodes(0), "nodeWAV"))
    FindNode(tvAssoc.Nodes(0), "nodeAVI").Checked = CheckType(FileTypes.File_AVI)
    FindNode(tvAssoc.Nodes(0), "nodeDIVX").Checked = CheckType(FileTypes.File_DIVX)
    FindNode(tvAssoc.Nodes(0), "nodeIVF").Checked = CheckType(FileTypes.File_IVF)
    FindNode(tvAssoc.Nodes(0), "nodeFLV").Checked = CheckType(FileTypes.File_FLV)
    FindNode(tvAssoc.Nodes(0), "nodeSPL").Checked = CheckType(FileTypes.File_SPL)
    FindNode(tvAssoc.Nodes(0), "nodeSWF").Checked = CheckType(FileTypes.File_SWF)
    CheckParents(FindNode(tvAssoc.Nodes(0), "nodeSWF"))
    FindNode(tvAssoc.Nodes(0), "nodeCDA").Checked = CheckType(FileTypes.File_CDA)
    FindNode(tvAssoc.Nodes(0), "nodeVCD").Checked = CheckType(FileTypes.File_DAT)
    FindNode(tvAssoc.Nodes(0), "nodeDVD").Checked = CheckType(FileTypes.File_VOB)
    CheckParents(FindNode(tvAssoc.Nodes(0), "nodeDVD"))
    FindNode(tvAssoc.Nodes(0), "nodeMPE").Checked = CheckType(FileTypes.File_MPE)
    FindNode(tvAssoc.Nodes(0), "nodeMPG").Checked = CheckType(FileTypes.File_MPG)
    FindNode(tvAssoc.Nodes(0), "nodeM1V").Checked = CheckType(FileTypes.File_M1V)
    FindNode(tvAssoc.Nodes(0), "nodeMP2").Checked = CheckType(FileTypes.File_MP2)
    FindNode(tvAssoc.Nodes(0), "nodeM2V").Checked = CheckType(FileTypes.File_M2V)
    FindNode(tvAssoc.Nodes(0), "nodeMP3").Checked = CheckType(FileTypes.File_MP3)
    FindNode(tvAssoc.Nodes(0), "nodeMPA").Checked = CheckType(FileTypes.File_MPA)
    FindNode(tvAssoc.Nodes(0), "nodeAAC").Checked = CheckType(FileTypes.File_AAC)
    FindNode(tvAssoc.Nodes(0), "nodeM2TS").Checked = CheckType(FileTypes.File_M2TS)
    FindNode(tvAssoc.Nodes(0), "nodeM4A").Checked = CheckType(FileTypes.File_M4A)
    FindNode(tvAssoc.Nodes(0), "nodeM4P").Checked = CheckType(FileTypes.File_M4P)
    FindNode(tvAssoc.Nodes(0), "nodeM4V").Checked = CheckType(FileTypes.File_M4V)
    CheckParents(FindNode(tvAssoc.Nodes(0), "nodeM4V"))
    FindNode(tvAssoc.Nodes(0), "nodeOGG").Checked = CheckType(FileTypes.File_OGG)
    FindNode(tvAssoc.Nodes(0), "nodeOGM").Checked = CheckType(FileTypes.File_OGM)
    CheckParents(FindNode(tvAssoc.Nodes(0), "nodeOGM"))
    FindNode(tvAssoc.Nodes(0), "nodeMKV").Checked = CheckType(FileTypes.File_MKV)
    FindNode(tvAssoc.Nodes(0), "nodeMKA").Checked = CheckType(FileTypes.File_MKA)
    CheckParents(FindNode(tvAssoc.Nodes(0), "nodeMKA"))
    FindNode(tvAssoc.Nodes(0), "nodeMOV").Checked = CheckType(FileTypes.File_MOV)
    FindNode(tvAssoc.Nodes(0), "nodeDV").Checked = CheckType(FileTypes.File_DV)
    FindNode(tvAssoc.Nodes(0), "node3GPP").Checked = CheckType(FileTypes.File_3GP)
    FindNode(tvAssoc.Nodes(0), "node3G2").Checked = CheckType(FileTypes.File_3G2)
    CheckParents(FindNode(tvAssoc.Nodes(0), "node3G2"))
    FindNode(tvAssoc.Nodes(0), "nodeRA").Checked = CheckType(FileTypes.File_RA)
    FindNode(tvAssoc.Nodes(0), "nodeRM").Checked = CheckType(FileTypes.File_RM)
    FindNode(tvAssoc.Nodes(0), "nodeRV").Checked = CheckType(FileTypes.File_RV)
    CheckParents(FindNode(tvAssoc.Nodes(0), "nodeRV"))
    FindNode(tvAssoc.Nodes(0), "nodeVFW").Checked = CheckType(FileTypes.File_VFW)
    FindNode(tvAssoc.Nodes(0), "nodeWMP").Checked = CheckType(FileTypes.File_WMP)
    FindNode(tvAssoc.Nodes(0), "nodeWM").Checked = CheckType(FileTypes.File_WM)
    FindNode(tvAssoc.Nodes(0), "nodeWMA").Checked = CheckType(FileTypes.File_WMA)
    FindNode(tvAssoc.Nodes(0), "nodeWMV").Checked = CheckType(FileTypes.File_WMV)
    CheckParents(FindNode(tvAssoc.Nodes(0), "nodeWMV"))
    FindNode(tvAssoc.Nodes(0), "nodeM3U").Checked = CheckType(FileTypes.File_M3U)
    FindNode(tvAssoc.Nodes(0), "nodePLS").Checked = CheckType(FileTypes.File_PLS)
    FindNode(tvAssoc.Nodes(0), "nodeLLPL").Checked = CheckType(FileTypes.File_LLPL)
    CheckParents(FindNode(tvAssoc.Nodes(0), "nodeLLPL"))
    FindNode(tvAssoc.Nodes(0), "nodeDIR").Checked = CheckType(FileTypes.File_Directory)
    CheckParents(FindNode(tvAssoc.Nodes(0), "nodeDIR"))
    tvAssoc.Nodes(0).Expand()
  End Sub

  Private Function CheckType(fType As FileTypes) As Boolean
    Select Case fType
      Case FileTypes.File_MPC
        CheckType = Chk_Associate("MPC")
      Case FileTypes.File_AC3
        CheckType = Chk_Associate("AC3")
      Case FileTypes.File_AIF
        CheckType = Chk_Associate("AIF", "AIFC", "AIFF")
      Case FileTypes.File_ASF
        CheckType = Chk_Associate("ASF", "ASX")
      Case FileTypes.File_AU
        CheckType = Chk_Associate("AU", "SND")
      Case FileTypes.File_MIDI
        CheckType = Chk_Associate("MID", "MIDI", "RMI")
      Case FileTypes.File_APE
        CheckType = Chk_Associate("APE")
      Case FileTypes.File_FLAC
        CheckType = Chk_Associate("FLAC")
      Case FileTypes.File_OFR
        CheckType = Chk_Associate("OFR")
      Case FileTypes.File_TTA
        CheckType = Chk_Associate("TTA")
      Case FileTypes.File_WAV
        CheckType = Chk_Associate("WAV")
      Case FileTypes.File_AVI
        CheckType = Chk_Associate("AVI")
      Case FileTypes.File_DIVX
        CheckType = Chk_Associate("DIVX")
      Case FileTypes.File_IVF
        CheckType = Chk_Associate("IVF")
      Case FileTypes.File_FLV
        CheckType = Chk_Associate("FLV")
      Case FileTypes.File_SPL
        CheckType = Chk_Associate("SPL")
      Case FileTypes.File_SWF
        CheckType = Chk_Associate("SWF")
      Case FileTypes.File_CDA
        CheckType = Chk_Associate("CDA", "AudioCD")
      Case FileTypes.File_DAT
        CheckType = Chk_Associate("DAT")
      Case FileTypes.File_VOB
        CheckType = Chk_Associate("VOB", "DVD")
      Case FileTypes.File_MPE
        CheckType = Chk_Associate("MPE")
      Case FileTypes.File_MPG
        CheckType = Chk_Associate("MPG", "MPEG")
      Case FileTypes.File_M1V
        CheckType = Chk_Associate("M1V")
      Case FileTypes.File_MP2
        CheckType = Chk_Associate("MP2")
      Case FileTypes.File_M2V
        CheckType = Chk_Associate("M2V", "MP2V", "MPV2")
      Case FileTypes.File_MP3
        CheckType = Chk_Associate("MP3")
      Case FileTypes.File_MPA
        CheckType = Chk_Associate("MPA")
      Case FileTypes.File_AAC
        CheckType = Chk_Associate("AAC")
      Case FileTypes.File_M2TS
        CheckType = Chk_Associate("M2TS")
      Case FileTypes.File_M4A
        CheckType = Chk_Associate("M4A")
      Case FileTypes.File_M4P
        CheckType = Chk_Associate("M4P")
      Case FileTypes.File_M4V
        CheckType = Chk_Associate("M4V", "MP4")
      Case FileTypes.File_OGG
        CheckType = Chk_Associate("OGG")
      Case FileTypes.File_OGM
        CheckType = Chk_Associate("OGM")
      Case FileTypes.File_MKV
        CheckType = Chk_Associate("MKV")
      Case FileTypes.File_MKA
        CheckType = Chk_Associate("MKA")
      Case FileTypes.File_MOV
        CheckType = Chk_Associate("MOV", "QT")
      Case FileTypes.File_DV
        CheckType = Chk_Associate("DV", "DIF")
      Case FileTypes.File_3GP
        CheckType = Chk_Associate("3GP", "3GPP")
      Case FileTypes.File_3G2
        CheckType = Chk_Associate("3G2", "3GP2")
      Case FileTypes.File_RA
        CheckType = Chk_Associate("RA", "RAM")
      Case FileTypes.File_RM
        CheckType = Chk_Associate("RM", "RMM")
      Case FileTypes.File_RV
        CheckType = Chk_Associate("RV")
      Case FileTypes.File_VFW
        CheckType = Chk_Associate("VFW")
      Case FileTypes.File_WMP
        CheckType = Chk_Associate("WMP")
      Case FileTypes.File_WM
        CheckType = Chk_Associate("WM", "WMX")
      Case FileTypes.File_WMA
        CheckType = Chk_Associate("WMA", "WAX")
      Case FileTypes.File_WMV
        CheckType = Chk_Associate("WMV", "WVX")
      Case FileTypes.File_M3U
        CheckType = Chk_Associate("M3U")
      Case FileTypes.File_PLS
        CheckType = Chk_Associate("PLS")
      Case FileTypes.File_LLPL
        CheckType = Chk_Associate("LLPL")
      Case FileTypes.File_Directory
        CheckType = Chk_Associate("Directory")
      Case Else
        CheckType = False
    End Select
  End Function

  Private Function Chk_Associate(ParamArray sType()) As Boolean
    Dim I As Integer
    Dim sTemp As String
    Dim bRet As Byte
    '0 = no
    '1 = yes
    '2 = both
    '3 = not set
    bRet = 3
    For I = 0 To UBound(sType)
      If My.Computer.Registry.ClassesRoot.OpenSubKey(sType(I) & "\shell\Play\command") IsNot Nothing Then
        sTemp = My.Computer.Registry.ClassesRoot.OpenSubKey(sType(I) & "\shell\Play\command").GetValue(String.Empty)
        If String.Compare(sTemp, """" & Application.ExecutablePath & """ ""%1""", True) = 0 Then
          If bRet = 0 Then
            bRet = 2
          Else
            bRet = 1
          End If
        ElseIf String.Compare(sTemp, """" & Application.ExecutablePath & """ /" & sType(I) & " ""%1""", True) = 0 Then
          If bRet = 0 Then
            bRet = 2
          Else
            bRet = 1
          End If
        Else
          If bRet = 1 Then
            bRet = 2
          Else
            bRet = 0
          End If
        End If
      Else
        bRet = 0
      End If
    Next I
    Return bRet = 1 Or bRet = 2
  End Function

  Private Sub CheckParents(Node As TreeNode)
    Dim bChk As Byte
    If Node IsNot Nothing AndAlso Node.Parent IsNot Nothing Then
      For I As Integer = 0 To Node.Parent.Nodes.Count - 1
        If Node.Parent.Nodes(I).Checked Then
          If bChk = 0 Then bChk = 1
        Else
          bChk = 2
        End If
      Next
      Node.Parent.Checked = (bChk = 1)
      CheckParents(Node.Parent)
    End If
  End Sub

  Private Sub CheckChildren(Node As TreeNode)
    For I As Integer = 0 To Node.Nodes.Count - 1
      Node.Nodes(I).Checked = Node.Checked
      If Node.Nodes(I).Nodes.Count > 0 Then CheckChildren(Node.Nodes(I))
    Next
  End Sub

  Private Sub tvAssoc_AfterCheck(sender As Object, e As System.Windows.Forms.TreeViewEventArgs) Handles tvAssoc.AfterCheck
    If e.Action = TreeViewAction.ByKeyboard Or e.Action = TreeViewAction.ByMouse Then
      If e.Node.Parent Is Nothing Then
        CheckChildren(e.Node)
      Else
        CheckChildren(e.Node)
        CheckParents(e.Node)
      End If
    End If
  End Sub

  Private Sub cmdAssociate_Click(sender As System.Object, e As System.EventArgs) Handles cmdAssociate.Click
    Dim assocList As String = String.Empty
    ChildTags(tvAssoc.Nodes(0), assocList)
    If chkThumbnails.Checked Then assocList &= " THUMB "
    If My.Computer.FileSystem.FileExists(Application.StartupPath & "\LSFA.exe") Then
      Dim X As New Process
      X.StartInfo = New ProcessStartInfo(Application.StartupPath & "\LSFA.exe", "Associate:" & assocList & " ")
      X.Start()
      X.WaitForExit()
      MsgBox("The selected file types have been associated with Lime Seed.", MsgBoxStyle.Information, "Associations Set")
    Else
      MsgBox("Unable to find Lime Seed File Association Tool! Please reinstall Lime Seed.", MsgBoxStyle.Critical, "File Association Tool Missing")
    End If
  End Sub

  Private Sub cmdKeyDefaults_Click(sender As System.Object, e As System.EventArgs) Handles cmdKeyDefaults.Click
    txtKeyOpen.Text = "Ctrl + O"
    txtKeyClose.Text = "Ctrl + C"
    txtKeyFileProperties.Text = "Ctrl + P"
    txtKeySettings.Text = "Ctrl + S"
    txtKeyWebpage.Text = "Ctrl + W"
    txtKeyAbout.Text = "Ctrl + F1"
    txtKeyDVDMenu.Text = "Alt + Spacebar"
    txtKeyDiscEject.Text = "Down Arrow"
    txtKeyPlayPause.Text = "Spacebar"
    txtKeyStop.Text = "Escape"
    txtKeyLast.Text = "Home"
    txtKeyNext.Text = "End"
    txtKeyMute.Text = "Ctrl + M"
    txtKeyFullScreen.Text = "Shift + Enter"
    txtKeyVolUp.Text = "Page Up"
    txtKeyVolDown.Text = "Page Down"
    txtKeySkipBack.Text = "Left Arrow"
    txtKeySkipFwd.Text = "Right Arrow"
    txtKeyAddToPL.Text = "Insert"
    txtKeyRemoveFromPL.Text = "Delete"
    txtKeyClearPL.Text = "Shift + Delete"
    txtKeySavePL.Text = "None"
    txtKeyOpenPL.Text = "None"
    txtKeyShuffle.Text = "None"
    txtKeyRepeatTrack.Text = "None"
    txtKeyRepeatPL.Text = "None"
    txtKeyRenamePL.Text = "Ctrl + F2"
  End Sub

  Private Sub cmdPadDefaults_Click(sender As System.Object, e As System.EventArgs) Handles cmdPadDefaults.Click
    txtPadOpen.Text = "Button 10"
    txtPadClose.Text = "None"
    txtPadProps.Text = "None"
    txtPadSettings.Text = "None"
    txtPadWebpage.Text = "None"
    txtPadAbout.Text = "None"
    txtPadDVDMenu.Text = "Button 9"
    txtPadDiscEject.Text = "None"
    txtPadPlayPause.Text = "Button 2"
    txtPadStop.Text = "Button 4"
    txtPadLast.Text = "Button 5"
    txtPadNext.Text = "Button 6"
    txtPadMute.Text = "Button 3"
    txtPadFullScreen.Text = "Button 1"
    txtPadVolUp.Text = "Y Axis Top"
    txtPadVolDown.Text = "Y Axis Bottom"
    txtPadSkipBack.Text = "X Axis Left"
    txtPadSkipFwd.Text = "X Axis Right"
    txtPadAddToPL.Text = "None"
    txtPadRemoveFromPL.Text = "None"
    txtPadClearPL.Text = "None"
    txtPadSavePL.Text = "None"
    txtPadOpenPL.Text = "None"
    txtPadShuffle.Text = "None"
    txtPadRepeatTrack.Text = "None"
    txtPadRepeatPL.Text = "None"
    txtPadRenamePL.Text = "None"
  End Sub

  Private Sub txtKey_DoubleClick(sender As Object, e As System.EventArgs) Handles txtKeyOpen.DoubleClick, txtKeyClose.DoubleClick, txtKeyFileProperties.DoubleClick, txtKeySettings.DoubleClick, txtKeyWebpage.DoubleClick, txtKeyAbout.DoubleClick, txtKeyPlayPause.DoubleClick, txtKeyStop.DoubleClick, txtKeyLast.DoubleClick, txtKeyNext.DoubleClick, txtKeyMute.DoubleClick, txtKeyFullScreen.DoubleClick, txtKeyVolUp.DoubleClick, txtKeyVolDown.DoubleClick, txtKeySkipBack.DoubleClick, txtKeySkipFwd.DoubleClick, txtKeyAddToPL.DoubleClick, txtKeyRemoveFromPL.DoubleClick, txtKeyClearPL.DoubleClick, txtKeySavePL.DoubleClick, txtKeyOpenPL.DoubleClick, txtKeyShuffle.DoubleClick, txtKeyRepeatTrack.DoubleClick, txtKeyRepeatPL.DoubleClick, txtKeyRenamePL.DoubleClick
    sender.Text = "None"
  End Sub

  Private Sub txtKey_GotFocus(sender As Object, e As System.EventArgs) Handles txtKeyOpen.GotFocus, txtKeyClose.GotFocus, txtKeyFileProperties.GotFocus, txtKeySettings.GotFocus, txtKeyWebpage.GotFocus, txtKeyAbout.GotFocus, txtKeyPlayPause.GotFocus, txtKeyStop.GotFocus, txtKeyLast.GotFocus, txtKeyNext.GotFocus, txtKeyMute.GotFocus, txtKeyFullScreen.GotFocus, txtKeyVolUp.GotFocus, txtKeyVolDown.GotFocus, txtKeySkipBack.GotFocus, txtKeySkipFwd.GotFocus, txtKeyAddToPL.GotFocus, txtKeyRemoveFromPL.GotFocus, txtKeyClearPL.GotFocus, txtKeySavePL.GotFocus, txtKeyOpenPL.GotFocus, txtKeyShuffle.GotFocus, txtKeyRepeatTrack.GotFocus, txtKeyRepeatPL.GotFocus, txtKeyRenamePL.GotFocus
    interceptKey = True
    sender.Tag = sender.Text
    sender.Text = String.Empty
  End Sub

  Private Sub txtKey_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtKeyOpen.KeyDown, txtKeyClose.KeyDown, txtKeyFileProperties.KeyDown, txtKeySettings.KeyDown, txtKeyWebpage.KeyDown, txtKeyAbout.KeyDown, txtKeyPlayPause.KeyDown, txtKeyStop.KeyDown, txtKeyLast.KeyDown, txtKeyNext.KeyDown, txtKeyMute.KeyDown, txtKeyFullScreen.KeyDown, txtKeyVolUp.KeyDown, txtKeyVolDown.KeyDown, txtKeySkipBack.KeyDown, txtKeySkipFwd.KeyDown, txtKeyAddToPL.KeyDown, txtKeyRemoveFromPL.KeyDown, txtKeyClearPL.KeyDown, txtKeySavePL.KeyDown, txtKeyOpenPL.KeyDown, txtKeyShuffle.KeyDown, txtKeyRepeatTrack.KeyDown, txtKeyRepeatPL.KeyDown, txtKeyRenamePL.KeyDown
    e.Handled = True
    e.SuppressKeyPress = True
    If e.KeyCode = Keys.Tab Then
      Stop
    End If
  End Sub

  Private Sub txtKey_KeyUp(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtKeyOpen.KeyUp, txtKeyClose.KeyUp, txtKeyFileProperties.KeyUp, txtKeySettings.KeyUp, txtKeyWebpage.KeyUp, txtKeyAbout.KeyUp, txtKeyPlayPause.KeyUp, txtKeyStop.KeyUp, txtKeyLast.KeyUp, txtKeyNext.KeyUp, txtKeyMute.KeyUp, txtKeyFullScreen.KeyUp, txtKeyVolUp.KeyUp, txtKeyVolDown.KeyUp, txtKeySkipBack.KeyUp, txtKeySkipFwd.KeyUp, txtKeyAddToPL.KeyUp, txtKeyRemoveFromPL.KeyUp, txtKeyClearPL.KeyUp, txtKeySavePL.KeyUp, txtKeyOpenPL.KeyUp, txtKeyShuffle.KeyUp, txtKeyRepeatTrack.KeyUp, txtKeyRepeatPL.KeyUp, txtKeyRenamePL.KeyUp
    If String.IsNullOrEmpty(KeyToStr(e.KeyCode)) Then Exit Sub
    sender.Text = String.Empty
    'If sender.SelectionLength > 0 Then sender.SelectedText = vbNullString
    If (e.Modifiers And Keys.Control) = Keys.Control Then sender.SelectedText = "Ctrl"
    If sender.Text.Length > 0 Then
      If (e.Modifiers And Keys.Alt) = Keys.Alt Then sender.SelectedText &= " + Alt"
    Else
      If (e.Modifiers And Keys.Alt) = Keys.Alt Then sender.SelectedText = "Alt"
    End If
    If sender.Text.Length > 0 Then
      If (e.Modifiers And Keys.Shift) = Keys.Shift Then sender.SelectedText &= " + Shift"
    Else
      If (e.Modifiers And Keys.Shift) = Keys.Shift Then sender.SelectedText = "Shift"
    End If
    If sender.Text.Length > 0 Then
      sender.SelectedText &= " + " & KeyToStr(e.KeyCode)
    Else
      sender.SelectedText = KeyToStr(e.KeyCode)
    End If
    e.Handled = True
    e.SuppressKeyPress = True
    sender.SelectionLength = 0
  End Sub

  Private Sub txtKey_LostFocus(sender As Object, e As System.EventArgs) Handles txtKeyOpen.LostFocus, txtKeyClose.LostFocus, txtKeyFileProperties.LostFocus, txtKeySettings.LostFocus, txtKeyWebpage.LostFocus, txtKeyAbout.LostFocus, txtKeyPlayPause.LostFocus, txtKeyStop.LostFocus, txtKeyLast.LostFocus, txtKeyNext.LostFocus, txtKeyMute.LostFocus, txtKeyFullScreen.LostFocus, txtKeyVolUp.LostFocus, txtKeyVolDown.LostFocus, txtKeySkipBack.LostFocus, txtKeySkipFwd.LostFocus, txtKeyAddToPL.LostFocus, txtKeyRemoveFromPL.LostFocus, txtKeyClearPL.LostFocus, txtKeySavePL.LostFocus, txtKeyOpenPL.LostFocus, txtKeyShuffle.LostFocus, txtKeyRepeatTrack.LostFocus, txtKeyRepeatPL.LostFocus, txtKeyRenamePL.LostFocus
    interceptKey = False
    Dim sendT As TextBox = sender
    If ((Not IsNothing(sendT.Tag)) AndAlso (Not String.IsNullOrEmpty(sendT.Tag))) And String.IsNullOrEmpty(sendT.Text) Then
      sendT.Text = sender.Tag
      sendT.Tag = Nothing
    ElseIf sender.Text = vbNullString Then
      sendT.Text = "None"
      sendT.Tag = Nothing
    End If
  End Sub

  Private Sub StrToKeys(ByVal sKeys As String, ByRef cKeys As Collection)
    cKeys.Clear()
    Dim sKeyList() As String = Split(sKeys, " + ")
    For Each sKey As String In sKeyList
      Select Case sKey
        Case "A" : cKeys.Add(Keys.A)
        Case "Add" : cKeys.Add(Keys.Add)
        Case "Alt" : cKeys.Add(Keys.Menu)
        Case "Apps" : cKeys.Add(Keys.Apps)
        Case "Attn" : cKeys.Add(Keys.Attn)
        Case "B" : cKeys.Add(Keys.B)
        Case "Backspace" : cKeys.Add(Keys.Back)
        Case "Browser Back" : cKeys.Add(Keys.BrowserBack)
        Case "Browser Favorites" : cKeys.Add(Keys.BrowserFavorites)
        Case "Browser Forward" : cKeys.Add(Keys.BrowserForward)
        Case "Browser Home" : cKeys.Add(Keys.BrowserHome)
        Case "Browser Refresh" : cKeys.Add(Keys.BrowserRefresh)
        Case "Browser Search" : cKeys.Add(Keys.BrowserSearch)
        Case "Browser Stop" : cKeys.Add(Keys.BrowserStop)
        Case "C" : cKeys.Add(Keys.C)
        Case "Cancel" : cKeys.Add(Keys.Cancel)
        Case "CapsLock" : cKeys.Add(Keys.CapsLock)
        Case "Clear" : cKeys.Add(Keys.Clear)
        Case "Ctrl" : cKeys.Add(Keys.ControlKey)
        Case "CrSel" : cKeys.Add(Keys.Crsel)
        Case "D" : cKeys.Add(Keys.D)
        Case "0" : cKeys.Add(Keys.D0)
        Case "1" : cKeys.Add(Keys.D1)
        Case "2" : cKeys.Add(Keys.D2)
        Case "3" : cKeys.Add(Keys.D3)
        Case "4" : cKeys.Add(Keys.D4)
        Case "5" : cKeys.Add(Keys.D5)
        Case "6" : cKeys.Add(Keys.D6)
        Case "7" : cKeys.Add(Keys.D7)
        Case "8" : cKeys.Add(Keys.D8)
        Case "9" : cKeys.Add(Keys.D9)
        Case "Decimal" : cKeys.Add(Keys.Decimal)
        Case "Delete" : cKeys.Add(Keys.Delete)
        Case "Divide" : cKeys.Add(Keys.Divide)
        Case "Down Arrow" : cKeys.Add(Keys.Down)
        Case "E" : cKeys.Add(Keys.E)
        Case "End" : cKeys.Add(Keys.End)
        Case "Enter" : cKeys.Add(Keys.Enter)
        Case "EraseEOF" : cKeys.Add(Keys.EraseEof)
        Case "Escape" : cKeys.Add(Keys.Escape)
        Case "Execute" : cKeys.Add(Keys.Execute)
        Case "ExSel" : cKeys.Add(Keys.Exsel)
        Case "F" : cKeys.Add(Keys.F)
        Case "F1" : cKeys.Add(Keys.F1)
        Case "F2" : cKeys.Add(Keys.F2)
        Case "F3" : cKeys.Add(Keys.F3)
        Case "F4" : cKeys.Add(Keys.F4)
        Case "F5" : cKeys.Add(Keys.F5)
        Case "F6" : cKeys.Add(Keys.F6)
        Case "F7" : cKeys.Add(Keys.F7)
        Case "F8" : cKeys.Add(Keys.F8)
        Case "F9" : cKeys.Add(Keys.F9)
        Case "F10" : cKeys.Add(Keys.F10)
        Case "F11" : cKeys.Add(Keys.F11)
        Case "F12" : cKeys.Add(Keys.F12)
        Case "F13" : cKeys.Add(Keys.F13)
        Case "F14" : cKeys.Add(Keys.F14)
        Case "F15" : cKeys.Add(Keys.F15)
        Case "F16" : cKeys.Add(Keys.F16)
        Case "F17" : cKeys.Add(Keys.F17)
        Case "F18" : cKeys.Add(Keys.F18)
        Case "F19" : cKeys.Add(Keys.F19)
        Case "F20" : cKeys.Add(Keys.F20)
        Case "F21" : cKeys.Add(Keys.F21)
        Case "F22" : cKeys.Add(Keys.F22)
        Case "F23" : cKeys.Add(Keys.F23)
        Case "F24" : cKeys.Add(Keys.F24)
        Case "IME Final Mode" : cKeys.Add(Keys.FinalMode)
        Case "G" : cKeys.Add(Keys.G)
        Case "H" : cKeys.Add(Keys.H)
        Case "IME Hangul Mode" : cKeys.Add(Keys.HangulMode)
        Case "IME Hanja Mode" : cKeys.Add(Keys.HanjaMode)
        Case "Help" : cKeys.Add(Keys.Help)
        Case "Home" : cKeys.Add(Keys.Home)
        Case "I" : cKeys.Add(Keys.I)
        Case "IME Accept" : cKeys.Add(Keys.IMEAccept)
        Case "IME Convert" : cKeys.Add(Keys.IMEConvert)
        Case "IME Mode Change" : cKeys.Add(Keys.IMEModeChange)
        Case "IME Nonconvert" : cKeys.Add(Keys.IMENonconvert)
        Case "Insert" : cKeys.Add(Keys.Insert)
        Case "J" : cKeys.Add(Keys.J)
        Case "IME Junja Mode" : cKeys.Add(Keys.JunjaMode)
        Case "K" : cKeys.Add(Keys.K)
        Case "IME Kana Mode" : cKeys.Add(Keys.KanaMode)
        Case "IME Kanji Mode" : cKeys.Add(Keys.KanjiMode)
        Case "L" : cKeys.Add(Keys.L)
        Case "Launch App1" : cKeys.Add(Keys.LaunchApplication1)
        Case "Launch App2" : cKeys.Add(Keys.LaunchApplication2)
        Case "Launch Mail" : cKeys.Add(Keys.LaunchMail)
        Case "Left Ctrl" : cKeys.Add(Keys.LControlKey)
        Case "Left Arrow" : cKeys.Add(Keys.Left)
        Case "Line Feed" : cKeys.Add(Keys.LineFeed)
        Case "Left Alt" : cKeys.Add(Keys.LMenu)
        Case "Left Shift" : cKeys.Add(Keys.LShiftKey)
        Case "Left Win" : cKeys.Add(Keys.LWin)
        Case "M" : cKeys.Add(Keys.M)
        Case "Media Next Track" : cKeys.Add(Keys.MediaNextTrack)
        Case "Media Play/Pause" : cKeys.Add(Keys.MediaPlayPause)
        Case "Media Previous Track" : cKeys.Add(Keys.MediaPreviousTrack)
        Case "Media Stop" : cKeys.Add(Keys.MediaStop)
        Case "Multiply" : cKeys.Add(Keys.Multiply)
        Case "N" : cKeys.Add(Keys.N)
        Case "NumLock" : cKeys.Add(Keys.NumLock)
        Case "NumPad 0" : cKeys.Add(Keys.NumPad0)
        Case "NumPad 1" : cKeys.Add(Keys.NumPad1)
        Case "NumPad 2" : cKeys.Add(Keys.NumPad2)
        Case "NumPad 3" : cKeys.Add(Keys.NumPad3)
        Case "NumPad 4" : cKeys.Add(Keys.NumPad4)
        Case "NumPad 5" : cKeys.Add(Keys.NumPad5)
        Case "NumPad 6" : cKeys.Add(Keys.NumPad6)
        Case "NumPad 7" : cKeys.Add(Keys.NumPad7)
        Case "NumPad 8" : cKeys.Add(Keys.NumPad8)
        Case "NumPad 9" : cKeys.Add(Keys.NumPad9)
        Case "O" : cKeys.Add(Keys.O)
        Case "OEM 1" : cKeys.Add(Keys.Oem1)
        Case "OEM 102" : cKeys.Add(Keys.Oem102)
        Case "OEM 2" : cKeys.Add(Keys.Oem2)
        Case "OEM 3" : cKeys.Add(Keys.Oem3)
        Case "OEM 4" : cKeys.Add(Keys.Oem4)
        Case "OEM 5" : cKeys.Add(Keys.Oem5)
        Case "OEM 6" : cKeys.Add(Keys.Oem6)
        Case "OEM 7" : cKeys.Add(Keys.Oem7)
        Case "OEM 8" : cKeys.Add(Keys.Oem8)
        Case "OEM \" : cKeys.Add(Keys.OemBackslash)
        Case "OEM Clear" : cKeys.Add(Keys.OemClear)
        Case "OEM Close Brackets" : cKeys.Add(Keys.OemCloseBrackets)
        Case "OEM ," : cKeys.Add(Keys.Oemcomma)
        Case "OEM -" : cKeys.Add(Keys.OemMinus)
        Case "OEM Open Brackets" : cKeys.Add(Keys.OemOpenBrackets)
        Case "OEM ." : cKeys.Add(Keys.OemPeriod)
        Case "OEM |" : cKeys.Add(Keys.OemPipe)
        Case "OEM Plus" : cKeys.Add(Keys.Oemplus)
        Case "OEM ?" : cKeys.Add(Keys.OemQuestion)
        Case "OEM Quotes" : cKeys.Add(Keys.OemQuotes)
        Case "OEM ;" : cKeys.Add(Keys.OemSemicolon)
        Case "OEM ~" : cKeys.Add(Keys.Oemtilde)
        Case "P" : cKeys.Add(Keys.P)
        Case "Pa1" : cKeys.Add(Keys.Pa1)
        Case "Page Down" : cKeys.Add(Keys.PageDown)
        Case "Page Up" : cKeys.Add(Keys.PageUp)
        Case "Pause" : cKeys.Add(Keys.Pause)
        Case "Play" : cKeys.Add(Keys.Play)
        Case "Print" : cKeys.Add(Keys.Print)
        Case "Print Screen" : cKeys.Add(Keys.PrintScreen)
        Case "Process" : cKeys.Add(Keys.ProcessKey)
        Case "Q" : cKeys.Add(Keys.Q)
        Case "R" : cKeys.Add(Keys.R)
        Case "Right Ctrl" : cKeys.Add(Keys.RControlKey)
        Case "Right Arrow" : cKeys.Add(Keys.Right)
        Case "Right Alt" : cKeys.Add(Keys.RMenu)
        Case "Right Shift" : cKeys.Add(Keys.RShiftKey)
        Case "Right Win" : cKeys.Add(Keys.RWin)
        Case "S" : cKeys.Add(Keys.S)
        Case "ScrollLock" : cKeys.Add(Keys.Scroll)
        Case "Select" : cKeys.Add(Keys.Select)
        Case "Select Media" : cKeys.Add(Keys.SelectMedia)
        Case "Seperator" : cKeys.Add(Keys.Separator)
        Case "Shift" : cKeys.Add(Keys.ShiftKey)
        Case "Sleep" : cKeys.Add(Keys.Sleep)
        Case "Spacebar" : cKeys.Add(Keys.Space)
        Case "Subtract" : cKeys.Add(Keys.Subtract)
        Case "T" : cKeys.Add(Keys.T)
        Case "Tab" : cKeys.Add(Keys.Tab)
        Case "U" : cKeys.Add(Keys.U)
        Case "Up Arrow" : cKeys.Add(Keys.Up)
        Case "V" : cKeys.Add(Keys.V)
        Case "Volume Down" : cKeys.Add(Keys.VolumeDown)
        Case "Volume Mute" : cKeys.Add(Keys.VolumeMute)
        Case "Volume Up" : cKeys.Add(Keys.VolumeUp)
        Case "W" : cKeys.Add(Keys.W)
        Case "X" : cKeys.Add(Keys.X)
        Case "Y" : cKeys.Add(Keys.Y)
        Case "Z" : cKeys.Add(Keys.Z)
        Case "Zoom" : cKeys.Add(Keys.Zoom)
      End Select
    Next
  End Sub

  Private Function KeyToStr(ByRef Key As Keys) As String
    Select Case Key
      Case Keys.A : Return "A"
      Case Keys.Add : Return "Add"
      Case Keys.Apps : Return "App Menu"
      Case Keys.Attn : Return "Attn"
      Case Keys.B : Return "B"
      Case Keys.Back : Return "Backspace"
      Case Keys.BrowserBack : Return "Browser Back"
      Case Keys.BrowserFavorites : Return "Browser Favorites"
      Case Keys.BrowserForward : Return "Browser Forward"
      Case Keys.BrowserHome : Return "Browser Home"
      Case Keys.BrowserRefresh : Return "Browser Refresh"
      Case Keys.BrowserSearch : Return "Browser Search"
      Case Keys.BrowserStop : Return "Browser Stop"
      Case Keys.C : Return "C"
      Case Keys.Cancel : Return "Cancel"
      Case Keys.CapsLock : Return "CapsLock"
      Case Keys.Clear : Return "Clear"
      Case Keys.Crsel : Return "CrSel"
      Case Keys.D : Return "D"
      Case Keys.D0 : Return "0"
      Case Keys.D1 : Return "1"
      Case Keys.D2 : Return "2"
      Case Keys.D3 : Return "3"
      Case Keys.D4 : Return "4"
      Case Keys.D5 : Return "5"
      Case Keys.D6 : Return "6"
      Case Keys.D7 : Return "7"
      Case Keys.D8 : Return "8"
      Case Keys.D9 : Return "9"
      Case Keys.Decimal : Return "Decimal"
      Case Keys.Delete : Return "Delete"
      Case Keys.Divide : Return "Divide"
      Case Keys.Down : Return "Down Arrow"
      Case Keys.E : Return "E"
      Case Keys.End : Return "End"
      Case Keys.Enter : Return "Enter"
      Case Keys.EraseEof : Return "EraseEOF"
      Case Keys.Escape : Return "Escape"
      Case Keys.Execute : Return "Execute"
      Case Keys.Exsel : Return "ExSel"
      Case Keys.F : Return "F"
      Case Keys.F1 : Return "F1"
      Case Keys.F2 : Return "F2"
      Case Keys.F3 : Return "F3"
      Case Keys.F4 : Return "F4"
      Case Keys.F5 : Return "F5"
      Case Keys.F6 : Return "F6"
      Case Keys.F7 : Return "F7"
      Case Keys.F8 : Return "F8"
      Case Keys.F9 : Return "F9"
      Case Keys.F10 : Return "F10"
      Case Keys.F11 : Return "F11"
      Case Keys.F12 : Return "F12"
      Case Keys.F13 : Return "F13"
      Case Keys.F14 : Return "F14"
      Case Keys.F15 : Return "F15"
      Case Keys.F16 : Return "F16"
      Case Keys.F17 : Return "F17"
      Case Keys.F18 : Return "F18"
      Case Keys.F19 : Return "F19"
      Case Keys.F20 : Return "F20"
      Case Keys.F21 : Return "F21"
      Case Keys.F22 : Return "F22"
      Case Keys.F23 : Return "F23"
      Case Keys.F24 : Return "F24"
      Case Keys.FinalMode : Return "IME Final Mode"
      Case Keys.G : Return "G"
      Case Keys.H : Return "H"
      Case Keys.HangulMode : Return "IME Hangul Mode"
      Case Keys.HanjaMode : Return "IME Hanja Mode"
      Case Keys.Help : Return "Help"
      Case Keys.Home : Return "Home"
      Case Keys.I : Return "I"
      Case Keys.IMEAccept : Return "IME Accept"
      Case Keys.IMEConvert : Return "IME Convert"
      Case Keys.IMEModeChange : Return "IME Mode Change"
      Case Keys.IMENonconvert : Return "IME Nonconvert"
      Case Keys.Insert : Return "Insert"
      Case Keys.J : Return "J"
      Case Keys.JunjaMode : Return "IME Junja Mode"
      Case Keys.K : Return "K"
      Case Keys.KanaMode : Return "IME Kana Mode"
      Case Keys.KanjiMode : Return "IME Kanji Mode"
      Case Keys.L : Return "L"
      Case Keys.LaunchApplication1 : Return "Launch App1"
      Case Keys.LaunchApplication2 : Return "Launch App2"
      Case Keys.LaunchMail : Return "Launch Mail"
      Case Keys.LControlKey : Return "Left Ctrl"
      Case Keys.Left : Return "Left Arrow"
      Case Keys.LineFeed : Return "Line Feed"
      Case Keys.LMenu : Return "Left Alt"
      Case Keys.LShiftKey : Return "Left Shift"
      Case Keys.LWin : Return "Left Win"
      Case Keys.M : Return "M"
      Case Keys.MediaNextTrack : Return "Media Next Track"
      Case Keys.MediaPlayPause : Return "Media Play/Pause"
      Case Keys.MediaPreviousTrack : Return "Media Previous Track"
      Case Keys.MediaStop : Return "Media Stop"
      Case Keys.Multiply : Return "Multiply"
      Case Keys.N : Return "N"
      Case Keys.NumLock : Return "NumLock"
      Case Keys.NumPad0 : Return "NumPad 0"
      Case Keys.NumPad1 : Return "NumPad 1"
      Case Keys.NumPad2 : Return "NumPad 2"
      Case Keys.NumPad3 : Return "NumPad 3"
      Case Keys.NumPad4 : Return "NumPad 4"
      Case Keys.NumPad5 : Return "NumPad 5"
      Case Keys.NumPad6 : Return "NumPad 6"
      Case Keys.NumPad7 : Return "NumPad 7"
      Case Keys.NumPad8 : Return "NumPad 8"
      Case Keys.NumPad9 : Return "NumPad 9"
      Case Keys.O : Return "O"
      Case Keys.Oem8 : Return "OEM 8"
      Case Keys.OemBackslash : Return "Backslash"
      Case Keys.OemClear : Return "OEM Clear"
      Case Keys.OemCloseBrackets : Return "Close Brackets"
      Case Keys.Oemcomma : Return "Comma"
      Case Keys.OemMinus : Return "Minus"
      Case Keys.OemOpenBrackets : Return "Open Brackets"
      Case Keys.OemPeriod : Return "Period"
      Case Keys.OemPipe : Return "Pipe"
      Case Keys.Oemplus : Return "Plus"
      Case Keys.OemQuestion : Return "Question"
      Case Keys.OemQuotes : Return "Quotes"
      Case Keys.OemSemicolon : Return "Semicolon"
      Case Keys.Oemtilde : Return "Tilde"
      Case Keys.P : Return "P"
      Case Keys.Pa1 : Return "Pa1"
      Case Keys.PageDown : Return "Page Down"
      Case Keys.PageUp : Return "Page Up"
      Case Keys.Pause : Return "Pause"
      Case Keys.Play : Return "Play"
      Case Keys.Print : Return "Print"
      Case Keys.PrintScreen : Return "Print Screen"
      Case Keys.ProcessKey : Return "Process"
      Case Keys.Q : Return "Q"
      Case Keys.R : Return "R"
      Case Keys.RControlKey : Return "Right Ctrl"
      Case Keys.Right : Return "Right Arrow"
      Case Keys.RMenu : Return "Right Alt"
      Case Keys.RShiftKey : Return "Right Shift"
      Case Keys.RWin : Return "Right Win"
      Case Keys.S : Return "S"
      Case Keys.Scroll : Return "ScrollLock"
      Case Keys.Select : Return "Select"
      Case Keys.SelectMedia : Return "Select Media"
      Case Keys.Separator : Return "Seperator"
      Case Keys.Sleep : Return "Sleep"
      Case Keys.Space : Return "Spacebar"
      Case Keys.Subtract : Return "Subtract"
      Case Keys.T : Return "T"
      Case Keys.Tab : Return "Tab"
      Case Keys.U : Return "U"
      Case Keys.Up : Return "Up Arrow"
      Case Keys.V : Return "V"
      Case Keys.VolumeDown : Return "Volume Down"
      Case Keys.VolumeMute : Return "Volume Mute"
      Case Keys.VolumeUp : Return "Volume Up"
      Case Keys.W : Return "W"
      Case Keys.X : Return "X"
      Case Keys.Y : Return "Y"
      Case Keys.Z : Return "Z"
      Case Keys.Zoom : Return "Zoom"
      Case Else : Return Nothing
    End Select
  End Function

  Protected Overrides Function ProcessDialogKey(keyData As System.Windows.Forms.Keys) As Boolean
    If interceptKey Then
      Return True
    Else
      Return MyBase.ProcessDialogKey(keyData)
    End If
  End Function

  Protected Overrides Function ProcessTabKey(forward As Boolean) As Boolean
    If interceptKey Then
      Return True
    Else
      Return MyBase.ProcessTabKey(forward)
    End If
  End Function

  Private Sub txtPad_GotFocus(sender As Object, e As System.EventArgs) Handles txtPadOpen.GotFocus, txtPadClose.GotFocus, txtPadProps.GotFocus, txtPadSettings.GotFocus, txtPadWebpage.GotFocus, txtPadAbout.GotFocus, txtPadPlayPause.GotFocus, txtPadStop.GotFocus, txtPadLast.GotFocus, txtPadNext.GotFocus, txtPadMute.GotFocus, txtPadFullScreen.GotFocus, txtPadVolUp.GotFocus, txtPadVolDown.GotFocus, txtPadSkipBack.GotFocus, txtPadSkipFwd.GotFocus, txtPadAddToPL.GotFocus, txtPadRemoveFromPL.GotFocus, txtPadClearPL.GotFocus, txtPadSavePL.GotFocus, txtPadOpenPL.GotFocus, txtPadShuffle.GotFocus, txtPadRepeatTrack.GotFocus, txtPadRepeatPL.GotFocus, txtPadRenamePL.GotFocus
    cJoy = New clsJoyDetection
    cJoy.Tag = sender
    sender.Tag = sender.Text
    sender.Text = String.Empty
  End Sub

  Private Sub txtPadOpen_LostFocus(sender As Object, e As System.EventArgs) Handles txtPadOpen.LostFocus, txtPadClose.LostFocus, txtPadProps.LostFocus, txtPadSettings.LostFocus, txtPadWebpage.LostFocus, txtPadAbout.LostFocus, txtPadPlayPause.LostFocus, txtPadStop.LostFocus, txtPadLast.LostFocus, txtPadNext.LostFocus, txtPadMute.LostFocus, txtPadFullScreen.LostFocus, txtPadVolUp.LostFocus, txtPadVolDown.LostFocus, txtPadSkipBack.LostFocus, txtPadSkipFwd.LostFocus, txtPadAddToPL.LostFocus, txtPadRemoveFromPL.LostFocus, txtPadClearPL.LostFocus, txtPadSavePL.LostFocus, txtPadOpenPL.LostFocus, txtPadShuffle.LostFocus, txtPadRepeatTrack.LostFocus, txtPadRepeatPL.LostFocus, txtPadRenamePL.LostFocus
    cJoy = Nothing
    Dim sendT As TextBox = sender
    If ((Not IsNothing(sendT.Tag)) AndAlso (Not String.IsNullOrEmpty(sendT.Tag))) And String.IsNullOrEmpty(sendT.Text) Then
      sendT.Text = sender.Tag
      sendT.Tag = Nothing
    ElseIf sender.Text = vbNullString Then
      sendT.Text = "None"
      sendT.Tag = Nothing
    End If
  End Sub

  Private Sub cJoy_ButtonDown(Button As Integer) Handles cJoy.ButtonDown
    cJoy.Tag.Text = "Button " & Button
  End Sub

  Private Sub cJoy_ButtonUp(Button As Integer) Handles cJoy.ButtonUp
    cJoy.Tag.Text = "Button " & Button
  End Sub

  Private Sub cJoy_POVSet(Degree As Integer) Handles cJoy.POVSet
    cJoy.Tag.Text = "POV " & Degree & "deg"
  End Sub

  Private Sub cJoy_RAxisLeft() Handles cJoy.RAxisLeft
    cJoy.Tag.text = "R Axis Left"
  End Sub

  Private Sub cJoy_RAxisRight() Handles cJoy.RAxisRight
    cJoy.Tag.text = "R Axis Right"
  End Sub

  Private Sub cJoy_UChange(Value As Object) Handles cJoy.UChange
    cJoy.Tag.text = "U Axis " & Value
  End Sub

  Private Sub cJoy_VChange(Value As Object) Handles cJoy.VChange
    cJoy.Tag.text = "V Axis " & Value
  End Sub

  Private Sub cJoy_XAxisLeft() Handles cJoy.XAxisLeft
    cJoy.Tag.text = "X Axis Left"
  End Sub

  Private Sub cJoy_XAxisRight() Handles cJoy.XAxisRight
    cJoy.Tag.text = "X Axis Right"
  End Sub

  Private Sub cJoy_YAxisBottom() Handles cJoy.YAxisBottom
    cJoy.Tag.text = "Y Axis Bottom"
  End Sub

  Private Sub cJoy_YAxisTop() Handles cJoy.YAxisTop
    cJoy.Tag.text = "Y Axis Top"
  End Sub

  Private Sub cJoy_ZAxisBottom() Handles cJoy.ZAxisBottom
    cJoy.Tag.text = "Z Axis Bottom"
  End Sub

  Private Sub cJoy_ZAxisTop() Handles cJoy.ZAxisTop
    cJoy.Tag.text = "Z Axis Top"
  End Sub

  Private Sub lstVis_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles lstVis.SelectedIndexChanged
    tmrVis.Enabled = False
    Dim selName As String = lstVis.GetItemText(lstVis.SelectedItem)
    If selName <> "None" Then
      Dim selPath As String = Application.StartupPath & "\Visualizations\" & selName & ".dll"
      Dim assem As System.Reflection.Assembly = System.Reflection.Assembly.Load(My.Computer.FileSystem.ReadAllBytes(selPath))
      For Each typeT As Type In assem.GetTypes
        methodDraw = typeT.GetMethod("Draw")
        If methodDraw Is Nothing Then Continue For
        objDraw = Activator.CreateInstance(typeT)
        If Attribute.IsDefined(assem, GetType(System.Reflection.AssemblyDescriptionAttribute)) Then
          lblVisDetails.Text = CType(Attribute.GetCustomAttribute(assem, GetType(System.Reflection.AssemblyDescriptionAttribute)), System.Reflection.AssemblyDescriptionAttribute).Description
        Else
          lblVisDetails.Text = "No Description"
        End If
        tmrVis.Enabled = True
        Exit For
      Next
    Else
      methodDraw = Nothing
      objDraw = Nothing
      lblVisDetails.Text = "No Visualization Selected."
      pctVisPre.Image = Nothing
    End If
  End Sub

  Private Sub tmrVis_Tick(sender As System.Object, e As System.EventArgs) Handles tmrVis.Tick
    If methodDraw IsNot Nothing Then
      Dim Channels As Integer = volDevice.AudioMeterInformation.PeakValues.Count
      Dim ChanVals(Channels - 1) As Double
      For I As Integer = 0 To Channels - 1
        ChanVals(I) = volDevice.AudioMeterInformation.PeakValues(I) * 100
      Next
      pctVisPre.Image = CType(methodDraw.Invoke(objDraw, New Object() {Channels, ChanVals, pctVisPre.DisplayRectangle.Size}), Drawing.Image)
    End If
  End Sub

  Private Sub chkKeyboard_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkKeyboard.CheckedChanged
    For Each Control In pnlKeyboard.Controls
      Control.Enabled = chkKeyboard.Checked
    Next
  End Sub

  Private Sub chkGamepad_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkGamepad.CheckedChanged
    For Each Control In pnlGamepad.Controls
      Control.Enabled = chkGamepad.Checked
    Next
  End Sub

  Private Sub mnuShortcutClear_Click(sender As System.Object, e As System.EventArgs) Handles mnuShortcutClear.Click
    Debug.Print(sender.name)
    If mnuShortcuts.SourceControl.GetType Is GetType(TextBox) Then
      Dim txtCommand As TextBox = mnuShortcuts.SourceControl
      txtCommand.Text = "None"
      txtCommand.Tag = Nothing
    End If
  End Sub

  Private Sub txtRate_ValueChanged(sender As System.Object, e As System.EventArgs) Handles txtRate.ValueChanged
    tmrVis.Interval = Int(1000 / txtRate.Value)
  End Sub
End Class

