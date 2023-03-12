using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using UnityEngine;

namespace USNL {
    public class ServerManager : MonoBehaviour {
        #region Variables

        public static ServerManager instance;

        [Header("Editor Config")]
        [SerializeField] private Package.ServerConfig serverConfig;
        [SerializeField] private bool useServerFilesInEditor;

        [Header("Server Config")]
        [Tooltip("In seconds.")]
        [SerializeField] private float timeoutTime = 11f;
        [SerializeField] private float serverInfoPacketSendInterval = 1f;

        private bool isMigratingHost = false;
        private DateTime timeOfStartup;

        private float lastServerInfoPacketSentTime;
        
        private int wanServerId;
        private int lanServerId;
        private string wanServerIp;
        private string lanServerIp;

        private bool quittingApplication = false;

        public Package.ServerConfig ServerConfig { get => serverConfig; set => serverConfig = value; }
        
        public bool IsMigratingHost { get => isMigratingHost; set => isMigratingHost = value; }
        public DateTime TimeOfStartup { get => timeOfStartup; set => timeOfStartup = value; }
        public bool ServerActive { get => USNL.Package.Server.ServerData.IsServerActive; }

        public int WanServerId { get => wanServerId; }
        public int LanServerId { get => lanServerId; }
        public string WanServerIp { get => wanServerIp; }
        public string LanServerIP { get => lanServerIp; }

        public bool AllowNewConnections { get => USNL.Package.Server.AllowNewConnections; set => USNL.Package.Server.AllowNewConnections = value; }

        #endregion

        #region Core

        private void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Debug.Log("Server Manager instance already exists, destroying object!");
                Destroy(this);
            }

            if (Application.isEditor) {
                Application.runInBackground = true;
            }

            StartCoroutine(SetServerId());
        }

        private void Start() {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        private void Update() {
            LookForServerQuitFile();
            CheckClientsTimedout();
            ContinuouslySendServerInfoPackets();
        }

        private void OnEnable() { USNL.CallbackEvents.OnWelcomeReceivedPacket += OnWelcomeReceivedPacket; }
        private void OnDisable() { USNL.CallbackEvents.OnWelcomeReceivedPacket -= OnWelcomeReceivedPacket; }

        private void OnApplicationQuit() {
            StopServer();
        }

        #endregion

        #region Server Manager

        public void StartServer() {
            LookForServerQuitFile(); // If quit is commanded before unity loading screen is complete, this is necessary
            if (!quittingApplication) {
                Package.Server.ServerData.IsServerActive = true;

                if ((useServerFilesInEditor & Application.isEditor) | !Application.isEditor) {
                    WriteServerDataFile();
                    ReadServerConfigFile();
                }

                Package.Server.Start(serverConfig.MaxClients, serverConfig.ServerPort);

                TimeOfStartup = DateTime.Now;
            }
        }

        public void StopServer() {
            Package.Server.CommandDisconnectAllClients("Server is shutting down.");

            Package.Server.ServerData.IsServerActive = false;
            if ((useServerFilesInEditor & Application.isEditor) | !Application.isEditor)
                WriteServerDataFile();

            Package.Server.ShutdownServer();
            //StartCoroutine(Package.Server.ShutdownServerCoroutine());
        }

        private void OnWelcomeReceivedPacket(object _packetObject) {
            USNL.Package.WelcomeReceivedPacket _wrp = (USNL.Package.WelcomeReceivedPacket)_packetObject;
            
            Debug.Log($"{Package.Server.Clients[_wrp.FromClient].Tcp.socket.Client.RemoteEndPoint} connected successfully and is now Client {_wrp.FromClient}.");
            if (_wrp.FromClient != _wrp.ClientIdCheck) {
                Debug.Log($"Client {_wrp.FromClient} has assumed the wrong client ID ({_wrp.ClientIdCheck}).");
            }

            USNL.Package.PacketSend.ServerInfo(_wrp.FromClient, serverConfig.ServerName, GetConnectedClientIds(), serverConfig.MaxClients, GetNumberOfConnectedClients() >= serverConfig.MaxClients);

            USNL.CallbackEvents.CallOnClientConnectedCallbacks(_wrp.FromClient);
        }

        #endregion

        #region Server Manager Helper Functions

        private void CheckClientsTimedout() {
            for (int i = 0; i < USNL.Package.Server.Clients.Count; i++) {
                if (!USNL.Package.Server.Clients[i].IsConnected) continue;
                if (-USNL.Package.Server.Clients[i].Tcp.LastPacketTime.Subtract(DateTime.Now).TotalSeconds < timeoutTime) continue;

                Debug.Log($"Client {i} has timed out.");
                USNL.Package.Server.Clients[i].Disconnect();
            }
        }

        private void SendServerInfoPacketToAllClients() {
            for (int i = 0; i < USNL.Package.Server.Clients.Count; i++) {
                if (!USNL.Package.Server.Clients[i].IsConnected) continue;

                USNL.Package.PacketSend.ServerInfo(USNL.Package.Server.Clients[i].ClientId, serverConfig.ServerName, GetConnectedClientIds(), serverConfig.MaxClients, GetNumberOfConnectedClients() >= serverConfig.MaxClients);
            }
        }

        private void ContinuouslySendServerInfoPackets() {
            if (Time.time - lastServerInfoPacketSentTime > serverInfoPacketSendInterval) {
                SendServerInfoPacketToAllClients();
                lastServerInfoPacketSentTime = Time.time;
            }
        }
        
        #endregion

        #region Server Config and Data

        private void LookForServerQuitFile() {
            if (File.Exists(GetApplicationPath() + "ServerQuit")) {
                quittingApplication = true;
                Debug.Log("Server Quit commanded from host client, shutting down server.");
                File.Delete(GetApplicationPath() + "ServerQuit");
                StopServer();
                StartCoroutine(QuitAfterDelay());
            }
        }

        private IEnumerator QuitAfterDelay() {
            yield return new WaitForSeconds(2f);
            Application.Quit();
        }

        public void WriteServerDataFile() {
            string jsonText = JsonConvert.SerializeObject(Package.Server.ServerData, Formatting.Indented);

            StreamWriter sw = new StreamWriter($"{GetApplicationPath()}ServerData.json");
            sw.Write(jsonText);
            sw.Flush();
            sw.Close();
            
            Debug.Log("Wrote Server Data file at: " + GetApplicationPath() + "ServerData.json");
        }

        public void ReadServerConfigFile() {
            string path = GetApplicationPath() + "/ServerConfig.json";

            if (!File.Exists(path)) {
                string jsonText = JsonConvert.SerializeObject(serverConfig, Formatting.Indented);

                StreamWriter sw = new StreamWriter($"{path}");
                sw.Write(jsonText);
                sw.Flush();
                sw.Close();

                Debug.Log("Server Config file did not exist. Created one.");
                return;
            }
            
            string text = File.ReadAllText($"{path}");
            serverConfig = JsonConvert.DeserializeObject<Package.ServerConfig>(text);

            Debug.Log("Read server config file.");
        }

        private string GetApplicationPath() {
            string dataPath = Application.dataPath;
            string[] slicedPath = dataPath.Split("/");
            string path = "";
            for (int i = 0; i < slicedPath.Length - 1; i++) {
                path += slicedPath[i] + "/";
            }

            return path;
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

        private string GetWanIP() {
            try {
                string url = "http://checkip.dyndns.org";
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                string response = sr.ReadToEnd().Trim();
                string[] a = response.Split(':');
                string a2 = a[1].Substring(1);
                string[] a3 = a2.Split('<');
                string a4 = a3[0];
                return a4;
            } catch {
                Debug.LogWarning("Could not Get WAN ID.");
                return "";
                /*attempts++;
                if (attempts < 5) return GetWanIP();
                else return "";*/
                // Kinda jank but it should work - Dont be an idiot that's an infinite loop
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
        
        public int IpToId(string _ip) {
            int[] ipOctets = new int[4];
            string[] ipOctetsString = _ip.Split('.');
            for (int i = 0; i < ipOctets.Length; i++) {
                ipOctets[i] = int.Parse(ipOctetsString[i]);
            }

            return ipOctets[0] * 16777216 + ipOctets[1] * 65536 + ipOctets[2] * 256 + ipOctets[3];
        }

        public string IdToIp(int _id) {
            // https://support.sumologic.com/hc/en-us/community/posts/5076590459927-convert-decimal-value-to-IP-address
            int[] ipOctets = new int[4];
            ipOctets[0] = (int)((_id / 16777216) % 256);
            ipOctets[1] = (int)((_id / 65536) % 256);
            ipOctets[2] = (int)((_id / 256) % 256);
            ipOctets[3] = (int)((_id / 1) % 256);

            return ipOctets[0] + "." + ipOctets[1] + "." + ipOctets[2] + "." + ipOctets[3];
        }

        #endregion

        #region Public Functions

        public static int GetNumberOfConnectedClients() {
            int result = 0;
            for (int i = 0; i < USNL.Package.Server.Clients.Count; i++) {
                if (USNL.Package.Server.Clients[i].IsConnected)
                    result++;
            }
            return result;
        }

        public static int[] GetConnectedClientIds() {
            List<int> result = new List<int>();
            for (int i = 0; i < USNL.Package.Server.Clients.Count; i++) {
                if (USNL.Package.Server.Clients[i].IsConnected)
                    result.Add(i);
            }
            return result.ToArray();
        }

        public void ClientDisconnected(int _clientId) {
            USNL.CallbackEvents.CallOnClientDisconnectedCallbacks(_clientId);
        }

        #endregion

        #region Client Functions

        public bool GetClientConnected(int _clientId) {
            return USNL.Package.Server.Clients[_clientId].IsConnected;
        }

        public int GetClientPacketRTT(int _clientId) {
            return USNL.Package.Server.Clients[_clientId].PacketRTT;
        }

        public int GetClientSmoothPacketRTT(int _clientId) {
            return USNL.Package.Server.Clients[_clientId].SmoothPacketRTT;
        }

        #endregion
    }
}
