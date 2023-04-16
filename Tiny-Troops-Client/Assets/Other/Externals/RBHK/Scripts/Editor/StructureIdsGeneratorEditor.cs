using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StructureIdsGenerator))]
public class StructureIdsGeneratorEditor : Editor {
    StructureIdsGenerator structureIdsGenerator;

    float verticalBreakSize;

    GUIStyle tooltipStyle;
    GUIStyle helpBoxStyle;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        #region Variables

        structureIdsGenerator = (StructureIdsGenerator)target; // Get TileIds script which is the target

        verticalBreakSize = 5;

        tooltipStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
        helpBoxStyle = new GUIStyle(GUI.skin.GetStyle("HelpBox")) { fontSize = 13 };

        #endregion

        VerticalBreak();

        // This button is used for updating tileIds at runtime
        if (GUILayout.Button("Generate Structure Ids Enum")) {
            structureIdsGenerator.UpdateEnums();

            // This is used to refresh the assets which adds the newly generated enums
            AssetDatabase.Refresh();
        }

        VerticalBreak();

        //HelpBox();

        // This is so the Scriptable Objects save when Unity closes
        EditorUtility.SetDirty(target);
    }

    private void VerticalBreak() {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Height(verticalBreakSize)); // This is a break
        EditorGUILayout.EndHorizontal();
    }

    private void HelpBox() {
        string helpMessage =
            "This is where tile enums (The dropdown of tile Ids) is configured. \n" +
            "In the array simply type in the tile Ids you want (Without Spaces) and click the generate button when finished. \n" +
            "From here you can use \"Tiles.(TileName)\" in code when needing the tile id.\n" +
            "The Tile Enum Path is the path where the enum script will be stored.\n" +
            "Tile Enum Name is a custom name for the enums instead of \"Tiles\", So you would do \"(Tile Enum Name).(Tile Name)\" instead of what's mentioned above.";

        GUILayout.TextArea(helpMessage, helpBoxStyle);
    }
}
