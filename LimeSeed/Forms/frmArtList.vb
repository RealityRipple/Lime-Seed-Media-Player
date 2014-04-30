Public Class frmArtList

  Public Sub Display(Artist As String, Album As String, Rows() As Generic.Dictionary(Of String, Object))
    artList.Display(Artist, Album, Rows)
  End Sub

  Private Sub artList_Cancelled() Handles artList.Cancelled
    Me.DialogResult = Windows.Forms.DialogResult.Cancel
    Me.Close()
  End Sub

  Private Sub artList_NewArt(row As Generic.Dictionary(Of String, Object)) Handles artList.NewArt
    Me.Tag = row
    Me.DialogResult = Windows.Forms.DialogResult.OK
    Me.Close()
  End Sub
End Class