using Client.Services.FileSend.Utils;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace UDP_SENDER_FILE_TEST
{
    public class UdpFileSender
    {
        #region Statics
        public static readonly UInt32 MaxBlockSize = 63 * 1024;   // 63KB
        #endregion 

        enum SenderState
        {
            NotRunning,
            WaitingForFileRequest,
            PreparingFileForTransfer,
            SendingFileInfo,
            WaitingForInfoACK,
            Transfering
        }

        private UdpClient _client;
        public readonly int Port;
        public bool Running { get; private set; } = false;

        public readonly string FilesDirectory;
        private HashSet<string> _transferableFiles;
        private Dictionary<UInt32, Block> _blocks = new Dictionary<UInt32, Block>();
        private Queue<NetworkMessage> _packetQueue = new Queue<NetworkMessage>();

        private MD5 _hasher;

        public UdpFileSender(string filesDirectory, int port)
        {
            FilesDirectory = filesDirectory;
            Port = port;
            _client = new UdpClient(Port, AddressFamily.InterNetwork);
            _hasher = MD5.Create();
        }

        public void Init()
        {
            List<string> files = new List<string>(Directory.EnumerateFiles(FilesDirectory));
            _transferableFiles = new HashSet<string>(files.Select(s => s.Substring(FilesDirectory.Length + 1)));

            if (_transferableFiles.Count != 0)
            {
                Running = true;
            }
        }

        public void Shutdown()
        {
            Running = false;
        }

        public void Run()
        {
            SenderState state = SenderState.WaitingForFileRequest;
            string requestedFile = "";
            IPEndPoint receiver = null;

            Action ResetTransferState = new Action(() =>
            {
                state = SenderState.WaitingForFileRequest;
                requestedFile = "";
                receiver = null;
                _blocks.Clear();
            });

            while (Running)
            {
                _checkForNetworkMessages();
                NetworkMessage nm = (_packetQueue.Count > 0) ? _packetQueue.Dequeue() : null;

                bool isBye = (nm == null) ? false : nm.Packet.IsBye;
                if (isBye)
                {
                    ResetTransferState();
                    _client.Close();
                    return;
                }
                switch (state)
                {
                    case SenderState.WaitingForFileRequest:

                        bool isRequestFile = (nm == null) ? false : nm.Packet.IsRequestFile;
                        if (isRequestFile)
                        {
                            RequestFilePacket REQF = new RequestFilePacket(nm.Packet);
                            AckPacket ACK = new AckPacket();
                            requestedFile = REQF.Filename;


                            if (_transferableFiles.Contains(requestedFile))
                            {
                                receiver = nm.Sender;
                                ACK.Message = requestedFile;
                                state = SenderState.PreparingFileForTransfer;

                            }
                            else
                                ResetTransferState();

                            byte[] buffer = ACK.GetBytes();
                            _client.Send(buffer, buffer.Length, nm.Sender);
                        }
                        break;

                    case SenderState.PreparingFileForTransfer:
                        byte[] checksum;
                        UInt32 fileSize;
                        if (_prepareFile(requestedFile, out checksum, out fileSize))
                        {
                            InfoPacket INFO = new InfoPacket();
                            INFO.Checksum = checksum;
                            INFO.FileSize = fileSize;
                            INFO.MaxBlockSize = MaxBlockSize;
                            INFO.BlockCount = Convert.ToUInt32(_blocks.Count);

                            byte[] buffer = INFO.GetBytes();
                            _client.Send(buffer, buffer.Length, receiver);

                            state = SenderState.WaitingForInfoACK;
                        }
                        else
                            ResetTransferState();
                        break;

                    case SenderState.WaitingForInfoACK:
                        bool isAck = (nm == null) ? false : (nm.Packet.IsAck);
                        if (isAck)
                        {
                            AckPacket ACK = new AckPacket(nm.Packet);
                            if (ACK.Message == "INFO")
                            {
                                state = SenderState.Transfering;
                            }
                        }
                        break;

                    case SenderState.Transfering:
                        bool isRequestBlock = (nm == null) ? false : nm.Packet.IsRequestBlock;
                        if (isRequestBlock)
                        {
                            RequestBlockPacket REQB = new RequestBlockPacket(nm.Packet);

                            Block block = _blocks[REQB.Number];
                            SendPacket SEND = new SendPacket();
                            SEND.Block = block;

                            byte[] buffer = SEND.GetBytes();
                            _client.Send(buffer, buffer.Length, nm.Sender);
                        }
                        break;
                }

                Thread.Sleep(1);
            }

            if (receiver != null)
            {
                Packet BYE = new Packet(Packet.Bye);
                byte[] buffer = BYE.GetBytes();
                _client.Send(buffer, buffer.Length, receiver);
            }

            state = SenderState.NotRunning;
        }

        public void Close()
        {
            _client.Close();
        }

        private void _checkForNetworkMessages()
        {
            if (!Running)
                return;

            int bytesAvailable = _client.Available;
            if (bytesAvailable >= 4)
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = _client.Receive(ref ep);

                NetworkMessage nm = new NetworkMessage();
                nm.Sender = ep;
                nm.Packet = new Packet(buffer);
                _packetQueue.Enqueue(nm);
            }
        }

        private bool _prepareFile(string requestedFile, out byte[] checksum, out UInt32 fileSize)
        {
            bool good = false;
            fileSize = 0;

            try
            {
                byte[] fileBytes = File.ReadAllBytes(Path.Combine(FilesDirectory, requestedFile));
                checksum = _hasher.ComputeHash(fileBytes);
                fileSize = Convert.ToUInt32(fileBytes.Length);
                Stopwatch timer = new Stopwatch();
                using (MemoryStream compressedStream = new MemoryStream())
                {
                    DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress, true);
                    timer.Start();
                    deflateStream.Write(fileBytes, 0, fileBytes.Length);
                    deflateStream.Close();
                    timer.Stop();

                    compressedStream.Position = 0;
                    long compressedSize = compressedStream.Length;
                    UInt32 id = 1;
                    while (compressedStream.Position < compressedSize)
                    {
                        long numBytesLeft = compressedSize - compressedStream.Position;
                        long allocationSize = (numBytesLeft > MaxBlockSize) ? MaxBlockSize : numBytesLeft;
                        byte[] data = new byte[allocationSize];
                        compressedStream.Read(data, 0, data.Length);

                        Block b = new Block(id++);
                        b.Data = data;
                        _blocks.Add(b.Number, b);
                    }
                    good = true;
                }
            }
            catch (Exception e)
            {
                _blocks.Clear();
                checksum = null;
            }

            return good;
        }

    }
}
