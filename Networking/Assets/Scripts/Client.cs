using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System;
using UnityEngine;

public class Client : MonoBehaviour
{
    public enum SendType
    {
        All,
        TCP,
        UDP,
        None
    }

    public static bool isConnected;

    private static Socket clientTCPSocket;
    private static Socket clientUDPSocket;

    private static EndPoint remoteServer;

    private static byte[] receiveBufferTCP = new byte[1024];
    private static byte[] receiveBufferUDP = new byte[1024];

    private static byte[] sendBuffer = new byte[1024];

    private static Vector3 position;
    private static Vector3 lastPosition;

    public static bool end;
    public static bool hasId;
    public static bool updatePosition;

    private void Update()
    {
        position = transform.position;
    }

    public static void StartClient(string ipAddress)
    {
        try
        {
            Debug.Log("Starting Client");

            IPAddress ip = IPAddress.Parse(ipAddress);
            IPEndPoint serverTCPEndPoint = new IPEndPoint(ip, 8888);
            IPEndPoint localUDPEndPoint = new IPEndPoint(ip, 0);

            remoteServer = new IPEndPoint(ip, 8889);

            clientTCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientUDPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            clientUDPSocket.Bind(localUDPEndPoint);

            clientUDPSocket.BeginReceiveFrom(receiveBufferUDP, 0, receiveBufferUDP.Length, SocketFlags.None, ref remoteServer, ReceiveFromCallback, clientUDPSocket);
            clientTCPSocket.BeginConnect(serverTCPEndPoint, ConnectCallbackTCP, null);

            //Thread sendThread = new Thread(new ThreadStart(SendLoop));
            //sendThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public static void StopClient()
    {
        Disconnect();
        //clientTCPSocket.BeginDisconnect(false, DisconnectCallback, clientTCPSocket);
    }

    private static void HandleConnect()
    {
        try
        {
            clientTCPSocket.BeginReceive(receiveBufferTCP, 0, receiveBufferTCP.Length, 0, ReceiveCallback, clientTCPSocket);
            isConnected = true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private static void ConnectCallbackTCP(IAsyncResult result)
    {
        clientTCPSocket.EndConnect(result);
        HandleConnect();
    }

    private static void DisconnectCallback(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndDisconnect(result);

            Disconnect();
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to disconnect client.");
            Console.WriteLine(e.Message);
        }
    }

    private static void HandleMessage(byte[] buffer, bool isTCP)
    {
        string message = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
        //Debug.Log($"[TCP:{isTCP}] Full Received: {message}");
        string[] messages = message.Split("<m>");

        foreach (string msg in messages)
        {
            if (msg == string.Empty) continue;
            GameManager.Instance.HandleMessage(msg);
        }
    }

    private static void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            int rec = socket.EndReceive(result);

            if (end) return;

            byte[] buffer = new byte[rec];
            Array.Copy(receiveBufferTCP, buffer, rec);

            HandleMessage(buffer, true);
            socket.BeginReceive(receiveBufferTCP, 0, receiveBufferTCP.Length, 0, ReceiveCallback, socket);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to Receive TCP Message");
            Debug.Log(e);
        }
    }

    private static void ReceiveFromCallback(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            int rec = socket.EndReceiveFrom(result, ref remoteServer);

            if (end) return;

            byte[] buffer = new byte[rec];
            Array.Copy(receiveBufferUDP, buffer, rec);

            HandleMessage(buffer, false);
            socket.BeginReceiveFrom(receiveBufferUDP, 0, receiveBufferUDP.Length, 0, ref remoteServer, ReceiveFromCallback, socket);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to Receive UDP Message");
            Debug.Log(e.Message);
        }
    }

    public static void HandleSend(string message, SendType sendType)
    {
        string formattedMessage = $"{message}<m>";
        sendBuffer = Encoding.ASCII.GetBytes(message);

        switch (sendType)
        {
            case SendType.All:
                HandleSend(clientTCPSocket, sendBuffer, true);
                HandleSend(clientUDPSocket, sendBuffer, false);
                break;
            case SendType.TCP:
                HandleSend(clientTCPSocket, sendBuffer, true);
                break;
            case SendType.UDP:
                HandleSend(clientUDPSocket, sendBuffer, false);
                break;
            default:
                break;
        }
    }

    private static void HandleSend(Socket socket, byte[] buffer, bool sendTCP)
    {
        try
        {
            if (sendTCP)
            {
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, socket);
            }
            else
            {
                socket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, remoteServer, SendToCallback, socket);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Failed to Send Message");
            Debug.Log(e.Message);
        }
    }

    private static void SendCallback(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to Send Message");
            Debug.Log(e.Message);
        }
    }

    private static void SendToCallback(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSendTo(result);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to Send Message");
            Debug.Log(e.Message);
        }
    }

    private static void SendLoop()
    {
        while (true)
        {
            if (end) return;
            if (!isConnected || !hasId) continue;

            while (position == lastPosition && !updatePosition)
            {
                Thread.Sleep(25);
            }
            if (updatePosition) updatePosition = false;
            lastPosition = position;

            HandleSend($"0<c>{GameManager.Instance.localId}<id>{position.x},{position.y},{position.z}", SendType.UDP);
            Thread.Sleep(50);
        }
    }

    public static void Disconnect()
    {
        try
        {
            end = true;

            if (clientTCPSocket != null)
            {
                if (clientTCPSocket.Connected)
                {
                    clientTCPSocket.Shutdown(SocketShutdown.Both);
                }
                clientTCPSocket.Close();
                clientTCPSocket = null;
            }

            if (clientUDPSocket != null)
            {
                clientUDPSocket.Close();
                clientUDPSocket = null;
            }

            isConnected = false;
            hasId = false;
        }
        catch (Exception e)
        {
            Debug.Log("Failed to Fully Disconnect");
            Debug.Log(e.Message);
        }
    }
}