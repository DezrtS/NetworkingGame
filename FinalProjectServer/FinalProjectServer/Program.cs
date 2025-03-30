using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace FinalProjectServer
{
    internal class Server
    {
        private enum SendType
        {
            All,
            TCP,
            UDP,
            None
        }

        private enum CommandType
        {
            Position,
            Chat,
            Connection,
            Quit
        }

        private static byte[] receiveBufferTCP = new byte[1024];
        private static byte[] receiveBufferUDP = new byte[1024];

        private static byte[] sendBuffer = new byte[1024];

        private static Socket serverTCPSocket;
        private static Socket serverUDPSocket;

        private static EndPoint remoteClient;

        private static Dictionary<Socket, int> clientSockets = new Dictionary<Socket, int>();

        static void Main(string[] args)
        {
            StartServer();

            //Thread sendThread = new Thread(new ThreadStart(SendLoop));
            //sendThread.Start();

            Console.ReadLine();
        }

        public static void StartServer()
        {
            try
            {
                IPAddress[] ipAddresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;

                Console.WriteLine("[-2] - Enter your own address");
                Console.WriteLine("[-1] - Local Host");
                for (int i = 0; i < ipAddresses.Length; i++)
                {
                    Console.WriteLine($"[{i}] - {ipAddresses[i]}");
                }

                Console.Write("\nSelect an Option: ");
                string stringOption = Console.ReadLine();

                IPAddress ip;

                int option = int.Parse(stringOption);
                if (option < -1)
                {
                    Console.Write("\nEnter an IpAddress: ");
                    ip = IPAddress.Parse(Console.ReadLine());
                }
                else if (option < 0)
                {
                    ip = IPAddress.Parse("127.0.0.1");
                }
                else
                {
                    ip = ipAddresses[option];
                }

                Console.WriteLine($"Starting Server [{ip}]");

                IPEndPoint localTCPEndPoint = new IPEndPoint(ip, 8888);
                IPEndPoint localUDPEndPoint = new IPEndPoint(ip, 8889);

                remoteClient = new IPEndPoint(IPAddress.Any, 0);

                serverTCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverUDPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                serverTCPSocket.Bind(localTCPEndPoint);
                serverUDPSocket.Bind(localUDPEndPoint);

                serverTCPSocket.Listen(1);

                serverTCPSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                serverUDPSocket.BeginReceiveFrom(receiveBufferUDP, 0, receiveBufferUDP.Length, SocketFlags.None, ref remoteClient, ReceiveFromCallback, serverUDPSocket);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to Start Server");
                Console.WriteLine(e.Message);
            }
        }

        private static void AcceptClient(Socket socket)
        {
            try
            {
                Console.WriteLine($"Client Connected to Server");
                Random random = new Random();
                clientSockets.Add(socket, random.Next(1000));
                List<int> values = clientSockets.Values.ToList();
                string ids = "";
                foreach (KeyValuePair<Socket, int> client in clientSockets)
                {
                    ids += $"{client.Value}<id>[C{client.Value}]: {client.Key.LocalEndPoint}<inst>";
                }
                int id = clientSockets[socket];
                HandleSend(new List<Socket>() { socket }, $"2<c>0<t>{id}", SendType.TCP);
                HandleSend(clientSockets.Keys.ToList(), $"2<c>1<t>{ids}", SendType.TCP);

                serverTCPSocket.BeginAccept(AcceptCallback, null);

                socket.BeginReceive(receiveBufferTCP, 0, receiveBufferTCP.Length, 0, ReceiveCallback, socket);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to Accept Client");
                Console.WriteLine(e.Message);
                serverTCPSocket.BeginAccept(AcceptCallback, null);
            }
        }

        private static void AcceptCallback(IAsyncResult result)
        {
            Socket socket = serverTCPSocket.EndAccept(result);
            AcceptClient(socket);
        }

        private static void DisconnectCallback(IAsyncResult result)
        {
            try
            {
                Socket socket = (Socket)result.AsyncState;
                socket.EndDisconnect(result);

                Console.WriteLine($"Client {socket.RemoteEndPoint} disconnected.");

                int id = clientSockets[socket];
                clientSockets.Remove(socket);
                socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to disconnect client.");
                Console.WriteLine(e.Message);
            }
        }

        private static void HandleMessage(string message, Socket socket, bool isTCP)
        {
            string[] commandPlusData = message.Split(new string[] { "<c>" }, StringSplitOptions.None);
            CommandType commandType = (CommandType)int.Parse(commandPlusData[0]);

            switch (commandType)
            {
                case CommandType.Position:
                    HandleSend(clientSockets.Keys.ToList(), $"0<c>{commandPlusData[1]}", SendType.TCP);
                    break;
                case CommandType.Chat:
                    if (commandPlusData[1].ToLower() == "quit")
                    {
                        HandleSend(clientSockets.Keys.ToList(), $"3<c>{clientSockets[socket]}", SendType.TCP);
                        socket.BeginDisconnect(false, DisconnectCallback, socket);
                        break;
                    }
                    int id = clientSockets[socket];
                    Console.WriteLine($"Received Message: \"{commandPlusData[1]}\" from [C{id}: {socket.LocalEndPoint}]");
                    HandleSend(clientSockets.Keys.ToList(), $"1<c>[C{id}: {socket.LocalEndPoint}]: {commandPlusData[1]}", SendType.TCP);
                    break;
                case CommandType.Quit:
                    HandleSend(clientSockets.Keys.ToList(), $"3<c>{clientSockets[socket]}", SendType.TCP);
                    socket.BeginDisconnect(false, DisconnectCallback, socket);
                    break;
                default:
                    break;
            }

            //Console.WriteLine($"[TCP:{isTCP}] Received: {message}, Content: {commandPlusData[1]}");
        }

        private static void ReceiveCallback(IAsyncResult result)
        {
            Socket socket = null;
            try
            {
                socket = (Socket)result.AsyncState;
                int rec = socket.EndReceive(result);

                byte[] buffer = new byte[rec];
                Array.Copy(receiveBufferTCP, buffer, rec);

                string message = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                string[] messages = message.Split(new string[] { "<m>" }, StringSplitOptions.None);

                foreach (string msg in messages)
                {
                    if (msg == string.Empty) continue;
                    HandleMessage(msg, socket, true);
                }

                socket.BeginReceive(receiveBufferTCP, 0, receiveBufferTCP.Length, 0, ReceiveCallback, socket);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to Receive TCP Message");

                if (socket != null && socket.Connected)
                {
                    Console.WriteLine(e.Message);
                    socket.BeginDisconnect(false, DisconnectCallback, socket);
                }
            }
        }

        private static void ReceiveFromCallback(IAsyncResult result)
        {
            try
            {
                Socket socket = (Socket)result.AsyncState;
                int rec = socket.EndReceiveFrom(result, ref remoteClient);

                byte[] buffer = new byte[rec];
                Array.Copy(receiveBufferUDP, buffer, rec);

                string message = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                string[] messages = message.Split(new string[] { "<m>" }, StringSplitOptions.None);

                foreach (string msg in messages)
                {
                    if (msg == string.Empty) continue;
                    HandleMessage(msg, socket, false);
                }

                serverUDPSocket.BeginReceiveFrom(receiveBufferUDP, 0, receiveBufferUDP.Length, SocketFlags.None, ref remoteClient, ReceiveFromCallback, serverUDPSocket);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to Receive UDP Message");
                Console.WriteLine(e.Message);
            }
        }

        private static void HandleSend(List<Socket> sockets, string message, SendType sendType)
        {
            string formattedMessage = $"{message}<m>";
            sendBuffer = Encoding.ASCII.GetBytes(formattedMessage);
            //Console.WriteLine("Sending");

            switch (sendType)
            {
                case SendType.All:
                    HandleSend(sockets, sendBuffer, true, true);
                    break;
                case SendType.TCP:
                    HandleSend(sockets, sendBuffer, true, false);
                    break;
                case SendType.UDP:
                    HandleSend(sockets, sendBuffer, false, true);
                    break;
                default:
                    break;
            }
        }

        private static void HandleSend(List<Socket> sockets, byte[] buffer, bool sendTCP, bool sendUDP)
        {
            try
            {
                if (sendTCP)
                {
                    foreach (Socket client in sockets.ToList())
                    {
                        if (client.Connected)
                        {
                            client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, client);
                        }
                        else
                        {
                            if (clientSockets.ContainsKey(client))
                            {
                                int id = clientSockets[client];
                                clientSockets.Remove(client);
                                HandleSend(clientSockets.Keys.ToList(), $"3<c>{id}", SendType.TCP);
                            }
                        }
                    }
                }
                if (sendUDP) serverUDPSocket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, remoteClient, SendToCallback, serverUDPSocket);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to Send Message");
                Console.WriteLine(e.Message);
            }
        }

        private static void SendCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);
        }

        private static void SendToCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSendTo(result);
        }
    }
}
