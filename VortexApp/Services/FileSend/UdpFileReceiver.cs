using Client.Services.FileSend.Utils;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace UDP_SENDER_FILE_TEST
{
    class UdpFileReceiver
    {
        #region Statics
        public static readonly int MD5ChecksumByteSize = 16;
        #endregion 

        enum ReceiverState
        {
            NotRunning,
            RequestingFile,
            WaitingForRequestFileACK,
            WaitingForInfo,
            PreparingForTransfer,
            Transfering,
            TransferSuccessful,
        }

        private UdpClient _client;
        public readonly int Port;
        public readonly string Hostname;
        private bool _shutdownRequested = false;
        private bool _running = false;

        private Dictionary<UInt32, Block> _blocksReceived = new Dictionary<UInt32, Block>();
        private Queue<UInt32> _blockRequestQueue = new Queue<UInt32>();
        private Queue<NetworkMessage> _packetQueue = new Queue<NetworkMessage>();

        private MD5 _hasher;

        public UdpFileReceiver(string hostname, int port)
        {
            Port = port;
            Hostname = hostname;

            _client = new UdpClient(Hostname, Port);
            _hasher = MD5.Create();
        }


        public void Shutdown()
        {
            _shutdownRequested = true;
        }


        public void GetFile(string filename, string path)
        {
            ReceiverState state = ReceiverState.RequestingFile;
            byte[] checksum = null;
            UInt32 fileSize = 0;
            UInt32 numBlocks = 0;
            UInt32 totalRequestedBlocks = 0;
            Stopwatch transferTimer = new Stopwatch();


            Action ResetTransferState = new Action(() =>
            {
                state = ReceiverState.RequestingFile;
                checksum = null;
                fileSize = 0;
                numBlocks = 0;
                totalRequestedBlocks = 0;
                _blockRequestQueue.Clear();
                _blocksReceived.Clear();
                transferTimer.Reset();
            });

            _running = true;
            bool senderQuit = false;
            bool wasRunning = _running;
            while (_running)
            {

                _checkForNetworkMessages();
                NetworkMessage nm = (_packetQueue.Count > 0) ? _packetQueue.Dequeue() : null;

                bool isBye = (nm == null) ? false : nm.Packet.IsBye;
                if (isBye)
                    senderQuit = true;

                switch (state)
                {
                    case ReceiverState.RequestingFile:
                        RequestFilePacket REQF = new RequestFilePacket();
                        REQF.Filename = filename;

                        byte[] buffer = REQF.GetBytes();
                        _client.Send(buffer, buffer.Length);

                        state = ReceiverState.WaitingForRequestFileACK;
                        break;

                    case ReceiverState.WaitingForRequestFileACK:
                        bool isAck = (nm == null) ? false : (nm.Packet.IsAck);
                        if (isAck)
                        {
                            AckPacket ACK = new AckPacket(nm.Packet);

                            if (ACK.Message == filename)
                            {
                                state = ReceiverState.WaitingForInfo;
                            }
                            else
                                ResetTransferState();
                        }
                        break;

                    case ReceiverState.WaitingForInfo:
                        bool isInfo = (nm == null) ? false : (nm.Packet.IsInfo);
                        if (isInfo)
                        {
                            InfoPacket INFO = new InfoPacket(nm.Packet);
                            fileSize = INFO.FileSize;
                            checksum = INFO.Checksum;
                            numBlocks = INFO.BlockCount;

                            AckPacket ACK = new AckPacket();
                            ACK.Message = "INFO";
                            buffer = ACK.GetBytes();
                            _client.Send(buffer, buffer.Length);

                            state = ReceiverState.PreparingForTransfer;
                        }
                        break;

                    case ReceiverState.PreparingForTransfer:
                        for (UInt32 id = 1; id <= numBlocks; id++)
                            _blockRequestQueue.Enqueue(id);
                        totalRequestedBlocks += numBlocks;

                        transferTimer.Start();
                        state = ReceiverState.Transfering;
                        break;

                    case ReceiverState.Transfering:
                        if (_blockRequestQueue.Count > 0)
                        {
                            UInt32 id = _blockRequestQueue.Dequeue();
                            RequestBlockPacket REQB = new RequestBlockPacket();
                            REQB.Number = id;

                            buffer = REQB.GetBytes();
                            _client.Send(buffer, buffer.Length);

                        }

                        bool isSend = (nm == null) ? false : (nm.Packet.IsSend);
                        if (isSend)
                        {
                            SendPacket SEND = new SendPacket(nm.Packet);
                            Block block = SEND.Block;
                            _blocksReceived.Add(block.Number, block);

                        }

                        if ((_blockRequestQueue.Count == 0) && (_blocksReceived.Count != numBlocks))
                        {
                            for (UInt32 id = 1; id <= numBlocks; id++)
                            {
                                if (!_blocksReceived.ContainsKey(id) && !_blockRequestQueue.Contains(id))
                                {
                                    _blockRequestQueue.Enqueue(id);
                                    totalRequestedBlocks++;
                                }
                            }
                        }

                        if (_blocksReceived.Count == numBlocks)
                            state = ReceiverState.TransferSuccessful;
                        break;

                    case ReceiverState.TransferSuccessful:
                        transferTimer.Stop();

                        Packet BYE = new Packet(Packet.Bye);
                        buffer = BYE.GetBytes();
                        _client.Send(buffer, buffer.Length);

                        if (_saveBlocksToFile(filename, checksum, fileSize, path)) { }

                        _running = false;
                        break;

                }

                Thread.Sleep(1);

                _running &= !_shutdownRequested;
                _running &= !senderQuit;
            }

            if (_shutdownRequested && wasRunning)
            {

                Packet BYE = new Packet(Packet.Bye);
                byte[] buffer = BYE.GetBytes();
                _client.Send(buffer, buffer.Length);
            }

            ResetTransferState();
            _shutdownRequested = false;
        }

        public void Close()
        {
            _client.Close();
        }

        private void _checkForNetworkMessages()
        {
            if (!_running)
                return;

            int bytesAvailable = _client.Available;
            if (bytesAvailable >= 4)
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = _client.Receive(ref ep);
                Packet p = new Packet(buffer);

                NetworkMessage nm = new NetworkMessage();
                nm.Sender = ep;
                nm.Packet = p;
                _packetQueue.Enqueue(nm);
            }
        }

        private bool _saveBlocksToFile(string filename, byte[] networkChecksum, UInt32 fileSize, string path)
        {
            bool good = false;

            try
            {
                int compressedByteSize = 0;
                foreach (Block block in _blocksReceived.Values)
                    compressedByteSize += block.Data.Length;
                byte[] compressedBytes = new byte[compressedByteSize];

                int cursor = 0;
                for (UInt32 id = 1; id <= _blocksReceived.Keys.Count; id++)
                {
                    Block block = _blocksReceived[id];
                    block.Data.CopyTo(compressedBytes, cursor);
                    cursor += Convert.ToInt32(block.Data.Length);
                }

                using (MemoryStream uncompressedStream = new MemoryStream())
                using (MemoryStream compressedStream = new MemoryStream(compressedBytes))
                using (DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                {
                    deflateStream.CopyTo(uncompressedStream);

                    uncompressedStream.Position = 0;
                    byte[] checksum = _hasher.ComputeHash(uncompressedStream);
                    if (!Enumerable.SequenceEqual(networkChecksum, checksum))
                        throw new Exception("Checksum of uncompressed blocks doesn't match that of INFO packet.");

                    uncompressedStream.Position = 0;
                    using (FileStream fileStream = new FileStream(path + filename, FileMode.Create))
                        uncompressedStream.CopyTo(fileStream);
                }

                good = true;
            }
            catch (Exception e)
            {
            }

            return good;
        }
    }
}
