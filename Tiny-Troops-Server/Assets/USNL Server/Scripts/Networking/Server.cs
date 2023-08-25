using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace USNL.Package {
    [Serializable]
    public struct ServerData {
        [SerializeField] private bool isServerActive;

        public bool IsServerActive { get => isServerActive; set => isServerActive = value; }
    }
    
    [Serializable]
    public struct ServerConfig {
        [SerializeField] private string serverName;
        [SerializeField] private int serverPort;
        [SerializeField] private int maxClients;
        [SerializeField] private string welcomeMessage;
        [Space]
        [SerializeField] private bool showGUI;

        public string ServerName { get => serverName; set => serverName = value; }
        public int ServerPort { get => serverPort; set => serverPort = value; }
        public int MaxClients { get => maxClients; set => maxClients = value; }
        public string WelcomeMessage { get => welcomeMessage; set => welcomeMessage = value; }
        public bool ShowGUI { get => showGUI; set => showGUI = value; }
    }

    public class Server {
        #region Variables

        public static int MaxClients { get; private set; }

        public static int Port { get; private set; }

        private static bool allowNewConnections = true;

        public static List<Client> Clients = new List<Client>();

        public static ServerData ServerData;

        public static int DataBufferSize = 4096;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;
        
        public static bool AllowNewConnections { get => allowNewConnections; set => allowNewConnections = value; }

        #endregion

        #region Core

        public static void Start(int _maxClients, int _port) {
            MaxClients = _maxClients;
            Port = _port;

            Debug.Log("Starting server...");
            InitializeServerData();

            ThreadManager.StartPacketHandleThread();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            InputManager.instance.Initialize();

            ServerData.IsServerActive = true;

            Debug.Log($"Server started on port {Port}.");

            USNL.CallbackEvents.CallOnServerStartedCallbacks(0);
        }

        private static void InitializeServerData() {
            for (int i = 0; i < MaxClients; i++) {
                Clients.Add(new Client(i));
            }
        }
        
        // Why did i think this was a good idea?
        public static IEnumerator ShutdownServerCoroutine() {
            float time = 0f;
            while (true) {
                if (USNL.ServerManager.GetNumberOfConnectedClients() <= 0 || time > 1f) {
                    ShutdownServer();
                    break;
                }
                yield return new WaitForEndOfFrame();
                time += Time.deltaTime;
            }
        }

        public static void ShutdownServer() {
            for (int i = 0; i < Clients.Count; i++)
                if (Clients[i].IsConnected) Clients[i].Disconnect();

            if (tcpListener != null) tcpListener.Stop();
            if (udpListener != null) udpListener.Close();

            ThreadManager.StopPacketHandleThread();

            ServerData.IsServerActive = false;

            USNL.CallbackEvents.CallOnServerStoppedCallbacks(0);

            Debug.Log("Server stopped.");
        }

        public static void CommandDisconnectAllClients(string _disconnectMessage) {
            for (int i = 0; i < Clients.Count; i++) {
                if (Clients[i].IsConnected) USNL.Package.PacketSend.DisconnectClient(i, _disconnectMessage);
            }
        }

        #endregion

        #region TCP & UDP

        private static void TCPConnectCallback(IAsyncResult _result) {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            if (!AllowNewConnections) {
                for (int i = 0; i < USNL.ServerManager.instance.AllowReconnectionIPs.Count; i++) {
                    if (USNL.ServerManager.instance.AllowReconnectionIPs[i] == _client.Client.RemoteEndPoint.ToString().Split(':')[0]) {
                        Debug.Log($"{_client.Client.RemoteEndPoint} is reconnecting.");
                    } else {
                        Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: New connections not allowed.");
                        return;
                    }
                }
            }

            for (int i = 0; i < MaxClients; i++) {
                if (Clients[i].Tcp.socket != null) continue;
                Clients[i].Tcp.Connect(_client);
                Clients[i].IpAddress = _client.Client.RemoteEndPoint.ToString();
                return;
            }
            
            Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full.");
        }

        private static void UDPReceiveCallback(IAsyncResult _result) {
            try {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4) {
                    return;
                }

                using (Packet _packet = new Packet(_data)) {
                    int _clientId = _packet.ReadInt();

                    if (Clients[_clientId].Udp.endPoint == null) {
                        Clients[_clientId].Udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (Clients[_clientId].Udp.endPoint.ToString() == _clientEndPoint.ToString()) {
                        Clients[_clientId].Udp.HandleData(_packet);
                    }
                }
            } catch (Exception _ex) {
                Debug.Log($"Error receoving UDP data: {_ex}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet) {
            try {
                if (_clientEndPoint != null) {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            } catch (Exception _ex) {
                Debug.Log($"Error sending data tp {_clientEndPoint} via UDP {_ex}");
            }
        }

        #endregion
    }
}