using System;
using System.Collections;
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

        [Header("Other")]
        [SerializeField] private TimeSpan uptime;

        // Too much memory? - adding a clear function, nvm it's just some ints
        // Index is packet Id
        private int[] totalBytesSentByPacket = new int[Enum.GetNames(typeof(USNL.Package.ServerPackets)).Length];
        private int[] totalBytesReceivedByPacket = new int[Enum.GetNames(typeof(USNL.Package.ClientPackets)).Length];

        // These are the values that can be accessed by the user
        private int[] bytesSentByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ServerPackets)).Length];
        private int[] bytesReceivedByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ClientPackets)).Length];
        private int[] packetsSentPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ServerPackets)).Length];
        private int[] packetsReceivedPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ClientPackets)).Length];

        // Index is packet Id - Temp because this is added to over a second, then reset
        private int[] tempBytesSentByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ServerPackets)).Length];
        private int[] tempBytesReceivedByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ClientPackets)).Length];
        private int[] tempPacketsSentPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ServerPackets)).Length];
        private int[] tempPacketsReceivedPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ClientPackets)).Length];

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
        public TimeSpan Uptime { get => uptime; set => uptime = value; }

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

        private void OnEnable() { USNL.CallbackEvents.OnPingPacket += OnPingPacketReceived; }
        private void OnDisable() { USNL.CallbackEvents.OnPingPacket -= OnPingPacketReceived; }

        #endregion

        #region Bytes and Packets

        private IEnumerator BytesAndPacketsPerSecondCoroutine() {
            while (true) {
                // Clear existing data
                bytesSentPerSecond = 0;
                bytesReceivedPerSecond = 0;

                totalPacketsSentPerSecond = 0;
                totalPacketsReceivedPerSecond = 0;

                bytesSentByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ServerPackets)).Length];
                bytesReceivedByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ClientPackets)).Length];

                packetsSentPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ServerPackets)).Length];
                packetsReceivedPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ClientPackets)).Length];

                uptime = ServerManager.instance.TimeOfStartup.Subtract(DateTime.Now);

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
                tempBytesSentByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ServerPackets)).Length];
                tempBytesReceivedByPacketPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ClientPackets)).Length];
                tempPacketsSentPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ServerPackets)).Length];
                tempPacketsReceivedPerSecond = new int[Enum.GetNames(typeof(USNL.Package.ClientPackets)).Length];

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

        private void OnPingPacketReceived(object _packetObject) {
            USNL.Package.PingPacket _pingPacket = (USNL.Package.PingPacket)_packetObject;

            // If this is to determine Client -> Server ping time, send packet back
            if (_pingPacket.SendPingBack)
                USNL.Package.PacketSend.Ping(_pingPacket.FromClient, false);

            if (_pingPacket.PreviousPingValue > 0)
                Package.Server.Clients[_pingPacket.FromClient].NewPing(_pingPacket.PreviousPingValue);
        }

        #endregion
    }
}