using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
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
        [SerializeField] private int maxPlayers;
        [SerializeField] private string welcomeMessage;
        [Space]
        [SerializeField] private bool showGUI;

        public string ServerName { get => serverName; set => serverName = value; }
        public int ServerPort { get => serverPort; set => serverPort = value; }
        public int MaxPlayers { get => maxPlayers; set => maxPlayers = value; }
        public string WelcomeMessage { get => welcomeMessage; set => welcomeMessage = value; }
        public bool ShowGUI { get => showGUI; set => showGUI = value; }
    }

    public class ServerHost {
        #region Variables

        private static Process serverProcess;

        private static ServerData serverData;
        private static bool launchingServer;

        public static ServerData GetServerData() { ReadServerDataFile(); return serverData; }
        public static bool LaunchingServer { get => launchingServer; set => launchingServer = value; }

        #endregion

        #region Server Hosting

        public static bool IsServerRunning() {
            if (serverProcess == null) return false;
            return Process.GetProcessById(serverProcess.Id) != null;
        }

        public static void LaunchServer() {
            if (IsServerRunning()) {
                UnityEngine.Debug.LogWarning("Cannot start server, server already running.");
                return;
            }

            if (!File.Exists($"{GetServerPath()}{ClientManager.instance.ServerExeName}")) {
                UnityEngine.Debug.LogError($"Server executable not found at {GetServerPath()}{ClientManager.instance.ServerExeName}");
                return;
            }

            UnityEngine.Debug.Log("Launching Server...");
            launchingServer = true;

            ClearServerDataFile();
            WriteServerConfigFile();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = $"{GetServerPath()}{ClientManager.instance.ServerExeName}";
            if (ClientManager.instance.ServerConfig.ShowGUI) startInfo.WindowStyle = ProcessWindowStyle.Normal;
            else startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            serverProcess = Process.Start(startInfo);

            Client.instance.IsHost = true;
        }

        public static void CloseServer() {
            if (!IsServerRunning()) {
                UnityEngine.Debug.LogWarning("Cannot close server, server not running.");
                return;
            }
            
            //serverProcess.CloseMainWindow();
            WriteServerQuitFile();
            serverProcess = null;

            Client.instance.IsHost = false;

            UnityEngine.Debug.Log("Closed Server.");
        }

        public static void KillServer() {
            if (!IsServerRunning()) {
                UnityEngine.Debug.LogWarning("Cannot kill server, server not running.");
                return;
            }

            serverProcess.Kill();
            serverProcess = null;

            Client.instance.IsHost = false;

            UnityEngine.Debug.Log("Killed Server. (This is not recommend, leads to buggy behaviour on clients)");
        }

        #endregion

        #region Server Files

        private static void WriteServerQuitFile() {
            StreamWriter sw = new StreamWriter($"{GetServerPath()}ServerQuit");
            sw.Write("");
            sw.Flush();
            sw.Close();
        }

        public static void ReadServerDataFile() {
            if (!File.Exists($"{GetServerPath()}serverData.json")) {
                return;
            }

            try {
                string jsonText = File.ReadAllText($"{GetServerPath()}serverData.json");

                serverData = JsonConvert.DeserializeObject<ServerData>(jsonText);
            } catch {
                // Janky way to "fix" an error but it works
            }
        }

        private static void WriteServerConfigFile() {
            string jsonText = JsonConvert.SerializeObject(ClientManager.instance.ServerConfig, Formatting.Indented);
            
            StreamWriter sw = new StreamWriter($"{GetServerPath()}ServerConfig.json");
            sw.Write(jsonText);
            sw.Flush();
            sw.Close();
        }

        private static void ClearServerDataFile() {
            serverData = new ServerData();
            string jsonText = JsonConvert.SerializeObject(ClientManager.instance.ServerData, Formatting.Indented);
            
            StreamWriter sw = new StreamWriter($"{GetServerPath()}ServerData.json");
            sw.Write(jsonText);
            sw.Flush();
            sw.Close();
        }

        #endregion

        #region Helper Functions

        private static string GetServerPath() {
            if (Application.isEditor)
                return $"{ClientManager.instance.EditorServerPath}";
            else if (ClientManager.instance.UseApplicationPath)
                return $"{GetApplicationPath()}{ClientManager.instance.ServerPath}";
            else
                return ClientManager.instance.ServerPath;
        }

        public static string GetApplicationPath() {
            string[] slicedPath = Application.dataPath.Split("/");
            string path = "";
            for (int i = 0; i < slicedPath.Length - 1; i++) {
                path += slicedPath[i] + "/";
            }

            return path;
        }

        #endregion
    }
}
