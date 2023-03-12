using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USNL {
    public class SyncedObjectManager : MonoBehaviour {
        #region Core

        public static SyncedObjectManager instance;

        [SerializeField] private Package.SyncedObjectPrefabs syncedObjectsPrefabs;

        private Dictionary<int, Transform> syncedObjects = new Dictionary<int, Transform>();

        [SerializeField] private bool clientSideInterpolation = false;

        public bool ClientSideInterpolation { get => clientSideInterpolation; set => clientSideInterpolation = value; }

        private void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Debug.Log("Synced Object Manager instance already exists, destroying object!");
                Destroy(this);
            }

            syncedObjectsPrefabs.GenerateSyncedObjectsDict();
        }

        private void OnEnable() {
            USNL.CallbackEvents.OnDisconnected += DisconnectedFromServer;

            USNL.CallbackEvents.OnSyncedObjectInterpolationModePacket += OnSyncedObjectInterpolationModePacket;

            USNL.CallbackEvents.OnSyncedObjectInstantiatePacket += OnSyncedObjectInstantiatePacket;
            USNL.CallbackEvents.OnSyncedObjectDestroyPacket += OnSyncedObjectDestroyPacket;

            USNL.CallbackEvents.OnSyncedObjectVec2PosUpdatePacket += OnSyncedObjectVec2PosUpdatePacket;
            USNL.CallbackEvents.OnSyncedObjectVec3PosUpdatePacket += OnSyncedObjectVec3PosUpdatePacket;
            USNL.CallbackEvents.OnSyncedObjectRotZUpdatePacket += OnSyncedObjectRotZUpdatePacket;
            USNL.CallbackEvents.OnSyncedObjectRotUpdatePacket += OnSyncedObjectRotUpdatePacket;
            USNL.CallbackEvents.OnSyncedObjectVec2ScaleUpdatePacket += OnSyncedObjectVec2ScaleUpdatePacket;
            USNL.CallbackEvents.OnSyncedObjectVec3ScaleUpdatePacket += OnSyncedObjectVec3ScaleUpdatePacket;

            USNL.CallbackEvents.OnSyncedObjectVec2PosInterpolationPacket += OnSyncedObjectVec2PosInterpolationPacket;
            USNL.CallbackEvents.OnSyncedObjectVec3PosInterpolationPacket += OnSyncedObjectVec3PosInterpolationPacket;
            USNL.CallbackEvents.OnSyncedObjectRotZInterpolationPacket += OnSyncedObjectRotZInterpolationPacket;
            USNL.CallbackEvents.OnSyncedObjectRotInterpolationPacket += OnSyncedObjectRotInterpolationPacket;
            USNL.CallbackEvents.OnSyncedObjectVec2ScaleInterpolationPacket += OnSyncedObjectVec2ScaleInterpolationPacket;
            USNL.CallbackEvents.OnSyncedObjectVec3ScaleInterpolationPacket += OnSyncedObjectVec3ScaleInterpolationPacket;
        }

        private void OnDisable() {
            USNL.CallbackEvents.OnDisconnected -= DisconnectedFromServer;

            USNL.CallbackEvents.OnSyncedObjectInterpolationModePacket -= OnSyncedObjectInterpolationModePacket;

            USNL.CallbackEvents.OnSyncedObjectInstantiatePacket -= OnSyncedObjectInstantiatePacket;
            USNL.CallbackEvents.OnSyncedObjectDestroyPacket -= OnSyncedObjectDestroyPacket;

            USNL.CallbackEvents.OnSyncedObjectVec2PosUpdatePacket -= OnSyncedObjectVec2PosUpdatePacket;
            USNL.CallbackEvents.OnSyncedObjectVec3PosUpdatePacket -= OnSyncedObjectVec3PosUpdatePacket;
            USNL.CallbackEvents.OnSyncedObjectRotZUpdatePacket -= OnSyncedObjectRotZUpdatePacket;
            USNL.CallbackEvents.OnSyncedObjectRotUpdatePacket -= OnSyncedObjectRotUpdatePacket;
            USNL.CallbackEvents.OnSyncedObjectVec2ScaleUpdatePacket -= OnSyncedObjectVec2ScaleUpdatePacket;
            USNL.CallbackEvents.OnSyncedObjectVec3ScaleUpdatePacket -= OnSyncedObjectVec3ScaleUpdatePacket;

            USNL.CallbackEvents.OnSyncedObjectVec2PosInterpolationPacket -= OnSyncedObjectVec2PosInterpolationPacket;
            USNL.CallbackEvents.OnSyncedObjectVec3PosInterpolationPacket -= OnSyncedObjectVec3PosInterpolationPacket;
            USNL.CallbackEvents.OnSyncedObjectRotZInterpolationPacket -= OnSyncedObjectRotZInterpolationPacket;
            USNL.CallbackEvents.OnSyncedObjectRotInterpolationPacket -= OnSyncedObjectRotInterpolationPacket;
            USNL.CallbackEvents.OnSyncedObjectVec2ScaleInterpolationPacket -= OnSyncedObjectVec2ScaleInterpolationPacket;
            USNL.CallbackEvents.OnSyncedObjectVec3ScaleInterpolationPacket -= OnSyncedObjectVec3ScaleInterpolationPacket;
        }

        #endregion

        #region Public Functions

        public SyncedObject GetSyncedObject(int _syncedObjectUUID) {
            if (syncedObjects.ContainsKey(_syncedObjectUUID)) {
                return syncedObjects[_syncedObjectUUID].GetComponent<SyncedObject>();
            } else {
                return null;
            }
        }

        #endregion

        #region Synced Object Management

        private void OnSyncedObjectInterpolationModePacket(object _packetObject) {
            USNL.Package.SyncedObjectInterpolationModePacket packet = (USNL.Package.SyncedObjectInterpolationModePacket)_packetObject;
            //clientSideInterpolation = !packet.ServerInterpolation;
        }

        private void OnSyncedObjectInstantiatePacket(object _packetObject) {
            USNL.Package.SyncedObjectInstantiatePacket _packet = (USNL.Package.SyncedObjectInstantiatePacket)_packetObject;

            if (syncedObjects.ContainsKey(_packet.SyncedObjectUUID)) return;
            
            try {
                GameObject newSyncedObject = Instantiate(syncedObjectsPrefabs.SyncedObjects[_packet.SyncedObjectTag], _packet.Position, _packet.Rotation);
                newSyncedObject.transform.localScale = _packet.Scale;

                // If object does not have a Syncede Object Component
                if (newSyncedObject.GetComponent<USNL.SyncedObject>() == null) { newSyncedObject.gameObject.AddComponent<USNL.SyncedObject>(); }
                newSyncedObject.gameObject.GetComponent<USNL.SyncedObject>().SyncedObjectUuid = _packet.SyncedObjectUUID;

                syncedObjects.Add(_packet.SyncedObjectUUID, newSyncedObject.transform);
            } catch {
                Debug.Log("Synced Object Manager: Could not instantiate synced object with tag: " + _packet.SyncedObjectTag);
            }
        }

        private void OnSyncedObjectDestroyPacket(object _packetObject) {
            USNL.Package.SyncedObjectDestroyPacket _packet = (USNL.Package.SyncedObjectDestroyPacket)_packetObject;

            if (syncedObjects.ContainsKey(_packet.SyncedObjectUUID)){
                Destroy(syncedObjects[_packet.SyncedObjectUUID].gameObject);
                syncedObjects.Remove(_packet.SyncedObjectUUID);
            }
        }

        private void DisconnectedFromServer(object _object) {
            ClearLocalSyncedObjects();
        }

        public void ClearLocalSyncedObjects() {
            if (syncedObjects.Count <= 0) return;
            foreach (KeyValuePair<int, Transform> syncedObject in syncedObjects) {
                Destroy(syncedObject.Value.gameObject);
            }
            syncedObjects.Clear();
        }

        #endregion

        #region Synced Object Updates

        private void OnSyncedObjectVec2PosUpdatePacket(object _packetObject) {
            USNL.Package.SyncedObjectVec2PosUpdatePacket _packet = (USNL.Package.SyncedObjectVec2PosUpdatePacket)_packetObject;

            for (int i = 0; i < _packet.SyncedObjectUUIDs.Length; i++) {
                if (syncedObjects.ContainsKey(_packet.SyncedObjectUUIDs[i])) {
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].position = new Vector3(_packet.Positions[i].x, _packet.Positions[i].y, syncedObjects[_packet.SyncedObjectUUIDs[i]].position.z);
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].GetComponent<USNL.SyncedObject>().PositionUpdate(_packet.Positions[i]);
                }
            }
        }

        private void OnSyncedObjectVec3PosUpdatePacket(object _packetObject) {
            USNL.Package.SyncedObjectVec3PosUpdatePacket _packet = (USNL.Package.SyncedObjectVec3PosUpdatePacket)_packetObject;

            for (int i = 0; i < _packet.SyncedObjectUUIDs.Length; i++) {
                if (syncedObjects.ContainsKey(_packet.SyncedObjectUUIDs[i])) {
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].position = _packet.Positions[i];
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].GetComponent<USNL.SyncedObject>().PositionUpdate(_packet.Positions[i]);
                }
            }
        }

        private void OnSyncedObjectRotZUpdatePacket(object _packetObject) {
            USNL.Package.SyncedObjectRotZUpdatePacket _packet = (USNL.Package.SyncedObjectRotZUpdatePacket)_packetObject;

            for (int i = 0; i < _packet.SyncedObjectUUIDs.Length; i++) {
                if (syncedObjects.ContainsKey(_packet.SyncedObjectUUIDs[i])) {
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].rotation = Quaternion.RotateTowards(syncedObjects[_packet.SyncedObjectUUIDs[i]].rotation, Quaternion.Euler(new Vector3(syncedObjects[_packet.SyncedObjectUUIDs[i]].rotation.x, syncedObjects[_packet.SyncedObjectUUIDs[i]].rotation.y, _packet.Rotations[i])), 999999f);
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].GetComponent<USNL.SyncedObject>().RotationUpdate(new Vector3(syncedObjects[_packet.SyncedObjectUUIDs[i]].rotation.x, syncedObjects[_packet.SyncedObjectUUIDs[i]].rotation.y, _packet.Rotations[i]));
                }
            }
        }

        private void OnSyncedObjectRotUpdatePacket(object _packetObject) {
            USNL.Package.SyncedObjectRotUpdatePacket _packet = (USNL.Package.SyncedObjectRotUpdatePacket)_packetObject;

            for (int i = 0; i < _packet.SyncedObjectUUIDs.Length; i++) {
                if (syncedObjects.ContainsKey(_packet.SyncedObjectUUIDs[i])) {
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].rotation = Quaternion.RotateTowards(syncedObjects[_packet.SyncedObjectUUIDs[i]].rotation, Quaternion.Euler(_packet.Rotations[i]), 999999f);
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].GetComponent<USNL.SyncedObject>().RotationUpdate(_packet.Rotations[i]);
                }
            }
        }

        private void OnSyncedObjectVec2ScaleUpdatePacket(object _packetObject) {
            USNL.Package.SyncedObjectVec2ScaleUpdatePacket _packet = (USNL.Package.SyncedObjectVec2ScaleUpdatePacket)_packetObject;

            for (int i = 0; i < _packet.SyncedObjectUUIDs.Length; i++) {
                if (syncedObjects.ContainsKey(_packet.SyncedObjectUUIDs[i])) {
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].localScale = new Vector3(_packet.Scales[i].x, _packet.Scales[i].y, syncedObjects[_packet.SyncedObjectUUIDs[i]].localScale.z);
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].GetComponent<USNL.SyncedObject>().ScaleUpdate(_packet.Scales[i]);
                }
            }
        }

        private void OnSyncedObjectVec3ScaleUpdatePacket(object _packetObject) {
            USNL.Package.SyncedObjectVec3ScaleUpdatePacket _packet = (USNL.Package.SyncedObjectVec3ScaleUpdatePacket)_packetObject;

            for (int i = 0; i < _packet.SyncedObjectUUIDs.Length; i++) {
                if (syncedObjects.ContainsKey(_packet.SyncedObjectUUIDs[i])) {
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].localScale = _packet.Scales[i];
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].GetComponent<USNL.SyncedObject>().ScaleUpdate(_packet.Scales[i]);
                }
            }
        }

        #endregion

        #region Synced Object Interpolation Packets

        private void OnSyncedObjectVec2PosInterpolationPacket(object _packetObject) {
            USNL.Package.SyncedObjectVec2PosInterpolationPacket _packet = (USNL.Package.SyncedObjectVec2PosInterpolationPacket)_packetObject;
            for (int i = 0; i < _packet.SyncedObjectUUIDs.Length; i++) {
                if (syncedObjects.ContainsKey(_packet.SyncedObjectUUIDs[i])) {
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].GetComponent<USNL.SyncedObject>().PositionInterpolationUpdate(_packet.InterpolatePositions[i]);
                }
            }
        }

        private void OnSyncedObjectVec3PosInterpolationPacket(object _packetObject) {
            USNL.Package.SyncedObjectVec3PosInterpolationPacket _packet = (USNL.Package.SyncedObjectVec3PosInterpolationPacket)_packetObject;

            for (int i = 0; i < _packet.SyncedObjectUUIDs.Length; i++) {
                if (syncedObjects.ContainsKey(_packet.SyncedObjectUUIDs[i])) {
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].GetComponent<USNL.SyncedObject>().PositionInterpolationUpdate(_packet.InterpolatePositions[i]);
                }
            }
        }

        private void OnSyncedObjectRotZInterpolationPacket(object _packetObject) {
            USNL.Package.SyncedObjectRotZInterpolationPacket _packet = (USNL.Package.SyncedObjectRotZInterpolationPacket)_packetObject;

            for (int i = 0; i < _packet.SyncedObjectUUIDs.Length; i++) {
                if (syncedObjects.ContainsKey(_packet.SyncedObjectUUIDs[i])) {
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].GetComponent<USNL.SyncedObject>().RotationInterpolationUpdate(new Vector3(syncedObjects[_packet.SyncedObjectUUIDs[i]].rotation.x, syncedObjects[_packet.SyncedObjectUUIDs[i]].rotation.y, _packet.InterpolateRotations[i]));
                }
            }
        }

        private void OnSyncedObjectRotInterpolationPacket(object _packetObject) {
            USNL.Package.SyncedObjectRotInterpolationPacket _packet = (USNL.Package.SyncedObjectRotInterpolationPacket)_packetObject;

            for (int i = 0; i < _packet.SyncedObjectUUIDs.Length; i++) {
                if (syncedObjects.ContainsKey(_packet.SyncedObjectUUIDs[i])) {
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].GetComponent<USNL.SyncedObject>().RotationInterpolationUpdate(_packet.InterpolateRotations[i]);
                }
            }
        }

        private void OnSyncedObjectVec2ScaleInterpolationPacket(object _packetObject) {
            USNL.Package.SyncedObjectVec2ScaleInterpolationPacket _packet = (USNL.Package.SyncedObjectVec2ScaleInterpolationPacket)_packetObject;
            for (int i = 0; i < _packet.SyncedObjectUUIDs.Length; i++) {
                if (syncedObjects.ContainsKey(_packet.SyncedObjectUUIDs[i])) {
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].GetComponent<USNL.SyncedObject>().ScaleInterpolationUpdate(_packet.InterpolateScales[i]);
                }
            }
        }

        private void OnSyncedObjectVec3ScaleInterpolationPacket(object _packetObject) {
            USNL.Package.SyncedObjectVec3ScaleInterpolationPacket _packet = (USNL.Package.SyncedObjectVec3ScaleInterpolationPacket)_packetObject;

            for (int i = 0; i < _packet.SyncedObjectUUIDs.Length; i++) {
                if (syncedObjects.ContainsKey(_packet.SyncedObjectUUIDs[i])) {
                    syncedObjects[_packet.SyncedObjectUUIDs[i]].GetComponent<USNL.SyncedObject>().ScaleInterpolationUpdate(_packet.InterpolateScales[i]);
                }
            }
        }

        #endregion
    }
}