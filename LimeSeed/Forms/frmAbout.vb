Public NotInheritable Class frmAbout
  Private WithEvents wsVer As New Net.WebClient
  Private sVerPath As String = Application.UserAppDataPath & "\ver.txt"
  Private sEXEPath As String = Application.UserAppDataPath & "\Setup.exe"

  Private Sub frmAbout_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
    If wsVer IsNot Nothing Then
      If wsVer.IsBusy Then wsVer.CancelAsync()
      Do While wsVer.IsBusy
        Application.DoEvents()
        Threading.Thread.Sleep(10)
      Loop
    End If
    If e.CloseReason = CloseReason.FormOwnerClosing Then
      If My.Computer.FileSystem.FileExists(sVerPath) Then My.Computer.FileSystem.DeleteFile(sVerPath)
      If My.Computer.FileSystem.FileExists(sEXEPath) Then Shell(sEXEPath & " /silent", AppWinStyle.NormalFocus, False)
    Else
      If My.Computer.FileSystem.FileExists(sVerPath) Then My.Computer.FileSystem.DeleteFile(sVerPath)
      If My.Computer.FileSystem.FileExists(sEXEPath) Then My.Computer.FileSystem.DeleteFile(sEXEPath)
    End If
  End Sub

  Private Sub frmAbout_Load(sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
    Dim ApplicationTitle As String
    If My.Application.Info.Title <> "" Then
      ApplicationTitle = My.Application.Info.Title
    Else
      ApplicationTitle = System.IO.Path.GetFileNameWithoutExtension(My.Application.Info.AssemblyName)
    End If
    Me.Text = String.Format("About {0}", ApplicationTitle)
    lblProduct.Text = My.Application.Info.ProductName
    lblVersion.Text = String.Format("Version {0}", My.Application.Info.Version.ToString)
    SetUpdateValue("Checking for Updates", True)
    lblCompany.Text = My.Application.Info.CompanyName
    Dim ffDshowPath As String = My.Computer.FileSystem.SpecialDirectories.ProgramFiles & "\ffdshow\ffdshow.ax"
    If My.Computer.FileSystem.FileExists(ffDshowPath) Then
      Dim verInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(ffDshowPath)
      Dim fileInfo = My.Computer.FileSystem.GetFileInfo(ffDshowPath)
      lblFFDShow.Text = String.Format("FFDshow rev{0}_{1}", verInfo.ProductBuildPart, fileInfo.LastWriteTime.ToString("yyyyMMdd"))
    Else
      lblFFDShow.Text = "FFDshow not installed"
    End If
    ttInfo.SetToolTip(lblFFDShow, lblFFDShow.Text)
    txtDescription.Text = My.Application.Info.Description
    wsVer.CachePolicy = New Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore)
    wsVer.DownloadStringAsync(New Uri("http://update.realityripple.com/Lime_Seed_Media_Player/ver.txt"), "INFO")
  End Sub

  Private Sub cmdOK_Click(sender As System.Object, ByVal e As System.EventArgs) Handles cmdOK.Click
    Me.Close()
  End Sub

  Private Sub wsVer_DownloadFileCompleted(sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs) Handles wsVer.DownloadFileCompleted
    If e.Error IsNot Nothing Then
      SetUpdateValue(e.Error.Message, False)
    Else
      wsVer.Dispose()
      wsVer = Nothing
      SetUpdateValue("Download Complete", False)
      tmrDone.Start()
    End If
  End Sub

  Private Sub wsVer_DownloadProgressChanged(sender As Object, ByVal e As System.Net.DownloadProgressChangedEventArgs) Handles wsVer.DownloadProgressChanged
    Dim sProgress As String = "(" & e.ProgressPercentage & "%)"
    Select Case CType(e.UserState, String)
      Case "INFO" : SetUpdateValue("Checking for Updates " & sProgress, True)
      Case "FILE" : SetUpdateValue("Downloading Update " & sProgress, True)
      Case Else : SetUpdateValue("Downloading Something " & sProgress, True)
    End Select
  End Sub

  Private Sub wsVer_DownloadStringCompleted(sender As Object, ByVal e As System.Net.DownloadStringCompletedEventArgs) Handles wsVer.DownloadStringCompleted
    If e.Error IsNot Nothing Then
      SetUpdateValue(e.Error.Message, False)
    Else
      Try
        Dim sTmp() As String = e.Result.Split("|"c) 'ver = 0, url = 1
        If CompareVersions(sTmp(0)) Then
          SetUpdateValue("New Update Available", False)
          Application.DoEvents()
          wsVer.CachePolicy = New Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore)
          wsVer.DownloadFileAsync(New Uri(sTmp(1)), sEXEPath, "FILE")
        Else
          SetUpdateValue("No New Updates", False)
        End If
      Catch ex As Exception
        SetUpdateValue("Version Parsing Error", False)
      End Try
    End If
  End Sub

  Public Function CompareVersions(sRemote As String) As Boolean
    Dim sLocal As String = Application.ProductVersion
    Dim LocalVer(3) As String
    If sLocal.Contains(".") Then
      LocalVer(0) = sLocal.Split(".")(0)
      For I As Integer = 1 To 3
        If sLocal.Split(".").Count > I Then
          Dim sTmp As String = sLocal.Split(".")(I).Trim
          If IsNumeric(sTmp) And sTmp.Length < 4 Then sTmp &= StrDup(4 - sTmp.Length, "0"c)
          LocalVer(I) = sTmp
        Else
          LocalVer(I) = "0000"
        End If
      Next
    ElseIf sLocal.Contains(",") Then
      LocalVer(0) = sLocal.Split(",")(0)
      For I As Integer = 1 To 3
        If sLocal.Split(",").Count > I Then
          Dim sTmp As String = sLocal.Split(",")(I).Trim
          If IsNumeric(sTmp) And sTmp.Length < 4 Then sTmp &= StrDup(4 - sTmp.Length, "0"c)
          LocalVer(I) = sTmp
        Else
          LocalVer(I) = "0000"
        End If
      Next
    End If
    Dim RemoteVer(3) As String
    If sRemote.Contains(".") Then
      RemoteVer(0) = sRemote.Split(".")(0)
      For I As Integer = 1 To 3
        If sRemote.Split(".").Count > I Then
          Dim sTmp As String = sRemote.Split(".")(I).Trim
          If IsNumeric(sTmp) And sTmp.Length < 4 Then sTmp &= StrDup(4 - sTmp.Length, "0"c)
          RemoteVer(I) = sTmp
        Else
          RemoteVer(I) = "0000"
        End If
      Next
    ElseIf sRemote.Contains(",") Then
      RemoteVer(0) = sRemote.Split(".")(0)
      For I As Integer = 1 To 3
        If sRemote.Split(",").Count > I Then
          Dim sTmp As String = sRemote.Split(",")(I).Trim
          If IsNumeric(sTmp) And sTmp.Length < 4 Then sTmp &= StrDup(4 - sTmp.Length, "0"c)
          RemoteVer(I) = sTmp
        Else
          RemoteVer(I) = "0000"
        End If
      Next
    End If
    Dim bUpdate As Boolean = False
    If Val(LocalVer(0)) > Val(RemoteVer(0)) Then
      'Local's OK
    ElseIf Val(LocalVer(0)) = Val(RemoteVer(0)) Then
      If Val(LocalVer(1)) > Val(RemoteVer(1)) Then
        'Local's OK
      ElseIf Val(LocalVer(1)) = Val(RemoteVer(1)) Then
        If Val(LocalVer(2)) > Val(RemoteVer(2)) Then
          'Local's OK
        ElseIf Val(LocalVer(2)) = Val(RemoteVer(2)) Then
          If Val(LocalVer(3)) >= Val(RemoteVer(3)) Then
            'Local's OK
          Else
            bUpdate = True
          End If
        Else
          bUpdate = True
        End If
      Else
        bUpdate = True
      End If
    Else
      bUpdate = True
    End If
    Return bUpdate
  End Function

  Private Sub SetUpdateValue(Message As String, ByVal Throbber As Boolean)
    ttInfo.SetToolTip(lblUpdate, Message)
    If Throbber Then
      If lblUpdate.Image Is Nothing Then lblUpdate.Image = My.Resources.throbber
      If Not lblUpdate.Text = "      " & Message Then lblUpdate.Text = "      " & Message
    Else
      If lblUpdate.Image IsNot Nothing Then lblUpdate.Image = Nothing
      If Not lblUpdate.Text = Message Then lblUpdate.Text = Message
    End If
  End Sub

  Private Sub cmdDonate_Click(sender As System.Object, e As System.EventArgs) Handles cmdDonate.Click
    Diagnostics.Process.Start("http://realityripple.com/donate.php?itm=LimeSeed")
  End Sub

  Private Sub cmdFFDshow_Click(sender As System.Object, e As System.EventArgs) Handles cmdFFDshow.Click
    If Environment.Is64BitProcess Then
      Diagnostics.Process.Start("http://sourceforge.net/projects/ffdshow-tryout/files/SVN%20builds%20by%20clsid/64-bit%20builds/")
    Else
      Diagnostics.Process.Start("http://sourceforge.net/projects/ffdshow-tryout/files/SVN%20builds%20by%20clsid/generic%20builds/")
    End If
  End Sub

  Private Sub tmrDone_Tick(sender As System.Object, e As System.EventArgs) Handles tmrDone.Tick
    If My.Computer.FileSystem.FileExists(sEXEPath) Then
      frmMain.Close()
    Else
      SetUpdateValue("Update Failure", False)
    End If
  End Sub
End Class
