using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace USNL.Package {
    public class USNLDebugDisplay : MonoBehaviour {
        [Header("Debug Menu")]
        [SerializeField] GameObject debugMenuParent;

        [Header("Client Manager Bool Values")]
        [SerializeField] private GameObject isConnected;
        [SerializeField] private GameObject isAttempingConnection;
        [SerializeField] private GameObject isHost;
        [SerializeField] private GameObject isServerActive;
        [SerializeField] private GameObject isMigratingHost;
        [SerializeField] private GameObject isBecomingHost;

        [Header("Network Info")]
        [SerializeField] private TextMeshProUGUI totalBytesSent;
        [SerializeField] private TextMeshProUGUI totalBytesReceived;
        [Space]
        [SerializeField] private TextMeshProUGUI bytesSentPerSecond;
        [SerializeField] private TextMeshProUGUI bytesReceivedPerSecond;
        [Space]
        [SerializeField] private TextMeshProUGUI totalPacketsSent;
        [SerializeField] private TextMeshProUGUI totalPacketsReceived;
        [Space]
        [SerializeField] private TextMeshProUGUI totalPacketsSentPerSecond;
        [SerializeField] private TextMeshProUGUI totalPacketsReceivedPerSecond;
        [Space]
        [SerializeField] private TextMeshProUGUI packetRTT;
        [Tooltip("Average of last 5 pings")]
        [SerializeField] private TextMeshProUGUI smoothPacketRTT;
        [Space]
        [SerializeField] private TextMeshProUGUI wanClientId;
        [SerializeField] private TextMeshProUGUI lanClientId;
        [SerializeField] private TextMeshProUGUI timeConnected;

        [Header("Connection Info")]
        [SerializeField] private TMP_InputField ip;
        [SerializeField] private TMP_InputField port;

        private void Update() {
            // Toggle debug menu visible
            if (Input.GetKey(KeyCode.LeftControl) & Input.GetKeyDown(KeyCode.BackQuote)) {
                debugMenuParent.SetActive(!debugMenuParent.activeSelf);
            }

            // If Debug Menu visible, update values
            if (debugMenuParent.activeSelf) {
                #region Client Mangaer Bool Values
                isConnected.SetActive(ClientManager.instance.IsConnected);
                isAttempingConnection.SetActive(ClientManager.instance.IsAttemptingConnection);
                isHost.SetActive(ClientManager.instance.IsHost);
                isServerActive.SetActive(ClientManager.instance.ServerData.IsServerActive);
                isMigratingHost.SetActive(ClientManager.instance.IsMigratingHost);
                isBecomingHost.SetActive(ClientManager.instance.IsBecomingHost);
                #endregion

                #region Network Info
                totalBytesSent.text = RoundBytesToString(USNL.NetworkDebugInfo.instance.TotalBytesSent);
                totalBytesReceived.text = RoundBytesToString(USNL.NetworkDebugInfo.instance.TotalBytesReceived);
                bytesSentPerSecond.text = RoundBytesToString(USNL.NetworkDebugInfo.instance.BytesSentPerSecond);
                bytesReceivedPerSecond.text = RoundBytesToString(USNL.NetworkDebugInfo.instance.BytesReceivedPerSecond);

                totalPacketsSent.text = String.Format("{0:n0}", USNL.NetworkDebugInfo.instance.TotalPacketsSent);
                totalPacketsReceived.text = String.Format("{0:n0}", USNL.NetworkDebugInfo.instance.TotalPacketsReceived);

                totalPacketsSentPerSecond.text = String.Format("{0:n0}", USNL.NetworkDebugInfo.instance.TotalPacketsSentPerSecond);
                totalPacketsReceivedPerSecond.text = String.Format("{0:n0}", USNL.NetworkDebugInfo.instance.TotalPacketsReceivedPerSecond);

                packetRTT.text = String.Format("{0:n0}", USNL.NetworkDebugInfo.instance.PacketRTT) + "ms";
                smoothPacketRTT.text = String.Format("{0:n0}", USNL.NetworkDebugInfo.instance.SmoothPacketRTT) + "ms";

                if (ClientManager.instance.IsConnected)
                    timeConnected.text = USNL.NetworkDebugInfo.instance.TimeConnected.ToString(@"hh\:mm\:ss");
                else
                    timeConnected.text = "00:00:00";

                wanClientId.text = ClientManager.instance.WanClientId.ToString();
                lanClientId.text = ClientManager.instance.LanClientId.ToString();
                #endregion

                // Read text from json file ???? what
            }
        }

        private string RoundBytesToString(int _bytes) {
            string output = "";

            if (_bytes > 1000000000) {
                output = String.Format("{0:n}", _bytes / 1000000000f) + "GB";
            } else if (_bytes > 1000000) {
                output = String.Format("{0:n}", _bytes / 1000000f) + "MB";
            } else if (_bytes > 1000) {
                output = String.Format("{0:n}", _bytes / 1000f) + "KB";
            } else {
                output = String.Format("{0:n0}", _bytes) + "B";
            }

            return output;
        }

        public void ConnectButton() {
            try {
                ClientManager.instance.ConnectToServer(int.Parse(ip.text), Int32.Parse(port.text));
            } catch (Exception _ex) {
                Debug.LogError($"Could not connect to server via Debug Menu, likely improper port.\n{_ex}");
            }
        }

        public void DisconnectButton() {
            ClientManager.instance.DisconnectFromServer();
        }

        public void CloseButton() {
            debugMenuParent.SetActive(false);
        }

        public void OpenButton() {
            debugMenuParent.SetActive(true);
        }

        public void StartHostingButton() {
            ClientManager.instance.LaunchServer();
        }

        public void StopHostingButton() {
            ClientManager.instance.CloseServer();
        }
    }
}
