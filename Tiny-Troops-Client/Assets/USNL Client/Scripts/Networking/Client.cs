using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UnityEngine;

namespace USNL.Package {
    public class Client : MonoBehaviour {
        #region Variables

        public static Client instance;
        
        string ip = "127.0.0.1";
        int port = 26950;
        
        public static int dataBufferSize = 4096;

        private int serverMaxPlayers;

        private int clientId = 0;
        private TCP tcp;
        private UDP udp;
        private bool isConnected = false;
        private bool isHost = false;
        private bool serverInfoReceived = false; // This is used to check if the server is full or not

        public int ServerMaxPlayers { get => serverMaxPlayers; set => serverMaxPlayers = value; }
        public int ClientId { get => clientId; set => clientId = value; }
        public UDP Udp { get => udp; set => udp = value; }
        public TCP Tcp { get => tcp; set => tcp = value; }
        public bool IsConnected { get => isConnected; set => isConnected = value; }
        public bool IsHost { get => isHost; set => isHost = value; }
        public bool ServerInfoReceived { get => serverInfoReceived; set => serverInfoReceived = value; }

        #endregion

        #region Core

        private void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Debug.Log("Client instance already exists, destroying object.");
                Destroy(this);
            }

            tcp = new TCP();
            udp = new UDP();
        }

        private void OnApplicationQuit() {
            Disconnect();
        }

        #endregion

        #region TCP & UDP

        public class TCP {
            public TcpClient socket;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            private DateTime lastPacketTime; // Time when the last packet was received

            public DateTime LastPacketTime { get => lastPacketTime; set => lastPacketTime = value; }

            public void Connect() {
                socket = new TcpClient {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                USNL.Package.ThreadManager.StartPacketHandleThread();

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
            }

            private void ConnectCallback(IAsyncResult _result) {
                socket.EndConnect(_result);

                if (!socket.Connected) { return; }

                stream = socket.GetStream();

                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                lastPacketTime = DateTime.Now;

                instance.isConnected = true;

                Debug.Log($"Connected to Server: {instance.ip}:{instance.port}");
            }

            public void SendData(Packet _packet) {
                try {
                    if (socket != null) {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                } catch (Exception _ex) {
                    Debug.Log($"Error sending data to server via TCP: {_ex}");
                    Disconnect();
                }
            }

            private void ReceiveCallback(IAsyncResult _result) {
                try {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0) {
                        instance.Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                } catch {
                    Disconnect();
                }
            }

            private bool HandleData(byte[] _data) {
                int _packetLength = 0;

                receivedData.SetBytes(_data);

                // If packet ID is invalid
                if (receivedData.UnreadLength() >= 4) {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0) {
                        return true;
                    }
                }

                // Read receivedPacket (_data) and create a new Packet type for it and send to a packet handler
                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength()) {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength); // Get byte data for packet

                    // Create new packet and copy byte data into it and send it to the appropriate packet handler
                    // Using ThreadManager because functions can't be called here without breaking the TCP listener because this is a callback function, can't start a stack trace here
                    USNL.Package.ThreadManager.ExecuteOnPacketHandleThread(() => {
                        using (Packet _packet = new Packet(_packetBytes)) {
                            lastPacketTime = DateTime.Now;
                            
                            _packet.PacketId = _packet.ReadInt();
                            USNL.Package.PacketHandlers.packetHandlers[_packet.PacketId](_packet);
                            USNL.NetworkDebugInfo.instance.PacketReceived(_packet.PacketId, _packet.Length() + 4); // +4 for packet length
                        }
                    });

                    // Check if packet ID is invalid again? idk what this is
                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4) {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0) {
                            return true;
                        }
                    }
                }

                // If packet has been read
                if (_packetLength <= 1) { return true; }

                return false;
            }

            private void Disconnect() {
                instance.Disconnect();
                Reset();
            }

            public void Reset() {
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP {
            public UdpClient socket;
            public IPEndPoint endPoint;

            public UDP() {
                endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
            }

            public void UpdateIP() {
                endPoint = null; // Feels like this line might be important
                endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
            }

            // This is run from ClientHandle TODO
            public void Connect(int _localPort) {
                socket = new UdpClient(_localPort);

                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                using (Packet _packet = new Packet()) {
                    SendData(_packet);
                }
            }

            public void SendData(Packet _packet) {
                try {
                    _packet.InsertInt(instance.clientId); // Add client ID to packet
                    if (socket != null) {
                        socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                    }
                } catch (Exception _ex) {
                    Debug.Log($"Error sending data to server via UDP: {_ex}");
                    Disconnect();
                }
            }

            private void ReceiveCallback(IAsyncResult _result) {
                try {
                    byte[] _data = socket.EndReceive(_result, ref endPoint);
                    socket.BeginReceive(ReceiveCallback, null);

                    // If there is no usable data (packet ID is 4 bytes)
                    if (_data.Length < 4) {
                        instance.Disconnect();
                        return;
                    }

                    HandleData(_data);
                } catch {
                    Disconnect();
                }
            }

            private void HandleData(byte[] _data) {
                // Create new packet and copy data into it
                using (Packet _packet = new Packet(_data)) {
                    int _packetLength = _packet.ReadInt();
                    _data = _packet.ReadBytes(_packetLength);
                };

                // Send packet to packet handler
                // Using ThreadManager because functions can't be called here without breaking the TCP listener because this is a callback function, can't start a stack trace here
                USNL.Package.ThreadManager.ExecuteOnPacketHandleThread(() => {
                    using (Packet _packet = new Packet(_data)) {
                        _packet.PacketId = _packet.ReadInt();
                        USNL.Package.PacketHandlers.packetHandlers[_packet.PacketId](_packet);
                        USNL.NetworkDebugInfo.instance.PacketReceived(_packet.PacketId, _packet.Length() + 4); // +4 for packet length
                    }
                });
            }

            private void Disconnect() {
                instance.Disconnect();
                Reset();
            }

            public void Reset() {
                endPoint = null;
                socket = null;
            }
        }

        #endregion

        #region Functions

        public void SetIP(int _id, int _port) {
            if (isConnected) {
                Debug.Log("Client already connected, cannot change IP.");
                return;
            }

            instance.ip = IDtoIP(_id);
            instance.port = _port;
        }

        public void ConnectToServer() {
            if (!isConnected) {
                Debug.Log($"Attempting to Connect to {instance.ip}:{instance.port}");
                tcp.Connect();
                udp.UpdateIP();
            } else {
                Debug.Log("Cannot connect to new server, already connected to a server.");
            }
        }

        public void Disconnect() {
            if (!isConnected) return;

            isConnected = false;

            tcp.socket.Close();
            if (udp.socket != null) udp.socket.Close();

            tcp.Reset();
            udp.Reset();

            serverInfoReceived = false;

            USNL.Package.ThreadManager.StopPacketHandleThread();

            USNL.ClientManager.instance.ServerInfo = new USNL.ServerInfo();

            USNL.CallbackEvents.CallOnDisconnectedCallbacks(0);

            Debug.Log("Disconnected from server.");
        }

        public int IPToID(string _ip) {
            int[] ipOctets = new int[4];
            string[] ipOctetsString = _ip.Split('.');
            for (int i = 0; i < ipOctets.Length; i++) {
                ipOctets[i] = int.Parse(ipOctetsString[i]);
            }
            
            return ipOctets[0] * 16777216 + ipOctets[1] * 65536 + ipOctets[2] * 256 + ipOctets[3];
        }

        public string IDtoIP(int _id) {
            // https://support.sumologic.com/hc/en-us/community/posts/5076590459927-convert-decimal-value-to-IP-address
            int[] ipOctets = new int[4];
            ipOctets[0] = (int)((_id/16777216) % 256);
            ipOctets[1] = (int)((_id / 65536) % 256);
            ipOctets[2] = (int)((_id / 256) % 256);
            ipOctets[3] = (int)((_id / 1) % 256);

            return ipOctets[0] + "." + ipOctets[1] + "." + ipOctets[2] + "." + ipOctets[3];
        }

        #endregion
    }
}
