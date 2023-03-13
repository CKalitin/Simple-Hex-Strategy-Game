using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileRule))]
public class TileRuleEditor : Editor {
    private float verticalBreakSize = 0; // This is the space between horizontal lines
    private float labelVerticalBreakSize = 0; // This is the space between the text areas and the labels

    private float textFieldWidth = 0;
    private float textFieldHeight = 0;

    private float labelHeight = 0;

    // A 25 pixel gap is needed from the end of a line to the edge, so -5 is used on lines with 5 elements and - 3.574f is used on lines with 7 elements
    private float topbotRowOffset = 0; // Top and Bottom Row offset
    private float middleRowOffset = 0; // Seven Element Row Offset

    private float topbotRowEdgeGap = 0; // Gap on the sides of the top and bottom rows
    private float middleRowEdgeGap = 0; // Gap on the sides of the middle row

    private float topbotRowMidGap = 0; // Gap between the elements on the top and bottom rows
    private float middleRowMidGap = 0; // Gap between the elements on the middle row

    TileRule tileRule;

    // This is to center labels
    GUIStyle labelStyle;
    GUIStyle helpBoxStyle;

    public override void OnInspectorGUI() {
        //DrawDefaultInspector(); // Draw existing variables in the TileRule script

        #region Update Variables

        verticalBreakSize = Screen.height * 0.01f; // This is the space between horizontal lines
        labelVerticalBreakSize = Screen.height * 0.000005f; // This is the space between the text areas and the labels
        
        textFieldWidth = Screen.width * 0.2f;
        textFieldHeight = Screen.height * 0.025f;

        labelHeight = Screen.height * 0.025f;

        // A 25 pixel gap is needed from the end of a line to the edge, so -5 is used on lines with 5 elements and - 3.574f is used on lines with 7 elements
        topbotRowOffset = 5; // Top and Bottom Row offset
        middleRowOffset = 3.574f; // Seven Element Row Offset
        
        topbotRowEdgeGap = Screen.width * 0.225f; // Gap on the sides of the top and bottom rows
        middleRowEdgeGap = Screen.width * 0.1f; // Gap on the sides of the middle row
        
        topbotRowMidGap = Screen.width * 0.05f; // Gap between the elements on the top and bottom rows
        middleRowMidGap = Screen.width * 0.05f; // Gap between the elements on the middle row

        tileRule = (TileRule)target; // Get TileRule script which is the target

        labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        helpBoxStyle = new GUIStyle(GUI.skin.GetStyle("HelpBox")) { fontSize = 13 };

        #endregion

        tileRule.IntendedTileId = (Tiles)EditorGUILayout.EnumPopup("Intended Tile Id", tileRule.IntendedTileId);

        VerticalBreak();

        TopBotRowLabel("Top-Left", "Top-Right");
        VerticalLabelBreak();
        TopBotRow(4, 5);

        VerticalBreak();

        MiddleRowLabel("Left", "Update Object", "Right");
        VerticalLabelBreak();
        MiddleRow(2, 3);

        VerticalBreak();

        TopBotRowLabel("Bottom-Left", "Bottom-Right");
        VerticalLabelBreak();
        TopBotRow(0, 1);

        VerticalBreak();

        HelpBox();

        VerticalBreak();

        // This is so the Scriptable Objects save when Unity closes
        EditorUtility.SetDirty(target);
    }

    #region Display Functions

    // Top or Bottom row display function
    private void TopBotRow(int _textAreaOneIndex, int _textAreaTwoIndex) {
        // Begin horizontal line of elements
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("", GUILayout.Width(topbotRowEdgeGap - topbotRowOffset)); // This is a break
        tileRule.RequiredAdjacentTiles[_textAreaOneIndex] = (Tiles)EditorGUILayout.EnumPopup(tileRule.RequiredAdjacentTiles[_textAreaOneIndex], GUILayout.Width(textFieldWidth - topbotRowOffset), GUILayout.Height(textFieldHeight));
        GUILayout.Label("", GUILayout.Width(topbotRowMidGap - topbotRowOffset)); // This is a break
        tileRule.RequiredAdjacentTiles[_textAreaTwoIndex] = (Tiles)EditorGUILayout.EnumPopup(tileRule.RequiredAdjacentTiles[_textAreaTwoIndex], GUILayout.Width(textFieldWidth - topbotRowOffset), GUILayout.Height(textFieldHeight));
        GUILayout.Label("", GUILayout.Width(topbotRowEdgeGap - topbotRowOffset)); // This is a break

        // End horizontal line of elements
        EditorGUILayout.EndHorizontal();
    }

    // Middle row display function
    private void MiddleRow(int _textAreaOneIndex, int _textAreaTwoIndex) {
        // Begin horizontal line of elements
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("", GUILayout.Width(middleRowEdgeGap - middleRowOffset)); // This is a break
        tileRule.RequiredAdjacentTiles[_textAreaOneIndex] = (Tiles)EditorGUILayout.EnumPopup(tileRule.RequiredAdjacentTiles[_textAreaOneIndex], GUILayout.Width(textFieldWidth - middleRowOffset), GUILayout.Height(textFieldHeight));
        GUILayout.Label("", GUILayout.Width(middleRowMidGap - middleRowOffset)); // This is a break
        tileRule.UpdatedTilePrefab = (GameObject)EditorGUILayout.ObjectField(tileRule.UpdatedTilePrefab, typeof(GameObject), false, GUILayout.Width(textFieldWidth - middleRowOffset), GUILayout.Height(textFieldHeight));
        GUILayout.Label("", GUILayout.Width(middleRowMidGap - middleRowOffset)); // This is a break
        tileRule.RequiredAdjacentTiles[_textAreaTwoIndex] = (Tiles)EditorGUILayout.EnumPopup(tileRule.RequiredAdjacentTiles[_textAreaTwoIndex], GUILayout.Width(textFieldWidth - middleRowOffset), GUILayout.Height(textFieldHeight));
        GUILayout.Label("", GUILayout.Width(middleRowEdgeGap - middleRowOffset)); // This is a break

        // End horizontal line of elements
        EditorGUILayout.EndHorizontal();
    }

    private void TopBotRowLabel(string _textAreaOneString, string _textAreaTwoString) {
        // Begin horizontal line of elements
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("", GUILayout.Width(topbotRowEdgeGap - topbotRowOffset)); // This is a break
        GUILayout.Label(_textAreaOneString, labelStyle, GUILayout.Width(textFieldWidth - topbotRowOffset), GUILayout.Height(labelHeight));
        GUILayout.Label("", GUILayout.Width(topbotRowMidGap - topbotRowOffset)); // This is a break
        GUILayout.Label(_textAreaTwoString, labelStyle, GUILayout.Width(textFieldWidth - topbotRowOffset), GUILayout.Height(labelHeight));
        GUILayout.Label("", GUILayout.Width(topbotRowEdgeGap - topbotRowOffset)); // This is a break

        // End horizontal line of elements
        EditorGUILayout.EndHorizontal();
    }

    private void MiddleRowLabel(string _textAreaOneString, string _textAreaTwoString, string _textAreaThreeString) {
        // Begin horizontal line of elements
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("", GUILayout.Width(middleRowEdgeGap - middleRowOffset)); // This is a break
        GUILayout.Label(_textAreaOneString, labelStyle, GUILayout.Width(textFieldWidth - middleRowOffset), GUILayout.Height(labelHeight));
        GUILayout.Label("", GUILayout.Width(middleRowMidGap - middleRowOffset)); // This is a break
        GUILayout.Label(_textAreaTwoString, labelStyle, GUILayout.Width(textFieldWidth - middleRowOffset), GUILayout.Height(labelHeight));
        GUILayout.Label("", GUILayout.Width(middleRowMidGap - middleRowOffset)); // This is a break
        GUILayout.Label(_textAreaThreeString, labelStyle, GUILayout.Width(textFieldWidth - middleRowOffset), GUILayout.Height(labelHeight));
        GUILayout.Label("", GUILayout.Width(middleRowEdgeGap - middleRowOffset)); // This is a break

        // End horizontal line of elements
        EditorGUILayout.EndHorizontal();
    }

    private void VerticalBreak() {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Height(verticalBreakSize)); // This is a break
        EditorGUILayout.EndHorizontal();
    }

    private void VerticalLabelBreak() {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Height(labelVerticalBreakSize)); // This is a break
        EditorGUILayout.EndHorizontal();
    }

    private void HelpBox() {
        string helpMessage =
            "This is how Tile Rules are configured. \n" +
            "The Intended Tile Id at the top is the Tile Id that this tile rule is intended to be used on. This field is only for the user and can be used to take notes, etc. \n" +
            "The Update Object in the center of the hexagon is what the Tile Object will be updated to if the rules are satisfied. \n" +
            "The boxes around the center are the Id's of required tiles to replace the Tile Object with the Update Object.\n" +
            "If a box is left empty it is ignored when the Tile Rule is used. \n" +
            "Every tile should be given an ID and the Id's of \"0\" and \"Null\" should be avoided.\n" +
            "There is a child of the tile which replaced, the parent GameObject and Variables are not changed.";

        GUILayout.TextArea(helpMessage, helpBoxStyle);
    }

    #endregion
}
