Imports System.Threading
Imports Newtonsoft.Json.Linq
Imports WinFormServer

Public Class Form1
    Dim androidServer As ServerLogic
    Dim VRServer As ServerLogic
    Dim VRCon As Boolean = False
    Dim AndroidCon As Boolean = False

    Public Sub New()

        InitializeComponent()
        androidServer = New ServerLogic(1337)
        AddHandler androidServer.connected, AddressOf ServerConnected
        AddHandler androidServer.messageRecieved, AddressOf MessageFromAndroid

        Dim ctThread As Thread = New Thread(AddressOf androidServer.StartServer)
        ctThread.IsBackground = True
        ctThread.Start()
        VRServer = New ServerLogic(6969)
        AddHandler VRServer.connected, AddressOf VRServerConnected
        AddHandler VRServer.messageRecieved, AddressOf VRMessageFromAndroid

        Dim vrThread As Thread = New Thread(AddressOf VRServer.StartServer)
        vrThread.IsBackground = True
        vrThread.Start()
    End Sub

    Private Sub VRMessageFromAndroid(sender As Object, e As myEventArgs)
        If AndroidCon = True Then
            androidServer.addMessage(e.passedStuff.ToString)
        Else
            PrintStuff(TextBox1, "NO ANDROID CONNECTED!!!!")
        End If
    End Sub

    Private Sub VRServerConnected(sender As Object, e As myEventArgs)
        VRCon = True
    End Sub

    Private Sub MessageFromAndroid(sender As Object, e As myEventArgs)
        If VRCon Then
            VRServer.addMessage(e.passedStuff.FirstOrDefault)
        End If
        Dim json As String = e.passedStuff.FirstOrDefault
        Dim ser As JObject = JObject.Parse(json)
        Dim stuff = ser.Item("nTitle")
        Dim stuff2 = ser.Item("nText")
        Dim mKey = ser.Item("nKey")
        Me.Invoke(Sub() AddControl(stuff.ToString, stuff2.ToString, mKey.ToString))
    End Sub
    Private Sub AddControl(myTitle As String, myText As String, myKey As String)
        Dim myNoti As New myControlvb(myTitle, myText, myKey)
        AddHandler myNoti.notiReply, AddressOf DealWIthReply
        FlowLayoutPanel1.Controls.Add(myNoti)
    End Sub

    Private Sub ServerConnected(sender As Object, e As myEventArgs)
        AndroidCon = True
        Me.Invoke(Sub() ToolStripTextBox1.Text = "Connected:" & e.passedStuff.FirstOrDefault)
    End Sub

    Private Delegate Sub PrintStuffDelegate(ByVal TB As TextBox, ByVal txt As String)
    Private Sub PrintStuff(ByVal TB As TextBox, ByVal txt As String)
        If TB.InvokeRequired Then
            TB.Invoke(New PrintStuffDelegate(AddressOf PrintStuff), New Object() {TB, txt & Environment.NewLine})
        Else
            TB.AppendText(txt & Environment.NewLine)
        End If
    End Sub

    Private Sub CloseToolStripMenuItem1_Click_1(sender As Object, e As EventArgs) Handles CloseToolStripMenuItem1.Click
        Application.Exit()
    End Sub

    Private Sub sendReply(who As String, message As String)
        Dim myJson As New JObject(New JProperty("who", who),
                New JProperty("message", message))
        Dim myMessage As String = myJson.ToString
        androidServer.addMessage(myMessage)
    End Sub

    Private Sub DealWIthReply(sender As Object, e As myEventArgs)
        sendReply(e.passedStuff.Item(0), e.passedStuff.Item(1))
    End Sub
End Class
