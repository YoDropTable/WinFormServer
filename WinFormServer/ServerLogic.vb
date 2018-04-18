Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports WinFormServer

Public Class ServerLogic

    Public Event messageRecieved As EventHandler(Of myEventArgs)
    Public Event connected As EventHandler(Of myEventArgs)
    Private messageQueue As Queue(Of String)
    Public ipHostInfo As IPHostEntry
    Public ListOfAddres As List(Of String)
    Public MyAddress As String
    Public serverSocket As TcpListener
    Public clientSocket As TcpClient

    Public Sub New(myPort As Integer)
        messageQueue = New Queue(Of String)
        ipHostInfo = Dns.GetHostEntry(Dns.GetHostName())
        ListOfAddres = New List(Of String)
        For Each myIp As IPAddress In ipHostInfo.AddressList
            ListOfAddres.Add(String.Format("myIp: {0} myFam: {1}", myIp, myIp.AddressFamily))
        Next
        Dim localEndPoint As IPEndPoint = New IPEndPoint(IPAddress.Any, myPort)
        MyAddress = String.Format("My IP ADDRESS: {0}", localEndPoint) + " : " + myPort.ToString

        serverSocket = New TcpListener(localEndPoint)
    End Sub

    Public Sub addMessage(myMessage As String)
        messageQueue.Enqueue(myMessage)
    End Sub

    Public Sub StartServer()
        serverSocket.Start()
        While True
            clientSocket = serverSocket.AcceptTcpClient()
            Dim client As HandleClient = New HandleClient(messageQueue)
            AddHandler client.Connected, AddressOf ClientConnected
            AddHandler client.MessageRecieved, AddressOf ClientMessageRecieved
            client.startClient(clientSocket)
        End While
    End Sub

    Private Sub ClientConnected(sender As Object, e As myEventArgs)
        RaiseEvent connected(Me, e)
    End Sub

    Private Sub ClientMessageRecieved(sender As Object, e As myEventArgs)
        RaiseEvent messageRecieved(Me, e)
    End Sub

    Private Class HandleClient

        Public Event MessageRecieved As EventHandler(Of myEventArgs)
        Public Event Connected As EventHandler(Of myEventArgs)
        Private myMessageQueue As Queue(Of String)
        Private clientSocket As TcpClient

        Private clNo As String

        Public Sub New(ByRef messageQueu As Queue(Of String))
            myMessageQueue = messageQueu
        End Sub

        Public Sub startClient(ByVal inClientSocket As TcpClient)
            Me.clientSocket = inClientSocket
            Dim ctThread As Thread = New Thread(AddressOf DoChat)
            ctThread.IsBackground = True
            ctThread.Start()
        End Sub
        Private Sub DoChat()
            Dim requestCount As Integer = 0
            Dim connected As Boolean = True


            While (connected)
                Try
                    If clientSocket.Connected Then
                        requestCount = requestCount + 1
                        Dim networkStream As NetworkStream = clientSocket.GetStream()
                        Dim myReadBuffer As Byte() = New Byte(1023) {}
                        Dim myCompleteMessage As StringBuilder = New StringBuilder()
                        Dim numberOfBytesRead As Integer = 0
                        Dim myMessage As String = ""
                        Do While networkStream.DataAvailable And myMessageQueue.Count = 0
                            Dim getLenBytes As Byte() = New Byte(4) {}
                            networkStream.Read(getLenBytes, 0, 4)
                            numberOfBytesRead = BitConverter.ToInt32(getLenBytes, 0)
                            Dim getBytes As Byte() = New Byte(numberOfBytesRead - 1) {}
                            networkStream.Read(getBytes, 0, numberOfBytesRead)
                            myMessage = Encoding.UTF8.GetString(getBytes)
                        Loop

                        If numberOfBytesRead > 0 Then
                            Dim temp As String = myMessage
                            Dim e As New myEventArgs
                            Dim myCommand As String() = temp.Split(" "c)
                            If myCommand(0).Contains("Connect") Then
                                Console.WriteLine("Client Connected " & clientSocket.Client.RemoteEndPoint.ToString & " " + DateTime.Now)
                                Dim serverResponse As String = "404 OK"
                                Dim clientInfo As New List(Of String)
                                clientInfo.Add(clientSocket.Client.LocalEndPoint.ToString())
                                e.passedStuff = clientInfo
                                RaiseEvent connected(Me, e)
                                Dim sendBytes As Byte() = Encoding.UTF8.GetBytes(serverResponse)
                                Dim num As Byte() = System.BitConverter.GetBytes(sendBytes.Length)
                                networkStream.Write(num, 0, 4)
                                networkStream.Write(sendBytes, 0, sendBytes.Length)
                            ElseIf myCommand(0).Contains("Disconnect") Then
                                connected = False
                            Else
                                Dim newList As New List(Of String)
                                newList.Add(temp)
                                e.passedStuff = newList
                                RaiseEvent MessageRecieved(Me, e)
                            End If
                        End If
                        While myMessageQueue.Count > 0
                            Dim message = myMessageQueue.Dequeue
                            Dim sendBytes As Byte() = Encoding.UTF8.GetBytes(message)
                            Dim num As Byte() = System.BitConverter.GetBytes(sendBytes.Length)
                            networkStream.Write(num, 0, 4)
                            networkStream.Write(sendBytes, 0, sendBytes.Length)
                        End While
                    End If
                Catch ex As Exception
                    connected = False
                    Console.WriteLine(" >> " & ex.ToString())
                End Try
            End While
        End Sub
    End Class


End Class



