using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace USNL {
    [Serializable]
    public enum IPType {
        WAN,
        LAN,
        Localhost,
        Unknown
    }

    [Serializable]
    public struct ServerInfo {
        public string ServerName;
        public int[] ConnectedClientIds;
        public int MaxClients;
        public bool ServerFull;
    }

    public class ClientManager : MonoBehaviour {
        #region Variables

        public static ClientManager instance;

        [Header("Connection Info")]
        [SerializeField] private long serverId;
        [SerializeField] private int port = 26950;
        [Space]
        [Tooltip("If connection is not established after x seconds, stop attemping connection.")]
        [SerializeField] private float attemptConnectionTime = 10f;
        [Tooltip("If this is too low the server will see 2 attempted connections and the client will not received UDP data, who knows why.")]
        private float timeBetweenConnectionAttempts = 3f;
        [Space]
        [Tooltip("In seconds.")]
        [SerializeField] private float timeoutTime = 5f;
        [Space]
        [SerializeField] private ServerInfo serverInfo;

        private long wanClientId = 0;
        private long lanClientId = 0;
        private string wanClientIp = "";
        private string lanClientIp = "";

        private bool attemptConnection = true;

        private bool isAttempingConnection = false;
        private bool isMigratingHost = false;
        private bool isBecomingHost = false;

        private bool timedOut = false;
        private bool serverClosed = false;

        private DateTime timeOfConnection;

        private string serverName;

        [Header("Server Host")]
        [SerializeField] private string serverExeName = "USNL-Server-Example-Project.exe";
        [Tooltip("This is a local path inside the Unity folder of the exported game. Be sure to add '/' at the end")]
        [SerializeField] private string serverPath = "Server";
        [Tooltip("Be sure to add '/' at the end.\nThis is not affected by useApplicationDataPath.")]
        [SerializeField] private string editorServerPath = "Server";
        [Tooltip("When the project is built this tick adds the path to the game files before the server path.\nIf server files are in a child folder of the game files, tick this.")]
        [SerializeField] private bool useApplicationPath = false;
        [SerializeField] private Package.ServerConfig serverConfig;

        public long WanClientId { get => wanClientId; set => wanClientId = value; }
        public long LanClientId { get => lanClientId; set => lanClientId = value; }
        public string WanClientIp { get => wanClientIp; set => wanClientIp = value; }
        public string LanClientIP { get => lanClientIp; set => lanClientIp = value; }
        
        public bool IsConnected { get => Package.Client.instance.IsConnected && USNL.Package.Client.instance.ServerInfoReceived; }
        public bool IsAttemptingConnection { get => isAttempingConnection; }
        public bool IsHost { get => Package.Client.instance.IsHost; }
        public bool IsMigratingHost { get => isMigratingHost; }
        public bool IsBecomingHost { get => isBecomingHost; }
        
        public bool TimedOut { get => timedOut; set => timedOut = value; }
        public bool ServerClosed { get => serverClosed; set => serverClosed = value; }
        
        public DateTime TimeOfConnection { get => timeOfConnection; set => timeOfConnection = value; }

        public string ServerExeName { get => serverExeName; set => serverExeName = value; }
        public string ServerPath { get => serverPath; set => serverPath = value; }
        public string EditorServerPath { get => editorServerPath; set => editorServerPath = value; }
        public bool UseApplicationPath { get => useApplicationPath; set => useApplicationPath = value; }

        public Package.ServerConfig ServerConfig { get => serverConfig; set => serverConfig = value; }
        public Package.ServerData ServerData { get => Package.ServerHost.GetServerData(); }

        public bool IsServerRunning { get => Package.ServerHost.IsServerRunning(); }
        
        public string ServerName { get => serverName; set => serverName = value; }
        public ServerInfo ServerInfo { get => serverInfo; set => serverInfo = value; }

        public bool IsServerActive() { Package.ServerHost.ReadServerDataFile(); return Package.ServerHost.GetServerData().IsServerActive; }
        public int ClientId { get => USNL.Package.Client.instance.ClientId; }

        #endregion

        #region Core

        private void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Debug.Log("Client Manager instance already exists, destroying object!");
                Destroy(this);
            }

            if (Application.isEditor) {
                Application.runInBackground = true;
            }
        }

        private void Start() {
            StartCoroutine(SetClientId());
        }

        private void Update() {
            CheckServerLaunchSuccessful();
            if (Package.ServerHost.GetServerData().IsServerActive == false && !Package.ServerHost.LaunchingServer) Package.Client.instance.IsHost = false;

            if (-USNL.Package.Client.instance.Tcp.LastPacketTime.Subtract(DateTime.Now).TotalSeconds >= timeoutTime & USNL.Package.Client.instance.IsConnected) {
                Debug.Log("Timed out.");
                timedOut = true;
                DisconnectFromServer();
            }
        }

        private void OnEnable() {
            USNL.CallbackEvents.OnWelcomePacket += OnWelcomePacket;
            USNL.CallbackEvents.OnDisconnectClientPacket += OnDisconnectClientPacket;
            USNL.CallbackEvents.OnServerInfoPacket += OnServerInfoPacket;
        }

        private void OnDisable() {
            USNL.CallbackEvents.OnWelcomePacket -= OnWelcomePacket;
            USNL.CallbackEvents.OnDisconnectClientPacket -= OnDisconnectClientPacket;
            USNL.CallbackEvents.OnServerInfoPacket -= OnServerInfoPacket;
        }

        private void OnApplicationQuit() {
            CloseServer();
        }

        #endregion

        #region Connection Functions

        public void ConnectToServer() {
            ResetClientManagerVariables();
            
            Package.Client.instance.SetIP(serverId, port);
            StartCoroutine(AttemptingConnection());
        }

        /// <summary> Attempts to connect to the server with ip and port provided. </summary>
        /// <param name="_id">IP Address</param>
        /// <param name="_port">Port</param>
        public void ConnectToServer(long _id, int _port) {
            serverId = _id;
            port = _port;

            ResetClientManagerVariables();

            Package.Client.instance.SetIP(serverId, port);
            StartCoroutine(AttemptingConnection());
        }

        public void DisconnectFromServer() {
            Package.Client.instance.Disconnect();
            attemptConnection = false;
        }

        public void StopAttemptingConnection() {
            attemptConnection = false;
        }

        private IEnumerator AttemptingConnection() {
            if (isAttempingConnection == true) yield break;
            
            isAttempingConnection = true;
            attemptConnection = true;

            int connectionsAttempted = 0;

            float timer = 0.00001f;
            while (timer < attemptConnectionTime && attemptConnection) {
                yield return new WaitForEndOfFrame();

                if (Package.Client.instance.IsConnected)
                    break;

                if (timer > connectionsAttempted * timeBetweenConnectionAttempts) {
                    Package.Client.instance.ConnectToServer();
                    connectionsAttempted++;
                }

                timer += Time.unscaledDeltaTime;
            }

            isAttempingConnection = false;
        }

        public void Disconnect() {
            USNL.Package.Client.instance.Disconnect();
        }

        private void ResetClientManagerVariables() {
            timedOut = false;
            serverClosed = false;
        }

        public bool CheckClientConnected(int _id) {
            if (ServerInfo.ConnectedClientIds != null) return ServerInfo.ConnectedClientIds.Contains(_id);
            else return false;
        }

        #endregion

        #region IP and ID Functions

        private IEnumerator SetClientId() {
            // This is a coroutine because of GetWanIP()

            wanClientIp = GetWanIP();
            lanClientIp = GetLanIP();
            
            if (wanClientIp != "") wanClientId = IPToID(wanClientIp);
            if (lanClientIp != "") lanClientId = IPToID(lanClientIp);
            
            yield return new WaitForEndOfFrame();
        }

        // https://www.code-sample.com/2019/12/how-to-get-public-ipv4-address-using-c.html
        private string GetWanIP() {
            try {
                string address = "";
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                using (System.IO.StreamReader stream = new System.IO.StreamReader(response.GetResponseStream())) {
                    address = stream.ReadToEnd();
                }

                int first = address.IndexOf("Address: ") + 9;
                int last = address.LastIndexOf("</body>");
                address = address.Substring(first, last - first);
                
                return address;
            } catch {
                Debug.LogWarning("Could not Get WAN IP.");
                return "";
            }
        }

        // https://stackoverflow.com/questions/6803073/get-local-ip-address
        private string GetLanIP() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }
            Debug.LogWarning("Failed to Get LAN IP. No network adapters with an IPv4 address in the system.");
            return "";
        }

        public IPType GetIPType(string _ip) {
            //if (_ip == wanClientIp) return IPType.WAN; TODO - If server can be pinged, it is valid
            if (_ip.Substring(0, 5) == lanClientIp.Substring(0, 5)) return IPType.LAN;
            else if (_ip == lanClientIp) return IPType.Localhost;
            else if (CheckIPOctets(_ip)) return IPType.WAN;
            else return IPType.Unknown;
        }

        private bool CheckIPOctets(string _ip) {
            char dot = '.';
            int count = _ip.Count(s => s == dot);
            
            if (count == 3) return true;
            return false;
        }

        public long IPToID(string _ip) {
            return USNL.Package.Client.instance.IPToID(_ip);
        }

        public string IDtoIP(long _id) {
            return USNL.Package.Client.instance.IDtoIP(_id);
        }

        #endregion

        #region Server Host Functions

        public void LaunchServer() {
            Package.ServerHost.LaunchServer();
        }

        public void CloseServer() {
            Package.ServerHost.CloseServer();
        }

        private void CheckServerLaunchSuccessful() {
            if (Package.ServerHost.LaunchingServer && ServerData.IsServerActive) {
                Package.ServerHost.LaunchingServer = false;
                Debug.Log("Successfuly launched server. Server is Active.");
            }
        }

        #endregion

        #region Callbacks

        private void OnWelcomePacket(object _packetObject) {
            USNL.Package.WelcomePacket _wp = (USNL.Package.WelcomePacket)_packetObject;

            Debug.Log($"Welcome message from Server ({_wp.ServerName}): {_wp.WelcomeMessage}, Client Id: {_wp.ClientId}");
            Package.Client.instance.ClientId = _wp.ClientId;
            serverName = _wp.ServerName;

            timeOfConnection = DateTime.Now;

            USNL.Package.Client.instance.Udp.Connect(((IPEndPoint)USNL.Package.Client.instance.Tcp.socket.Client.LocalEndPoint).Port);

            USNL.Package.PacketSend.WelcomeReceived(_wp.ClientId);
            
            USNL.CallbackEvents.CallOnConnectedCallbacks(0);
        }

        private void OnDisconnectClientPacket(object _packetObject) {
            USNL.Package.DisconnectClientPacket _dcp = (USNL.Package.DisconnectClientPacket)_packetObject;

            serverClosed = true;

            Debug.Log($"Disconnection commanded from server.\nMessage: {_dcp.DisconnectMessage}");

            USNL.Package.Client.instance.Disconnect();
        }

        private void OnServerInfoPacket(object _packetObject) {
            USNL.Package.ServerInfoPacket _si = (USNL.Package.ServerInfoPacket)_packetObject;
            serverInfo.ServerName = _si.ServerName;
            serverInfo.ConnectedClientIds = _si.ConnectedClientIds;
            serverInfo.MaxClients = _si.MaxClients;
            serverInfo.ServerFull = _si.ServerFull;

            if (!USNL.Package.Client.instance.ServerInfoReceived) {
                USNL.Package.Client.instance.ServerInfoReceived = true;

                if (_si.ConnectedClientIds.Length > _si.MaxClients) {
                    Debug.Log($"{_si.ConnectedClientIds.Length} {_si.MaxClients}");
                    DisconnectFromServer();
                }
            }
        }

        #endregion
    }
}