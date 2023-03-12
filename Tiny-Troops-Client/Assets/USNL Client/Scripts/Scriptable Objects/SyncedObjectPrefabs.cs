using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USNL.Package {
    [Serializable]
    public struct SyncedObjectStruct {
        public string tag;
        public GameObject prefab;
    }

    [CreateAssetMenu(fileName = "SyncedObjects", menuName = "USNL/Synced Objects", order = 0)]
    public class SyncedObjectPrefabs : ScriptableObject {
        [SerializeField] private SyncedObjectStruct[] syncedObjectStructs;

        private Dictionary<string, GameObject> syncedObjects = new Dictionary<string, GameObject>();

        public Dictionary<string, GameObject> SyncedObjects { get => syncedObjects; set => syncedObjects = value; }
        public SyncedObjectStruct[] SyncedObjectStructs { get => syncedObjectStructs; set => syncedObjectStructs = value; }

        public void GenerateSyncedObjectsDict() {
            syncedObjects.Clear();
            foreach (SyncedObjectStruct syncedObject in syncedObjectStructs) {
                if (syncedObject.tag != null && syncedObject.prefab != null)
                    syncedObjects.Add(syncedObject.tag, syncedObject.prefab);
            }
        }
    }
}
