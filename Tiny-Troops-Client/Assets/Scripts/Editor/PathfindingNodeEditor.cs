using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathfindingNode))]
public class PathfindingNodeEditor : Editor {
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

    PathfindingNode pathfindingNode;

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

        pathfindingNode = (PathfindingNode)target; // Get TileRule script which is the target

        labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        helpBoxStyle = new GUIStyle(GUI.skin.GetStyle("HelpBox")) { fontSize = 13 };

        #endregion

        //VerticalBreak();
        //    (0, 1)  (1, 1)
        // (-1,0) (self) (1, 0)
        //    (0,-1)  (1,-1)
        TopBotRowLabel("Top-Left (0, 1)", "Top-Right (1, 1)");
        VerticalLabelBreak();
        TopBotRow(5, 0);

        VerticalBreak();

        MiddleRowLabel("Left (-1,0)", "", "Right (1, 0)");
        VerticalLabelBreak();
        MiddleRow(4, 1);

        VerticalBreak();

        TopBotRowLabel("Btm-Left  (0,-1)", "Btm-Right (1,-1)");
        VerticalLabelBreak();
        TopBotRow(3, 2);

        VerticalBreak();
        
        pathfindingNode.FinalNode = EditorGUILayout.Toggle("Final Node", pathfindingNode.FinalNode);
        pathfindingNode.Radius = EditorGUILayout.FloatField("Radius", pathfindingNode.Radius);

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
        pathfindingNode.NodesArray[_textAreaOneIndex] = (PathfindingNode)EditorGUILayout.ObjectField(pathfindingNode.NodesArray[_textAreaOneIndex], typeof(PathfindingNode), true, GUILayout.Width(textFieldWidth - topbotRowOffset), GUILayout.Height(textFieldHeight));
        GUILayout.Label("", GUILayout.Width(topbotRowMidGap - topbotRowOffset)); // This is a break
        pathfindingNode.NodesArray[_textAreaTwoIndex] = (PathfindingNode)EditorGUILayout.ObjectField(pathfindingNode.NodesArray[_textAreaTwoIndex], typeof(PathfindingNode), true, GUILayout.Width(textFieldWidth - topbotRowOffset), GUILayout.Height(textFieldHeight));
        GUILayout.Label("", GUILayout.Width(topbotRowEdgeGap - topbotRowOffset)); // This is a break

        // End horizontal line of elements
        EditorGUILayout.EndHorizontal();
    }

    // Middle row display function
    private void MiddleRow(int _textAreaOneIndex, int _textAreaTwoIndex) {
        // Begin horizontal line of elements
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("", GUILayout.Width(middleRowEdgeGap - middleRowOffset)); // This is a break
        pathfindingNode.NodesArray[_textAreaOneIndex] = (PathfindingNode)EditorGUILayout.ObjectField(pathfindingNode.NodesArray[_textAreaOneIndex], typeof(PathfindingNode), true, GUILayout.Width(textFieldWidth - middleRowOffset), GUILayout.Height(textFieldHeight));
        GUILayout.Label("", GUILayout.Width(middleRowMidGap - middleRowOffset)); // This is a break
        GUILayout.Label("", GUILayout.Width(textFieldWidth - middleRowOffset)); // This is a break
        GUILayout.Label("", GUILayout.Width(middleRowMidGap - middleRowOffset)); // This is a break
        pathfindingNode.NodesArray[_textAreaTwoIndex] = (PathfindingNode)EditorGUILayout.ObjectField(pathfindingNode.NodesArray[_textAreaTwoIndex], typeof(PathfindingNode), true, GUILayout.Width(textFieldWidth - middleRowOffset), GUILayout.Height(textFieldHeight));
        GUILayout.Label("", GUILayout.Width(middleRowEdgeGap - middleRowOffset)); // This is a break

        // End horizontal line of elements
        EditorGUILayout.EndHorizontal();
    }

    private void TopBotRowLabel(string _textAreaOneString, string _textAreaTwoString) {
        // Begin horizontal line of elements
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("", GUILayout.Width(topbotRowEdgeGap - topbotRowOffset - 5)); // This is a break
        GUILayout.Label(_textAreaOneString, labelStyle, GUILayout.Width(textFieldWidth - topbotRowOffset + 5), GUILayout.Height(labelHeight));
        GUILayout.Label("", GUILayout.Width(topbotRowMidGap - topbotRowOffset)); // This is a break
        GUILayout.Label(_textAreaTwoString, labelStyle, GUILayout.Width(textFieldWidth - topbotRowOffset + 5), GUILayout.Height(labelHeight));
        GUILayout.Label("", GUILayout.Width(topbotRowEdgeGap - topbotRowOffset - 5)); // This is a break

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
            "This is how troops know which nodes to move to.\n" +
            "When agents reach their target tile they look at the list of nodes to find the next target tile.\n" +
            "The key in the dictionary of nodes is the direction of the next tile. (Eg. 1, 0)\n" +
            "If there is no node after this node because it leads to another tile, leave it null.\n" +
            "The agent looks at the TilePathfinding script on the next tile\n" +
            "\n" +
            "FinalNode: The final node on a path out of this tile. As mentioned above.\n" +
            "Radius: The area around this node agents can move to. This staggers their movement so it isn't perfectly uniform.";

        GUILayout.TextArea(helpMessage, helpBoxStyle);
    }

    #endregion
}