
Public Class myControlvb
    Public Event notiReply As EventHandler(Of myEventArgs)
    Public Sub New(who As String, message As String, myKey As String)
        ' This call is required by the designer.
        InitializeComponent()
        Label1.Text = who
        Label2.Text = message
        Label3.Text = myKey
        ' Add any initialization after the InitializeComponent() call.
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim myE As New myEventArgs
        myE.passedStuff.Add(Label1.Text)
        myE.passedStuff.Add(TextBox1.Text)
        RaiseEvent notiReply(Me, myE)
        TextBox1.Enabled = False
        Button1.Enabled = False
    End Sub
End Class
