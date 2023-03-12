using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USNL {
    public class SyncedObjectManager : MonoBehaviour {
        #region Variables

        public static SyncedObjectManager instance;

        [Header("Synced Object Updates")]
        [Tooltip("If Synced Objects should be updated in Vector3s, or Vector2s for a 2D game.")]
        [SerializeField] private bool vector2Mode;
        [Space]
        [Tooltip("Minimum ammount a Synced Object needs to move before it is updated on the Clients.")]
        [SerializeField] private float minPosChange = 0.001f;
        [Tooltip("Minimum ammount a Synced Object needs to rotate before it is updated on the Clients.")]
        [SerializeField] private float minRotChange = 0.001f;
        [Tooltip("Minimum ammount a Synced Object needs to change in scale before it is updated on the Clients.")]
        [SerializeField] private float minScaleChange = 0.001f;

        [Header("Synced Object Updates")]
        [Tooltip("Second between Synced Object data being sent to clients.")]
        [SerializeField] private float syncedObjectClientUpdateRate = 0.1f;
        [Space]
        [Tooltip("Nth Synced Object update sends an Interpolation Update as well. Interpolation data may not need to be sent every Synced Object update.")]
        [SerializeField] private int nthSyncedObjectUpdatePerInterpolation = 2;
        [Space]
        [Tooltip("Nth Synced Object update sends a position update.")]
        [SerializeField] private int nthPositionUpdateRate = 3;
        [Tooltip("Nth Synced Object update sends a rotation update.")]
        [SerializeField] private int nthRotationUpdateRate = 1;
        [Tooltip("Nth Synced Object update sends a scale update.")]
        [SerializeField] private int nthScaleUpdateRate = 3;
        [Space]
        [Tooltip("Nth Interpolation update sends a position interpolation update.")]
        [SerializeField] private int nthPositionInterpolateRate = 3;
        [Tooltip("Nth Interpolation update sends a rotation interpolation update.")]
        [SerializeField] private int nthRotationInterpolateRate = 1;
        [Tooltip("Nth Interpolation update sends a scale interpolation update.")]
        [SerializeField] private int nthScaleInterpolateRate = 3;

        [Header("Interpolation Toggles")]
        [Tooltip("Server-side interpolation is better than Client-side because there's more frames to work with.\nClient-side interpolation does not support rotation interpolation.\nIf the server is running at or over 30 Synced Object updates per second it is wise to disable this.\nThis is only updates on Server Startup.")]
        [SerializeField] private bool serverSideInterpolation = true;
        [Space]
        [Tooltip("This overrides local Synced Object toggles.")]
        [SerializeField] private bool interpolatePosition = true;
        [Tooltip("This overrides local Synced Object toggles.")]
        [SerializeField] private bool interpolateRotation = false;
        [Tooltip("This overrides local Synced Object toggles.")]
        [SerializeField] private bool interpolateScale = true;

        private List<SyncedObject> syncedObjects = new List<SyncedObject>();

        private int syncedObjectUpdatesTotal = 0;
        private int syncedObjectInterpolationUpdatesTotal = 0;

        // These are public so Synced Object can use them as a reference parameter
        [HideInInspector] public List<SyncedObject> soVec2PosUpdate = new List<SyncedObject>();
        [HideInInspector] public List<SyncedObject> soVec3PosUpdate = new List<SyncedObject>();
        [HideInInspector] public List<SyncedObject> soRotZUpdate = new List<SyncedObject>();
        [HideInInspector] public List<SyncedObject> soRotUpdate = new List<SyncedObject>();
        [HideInInspector] public List<SyncedObject> soVec2ScaleUpdate = new List<SyncedObject>();
        [HideInInspector] public List<SyncedObject> soVec3ScaleUpdate = new List<SyncedObject>();

        [HideInInspector] public List<SyncedObject> soVec2PosInterpolation = new List<SyncedObject>();
        [HideInInspector] public List<SyncedObject> soVec3PosInterpolation = new List<SyncedObject>();
        [HideInInspector] public List<SyncedObject> soRotZInterpolation = new List<SyncedObject>();
        [HideInInspector] public List<SyncedObject> soRotInterpolation = new List<SyncedObject>();
        [HideInInspector] public List<SyncedObject> soVec2ScaleInterpolation = new List<SyncedObject>();
        [HideInInspector] public List<SyncedObject> soVec3ScaleInterpolation = new List<SyncedObject>();

        public bool Vector2Mode { get => vector2Mode; set => vector2Mode = value; }
        public bool ServerSideInterpolation { get => serverSideInterpolation; set => serverSideInterpolation = value; }
        public float MinPosChange { get => minPosChange; set => minPosChange = value; }
        public float MinRotChange { get => minRotChange; set => minRotChange = value; }
        public float MinScaleChange { get => minScaleChange; set => minScaleChange = value; }
        public bool InterpolatePosition { get => interpolatePosition; set => interpolatePosition = value; }
        public bool InterpolateRotation { get => interpolateRotation; set => interpolateRotation = value; }
        public bool InterpolateScale { get => interpolateScale; set => interpolateScale = value; }

        #endregion

        #region Core

        private void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Debug.Log("Synced Object Manager instance already exists, destroying object!");
                Destroy(this);
            }
        }

        private IEnumerator UpdateSyncedObjects() {
            while (Package.Server.ServerData.IsServerActive) {
                yield return new WaitForSeconds(syncedObjectClientUpdateRate);
                SendSyncedObjectUpdatePackets();

                syncedObjectUpdatesTotal++;
                if (syncedObjectUpdatesTotal % nthSyncedObjectUpdatePerInterpolation == 0) {
                    syncedObjectInterpolationUpdatesTotal++;
                    SendSyncedObjectInterpolationPackets();
                }
            }
        }

        private void OnServerStarted(object _object) {
            StartCoroutine(UpdateSyncedObjects());
        }

        private void OnEnable() { USNL.CallbackEvents.OnClientConnected += OnClientConnected; USNL.CallbackEvents.OnServerStarted += OnServerStarted; }
        private void OnDisable() { USNL.CallbackEvents.OnClientConnected -= OnClientConnected; }

        #endregion
        
        #region Synced Object Management


        private void SendSyncedObjectUpdatePackets() {
            CallSyncedObjectUpdateFunctions();
            if (syncedObjectUpdatesTotal % nthPositionUpdateRate == 0)
                SendSOPositionUpdate();
            if (syncedObjectUpdatesTotal % nthRotationUpdateRate == 0)
                SendSORotationUpdate();
            if (syncedObjectUpdatesTotal % nthScaleUpdateRate == 0)
                SendSOScaleUpdate();
        }

        private void SendSyncedObjectInterpolationPackets() {
            if (!serverSideInterpolation) { return; }

            CallSyncedObjectUpdateInterpolationFunctions();
            if (syncedObjectInterpolationUpdatesTotal % nthPositionInterpolateRate == 0)
                SendSOPositionInterpolation();
            if (syncedObjectInterpolationUpdatesTotal % nthRotationInterpolateRate == 0)
                SendSORotationInterpolation();
            if (syncedObjectInterpolationUpdatesTotal % nthScaleInterpolateRate == 0)
                SendSOScaleInterpolation();
        }

        private void SendSOPositionUpdate() {
            if (soVec2PosUpdate.Count > 0) { SyncedObjectVec2PosUpdates(); }
            if (soVec3PosUpdate.Count > 0) { SyncedObjectVec3PosUpdates(); }
        }

        private void SendSORotationUpdate() {
            if (soRotZUpdate.Count > 0) { SyncedObjectRotZUpdates(); }
            if (soRotUpdate.Count > 0) { SyncedObjectRotUpdates(); }
        }

        private void SendSOScaleUpdate() {
            if (soVec2ScaleUpdate.Count > 0) { SyncedObjectVec2ScaleUpdates(); }
            if (soVec3ScaleUpdate.Count > 0) { SyncedObjectVec3ScaleUpdates(); }
        }

        private void SendSOPositionInterpolation() {
            if (soVec2PosInterpolation.Count > 0) { SendSOVec2PosInterpolation(); }
            if (soVec3PosInterpolation.Count > 0) { SendSOVec3PosInterpolation(); }
        }

        private void SendSORotationInterpolation() {
            if (soRotZInterpolation.Count > 0) { SendSORotZInterpolation(); }
            if (soRotInterpolation.Count > 0) { SendSORotInterpolation(); }
        }

        private void SendSOScaleInterpolation() {
            if (soVec2ScaleInterpolation.Count > 0) { SendSOVec2ScaleInterpolation(); }
            if (soVec3ScaleInterpolation.Count > 0) { SendSOVec3ScaleInterpolation(); }
        }

        private void CallSyncedObjectUpdateFunctions() {
            for (int i = 0; i < syncedObjects.Count; i++) {
                syncedObjects[i].UpdateSyncedObject();
            }
        }

        private void CallSyncedObjectUpdateInterpolationFunctions() {
            for (int i = 0; i < syncedObjects.Count; i++) {
                syncedObjects[i].UpdateInterpolation();
            }
        }

        private void OnClientConnected(object _clientIdObject) {
            SendAllSyncedObjectsToClient((int)_clientIdObject);
        }

        private void SendAllSyncedObjectsToClient(int _toClient) {
            Package.PacketSend.SyncedObjectInterpolationMode(_toClient, serverSideInterpolation);
            for (int i = 0; i < syncedObjects.Count; i++) {
                Package.PacketSend.SyncedObjectInstantiate(_toClient, syncedObjects[i].SyncedObjectTag, syncedObjects[i].SyncedObjectUUID, syncedObjects[i].transform.position, syncedObjects[i].transform.rotation, syncedObjects[i].transform.lossyScale);
            }
        }

        public void InstantiateSyncedObject(SyncedObject _so) {
            syncedObjects.Add(_so);

            for (int i = 0; i < Package.Server.MaxClients; i++) {
                if (Package.Server.Clients[i].IsConnected) {
                    Package.PacketSend.SyncedObjectInstantiate(i, _so.SyncedObjectTag, _so.SyncedObjectUUID, _so.transform.position, _so.transform.rotation, _so.transform.lossyScale);
                }
            }
        }

        public void DestroySyncedObject(SyncedObject _so) {
            syncedObjects.Remove(_so);

            for (int i = 0; i < Package.Server.MaxClients; i++) {
                if (Package.Server.Clients[i].IsConnected) {
                    Package.PacketSend.SyncedObjectDestroy(i, _so.SyncedObjectUUID);
                }
            }
        }

        #endregion

        #region Synced Object Updates

        private void SyncedObjectVec2PosUpdates() {
            // 1300 is the max Vector2 updates per packet because of the 4096 byte limit
            int max = 1300; // Max values per packets
            for (int i = 0; i < Mathf.CeilToInt((float)soVec2PosUpdate.Count / (float)max); i++) {
                // If there is more than max values to send in this packet, length is max, otherwise length is the amount of values
                int length = soVec2PosUpdate.Count;
                if (Mathf.CeilToInt(((float)soVec2PosUpdate.Count / (float)max) - i * max) > 1) { length = max; }

                int[] indexes = new int[length];
                Vector2[] values = new Vector2[length];

                for (int x = i * max; x < length + (i * max); x++) {
                    indexes[x] = soVec2PosUpdate[x].SyncedObjectUUID;
                    values[x] = soVec2PosUpdate[x].transform.position;
                }

                USNL.Package.PacketSend.SyncedObjectVec2PosUpdate(indexes, values);
            }
            soVec2PosUpdate.Clear();
        }

        private void SyncedObjectVec3PosUpdates() {
            // 1000 is the max Vector3 updates per packet because of the 4096 byte limit
            int max = 1000; // Max values per packets
            for (int i = 0; i < Mathf.CeilToInt((float)soVec3PosUpdate.Count / (float)max); i++) {
                // If there is more than max values to send in this packet, length is max, otherwise length is the amount of values
                int length = soVec3PosUpdate.Count;
                if (Mathf.CeilToInt(((float)soVec3PosUpdate.Count / (float)max) - i * max) > 1) { length = max; }

                int[] indexes = new int[length];
                Vector3[] values = new Vector3[length];

                for (int x = i * max; x < length + (i * max); x++) {
                    indexes[x] = soVec3PosUpdate[x].SyncedObjectUUID;
                    values[x] = soVec3PosUpdate[x].transform.position;
                }

                USNL.Package.PacketSend.SyncedObjectVec3PosUpdate(indexes, values);
            }
            soVec3PosUpdate.Clear();
        }

        private void SyncedObjectRotZUpdates() {
            // 2000 is the max float updates per packet because of the 4096 byte limit
            int max = 2000; // Max values per packets
            for (int i = 0; i < Mathf.CeilToInt((float)soRotZUpdate.Count / (float)max); i++) {
                // If there is more than max values to send in this packet, length is max, otherwise length is the amount of values
                int length = soRotZUpdate.Count;
                if (Mathf.CeilToInt(((float)soRotZUpdate.Count / (float)max) - i * max) > 1) { length = max; }

                int[] indexes = new int[length];
                float[] values = new float[length];

                for (int x = i * max; x < length + (i * max); x++) {
                    indexes[x] = soRotZUpdate[x].SyncedObjectUUID;
                    values[x] = soRotZUpdate[x].transform.eulerAngles.z;
                }

                USNL.Package.PacketSend.SyncedObjectRotZUpdate(indexes, values);
            }
            soRotZUpdate.Clear();
        }

        private void SyncedObjectRotUpdates() {
            // 750 is the max quaternion updates per packet because of the 4096 byte limit
            int max = 750; // Max values per packets
            for (int i = 0; i < Mathf.CeilToInt((float)soRotUpdate.Count / (float)max); i++) {
                // If there is more than max values to send in this packet, length is max, otherwise length is the amount of values
                int length = soRotUpdate.Count;
                if (Mathf.CeilToInt(((float)soRotUpdate.Count / (float)max) - i * max) > 1) { length = max; }

                int[] indexes = new int[length];
                Vector3[] values = new Vector3[length];

                for (int x = i * max; x < length + (i * max); x++) {
                    indexes[x] = soRotUpdate[x].SyncedObjectUUID;
                    values[x] = soRotUpdate[x].transform.eulerAngles;
                }

                USNL.Package.PacketSend.SyncedObjectRotUpdate(indexes, values);
            }
            soRotUpdate.Clear();
        }

        private void SyncedObjectVec2ScaleUpdates() {
            // 1300 is the max Vector2 updates per packet because of the 4096 byte limit
            int max = 1300; // Max values per packets
            for (int i = 0; i < Mathf.CeilToInt((float)soVec2ScaleUpdate.Count / (float)max); i++) {
                // If there is more than max values to send in this packet, length is max, otherwise length is the amount of values
                int length = soVec2ScaleUpdate.Count;
                if (Mathf.CeilToInt(((float)soVec2ScaleUpdate.Count / (float)max) - i * max) > 1) { length = max; }

                int[] indexes = new int[length];
                Vector2[] values = new Vector2[length];

                for (int x = i * max; x < length + (i * max); x++) {
                    indexes[x] = soVec2ScaleUpdate[x].SyncedObjectUUID;
                    values[x] = soVec2ScaleUpdate[x].transform.lossyScale;
                }

                USNL.Package.PacketSend.SyncedObjectVec2ScaleUpdate(indexes, values);
            }
            soVec2ScaleUpdate.Clear();
        }

        private void SyncedObjectVec3ScaleUpdates() {
            // 1000 is the max Vector3 updates per packet because of the 4096 byte limit
            int max = 1000; // Max values per packets
            for (int i = 0; i < Mathf.CeilToInt((float)soVec3ScaleUpdate.Count / (float)max); i++) {
                // If there is more than max values to send in this packet, length is max, otherwise length is the amount of values
                int length = soVec3ScaleUpdate.Count;
                if (Mathf.CeilToInt(((float)soVec3ScaleUpdate.Count / (float)max) - i * max) > 1) { length = max; }

                int[] indexes = new int[length];
                Vector3[] values = new Vector3[length];

                for (int x = i * max; x < length + (i * max); x++) {
                    indexes[x] = soVec3ScaleUpdate[x].SyncedObjectUUID;
                    values[x] = soVec3ScaleUpdate[x].transform.lossyScale;
                }

                USNL.Package.PacketSend.SyncedObjectVec3ScaleUpdate(indexes, values);
            }
            soVec3ScaleUpdate.Clear();
        }

        #endregion

        #region Synced Object Interpolation

        private void SendSOVec2PosInterpolation() {
            // 1300 is the max Vector2 updates per packet because of the 4096 byte limit
            int max = 1300; // Max values per packets
            for (int i = 0; i < Mathf.CeilToInt((float)soVec2PosInterpolation.Count / (float)max); i++) {
                // If there is more than max values to send in this packet, length is max, otherwise length is the amount of values
                int length = soVec2PosInterpolation.Count;
                if (Mathf.CeilToInt(((float)soVec2PosInterpolation.Count / (float)max) - i * max) > 1) { length = max; }

                int[] indexes = new int[length];
                Vector2[] interpolateValues = new Vector2[length];

                for (int x = i * max; x < length + (i * max); x++) {
                    indexes[x] = soVec2PosInterpolation[x].SyncedObjectUUID;
                    interpolateValues[x] = soVec2PosInterpolation[x].PositionInterpolation;
                }

                USNL.Package.PacketSend.SyncedObjectVec2PosInterpolation(indexes, interpolateValues);
            }
            soVec2PosInterpolation.Clear();
        }

        private void SendSOVec3PosInterpolation() {
            // 1000 is the max Vector3 updates per packet because of the 4096 byte limit
            int max = 1000; // Max values per packets
            for (int i = 0; i < Mathf.CeilToInt((float)soVec3PosInterpolation.Count / (float)max); i++) {
                // If there is more than max values to send in this packet, length is max, otherwise length is the amount of values
                int length = soVec3PosInterpolation.Count;
                if (Mathf.CeilToInt(((float)soVec3PosInterpolation.Count / (float)max) - i * max) > 1) { length = max; }

                int[] indexes = new int[length];
                Vector3[] interpolateValues = new Vector3[length];

                for (int x = i * max; x < length + (i * max); x++) {
                    indexes[x] = soVec3PosInterpolation[x].SyncedObjectUUID;
                    interpolateValues[x] = soVec3PosInterpolation[x].PositionInterpolation;
                }

                USNL.Package.PacketSend.SyncedObjectVec3PosInterpolation(indexes, interpolateValues);
            }
            soVec3PosInterpolation.Clear();
        }

        private void SendSORotZInterpolation() {
            // 2000 is the max float updates per packet because of the 4096 byte limit
            int max = 2000; // Max values per packets
            for (int i = 0; i < Mathf.CeilToInt((float)soRotZInterpolation.Count / (float)max); i++) {
                // If there is more than max values to send in this packet, length is max, otherwise length is the amount of values
                int length = soRotZInterpolation.Count;
                if (Mathf.CeilToInt(((float)soRotZInterpolation.Count / (float)max) - i * max) > 1) { length = max; }

                int[] indexes = new int[length];
                float[] interpolateValues = new float[length];

                for (int x = i * max; x < length + (i * max); x++) {
                    indexes[x] = soRotZInterpolation[x].SyncedObjectUUID;
                    interpolateValues[x] = soRotZInterpolation[x].RotationInterpolation.z;
                }

                USNL.Package.PacketSend.SyncedObjectRotZInterpolation(indexes, interpolateValues);
            }
            soRotZInterpolation.Clear();
        }

        private void SendSORotInterpolation() {
            // 750 is the max quaternion updates per packet because of the 4096 byte limit
            int max = 750; // Max values per packets
            for (int i = 0; i < Mathf.CeilToInt((float)soRotInterpolation.Count / (float)max); i++) {
                // If there is more than max values to send in this packet, length is max, otherwise length is the amount of values
                int length = soRotInterpolation.Count;
                if (Mathf.CeilToInt(((float)soRotInterpolation.Count / (float)max) - i * max) > 1) { length = max; }

                int[] indexes = new int[length];
                Vector3[] interpolateValues = new Vector3[length];

                for (int x = i * max; x < length + (i * max); x++) {
                    indexes[x] = soRotInterpolation[x].SyncedObjectUUID;
                    interpolateValues[x] = soRotInterpolation[x].RotationInterpolation;
                }

                USNL.Package.PacketSend.SyncedObjectRotInterpolation(indexes, interpolateValues);
            }
            soRotInterpolation.Clear();
        }

        private void SendSOVec2ScaleInterpolation() {
            // 1300 is the max Vector2 updates per packet because of the 4096 byte limit
            int max = 1300; // Max values per packets
            for (int i = 0; i < Mathf.CeilToInt((float)soVec2ScaleInterpolation.Count / (float)max); i++) {
                // If there is more than max values to send in this packet, length is max, otherwise length is the amount of values
                int length = soVec2ScaleInterpolation.Count;
                if (Mathf.CeilToInt(((float)soVec2ScaleInterpolation.Count / (float)max) - i * max) > 1) { length = max; }

                int[] indexes = new int[length];
                Vector2[] interpolateValues = new Vector2[length];

                for (int x = i * max; x < length + (i * max); x++) {
                    indexes[x] = soVec2ScaleInterpolation[x].SyncedObjectUUID;
                    interpolateValues[x] = soVec2ScaleInterpolation[x].ScaleInterpolation;
                }

                USNL.Package.PacketSend.SyncedObjectVec2ScaleInterpolation(indexes, interpolateValues);
            }
            soVec2ScaleInterpolation.Clear();
        }

        private void SendSOVec3ScaleInterpolation() {
            // 1000 is the max Vector3 updates per packet because of the 4096 byte limit
            int max = 1000; // Max values per packets
            for (int i = 0; i < Mathf.CeilToInt((float)soVec3ScaleInterpolation.Count / (float)max); i++) {
                // If there is more than max values to send in this packet, length is max, otherwise length is the amount of values
                int length = soVec3ScaleInterpolation.Count;
                if (Mathf.CeilToInt(((float)soVec3ScaleInterpolation.Count / (float)max) - i * max) > 1) { length = max; }

                int[] indexes = new int[length];
                Vector3[] interpolateValues = new Vector3[length];

                for (int x = i * max; x < length + (i * max); x++) {
                    indexes[x] = soVec3ScaleInterpolation[x].SyncedObjectUUID;
                    interpolateValues[x] = soVec3ScaleInterpolation[x].ScaleInterpolation;
                }

                USNL.Package.PacketSend.SyncedObjectVec3ScaleInterpolation(indexes, interpolateValues);
            }
            soVec3ScaleInterpolation.Clear();
        }

        #endregion
    }
}