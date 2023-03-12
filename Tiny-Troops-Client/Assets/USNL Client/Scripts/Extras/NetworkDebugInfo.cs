using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace USNL {
    public class NetworkDebugInfo : MonoBehaviour {
        #region Variables

        public static NetworkDebugInfo instance;

        [Header("Bytes")]
        [SerializeField] private int totalBytesSent;
        [SerializeField] private int totalBytesReceived;
        [Space]
        [SerializeField] private int bytesSentPerSecond;
        [SerializeField] private int bytesReceivedPerSecond;

        [Header("Packets")]
        [SerializeField] private int totalPacketsSent;
        [SerializeField] private int totalPacketsReceived;
        [Space]
        [SerializeField] private int totalPacketsSentPerSecond;
        [SerializeField] private int totalPacketsReceivedPerSecond;

        [Header("Ping")]
        [SerializeField] private int packetRTT;
        [Tooltip("Average of last 5 pings")]
        [SerializeField] private int smoothPacketRTT;

        [Header("Other")]
        [SerializeField] private TimeSpan timeConnected;

        private List<int> packetRTTs = new List<int>();
        private float packetPingSentTime = -1; // Seconds since startup when ping packet was sent

        // Too much memory? - adding a clear function, nvm it's just some ints
        // Index is packet Id
        private int[] totalBytesSentByPacket = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
        private int[] totalBytesReceivedByPacket = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];

        // These are the values that can be accessed by the user
        private int[] bytesSentByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
        private int[] bytesReceivedByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];
        private int[] packetsSentPerSecond = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
        private int[] packetsReceivedPerSecond = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];

        // Index is packet Id - Temp because this is added to over a second, then reset
        private int[] tempBytesSentByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
        private int[] tempBytesReceivedByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];
        private int[] tempPacketsSentPerSecond = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
        private int[] tempPacketsReceivedPerSecond = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];

        public int TotalBytesSent { get => totalBytesSent; set => totalBytesSent = value; }
        public int TotalBytesReceived { get => totalBytesReceived; set => totalBytesReceived = value; }

        public int BytesSentPerSecond { get => bytesSentPerSecond; set => bytesSentPerSecond = value; }
        public int BytesReceivedPerSecond { get => bytesReceivedPerSecond; set => bytesReceivedPerSecond = value; }

        public int TotalPacketsSent { get => totalPacketsSent; set => totalPacketsSent = value; }
        public int TotalPacketsReceived { get => totalPacketsReceived; set => totalPacketsReceived = value; }

        public int TotalPacketsSentPerSecond { get => totalPacketsSentPerSecond; set => totalPacketsSentPerSecond = value; }
        public int TotalPacketsReceivedPerSecond {
            get => totalPacketsReceivedPerSecond; set => totalPacketsReceivedPerSecond
                = value;
        }

        public int PacketRTT { get => packetRTT; set => packetRTT = value; }
        public int SmoothPacketRTT { get => smoothPacketRTT; set => smoothPacketRTT = value; }

        public TimeSpan TimeConnected { get => timeConnected; set => timeConnected = value; }

        public int[] BytesSentByPacketPerSecond { get => bytesSentByPacketPerSecond; set => bytesSentByPacketPerSecond = value; }
        public int[] BytesReceivedByPacketPerSecond { get => bytesReceivedByPacketPerSecond; set => bytesReceivedByPacketPerSecond = value; }
        public int[] PacketsSentPerSecond { get => packetsSentPerSecond; set => packetsSentPerSecond = value; }
        public int[] PacketsReceivedPerSecond { get => packetsReceivedPerSecond; set => packetsReceivedPerSecond = value; }

        public int[] TempBytesSentByPacketPerSecond { get => tempBytesSentByPacketPerSecond; set => tempBytesSentByPacketPerSecond = value; }
        public int[] TempBytesReceivedByPacketPerSecond { get => tempBytesReceivedByPacketPerSecond; set => tempBytesReceivedByPacketPerSecond = value; }
        public int[] TempPacketsSentPerSecond { get => tempPacketsSentPerSecond; set => tempPacketsSentPerSecond = value; }
        public int[] TempPacketsReceivedPerSecond { get => tempPacketsReceivedPerSecond; set => tempPacketsReceivedPerSecond = value; }

        #endregion

        #region Core

        private void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Debug.Log("Network Debug Info instance already exists, destroying object.");
                Destroy(this);
            }
        }

        private void Start() {
            StartCoroutine(BytesAndPacketsPerSecondCoroutine());
        }

        private void Update() {
            timeConnected = DateTime.Now.Subtract(ClientManager.instance.TimeOfConnection);
        }

        private void OnEnable() {
            USNL.CallbackEvents.OnPingPacket += OnPingPacketReceived;
            USNL.CallbackEvents.OnDisconnected += OnDisconnected;
        }

        private void OnDisable() {
            USNL.CallbackEvents.OnPingPacket -= OnPingPacketReceived;
            USNL.CallbackEvents.OnDisconnected -= OnDisconnected;
        }

        #endregion

        #region Bytes and Packets

        private IEnumerator BytesAndPacketsPerSecondCoroutine() {
            while (true) {
                if (Package.Client.instance.IsConnected) { SendPingPacket(); }

                // Clear existing data
                bytesSentPerSecond = 0;
                bytesReceivedPerSecond = 0;

                totalPacketsSentPerSecond = 0;
                totalPacketsReceivedPerSecond = 0;

                bytesSentByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
                bytesReceivedByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];

                packetsSentPerSecond = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
                packetsReceivedPerSecond = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];


                // Loop through temp data
                bytesSentByPacketPerSecond = tempBytesSentByPacketPerSecond;
                packetsSentPerSecond = tempPacketsSentPerSecond;
                for (int i = 0; i < tempBytesSentByPacketPerSecond.Length; i++) {
                    bytesSentPerSecond += tempBytesSentByPacketPerSecond[i];
                }

                bytesReceivedByPacketPerSecond = tempBytesReceivedByPacketPerSecond;
                packetsReceivedPerSecond = tempPacketsReceivedPerSecond;
                for (int i = 0; i < tempBytesReceivedByPacketPerSecond.Length; i++) {
                    bytesReceivedPerSecond += tempBytesReceivedByPacketPerSecond[i];
                }

                for (int i = 0; i < tempPacketsSentPerSecond.Length; i++) {
                    totalPacketsSentPerSecond += tempPacketsSentPerSecond[i];
                }

                for (int i = 0; i < tempPacketsReceivedPerSecond.Length; i++) {
                    totalPacketsReceivedPerSecond += tempPacketsReceivedPerSecond[i];
                }

                // Reset temp variables
                tempBytesSentByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
                tempBytesReceivedByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];
                tempPacketsSentPerSecond = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
                tempPacketsReceivedPerSecond = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];

                yield return new WaitForSecondsRealtime(1f);
            }
        }

        public void PacketSent(int _packetId, int _bytesLength) {
            totalBytesSent += _bytesLength;

            totalBytesSentByPacket[_packetId] += _bytesLength;
            tempBytesSentByPacketPerSecond[_packetId] += _bytesLength;
            tempPacketsSentPerSecond[_packetId]++;
            totalPacketsSent++;
        }

        public void PacketReceived(int _packetId, int _bytesLength) {
            totalBytesReceived += _bytesLength;

            totalBytesReceivedByPacket[_packetId] += _bytesLength;
            tempBytesReceivedByPacketPerSecond[_packetId] += _bytesLength;
            tempPacketsReceivedPerSecond[_packetId]++;
            totalPacketsReceived++;
        }

        #endregion

        #region Ping

        private void SendPingPacket() {
            // If packet has been received packetPingSentTime will be -1
            if (packetPingSentTime < 0) {
                packetPingSentTime = Time.realtimeSinceStartup;
                if (packetRTT > 0) USNL.Package.PacketSend.Ping(true, packetRTT);
                else USNL.Package.PacketSend.Ping(true, -1);
            } else {
                // Set smoothPacketRTT
                if (packetRTTs.Count > 5) { packetRTTs.RemoveAt(0); }
                packetRTTs.Add(-1);
                smoothPacketRTT = packetRTTs.Sum() / packetRTTs.Count;

                Debug.Log("Packet RTT/ping is greater than 1000ms");
            }
        }

        private void OnPingPacketReceived(object _packetObject) {
            USNL.Package.PingPacket pingPacket = (USNL.Package.PingPacket)_packetObject;
            
            packetRTT = Mathf.RoundToInt((Time.realtimeSinceStartup - packetPingSentTime) * 1000); // Round to ms (*1000), instead of seconds

            // Set smoothPacketRTT
            if (packetRTTs.Count > 5) { packetRTTs.RemoveAt(0); }
            packetRTTs.Add(packetRTT);
            smoothPacketRTT = packetRTTs.Sum() / packetRTTs.Count;

            packetPingSentTime = -1; // Reset Packet Ping Sent Time so SendPingPacket() works
        }

        #endregion

        #region Other

        private void OnDisconnected(object _object) {
            packetPingSentTime = -1; // Reset Packet Ping Sent Time so SendPingPacket() works
            ResetData();
        }

        public void ResetData() {
            totalBytesSent = 0;
            totalBytesReceived = 0;

            bytesSentPerSecond = 0;
            bytesReceivedPerSecond = 0;


            totalPacketsSent = 0;
            totalPacketsReceived = 0;

            totalPacketsSentPerSecond = 0;
            totalPacketsReceivedPerSecond = 0;

            packetRTT = 0;
            smoothPacketRTT = 0;

            packetRTTs = new List<int>();
            packetPingSentTime = -1;

            totalBytesSentByPacket = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
            totalBytesReceivedByPacket = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];

            bytesSentByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
            bytesReceivedByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];
            packetsSentPerSecond = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
            packetsReceivedPerSecond = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];

            tempBytesSentByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
            tempBytesReceivedByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];
            tempPacketsSentPerSecond = new int[Enum.GetNames(typeof(USNL.ClientPackets)).Length];
            tempPacketsReceivedPerSecond = new int[Enum.GetNames(typeof(USNL.ServerPackets)).Length];

        }

        #endregion
    }
}
