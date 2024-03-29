﻿using Client.Services.Audio;
using Client.Services.FileSend;
using Client.Services.Network.Utilits;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using VortexApp.UI.Events;
using static Client.Services.Network.Utilits.ChatHelper;

namespace Client.Services.Network
{
    public class ChatClient
    {
        #region Fields

        private readonly TcpClient server;
        private readonly Socket clientSocket;
        private readonly IPEndPoint localEndPoint;
        private IPEndPoint remoteEndPoint;
        private Thread tcpRecieveThread;
        #endregion

        #region Events
        public event EventHandler UserListReceived;
        public event EventHandler MessageReceived;
        public event EventHandler CallRecieved;
        public event EventHandler CallRequestResponded;
        public event EventHandler FileRecieved;
        #endregion

        #region Properties

        #region Profile Info
        public string Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string NickName { get; set; }

        public Dictionary<Guid, string> Friends { get; set; } = new();
        public Dictionary<Guid, string> FriendsRequests { get; set; } = new();
        public string IP { get; set; }
        #endregion
        public string ServerAddress { get; set; }
        public string ServerName { get; set; }

        public bool IsConnected { get; set; }

        private string _file_path_transfer = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Services//FileSend//FileBuff");
        private int file_port = 63001;
        private VoiceCallHandler voiceCallHandler;
        private int Aviable_server_port = 0;
        private Thread fileThread;
        #endregion

        #region Consructor
        public ChatClient(int port, string serverAddress, string email, string password, string IP)
        {
            try
            {
                server = new TcpClient(serverAddress, port);
                IsConnected = true;
                clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);
                localEndPoint = GetHostEndPoint(IP);
                ServerAddress = serverAddress;
            }
            catch (SocketException)
            {
                Console.WriteLine("Socket Exception");
                return;
            }
            Data data = new(Command.Auth, "Client", "Server", IP, email + " " + CreateMD5(password));
            SendComamnd(data);
        }

        public ChatClient(int port, string serverAddress, string IP, string login, string password, string nickName, string email, DateTime? birthday)
        {
            try
            {
                server = new TcpClient(serverAddress, port);
                IsConnected = true;
                clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);
                localEndPoint = GetHostEndPoint(IP);
                ServerAddress = serverAddress;
            }
            catch (SocketException)
            {
                Console.WriteLine("Socket Exception");
                return;
            }
            Data data = new(Command.Reg, "Client", "Server", IP, login + " " + CreateMD5(password) + " " + nickName + " " + email + " " + birthday.ToString());
            SendComamnd(data);
        }
        #endregion

        #region Methods

        private void BindSocket()
        {
            if (!clientSocket.IsBound)
                clientSocket.Bind(localEndPoint);
        }

        private IPEndPoint GetHostEndPoint(string ip)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            if (ipAddress == null)
                return null;
            var random = new Random();
            var endPoint = new IPEndPoint(ipAddress, random.Next(65000, 65536));
            IP = ip;
            //IP = $"{endPoint.Address}:{endPoint.Port}";
            return endPoint;
        }


        public void Init()
        {
            tcpRecieveThread = new Thread(RecieveFromServer) { Priority = ThreadPriority.Normal };
            tcpRecieveThread.Start();
        }

        private void RecieveFromServer()
        {
            var state = new ChatHelper.StateObject
            {
                WorkSocket = server.Client
            };
            while (IsConnected)
            {
                if (IsReceivingData)
                    continue;
                IsReceivingData = true;
                server.Client.BeginReceive(state.Buffer, 0, ChatHelper.StateObject.BUFFER_SIZE, 0,
                    OnReceive, state);
            }
        }

        public bool IsReceivingData { get; set; }

        public void OnReceive(IAsyncResult ar)
        {
            var state = ar.AsyncState as ChatHelper.StateObject;
            if (state == null)
                return;
            var handler = state.WorkSocket;
            if (!handler.Connected)
                return;
            try
            {
                var bytesRead = handler.EndReceive(ar);
                if (bytesRead <= 0)
                    return;
                ParseMessage(Data.GetBytes(state.Buffer));
                state.Buffer = null;
                state.Buffer = new byte[StateObject.BUFFER_SIZE];
                GC.Collect();
                server.Client.BeginReceive(state.Buffer, 0, ChatHelper.StateObject.BUFFER_SIZE, 0, OnReceive, state);
            }
            catch (SocketException)
            {
                IsConnected = false;
                server.Client.Disconnect(true);
            }
        }

        private void ParseMessage(Data data)
        {
            if (data.Command == Command.Good_Auth)
            {
                var arr = data.Message.Split(';');
                Id = arr[0];
                NickName = arr[1];
                var friends = arr[2].Split(',');
                var requests = arr[3].Split(',');
                UIEvents.UserUpData(Id, NickName);
                for (int i = 0; i < friends.Length; i = i + 2)
                {
                    if (friends.ElementAtOrDefault(i + 1) != null)
                        Friends.Add(Guid.Parse(friends[i]), friends[i + 1]);
                }
                if (Friends.Count > 0) UIEvents.SetFriendsUI(Friends);


                for (int i = 0; i < requests.Length; i = i + 2)
                {
                    if (requests.ElementAtOrDefault(i + 1) != null)
                        FriendsRequests.Add(Guid.Parse(requests[i]), requests[i + 1]);
                }
                if (FriendsRequests.Count > 0) UIEvents.SetReqUI(FriendsRequests);

                UIEvents.MainWindowUse();
                GC.Collect();
            }
            else if (data.Command == Command.Bad_Auth)
            {
                UIEvents.ClientBadAuth();
                GC.Collect();
            }
            else if (data.Command == Command.Good_Reg)
            {
                UIEvents.RegGOOD();
                GC.Collect();
            }
            else if (data.Command == Command.Bad_Reg)
            {
                Console.WriteLine(data.Command.ToString());
            }
            else if (data.Command == Command.Accept_Port)
            {
                Aviable_server_port = int.Parse(data.Message);
            }
            else if (data.Command == Command.TryFileGood)
            {
                SendFile(data.Message, data.To);
            }
            else if (data.Command == Command.TryFileBad)
            {
                UIEvents.UserNotConnectedUI(data.Message);
            }
            else if (data.Command == Command.StopAccept_File)
            {
                fileThread = null;
                GC.Collect();
            }
            else if (data.Command == Command.Accept_File)
            {
                UIEvents.NewFileUI(data.From, data.Message);
            }
            else if (data.Command == Command.Send_Message)
            {
                UIEvents.SendMessageUI(data.Message, data.From);
            }
            else if (data.Command == Command.End_Call)
            {
                UIEvents.EndCallUI();
            }
            else if (data.Command == Command.Request_Call)
            {
                UIEvents.RequestCallUI(data.From, data.ClientAddress);
            }
            else if (data.Command == Command.Accept_Call)
            {
                UIEvents.AcceptCallUI(data.ClientAddress);
            }
            else if (data.Command == Command.Cancel_Call)
            {
                UIEvents.CancelCallUI();
            }
            else if (data.Command == Command.FriendRequestGood)
            {
                UIEvents.FriendReqGOOD();
            }
            else if (data.Command == Command.FriendRequestFailed)
            {
                UIEvents.FriendReqFailedUI();
            }
            else if (data.Command == Command.FriendRequest)
            {
                UIEvents.NewFriendReqUI(data.Message, data.To);
            }
            else if (data.Command == Command.NewFriend)
            {
                UIEvents.NewFriendUI(data.Message, data.From);
            }
            else if (data.Command == Command.UserNotConnected)
            {
                UIEvents.UserNotConnectedUI(data.To);
            }
        }
        public void DeclineFriendRequest(Guid friendGuid)
        {
            SendComamnd(new Data(Command.DeclineFriendRequest, Id.ToString(), friendGuid.ToString(), IP, ""));
        }
        public void AcceptFriendRequest(Guid friendGuid)
        {
            SendComamnd(new Data(Command.AcceptFriendRequest, Id.ToString(), friendGuid.ToString(), IP, ""));
        }
        public void SendFriendRequest(Guid friendGuid)
        {
            SendComamnd(new Data(Command.FriendRequest, Id.ToString(), friendGuid.ToString(), IP, ""));
        }
        public void TryCall(Guid friendGuid)
        {
            SendComamnd(new Data(Command.Request_Call, Id.ToString(), friendGuid.ToString(), IP, ""));
        }
        public void AcceptCall(string friendID)
        {
            SendComamnd(new(Command.Accept_Call, Id.ToString(), friendID, IP, ""));
        }
        public void CancelCall(string friendID)
        {
            SendComamnd(new(Command.Cancel_Call, Id.ToString(), friendID, IP, ""));
        }
        public void EndCall(Guid friendGuid)
        {
            voiceCallHandler.StopReceive();
            voiceCallHandler.StopSend();

            SendComamnd(new Data(Command.End_Call, Id.ToString(), friendGuid.ToString(), IP, ""));
        }
        public void EndCallAccept()
        {
            if (voiceCallHandler != null)
            {
                voiceCallHandler.StopReceive();
                voiceCallHandler.StopSend();
            }
        }
        private void SendComamnd(Data data)
        {
            server.Client.Send(data.ToBytes());
        }
        public bool GetFile(string filename, string path)
        {
            if (Aviable_server_port == 0)
            {
                SendComamnd(new Data(Command.Accept_Port, Id.ToString(), "Server", IP, ""));
                return false;
            }
            else
            {
                SendComamnd(new Data(Command.Send_File, Id.ToString(), "Server", IP, filename + " " + Aviable_server_port.ToString()));
                Thread.Sleep(1500);
                FileTool.ReceiverFile(ServerAddress, Aviable_server_port, filename, path);
                Aviable_server_port = 0;
                return true;
            }
        }

        public void SendMessage(string message, string friend_ID)
        {
            Data data = new(Command.Send_Message, Id.ToString(), friend_ID, IP, message);
            SendComamnd(data);
        }
        private void SendFile(string filename, string friend_ID)
        {
            fileThread = new Thread(() => FileTool.SendFile(_file_path_transfer, file_port));
            Data data = new(Command.Accept_File, Id.ToString(), friend_ID, IP, filename);
            SendComamnd(data);
            fileThread.Start();
        }
        public void TrySendFile(string filename, string friend_ID)
        {
            Data data = new(Command.TryAccept_File, Id.ToString(), friend_ID, IP, filename);
            SendComamnd(data);
        }
        public void Disconect()
        {
            IsConnected = false;

            var data = new Data(Command.Disconnect, Id.ToString(), "Server", IP, "");
            if (server.Client.Connected)
                server.Client.Send(data.ToBytes());
        }

        public void CloseConnection()
        {
            IsConnected = false;
            var data = new Data(Command.CloseConnection, Id.ToString(), "Server", IP, "");
            if (server.Client.Connected)
                server.Client.Send(data.ToBytes());
        }

        public void StartCall(string ClientAddress)
        {
            IPEndPoint ipEndPointReceive = new(IPAddress.Parse("127.0.0.1"), 60202);// 60202
            IPEndPoint ipEndPointSend = new(IPAddress.Parse(ClientAddress), 60201);// 60201
            voiceCallHandler = new(ipEndPointReceive, ipEndPointSend);
            voiceCallHandler.Receive();
            voiceCallHandler.Send();
        }

        public void StartCallAccept(string IP)
        {
            IPEndPoint ipEndPointReceive = new(IPAddress.Parse("127.0.0.1"), 60201);// 60201
            IPEndPoint ipEndPointSend = new(IPAddress.Parse(IP), 60202);// 60202
            voiceCallHandler = new(ipEndPointReceive, ipEndPointSend);
            voiceCallHandler.Receive();
            voiceCallHandler.Send();

        }

        #endregion

        static string CreateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);

                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    stringBuilder.Append(hashBytes[i].ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }
    }
}
