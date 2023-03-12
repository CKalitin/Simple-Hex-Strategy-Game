using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace USNL.Package {
    [CustomEditor(typeof(SyncedObjectPrefabs))]
    [CanEditMultipleObjects]
    public class SyncedObjectPrefabsEditor : Editor {
        SyncedObjectPrefabs sop;

        // Display Variables
        float verticalBreakSize = 5;

        GUIStyle tooltipStyle;
        GUIStyle helpBoxStyle;

        GUIStyle labelStyle;
        float labelWidth;
        float labelFieldWidth;
        float labelHeight;

        GUIStyle syncedObjectLabelStyle;
        float syncedObjectElementHeight;
        float syncedObjectTagWidth;
        float syncedObjectGameObjectFieldWidth;

        // Real Variables
        int targetSyncedObjectsLength;
        
        private void OnEnable() {
            sop = (SyncedObjectPrefabs)target;

            targetSyncedObjectsLength = sop.SyncedObjectStructs.Length;

        }
        
        public override void OnInspectorGUI() {
            #region Variables

            tooltipStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            helpBoxStyle = new GUIStyle(GUI.skin.GetStyle("HelpBox")) { fontSize = 13 };

            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            labelWidth = Screen.width * 0.6f;
            labelFieldWidth = Screen.width * 0.3f;
            labelHeight = 20;
            
            syncedObjectLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Normal };
            syncedObjectElementHeight = 19;
            syncedObjectTagWidth = Screen.width * 0.45f;
            syncedObjectGameObjectFieldWidth = Screen.width * 0.45f;

            #endregion

            VerticalBreak();

            SyncedObjectsField();
            
            VerticalBreak();

            HelpBox();

            VerticalBreak();

            // This is so the Scriptable Objects save when Unity closes
            EditorUtility.SetDirty(target);
        }
        
        #region Synced Objects
        
        private void SyncedObjectsField() {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Synced Objects", labelStyle, GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
            GUI.SetNextControlName("SyncedObjectLengthField"); // This line means GetNameOfFocusedControl() returns "ArrayLengthField" when this box is selected
            targetSyncedObjectsLength = EditorGUILayout.IntField(targetSyncedObjectsLength, GUILayout.Width(labelFieldWidth), GUILayout.Height(labelHeight));

            EditorGUILayout.EndHorizontal();

            VerticalBreak(verticalBreakSize);

            for (int i = 0; i < sop.SyncedObjectStructs.Length; i++) {
                VerticalBreak(0);
                DisplaySyncedObjectElement(i);
            }

            // if targetListSize field is not selected, or is enter is pressed
            if (GUI.GetNameOfFocusedControl() != "SyncedObjectLengthField" || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
                // Create new array of targetListSize, copy old array contents into new array, replace old array with new array
                SyncedObjectStruct[] newSyncedObjectStructs = new SyncedObjectStruct[targetSyncedObjectsLength];
                
                // Repopulate the new array with the old array's contents
                for (int i = 0; i < Mathf.Clamp(newSyncedObjectStructs.Length, 0, sop.SyncedObjectStructs.Length); i++) {
                    newSyncedObjectStructs[i] = sop.SyncedObjectStructs[i];
                }

                // Remaining Client Packets
                for (int i = sop.SyncedObjectStructs.Length; i < newSyncedObjectStructs.Length; i++) {
                    newSyncedObjectStructs[i] = new SyncedObjectStruct();
                }
                sop.SyncedObjectStructs = newSyncedObjectStructs;
            }
        }
        
        private void DisplaySyncedObjectElement(int _index) {
            // Begin horizontal line of elements
            EditorGUILayout.BeginHorizontal();

            try {
                GUI.SetNextControlName("RegenerateDictionary");
                sop.SyncedObjectStructs[_index].tag = EditorGUILayout.TextField(sop.SyncedObjectStructs[_index].tag, GUILayout.Width(syncedObjectTagWidth), GUILayout.Height(syncedObjectElementHeight));
                GUI.SetNextControlName("RegenerateDictionary");
                sop.SyncedObjectStructs[_index].prefab = (GameObject)EditorGUILayout.ObjectField(sop.SyncedObjectStructs[_index].prefab, typeof(GameObject), false, GUILayout.Width(syncedObjectGameObjectFieldWidth), GUILayout.Height(syncedObjectElementHeight));
            } catch {
                // This prevents an error message
            }

            // End horizontal line of elements
            EditorGUILayout.EndHorizontal();

            if (GUI.GetNameOfFocusedControl() != "RegenerateDictionary" || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
                try {
                    sop.GenerateSyncedObjectsDict();
                } catch {
                    Debug.LogError("Error in Synced Objects Prefab Scriptable Object. Likely a repeated tag.");
                }
            }
        }
        
        #endregion

        #region Helper Functions

        private void VerticalBreak() {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Height(verticalBreakSize)); // This is a break
            EditorGUILayout.EndHorizontal();
        }

        private void VerticalBreak(float _height) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Height(_height)); // This is a break
            EditorGUILayout.EndHorizontal();
        }

        private void HelpBox() {
            string helpMessage =
                "The Synced Object tag must be unique and the same as the tag on the Synced Object on the server.";

            GUILayout.TextArea(helpMessage, helpBoxStyle);
        }

        #endregion
    }
}
