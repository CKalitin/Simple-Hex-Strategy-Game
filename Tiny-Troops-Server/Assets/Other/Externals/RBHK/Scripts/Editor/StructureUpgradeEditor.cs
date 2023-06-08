using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(StructureUpgrade)), CanEditMultipleObjects]
public class StructureUpgradeEditor : Editor {
    StructureUpgrade structureUpgrade;

    float verticalBreakSize;
    float horizontalBreakSize;

    // Resource Entry Display vars
    SerializedObject so;
    SerializedProperty resourceEntriesProperty;

    // Array management variables
    float amHeight;
    float amLabelWidth;
    float amFieldWidth;
    GUIStyle amLabelStyle;

    // Array element display variables, ae = array element
    float aeHeight;
    float aeVerticalBreakSize;

    float aeLabelWidth;
    float aeCostTypeWidth;
    float aeCostAmountWidth;

    int targetListSize;

    private void OnEnable() {
        structureUpgrade = (StructureUpgrade)target; // Get ResourceIds script which is the target
        targetListSize = structureUpgrade.Cost.Length;

        so = new SerializedObject(structureUpgrade);
        resourceEntriesProperty = so.FindProperty("resourceEntries");

    }

    public override void OnInspectorGUI() {
        #region Variables

        verticalBreakSize = 5;
        horizontalBreakSize = Screen.width * 0.01f;

        // Array management variables
        amHeight = Screen.height * 0.02f;
        amLabelWidth = Screen.width * 0.7f;
        amFieldWidth = Screen.width * 0.2f;
        amLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };

        // Array element display variables
        aeHeight = Screen.height * 0.019f;
        aeVerticalBreakSize = Screen.height * 0.003f;

        aeLabelWidth = Screen.width * 0.05f;
        aeCostTypeWidth = Screen.width * 0.5f;
        aeCostAmountWidth = Screen.width * 0.4f;

        #endregion

        VerticalBreak(verticalBreakSize);
        ResourceEntriesField();
        VerticalBreak(verticalBreakSize);
        ArrayLengthField();
        try {
            VerticalBreak(verticalBreakSize);
            for (int i = 0; i < structureUpgrade.Cost.Length; i++) {
                DisplayArrayElement(i);

                VerticalBreak(aeVerticalBreakSize);
            }
        } catch {
            // To prevent error:
            // ArgumentException: Getting control 5's position in a group with only 5 controls when doing repaint
            // Aborting
        }

        // This is so the Scriptable Objects save when Unity closes
        EditorUtility.SetDirty(target);
    }

    private void ResourceEntriesField() {
        EditorGUILayout.PropertyField(resourceEntriesProperty, true); // True means show children
        so.ApplyModifiedProperties(); // Remember to apply modified properties
    }

    private void ArrayLengthField() {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Upgrade Cost", amLabelStyle, GUILayout.Width(amLabelWidth), GUILayout.Height(amHeight));
        GUI.SetNextControlName("ArrayLengthField"); // This line means GetNameOfFocusedControl() returns "ArrayLengthField" when this box is selected
        targetListSize = EditorGUILayout.IntField(targetListSize, GUILayout.Width(amFieldWidth), GUILayout.Height(amHeight));

        EditorGUILayout.EndHorizontal();

        // if targetListSize field is not selected, or is enter is pressed
        if (GUI.GetNameOfFocusedControl() != "ArrayLengthField" || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
            // Create new array of targetListSize, copy old array contents into new array, replace old array with new array
            RBHKCost[] newCost = new RBHKCost[targetListSize];

            for (int i = 0; i < Mathf.Clamp(structureUpgrade.Cost.Length, 0, newCost.Length); i++) {
                newCost[i] = structureUpgrade.Cost[i];
            }

            structureUpgrade.Cost = newCost;
        }
    }

    // Display a single element of the list of costs in a horizontal line
    private void DisplayArrayElement(int _index) {
        // Begin horizontal line of elements
        EditorGUILayout.BeginHorizontal();

        //EditorGUILayout.LabelField(_index.ToString(), GUILayout.Width(aeLabelWidth), GUILayout.Height(aeHeight));
        structureUpgrade.Cost[_index].Resource = (GameResource)EditorGUILayout.EnumPopup(structureUpgrade.Cost[_index].Resource, GUILayout.Width(aeCostTypeWidth), GUILayout.Height(aeHeight));
        structureUpgrade.Cost[_index].Amount = EditorGUILayout.FloatField(structureUpgrade.Cost[_index].Amount, GUILayout.Width(aeCostAmountWidth), GUILayout.Height(aeHeight));
        // End horizontal line of elements
        EditorGUILayout.EndHorizontal();
    }

    private void VerticalBreak(float _height) {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Height(_height)); // This is a break
        EditorGUILayout.EndHorizontal();
    }
}
