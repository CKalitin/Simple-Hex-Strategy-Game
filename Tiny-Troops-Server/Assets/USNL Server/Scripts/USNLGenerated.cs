using System.Collections.Generic;
using UnityEngine;

#region Packets
namespace USNL {
    #region Packet Enums

    public enum ClientPackets {
        WelcomeReceived,
        Ping,
        ClientInput,
        PlayerSetupInfo,
        PlayerReady,
        BuildStructure,
        DestroyStructure,
        StructureAction,
        UnitPathfind,
    }

    public enum ServerPackets {
        Welcome,
        Ping,
        ServerInfo,
        DisconnectClient,
        SyncedObjectInstantiate,
        SyncedObjectDestroy,
        SyncedObjectInterpolationMode,
        SyncedObjectVec2PosUpdate,
        SyncedObjectVec3PosUpdate,
        SyncedObjectRotZUpdate,
        SyncedObjectRotUpdate,
        SyncedObjectVec2ScaleUpdate,
        SyncedObjectVec3ScaleUpdate,
        SyncedObjectVec2PosInterpolation,
        SyncedObjectVec3PosInterpolation,
        SyncedObjectRotZInterpolation,
        SyncedObjectRotInterpolation,
        SyncedObjectVec2ScaleInterpolation,
        SyncedObjectVec3ScaleInterpolation,
        MatchUpdate,
        Countdown,
        PlayerReady,
        PlayerInfo,
        Tiles,
        Resources,
        BuildStructure,
        StructureAction,
        UnitPathfind,
        SetUnitLocation,
        UnitHealth,
        StructureHealth,
        GameEnded,
        StructureConstruction,
    }

    #endregion

    #region Packet Structs

    public struct PlayerSetupInfoPacket {
        private int fromClient;

        private string username;
        private Vector3 color;

        public PlayerSetupInfoPacket(int _fromClient, string _username, Vector3 _color) {
            fromClient = _fromClient;
            username = _username;
            color = _color;
        }

        public int FromClient { get => fromClient; set => fromClient = value; }
        public string Username { get => username; set => username = value; }
        public Vector3 Color { get => color; set => color = value; }
    }

    public struct PlayerReadyPacket {
        private int fromClient;

        private bool ready;

        public PlayerReadyPacket(int _fromClient, bool _ready) {
            fromClient = _fromClient;
            ready = _ready;
        }

        public int FromClient { get => fromClient; set => fromClient = value; }
        public bool Ready { get => ready; set => ready = value; }
    }

    public struct BuildStructurePacket {
        private int fromClient;

        private int playerID;
        private Vector2 targetTileLocation;
        private int structureID;

        public BuildStructurePacket(int _fromClient, int _playerID, Vector2 _targetTileLocation, int _structureID) {
            fromClient = _fromClient;
            playerID = _playerID;
            targetTileLocation = _targetTileLocation;
            structureID = _structureID;
        }

        public int FromClient { get => fromClient; set => fromClient = value; }
        public int PlayerID { get => playerID; set => playerID = value; }
        public Vector2 TargetTileLocation { get => targetTileLocation; set => targetTileLocation = value; }
        public int StructureID { get => structureID; set => structureID = value; }
    }

    public struct DestroyStructurePacket {
        private int fromClient;

        private int playerID;
        private Vector2 targetTileLocation;

        public DestroyStructurePacket(int _fromClient, int _playerID, Vector2 _targetTileLocation) {
            fromClient = _fromClient;
            playerID = _playerID;
            targetTileLocation = _targetTileLocation;
        }

        public int FromClient { get => fromClient; set => fromClient = value; }
        public int PlayerID { get => playerID; set => playerID = value; }
        public Vector2 TargetTileLocation { get => targetTileLocation; set => targetTileLocation = value; }
    }

    public struct StructureActionPacket {
        private int fromClient;

        private int actionID;
        private Vector2 targetTileLocation;
        private int[] configurationInts;

        public StructureActionPacket(int _fromClient, int _actionID, Vector2 _targetTileLocation, int[] _configurationInts) {
            fromClient = _fromClient;
            actionID = _actionID;
            targetTileLocation = _targetTileLocation;
            configurationInts = _configurationInts;
        }

        public int FromClient { get => fromClient; set => fromClient = value; }
        public int ActionID { get => actionID; set => actionID = value; }
        public Vector2 TargetTileLocation { get => targetTileLocation; set => targetTileLocation = value; }
        public int[] ConfigurationInts { get => configurationInts; set => configurationInts = value; }
    }

    public struct UnitPathfindPacket {
        private int fromClient;

        private int[] unitUUIDs;
        private Vector2 targetTileLocation;

        public UnitPathfindPacket(int _fromClient, int[] _unitUUIDs, Vector2 _targetTileLocation) {
            fromClient = _fromClient;
            unitUUIDs = _unitUUIDs;
            targetTileLocation = _targetTileLocation;
        }

        public int FromClient { get => fromClient; set => fromClient = value; }
        public int[] UnitUUIDs { get => unitUUIDs; set => unitUUIDs = value; }
        public Vector2 TargetTileLocation { get => targetTileLocation; set => targetTileLocation = value; }
    }


    #endregion

    #region Packet Send

    public static class PacketSend {
        #region TCP & UDP Send Functions
    
        private static void SendTCPData(int _toClient, USNL.Package.Packet _packet) {
            _packet.WriteLength();
            USNL.Package.Server.Clients[_toClient].Tcp.SendData(_packet);
            if (USNL.Package.Server.Clients[_toClient].IsConnected) { NetworkDebugInfo.instance.PacketSent(_packet.PacketId, _packet.Length()); }
        }
    
        private static void SendTCPDataToAll(USNL.Package.Packet _packet) {
            _packet.WriteLength();
            for (int i = 0; i < USNL.Package.Server.MaxClients; i++) {
                USNL.Package.Server.Clients[i].Tcp.SendData(_packet);
                if (USNL.Package.Server.Clients[i].IsConnected) { NetworkDebugInfo.instance.PacketSent(_packet.PacketId, _packet.Length()); }
            }
        }
    
        private static void SendTCPDataToAll(int _excpetClient, USNL.Package.Packet _packet) {
            _packet.WriteLength();
            for (int i = 0; i < USNL.Package.Server.MaxClients; i++) {
                if (i != _excpetClient) {
                    USNL.Package.Server.Clients[i].Tcp.SendData(_packet);
                    if (USNL.Package.Server.Clients[i].IsConnected) { NetworkDebugInfo.instance.PacketSent(_packet.PacketId, _packet.Length()); }
                }
            }
        }
    
        private static void SendUDPData(int _toClient, USNL.Package.Packet _packet) {
            _packet.WriteLength();
            USNL.Package.Server.Clients[_toClient].Udp.SendData(_packet);
            if (USNL.Package.Server.Clients[_toClient].IsConnected) { NetworkDebugInfo.instance.PacketSent(_packet.PacketId, _packet.Length()); }
        }
    
        private static void SendUDPDataToAll(USNL.Package.Packet _packet) {
            _packet.WriteLength();
            for (int i = 0; i < USNL.Package.Server.MaxClients; i++) {
                USNL.Package.Server.Clients[i].Udp.SendData(_packet);
                if (USNL.Package.Server.Clients[i].IsConnected) { NetworkDebugInfo.instance.PacketSent(_packet.PacketId, _packet.Length()); }
            }
        }
    
        private static void SendUDPDataToAll(int _excpetClient, USNL.Package.Packet _packet) {
            _packet.WriteLength();
            for (int i = 0; i < USNL.Package.Server.MaxClients; i++) {
                if (i != _excpetClient) {
                    USNL.Package.Server.Clients[i].Udp.SendData(_packet);
                    if (USNL.Package.Server.Clients[i].IsConnected) { NetworkDebugInfo.instance.PacketSent(_packet.PacketId, _packet.Length()); }
                }
            }
        }
    
        #endregion
    
        public static void MatchUpdate(int _toClient, int _matchState) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.MatchUpdate)) {
                _packet.Write(_matchState);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void Countdown(int[] _startTimeArray, float _duration, string _countdownTag) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.Countdown)) {
                _packet.Write(_startTimeArray);
                _packet.Write(_duration);
                _packet.Write(_countdownTag);

                SendTCPDataToAll(_packet);
            }
        }

        public static void PlayerReady(int _clientID, bool _ready) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.PlayerReady)) {
                _packet.Write(_clientID);
                _packet.Write(_ready);

                SendTCPDataToAll(_packet);
            }
        }

        public static void PlayerInfo(int _clientID, string _username, int _score, Vector3 _color) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.PlayerInfo)) {
                _packet.Write(_clientID);
                _packet.Write(_username);
                _packet.Write(_score);
                _packet.Write(_color);

                SendTCPDataToAll(_packet);
            }
        }

        public static void Tiles(int _toClient, int[] _tileIDs, Vector2[] _tileLocations) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.Tiles)) {
                _packet.Write(_tileIDs);
                _packet.Write(_tileLocations);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void Resources(int _toClient, int _playerID, float[] _supplys, float[] _demands) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.Resources)) {
                _packet.Write(_playerID);
                _packet.Write(_supplys);
                _packet.Write(_demands);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void BuildStructure(int _toClient, int _playerID, Vector2 _targetTileLocation, int _structureID) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.BuildStructure)) {
                _packet.Write(_playerID);
                _packet.Write(_targetTileLocation);
                _packet.Write(_structureID);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void StructureAction(int _playerID, Vector2 _targetTileLocation, int _actionID, int[] _configurationInts) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.StructureAction)) {
                _packet.Write(_playerID);
                _packet.Write(_targetTileLocation);
                _packet.Write(_actionID);
                _packet.Write(_configurationInts);

                SendTCPDataToAll(_packet);
            }
        }

        public static void UnitPathfind(int[] _unitUUIDs, Vector2 _targetTileLocation, Vector2[] _path) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.UnitPathfind)) {
                _packet.Write(_unitUUIDs);
                _packet.Write(_targetTileLocation);
                _packet.Write(_path);

                SendTCPDataToAll(_packet);
            }
        }

        public static void SetUnitLocation(int _unitUUID, Vector2 _targetTileLocation, Vector2 _position) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SetUnitLocation)) {
                _packet.Write(_unitUUID);
                _packet.Write(_targetTileLocation);
                _packet.Write(_position);

                SendTCPDataToAll(_packet);
            }
        }

        public static void UnitHealth(int _unitUUID, float _health, float _maxHealth) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.UnitHealth)) {
                _packet.Write(_unitUUID);
                _packet.Write(_health);
                _packet.Write(_maxHealth);

                SendTCPDataToAll(_packet);
            }
        }

        public static void StructureHealth(Vector2 _location, float _health, float _maxHealth) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.StructureHealth)) {
                _packet.Write(_location);
                _packet.Write(_health);
                _packet.Write(_maxHealth);

                SendTCPDataToAll(_packet);
            }
        }

        public static void GameEnded(int _winnerPlayerID) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.GameEnded)) {
                _packet.Write(_winnerPlayerID);

                SendTCPDataToAll(_packet);
            }
        }

        public static void StructureConstruction(Vector2 _location, float _percentage) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.StructureConstruction)) {
                _packet.Write(_location);
                _packet.Write(_percentage);

                SendTCPDataToAll(_packet);
            }
        }
        }

    #endregion
}

namespace USNL.Package {
    #region Packet Enums
    public enum ClientPackets {
        WelcomeReceived,
        Ping,
        ClientInput,
        PlayerSetupInfo,
        PlayerReady,
        BuildStructure,
        DestroyStructure,
        StructureAction,
        UnitPathfind,
    }

    public enum ServerPackets {
        Welcome,
        Ping,
        ServerInfo,
        DisconnectClient,
        SyncedObjectInstantiate,
        SyncedObjectDestroy,
        SyncedObjectInterpolationMode,
        SyncedObjectVec2PosUpdate,
        SyncedObjectVec3PosUpdate,
        SyncedObjectRotZUpdate,
        SyncedObjectRotUpdate,
        SyncedObjectVec2ScaleUpdate,
        SyncedObjectVec3ScaleUpdate,
        SyncedObjectVec2PosInterpolation,
        SyncedObjectVec3PosInterpolation,
        SyncedObjectRotZInterpolation,
        SyncedObjectRotInterpolation,
        SyncedObjectVec2ScaleInterpolation,
        SyncedObjectVec3ScaleInterpolation,
        MatchUpdate,
        Countdown,
        PlayerReady,
        PlayerInfo,
        Tiles,
        Resources,
        BuildStructure,
        StructureAction,
        UnitPathfind,
        SetUnitLocation,
        UnitHealth,
        StructureHealth,
        GameEnded,
        StructureConstruction,
    }
    #endregion

    #region Packet Structs

    public struct WelcomeReceivedPacket {
        private int fromClient;

        private int clientIdCheck;

        public WelcomeReceivedPacket(int _fromClient, int _clientIdCheck) {
            fromClient = _fromClient;
            clientIdCheck = _clientIdCheck;
        }

        public int FromClient { get => fromClient; set => fromClient = value; }
        public int ClientIdCheck { get => clientIdCheck; set => clientIdCheck = value; }
    }

    public struct PingPacket {
        private int fromClient;

        private bool sendPingBack;
        private int previousPingValue;

        public PingPacket(int _fromClient, bool _sendPingBack, int _previousPingValue) {
            fromClient = _fromClient;
            sendPingBack = _sendPingBack;
            previousPingValue = _previousPingValue;
        }

        public int FromClient { get => fromClient; set => fromClient = value; }
        public bool SendPingBack { get => sendPingBack; set => sendPingBack = value; }
        public int PreviousPingValue { get => previousPingValue; set => previousPingValue = value; }
    }

    public struct ClientInputPacket {
        private int fromClient;

        private int[] keycodesDown;
        private int[] keycodesUp;

        public ClientInputPacket(int _fromClient, int[] _keycodesDown, int[] _keycodesUp) {
            fromClient = _fromClient;
            keycodesDown = _keycodesDown;
            keycodesUp = _keycodesUp;
        }

        public int FromClient { get => fromClient; set => fromClient = value; }
        public int[] KeycodesDown { get => keycodesDown; set => keycodesDown = value; }
        public int[] KeycodesUp { get => keycodesUp; set => keycodesUp = value; }
    }


    #endregion

    #region Packet Handlers

    public static class PacketHandlers {
       public delegate void PacketHandler(USNL.Package.Packet _packet);
        public static List<PacketHandler> packetHandlers = new List<PacketHandler>() {
            { WelcomeReceived },
            { Ping },
            { ClientInput },
            { PlayerSetupInfo },
            { PlayerReady },
            { BuildStructure },
            { DestroyStructure },
            { StructureAction },
            { UnitPathfind },
        };

        public static void WelcomeReceived(Packet _packet) {
            int clientIdCheck = _packet.ReadInt();

            WelcomeReceivedPacket welcomeReceivedPacket = new WelcomeReceivedPacket(_packet.FromClient, clientIdCheck);
            PacketManager.instance.PacketReceived(_packet, welcomeReceivedPacket);
        }

        public static void Ping(Packet _packet) {
            bool sendPingBack = _packet.ReadBool();
            int previousPingValue = _packet.ReadInt();

            PingPacket pingPacket = new PingPacket(_packet.FromClient, sendPingBack, previousPingValue);
            PacketManager.instance.PacketReceived(_packet, pingPacket);
        }

        public static void ClientInput(Packet _packet) {
            int[] keycodesDown = _packet.ReadInts();
            int[] keycodesUp = _packet.ReadInts();

            ClientInputPacket clientInputPacket = new ClientInputPacket(_packet.FromClient, keycodesDown, keycodesUp);
            PacketManager.instance.PacketReceived(_packet, clientInputPacket);
        }

        public static void PlayerSetupInfo(Packet _packet) {
            string username = _packet.ReadString();
            Vector3 color = _packet.ReadVector3();

            PlayerSetupInfoPacket playerSetupInfoPacket = new PlayerSetupInfoPacket(_packet.FromClient, username, color);
            PacketManager.instance.PacketReceived(_packet, playerSetupInfoPacket);
        }

        public static void PlayerReady(Packet _packet) {
            bool ready = _packet.ReadBool();

            PlayerReadyPacket playerReadyPacket = new PlayerReadyPacket(_packet.FromClient, ready);
            PacketManager.instance.PacketReceived(_packet, playerReadyPacket);
        }

        public static void BuildStructure(Packet _packet) {
            int playerID = _packet.ReadInt();
            Vector2 targetTileLocation = _packet.ReadVector2();
            int structureID = _packet.ReadInt();

            BuildStructurePacket buildStructurePacket = new BuildStructurePacket(_packet.FromClient, playerID, targetTileLocation, structureID);
            PacketManager.instance.PacketReceived(_packet, buildStructurePacket);
        }

        public static void DestroyStructure(Packet _packet) {
            int playerID = _packet.ReadInt();
            Vector2 targetTileLocation = _packet.ReadVector2();

            DestroyStructurePacket destroyStructurePacket = new DestroyStructurePacket(_packet.FromClient, playerID, targetTileLocation);
            PacketManager.instance.PacketReceived(_packet, destroyStructurePacket);
        }

        public static void StructureAction(Packet _packet) {
            int actionID = _packet.ReadInt();
            Vector2 targetTileLocation = _packet.ReadVector2();
            int[] configurationInts = _packet.ReadInts();

            StructureActionPacket structureActionPacket = new StructureActionPacket(_packet.FromClient, actionID, targetTileLocation, configurationInts);
            PacketManager.instance.PacketReceived(_packet, structureActionPacket);
        }

        public static void UnitPathfind(Packet _packet) {
            int[] unitUUIDs = _packet.ReadInts();
            Vector2 targetTileLocation = _packet.ReadVector2();

            UnitPathfindPacket unitPathfindPacket = new UnitPathfindPacket(_packet.FromClient, unitUUIDs, targetTileLocation);
            PacketManager.instance.PacketReceived(_packet, unitPathfindPacket);
        }
    }

    #endregion

    #region Packet Send

    public static class PacketSend {
        #region TCP & UDP Send Functions
    
        private static void SendTCPData(int _toClient, USNL.Package.Packet _packet) {
            _packet.WriteLength();
            USNL.Package.Server.Clients[_toClient].Tcp.SendData(_packet);
            if (USNL.Package.Server.Clients[_toClient].IsConnected) { NetworkDebugInfo.instance.PacketSent(_packet.PacketId, _packet.Length()); }
        }
    
        private static void SendTCPDataToAll(USNL.Package.Packet _packet) {
            _packet.WriteLength();
            for (int i = 0; i < USNL.Package.Server.MaxClients; i++) {
                USNL.Package.Server.Clients[i].Tcp.SendData(_packet);
                if (USNL.Package.Server.Clients[i].IsConnected) { NetworkDebugInfo.instance.PacketSent(_packet.PacketId, _packet.Length()); }
            }
        }
    
        private static void SendTCPDataToAll(int _excpetClient, USNL.Package.Packet _packet) {
            _packet.WriteLength();
            for (int i = 0; i < USNL.Package.Server.MaxClients; i++) {
                if (i != _excpetClient) {
                    USNL.Package.Server.Clients[i].Tcp.SendData(_packet);
                    if (USNL.Package.Server.Clients[i].IsConnected) { NetworkDebugInfo.instance.PacketSent(_packet.PacketId, _packet.Length()); }
                }
            }
        }
    
        private static void SendUDPData(int _toClient, USNL.Package.Packet _packet) {
            _packet.WriteLength();
            USNL.Package.Server.Clients[_toClient].Udp.SendData(_packet);
            if (USNL.Package.Server.Clients[_toClient].IsConnected) { NetworkDebugInfo.instance.PacketSent(_packet.PacketId, _packet.Length()); }
        }
    
        private static void SendUDPDataToAll(USNL.Package.Packet _packet) {
            _packet.WriteLength();
            for (int i = 0; i < USNL.Package.Server.MaxClients; i++) {
                USNL.Package.Server.Clients[i].Udp.SendData(_packet);
                if (USNL.Package.Server.Clients[i].IsConnected) { NetworkDebugInfo.instance.PacketSent(_packet.PacketId, _packet.Length()); }
            }
        }
    
        private static void SendUDPDataToAll(int _excpetClient, USNL.Package.Packet _packet) {
            _packet.WriteLength();
            for (int i = 0; i < USNL.Package.Server.MaxClients; i++) {
                if (i != _excpetClient) {
                    USNL.Package.Server.Clients[i].Udp.SendData(_packet);
                    if (USNL.Package.Server.Clients[i].IsConnected) { NetworkDebugInfo.instance.PacketSent(_packet.PacketId, _packet.Length()); }
                }
            }
        }
    
        #endregion
    
        public static void Welcome(int _toClient, string _welcomeMessage, string _serverName, int _clientId) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.Welcome)) {
                _packet.Write(_welcomeMessage);
                _packet.Write(_serverName);
                _packet.Write(_clientId);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void Ping(int _toClient, bool _placeholder) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.Ping)) {
                _packet.Write(_placeholder);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void ServerInfo(int _toClient, string _serverName, int[] _connectedClientIds, int _maxClients, bool _serverFull) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.ServerInfo)) {
                _packet.Write(_serverName);
                _packet.Write(_connectedClientIds);
                _packet.Write(_maxClients);
                _packet.Write(_serverFull);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void DisconnectClient(int _toClient, string _disconnectMessage) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.DisconnectClient)) {
                _packet.Write(_disconnectMessage);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void SyncedObjectInstantiate(int _toClient, string _syncedObjectTag, int _syncedObjectUUID, Vector3 _position, Quaternion _rotation, Vector3 _scale) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectInstantiate)) {
                _packet.Write(_syncedObjectTag);
                _packet.Write(_syncedObjectUUID);
                _packet.Write(_position);
                _packet.Write(_rotation);
                _packet.Write(_scale);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void SyncedObjectDestroy(int _toClient, int _syncedObjectUUID) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectDestroy)) {
                _packet.Write(_syncedObjectUUID);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void SyncedObjectInterpolationMode(int _toClient, bool _serverInterpolation) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectInterpolationMode)) {
                _packet.Write(_serverInterpolation);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void SyncedObjectVec2PosUpdate(int[] _syncedObjectUUIDs, Vector2[] _positions) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectVec2PosUpdate)) {
                _packet.Write(_syncedObjectUUIDs);
                _packet.Write(_positions);

                SendUDPDataToAll(_packet);
            }
        }

        public static void SyncedObjectVec3PosUpdate(int[] _syncedObjectUUIDs, Vector3[] _positions) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectVec3PosUpdate)) {
                _packet.Write(_syncedObjectUUIDs);
                _packet.Write(_positions);

                SendUDPDataToAll(_packet);
            }
        }

        public static void SyncedObjectRotZUpdate(int[] _syncedObjectUUIDs, float[] _rotations) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectRotZUpdate)) {
                _packet.Write(_syncedObjectUUIDs);
                _packet.Write(_rotations);

                SendUDPDataToAll(_packet);
            }
        }

        public static void SyncedObjectRotUpdate(int[] _syncedObjectUUIDs, Vector3[] _rotations) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectRotUpdate)) {
                _packet.Write(_syncedObjectUUIDs);
                _packet.Write(_rotations);

                SendUDPDataToAll(_packet);
            }
        }

        public static void SyncedObjectVec2ScaleUpdate(int[] _syncedObjectUUIDs, Vector2[] _scales) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectVec2ScaleUpdate)) {
                _packet.Write(_syncedObjectUUIDs);
                _packet.Write(_scales);

                SendUDPDataToAll(_packet);
            }
        }

        public static void SyncedObjectVec3ScaleUpdate(int[] _syncedObjectUUIDs, Vector3[] _scales) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectVec3ScaleUpdate)) {
                _packet.Write(_syncedObjectUUIDs);
                _packet.Write(_scales);

                SendUDPDataToAll(_packet);
            }
        }

        public static void SyncedObjectVec2PosInterpolation(int[] _syncedObjectUUIDs, Vector2[] _interpolatePositions) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectVec2PosInterpolation)) {
                _packet.Write(_syncedObjectUUIDs);
                _packet.Write(_interpolatePositions);

                SendUDPDataToAll(_packet);
            }
        }

        public static void SyncedObjectVec3PosInterpolation(int[] _syncedObjectUUIDs, Vector3[] _interpolatePositions) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectVec3PosInterpolation)) {
                _packet.Write(_syncedObjectUUIDs);
                _packet.Write(_interpolatePositions);

                SendUDPDataToAll(_packet);
            }
        }

        public static void SyncedObjectRotZInterpolation(int[] _syncedObjectUUIDs, float[] _interpolateRotations) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectRotZInterpolation)) {
                _packet.Write(_syncedObjectUUIDs);
                _packet.Write(_interpolateRotations);

                SendUDPDataToAll(_packet);
            }
        }

        public static void SyncedObjectRotInterpolation(int[] _syncedObjectUUIDs, Vector3[] _interpolateRotations) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectRotInterpolation)) {
                _packet.Write(_syncedObjectUUIDs);
                _packet.Write(_interpolateRotations);

                SendUDPDataToAll(_packet);
            }
        }

        public static void SyncedObjectVec2ScaleInterpolation(int[] _syncedObjectUUIDs, Vector2[] _interpolateScales) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectVec2ScaleInterpolation)) {
                _packet.Write(_syncedObjectUUIDs);
                _packet.Write(_interpolateScales);

                SendUDPDataToAll(_packet);
            }
        }

        public static void SyncedObjectVec3ScaleInterpolation(int[] _syncedObjectUUIDs, Vector3[] _interpolateScales) {
            using (USNL.Package.Packet _packet = new USNL.Package.Packet((int)ServerPackets.SyncedObjectVec3ScaleInterpolation)) {
                _packet.Write(_syncedObjectUUIDs);
                _packet.Write(_interpolateScales);

                SendUDPDataToAll(_packet);
            }
        }
    }

    #endregion
}

#endregion Packets

#region Callbacks

namespace USNL {
    public static class CallbackEvents {
        public delegate void CallbackEvent(object _param);

        public static CallbackEvent[] PacketCallbackEvents = {
            CallOnWelcomeReceivedPacketCallbacks,
            CallOnPingPacketCallbacks,
            CallOnClientInputPacketCallbacks,
            CallOnPlayerSetupInfoPacketCallbacks,
            CallOnPlayerReadyPacketCallbacks,
            CallOnBuildStructurePacketCallbacks,
            CallOnDestroyStructurePacketCallbacks,
            CallOnStructureActionPacketCallbacks,
            CallOnUnitPathfindPacketCallbacks,
        };

        public static event CallbackEvent OnServerStarted;
        public static event CallbackEvent OnServerStopped;
        public static event CallbackEvent OnClientConnected;
        public static event CallbackEvent OnClientDisconnected;

        public static event CallbackEvent OnWelcomeReceivedPacket;
        public static event CallbackEvent OnPingPacket;
        public static event CallbackEvent OnClientInputPacket;
        public static event CallbackEvent OnPlayerSetupInfoPacket;
        public static event CallbackEvent OnPlayerReadyPacket;
        public static event CallbackEvent OnBuildStructurePacket;
        public static event CallbackEvent OnDestroyStructurePacket;
        public static event CallbackEvent OnStructureActionPacket;
        public static event CallbackEvent OnUnitPathfindPacket;

        public static void CallOnServerStartedCallbacks(object _param) { if (OnServerStarted != null) { OnServerStarted(_param); } }
        public static void CallOnServerStoppedCallbacks(object _param) { if (OnServerStopped != null) { OnServerStopped(_param); } }
        public static void CallOnClientConnectedCallbacks(object _param) { if (OnClientConnected != null) { OnClientConnected(_param); } }
        public static void CallOnClientDisconnectedCallbacks(object _param) { if (OnClientDisconnected != null) { OnClientDisconnected(_param); } }

        public static void CallOnWelcomeReceivedPacketCallbacks(object _param) { if (OnWelcomeReceivedPacket != null) { OnWelcomeReceivedPacket(_param); } }
        public static void CallOnPingPacketCallbacks(object _param) { if (OnPingPacket != null) { OnPingPacket(_param); } }
        public static void CallOnClientInputPacketCallbacks(object _param) { if (OnClientInputPacket != null) { OnClientInputPacket(_param); } }
        public static void CallOnPlayerSetupInfoPacketCallbacks(object _param) { if (OnPlayerSetupInfoPacket != null) { OnPlayerSetupInfoPacket(_param); } }
        public static void CallOnPlayerReadyPacketCallbacks(object _param) { if (OnPlayerReadyPacket != null) { OnPlayerReadyPacket(_param); } }
        public static void CallOnBuildStructurePacketCallbacks(object _param) { if (OnBuildStructurePacket != null) { OnBuildStructurePacket(_param); } }
        public static void CallOnDestroyStructurePacketCallbacks(object _param) { if (OnDestroyStructurePacket != null) { OnDestroyStructurePacket(_param); } }
        public static void CallOnStructureActionPacketCallbacks(object _param) { if (OnStructureActionPacket != null) { OnStructureActionPacket(_param); } }
        public static void CallOnUnitPathfindPacketCallbacks(object _param) { if (OnUnitPathfindPacket != null) { OnUnitPathfindPacket(_param); } }
    }
}

#endregion
