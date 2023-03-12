using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace USNL.Package {
    [CustomEditor(typeof(ScriptGenerator))]
    [CanEditMultipleObjects]
    public class ScriptGeneratorEditor : Editor {
        ScriptGenerator scriptGenerator;

        // Display Variables
        float verticalBreakSize = 5;

        GUIStyle tooltipStyle;
        GUIStyle helpBoxStyle;

        GUIStyle labelStyle;
        float labelWidth;
        float labelFieldWidth;
        float labelHeight;

        float elementVisibleButtonWidth;

        float packetElementHeight;
        float packetElementNameWidth;
        float clientPacketElementProtocolWidth;
        float serverPacketElementSendTypeWidth;
        float serverPacketElementProtocolWidth;
        float packetElementVarTextWidth;
        float packetElementVarLengthWidth;

        GUIStyle variableLabelStyle;
        float variableElementHeight;
        float variableNameWidth;
        float variableTypeWidth;
        float variableElementVerticalBreakSize = 5;

        // Real Variables
        int targetClientPacketsListSize;
        int targetServerPacketsListSize;

        int[] targetClientVariblesLengths;
        int[] targetServerVariblesLengths;

        int previousTargetClientPacketsListSize;
        int previousTargetServerPacketsListSize;

        int[] previousTargetClientVariblesLengths;
        int[] previousTargetServerVariblesLengths;

        private void OnEnable() {
            scriptGenerator = (ScriptGenerator)target;

            targetClientPacketsListSize = scriptGenerator.ClientPackets.Length;
            targetServerPacketsListSize = scriptGenerator.ServerPackets.Length;

            targetClientVariblesLengths = new int[scriptGenerator.ClientPackets.Length];
            targetServerVariblesLengths = new int[scriptGenerator.ServerPackets.Length];

            for (int i = 0; i < scriptGenerator.ClientPackets.Length; i++) targetClientVariblesLengths[i] = scriptGenerator.ClientPackets[i].PacketVariables.Length;
            for (int i = 0; i < scriptGenerator.ServerPackets.Length; i++) targetServerVariblesLengths[i] = scriptGenerator.ServerPackets[i].PacketVariables.Length;

            previousTargetClientVariblesLengths = new int[scriptGenerator.ClientPackets.Length];
            previousTargetServerVariblesLengths = new int[scriptGenerator.ServerPackets.Length];

            for (int i = 0; i < scriptGenerator.ClientPackets.Length; i++) previousTargetClientVariblesLengths[i] = scriptGenerator.ClientPackets[i].PacketVariables.Length;
            for (int i = 0; i < scriptGenerator.ServerPackets.Length; i++) previousTargetServerVariblesLengths[i] = scriptGenerator.ServerPackets[i].PacketVariables.Length;

            if (scriptGenerator.ClientPacketFoldouts == null || scriptGenerator.ClientPacketFoldouts.Length <= 0) {
                bool[] newCPF = new bool[targetClientPacketsListSize];
                for (int i = 0; i < scriptGenerator.ClientPacketFoldouts.Length; i++) newCPF[i] = scriptGenerator.ClientPacketFoldouts[i];
                scriptGenerator.ClientPacketFoldouts = newCPF;
            }
            if (scriptGenerator.ServerPacketFoldouts == null || scriptGenerator.ServerPacketFoldouts.Length <= 0) {
                bool[] newSPF = new bool[targetServerPacketsListSize];
                for (int i = 0; i < scriptGenerator.ServerPacketFoldouts.Length; i++) newSPF[i] = scriptGenerator.ServerPacketFoldouts[i];
                scriptGenerator.ServerPacketFoldouts = newSPF;
            }
        }

        public override void OnInspectorGUI() {
            #region Variables

            tooltipStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            helpBoxStyle = new GUIStyle(GUI.skin.GetStyle("HelpBox")) { fontSize = 13 };

            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            labelWidth = Screen.width * 0.6f;
            labelFieldWidth = Screen.width * 0.3f;
            labelHeight = 20;

            packetElementHeight = 17f;
            elementVisibleButtonWidth = 17f;
            packetElementNameWidth = Screen.width * 0.3f;
            clientPacketElementProtocolWidth = Screen.width * 0.315f - 20f;
            serverPacketElementSendTypeWidth = Screen.width * 0.1575f - 10f;
            serverPacketElementProtocolWidth = Screen.width * 0.1575f - 10f;
            packetElementVarTextWidth = Screen.width * 0.22f;
            packetElementVarLengthWidth = Screen.width * 0.05f;

            variableLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Normal };
            variableElementHeight = 17;
            variableNameWidth = Screen.width * 0.45f;
            variableTypeWidth = Screen.width * 0.45f;

            #endregion

            VerticalBreak();

            ClientPacketsField();

            VerticalBreak();

            ServerPacketsField();

            VerticalBreak();
            // This button is used for updating packets at runtime
            if (GUILayout.Button("Generate")) {
                scriptGenerator.GenerateScript();

                // This is used to refresh the assets which adds the newly generated enums
                AssetDatabase.Refresh();
            }
            VerticalBreak();

            HelpBox();

            VerticalBreak();

            // This is so the Scriptable Objects save when Unity closes
            EditorUtility.SetDirty(target);
        }

        #region Packets

        private void ClientPacketsField() {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Client Packets", labelStyle, GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
            GUI.SetNextControlName("ClientPacketsLengthField"); // This line means GetNameOfFocusedControl() returns "ArrayLengthField" when this box is selected
            targetClientPacketsListSize = EditorGUILayout.IntField(targetClientPacketsListSize, GUILayout.Width(labelFieldWidth), GUILayout.Height(labelHeight));

            EditorGUILayout.EndHorizontal();

            // if targetListSize field is not selected, or is enter is pressed
            if (GUI.GetNameOfFocusedControl() != "ClientPacketsLengthField" || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
                if (previousTargetClientPacketsListSize != targetClientPacketsListSize) {

                    // Create new array of targetListSize, copy old array contents into new array, replace old array with new array
                    ScriptGenerator.ClientPacketConfig[] packets = new ScriptGenerator.ClientPacketConfig[targetClientPacketsListSize];
                    bool[] foldouts = new bool[targetClientPacketsListSize];

                    // Repopulate the new array with the old array's contents
                    for (int i = 0; i < Mathf.Clamp(scriptGenerator.ClientPackets.Length, 0, packets.Length); i++) {
                        packets[i] = scriptGenerator.ClientPackets[i];
                        foldouts[i] = scriptGenerator.ClientPacketFoldouts[i];
                    }

                    // Remaining Client Packets
                    for (int i = scriptGenerator.ClientPackets.Length; i < packets.Length; i++) {
                        if (scriptGenerator.ClientPackets.Length > 0) {
                            ScriptGenerator.ClientPacketConfig fromPacket = scriptGenerator.ClientPackets[scriptGenerator.ClientPackets.Length - 1];
                            packets[i] = new ScriptGenerator.ClientPacketConfig(fromPacket.PacketName, new ScriptGenerator.PacketVariable[fromPacket.PacketVariables.Length], fromPacket.Protocol);
                            for (int x = 0; x < fromPacket.PacketVariables.Length; x++) {
                                packets[i].PacketVariables[x] = new ScriptGenerator.PacketVariable(fromPacket.PacketVariables[x].VariableName, fromPacket.PacketVariables[x].VariableType);
                            }
                        } else packets[i] = new ScriptGenerator.ClientPacketConfig("", new ScriptGenerator.PacketVariable[0], ScriptGenerator.Protocol.TCP);
                        foldouts[i] = true;
                    }
                    scriptGenerator.ClientPackets = packets;
                    scriptGenerator.ClientPacketFoldouts = foldouts;

                    int[] newTargetClientVariblesLengths = new int[targetClientPacketsListSize];
                    int[] newPreviousTargetClientVariablesLength = new int[targetClientPacketsListSize];
                    for (int i = 0; i < targetClientVariblesLengths.Length && i < newTargetClientVariblesLengths.Length; i++) {
                        newTargetClientVariblesLengths[i] = targetClientVariblesLengths[i];
                        newPreviousTargetClientVariablesLength[i] = previousTargetClientVariblesLengths[i];
                    }

                    for (int i = targetClientVariblesLengths.Length; i < newTargetClientVariblesLengths.Length; i++) {
                        if (targetClientVariblesLengths.Length > 0) {
                            newTargetClientVariblesLengths[i] = targetClientVariblesLengths[targetClientVariblesLengths.Length - 1];
                            newPreviousTargetClientVariablesLength[i] = previousTargetClientVariblesLengths[previousTargetClientVariblesLengths.Length - 1];
                        } else {
                            newTargetClientVariblesLengths[i] = 0;
                            newPreviousTargetClientVariablesLength[i] = 0;
                        }
                    }

                    targetClientVariblesLengths = newTargetClientVariblesLengths;
                    previousTargetClientVariblesLengths = newPreviousTargetClientVariablesLength;

                    previousTargetClientPacketsListSize = targetClientPacketsListSize;
                }
            }

            for (int i = 0; i < scriptGenerator.ClientPackets.Length; i++) {
                VerticalBreak(variableElementVerticalBreakSize);
                DisplayClientPacketElement(i);
            }
        }

        private void DisplayClientPacketElement(int _index) {
            // Begin horizontal line of elements
            EditorGUILayout.BeginHorizontal();

            try {
                scriptGenerator.ClientPacketFoldouts[_index] = EditorGUILayout.Toggle(scriptGenerator.ClientPacketFoldouts[_index], GUILayout.Width(elementVisibleButtonWidth), GUILayout.Height(packetElementHeight));
                scriptGenerator.ClientPackets[_index].PacketName = EditorGUILayout.TextField(scriptGenerator.ClientPackets[_index].PacketName, GUILayout.Width(packetElementNameWidth), GUILayout.Height(packetElementHeight));
                scriptGenerator.ClientPackets[_index].Protocol = (ScriptGenerator.Protocol)EditorGUILayout.EnumPopup(scriptGenerator.ClientPackets[_index].Protocol, GUILayout.Width(clientPacketElementProtocolWidth), GUILayout.Height(packetElementHeight));
                EditorGUILayout.LabelField("Num. Variables ", GUILayout.Width(packetElementVarTextWidth), GUILayout.Height(packetElementHeight));
                GUI.SetNextControlName("ClientPacketVariablesLengthField"); // This line means GetNameOfFocusedControl() returns "ArrayLengthField" when this box is selected
                targetClientVariblesLengths[_index] = EditorGUILayout.IntField(targetClientVariblesLengths[_index], GUILayout.Width(packetElementVarLengthWidth), GUILayout.Height(packetElementHeight));
            } catch {
                // This prevents an error
            }

            // End horizontal line of elements
            EditorGUILayout.EndHorizontal();

            if (scriptGenerator.ClientPacketFoldouts[_index])
                for (int i = 0; i < scriptGenerator.ClientPackets[_index].PacketVariables.Length; i++) { DisplayClientPacketVariableElement(_index, i); }

            // if targetListSize field is not selected, or is enter is pressed
            if (GUI.GetNameOfFocusedControl() != "ClientPacketVariablesLengthField" || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
                if (previousTargetClientVariblesLengths[_index] == targetClientVariblesLengths[_index]) return;

                // Create new array of targetListSize, copy old array contents into new array, replace old array with new array
                ScriptGenerator.PacketVariable[] packetVariables = new ScriptGenerator.PacketVariable[targetClientVariblesLengths[_index]];

                for (int i = 0; i < scriptGenerator.ClientPackets[_index].PacketVariables.Length && i < targetClientVariblesLengths[_index]; i++) {
                    packetVariables[i] = scriptGenerator.ClientPackets[_index].PacketVariables[i];
                }

                for (int i = targetClientVariblesLengths[_index]; i < packetVariables.Length; i++) {
                    if (scriptGenerator.ClientPackets[_index].PacketVariables.Length > 0)
                        packetVariables[i] = new ScriptGenerator.PacketVariable(scriptGenerator.ClientPackets[_index].PacketVariables[scriptGenerator.ClientPackets[_index].PacketVariables.Length - 1].VariableName, scriptGenerator.ClientPackets[_index].PacketVariables[scriptGenerator.ClientPackets[_index].PacketVariables.Length - 1].VariableType);
                    else
                        packetVariables[i] = new ScriptGenerator.PacketVariable("", ScriptGenerator.PacketVarType.Byte);
                }

                scriptGenerator.ClientPackets[_index].PacketVariables = packetVariables;

                previousTargetClientVariblesLengths[_index] = targetClientVariblesLengths[_index];
            }
        }

        private void DisplayClientPacketVariableElement(int _packetIndex, int _variableIndex) {
            // Begin horizontal line of elements
            EditorGUILayout.BeginHorizontal();

            try {
                scriptGenerator.ClientPackets[_packetIndex].PacketVariables[_variableIndex].VariableName = EditorGUILayout.TextField(scriptGenerator.ClientPackets[_packetIndex].PacketVariables[_variableIndex].VariableName, GUILayout.Width(variableNameWidth), GUILayout.Height(variableElementHeight));
                scriptGenerator.ClientPackets[_packetIndex].PacketVariables[_variableIndex].VariableType = (ScriptGenerator.PacketVarType)EditorGUILayout.EnumPopup(scriptGenerator.ClientPackets[_packetIndex].PacketVariables[_variableIndex].VariableType, GUILayout.Width(variableTypeWidth), GUILayout.Height(variableElementHeight));
            } catch {
                // This prevents an error message
            }

            // End horizontal line of elements
            EditorGUILayout.EndHorizontal();
        }

        private void ServerPacketsField() {
            EditorGUILayout.BeginHorizontal();

            try {
                EditorGUILayout.LabelField("Server Packets", labelStyle, GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
                GUI.SetNextControlName("ServerPacketsLengthField"); // This line means GetNameOfFocusedControl() returns "ArrayLengthField" when this box is selected
                targetServerPacketsListSize = EditorGUILayout.IntField(targetServerPacketsListSize, GUILayout.Width(labelFieldWidth), GUILayout.Height(labelHeight));

            } catch {
                // This prevents an error message
            }

            EditorGUILayout.EndHorizontal();

            // if targetListSize field is not selected, or is enter is pressed
            if (GUI.GetNameOfFocusedControl() != "ServerPacketsLengthField" || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
                if (previousTargetServerPacketsListSize != targetServerPacketsListSize) {

                    // Create new array of targetListSize, copy old array contents into new array, replace old array with new array
                    ScriptGenerator.ServerPacketConfig[] packets = new ScriptGenerator.ServerPacketConfig[targetServerPacketsListSize];
                    bool[] foldouts = new bool[targetServerPacketsListSize];

                    // Repopulate the new array with the old array's contents
                    for (int i = 0; i < Mathf.Clamp(scriptGenerator.ServerPackets.Length, 0, packets.Length); i++) {
                        packets[i] = scriptGenerator.ServerPackets[i];
                        foldouts[i] = scriptGenerator.ServerPacketFoldouts[i];
                    }

                    // Remaining Server Packets
                    for (int i = scriptGenerator.ServerPackets.Length; i < packets.Length; i++) {
                        if (scriptGenerator.ServerPackets.Length > 0) {
                            ScriptGenerator.ServerPacketConfig fromPacket = scriptGenerator.ServerPackets[scriptGenerator.ServerPackets.Length - 1];
                            packets[i] = new ScriptGenerator.ServerPacketConfig(fromPacket.PacketName, new ScriptGenerator.PacketVariable[fromPacket.PacketVariables.Length], fromPacket.SendType, fromPacket.Protocol);
                            for (int x = 0; x < fromPacket.PacketVariables.Length; x++) {
                                packets[i].PacketVariables[x] = new ScriptGenerator.PacketVariable(fromPacket.PacketVariables[x].VariableName, fromPacket.PacketVariables[x].VariableType);
                            }
                        } else {
                            packets[i] = new ScriptGenerator.ServerPacketConfig("", new ScriptGenerator.PacketVariable[0], ScriptGenerator.ServerPacketType.SendToAllClients, ScriptGenerator.Protocol.TCP);
                        }
                        foldouts[i] = true;
                    }
                    scriptGenerator.ServerPackets = packets;
                    scriptGenerator.ServerPacketFoldouts = foldouts;

                    int[] newTargetServerVariblesLengths = new int[targetServerPacketsListSize];
                    int[] newPreviousTargetServerVariablesLength = new int[targetServerPacketsListSize];
                    for (int i = 0; i < targetServerVariblesLengths.Length && i < newTargetServerVariblesLengths.Length; i++) {
                        newTargetServerVariblesLengths[i] = targetServerVariblesLengths[i];
                        newPreviousTargetServerVariablesLength[i] = previousTargetServerVariblesLengths[i];
                    }

                    for (int i = targetServerVariblesLengths.Length; i < newTargetServerVariblesLengths.Length; i++) {
                        if (targetServerVariblesLengths.Length > 0) {
                            newTargetServerVariblesLengths[i] = targetServerVariblesLengths[targetServerVariblesLengths.Length - 1];
                            newPreviousTargetServerVariablesLength[i] = previousTargetServerVariblesLengths[previousTargetServerVariblesLengths.Length - 1];
                        } else {
                            newTargetServerVariblesLengths[i] = 0;
                            newPreviousTargetServerVariablesLength[i] = 0;
                        }
                    }

                    targetServerVariblesLengths = newTargetServerVariblesLengths;
                    previousTargetServerVariblesLengths = newPreviousTargetServerVariablesLength;

                    previousTargetServerPacketsListSize = targetServerPacketsListSize;
                }
            }

            for (int i = 0; i < scriptGenerator.ServerPackets.Length; i++) {
                VerticalBreak(variableElementVerticalBreakSize);
                DisplayServerPacketElement(i);
            }
        }

        private void DisplayServerPacketElement(int _index) {
            // Begin horizontal line of elements
            EditorGUILayout.BeginHorizontal();

            try {
                scriptGenerator.ServerPacketFoldouts[_index] = EditorGUILayout.Toggle(scriptGenerator.ServerPacketFoldouts[_index], GUILayout.Width(elementVisibleButtonWidth), GUILayout.Height(packetElementHeight));
                scriptGenerator.ServerPackets[_index].PacketName = EditorGUILayout.TextField(scriptGenerator.ServerPackets[_index].PacketName, GUILayout.Width(packetElementNameWidth), GUILayout.Height(packetElementHeight));
                scriptGenerator.ServerPackets[_index].SendType = (ScriptGenerator.ServerPacketType)EditorGUILayout.EnumPopup(scriptGenerator.ServerPackets[_index].SendType, GUILayout.Width(serverPacketElementSendTypeWidth), GUILayout.Height(packetElementHeight));
                scriptGenerator.ServerPackets[_index].Protocol = (ScriptGenerator.Protocol)EditorGUILayout.EnumPopup(scriptGenerator.ServerPackets[_index].Protocol, GUILayout.Width(serverPacketElementProtocolWidth), GUILayout.Height(packetElementHeight));
                EditorGUILayout.LabelField("Num. Variables ", GUILayout.Width(packetElementVarTextWidth), GUILayout.Height(packetElementHeight));
                GUI.SetNextControlName("ServerPacketVariablesLengthField"); // This line means GetNameOfFocusedControl() returns "ArrayLengthField" when this box is selected
                targetServerVariblesLengths[_index] = EditorGUILayout.IntField(targetServerVariblesLengths[_index], GUILayout.Width(packetElementVarLengthWidth), GUILayout.Height(packetElementHeight));
            } catch {
                // This prevents an error
            }

            // End horizontal line of elements
            EditorGUILayout.EndHorizontal();

            if (scriptGenerator.ServerPacketFoldouts[_index])
                for (int i = 0; i < scriptGenerator.ServerPackets[_index].PacketVariables.Length; i++) { DisplayServerPacketVariableElement(_index, i); }

            // if targetListSize field is not selected, or is enter is pressed
            if (GUI.GetNameOfFocusedControl() != "ServerPacketVariablesLengthField" || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
                if (previousTargetServerVariblesLengths[_index] == targetServerVariblesLengths[_index]) return;

                // Create new array of targetListSize, copy old array contents into new array, replace old array with new array
                ScriptGenerator.PacketVariable[] packetVariables = new ScriptGenerator.PacketVariable[targetServerVariblesLengths[_index]];

                for (int i = 0; i < scriptGenerator.ServerPackets[_index].PacketVariables.Length && i < targetServerVariblesLengths[_index]; i++) {
                    packetVariables[i] = scriptGenerator.ServerPackets[_index].PacketVariables[i];
                }

                for (int i = targetServerVariblesLengths[_index]; i < packetVariables.Length; i++) {
                    if (scriptGenerator.ServerPackets[_index].PacketVariables.Length > 0)
                        packetVariables[i] = new ScriptGenerator.PacketVariable(scriptGenerator.ServerPackets[_index].PacketVariables[scriptGenerator.ServerPackets[_index].PacketVariables.Length - 1].VariableName, scriptGenerator.ServerPackets[_index].PacketVariables[scriptGenerator.ServerPackets[_index].PacketVariables.Length - 1].VariableType);
                    else
                        packetVariables[i] = new ScriptGenerator.PacketVariable("", ScriptGenerator.PacketVarType.Byte);
                }

                scriptGenerator.ServerPackets[_index].PacketVariables = packetVariables;

                previousTargetServerVariblesLengths[_index] = targetServerVariblesLengths[_index];
            }
        }

        private void DisplayServerPacketVariableElement(int _packetIndex, int _variableIndex) {
            // Begin horizontal line of elements
            EditorGUILayout.BeginHorizontal();

            scriptGenerator.ServerPackets[_packetIndex].PacketVariables[_variableIndex].VariableName = EditorGUILayout.TextField(scriptGenerator.ServerPackets[_packetIndex].PacketVariables[_variableIndex].VariableName, GUILayout.Width(variableNameWidth), GUILayout.Height(variableElementHeight));
            scriptGenerator.ServerPackets[_packetIndex].PacketVariables[_variableIndex].VariableType = (ScriptGenerator.PacketVarType)EditorGUILayout.EnumPopup(scriptGenerator.ServerPackets[_packetIndex].PacketVariables[_variableIndex].VariableType, GUILayout.Width(variableTypeWidth), GUILayout.Height(variableElementHeight));

            // End horizontal line of elements
            EditorGUILayout.EndHorizontal();
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
                "Server Packets: Server -> Client" +
                "\nClient Packets: Client -> Server" +
                "\n" +
                "\nType variable names should be in C# format: exampleVariable." +
                "\n" +
                "\nThis is just a ScriptableObject so the data can be lost if something happens to the package. Good idea to back it up.";

            GUILayout.TextArea(helpMessage, helpBoxStyle);
        }

        #endregion
    }
}