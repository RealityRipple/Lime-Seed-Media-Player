Public Class frmOpen
  Public sResult() As String

  Private Sub cmdCancel_Click(sender As System.Object, e As System.EventArgs) Handles cmdCancel.Click
    sResult = Nothing
    Me.DialogResult = Windows.Forms.DialogResult.Cancel
    Me.Close()
  End Sub

  Private Sub cmdOpen_Click(sender As System.Object, e As System.EventArgs) Handles cmdOpen.Click
    If tbsOpen.SelectedTab Is tabFile Then
      sResult = Split(txtOpenFile.Text, "|")
    Else
      If cmbDisc.Text.Length > 3 Then
        sResult = {cmbDisc.Text.Substring(0, 3)}
      Else
        sResult = {cmbDisc.Text}
      End If
    End If
    Me.DialogResult = Windows.Forms.DialogResult.OK
    Me.Close()
  End Sub

  Private Sub cmdBrowse_Click(sender As System.Object, e As System.EventArgs) Handles cmdBrowse.Click
    Dim cdlBrowse As New OpenFileDialog With {.Filter = "All Files|*.*", .Multiselect = True}
    If cdlBrowse.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
      If String.IsNullOrEmpty(txtOpenFile.Text) Then
        If cdlBrowse.FileNames.Length > 1 Then
          txtOpenFile.Text = Join(cdlBrowse.FileNames, "|")
        Else
          txtOpenFile.Text = cdlBrowse.FileName
        End If
      Else
        If cdlBrowse.FileNames.Length > 1 Then
          txtOpenFile.Text &= "|" & Join(cdlBrowse.FileNames, "|")
        Else
          txtOpenFile.Text &= "|" & cdlBrowse.FileName
        End If
      End If
    End If
  End Sub

  Private Sub frmOpen_Load(sender As Object, e As System.EventArgs) Handles Me.Load
    txtOpenFile.Text = Nothing
    cmbDisc.Items.Clear()
    For Each drive In My.Computer.FileSystem.Drives
      If drive.DriveType = IO.DriveType.CDRom Then
        If drive.IsReady Then
          cmbDisc.Items.Add(drive.Name & " [" & drive.VolumeLabel & "]")
          If cmbDisc.SelectedIndex = -1 Then cmbDisc.SelectedIndex = cmbDisc.Items.Count - 1
        Else
          cmbDisc.Items.Add(drive.Name)
        End If
      End If
    Next
  End Sub

  Private Sub cmdEject_Click(sender As System.Object, e As System.EventArgs) Handles cmdEject.Click
    If cmdEject.Tag = True Then
      Seed.clsAudioCD.CDTray(cmbDisc.Text(0), Seed.clsAudioCD.DoorOption.Close)
      cmdEject.Tag = False
    Else
      Seed.clsAudioCD.CDTray(cmbDisc.Text(0), Seed.clsAudioCD.DoorOption.Open)
      cmdEject.Tag = True
    End If
  End Sub

  Private Sub tbsOpen_Selected(sender As Object, e As System.Windows.Forms.TabControlEventArgs) Handles tbsOpen.Selected
    If e.Action = TabControlAction.Selecting Or e.Action = TabControlAction.Selected Then
      If e.TabPageIndex = 0 Then
        pctOpenIcon.Image = My.Resources.open
      Else
        pctOpenIcon.Image = My.Resources.open_disc
      End If
    End If
  End Sub
End Class