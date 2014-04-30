<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
    Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
    Me.pctPreview = New System.Windows.Forms.PictureBox()
    Me.cmdNext = New System.Windows.Forms.Button()
    Me.cmdBack = New System.Windows.Forms.Button()
    Me.cmdReload = New System.Windows.Forms.Button()
    Me.TableLayoutPanel1.SuspendLayout()
    CType(Me.pctPreview, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.SuspendLayout()
    '
    'TableLayoutPanel1
    '
    Me.TableLayoutPanel1.ColumnCount = 3
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
    Me.TableLayoutPanel1.Controls.Add(Me.pctPreview, 0, 0)
    Me.TableLayoutPanel1.Controls.Add(Me.cmdNext, 2, 1)
    Me.TableLayoutPanel1.Controls.Add(Me.cmdBack, 0, 1)
    Me.TableLayoutPanel1.Controls.Add(Me.cmdReload, 1, 1)
    Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
    Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
    Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
    Me.TableLayoutPanel1.RowCount = 2
    Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle())
    Me.TableLayoutPanel1.Size = New System.Drawing.Size(284, 262)
    Me.TableLayoutPanel1.TabIndex = 0
    '
    'pctPreview
    '
    Me.TableLayoutPanel1.SetColumnSpan(Me.pctPreview, 3)
    Me.pctPreview.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pctPreview.Location = New System.Drawing.Point(3, 3)
    Me.pctPreview.Name = "pctPreview"
    Me.pctPreview.Size = New System.Drawing.Size(278, 227)
    Me.pctPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
    Me.pctPreview.TabIndex = 0
    Me.pctPreview.TabStop = False
    '
    'cmdNext
    '
    Me.cmdNext.Anchor = System.Windows.Forms.AnchorStyles.Right
    Me.cmdNext.Location = New System.Drawing.Point(206, 236)
    Me.cmdNext.Name = "cmdNext"
    Me.cmdNext.Size = New System.Drawing.Size(75, 23)
    Me.cmdNext.TabIndex = 2
    Me.cmdNext.Text = "> >"
    Me.cmdNext.UseVisualStyleBackColor = True
    '
    'cmdBack
    '
    Me.cmdBack.Anchor = System.Windows.Forms.AnchorStyles.Left
    Me.cmdBack.Location = New System.Drawing.Point(3, 236)
    Me.cmdBack.Name = "cmdBack"
    Me.cmdBack.Size = New System.Drawing.Size(75, 23)
    Me.cmdBack.TabIndex = 1
    Me.cmdBack.Text = "< <"
    Me.cmdBack.UseVisualStyleBackColor = True
    '
    'cmdReload
    '
    Me.cmdReload.Anchor = System.Windows.Forms.AnchorStyles.None
    Me.cmdReload.Location = New System.Drawing.Point(104, 236)
    Me.cmdReload.Name = "cmdReload"
    Me.cmdReload.Size = New System.Drawing.Size(75, 23)
    Me.cmdReload.TabIndex = 3
    Me.cmdReload.Text = "Reload"
    Me.cmdReload.UseVisualStyleBackColor = True
    '
    'Form1
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(284, 262)
    Me.Controls.Add(Me.TableLayoutPanel1)
    Me.Name = "Form1"
    Me.Text = "Form1"
    Me.TableLayoutPanel1.ResumeLayout(False)
    CType(Me.pctPreview, System.ComponentModel.ISupportInitialize).EndInit()
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents pctPreview As System.Windows.Forms.PictureBox
  Friend WithEvents cmdNext As System.Windows.Forms.Button
  Friend WithEvents cmdBack As System.Windows.Forms.Button
  Friend WithEvents cmdReload As System.Windows.Forms.Button

End Class
