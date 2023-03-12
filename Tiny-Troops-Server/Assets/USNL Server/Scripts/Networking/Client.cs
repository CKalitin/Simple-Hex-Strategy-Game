using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace USNL.Package {
    public class Client {
        #region Variables & Core

        private TCP tcp;
        private UDP udp;

        private int clientId;

        private bool isConnected = false;

        private List<int> packetRTTs = new List<int>();
        private int packetRTT;
        private int smoothPacketRTT;

        public TCP Tcp { get => tcp; set => tcp = value; }
        public UDP Udp { get => udp; set => udp = value; }
        public int ClientId { get => clientId; set => clientId = value; }
        public bool IsConnected { get => isConnected; set => isConnected = value; }
        public int PacketRTT { get => packetRTT; set => packetRTT = value; }
        public int SmoothPacketRTT { get => smoothPacketRTT; set => smoothPacketRTT = value; }

        public Client(int _clientID) {
            clientId = _clientID;
            tcp = new TCP(clientId, this);
            udp = new UDP(clientId);
        }

        #endregion

        #region TCP & UDP

        public class TCP {
            public TcpClient socket;

            private readonly int clientId;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            Client client;

            private DateTime lastPacketTime; // Time when the last packet was received

            public DateTime LastPacketTime { get => lastPacketTime; set => lastPacketTime = value; }

            public TCP(int _id, Client _client) {
                clientId = _id;
                client = _client;
            }

            public void Connect(TcpClient _socket) {
                socket = _socket;
                socket.ReceiveBufferSize = USNL.Package.Server.DataBufferSize;
                socket.SendBufferSize = USNL.Package.Server.DataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[USNL.Package.Server.DataBufferSize];

                stream.BeginRead(receiveBuffer, 0, USNL.Package.Server.DataBufferSize, ReceiveCallback, null);

                lastPacketTime = DateTime.Now;
                
                USNL.Package.PacketSend.Welcome(clientId, ServerManager.instance.ServerConfig.WelcomeMessage, ServerManager.instance.ServerConfig.ServerName, clientId);

                client.isConnected = true;
            }

            public void SendData(Packet _packet) {
                try {
                    if (socket != null) {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                } catch (Exception _ex) {
                    Debug.Log($"Error sending data to client {clientId} via TCP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result) {
                try {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0) {
                        Server.Clients[clientId].Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, USNL.Package.Server.DataBufferSize, ReceiveCallback, null);
                } catch (Exception _ex) {
                    Debug.Log($"Error recieving TCP data: {_ex}");
                    Server.Clients[clientId].Disconnect();
                }
            }


            private bool HandleData(byte[] _data) {
                int _packetLength = 0;

                receivedData.SetBytes(_data);

                if (receivedData.UnreadLength() >= 4) {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0) {
                        return true;
                    }
                }

                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength()) {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnPacketHandleThread(() => {
                        using (Packet _packet = new Packet(_packetBytes)) {
                            lastPacketTime = DateTime.Now;
                            
                            _packet.PacketId = _packet.ReadInt();
                            _packet.FromClient = clientId;
                            USNL.Package.PacketHandlers.packetHandlers[_packet.PacketId](_packet);
                            NetworkDebugInfo.instance.PacketReceived(_packet.PacketId, _packet.Length() + 4); // +4 for packet length
                        }
                    });

                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4) {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0) {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1) {
                    return true;
                }

                return false;
            }

            public void Disconnect() {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP {
            public IPEndPoint endPoint;

            private int clientId;

            public UDP(int _id) {
                clientId = _id;
            }

            public void Connect(IPEndPoint _endPoint) {
                endPoint = _endPoint;
            }

            public void SendData(Packet _packet) {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet _packetData) {
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);


                ThreadManager.ExecuteOnPacketHandleThread(() => {
                    using (Packet _packet = new Packet(_packetBytes)) {
                        _packet.PacketId = _packet.ReadInt();
                        _packet.FromClient = clientId;
                        USNL.Package.PacketHandlers.packetHandlers[_packet.PacketId](_packet);
                        NetworkDebugInfo.instance.PacketReceived(_packet.PacketId, _packet.Length() + 4); // +4 for packet length
                    }
                });
            }

            public void Disconnect() {
                endPoint = null;
            }
        }

        #endregion

        #region Functions

        public void Disconnect() {
            Debug.Log($"Client {clientId} ({tcp.socket.Client.RemoteEndPoint}) has disconnected.");

            isConnected = false;

            ThreadManager.ExecuteOnMainThread(() => {
                ServerManager.instance.ClientDisconnected(clientId);
            });

            tcp.Disconnect();
            udp.Disconnect();
        }

        public void NewPing(int _rtt) {
            packetRTT = _rtt;
            
            if (packetRTTs.Count > 5) { packetRTTs.RemoveAt(0); }
            packetRTTs.Add(packetRTT);
            smoothPacketRTT = packetRTTs.Sum() / packetRTTs.Count;
        }

        #endregion
    }
}
